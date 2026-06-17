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

        // 1. Kiểm tra xem nước đi này có giúp hoàn thành nhiệm vụ ĐÚNG LÚC hay không (Thắng giữa trận)
        if (QuestManager.Instance != null && QuestManager.Instance.IsQuestCompleted())
        {
            TriggerWin();
            return;
        }

        // 2. Nếu lượt đi chạm mốc số 0 mà chưa thắng, tiến hành kiểm tra kết quả trận đấu
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
            TriggerWin();
            return;
        }

        // --- CẬP NHẬT: Nếu lượt đi đã hết mà vẫn chưa hoàn thành mục tiêu ăn kẹo -> Thua cuộc ---
        isGameOver = true;
        Debug.Log("💥 GAME OVER! Bạn đã hết lượt di chuyển nhưng chưa đạt mục tiêu!");

        // Kích hoạt hiển thị màn hình thua cuộc từ Script LoseGame vừa viết
        if (LoseGame.Instance != null)
        {
            LoseGame.Instance.ShowLoseWindow();
        }
        else
        {
            Debug.LogWarning("Không tìm thấy Instance của script LoseGame trong Scene hiện tại!");
        }
        // ----------------------------------------------------------------------------------------
    }

    /// <summary>
    /// Hàm phụ trách kích hoạt màn hình WinGame và truyền trạng thái thắng cuộc
    /// </summary>
    private void TriggerWin()
    {
        if (isGameOver) return; // Tránh gọi trùng lặp nhiều lần
        isGameOver = true;

        int finalScore = 0;

        // Lấy số điểm hiện tại từ ScoreManager của bạn (nếu có)
        if (ScoreManager.Instance != null)
        {
            finalScore = ScoreManager.Instance.currentScore;
        }

        // CẬP NHẬT ĐỒNG BỘ: Gọi màn hình Win và thông báo thắng do hoàn thành Quest (isQuestWin = true)
        if (WinGame.Instance != null)
        {
            // Truyền vào true để WinGame biết đây là trận thắng, tự động kích hoạt thẳng 3 sao lấp lánh!
            WinGame.Instance.ShowWinWindow(finalScore, true);
        }
        else
        {
            Debug.LogWarning("Không tìm thấy Instance của WinGame trong Scene!");
        }
    }

    // Trả về trạng thái game hiện tại để các script khác không xử lý logic khi đã kết thúc
    public bool IsGameOver()
    {
        return isGameOver;
    }
}