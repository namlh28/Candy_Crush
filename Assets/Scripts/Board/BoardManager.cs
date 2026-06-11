using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BoardManager : MonoBehaviour
{
    [Header("Tilemap Setup")]
    public Tilemap candyTilemap;

    [Header("Candy Prefabs")]
    public GameObject[] candyPrefabs;

    [Header("Board Settings")]
    public int width = 8;
    public int height = 8;
    public Candy[,] allCandies;

    private BoundsInt boardBounds;
    private bool isProcessing = false; // Khóa chống người chơi click khi kẹo đang rơi

    void Start()
    {
        // 1. Lấy vùng biên thực tế của Tilemap bạn đã vẽ
        boardBounds = candyTilemap.cellBounds;
        width = boardBounds.size.x;
        height = boardBounds.size.y;

        // 2. Khởi tạo mảng ô trống ban đầu
        allCandies = new Candy[width, height];

        // 3. Kích hoạt chuỗi thả kẹo từ trên trời xuống lấp đầy bảng lần đầu tiên
        StartCoroutine(InitializeBoardRoutine());
    }

    /// <summary>
    /// Chuỗi khởi tạo: Thả kẹo từ trên xuống -> Kiểm tra có match sẵn không -> Khử match trùng đầu game
    /// </summary>
    private IEnumerator InitializeBoardRoutine()
    {
        isProcessing = true;

        // Thả kẹo đầy bảng
        yield return StartCoroutine(RefillBoardCo());

        // Vòng lặp khử toàn bộ các cụm Match-3 vô tình xuất hiện khi mới sinh ngẫu nhiên
        HashSet<Candy> matches = FindAllMatches();
        while (matches.Count > 0)
        {
            // Sử dụng hàm hủy im lặng, dọn sạch bàn cờ đầu game không tính điểm
            DestroyMatchesSilent(matches);
            yield return new WaitForSeconds(0.2f);
            yield return StartCoroutine(DecreaseRowCo());
            yield return StartCoroutine(RefillBoardCo());
            matches = FindAllMatches();
        }

        isProcessing = false;
        Debug.Log("Bàn chơi đã chuẩn bị xong và sẵn sàng tương tác!");
    }

    /// <summary>
    /// Tính toán tọa độ thế giới (World Position) chuẩn dựa trên chỉ số mảng (x, y)
    /// </summary>
    public Vector3 GetWorldPosFromGrid(int x, int y)
    {
        int tileX = x + boardBounds.xMin;
        int tileY = y + boardBounds.yMin;
        return candyTilemap.GetCellCenterWorld(new Vector3Int(tileX, tileY, 0));
    }

    // ================= LOGIC SWAP & ĐIỀU PHỐI CHUỖI NỔ COMBO =================

    /// <summary>
    /// Coroutine xử lý việc đổi chỗ 2 viên kẹo, kiểm tra match, nếu không hợp lệ thì trả về chỗ cũ.
    /// </summary>
    /// <summary>
    /// Coroutine xử lý việc đổi chỗ 2 viên kẹo, kiểm tra match, nếu không hợp lệ thì trả về chỗ cũ.
    /// </summary>
    public IEnumerator SwapCandiesRoutine(int x1, int y1, int x2, int y2)
    {
        // BỔ SUNG: Nếu game đã kết thúc (thua/thắng) thì chặn không cho người chơi swap kẹo nữa
        if (MoveManager.Instance != null && MoveManager.Instance.IsGameOver()) yield break;
        if (isProcessing) yield break;

        Candy candy1 = allCandies[x1, y1];
        Candy candy2 = allCandies[x2, y2];

        if (candy1 == null || candy2 == null) yield break;

        isProcessing = true;

        // 1. Hoán đổi dữ liệu trong mảng 2D trước
        allCandies[x1, y1] = candy2;
        allCandies[x2, y2] = candy1;

        candy1.x = x2; candy1.y = y2;
        candy2.x = x1; candy2.y = y1;

        // 2. Chạy hiệu ứng di chuyển đổi chỗ mượt mà trên màn hình
        StartCoroutine(MoveCandyAnimation(candy1.transform, GetWorldPosFromGrid(x2, y2), 0.2f));
        yield return StartCoroutine(MoveCandyAnimation(candy2.transform, GetWorldPosFromGrid(x1, y1), 0.2f));

        // 3. Quét kiểm tra cụm Match-3
        HashSet<Candy> matches = FindAllMatches();

        if (matches.Count > 0)
        {
            // Bắt đầu một nước đi mới, reset hệ số nhân combo bên ScoreManager về 1
            if (ScoreManager.Instance != null) ScoreManager.Instance.ResetCombo();

            // >>> BỔ SUNG: Trừ 1 lượt di chuyển của người chơi khi có Match thành công
            if (MoveManager.Instance != null)
            {
                MoveManager.Instance.UseAMove();
            }

            // Có Match thành công! Chuyển quyền xử lý cho chuỗi nổ và cascade rơi kẹo
            yield return StartCoroutine(MatchAndFallRoutine(matches));
        }
        else
        {
            // KHÔNG CÓ MATCH HỢP LỆ -> Hủy nước đi, hoàn trả kẹo về vị trí ban đầu
            allCandies[x1, y1] = candy1;
            allCandies[x2, y2] = candy2;

            candy1.x = x1; candy1.y = y1;
            candy2.x = x2; candy2.y = y2;

            StartCoroutine(MoveCandyAnimation(candy1.transform, GetWorldPosFromGrid(x1, y1), 0.2f));
            yield return StartCoroutine(MoveCandyAnimation(candy2.transform, GetWorldPosFromGrid(x2, y2), 0.2f));

            isProcessing = false;
        }
    }

    /// <summary>
    /// Kích hoạt chuỗi xử lý (Nổ -> Rơi -> Đổ đầy) sau khi người chơi thực hiện Match thành công
    /// </summary>
    public void ProcessMatches()
    {
        if (isProcessing) return;
        HashSet<Candy> matches = FindAllMatches();
        if (matches.Count > 0)
        {
            if (ScoreManager.Instance != null) ScoreManager.Instance.ResetCombo();
            StartCoroutine(MatchAndFallRoutine(matches));
        }
    }

    private IEnumerator MatchAndFallRoutine(HashSet<Candy> matches)
    {
        isProcessing = true;

        // 1. Xóa kẹo nổ đồng thời kích hoạt báo điểm sang ScoreManager
        DestroyMatches(matches);
        yield return new WaitForSeconds(0.2f);

        // 2. Đẩy kẹo trên cao rơi xuống ô trống
        yield return StartCoroutine(DecreaseRowCo());
        yield return new WaitForSeconds(0.1f);

        // 3. Sinh kẹo mới lấp đầy bàn chơi
        yield return StartCoroutine(RefillBoardCo());

        // 4. KIỂM TRA ĐỆ QUY CHUỖI NỔ (Cascade): Xem kẹo mới rơi xuống có tự tạo thành Match mới không
        HashSet<Candy> nextMatches = FindAllMatches();
        if (nextMatches.Count > 0)
        {
            // Tăng hệ số nhân combo bên hệ thống tính điểm lên trước khi nổ tiếp đợt sau
            if (ScoreManager.Instance != null) ScoreManager.Instance.IncreaseCombo();

            yield return StartCoroutine(MatchAndFallRoutine(nextMatches));
        }
        else
        {
            // Bàn chơi dừng hẳn, reset combo chuẩn bị cho lượt vuốt mới
            if (ScoreManager.Instance != null) ScoreManager.Instance.ResetCombo();
            isProcessing = false;
        }
    }

    /// <summary>
    /// Hàm xóa kẹo nổ và báo dữ liệu sang hệ thống điểm số
    /// </summary>
    private void DestroyMatches(HashSet<Candy> matches)
    {
        if (matches == null || matches.Count == 0) return;

        // GỬI LỆNH: Báo số lượng kẹo bị phá hủy sang ScoreManager để lo tính toán
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.AddScore(matches.Count);
        }

        // Tiến hành xóa GameObject khỏi Scene
        foreach (Candy candy in matches)
        {
            if (candy != null)
            {
                // BỔ SUNG: Báo loại kẹo vừa nổ sang QuestManager trước khi hủy vật thể
                if (QuestManager.Instance != null)
                {
                    QuestManager.Instance.CheckCandyDestroyed(candy.candyType);
                }

                allCandies[candy.x, candy.y] = null;
                Destroy(candy.gameObject);
            }
        }
    }

    /// <summary>
    /// Hàm xóa kẹo im lặng để làm sạch bàn cờ lúc bắt đầu game
    /// </summary>
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

    // =======================================================================

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
                    Vector3 spawnPos = GetWorldPosFromGrid(x, height);
                    spawnPos.y += 0.5f;

                    int randomIndex = Random.Range(0, candyPrefabs.Length);
                    GameObject newCandyObj = Instantiate(candyPrefabs[randomIndex], spawnPos, Quaternion.identity);
                    newCandyObj.transform.parent = this.transform;
                    newCandyObj.transform.localScale = new Vector3(cellSize.x, cellSize.y, 1f);

                    Candy candyScript = newCandyObj.GetComponent<Candy>();
                    candyScript.x = x;
                    candyScript.y = y;
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

        // Quét ngang
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width - 2; x++)
            {
                Candy c1 = allCandies[x, y]; Candy c2 = allCandies[x + 1, y]; Candy c3 = allCandies[x + 2, y];
                if (c1 != null && c2 != null && c3 != null && c1.candyType == c2.candyType && c2.candyType == c3.candyType)
                {
                    matchingCandies.Add(c1); matchingCandies.Add(c2); matchingCandies.Add(c3);
                }
            }
        }
        // Quét dọc
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height - 2; y++)
            {
                Candy c1 = allCandies[x, y]; Candy c2 = allCandies[x, y + 1]; Candy c3 = allCandies[x, y + 2];
                if (c1 != null && c2 != null && c3 != null && c1.candyType == c2.candyType && c2.candyType == c3.candyType)
                {
                    matchingCandies.Add(c1); matchingCandies.Add(c2); matchingCandies.Add(c3);
                }
            }
        }
        return matchingCandies;
    }
}