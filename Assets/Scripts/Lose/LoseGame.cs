using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class LoseGame : MonoBehaviour
{
    public static LoseGame Instance;

    [Header("UI Panels Reference")]
    public GameObject losePopupPanel;       // Kéo thả Object 'LosePopup' gốc vào đây
    public TextMeshProUGUI levelText;      // Kéo thả Object 'LevelText' nằm trong LosePopup vào đây
    public TextMeshProUGUI titleText;      // Kéo thả Text tiêu đề (VD: So Close!) vào đây
    public Button retryButton;             // Kéo thả Object 'RetryButton' vào đây

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (losePopupPanel != null) losePopupPanel.SetActive(false);
    }

    private void Start()
    {
        if (retryButton != null)
        {
            retryButton.onClick.AddListener(OnRetryButtonClicked);
        }
    }

    public void ShowLoseWindow()
    {
        if (losePopupPanel == null) return;

        losePopupPanel.SetActive(true);

        // TỰ ĐỘNG TÍNH LEVEL CHO LOSE POPUP
        if (levelText != null)
        {
            int buildIndex = SceneManager.GetActiveScene().buildIndex;
            // Thuật toán thông minh: Nếu chưa có Main Menu (index đang là 0) thì in ra Level 1.
            // Sau này nếu có Main Menu chiếm vị trí 0, Scene hiện tại nhảy lên Index 1 -> Code giữ nguyên Level 1 và tự tăng lũy tiến tự động!
            int currentLevel = (buildIndex == 0) ? 1 : buildIndex - 1;
            levelText.text = "Level " + currentLevel;
        }

        StartCoroutine(AnimateLoseScreenRoutine());
    }

    private IEnumerator AnimateLoseScreenRoutine()
    {
        if (titleText != null)
        {
            titleText.text = "So close!";
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
    }

    public void OnRetryButtonClicked()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(currentSceneIndex);
    }
}