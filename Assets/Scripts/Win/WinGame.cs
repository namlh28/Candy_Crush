using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class WinGame : MonoBehaviour
{
    public static WinGame Instance;

    [Header("UI Panels Reference")]
    public GameObject winPopupPanel;        // Kéo chính Object 'WinPopup' vào đây
    public TextMeshProUGUI levelText;       // Kéo Object 'LevelText' nằm trong WinPopup vào đây
    public TextMeshProUGUI titleText;       // Kéo Object 'TileText' (Chữ Divine!) vào đây
    public TextMeshProUGUI scoreText;       // Kéo Object 'ScoreText' hiển thị điểm vào đây
    public Button nextButton;               // Kéo Object 'NextButton' vào đây

    [Header("Stars UI Setup")]
    public Image[] stars;                   // Kéo lần lượt Star1, Star2, Star3 vào đây

    [Header("Score Thresholds (Chỉ dùng cho 1 và 2 Sao)")]
    public int star1Score = 1500;
    public int star2Score = 4000;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (winPopupPanel != null) winPopupPanel.SetActive(false);
    }

    private void Start()
    {
        if (nextButton != null)
        {
            nextButton.onClick.AddListener(OnNextLevelButtonClicked);
        }
    }

    public void ShowWinWindow(int finalScore, bool isQuestWin)
    {
        winPopupPanel.SetActive(true);

        // ĐỒNG BỘ HIỂN THỊ LEVEL CHUẨN TỪ LEVEL MANAGER TRUNG TÂM
        if (levelText != null)
        {
            levelText.text = "Level " + LevelManager.CurrentSelectedLevel;
        }

        // TỰ ĐỘNG MỞ KHÓA MÀN CHƠI TIẾP THEO TRÊN BẢN ĐỒ
        LevelManager.UnlockNextLevel(LevelManager.CurrentSelectedLevel);

        foreach (Image star in stars)
        {
            if (star != null) star.color = new Color32(69, 69, 69, 255);
        }

        int starsEarned = 1;

        if (isQuestWin)
        {
            starsEarned = 3;
        }
        else
        {
            if (finalScore >= star2Score) starsEarned = 2;
            else if (finalScore >= star1Score) starsEarned = 1;
        }

        StartCoroutine(AnimateScoreProgressRoutine());
        StartCoroutine(AnimateWinScreenRoutine(starsEarned));
    }

    private IEnumerator AnimateScoreProgressRoutine()
    {
        float duration = 1.5f;
        float timer = 0f;
        int startScore = 0;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float progress = timer / duration;

            int currentTargetScore = startScore;
            if (ScoreManager.Instance != null)
            {
                currentTargetScore = ScoreManager.Instance.currentScore;
            }

            int displayScore = Mathf.RoundToInt(Mathf.Lerp(startScore, currentTargetScore, progress));

            if (scoreText != null)
            {
                scoreText.text = displayScore.ToString();
            }

            yield return null;
        }

        if (ScoreManager.Instance != null && scoreText != null)
        {
            scoreText.text = ScoreManager.Instance.currentScore.ToString();
        }
    }

    private IEnumerator AnimateWinScreenRoutine(int totalStarsActive)
    {
        if (titleText != null)
        {
            titleText.text = "Divine!";
            titleText.transform.localScale = Vector3.zero;

            float timer = 0f;
            float duration = 0.3f;
            while (timer < duration)
            {
                timer += Time.deltaTime;
                float progress = timer / duration;

                float scale = Mathf.Lerp(0f, 1.3f, progress);
                if (progress > 0.7f)
                {
                    scale = Mathf.Lerp(1.3f, 1.0f, (progress - 0.7f) / 0.3f);
                }
                titleText.transform.localScale = Vector3.one * scale;
                yield return null;
            }
            titleText.transform.localScale = Vector3.one;
        }

        yield return new WaitForSeconds(0.2f);

        for (int i = 0; i < totalStarsActive; i++)
        {
            if (i < stars.Length && stars[i] != null)
            {
                Image targetStar = stars[i];
                Vector3 originalScale = targetStar.transform.localScale;

                float timer = 0f;
                float duration = 0.25f;

                while (timer < duration)
                {
                    timer += Time.deltaTime;
                    float progress = timer / duration;

                    targetStar.color = Color.Lerp(new Color32(69, 69, 69, 255), Color.white, progress);

                    float scaleMultiplier = Mathf.Sin(progress * Mathf.PI) * 0.2f;
                    targetStar.transform.localScale = originalScale * (1f + scaleMultiplier);

                    yield return null;
                }

                targetStar.transform.localScale = originalScale;
                yield return new WaitForSeconds(0.25f);
            }
        }
    }

    /// <summary>
    /// Xử lý sự kiện khi nhấn nút Next Level trên popup thắng trận
    /// </summary>
    public void OnNextLevelButtonClicked()
    {
        // Khi chơi xong một màn, quay về Scene chọn màn (LevelSelect) 
        // để hệ thống tự load lại danh sách 1000 ô tròn và mở khóa ô tiếp theo.
        SceneManager.LoadScene("LevelSelect");
    }
}