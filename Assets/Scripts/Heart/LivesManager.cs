using UnityEngine;
using TMPro;
using System;
using UnityEngine.SceneManagement;

public class LivesManager : MonoBehaviour
{
    public static LivesManager Instance { get; private set; }

    private TextMeshProUGUI liveCountText;
    private TextMeshProUGUI statusText;

    [Header("--- Configuration ---")]
    [SerializeField] private int maxLives = 5;
    [SerializeField] private int restoreDuration = 1770;

    private int currentLives;
    private DateTime nextLiveTime;

    private const string LIVES_KEY = "CurrentLivesAmount";
    private const string NEXT_LIVE_TIME_KEY = "NextLiveTimeTicks";

    private void Awake()
    {
        // Quản lý Singleton chuẩn hóa
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadLivesData();
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        UpdateLivesUI();
    }

    private void Update()
    {
        if (currentLives < maxLives)
        {
            UpdateTimer();
        }
    }

    // Hàm tiếp nhận UI từ Scene mới gửi tới
    public void RegisterUI(TextMeshProUGUI countText, TextMeshProUGUI statText)
    {
        // Làm sạch bộ nhớ đệm để tránh giữ lại ô chữ cũ từ Scene trước
        liveCountText = null;
        statusText = null;

        // Gán ô chữ mới của Scene hiện tại vào
        liveCountText = countText;
        statusText = statText;

        // Nạp dữ liệu và cập nhật giao diện ngay lập tức
        LoadLivesData();
        UpdateLivesUI();
    }

    private void LoadLivesData()
    {
        currentLives = PlayerPrefs.GetInt(LIVES_KEY, maxLives);
        string savedTimeStr = PlayerPrefs.GetString(NEXT_LIVE_TIME_KEY, string.Empty);

        if (!string.IsNullOrEmpty(savedTimeStr))
        {
            long ticks = Convert.ToInt64(savedTimeStr);
            nextLiveTime = new DateTime(ticks);

            TimeSpan timePassed = DateTime.Now - nextLiveTime;

            if (timePassed.TotalSeconds > 0)
            {
                currentLives++;
                double remainingSeconds = timePassed.TotalSeconds;

                while (remainingSeconds >= restoreDuration && currentLives < maxLives)
                {
                    currentLives++;
                    remainingSeconds -= restoreDuration;
                }

                if (currentLives >= maxLives)
                {
                    currentLives = maxLives;
                    PlayerPrefs.SetString(NEXT_LIVE_TIME_KEY, string.Empty);
                }
                else
                {
                    nextLiveTime = DateTime.Now.AddSeconds(restoreDuration - remainingSeconds);
                    PlayerPrefs.SetString(NEXT_LIVE_TIME_KEY, nextLiveTime.Ticks.ToString());
                }

                SaveLivesData();
            }
        }
    }

    private void UpdateTimer()
    {
        TimeSpan remainingTime = nextLiveTime - DateTime.Now;

        if (remainingTime.TotalSeconds <= 0)
        {
            currentLives++;
            if (currentLives >= maxLives)
            {
                currentLives = maxLives;
                PlayerPrefs.SetString(NEXT_LIVE_TIME_KEY, string.Empty);
            }
            else
            {
                nextLiveTime = DateTime.Now.AddSeconds(restoreDuration);
                PlayerPrefs.SetString(NEXT_LIVE_TIME_KEY, nextLiveTime.Ticks.ToString());
            }
            SaveLivesData();
            UpdateLivesUI();
        }
        else
        {
            // Chỉ hiển thị thời gian đếm ngược mm:ss lên ô statusText ở màn LevelSelect
            if (statusText != null && SceneManager.GetActiveScene().name != "Game")
            {
                statusText.text = string.Format("{0}:{1:00}", remainingTime.Minutes, remainingTime.Seconds);
            }
        }
    }

    public void ReduceLive()
    {
        if (currentLives <= 0) return;

        if (currentLives == maxLives)
        {
            nextLiveTime = DateTime.Now.AddSeconds(restoreDuration);
            PlayerPrefs.SetString(NEXT_LIVE_TIME_KEY, nextLiveTime.Ticks.ToString());
        }

        currentLives--;
        SaveLivesData();
        UpdateLivesUI();
    }

    public bool HasEnoughLives() => currentLives > 0;

    private void SaveLivesData()
    {
        PlayerPrefs.SetInt(LIVES_KEY, currentLives);
        PlayerPrefs.Save();
    }


    public void UpdateLivesUI()
    {
        // 1. Luôn hiển thị số lượng lượt chơi (5, 4, 3, 2, 1, 0) ở CẢ HAI MÀN HÌNH
        if (liveCountText != null)
        {
            liveCountText.text = currentLives.ToString();
        }

        // 2. Nếu là màn chơi Game (màn xanh), dừng ngay tại đây, tuyệt đối không đụng đến chữ "Full"
        if (SceneManager.GetActiveScene().name == "Game")
        {
            return;
        }

        // 3. Logic dành riêng cho màn LevelSelect (màn hồng)
        if (statusText != null)
        {
            if (currentLives >= maxLives)
            {
                statusText.text = "Full"; // Chỉ hiện "Full" ở ô bộ đếm ngoài sảnh
            }
        }
    }
}