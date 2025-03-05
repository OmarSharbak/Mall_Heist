using System.Collections.Generic;
using UnityEngine;

public class SaveLoadManager : MonoBehaviour
{

    public SaveData currentSaveData;

    private const string EncryptionKey = "OmarShEv3MaLLhEi$t!";
    private const string saveFileName = "gameSaveTest3.es3";

    public static SaveLoadManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Make sure it persists across scenes
        }
        else
        {
            Destroy(gameObject); // Destroy duplicate instances
        }
    }
    void Start()
    {
        LoadGameOrCreateNew();
    }

    public void LoadGameOrCreateNew()
    {
        var settings = new ES3Settings(ES3.EncryptionType.AES, EncryptionKey);

        if (ES3.FileExists(saveFileName, settings))
        {
            LoadGame();
        }
        else
        {
            InitializeNewSave();
        }
    }

    public void LoadGame()
    {
        var settings = new ES3Settings(ES3.EncryptionType.AES, EncryptionKey);
        // Adjust this to use the correct file name and settings for encryption
        currentSaveData = ES3.Load<SaveData>("currentSaveData", saveFileName, settings);
        Debug.Log("File Loaded");
    }

    public void SaveGame()
    {
        var settings = new ES3Settings(ES3.EncryptionType.AES, EncryptionKey);
        // Adjust this to save the entire SaveData object under a general key, with encryption
        ES3.Save("currentSaveData", currentSaveData, saveFileName, settings);
    }

    void InitializeNewSave()
    {
        // Initialize your SaveData with default values or specific starting values here
        currentSaveData = new SaveData();
        // For example, set up initial levels here
        InitializeLevels(); // Make sure this method sets up your initial levels correctly

        SaveGame(); // Save the initial state with encryption
    }

    private void InitializeLevels()
    {
        currentSaveData.levelsData = new List<LevelData>();

        string[] levelNames = new string[] { "Level1", "Level2", "Level3", "Level4", "Level5", "Level6" };

        for (int i = 0; i < levelNames.Length; i++)
        {
            if (i == 0 || i == 5)
            {
                LevelData newLevel = new LevelData()
                {
                    levelName = levelNames[i],
                    isUnlocked = true,
                    bestTime = float.MaxValue,
                    bestTier = "None",
                    achievements = new List<bool> { false, false, false },
                    finished = false
                };

                currentSaveData.levelsData.Add(newLevel);
            }
            else
            {
                LevelData newLevel = new LevelData()
                {
                    levelName = levelNames[i],
                    isUnlocked = false,
                    bestTime = float.MaxValue,
                    bestTier = "None",
                    achievements = new List<bool> { false, false, false },
                    finished = false
                };

                currentSaveData.levelsData.Add(newLevel);
            }



        }

        Debug.Log("Save File Created");
    }
    public void UpdateLevelData(string levelName, float completionTime, string achievedTier, List<bool> newAchievements)
    {
        // Find the level data for the specified level
        LevelData levelData = currentSaveData.levelsData.Find(level => level.levelName == levelName);

        if (levelData != null)
        {
            // Update the level as finished
            levelData.finished = true;

            // Check if the new time is better and update
            if (completionTime < levelData.bestTime)
            {
                levelData.bestTime = completionTime;
                levelData.bestTier = achievedTier; // Assuming you have some logic to determine the tier based on time
            }

            // Update achievements if necessary
            for (int i = 0; i < newAchievements.Count; i++)
            {
                if (newAchievements[i] && i < levelData.achievements.Count)
                {
                    levelData.achievements[i] = true;
                }
            }

            // Save the updated save data
            SaveGame();
        }
        else
        {
            Debug.LogError("Level data not found for: " + levelName);

            LevelData newLevel = new LevelData()
            {
                levelName = levelName,
                isUnlocked = false,
                bestTime = float.MaxValue,
                bestTier = "None",
                achievements = new List<bool> { false, false, false },
                finished = false
            };

            currentSaveData.levelsData.Add(newLevel);
        }
    }

    public LevelData GetCurrentLevelData(string levelName)
    {
        return currentSaveData.levelsData.Find(level => level.levelName == levelName);
    }


}
