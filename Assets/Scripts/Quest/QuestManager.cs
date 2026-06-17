using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance { get; private set; }

    [Header("Quest Settings")]
    public int targetCandyType = 3;
    public int targetCount = 100;
    private int currentCount = 0;
    private bool isQuestDone = false; // Trạng thái đã hoàn thành nhiệm vụ hay chưa

    [Header("UI References (Hình ảnh & Con số)")]
    public Image questIconImage;
    public TextMeshProUGUI questCountText;

    [Header("Quest Icon Data")]
    public Sprite targetCandySprite;

    private void Awake()
    {
        if (Instance == null) { Instance = this; }
        else { Destroy(gameObject); }
    }

    private void Start()
    {
        currentCount = 0;
        isQuestDone = false; // Reset trạng thái khi bắt đầu game

        if (questIconImage != null && targetCandySprite != null)
        {
            questIconImage.sprite = targetCandySprite;
        }

        UpdateQuestUI();
    }

    public void CheckCandyDestroyed(int candyType)
    {
        // Nếu game đã kết thúc (thua do hết lượt hoặc đã thắng trước đó) thì ngừng đếm kẹo
        if (MoveManager.Instance != null && MoveManager.Instance.IsGameOver()) return;

        if (candyType == targetCandyType)
        {
            currentCount++;

            if (currentCount > targetCount)
                currentCount = targetCount;

            UpdateQuestUI();

            // Kiểm tra điều kiện thắng chuẩn xác hơn
            if (currentCount >= targetCount && !isQuestDone)
            {
                isQuestDone = true;
                OnQuestComplete();
            }
        }
    }

    private void UpdateQuestUI()
    {
        if (questCountText != null)
        {
            questCountText.text = $"{currentCount} / {targetCount}";
        }
    }

    private void OnQuestComplete()
    {
        Debug.Log("🎉 Hoàn thành nhiệm vụ thu thập kẹo! Kích hoạt hiệu ứng CHIẾN THẮNG!");

        // 1. Đồng bộ sang StarManager để xử lý đồng thời thanh Slider/Star ngoài giao diện chính
        if (StarManager.Instance != null)
        {
            StarManager.Instance.CompleteQuestAndEarnThirdStar();
        }

        // 2. Lấy số điểm hiện tại từ ScoreManager để hiển thị lên bảng Win
        int finalScore = 0;
        if (ScoreManager.Instance != null)
        {
            finalScore = ScoreManager.Instance.currentScore;
        }

        // 3. SỬ ĐỔI: Gọi trực tiếp màn hình WinGame và gửi tín hiệu ép hiển thị 3 sao
        if (WinGame.Instance != null)
        {
            // Tham số thứ hai truyền vào là true -> Chỉ định thắng nhờ Quest -> Tự động kích hoạt 3 sao lấp lánh!
            WinGame.Instance.ShowWinWindow(finalScore, true);
        }
        else
        {
            Debug.LogWarning("Không tìm thấy Instance của script WinGame trong Scene hiện tại!");
        }
    }

    // Để MoveManager gọi kiểm tra xem người chơi đã gom đủ kẹo chưa khi hết nước đi
    public bool IsQuestCompleted()
    {
        return isQuestDone;
    }
}