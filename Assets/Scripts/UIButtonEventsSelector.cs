using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

public class UIButtonEventsSelector : MonoBehaviour
{
    [SerializeField] GameObject mainBar;
    [SerializeField] GameObject startButton;
    [SerializeField] GameObject levelsBar;
    [SerializeField] GameObject level1Button;
    [SerializeField] GameObject settingsMenu;
    [SerializeField] GameObject settingsBackButton;
    [SerializeField] TMP_Text toggleLabel;
    [SerializeField] TMP_Text musicLabel;
    [SerializeField] TMP_Text sfxLabel;
    [SerializeField] Image dropDownBG;

    [SerializeField] VirtualMouseInput virtualMouse;
    [SerializeField] Image cursorImage;

    [SerializeField] InputSchemeChecker inputSchemeChecker;
    private void Start()
    {
        EnableCursor();
    }

    public void DisableCursor()
    {
        //virtualMouse.enabled = false;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void EnableCursor()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        //virtualMouse.enabled = true;
    }
    private void Update()
    {
        if(mainBar != null && levelsBar != null) 
        {
            if (mainBar.active == true && EventSystem.current.currentSelectedGameObject == null)
            {
                EventSystem.current.SetSelectedGameObject(null); // Deselect current selection
                EventSystem.current.SetSelectedGameObject(startButton); // Set new selection
            }
            else if (levelsBar.active == true && EventSystem.current.currentSelectedGameObject == null)
            {
                EventSystem.current.SetSelectedGameObject(null); // Deselect current selection
                EventSystem.current.SetSelectedGameObject(level1Button); // Set new selection
            }else if(settingsMenu.active == true && EventSystem.current.currentSelectedGameObject == null)
            {
                EventSystem.current.SetSelectedGameObject(null); // Deselect current selection
                EventSystem.current.SetSelectedGameObject(settingsBackButton); // Set new selection
            }
        }

        AdjustSettingsUI();

        if(inputSchemeChecker != null)
        {
            Debug.Log(inputSchemeChecker.currentScheme);
            if (inputSchemeChecker.currentScheme == "Keyboard&Mouse")
            {
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;

            }
            else
            {
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
            }
        }
            
    }

    void AdjustSettingsUI()
    {
        if (EventSystem.current.currentSelectedGameObject != null)
        {
            if (EventSystem.current.currentSelectedGameObject.name == "Toggle")
            {
                toggleLabel.color = Color.red;
            }
            else
            {
                toggleLabel.color = Color.white;
            }
            if (EventSystem.current.currentSelectedGameObject.name == "SFXSlider")
            {
                sfxLabel.color = Color.red;
            }
            else
            {
                sfxLabel.color = Color.white;
            }
            if (EventSystem.current.currentSelectedGameObject.name == "MusicSlider")
            {
                musicLabel.color = Color.red;
            }
            else
            {
                musicLabel.color = Color.white;
            }
            if (EventSystem.current.currentSelectedGameObject.name == "Dropdown")
            {
                dropDownBG.color = new Color(240f / 255f, 40f / 255f, 40f / 255f);
            }
            else
            {
                dropDownBG.color = Color.white;
            }
        }
    }
    // Function to select a UI button
    public void SelectButton(GameObject button)
    {
        if (button != null)
        {
            EventSystem.current.SetSelectedGameObject(null); // Deselect current selection
            EventSystem.current.SetSelectedGameObject(button); // Set new selection
        }
    }
}
