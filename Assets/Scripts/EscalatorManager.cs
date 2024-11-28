using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using EPOOutline;
using TMPro;
using UnityEngine.SceneManagement;
using EmeraldAI;
using EmeraldAI.Utility;
using MoreMountains.Feedbacks;
using System;

public class EscalatorManager : MonoBehaviour
{
    public static EscalatorManager Instance {  get; private set; }

    // List of doors that can be sealed by the player
    public List<SealableDoor> sealableDoors;

    // Escalator objects that will be used to update the outline
    public GameObject escalatorObject1;
    public GameObject escalatorObject2;

    // UI for completion and the regular game HUD
    public GameObject floorCompletedCanvas;
    public GameObject regularCanvas;

    // Key for player interaction
    public KeyCode interactionKey = KeyCode.E;

    // Determines if player is near the escalator for interaction
    private bool playerNearEscalator = false;

    public List<Register> registers;

    // UI texts for gameplay status
    public TMP_Text moneyText;
    public TMP_Text systemsOverrideText;
    public List<TMP_Text> countObjectives;

    int totalMoney;
    public int moneyCollected = 0;

    // Player's inventory system
    private Inventory inventory;

    // List of all guard AI in the level
    public List<EmeraldAIEventsManager> allGuards;

    //[HideInInspector] 
    List<EmeraldAIEventsManager> chasingGuards = new List<EmeraldAIEventsManager>();

    public List<EmeraldAIEventsManager> guardsToRemove;

    // Flag to determine if the player has been detected or not
    public bool exposed = false;

    // Reference to the player's transform for AI interactions
    private Transform playerTransform;

    //Music Fading
    public AudioSource audioSource1;
    public AudioSource audioSource2;
    private bool switchSource = false;
    private const float FadeDuration = 1.0f;
    private float originalVolume;


    void CalculateTotalMoney()
    {
        foreach(var register in registers)
        {
            totalMoney += register.moneyAmount;
        }

        moneyText.text = moneyCollected.ToString() + "/" + totalMoney.ToString();
        Debug.Log("Total money calculated");
    }

    // Function to mark a guard for removal
    public void CheckExposed()
    {
        bool isExposed = false;

        foreach (EmeraldAIEventsManager guard in allGuards)
        {
            EmeraldAIDetection emeraldAIDetection = guard.GetComponent<EmeraldAIDetection>();

            Debug.Log(emeraldAIDetection.gameObject.name + emeraldAIDetection.EmeraldComponent.CurrentTarget);

            if (emeraldAIDetection.EmeraldComponent.CurrentTarget != null)
                isExposed = true;
        }

        if (isExposed == false) { 
            SetExposed(false);
        }
    }

    public void CheckExposedOnDeath(EmeraldAIEventsManager deadGuard)
    {
        bool isExposed = false;

        foreach (EmeraldAIEventsManager guard in allGuards)
        {
            if (guard == deadGuard)
            {
                continue;
            }
            EmeraldAIDetection emeraldAIDetection = guard.GetComponent<EmeraldAIDetection>();

            Debug.Log(emeraldAIDetection.gameObject.name + emeraldAIDetection.EmeraldComponent.CurrentTarget);

            if (emeraldAIDetection.EmeraldComponent.CurrentTarget != null)
                isExposed = true;
        }

        if (isExposed == false)
        {
            SetExposed(false);
        }
    }

    // Called when a guard detects a player, this alerts other guards in the same room
    public void AlertOtherGuards(EmeraldAIEventsManager alertedGuard)
    {
        // Get the room of the alerted guard
        string alertedRoom = alertedGuard.GetComponent<GuardRoom>().currentRoom;
        SetExposed(true);
        // Iterate through all guards to alert guards in the same room
        foreach (EmeraldAIEventsManager guard in allGuards)
        {
            if (guard.GetComponent<GuardRoom>().currentRoom == alertedRoom)
            {
                EmeraldAIDetection emeraldAIDetection = guard.GetComponent<EmeraldAIDetection>();

                // Set the player as the destination for the guard
                if (emeraldAIDetection.EmeraldComponent.CurrentTarget == null)
                {
                    if (guard.gameObject.GetComponent<GuardOnCollision>().canDetect) { 
                        emeraldAIDetection.SetDetectedTarget(playerTransform);
                        Debug.Log("Alerted a guard");
                    }
                }
            }
        }
    }

    // Setter function for the exposed state
    public void SetExposed(bool input)
    {
        exposed = input;
        if (input)
        {
            SetGameState(GameState.Chase);
            UpdateMusicState();
        }
        else //NO OTHER GUARDS CHASING
        {
            Debug.Log("SET NOT EXPOSED");
            ClearTargetAll();
            UpdateEscalatorOutLine();

            Debug.Log(currentState);

            if (currentState != GameState.Defeat)
                SetGameState(GameState.Stealth);

            UpdateMusicState();
        }
    }

    // Clears the target for all guards, useful to reset guard behavior
    public void ClearTargetAll()
    {
        foreach (var e in allGuards)
        {
            e.ClearTarget();

        }

        if(currentState != GameState.Defeat)
        {
            SetGameState(GameState.Stealth);
        }
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            //DontDestroyOnLoad(gameObject); // This ensures that the instance is not destroyed between scene loads.
        }
        else if (Instance != this)
        {
            Destroy(gameObject); // This ensures that there's only one instance of the EscalatorManager.
        }

        currentState = GameState.WaitingToStart;
    }

    ThirdPersonController thirdPersonController;
    [SerializeField] TMP_Text countDownText;

	private void HandleLocalPlayerStarted(ThirdPersonController localPlayer)
	{
		thirdPersonController = localPlayer;
		playerTransform=thirdPersonController.transform;
        inventory= playerTransform.GetComponent<Inventory>();


		Initialize();
	}


	private void Initialize()
    {
        thirdPersonController = playerTransform.GetComponent<ThirdPersonController>();
        if (thirdPersonController != null)
        {
            thirdPersonController.canMove = false;
        }
        CalculateTotalMoney();
        //Cursor.visible = false;
        //Cursor.lockState = CursorLockMode.Locked;

        originalVolume = audioSource1.volume;
        audioSource2.volume = 0;

        Time.timeScale = 1f;
        
        //Start animating 3 2 1 then start the game with coroutine
        SetGameState(GameState.CountdownToStart);
        // Initialize game settings on start

        mmFeedbacksCompleted = GameObject.Find("MMFeedbacks(completed)").GetComponent<MMFeedbacks>();

    }
        
    public void InitiateGameStart()
    {
        Time.timeScale = 1f;
        countDownText.text = "GO!";
        StartCoroutine(ClearTextAfterDelay(1f));
        StartTimer();

        if (thirdPersonController != null)
            thirdPersonController.canMove = true;

        SetGameState(GameState.Stealth);
    }

    private IEnumerator ClearTextAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay); // Wait for the specified delay
        countDownText.text = ""; // Clear the text
        countDownText.enabled = false; // Optionally disable the text component if not used further
    }

    private float checkExposedTimer = 0f;
    private const float checkExposedInterval = 5f;
    private void Update()
    {
        /*checkExposedTimer += Time.deltaTime;

        if (checkExposedTimer >= checkExposedInterval)
        {
            CheckExposed();
            checkExposedTimer = 0f; // Reset the timer
        }*/

        // Main game loop for the manager
        UpdateEscalatorOutLine();

        moneyText.text = moneyCollected.ToString() + "/" + totalMoney.ToString();

        if (currentState == GameState.CountdownToStart)
        {
            //countDownText.text = Mathf.Ceil(countdownToStartTimer).ToString();
            //countdownToStartTimer -= Time.deltaTime;
            //if (countdownToStartTimer < 0f)
            //{
            //    InitiateGameStart();
            //}
        }

        // Update the stopwatch timer if timing is active
        if (isTiming)
        {
            elapsedTime += Time.deltaTime;
            UpdateStopwatchText();
        }

        // Update the systems override UI text
        int sealedCount = doorsSealedCount();
        systemsOverrideText.text = "Systems To Override: " + sealedCount + "/4";

        UpdateMusicState();

        // Check if there is no currently selected GameObject
        if (EventSystem.current.currentSelectedGameObject == null && currentState == GameState.Victory)
        {
            // If nothing is selected, select the resume button GameObject
            EventSystem.current.SetSelectedGameObject(winNextLevelButtonGameObject);
        }

        if (EventSystem.current.currentSelectedGameObject == null && currentState == GameState.Defeat)
        {
            // If nothing is selected, select the resume button GameObject
            EventSystem.current.SetSelectedGameObject(loseRestartLevelButtonGameObject);
        }

        if (playerNearEscalator == false)
        {
            if (promptUIManager != null)
            {
                promptUIManager.HideSouthButtonEscalatorUI();
            }
        }
        else if (playerNearEscalator == true && promptUIManager != null && AreAllObjectivesComplete() && (totalMoney == moneyCollected))
        {
            if(!exposed)
                promptUIManager.ShowSouthButtonEscalatorUI();
        }

    }

    public void UpdateCountdown(float countDown)
    {
        countDownText.text = Mathf.Ceil(countDown).ToString();

    }

	private bool AreAllObjectivesComplete()
    {
        foreach (TMP_Text objectiveText in countObjectives)
        {
            if (objectiveText.text != "1/1")
            {
                return false;
            }
        }
        return true;
    }

    private AudioSource audioSource; // This will play the music

    public AudioClip chaseMusic;     // Chase music clip
    public AudioClip stealthMusic;   // Stealth music clip
    public AudioClip pauseMusic;   // Pause music clip
    public AudioClip victoryMusic;
    public AudioClip defeatMusic;


    public enum GameState
    {
        WaitingToStart,
        CountdownToStart,
        Stealth,
        Chase,
        Pause,
        Defeat,
        Victory
    }

    // Current game state
    public GameState currentState { get; set; }

    public float countdownToStartTimer = 3;

    // Set the current game state and handle related behavior
    public void SetGameState(GameState newState)
    {
        currentState = newState;

        switch (currentState)
        {
            case GameState.WaitingToStart:
                // Handle logic related to normal gameplay here, e.g., resume gameplay
                Time.timeScale = 1.0f;
                // ... any other logic
                break;

            case GameState.Pause:
                // Handle game pause logic here, e.g., stop gameplay
                Time.timeScale = 0.0f;
                // ... any other logic
                UpdateMusicState();
                break;

            case GameState.Defeat:
                // Handle logic related to player's defeat here
                // ... e.g., display defeat screen, play defeat music, etc.
                //Cursor.visible = true;
                //Cursor.lockState = CursorLockMode.None;
                thirdPersonController.canMove = false;
                thirdPersonController.StopMovement();
                thirdPersonController.ToggleVisibility();
                //Time.timeScale = 0.0f;
                break;

            case GameState.Victory:
                // Handle logic related to player's defeat here
                // ... e.g., display defeat screen, play defeat music, etc.
                //Cursor.visible = true;
                //Cursor.lockState = CursorLockMode.None;
                thirdPersonController.canMove = false;
                thirdPersonController.StopMovement();
                Time.timeScale = 0.0f;
                break;

            case GameState.CountdownToStart:
                // Handle logic related to player's victory here
                // ... e.g., display victory screen, play victory music, etc.
                Time.timeScale = 1.0f;
                break;

            case GameState.Stealth:
                // Handle logic related to player's victory here
                // ... e.g., display victory screen, play victory music, etc.
                Time.timeScale = 1.0f;
                UpdateMusicState();
                break;

            case GameState.Chase:
                // Handle logic related to player's victory here
                // ... e.g., display victory screen, play victory music, etc.
                Time.timeScale = 1.0f;
                UpdateMusicState();
                break;

            default:
                Debug.LogWarning("Unknown game state set: " + newState);
                break;
        }
    }

    IEnumerator CrossfadeMusic(AudioSource fromSource, AudioSource toSource, AudioClip toClip, float duration)
    {
        toSource.clip = toClip;
        toSource.volume = 0;  // Ensure the starting volume is 0
        toSource.Play();

        float elapsed = 0.0f;
        float fromStartVolume = fromSource.volume;
        float toFinalVolume = toSource == audioSource2 ? 0.3f : (toSource == audioSource1 ? 0.6f : fromStartVolume);

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / duration;

            fromSource.volume = Mathf.Lerp(fromStartVolume, 0, t);
            toSource.volume = Mathf.Lerp(0, toFinalVolume, t);

            yield return null;
        }

        fromSource.Stop();
        fromSource.volume = fromStartVolume;  // Reset the volume back to its original state for potential future plays.
    }


    private bool IsClipPlaying(AudioClip clip)
    {
        return (audioSource1.isPlaying && audioSource1.clip == clip) ||
               (audioSource2.isPlaying && audioSource2.clip == clip);
    }

    public void UpdateMusicState()
    {
        switch (currentState)
        {
            case GameState.Stealth:
                if (!IsClipPlaying(stealthMusic))
                {
                    if (audioSource1.isPlaying)  // We check if it's playing rather than the clip since the clip could be a completed victory or defeat clip.
                        StartCoroutine(CrossfadeMusic(audioSource1, audioSource2, stealthMusic, FadeDuration));
                    else
                        StartCoroutine(CrossfadeMusic(audioSource2, audioSource1, stealthMusic, FadeDuration));
                }
                break;

            case GameState.Chase:
                if (!IsClipPlaying(chaseMusic))
                {
                    if (audioSource1.isPlaying)
                        StartCoroutine(CrossfadeMusic(audioSource1, audioSource2, chaseMusic, FadeDuration));
                    else
                        StartCoroutine(CrossfadeMusic(audioSource2, audioSource1, chaseMusic, FadeDuration));
                }
                break;

            case GameState.Pause:
                if (!IsClipPlaying(pauseMusic))
                {
                    if (audioSource1.isPlaying)
                        StartCoroutine(CrossfadeMusic(audioSource1, audioSource2, pauseMusic, FadeDuration));
                    else
                        StartCoroutine(CrossfadeMusic(audioSource2, audioSource1, pauseMusic, FadeDuration));
                }
                break;

                case GameState.Victory:
                    if (!IsClipPlaying(victoryMusic))
                    {
                        if (audioSource1.isPlaying)
                            StartCoroutine(CrossfadeMusic(audioSource1, audioSource2, victoryMusic, FadeDuration));
                        else
                            StartCoroutine(CrossfadeMusic(audioSource2, audioSource1, victoryMusic, FadeDuration));
                    }
                    break;

                case GameState.Defeat:
                    if (!IsClipPlaying(defeatMusic))
                    {
                        if (audioSource1.isPlaying)
                            StartCoroutine(CrossfadeMusic(audioSource1, audioSource2, defeatMusic, FadeDuration));
                        else
                            StartCoroutine(CrossfadeMusic(audioSource2, audioSource1, defeatMusic, FadeDuration));
                    }
                    break;
        }
    }

    InputPromptUIManager promptUIManager;

    public string levelName;

    public static event Action OnLevelFinished;

    // Handles player interaction with the escalator

    public void InteractEscalator()
    {
        if (playerNearEscalator && AreAllObjectivesComplete() && (totalMoney == moneyCollected))
        {
            if (!exposed)
            {
                OnLevelFinished?.Invoke();
                LevelPerformanceManager.Instance.EvaluateLevelPerformance(levelName, elapsedTime);
                SetGameState(GameState.Victory);
                UpdateMusicState();
                StopTimer();
                ShowFloorCompletedUI();
            }
            
        }
        else if (playerNearEscalator)
        {
            Debug.Log("Not all objectives are complete or your exposed");
        }
    }

    // Reloads the current level
    public void RestartLevel()
    {
        int activeSceneIndex = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(activeSceneIndex);
    }

    public void NextLevel()
    {
        int activeSceneIndex = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(activeSceneIndex + 1);
    }

    public void LoadTitleScreen()
    {
        SceneManager.LoadScene(0);
    }

    bool completedOnce = false;

    public bool doorOpen = false;

    MMFeedbacks mmFeedbacksCompleted;
    // Update the outline of the escalator based on gameplay conditions
    public void UpdateEscalatorOutLine()
    {
        //bool allDoorsSealed = AreAllDoorsSealed();

        // Check conditions and update escalator outlines accordingly
        if (AreAllObjectivesComplete() && (totalMoney == moneyCollected)) // && allDoorsSealed
        {
            //Debug.Log("GREEN ESCALAOTR");
            if (completedOnce == false)
            {
                completedOnce = true;
                if (mmFeedbacksCompleted != null)
                    mmFeedbacksCompleted.PlayFeedbacks();
            }

            if (!exposed)
            {
                

                // Update to green if conditions met
                var outlinable1 = escalatorObject1.GetComponent<Outlinable>();
                //var outlinable2 = escalatorObject2.GetComponent<Outlinable>();
                outlinable1.FrontParameters.Color = Color.green;
                doorOpen = true;
                //outlinable2.FrontParameters.Color = Color.green;
            }
            else
            {
                // Update to red if conditions not met
                var outlinable1 = escalatorObject1.GetComponent<Outlinable>();
                //var outlinable2 = escalatorObject2.GetComponent<Outlinable>();
                outlinable1.FrontParameters.Color = Color.red;
                doorOpen = false;
                //outlinable2.FrontParameters.Color = Color.red;
            }

        }
        
    }

    // Check if all doors are sealed
    private bool AreAllDoorsSealed()
    {
        foreach (SealableDoor door in sealableDoors)
        {
            if (!door.isSealed)
            {
                return false;
            }
        }
        return true;
    }

    // Count the number of doors that are sealed
    private int doorsSealedCount()
    {
        int count = 0;
        foreach (SealableDoor door in sealableDoors)
        {
            if (door.isSealed)
            {
                count++;
            }
        }
        return count;
    }

    [SerializeField] GameObject winRestartButtonGameObject;
    [SerializeField] GameObject winNextLevelButtonGameObject;
    [SerializeField] GameObject loseRestartLevelButtonGameObject;
    [SerializeField] Outliner outliner;

    // Displays the UI for floor completion
    public void ShowFloorCompletedUI()
    {
        if (floorCompletedCanvas != null)
        {
            floorCompletedCanvas.SetActive(true);
            UpdateStopwatchFinishedText();
            regularCanvas.SetActive(false);
            //Cursor.visible = true;
            //Cursor.lockState = CursorLockMode.None;
            outliner.enabled = false;
            // Access the EventSystem and set the selected GameObject
            EventSystem.current.SetSelectedGameObject(null); // Deselect current selection
            EventSystem.current.SetSelectedGameObject(winRestartButtonGameObject); // Set new selection
            //Cursor.visible = true;
            //Cursor.lockState = CursorLockMode.None;

            Time.timeScale = 0f;
        }
    }

    // Update the state of player's proximity to the escalator for interactions
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") || other.CompareTag("PlayerInvisible"))
        {
            playerNearEscalator = true;

        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") || other.CompareTag("PlayerInvisible"))
        {
            playerNearEscalator = false;
			promptUIManager = GameObject.Find("InteractionPrompts").GetComponent<InputPromptUIManager>();
			promptUIManager.HideSouthButtonEscalatorUI();
            promptUIManager = null;
        }
    }

    // Not yet implemented methods related to escalator camera
    public GameObject escalatorCamera;

    public void EnableObjectForDuration(float duration)
    {
        // Placeholder for future implementation
    }

    // Stopwatch related variables and methods for gameplay timing
    private float elapsedTime = 0.0f;
    public TMP_Text stopwatchText;
    private bool isTiming = false;
    public TMP_Text stopwatchTextFinished;


    // Start the stopwatch
    public void StartTimer()
    {
        elapsedTime = 0.0f;
        isTiming = true;
    }

    // Stop the stopwatch
    public void StopTimer()
    {
        isTiming = false;
    }

    // Resume the stopwatch
    public void ResumeTimer()
    {
        isTiming = true;
    }

    // Update the displayed stopwatch time
    private void UpdateStopwatchText()
    {
        int minutes = Mathf.FloorToInt(elapsedTime / 60F);
        int seconds = Mathf.FloorToInt(elapsedTime - minutes * 60);
        string formattedTime = string.Format("{0:0}:{1:00}", minutes, seconds);
        stopwatchText.text = formattedTime;
        
    }

    // Update the displayed stopwatch time for the completion screen
    private void UpdateStopwatchFinishedText()
    {
        int minutes = Mathf.FloorToInt(elapsedTime / 60F);
        int seconds = Mathf.FloorToInt(elapsedTime - minutes * 60);
        string formattedTime = string.Format("{0:0}:{1:00}", minutes, seconds);
        stopwatchTextFinished.text = formattedTime;
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