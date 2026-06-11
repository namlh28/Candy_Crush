using UnityEngine;
using UnityEngine.UI;

public class StarManager : MonoBehaviour
{
    public static StarManager Instance { get; private set; }

    [Header("Star Score Thresholds")]
    public int scoreFor1Star = 1000;
    public int scoreFor2Stars = 2500;
    public int scoreFor3Stars = 5000;

    [Header("UI References (Kéo 3 ngôi sao UI vào đây)")]
    public Slider starSlider;
    public Image star1Image;
    public Image star2Image;
    public Image star3Image;

    [Header("Color Settings")]
    // Màu tối lúc chưa đạt điểm (Màu nâu xám tối)
    public Color lockedColor = new Color(0.3f, 0.2f, 0.1f, 1f);
    // Màu sáng gốc rực rỡ khi đủ điểm (Màu trắng tinh trong Unity sẽ giữ nguyên màu gốc của ảnh)
    public Color unlockedColor = Color.white;

    private int currentStarsEarned = 0;

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
            starSlider.maxValue = scoreFor3Stars;
            starSlider.value = 0;
        }

        ResetStarsStatus();
    }

    public void UpdateStarMeter(int currentScore)
    {
        if (starSlider != null)
        {
            starSlider.value = currentScore;
        }

        // --- KIỂM TRA MỐC 1 SAO ---
        if (currentScore >= scoreFor1Star)
        {
            if (star1Image != null) star1Image.color = unlockedColor;
            if (currentStarsEarned < 1) currentStarsEarned = 1;
        }
        else
        {
            if (star1Image != null) star1Image.color = lockedColor;
        }

        // --- KIỂM TRA MỐC 2 SAO ---
        if (currentScore >= scoreFor2Stars)
        {
            if (star2Image != null) star2Image.color = unlockedColor;
            if (currentStarsEarned < 2) currentStarsEarned = 2;
        }
        else
        {
            if (star2Image != null) star2Image.color = lockedColor;
        }

        // --- KIỂM TRA MỐC 3 SAO ---
        if (currentScore >= scoreFor3Stars)
        {
            if (star3Image != null) star3Image.color = unlockedColor;
            if (currentStarsEarned < 3) currentStarsEarned = 3;
        }
        else
        {
            if (star3Image != null) star3Image.color = lockedColor;
        }
    }

    public void ResetStarsStatus()
    {
        if (star1Image != null) star1Image.color = lockedColor;
        if (star2Image != null) star2Image.color = lockedColor;
        if (star3Image != null) star3Image.color = lockedColor;
        currentStarsEarned = 0;
    }

    public int GetStarsEarned()
    {
        return currentStarsEarned;
    }
}