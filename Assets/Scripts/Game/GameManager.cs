using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("--- UI Popups ---")]
    [SerializeField] private GameObject losePopupPanel;

    public void OnLevelWin()
    {
        int playingLevel = LevelManager.CurrentSelectedLevel;
        LevelManager.UnlockNextLevel(playingLevel);
        ReturnToMap();
    }

    public void OnLevelLose()
    {
        if (losePopupPanel != null)
        {
            losePopupPanel.SetActive(true);
        }

        if (LivesManager.Instance != null)
        {
            LivesManager.Instance.ReduceLive();
        }
    }

    public void OnRetryButtonClick()
    {
        if (LivesManager.Instance != null && LivesManager.Instance.HasEnoughLives())
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
        else
        {
            Debug.Log("Không đủ mạng để Retry!");
        }
    }

    public void OnCloseLosePopupClick()
    {
        ReturnToMap();
    }

    private void ReturnToMap()
    {
        SceneManager.LoadScene("MapSelect");
    }
}