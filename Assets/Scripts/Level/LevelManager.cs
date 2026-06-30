using UnityEngine;

public static class LevelManager
{
    private const string REACHED_KEY = "HighestLevelReached";
    private const string GOLD_SAVED_KEY = "UserGoldAmount";
    public static int CurrentSelectedLevel = 1;
    private static int totalNodesInMap = 10;

    public static int GetLevelReached()
    {
        return PlayerPrefs.GetInt(REACHED_KEY, 1);
    }

    public static void UnlockNextLevel(int currentLevel)
    {
        int reached = GetLevelReached();
        if (currentLevel == reached)
        {
            PlayerPrefs.SetInt(REACHED_KEY, reached + 1);
            PlayerPrefs.Save();
        }

        if (currentLevel == totalNodesInMap)
        {
            int isMapRewarded = PlayerPrefs.GetInt("MapRewarded_" + currentLevel, 0);
            if (isMapRewarded == 0)
            {
                if (GoldManager.Instance != null)
                {
                    GoldManager.Instance.AddGold(30);
                }
                else
                {
                    int currentGold = PlayerPrefs.GetInt(GOLD_SAVED_KEY, 0);
                    currentGold += 30;
                    PlayerPrefs.SetInt(GOLD_SAVED_KEY, currentGold);
                }
                PlayerPrefs.SetInt("MapRewarded_" + currentLevel, 1);
                PlayerPrefs.Save();
            }
        }
    }
}