using UnityEngine;
using UnityEngine.UI; // For UI components
using UnityEngine.SceneManagement; // For scene loading
using Unity.VisualScripting;
using UnityEngine.EventSystems;
using EPOOutline;
public class LevelTrigger : MonoBehaviour
{
    public GameObject levelInfoUI; // Assign your level info UI in the inspector
    public Button loadLevelButton; // Assign your button for loading the level
    public string levelName; // The name of the level to load
    InputPromptUIManager promptUI;

    bool inRange = false;

    GameObject playerGO;
    private void Awake()
    {
        if(IsLevelUnlocked(levelName) == false)
        {
            GetComponent<Outlinable>().enabled = false;
            Destroy(this);
        }
        // Initially disable the level info UI
        levelInfoUI.SetActive(false);

        // Subscribe to the button's onClick event
        loadLevelButton.onClick.AddListener(LoadLevel);
    }
    public void InteractBuilding()
    {
        Debug.Log("Interacted with Level");
        promptUI.HideSouthButtonObjectsUI();
        playerGO.GetComponent<SimpleCarController>().canMove = false;
        levelInfoUI.SetActive(true);
        EventSystem.current.SetSelectedGameObject(null); // Deselect current selection
        EventSystem.current.SetSelectedGameObject(loadLevelButton.gameObject); // Set new selection
    }
    private void OnTriggerEnter(Collider other)
    {
        // Check if the player has entered the trigger
        if (other.CompareTag("Player"))
        {
            inRange = true;
            Debug.Log("Inrange");
            promptUI = other.gameObject.GetComponent<InputPromptUIManager>();
            promptUI.ShowSouthButtonObjectsUI();
            playerGO = other.gameObject;
            other.GetComponent<SimpleCarController>().levelTrigger = this;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Check if the player has exited the trigger
        if (other.CompareTag("Player"))
        {
            inRange = false;
            Debug.Log("Out of range");
            promptUI.HideSouthButtonObjectsUI();
            promptUI = null;
            other.GetComponent<SimpleCarController>().levelTrigger = null;
            other.GetComponent<SimpleCarController>().canMove = true;
        }
    }

    private void LoadLevel()
    {
        // Load the level with the specified name
        SceneManager.LoadScene(levelName);
    }

    private void OnDestroy()
    {
        // Unsubscribe from the button's onClick event to avoid memory leaks
        loadLevelButton.onClick.RemoveListener(LoadLevel);
    }

    bool IsLevelUnlocked(string levelName)
    {
        if(levelName == "Level1")
            return true;
        else return PlayerPrefs.GetInt(levelName + "_Unlocked", 0) == 1;
    }
}
