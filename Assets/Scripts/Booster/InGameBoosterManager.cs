using UnityEngine;

public class InGameBoosterManager : MonoBehaviour
{
    private void Start()
    {
        CheckAndApplyBoosters();
    }

    private void CheckAndApplyBoosters()
    {
        // 1. Kiểm tra xem người chơi có chọn mang Booster Choco vào không
        if (PlayerPrefs.GetInt("Booster_Choco_Selected", 0) == 1)
        {
            Debug.Log("KÍCH HOẠT: Người chơi mang theo Choco Booster vào trận!");

            // Thực hiện trừ đi 1 quả trong kho vì đã sử dụng
            int qty = PlayerPrefs.GetInt("Booster_Choco_Qty", 3);
            PlayerPrefs.SetInt("Booster_Choco_Qty", qty - 1);
            PlayerPrefs.Save();

            // Gọi hàm tạo sẵn 1 quả cầu Choco ngẫu nhiên trên bảng kẹo của bạn ở đây
            // Ví dụ: CandyGrid.Instance.SpawnChocoOnStart();
        }

        // Tương tự cho các loại booster khác...
        // Reset lại trạng thái chọn để ván sau không bị tự động mang vào tiếp
        PlayerPrefs.SetInt("Booster_Choco_Selected", 0);
    }
}