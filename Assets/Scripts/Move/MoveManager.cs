using UnityEngine;
using TMPro;

public class MoveManager : MonoBehaviour
{
    public static MoveManager Instance { get; private set; }

    [Header("Move Settings")]
    public int maxMoves = 28;         // Số lượt di chuyển tối đa cho phép
    private int currentMoves;         // Số lượt còn lại thực tế trong trận

    [Header("UI Reference")]
    public TextMeshProUGUI moveText;  // Kéo thả Text hiển thị số lượt đi (VD: số 28) vào đây

    private bool isGameOver = false;

    private void Awake()
    {
        if (Instance == null) { Instance = this; }
        else { Destroy(gameObject); }
    }

    private void Start()
    {
        currentMoves = maxMoves;
        isGameOver = false;
        UpdateMoveUI();
    }

    /// <summary>
    /// Hàm trừ lượt di chuyển, được gọi từ BoardManager khi hoán đổi kẹo thành công
    /// </summary>
    public void UseAMove()
    {
        if (isGameOver) return;

        currentMoves--;
        if (currentMoves < 0) currentMoves = 0;

        UpdateMoveUI();

        // Nếu lượt đi chạm mốc số 0, tiến hành kiểm tra kết quả trận đấu
        if (currentMoves <= 0)
        {
            CheckGameOverCondition();
        }
    }

    private void UpdateMoveUI()
    {
        if (moveText != null)
        {
            moveText.text = currentMoves.ToString();
        }
    }

    private void CheckGameOverCondition()
    {
        // Hỏi QuestManager xem lúc này đã kịp hoàn thành nhiệm vụ chưa
        if (QuestManager.Instance != null && QuestManager.Instance.IsQuestCompleted())
        {
            // Nếu đã thu thập đủ kẹo, dù lượt đi về 0 vẫn tính là Thắng
            return;
        }

        // Nếu lượt đi đã hết mà chưa hoàn thành mục tiêu ăn kẹo -> Thua cuộc
        isGameOver = true;
        Debug.Log("💥 GAME OVER! Bạn đã hết lượt di chuyển nhưng chưa đạt mục tiêu!");
        // Bạn có thể kích hoạt Popup Thua Cuộc (Lose Panel) tại đây
    }

    // Trả về trạng thái game hiện tại để các script khác không xử lý logic khi đã thua
    public bool IsGameOver()
    {
        return isGameOver;
    }
}