using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class SettingsMenu : MonoBehaviour
{
    public TMP_Dropdown resolutionDropdown;
    public Toggle fullscreenToggle;
    public Button applyButton;

    private Resolution[] resolutions;
    private HashSet<string> uniqueResolutions; // Used to filter out duplicate resolutions

    private void Awake()
    {
        //Cursor.visible = true;
        //Cursor.lockState = CursorLockMode.None;
    }
    void Start()
    {
        resolutions = Screen.resolutions;
        resolutionDropdown.ClearOptions();

        uniqueResolutions = new HashSet<string>();
        List<string> options = new List<string>();
        int currentResolutionIndex = 0;

        // Calculate the aspect ratio of the current screen
        float screenAspectRatio = (float)Screen.currentResolution.width / Screen.currentResolution.height;

        for (int i = 0; i < resolutions.Length; i++)
        {
            // Calculate the aspect ratio for each resolution
            float resolutionAspectRatio = (float)resolutions[i].width / resolutions[i].height;

            // Compare the aspect ratio of the current resolution to the screen's aspect ratio
            // If they are equal (within a small tolerance), add the resolution to the options
            if (Mathf.Approximately(resolutionAspectRatio, screenAspectRatio))
            {
                string option = resolutions[i].width + " x " + resolutions[i].height;
                // Only add the resolution if it's not already in the HashSet
                if (uniqueResolutions.Add(option))
                {
                    options.Add(option);

                    if (resolutions[i].width == Screen.currentResolution.width &&
                        resolutions[i].height == Screen.currentResolution.height)
                    {
                        currentResolutionIndex = options.Count - 1;
                    }
                }
            }
        }

        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = currentResolutionIndex;
        resolutionDropdown.RefreshShownValue();

        fullscreenToggle.isOn = Screen.fullScreen;

        applyButton.onClick.AddListener(ApplySettings);
    }

    public void ApplySettings()
    {
        // Find the resolution in the original array (which includes refresh rates)
        string selectedOption = resolutionDropdown.options[resolutionDropdown.value].text;
        Resolution selectedResolution = new Resolution();
        foreach (var res in resolutions)
        {
            if (selectedOption == res.width + " x " + res.height)
            {
                selectedResolution = res;
                break;
            }
        }

        Screen.SetResolution(selectedResolution.width, selectedResolution.height, fullscreenToggle.isOn);
    }

    public void ResetSettings()
    {
        Resolution[] allResolutions = Screen.resolutions;
        Resolution highestResolution = allResolutions[0];

        foreach (Resolution res in allResolutions)
        {
            if (res.width * res.height > highestResolution.width * highestResolution.height)
            {
                highestResolution = res;
            }
        }

        // Set the game to use the highest resolution and windowed mode
        Screen.SetResolution(highestResolution.width, highestResolution.height, false);

        // Update the dropdown and toggle UI elements to reflect the highest resolution and windowed mode
        resolutionDropdown.value = resolutionDropdown.options.FindIndex(option =>
            option.text == $"{highestResolution.width} x {highestResolution.height}");
        fullscreenToggle.isOn = true;

        // Apply and save the new settings
        ApplySettings();
    }


}
