using UnityEngine;
using UnityEngine.EventSystems; // Bắt buộc phải có để sử dụng hệ thống Pointer của Unity 6

public class Candy : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [Header("Grid Position")]
    public int x; // Tọa độ cột
    public int y; // Tọa độ hàng
    public int candyType; // Loại kẹo (0, 1, 2...)

    private Vector2 firstTouchPosition;
    private Vector2 finalTouchPosition;
    private float swipeAngle = 0;
    private float swipeResist = 0.3f; // Giảm một chút xuống 0.3f để vuốt nhạy hơn

    private BoardManager board;

    void Start()
    {
        // Tự động tìm kiếm BoardManager nằm trong Scene để ra lệnh
        board = FindObjectOfType<BoardManager>();
    }

    // 1. CHUẨN UNITY 6: Kích hoạt khi người chơi nhấn chuột/chạm tay vào Collider viên kẹo
    public void OnPointerDown(PointerEventData eventData)
    {
        if (Camera.main != null)
        {
            // Lấy vị trí click chính xác trong không gian thế giới 2D
            firstTouchPosition = Camera.main.ScreenToWorldPoint(eventData.position);
        }
    }

    // 2. CHUẨN UNITY 6: Kích hoạt khi người chơi nhấc chuột/thả tay ra khỏi màn hình
    public void OnPointerUp(PointerEventData eventData)
    {
        if (Camera.main != null)
        {
            finalTouchPosition = Camera.main.ScreenToWorldPoint(eventData.position);
            CalculateAngle();
        }
    }

    // 3. Tính toán góc vuốt để xem người chơi muốn đổi chỗ sang hướng nào
    void CalculateAngle()
    {
        // Kiểm tra xem người chơi có thực sự vuốt không (khoảng cách vuốt phải lớn hơn swipeResist)
        if (Vector2.Distance(firstTouchPosition, finalTouchPosition) > swipeResist)
        {
            // Tính góc bằng hàm lượng giác Atan2
            swipeAngle = Mathf.Atan2(finalTouchPosition.y - firstTouchPosition.y, finalTouchPosition.x - firstTouchPosition.x) * Mathf.Rad2Deg;
            MovePieces();
        }
    }

    // 4. Ra lệnh cho BoardManager thực hiện đổi chỗ dựa theo hướng vuốt
    void MovePieces()
    {
        if (board == null) return;

        // Vuốt sang PHẢI (Góc từ -45 đến 45 độ)
        if (swipeAngle > -45 && swipeAngle <= 45 && x < board.width - 1)
        {
            StartCoroutine(board.SwapCandiesRoutine(x, y, x + 1, y));
        }
        // Vuốt lên TRÊN (Góc từ 45 đến 135 độ)
        else if (swipeAngle > 45 && swipeAngle <= 135 && y < board.height - 1)
        {
            StartCoroutine(board.SwapCandiesRoutine(x, y, x, y + 1));
        }
        // Vuốt sang TRÁI (Góc lớn hơn 135 hoặc nhỏ hơn -135 độ)
        else if ((swipeAngle > 135 || swipeAngle <= -135) && x > 0)
        {
            StartCoroutine(board.SwapCandiesRoutine(x, y, x - 1, y));
        }
        // Vuốt xuống DƯỚI (Góc từ -135 đến -45 độ)
        else if (swipeAngle > -135 && swipeAngle <= -45 && y > 0)
        {
            StartCoroutine(board.SwapCandiesRoutine(x, y, x, y - 1));
        }
    }
}