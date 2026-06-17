using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[System.Serializable]
public struct SpecialCandyUIData
{
    public string candyColorName;       // Tên màu kẹo (Blue, Green, Orange...)
    public int candyTypeId;             // ID của màu kẹo tương ứng

    [Header("Candy Prefabs UI")]
    public GameObject normalCandyPrefab;       // Prefab kẹo thường gốc
    public GameObject horizontalStripedPrefab; // Prefab kẹo sọc ngang
    public GameObject verticalStripedPrefab;   // Prefab kẹo sọc dọc
    public GameObject wrappedPrefab;           // Prefab kẹo bọc
}

public class BoardManager : MonoBehaviour
{
    [Header("Tilemap Setup")]
    public Tilemap candyTilemap;

    [Header("Old Candy Prefabs (Fallback)")]
    public GameObject[] candyPrefabs;

    [Header("==== SPECIAL CANDIES DATABASE UI ====")]
    public List<SpecialCandyUIData> candyDatabase;
    public GameObject colorBombPrefab; // Prefab viên Choco đa sắc độc lập

    [Header("Board Settings (Chỉnh cố định tại đây)")]
    public int width = 8;
    public int height = 9;

    public Candy[,] allCandies;

    private BoundsInt boardBounds;
    private bool isProcessing = false; // Khóa chống người chơi click khi kẹo đang rơi

    private Candy lastSwappedCandy;
    private bool lastSwipeWasVertical;

    void Start()
    {
        // Lấy giới hạn góc tọa độ của Tilemap để dịch vị trí chính xác
        boardBounds = candyTilemap.cellBounds;

        // KHÔNG TỰ CẬP NHẬT WIDTH/HEIGHT TỪ TILEMAP NỮA ĐỂ TRÁNH LỖI NHẢY LÊN 10 HÀNG
        // Giữ nguyên width và height bạn cấu hình trên Inspector

        allCandies = new Candy[width, height];
        StartCoroutine(InitializeBoardRoutine());
    }

    private IEnumerator InitializeBoardRoutine()
    {
        isProcessing = true;

        yield return StartCoroutine(RefillBoardCo());

        HashSet<Candy> matches = FindAllMatches();
        while (matches.Count > 0)
        {
            DestroyMatchesSilent(matches);
            yield return new WaitForSeconds(0.2f);
            yield return StartCoroutine(DecreaseRowCo());
            yield return StartCoroutine(RefillBoardCo());
            matches = FindAllMatches();
        }

        isProcessing = false;

    }

    public Vector3 GetWorldPosFromGrid(int x, int y)
    {
        int tileX = x + boardBounds.xMin;
        int tileY = y + boardBounds.yMin;
        return candyTilemap.GetCellCenterWorld(new Vector3Int(tileX, tileY, 0));
    }

    public void SetSwipeHistory(Candy clickedCandy, bool isVertical)
    {
        lastSwappedCandy = clickedCandy;
        lastSwipeWasVertical = isVertical;
    }

    // ================= LOGIC SWAP & ĐIỀU PHỐI CHUỖI NỔ COMBO =================

    public IEnumerator SwapCandiesRoutine(int x1, int y1, int x2, int y2)
    {
        if (MoveManager.Instance != null && MoveManager.Instance.IsGameOver()) yield break;
        if (isProcessing) yield break;

        Candy candy1 = allCandies[x1, y1];
        Candy candy2 = allCandies[x2, y2];

        if (candy1 == null || candy2 == null) yield break;

        isProcessing = true;

        allCandies[x1, y1] = candy2;
        allCandies[x2, y2] = candy1;

        candy1.x = x2; candy1.y = y2;
        candy2.x = x1; candy2.y = y1;

        StartCoroutine(MoveCandyAnimation(candy1.transform, GetWorldPosFromGrid(x2, y2), 0.2f));
        yield return StartCoroutine(MoveCandyAnimation(candy2.transform, GetWorldPosFromGrid(x1, y1), 0.2f));

        if (candy1.specialType != Candy.SpecialType.None || candy2.specialType != Candy.SpecialType.None)
        {
            if (CheckAndExecuteSpecialCombo(candy1, candy2))
            {
                if (MoveManager.Instance != null) MoveManager.Instance.UseAMove();
                yield break;
            }
        }

        HashSet<Candy> matches = FindAllMatches();

        if (matches.Count > 0)
        {
            if (ScoreManager.Instance != null) ScoreManager.Instance.ResetCombo();
            if (MoveManager.Instance != null) MoveManager.Instance.UseAMove();

            yield return StartCoroutine(MatchAndFallRoutine(matches));
        }
        else
        {
            allCandies[x1, y1] = candy1;
            allCandies[x2, y2] = candy2;

            candy1.x = x1; candy1.y = y1;
            candy2.x = x2; candy2.y = y2;

            StartCoroutine(MoveCandyAnimation(candy1.transform, GetWorldPosFromGrid(x1, y1), 0.2f));
            yield return StartCoroutine(MoveCandyAnimation(candy2.transform, GetWorldPosFromGrid(x2, y2), 0.2f));

            isProcessing = false;
        }
    }

    private IEnumerator MatchAndFallRoutine(HashSet<Candy> matches)
    {
        isProcessing = true;

        HashSet<Candy> totalExplodedCandies = GetSpecialExplosionArea(matches);
        ProcessSpecialCandyCreation(matches, totalExplodedCandies);

        DestroyMatches(totalExplodedCandies);
        yield return new WaitForSeconds(0.2f);

        yield return StartCoroutine(DecreaseRowCo());
        yield return new WaitForSeconds(0.1f);

        yield return StartCoroutine(RefillBoardCo());

        HashSet<Candy> nextMatches = FindAllMatches();
        if (nextMatches.Count > 0)
        {
            if (ScoreManager.Instance != null) ScoreManager.Instance.IncreaseCombo();
            yield return StartCoroutine(MatchAndFallRoutine(nextMatches));
        }
        else
        {
            if (ScoreManager.Instance != null) ScoreManager.Instance.ResetCombo();
            isProcessing = false;
        }
    }

    // ================= 🧠 THUẬT TOÁN: TẠO VÀ SINH KẸO ĐẶC BIỆT 🧠 =================

    private void ProcessSpecialCandyCreation(HashSet<Candy> baseMatches, HashSet<Candy> totalExploded)
    {
        Dictionary<int, List<Candy>> clusterByColor = new Dictionary<int, List<Candy>>();
        foreach (Candy c in baseMatches)
        {
            if (c == null || c.specialType == Candy.SpecialType.ColorBomb) continue;
            if (!clusterByColor.ContainsKey(c.candyType))
                clusterByColor[c.candyType] = new List<Candy>();
            clusterByColor[c.candyType].Add(c);
        }

        foreach (var pair in clusterByColor)
        {
            List<Candy> matchGroup = pair.Value;
            int colorId = pair.Key;

            if (matchGroup.Count < 4) continue;

            Candy spawnPoint = matchGroup[0];
            foreach (Candy c in matchGroup)
            {
                if (c == lastSwappedCandy)
                {
                    spawnPoint = c;
                    break;
                }
            }

            GameObject specialPrefab = null;
            Candy.SpecialType generatedType = Candy.SpecialType.None;

            if (matchGroup.Count >= 5 && IsLineMatch(matchGroup))
            {
                specialPrefab = colorBombPrefab;
                generatedType = Candy.SpecialType.ColorBomb;
            }
            else if (matchGroup.Count >= 5 && !IsLineMatch(matchGroup))
            {
                SpecialCandyUIData data = candyDatabase.Find(d => d.candyTypeId == colorId);
                specialPrefab = data.wrappedPrefab;
                generatedType = Candy.SpecialType.Wrapped;
            }
            else if (matchGroup.Count == 4)
            {
                SpecialCandyUIData data = candyDatabase.Find(d => d.candyTypeId == colorId);
                specialPrefab = lastSwipeWasVertical ? data.horizontalStripedPrefab : data.verticalStripedPrefab;
                generatedType = lastSwipeWasVertical ? Candy.SpecialType.HorizontalStriped : Candy.SpecialType.VerticalStriped;
            }

            if (specialPrefab != null)
            {
                totalExploded.Remove(spawnPoint);

                int sx = spawnPoint.x;
                int sy = spawnPoint.y;

                if (allCandies[sx, sy] != null) Destroy(allCandies[sx, sy].gameObject);

                Vector3 spawnPos = GetWorldPosFromGrid(sx, sy);
                GameObject specialObj = Instantiate(specialPrefab, spawnPos, Quaternion.identity, this.transform);

                Vector3 cellSize = candyTilemap.cellSize;
                specialObj.transform.localScale = new Vector3(cellSize.x, cellSize.y, 1f);

                Candy specialScript = specialObj.GetComponent<Candy>();
                specialScript.x = sx;
                specialScript.y = sy;

                specialScript.candyType = (generatedType == Candy.SpecialType.ColorBomb) ? 999 : colorId;
                specialScript.specialType = generatedType;

                allCandies[sx, sy] = specialScript;
            }
        }
    }

    private bool IsLineMatch(List<Candy> group)
    {
        bool sameX = true; bool sameY = true;
        int firstX = group[0].x; int firstY = group[0].y;
        foreach (Candy c in group)
        {
            if (c.x != firstX) sameX = false;
            if (c.y != firstY) sameY = false;
        }
        return sameX || sameY;
    }

    // ================= 💥 THUẬT TOÁN: TÍNH TOÁN BÁN KÍNH NỔ LAN 💥 =================

    public HashSet<Candy> GetSpecialExplosionArea(HashSet<Candy> baseMatches)
    {
        HashSet<Candy> extendedExplosions = new HashSet<Candy>(baseMatches);
        Queue<Candy> candiesToEvaluate = new Queue<Candy>(baseMatches);

        while (candiesToEvaluate.Count > 0)
        {
            Candy current = candiesToEvaluate.Dequeue();
            if (current == null) continue;

            switch (current.specialType)
            {
                case Candy.SpecialType.HorizontalStriped:
                    for (int col = 0; col < width; col++)
                        AddCandyToExplosionList(allCandies[col, current.y], extendedExplosions, candiesToEvaluate);
                    break;

                case Candy.SpecialType.VerticalStriped:
                    for (int row = 0; row < height; row++)
                        AddCandyToExplosionList(allCandies[current.x, row], extendedExplosions, candiesToEvaluate);
                    break;

                case Candy.SpecialType.Wrapped:
                    for (int nx = current.x - 1; nx <= current.x + 1; nx++)
                    {
                        for (int ny = current.y - 1; ny <= current.y + 1; ny++)
                        {
                            if (nx >= 0 && nx < width && ny >= 0 && ny < height)
                                AddCandyToExplosionList(allCandies[nx, ny], extendedExplosions, candiesToEvaluate);
                        }
                    }
                    break;
            }
        }
        return extendedExplosions;
    }

    private void AddCandyToExplosionList(Candy target, HashSet<Candy> explosionSet, Queue<Candy> evalQueue)
    {
        if (target != null && !explosionSet.Contains(target))
        {
            explosionSet.Add(target);
            evalQueue.Enqueue(target);
        }
    }

    // ================= 🚀 LOGIC: CÁC SIÊU COMBO ĐỔI CHỖ 🚀 =================

    private bool CheckAndExecuteSpecialCombo(Candy c1, Candy c2)
    {
        Candy.SpecialType t1 = c1.specialType;
        Candy.SpecialType t2 = c2.specialType;

        if (t1 == Candy.SpecialType.ColorBomb && t2 == Candy.SpecialType.ColorBomb)
        {
            ExecuteWholeBoardExplosion();
            return true;
        }

        if (t1 == Candy.SpecialType.Wrapped && t2 == Candy.SpecialType.Wrapped)
        {
            ExecuteDoubleWrappedCombo(c2.x, c2.y);
            return true;
        }

        if (t1 == Candy.SpecialType.ColorBomb || t2 == Candy.SpecialType.ColorBomb)
        {
            Candy bomb = (t1 == Candy.SpecialType.ColorBomb) ? c1 : c2;
            Candy special = (t1 == Candy.SpecialType.ColorBomb) ? c2 : c1;

            if (special.specialType == Candy.SpecialType.HorizontalStriped || special.specialType == Candy.SpecialType.VerticalStriped)
            {
                ExecuteColorBombWithStripeCombo(special.candyType);
                return true;
            }
            else if (special.specialType == Candy.SpecialType.Wrapped)
            {
                ExecuteColorBombWithWrappedCombo(special.candyType);
                return true;
            }
            else if (special.specialType == Candy.SpecialType.None)
            {
                ExecuteColorBombWithNormal(special.candyType, bomb);
                return true;
            }
        }

        bool isStripe1 = (t1 == Candy.SpecialType.HorizontalStriped || t1 == Candy.SpecialType.VerticalStriped);
        bool isStripe2 = (t2 == Candy.SpecialType.HorizontalStriped || t2 == Candy.SpecialType.VerticalStriped);

        if ((isStripe1 && t2 == Candy.SpecialType.Wrapped) || (isStripe2 && t1 == Candy.SpecialType.Wrapped))
        {
            ExecuteBigCrossExplosion(c2.x, c2.y);
            return true;
        }

        if (isStripe1 && isStripe2)
        {
            ExecuteNormalCrossExplosion(c2.x, c2.y);
            return true;
        }

        return false;
    }

    private void ExecuteWholeBoardExplosion()
    {
        HashSet<Candy> targets = new HashSet<Candy>();
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                if (allCandies[x, y] != null) targets.Add(allCandies[x, y]);

        StartCoroutine(DestroyAndRefillRoutine(targets));
    }

    private void ExecuteDoubleWrappedCombo(int targetX, int targetY)
    {
        StartCoroutine(DoubleWrappedRoutine(targetX, targetY));
    }

    private IEnumerator DoubleWrappedRoutine(int centerX, int centerY)
    {
        HashSet<Candy> targets1 = Get5x5ExplosionArea(centerX, centerY);
        HashSet<Candy> totalExploded1 = GetSpecialExplosionArea(targets1);

        DestroyMatches(totalExploded1);
        yield return new WaitForSeconds(0.25f);

        yield return StartCoroutine(DecreaseRowCo());
        yield return new WaitForSeconds(0.1f);

        HashSet<Candy> targets2 = Get5x5ExplosionArea(centerX, centerY);
        HashSet<Candy> totalExploded2 = GetSpecialExplosionArea(targets2);

        DestroyMatches(totalExploded2);
        yield return new WaitForSeconds(0.2f);

        yield return StartCoroutine(DecreaseRowCo());
        yield return StartCoroutine(RefillBoardCo());

        HashSet<Candy> nextMatches = FindAllMatches();
        if (nextMatches.Count > 0) yield return StartCoroutine(MatchAndFallRoutine(nextMatches));
        else isProcessing = false;
    }

    private HashSet<Candy> Get5x5ExplosionArea(int centerX, int centerY)
    {
        HashSet<Candy> area = new HashSet<Candy>();
        for (int nx = centerX - 2; nx <= centerX + 2; nx++)
        {
            for (int ny = centerY - 2; ny <= centerY + 2; ny++)
            {
                if (nx >= 0 && nx < width && ny >= 0 && ny < height)
                {
                    if (allCandies[nx, ny] != null) area.Add(allCandies[nx, ny]);
                }
            }
        }
        return area;
    }

    private void ExecuteNormalCrossExplosion(int targetX, int targetY)
    {
        HashSet<Candy> targets = new HashSet<Candy>();
        for (int i = 0; i < width; i++) if (allCandies[i, targetY] != null) targets.Add(allCandies[i, targetY]);
        for (int j = 0; j < height; j++) if (allCandies[targetX, j] != null) targets.Add(allCandies[targetX, j]);

        StartCoroutine(DestroyAndRefillRoutine(GetSpecialExplosionArea(targets)));
    }

    private void ExecuteBigCrossExplosion(int targetX, int targetY)
    {
        HashSet<Candy> targets = new HashSet<Candy>();
        for (int i = 0; i < width; i++)
        {
            for (int offset = -1; offset <= 1; offset++)
            {
                int ny = targetY + offset;
                if (ny >= 0 && ny < height && allCandies[i, ny] != null) targets.Add(allCandies[i, ny]);
            }
        }
        for (int j = 0; j < height; j++)
        {
            for (int offset = -1; offset <= 1; offset++)
            {
                int nx = targetX + offset;
                if (nx >= 0 && nx < width && allCandies[nx, j] != null) targets.Add(allCandies[nx, j]);
            }
        }
        StartCoroutine(DestroyAndRefillRoutine(GetSpecialExplosionArea(targets)));
    }

    private void ExecuteColorBombWithNormal(int targetColor, Candy bombCandy)
    {
        HashSet<Candy> targets = new HashSet<Candy> { bombCandy };
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                if (allCandies[x, y] != null && allCandies[x, y].candyType == targetColor)
                    targets.Add(allCandies[x, y]);

        StartCoroutine(DestroyAndRefillRoutine(GetSpecialExplosionArea(targets)));
    }

    private void ExecuteColorBombWithStripeCombo(int targetColor)
    {
        HashSet<Candy> targets = new HashSet<Candy>();
        SpecialCandyUIData data = candyDatabase.Find(d => d.candyTypeId == targetColor);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Candy c = allCandies[x, y];
                if (c != null && (c.candyType == targetColor || c.specialType == Candy.SpecialType.ColorBomb))
                {
                    targets.Add(c);
                    if (c.specialType != Candy.SpecialType.ColorBomb)
                    {
                        GameObject newPrefab = Random.value > 0.5f ? data.horizontalStripedPrefab : data.verticalStripedPrefab;
                        Candy.SpecialType newStripeType = Random.value > 0.5f ? Candy.SpecialType.HorizontalStriped : Candy.SpecialType.VerticalStriped;

                        Vector3 pos = GetWorldPosFromGrid(c.x, c.y);
                        int oldX = c.x, oldY = c.y;
                        Destroy(c.gameObject);

                        GameObject obj = Instantiate(newPrefab, pos, Quaternion.identity, this.transform);
                        Vector3 cellSize = candyTilemap.cellSize;
                        obj.transform.localScale = new Vector3(cellSize.x, cellSize.y, 1f);

                        Candy script = obj.GetComponent<Candy>();
                        script.x = oldX; script.y = oldY; script.candyType = targetColor;
                        script.specialType = newStripeType;
                        allCandies[oldX, oldY] = script;
                        targets.Add(script);
                    }
                }
            }
        }
        StartCoroutine(DestroyAndRefillRoutine(GetSpecialExplosionArea(targets)));
    }

    private void ExecuteColorBombWithWrappedCombo(int targetColor)
    {
        HashSet<Candy> targets = new HashSet<Candy>();
        SpecialCandyUIData data = candyDatabase.Find(d => d.candyTypeId == targetColor);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Candy c = allCandies[x, y];
                if (c != null && (c.candyType == targetColor || c.specialType == Candy.SpecialType.ColorBomb))
                {
                    targets.Add(c);
                    if (c.specialType != Candy.SpecialType.ColorBomb)
                    {
                        Vector3 pos = GetWorldPosFromGrid(c.x, c.y);
                        int oldX = c.x, oldY = c.y;
                        Destroy(c.gameObject);

                        GameObject obj = Instantiate(data.wrappedPrefab, pos, Quaternion.identity, this.transform);
                        Vector3 cellSize = candyTilemap.cellSize;
                        obj.transform.localScale = new Vector3(cellSize.x, cellSize.y, 1f);

                        Candy script = obj.GetComponent<Candy>();
                        script.x = oldX; script.y = oldY; script.candyType = targetColor;
                        script.specialType = Candy.SpecialType.Wrapped;
                        allCandies[oldX, oldY] = script;
                        targets.Add(script);
                    }
                }
            }
        }
        StartCoroutine(DestroyAndRefillRoutine(GetSpecialExplosionArea(targets)));
    }

    private IEnumerator DestroyAndRefillRoutine(HashSet<Candy> targets)
    {
        DestroyMatches(targets);
        yield return new WaitForSeconds(0.2f);
        yield return StartCoroutine(DecreaseRowCo());
        yield return StartCoroutine(RefillBoardCo());

        HashSet<Candy> nextMatches = FindAllMatches();
        if (nextMatches.Count > 0) yield return StartCoroutine(MatchAndFallRoutine(nextMatches));
        else isProcessing = false;
    }

    // =======================================================================

    private void DestroyMatches(HashSet<Candy> matches)
    {
        if (matches == null || matches.Count == 0) return;

        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.AddScore(matches.Count);
        }

        foreach (Candy candy in matches)
        {
            if (candy != null)
            {
                if (QuestManager.Instance != null)
                {
                    QuestManager.Instance.CheckCandyDestroyed(candy.candyType);
                }

                allCandies[candy.x, candy.y] = null;
                Destroy(candy.gameObject);
            }
        }
    }

    private void DestroyMatchesSilent(HashSet<Candy> matches)
    {
        foreach (Candy candy in matches)
        {
            if (candy != null)
            {
                allCandies[candy.x, candy.y] = null;
                Destroy(candy.gameObject);
            }
        }
    }

    private IEnumerator DecreaseRowCo()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (allCandies[x, y] == null)
                {
                    for (int k = y + 1; k < height; k++)
                    {
                        if (allCandies[x, k] != null)
                        {
                            allCandies[x, y] = allCandies[x, k];
                            allCandies[x, y].y = y;
                            allCandies[x, k] = null;

                            Vector3 targetPos = GetWorldPosFromGrid(x, y);
                            StartCoroutine(MoveCandyAnimation(allCandies[x, y].transform, targetPos, 0.15f));
                            break;
                        }
                    }
                }
            }
        }
        yield return new WaitForSeconds(0.15f);
    }

    private IEnumerator RefillBoardCo()
    {
        Vector3 cellSize = candyTilemap.cellSize;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (allCandies[x, y] == null)
                {
                    // Sinh kẹo từ vị trí biên giới của bàn cờ tùy biến thay vì biên Tilemap dôi dư
                    Vector3 spawnPos = GetWorldPosFromGrid(x, height - 1);
                    spawnPos.y += cellSize.y;

                    int randomIndex = Random.Range(0, candyPrefabs.Length);
                    GameObject newCandyObj = Instantiate(candyPrefabs[randomIndex], spawnPos, Quaternion.identity);
                    newCandyObj.transform.parent = this.transform;

                    newCandyObj.transform.localScale = new Vector3(cellSize.x, cellSize.y, 1f);

                    Candy candyScript = newCandyObj.GetComponent<Candy>();
                    candyScript.x = x;
                    candyScript.y = y;
                    candyScript.candyType = randomIndex;
                    candyScript.specialType = Candy.SpecialType.None;

                    allCandies[x, y] = candyScript;

                    Vector3 targetPos = GetWorldPosFromGrid(x, y);
                    StartCoroutine(MoveCandyAnimation(newCandyObj.transform, targetPos, 0.2f));
                }
            }
        }
        yield return new WaitForSeconds(0.2f);
    }

    private IEnumerator MoveCandyAnimation(Transform candyTransform, Vector3 targetPos, float duration)
    {
        float time = 0;
        Vector3 startPos = candyTransform.position;
        while (time < duration)
        {
            if (candyTransform == null) yield break;
            candyTransform.position = Vector3.Lerp(startPos, targetPos, time / duration);
            time += Time.deltaTime;
            yield return null;
        }
        if (candyTransform != null) candyTransform.position = targetPos;
    }

    public HashSet<Candy> FindAllMatches()
    {
        HashSet<Candy> matchingCandies = new HashSet<Candy>();

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width - 2; x++)
            {
                Candy c1 = allCandies[x, y]; Candy c2 = allCandies[x + 1, y]; Candy c3 = allCandies[x + 2, y];
                if (c1 != null && c2 != null && c3 != null && c1.candyType == c2.candyType && c2.candyType == c3.candyType)
                {
                    if (c1.specialType == Candy.SpecialType.ColorBomb || c2.specialType == Candy.SpecialType.ColorBomb) continue;
                    matchingCandies.Add(c1); matchingCandies.Add(c2); matchingCandies.Add(c3);
                }
            }
        }

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height - 2; y++)
            {
                Candy c1 = allCandies[x, y]; Candy c2 = allCandies[x, y + 1]; Candy c3 = allCandies[x, y + 2];
                if (c1 != null && c2 != null && c3 != null && c1.candyType == c2.candyType && c2.candyType == c3.candyType)
                {
                    if (c1.specialType == Candy.SpecialType.ColorBomb || c2.specialType == Candy.SpecialType.ColorBomb) continue;
                    matchingCandies.Add(c1); matchingCandies.Add(c2); matchingCandies.Add(c3);
                }
            }
        }
        return matchingCandies;
    }
}