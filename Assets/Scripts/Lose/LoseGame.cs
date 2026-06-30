using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class LoseGame : MonoBehaviour
{
    public static LoseGame Instance;

    [Header("UI Panels Reference")]
    public GameObject losePopupPanel;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI titleText;
    public Button retryButton;
    public Button closeButton;

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

        if (closeButton != null)
        {
            closeButton.onClick.AddListener(OnCloseButtonClicked);
        }
    }

    public void ShowLoseWindow()
    {
        if (losePopupPanel == null) return;

        losePopupPanel.SetActive(true);

        if (LivesManager.Instance != null)
        {
            LivesManager.Instance.ReduceLive();
        }

        if (levelText != null)
        {
            int buildIndex = SceneManager.GetActiveScene().buildIndex;
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
        if (LivesManager.Instance != null && !LivesManager.Instance.HasEnoughLives())
        {
            Debug.Log("Không đủ mạng để chơi lại!");
            return;
        }

        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(currentSceneIndex);
    }

    public void OnCloseButtonClicked()
    {
        SceneManager.LoadScene("LevelSelect");
    }
}