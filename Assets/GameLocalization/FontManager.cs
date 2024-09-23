using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class FontManager : MonoBehaviour
{
    public static FontManager Instance;

    private List<TMP_Text> registeredTexts = new List<TMP_Text>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void RegisterText(TMP_Text textElement)
    {
        if (!registeredTexts.Contains(textElement))
        {
            registeredTexts.Add(textElement);
            // Optionally update the text element's font immediately
            UpdateFont(textElement);
        }
    }

    public void UnregisterText(TMP_Text textElement)
    {
        registeredTexts.Remove(textElement);
    }

    public void ChangeLanguage(string language)
    {
        //TMP_FontAsset selectedFont = // ... determine based on language
        foreach (var textElement in registeredTexts)
        {
            //textElement.font = selectedFont;
        }
    }

    private void UpdateFont(TMP_Text textElement)
    {
        // Update the font of the text element based on the current language
    }
}

