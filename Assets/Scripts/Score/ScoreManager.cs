using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    // Áp dụng mô hình Singleton để BoardManager dễ dàng gọi từ bất cứ đâu mà không cần kéo thả liên kết
    public static ScoreManager Instance { get; private set; }

    [Header("UI Reference")]
    public TextMeshProUGUI scoreText; // Nơi kéo thả ô chữ hiển thị điểm từ Hierarchy vào

    [Header("Candy Crush Score Settings")]
    public int scorePerCandy = 60;    // Điểm cơ bản cho mỗi viên kẹo nổ

    private int currentScore = 0;     // Tổng điểm tích lũy của màn chơi
    private int comboMultiplier = 1;  // Hệ số nhân nổ chuỗi liên hoàn (Cascade Combo)

    private void Awake()
    {
        // Khởi tạo và bảo vệ cấu trúc Singleton độc nhất
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        ResetCombo();
        UpdateScoreUI();
    }

    /// <summary>
    /// Cộng điểm dựa trên số kẹo nổ ở lượt hiện tại và hệ số nhân combo
    /// </summary>
    public void AddScore(int candyCount)
    {
        int scoreGained = candyCount * scorePerCandy * comboMultiplier;
        currentScore += scoreGained;

        Debug.Log($"[COMBO x{comboMultiplier}] Đã nổ {candyCount} viên kẹo! +{scoreGained} điểm. (Tổng điểm: {currentScore})");

        UpdateScoreUI();

        // >>> BỔ SUNG DÒNG NÀY: Cập nhật thanh đo sao theo tổng điểm mới nhất
        if (StarManager.Instance != null)
        {
            StarManager.Instance.UpdateStarMeter(currentScore);
        }
    }

    /// <summary>
    /// Tăng hệ số nhân khi chuỗi kẹo tự động rơi trúng nhau liên tiếp
    /// </summary>
    public void IncreaseCombo()
    {
        comboMultiplier++;
    }

    /// <summary>
    /// Reset hệ số combo về 1 khi kết thúc một chuỗi nổ (người chơi chuẩn bị đi lượt mới)
    /// </summary>
    public void ResetCombo()
    {
        comboMultiplier = 1;
    }

    /// <summary>
    /// Cập nhật nội dung hiển thị điểm số thực tế ra giao diện UI
    /// </summary>
    private void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + currentScore.ToString();
        }
    }

    // Hàm phụ trợ giúp lấy điểm số hiện tại sang các hệ thống khác (Ví dụ: tính số sao, màn hình Thắng/Thua)
    public int GetCurrentScore()
    {
        return currentScore;
    }
}