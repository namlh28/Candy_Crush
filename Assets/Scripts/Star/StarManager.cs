using UnityEngine;
using UnityEngine.UI;

public class StarManager : MonoBehaviour
{
    public static StarManager Instance { get; private set; }

    [Header("Star Score Thresholds (Chỉ cho Sao 1 và Sao 2)")]
    public int scoreFor1Star = 1000;
    public int scoreFor2Stars = 2500;

    [Header("UI References")]
    public Slider starSlider;
    public Image star1Image;
    public Image star2Image;
    public Image star3Image;

    [Header("Color Settings")]
    public Color lockedColor = new Color(0.3f, 0.2f, 0.1f, 1f);
    public Color unlockedColor = Color.white;

    private int currentStarsEarned = 0;
    private bool isQuestCompleted = false;

    private void Awake()
    {
        if (Instance == null) { Instance = this; }
        else { Destroy(gameObject); }
    }

    private void Start()
    {
        if (starSlider != null)
        {
            starSlider.minValue = 0;
            // Cho thanh Slider đầy khi đạt mốc 2 sao, hoặc bạn có thể đặt một mốc điểm tối đa tùy ý
            starSlider.maxValue = scoreFor2Stars * 1.2f;
            starSlider.value = 0;
        }

        ResetStarsStatus();
    }

    /// <summary>
    /// Hàm cập nhật điểm số - Chỉ kiểm tra Sao 1 và Sao 2
    /// </summary>
    public void UpdateStarMeter(int currentScore)
    {
        if (starSlider != null)
        {
            starSlider.value = currentScore;
        }

        // Kiểm tra mốc 1 sao dựa vào điểm
        if (currentScore >= scoreFor1Star)
        {
            if (star1Image != null) star1Image.color = unlockedColor;
            if (currentStarsEarned < 1) currentStarsEarned = 1;
        }
        else
        {
            if (star1Image != null) star1Image.color = lockedColor;
        }

        // Kiểm tra mốc 2 sao dựa vào điểm
        if (currentScore >= scoreFor2Stars)
        {
            if (star2Image != null) star2Image.color = unlockedColor;
            if (currentStarsEarned < 2) currentStarsEarned = 2;
        }
        else
        {
            if (star2Image != null) star2Image.color = lockedColor;
        }
    }

    /// <summary>
    /// Hàm này sẽ được gọi từ QuestManager khi người chơi hoàn thành nhiệm vụ thu thập kẹo
    /// </summary>
    public void CompleteQuestAndEarnThirdStar()
    {
        isQuestCompleted = true;
        if (star3Image != null)
        {
            star3Image.color = unlockedColor; // Ngôi sao số 3 hóa vàng!
        }

        // Cập nhật số lượng sao tối đa thu thập được
        if (currentStarsEarned < 3)
        {
            currentStarsEarned = 3;
        }
    }

    public void ResetStarsStatus()
    {
        if (star1Image != null) star1Image.color = lockedColor;
        if (star2Image != null) star2Image.color = lockedColor;
        if (star3Image != null) star3Image.color = lockedColor;
        currentStarsEarned = 0;
        isQuestCompleted = false;
    }

    public int GetStarsEarned()
    {
        return currentStarsEarned;
    }
}