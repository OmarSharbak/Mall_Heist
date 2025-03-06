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

    [SerializeField] private GameObject fKeyboardUI; // Assign in inspector
    [SerializeField] private GameObject westPadUI;  // Assign in inspector

    [SerializeField] private GameObject _zKeyboardUI;
    [SerializeField] private GameObject _xKeyboardUI;
    [SerializeField] private GameObject _r1PadUI;
    [SerializeField] private GameObject _l1PadUI;

    [SerializeField] private UnityEngine.UI.Image _atmInteractionImage;

    InputSchemeChecker schemeChecker;


    private void HandleLocalPlayerStarted(ThirdPersonController localPlayer)
    {
        schemeChecker = localPlayer.transform.GetComponent<InputSchemeChecker>();

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
        HideWestButtonUI();

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

    public void ShowWestButtonUI()
    {
        HideSouthButtonEscalatorUI();
        HideSouthButtonObjectsUI();
        HideSouthButtonUI();
        HideNorthButtonUI();

        string currentScheme = schemeChecker.currentScheme;

        if (fKeyboardUI != null && currentScheme == "KeyboardMouse")
        {
            fKeyboardUI.SetActive(true);
        }
        if (westPadUI != null && currentScheme == "Gamepad")
        {
            westPadUI.SetActive(true);
        }
    }

    public void HideNorthButtonUI()
    {
        if (xKeyboardUI != null) xKeyboardUI.SetActive(false);
        if (northPadUI != null) northPadUI.SetActive(false);
    }

    public void HideWestButtonUI()
    {
        if (fKeyboardUI != null) fKeyboardUI.SetActive(false);
        if (westPadUI != null) westPadUI.SetActive(false);
    }


    public void HideSouthButtonUI()
    {
        if (eKeyboardUI != null) eKeyboardUI.SetActive(false);
        if (southPadUI != null) southPadUI.SetActive(false);
    }

    public void SetInteractionImage(Sprite sprite)
    {
        _atmInteractionImage.sprite = sprite;
    }

    public void ShowInteractionImage()
    {
        _atmInteractionImage.gameObject.SetActive(true);
    }

    public void HideInteractionImage()
    {
        _atmInteractionImage.gameObject.SetActive(false);
    }

    public void ShowSafeDialInteraction()
    {
        string currentScheme = schemeChecker.currentScheme;

        if (currentScheme == "KeyboardMouse")
        {
            _zKeyboardUI.SetActive(true);
            _xKeyboardUI.SetActive(true);
        }
        else if (currentScheme == "Gamepad")
        {
            _r1PadUI.SetActive(true);
            _l1PadUI.SetActive(true);
        }
    }

    public void HideSafeDialInteraction()
    {
        _zKeyboardUI.SetActive(false);
        _xKeyboardUI.SetActive(false);
        _r1PadUI.SetActive(false);
        _l1PadUI.SetActive(false);
    }

    private void OnEnable()
    {
        // Subscribe to the event
        ThirdPersonController.OnLocalPlayerStarted += HandleLocalPlayerStarted;
    }

    private void OnDisable()
    {
        // Unsubscribe from the event to avoid memory leaks
        ThirdPersonController.OnLocalPlayerStarted -= HandleLocalPlayerStarted;
    }
}
