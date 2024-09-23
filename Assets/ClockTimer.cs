using UnityEngine;
using UnityEngine.UI;
using System.Collections; // Required for IEnumerator

public class ClockTimer : MonoBehaviour
{
    public Image fillImage; // Assign this in the inspector
    public Image clockImage;
    public Color goldColor, silverColor, bronzeColor; // Colors for each tier
    public float goldTime = 60f, silverTime = 120f, bronzeTime = 180f; // Duration for each tier

    private float currentTierTime;
    private float timeLeft;
    private enum Tier { Gold, Silver, Bronze, None }
    private Tier currentTier;

    bool start = false;

    void Start()
    {
        StartCoroutine(StartAfterDelay(3f)); // Start the timer after a 3-second delay
        
    }

    IEnumerator StartAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        start = true;
        SetTier(Tier.Gold); // Start with the gold tier after the delay
    }

    void Update()
    {
        if (start)
        {
            if (timeLeft > 0)
            {
                timeLeft -= Time.deltaTime;
                fillImage.fillAmount = timeLeft / currentTierTime;
            }
            else
            {
                AdvanceTier();
            }
        }  
    }

    void SetTier(Tier tier)
    {
        currentTier = tier;
        switch (tier)
        {
            case Tier.Gold:
                currentTierTime = goldTime;
                break;
            case Tier.Silver:
                currentTierTime = silverTime - goldTime;
                break;
            case Tier.Bronze:
                currentTierTime = bronzeTime - silverTime;
                break;
            default:
                fillImage.enabled = false;
                clockImage.enabled = false;
                return; // Exit if no more tiers are left
        }

        // Set color and reset time and fill amount
        fillImage.color = clockImage.color = GetColorForTier(tier);
        timeLeft = currentTierTime;
        fillImage.fillAmount = 1; // Reset to full circle
    }

    void AdvanceTier()
    {
        if (currentTier == Tier.Gold)
            SetTier(Tier.Silver);
        else if (currentTier == Tier.Silver)
            SetTier(Tier.Bronze);
        else
            SetTier(Tier.None); // Disable after bronze
    }

    Color GetColorForTier(Tier tier)
    {
        switch (tier)
        {
            case Tier.Gold: return goldColor;
            case Tier.Silver: return silverColor;
            case Tier.Bronze: return bronzeColor;
            default: return Color.clear;
        }
    }
}
