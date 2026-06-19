using UnityEngine;

public static class LevelManager
{
    private const string REACHED_KEY = "HighestLevelReached";
    public static int CurrentSelectedLevel = 1; // Lưu màn đang chọn để chơi

    // Lấy màn cao nhất đang mở (Mặc định mới chơi là màn 1)
    public static int GetLevelReached()
    {
        return PlayerPrefs.GetInt(REACHED_KEY, 1);
    }

    // Hàm gọi khi thắng ở Scene Game để tự động tăng tiến trình lên
    public static void UnlockNextLevel(int currentLevel)
    {
        int reached = GetLevelReached();
        if (currentLevel == reached)
        {
            PlayerPrefs.SetInt(REACHED_KEY, reached + 1);
            PlayerPrefs.Save();
        }
    }
}