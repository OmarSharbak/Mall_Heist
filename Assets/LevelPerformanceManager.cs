using System.Collections.Generic;
using UnityEngine;

public class LevelPerformanceManager : MonoBehaviour
{
    public static LevelPerformanceManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }


    [Header("Tier Time Limits")]
    [SerializeField] private float goldTierTimeLimit = 45f;
    [SerializeField] private float silverTierTimeLimit = 70f;
    [SerializeField] private float bronzeTierTimeLimit = 140f;

    // Initialize a list of 3 booleans to false
    private List<bool> achievementsUnlocked = new List<bool> { false, false, false };

    private List<bool> toBeSavedAchievements = new List<bool> { false, false, false };
    public void EvaluateLevelPerformance(string levelName, float completionTime)
    {
        LevelData levelData = SaveLoadManager.Instance.GetCurrentLevelData(levelName);

        if (levelData == null)
        {
            Debug.LogError($"Level data not found for: {levelName}");
            return;
        }

        // Merge achievements
        MergeAchievements(levelData);

        string newTier = CalculateTier(completionTime);
        string bestTier = CalculateBestTier(newTier, levelData.bestTier);

        // Update the bestTier if the new tier is better than the existing one
        levelData.bestTier = bestTier;

        // Mark the level as finished if it qualifies for at least a Bronze tier
        if (levelData.bestTier != "None")
        {
            levelData.finished = true;
            UnlockNextLevel(levelName);
        }

        // Check if the new time is a record
        if (completionTime < levelData.bestTime)
        {
            levelData.bestTime = completionTime;
        }

        Debug.Log("Level Saved Successfully! " + levelName);

        // Save the updated save data
        SaveLoadManager.Instance.SaveGame();
    }

    private void MergeAchievements(LevelData levelData)
    {
        // Only update achievements if the level was finished at least once before
        if (levelData.finished)
        {
            // Ensure levelData.achievements has enough capacity
            while (levelData.achievements.Count < achievementsUnlocked.Count)
            {
                levelData.achievements.Add(false);
            }

            for (int i = 0; i < achievementsUnlocked.Count; i++)
            {

                if (achievementsUnlocked[i])
                {
                    levelData.achievements[i] = true;
                }
                
            }
        }
    }


    private string CalculateTier(float completionTime)
    {
        if (completionTime <= goldTierTimeLimit) return "Gold";
        if (completionTime <= silverTierTimeLimit) return "Silver";
        if (completionTime <= bronzeTierTimeLimit) return "Bronze";
        return "None";
    }

    private string CalculateBestTier(string newTier, string currentTier)
    {
        var tiers = new List<string> { "None", "Bronze", "Silver", "Gold" };
        int newIndex = tiers.IndexOf(newTier);
        int currentIndex = tiers.IndexOf(currentTier);

        return newIndex > currentIndex ? newTier : currentTier;
    }

    public void UnlockAchievement1()
    {
        achievementsUnlocked[0] = true;
    }

    public void UnlockAchievement2()
    {
        achievementsUnlocked[1] = true;
    }

    public void UnlockAchievement3()
    {
        achievementsUnlocked[2] = true;
    }
    private void UnlockNextLevel(string currentLevelName)
    {
        var levels = SaveLoadManager.Instance.currentSaveData.levelsData;
        for (int i = 0; i < levels.Count; i++)
        {
            if (levels[i].levelName == currentLevelName && i + 1 < levels.Count)
            {
                levels[i + 1].isUnlocked = true;
                Debug.Log($"Unlocked {levels[i + 1].levelName}");
                SaveLoadManager.Instance.SaveGame();
                break;
            }
        }
    }
}
