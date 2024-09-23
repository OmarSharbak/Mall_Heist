using UnityEngine;
using TMPro;
using System.Collections;
using Febucci.UI;

public class PopupTextManager : MonoBehaviour
{
    public static PopupTextManager Instance; // Singleton instance
    public TMP_Text popupText; // Reference to the TMP Text component in your canvas
    private float hideDelay = 2.0f; // Duration to show the text
    private Vector3 originalScale; // To store the original scale of the text

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (popupText != null)
        {
            popupText.gameObject.SetActive(false); // Initially disable the text
            originalScale = popupText.transform.localScale; // Store the original scale
        }
    }

    public void ShowPopupText(string message)
    {
        if (popupText == null) return;

        popupText.GetComponent<TextAnimator_TMP>().ResetState();
        popupText.text = message;
        popupText.transform.localScale = originalScale; // Reset scale to original before showing
        popupText.gameObject.SetActive(true); // Show the text

        StopAllCoroutines(); // Stop any ongoing scale animations
        StartCoroutine(ScaleDownText()); // Start scaling down after showing
    }

    IEnumerator ScaleDownText()
    {
        float timer = 0;
        while (timer < hideDelay)
        {
            // Scale down over time until it reaches nearly zero
            float scale = Mathf.Lerp(originalScale.x, 0.7f, timer / hideDelay);
            popupText.transform.localScale = new Vector3(scale, scale, scale);

            timer += Time.deltaTime;
            yield return null;
        }

        HidePopupText(); // Hide text and stop scaling when done
    }

    void HidePopupText()
    {
        if (popupText != null)
        {
            popupText.text = "";
            popupText.gameObject.SetActive(false); // Hide the text
            popupText.transform.localScale = originalScale; // Reset scale to original after hiding
        }
    }
}
