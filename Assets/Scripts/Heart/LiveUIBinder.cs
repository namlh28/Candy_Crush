using UnityEngine;
using TMPro;

public class LiveUIBinder : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI myCountText;
    [SerializeField] private TextMeshProUGUI myStatusText;

    private void Awake()
    {
        // Nếu lỡ như bấm Play thẳng từ cảnh Game mà chưa có LivesManager
        if (LivesManager.Instance == null)
        {
            // Tự động tạo ra một Object mới và dán LivesManager vào để cứu cánh
            GameObject go = new GameObject("LivesManager_AutoCreated");
            go.AddComponent<LivesManager>();
        }

        // Bây giờ chắc chắn LivesManager đã tồn tại, tiến hành đăng ký UI
        if (LivesManager.Instance != null)
        {
            LivesManager.Instance.RegisterUI(myCountText, myStatusText);
        }
    }
}