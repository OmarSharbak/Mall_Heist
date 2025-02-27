using UnityEngine;
using UnityEngine.UI; // For Image component
using TMPro;
using I2.Loc;

public class UILevelDataDisplay : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text bestTimeText;
    [SerializeField] private TMP_Text currentTierText;
    [SerializeField] private Image[] achievementImages; // Use Image components for achievements
    [SerializeField] private Image clockImage; // Reference to the clock image component
    [SerializeField] private Image mapImage;
    [SerializeField] private GameObject achievementsOnGO;
    [SerializeField] private GameObject achievementsOffGO;
    [SerializeField] private GameObject levelLockedGO;
    [SerializeField] private GameObject goldClockGO;
    [SerializeField] private GameObject silverClockGO;
    [SerializeField] private GameObject bronzeClockGO;


    [Header("Sprites")]
    [SerializeField] private Sprite starFilled; // Assign in Inspector
    [SerializeField] private Sprite starEmpty; // Assign in Inspector
    [SerializeField] private Sprite goldClock; // Gold clock sprite
    [SerializeField] private Sprite silverClock; // Silver clock sprite
    [SerializeField] private Sprite bronzeClock; // Bronze clock sprite

    [Header("Level Info")]
    [SerializeField] private string levelName; // The name of the level this UI element represents

    private void Start()
    {
        UpdateLevelDataUI();
    }

    public void UpdateLevelDataUI()
    {
        // Retrieve the level data
        LevelData levelData = SaveLoadManager.Instance.GetCurrentLevelData(levelName);

        if (levelData == null)
        {
			Debug.LogError("Level data not found for: " + levelName);
            return;
        }

		// Update the best time display
		
        bestTimeText.text = FormatTime(levelData.bestTime);

        // Update the current tier display and clock image
        UpdateClockImage(levelData.bestTier); // Update the clock image based on the tier

        if(levelData.finished)
        {
            achievementsOffGO.SetActive(false);
            achievementsOnGO.SetActive(true);
            if(levelLockedGO != null)
                levelLockedGO.SetActive(false);
        }
        else if (levelData.isUnlocked && !levelData.finished)
        {
            achievementsOnGO.SetActive(false);
            achievementsOffGO.SetActive(true);
			if (levelLockedGO != null)
				levelLockedGO.SetActive(false);
        }
        else if(levelData.isUnlocked == false)
        {
            achievementsOnGO.SetActive(false);
            achievementsOffGO.SetActive(false);
			if (levelLockedGO != null)
				levelLockedGO.SetActive(true);
            
            goldClockGO.SetActive(false);
            silverClockGO.SetActive(false);
            bronzeClockGO.SetActive(false);
            mapImage.color = new Color(.12f, .12f, .12f, 1);
        }


        // Update achievements display using images
        for (int i = 0; i < achievementImages.Length; i++)
        {
            if (i < levelData.achievements.Count)
            {
                achievementImages[i].sprite = levelData.achievements[i] ? starFilled : starEmpty;
            }
            else
            {
                achievementImages[i].gameObject.SetActive(false);
            }
        }
    }

    private void UpdateClockImage(string tier)
    {
        switch (tier)
        {
            case "Gold":
                AdjustClockHeight(140);
                clockImage.sprite = goldClock;
                break;
            case "Silver":
                AdjustClockHeight(140);
                clockImage.sprite = silverClock;
                break;
            case "Bronze":
                AdjustClockHeight(140);
                clockImage.sprite = bronzeClock;
                break;
            default:
                break;
        }
    }

    private void AdjustClockHeight(float height)
    {
        RectTransform rectTransform = clockImage.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            Vector2 size = rectTransform.sizeDelta;
            size.y = height; // Set the height
            rectTransform.sizeDelta = size; // Apply the new size
        }
    }

    private string FormatTime(float timeInSeconds)
    {
        if (timeInSeconds > 3600 || timeInSeconds < 0) // 3600 seconds = 60 minutes
        {
            return "---";
        }
        else
        {
            int minutes = (int)timeInSeconds / 60;
            int seconds = (int)timeInSeconds % 60;
            return $"{minutes:00}:{seconds:00}";
        }
    }

}
