using UnityEngine;
using UnityEngine.UI;

public class LevelMedalDisplay : MonoBehaviour
{
    public string levelName; // Name of the level this UI element represents
    public Image medalImage; // Reference to the UI Image component that will display the medal

    public Sprite goldMedalSprite; // Assign these in the inspector
    public Sprite silverMedalSprite;
    public Sprite bronzeMedalSprite;
    public Sprite noMedalSprite; // Optional: Display if no medal has been earned

    private void Start()
    {
        UpdateMedalDisplay();
    }

    public void UpdateMedalDisplay()
    {
        // Retrieve the best tier achieved for the level
        string bestTier = PlayerPrefs.GetString(levelName + "_BestTier", "No Tier");

        // Update the medalImage sprite based on the best tier
        switch (bestTier)
        {
            case "Gold":
                medalImage.sprite = goldMedalSprite;
                break;
            case "Silver":
                medalImage.sprite = silverMedalSprite;
                break;
            case "Bronze":
                medalImage.sprite = bronzeMedalSprite;
                break;
            default:
                medalImage.sprite = noMedalSprite; // Use this if no medal has been earned or if you have a default image
                break;
        }
    }

    // Optional: Call this method to refresh the UI when the world map is loaded or when returning to the world map
    public void RefreshUI()
    {
        UpdateMedalDisplay();
    }
}
