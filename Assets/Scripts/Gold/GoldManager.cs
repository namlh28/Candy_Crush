using UnityEngine;
using TMPro;

public class GoldManager : MonoBehaviour
{
    public static GoldManager Instance { get; private set; }

    [SerializeField] private TextMeshProUGUI goldText;

    private int currentGold = 0;
    private const string GOLD_SAVED_KEY = "UserGoldAmount";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        currentGold = PlayerPrefs.GetInt(GOLD_SAVED_KEY, 0);
        UpdateGoldUI();
    }

    public int GetCurrentGold()
    {
        return currentGold;
    }

    public void AddGold(int amount)
    {
        currentGold += amount;
        PlayerPrefs.SetInt(GOLD_SAVED_KEY, currentGold);
        PlayerPrefs.Save();
        UpdateGoldUI();
    }

    public bool TrySpendGold(int amount)
    {
        if (currentGold >= amount)
        {
            currentGold -= amount;
            PlayerPrefs.SetInt(GOLD_SAVED_KEY, currentGold);
            PlayerPrefs.Save();
            UpdateGoldUI();
            return true;
        }
        return false;
    }

    private void UpdateGoldUI()
    {
        if (goldText != null)
        {
            goldText.text = currentGold.ToString();
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            AddGold(50);
        }
    }
}