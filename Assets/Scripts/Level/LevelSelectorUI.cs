using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class LevelSelectorUI : MonoBehaviour
{
    [Header("--- Map Container Setup ---")]
    public Transform levelContainer;

    [Header("--- Popup UI Setup ---")]
    public GameObject infoPopupPanel;
    public TextMeshProUGUI popupTitleText;
    public Button playButton;
    public Button closeButton;

    private void Start()
    {
        if (infoPopupPanel != null) infoPopupPanel.SetActive(false);

        if (playButton != null) playButton.onClick.AddListener(StartGame);
        if (closeButton != null) closeButton.onClick.AddListener(ClosePopup);

        RefreshExistingLevels();
    }

    private void OnEnable()
    {
        RefreshExistingLevels();
    }

    private void RefreshExistingLevels()
    {
        if (levelContainer == null) return;

        int highestUnlocked = LevelManager.GetLevelReached();
        int totalNodes = levelContainer.childCount;

        for (int i = 0; i < totalNodes; i++)
        {
            Transform child = levelContainer.GetChild(i);
            int levelNum = i + 1;

            Button btn = child.GetComponent<Button>();
            TextMeshProUGUI txt = child.GetComponentInChildren<TextMeshProUGUI>();

            if (txt != null) txt.text = levelNum.ToString();

            if (levelNum <= highestUnlocked)
            {
                btn.interactable = true;
                btn.image.color = Color.white;

                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => OpenPopup(levelNum));
            }
            else
            {
                btn.interactable = false;
                btn.image.color = new Color32(100, 100, 100, 255);
            }
        }
    }

    public void OpenPopup(int levelId)
    {
        LevelManager.CurrentSelectedLevel = levelId;

        if (infoPopupPanel != null)
        {
            infoPopupPanel.SetActive(true);
            if (popupTitleText != null) popupTitleText.text = "Level " + levelId;
        }
    }

    public void ClosePopup()
    {
        if (infoPopupPanel != null) infoPopupPanel.SetActive(false);
    }

    public void StartGame()
    {
        if (LivesManager.Instance != null && !LivesManager.Instance.HasEnoughLives())
        {
            Debug.Log("Không đủ mạng để vào chơi!");
            return;
        }

        SceneManager.LoadScene("Game");
    }
}