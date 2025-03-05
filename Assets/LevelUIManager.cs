using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelUIManager : MonoBehaviour
{
    [SerializeField] private GameObject[] levelUIs; // Assign in the inspector
    [SerializeField] private Button[] playButtons; // Assign each level's play button in the inspector
    private int currentLevelIndex = 0; // Start from Level 1 (index 0)

    private void Start()
    {
        // Initialize UI states based on level unlock status
        UpdateLevelButtons();
        ShowLevelUI(currentLevelIndex); // Show the first level UI on start
    }

    // Update each level's play button active state based on unlock status
    private void UpdateLevelButtons()
    {
        for (int i = 0; i < levelUIs.Length; i++)
        {
            LevelData levelData = SaveLoadManager.Instance.GetCurrentLevelData(levelUIs[i].name);
            if (levelData != null)
            {
                playButtons[i].gameObject.SetActive(levelData.isUnlocked);
            }
            else
            {
                LevelData newLevel = new LevelData()
                {
                    levelName = levelUIs[i].name,
                    isUnlocked = false,
                    bestTime = float.MaxValue,
                    bestTier = "None",
                    achievements = new List<bool> { false, false, false },
                    finished = false
                };

                SaveLoadManager.Instance.currentSaveData.levelsData.Add(newLevel);

                playButtons[i].gameObject.SetActive(false); // Disable the button if no data is found
                //Debug.LogError("Level data not found for: " + levelUIs[i].name);
            }
        }
    }

    public void ShowLevelUI(int levelIndex)
    {
        // Hide all levels UIs first
        foreach (var levelUI in levelUIs)
        {
            levelUI.SetActive(false);
        }

        // Then show the requested level UI and ensure its play button status is correct
        levelUIs[levelIndex].SetActive(true);
        currentLevelIndex = levelIndex; // Update the current level index
    }

    public void OnNextPressed()
    {
        if (currentLevelIndex + 1 < levelUIs.Length)
        {
            currentLevelIndex++;
            ShowLevelUI(currentLevelIndex);
        }
    }

    public void OnPreviousPressed()
    {
        if (currentLevelIndex - 1 >= 0)
        {
            currentLevelIndex--;
            ShowLevelUI(currentLevelIndex);
        }
    }
}
