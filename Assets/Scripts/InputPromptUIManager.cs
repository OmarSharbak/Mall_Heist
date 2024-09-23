using UnityEngine;

public class InputPromptUIManager : MonoBehaviour
{
    [SerializeField] private GameObject eKeyboardUI; // Assign in inspector
    [SerializeField] private GameObject southPadUI;  // Assign in inspector

    [SerializeField] private GameObject eKeyboardObjectsUI; // Assign in inspector
    [SerializeField] private GameObject southPadObjectsUI;  // Assign in inspector

    [SerializeField] private GameObject eKeyboardEscalatorUI; // Assign in inspector
    [SerializeField] private GameObject southPadEscalatorUI;  // Assign in inspector

    [SerializeField] private GameObject xKeyboardUI; // Assign in inspector
    [SerializeField] private GameObject northPadUI;  // Assign in inspector

    InputSchemeChecker schemeChecker;

    private void Start()
    {
        schemeChecker = GetComponent<InputSchemeChecker>();
    }

    public void ShowSouthButtonUI()
    {
        string currentScheme = schemeChecker.currentScheme;

        if (eKeyboardUI != null && currentScheme == "KeyboardMouse")
        {
            eKeyboardUI.SetActive(true);
        }
        if (southPadUI != null && currentScheme == "Gamepad")
        {
            southPadUI.SetActive(true);
        }
    }

    public void ShowSouthButtonObjectsUI(bool show = true)
    {
        
        string currentScheme = schemeChecker.currentScheme;

        if (eKeyboardObjectsUI != null && currentScheme == "KeyboardMouse")
        {
            eKeyboardObjectsUI.SetActive(show);
        }
        if (southPadObjectsUI != null && currentScheme == "Gamepad")
        {
            southPadObjectsUI.SetActive(show);
        }
    }

    public void HideSouthButtonObjectsUI()
    {
        if (eKeyboardObjectsUI != null) eKeyboardObjectsUI.SetActive(false);
        if (southPadObjectsUI != null) southPadObjectsUI.SetActive(false);
    }

    public void ShowSouthButtonEscalatorUI(bool show = true)
    {
        string currentScheme = schemeChecker.currentScheme;

        if (eKeyboardEscalatorUI != null && currentScheme == "KeyboardMouse")
        {
            eKeyboardEscalatorUI.SetActive(show);
        }
        if (southPadEscalatorUI != null && currentScheme == "Gamepad")
        {
            southPadEscalatorUI.SetActive(show);
        }
    }

    public void HideSouthButtonEscalatorUI()
    {
        if (eKeyboardEscalatorUI != null) eKeyboardEscalatorUI.SetActive(false);
        if (southPadEscalatorUI != null) southPadEscalatorUI.SetActive(false);
    }

    public void ShowNorthButtonUI()
    {
        HideSouthButtonEscalatorUI();
        HideSouthButtonObjectsUI();
        HideSouthButtonUI();

        string currentScheme = schemeChecker.currentScheme;

        if (xKeyboardUI != null && currentScheme == "KeyboardMouse")
        {
            xKeyboardUI.SetActive(true);
        }
        if (northPadUI != null && currentScheme == "Gamepad")
        {
            northPadUI.SetActive(true);
        }
    }

    public void HideNorthButtonUI()
    {
        if (xKeyboardUI != null) xKeyboardUI.SetActive(false);
        if (northPadUI != null) northPadUI.SetActive(false);
    }

    public void HideSouthButtonUI()
    {
        if (eKeyboardUI != null) eKeyboardUI.SetActive(false);
        if (southPadUI != null) southPadUI.SetActive(false);
    }
}
