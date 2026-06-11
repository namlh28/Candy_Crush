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
    private bool isQuestDone = false; // BỔ SUNG: Kiểm tra trạng thái đã hoàn thành chưa

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
        isQuestDone = false; // BỔ SUNG: Reset trạng thái khi bắt đầu game

        if (questIconImage != null && targetCandySprite != null)
        {
            questIconImage.sprite = targetCandySprite;
        }

        UpdateQuestUI();
    }

    public void CheckCandyDestroyed(int candyType)
    {
        // BỔ SUNG: Nếu game đã kết thúc (thua do hết lượt) thì ngừng đếm kẹo
        if (MoveManager.Instance != null && MoveManager.Instance.IsGameOver()) return;

        if (candyType == targetCandyType)
        {
            currentCount++;

            if (currentCount > targetCount)
                currentCount = targetCount;

            UpdateQuestUI();

            // SỬA ĐỔI: Kiểm tra điều kiện thắng chuẩn xác hơn
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
        Debug.Log("🎉 Hoàn thành nhiệm vụ thu thập kẹo! CHIẾN THẮNG!");
        // Bạn có thể kích hoạt Popup Thắng Cuộc (Win Panel) tại đây
    }

    // BỔ SUNG HÀM NÀY: Để MoveManager gọi kiểm tra xem người chơi đã gom đủ kẹo chưa
    public bool IsQuestCompleted()
    {
        return isQuestDone;
    }
}