using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BoosterSelection : MonoBehaviour
{
    [Header("--- UI Elements (Kéo từ Hierarchy vào) ---")]
    [SerializeField] private Image bgImage;          // Kéo chính ô cha (BoosterItemChoco) vào
    [SerializeField] private GameObject tickImage;   // Kéo ô con (TickImage) vào
    [SerializeField] private TextMeshProUGUI quantityText; // Kéo ô chữ (QuantityText) vào

    [Header("--- Sprites Gốc (Kéo từ Project vào) ---")]
    [SerializeField] private Sprite blueNormalBg;    // Kéo file ảnh vòng tròn xanh dương (booster_0)
    [SerializeField] private Sprite greenSelectedBg;  // Kéo file ảnh vòng tròn xanh lá (booster2_0)

    [Header("--- Cấu Hình Booster ---")]
    [SerializeField] private string boosterKey = "Booster_Choco";
    [SerializeField] private int defaultQuantity = 3;

    private bool isSelected = false;
    private int currentQuantity;

    private void Awake()
    {
        // Tự động gán nếu quên kéo Image nền của cha
        if (bgImage == null) bgImage = GetComponent<Image>();
    }

    private void Start()
    {
        // Tải số lượng vật phẩm từ bộ nhớ
        currentQuantity = PlayerPrefs.GetInt(boosterKey + "_Qty", defaultQuantity);
        if (quantityText != null)
        {
            quantityText.text = currentQuantity.ToString();
        }

        // Đặt trạng thái ban đầu: Chưa chọn (Hiện nền xanh dương, ẩn dấu tích)
        isSelected = false;

        if (bgImage != null && blueNormalBg != null)
        {
            bgImage.sprite = blueNormalBg;
        }

        if (tickImage != null)
        {
            tickImage.SetActive(false);
        }

        // Reset dữ liệu lựa chọn trong trận (Chưa mang vào game)
        PlayerPrefs.SetInt(boosterKey + "_Selected", 0);
        PlayerPrefs.Save();
    }

    // Hàm gọi khi Click chuột vào ô Booster này
    public void ToggleSelection()
    {
        if (currentQuantity <= 0) return;

        isSelected = !isSelected;

        if (isSelected)
        {
            // BẬT CHỌN: Đổi nền cha sang xanh lá, hiện dấu tích con ở góc
            if (bgImage != null && greenSelectedBg != null) bgImage.sprite = greenSelectedBg;
            if (tickImage != null) tickImage.SetActive(true);

            PlayerPrefs.SetInt(boosterKey + "_Selected", 1);
        }
        else
        {
            // HỦY CHỌN: Trả nền cha về xanh dương, ẩn dấu tích con
            if (bgImage != null && blueNormalBg != null) bgImage.sprite = blueNormalBg;
            if (tickImage != null) tickImage.SetActive(false);

            PlayerPrefs.SetInt(boosterKey + "_Selected", 0);
        }

        PlayerPrefs.Save();
    }
}