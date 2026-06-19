using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class LevelSelectorUI : MonoBehaviour
{
    [Header("--- Map Container Setup ---")]
    public Transform levelContainer;        // Kéo Object LevelContainer (chứa 10 nút xếp tay) vào đây

    [Header("--- Popup UI Setup ---")]
    public GameObject infoPopupPanel;       // Kéo Panel Bảng Gợi Ý (Popup) vào đây
    public TextMeshProUGUI popupTitleText;  // Kéo Text hiển thị tên Màn chơi trên popup
    public Button playButton;               // Kéo nút Play trên popup vào đây
    public Button closeButton;              // Kéo nút X đóng popup vào đây

    private void Start()
    {
        // Ban đầu ẩn popup đi
        if (infoPopupPanel != null) infoPopupPanel.SetActive(false);

        // Gán sự kiện cho các nút trên popup
        if (playButton != null) playButton.onClick.AddListener(StartGame);
        if (closeButton != null) closeButton.onClick.AddListener(ClosePopup);

        // Cập nhật trạng thái cho 10 nút ĐÃ CÓ SẴN trên bản đồ
        RefreshExistingLevels();
    }

    private void RefreshExistingLevels()
    {
        if (levelContainer == null) return;

        int highestUnlocked = LevelManager.GetLevelReached();

        // Đếm xem trong LevelContainer bạn đã xếp bao nhiêu nút tay thực tế
        int totalNodes = levelContainer.childCount;

        for (int i = 0; i < totalNodes; i++)
        {
            // Lấy ra từng nút theo thứ tự sắp xếp từ trên xuống dưới trong Hierarchy
            Transform child = levelContainer.GetChild(i);
            int levelNum = i + 1; // Nút 1 ứng với Level 1, nút 2 ứng với Level 2...

            Button btn = child.GetComponent<Button>();
            TextMeshProUGUI txt = child.GetComponentInChildren<TextMeshProUGUI>();

            // Tự động điền số chuẩn (1 đến 10...) lên mặt nút
            if (txt != null) txt.text = levelNum.ToString();

            // Xử lý logic Khóa / Mở khóa dựa theo dữ liệu đã lưu
            if (levelNum <= highestUnlocked)
            {
                btn.interactable = true;
                btn.image.color = Color.white; // Màu sáng bình thường

                // Reset sự kiện cũ tránh trùng lặp và gán lệnh mở popup
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => OpenPopup(levelNum));
            }
            else
            {
                btn.interactable = false;
                btn.image.color = new Color32(100, 100, 100, 255); // Làm tối màu biểu thị bị khóa
            }
        }
    }

    // Hàm mở bảng Popup thông tin
    public void OpenPopup(int levelId)
    {
        LevelManager.CurrentSelectedLevel = levelId; // Ghi nhận màn được chọn vào lõi

        if (infoPopupPanel != null)
        {
            infoPopupPanel.SetActive(true);
            if (popupTitleText != null) popupTitleText.text = "Level " + levelId;
        }
    }

    public void ClosePopup() => infoPopupPanel.SetActive(false);

    // Chuyển sang Scene chơi game chính
    public void StartGame()
    {
        SceneManager.LoadScene("Game"); // Đảm bảo điền đúng tên Scene chơi game của bạn ở đây
    }
}