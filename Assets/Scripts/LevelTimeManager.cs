using UnityEngine;

public class LevelTimeManager : MonoBehaviour
{
    public static LevelTimeManager Instance { get; private set; }

    // Example tier times for levels (in seconds)
    public float goldTime = 60f;
    public float silverTime = 90f;
    public float bronzeTime = 120f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    public void SaveCompletionTime(string levelName, float completionTime)
    {
        // Generate keys for PlayerPrefs
        string timeKey = levelName + "_BestTime";
        string tierKey = levelName + "_BestTier";

        // Check if the completion time is better than the previously saved time, or if no time is saved yet
        float bestTime = PlayerPrefs.GetFloat(timeKey, float.MaxValue);
        if (completionTime < bestTime)
        {
            PlayerPrefs.SetFloat(timeKey, completionTime);

            // Determine the tier based on the completion time
            string tier = DetermineTier(completionTime);
            PlayerPrefs.SetString(tierKey, tier);

            // Unlock the next level if the achieved tier is at least Bronze
            if (tier != "No Tier")
            {
                UnlockNextLevel(levelName);
            }

            PlayerPrefs.Save();
        }
    }

    private string DetermineTier(float completionTime)
    {
        if (completionTime <= goldTime) return "Gold";
        else if (completionTime <= silverTime) return "Silver";
        else if (completionTime <= bronzeTime) return "Bronze";
        else return "No Tier";
    }

    private void UnlockNextLevel(string currentLevelName)
    {
        // Assuming level names are in the format "Level1", "Level2", etc.
        string prefix = "Level";
        int currentLevelNumber = int.Parse(currentLevelName.Replace(prefix, ""));
        int nextLevelNumber = currentLevelNumber + 1;
        string nextLevelName = prefix + nextLevelNumber;

        // Unlock the next level
        PlayerPrefs.SetInt(nextLevelName + "_Unlocked", 1);
        Debug.Log("Next level name is: " + nextLevelName);
    }

    public bool IsLevelUnlocked(string levelName)
    {
        return PlayerPrefs.GetInt(levelName + "_Unlocked", 0) == 1;
    }
}
