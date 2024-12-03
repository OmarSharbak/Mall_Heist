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
using Unity.VisualScripting;
using UnityEditor;
using Mirror;
using UnityEngine.AI;

public class EscalatorManager : NetworkBehaviour
{
	public static EscalatorManager Instance { get; private set; }

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
	[SyncVar]
	public int moneyCollected = 0;

	// Player's inventory system
	private Inventory inventory;

	// List of all guard AI in the level
	public List<EmeraldAIEventsManager> allGuards;

	//[HideInInspector] 
	List<EmeraldAIEventsManager> chasingGuards = new List<EmeraldAIEventsManager>();

	public List<EmeraldAIEventsManager> guardsToRemove;

	//Music Fading
	public AudioSource audioSource1;
	public AudioSource audioSource2;
	private bool switchSource = false;
	private const float FadeDuration = 1.0f;
	private float originalVolume;

	public PlayerState player1 = null;
	public PlayerState player2 = null;

	void CalculateTotalMoney()
	{
		foreach (var register in registers)
		{
			totalMoney += register.moneyAmount;
		}

		moneyText.text = moneyCollected.ToString() + "/" + totalMoney.ToString();
		Debug.Log("Total money calculated");
	}


	public void CheckExposed(EmeraldAIEventsManager alertedGuard)
	{

		EmeraldAISystem emeraldAISystem = alertedGuard.transform.GetComponent<EmeraldAISystem>();

		Transform combatTarget = emeraldAISystem.PlayerDamageComponent.transform;

		if (combatTarget != null)
		{
			CheckExposed(combatTarget.GetComponent<ThirdPersonController>());
		}
		else
		{
			Debug.Log("Warn! check exposed target null");

		}
	}

	public void CheckExposed(ThirdPersonController thirdPersonController)
	{
		if (player1 != null && player1.thirdPersonController == thirdPersonController)
			CheckExposed(player1);

		if (player2 != null && player2.thirdPersonController == thirdPersonController)
			CheckExposed(player2);
	}
	// Function to mark a guard for removal
	public void CheckExposed(PlayerState player)
	{
		bool isExposed = false;

		foreach (EmeraldAIEventsManager guard in allGuards)
		{
			EmeraldAIDetection emeraldAIDetection = guard.GetComponent<EmeraldAIDetection>();

			Debug.Log(emeraldAIDetection.gameObject.name + emeraldAIDetection.EmeraldComponent.CurrentTarget);

			if (emeraldAIDetection.EmeraldComponent.CurrentTarget == player.playerTransform)
				isExposed = true;

		}

		if (isExposed == false)
		{
			SetExposed(player, false);
		}
	}

	public void CheckExposedOnDeath(PlayerState player, EmeraldAIEventsManager deadGuard)
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

			if (emeraldAIDetection.EmeraldComponent.CurrentTarget == player.playerTransform)
				isExposed = true;
		}

		if (isExposed == false)
		{
			SetExposed(player, false);
		}
	}

	public void AlertOtherGuards(EmeraldAIEventsManager alertedGuard)
	{
		EmeraldAISystem emeraldAISystem = alertedGuard.transform.GetComponent<EmeraldAISystem>();

		Transform combatTarget = emeraldAISystem.PlayerDamageComponent.transform;

		if (combatTarget != null)
		{
			AlertOtherGuards(combatTarget.GetComponent<ThirdPersonController>(), alertedGuard);
		}
		else
		{
			Debug.Log("Warn! alert other guards target null");
		}
	}

	// Called when a guard detects a player, this alerts other guards in the same room
	public void AlertOtherGuards(ThirdPersonController thirdPersonController, EmeraldAIEventsManager alertedGuard)
	{
		if (player1 != null && player1.thirdPersonController == thirdPersonController)
			AlertOtherGuards(player1, alertedGuard);

		if (player2 != null && player2.thirdPersonController == thirdPersonController)
			AlertOtherGuards(player2, alertedGuard);
	}
	// Called when a guard detects a player, this alerts other guards in the same room
	public void AlertOtherGuards(PlayerState player, EmeraldAIEventsManager alertedGuard)
	{
		// Get the room of the alerted guard
		string alertedRoom = alertedGuard.GetComponent<GuardRoom>().currentRoom;
		SetExposed(player, true);
		// Iterate through all guards to alert guards in the same room
		foreach (EmeraldAIEventsManager guard in allGuards)
		{
			if (guard.GetComponent<GuardRoom>().currentRoom == alertedRoom)
			{
				EmeraldAIDetection emeraldAIDetection = guard.GetComponent<EmeraldAIDetection>();

				// Set the player as the destination for the guard
				if (emeraldAIDetection.EmeraldComponent.CurrentTarget == null)
				{
					if (guard.gameObject.GetComponent<GuardOnCollision>().canDetect)
					{
						emeraldAIDetection.SetDetectedTarget(player.playerTransform);
						Debug.Log("Alerted a guard");
					}
				}
			}
		}
	}

	public void SetExposedFalse(EmeraldAIEventsManager alertedGuard)
	{
		EmeraldAISystem emeraldAISystem = alertedGuard.transform.GetComponent<EmeraldAISystem>();

		Transform combatTarget = emeraldAISystem.PlayerDamageComponent.transform;

		if (combatTarget != null)
		{
			SetExposed(combatTarget.GetComponent<ThirdPersonController>(), false);
		}
		else
		{
			Debug.Log("Warn! check exposed target null");

		}
	}
	public void SetExposed(ThirdPersonController thirdPersonController, bool input)
	{
		if (player1 != null && thirdPersonController == player1.thirdPersonController)
			SetExposed(player1, input);

		if (player2 != null && thirdPersonController == player2.thirdPersonController)
			SetExposed(player2, input);
	}

	[ClientCallback]
	// Setter function for the exposed state
	public void SetExposed(PlayerState player, bool input)
	{
		//player.exposed = input;
		//if (input)
		//{
		//	player.SetGameState(GameState.Chase);
		//	UpdateMusicState(player);
		//}
		//else //NO OTHER GUARDS CHASING
		//{
		//	Debug.Log("SET NOT EXPOSED");
		//	ClearTargetAll(player);
		//	UpdateEscalatorOutLine(player);

		//	Debug.Log(player.currentState);

		//	if (player.currentState != GameState.Defeat)
		//		player.SetGameState(GameState.Stealth);

		//	UpdateMusicState(player);
		//}
		CmdSetExposed(player.thirdPersonController.netId, input);

	}

	[Command]
	private void CmdSetExposed(uint netId, bool input)
	{
		Debug.Log(" SERVER set exposed called ");

		if (NetworkClient.spawned.TryGetValue(netId, out NetworkIdentity identity))
		{
			if (identity != null)
			{
				PlayerState player = identity.transform.GetComponent<PlayerState>();
				if (player != null)
				{

					player.exposed = input;
					if (input)
					{
						player.SetGameState(GameState.Chase);
						UpdateMusicState(player);
						Debug.Log("SET EXPOSED - SERVER");

					}
					else //NO OTHER GUARDS CHASING
					{
						Debug.Log("SET NOT EXPOSED - SERVER");
						ClearTargetAll(player);
						UpdateEscalatorOutLine(player);

						Debug.Log(player.currentState);

						if (player.currentState != GameState.Defeat)
							player.SetGameState(GameState.Stealth);

						UpdateMusicState(player);
					}

				}
			}
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

	}

	[SerializeField] TMP_Text countDownText;

	private void HandleLocalPlayerStarted(ThirdPersonController localPlayer)
	{

		player1 = localPlayer.GetComponent<PlayerState>();

		inventory = player1.playerTransform.GetComponent<Inventory>();


		Initialize(player1);

		Debug.Log("Player 1 State correct!");
	}

	private void HandlePlayerJoined(ThirdPersonController playerJoined)
	{
		if (player1 != playerJoined.transform.GetComponent<PlayerState>())
		{

			player2 = playerJoined.transform.GetComponent<PlayerState>();

			//inventory = player2.playerTransform.GetComponent<Inventory>();


			Initialize(player2);
			Debug.Log("Player 2 State correct!");


		}
		else
		{
			Debug.Log("On joined 2: Player 1 equals than player joined!");
		}


	}

	private void Initialize(PlayerState player)
	{
		if (player.thirdPersonController != null)
		{
			player.thirdPersonController.canMove = false;
		}
		if (player == player1)
		{
			CalculateTotalMoney();
			//Cursor.visible = false;
			//Cursor.lockState = CursorLockMode.Locked;

			originalVolume = audioSource1.volume;
			audioSource2.volume = 0;

			Time.timeScale = 1f;

			mmFeedbacksCompleted = GameObject.Find("MMFeedbacks(completed)").GetComponent<MMFeedbacks>();

		}
		//Start animating 3 2 1 then start the game with coroutine
		player.SetGameState(GameState.CountdownToStart);
		// Initialize game settings on start


	}

	public void InitiateGameStart()
	{
		Time.timeScale = 1f;
		countDownText.text = "GO!";
		StartCoroutine(ClearTextAfterDelay(1f));
		StartTimer();

		if (player1 != null && player1.thirdPersonController != null)
			player1.thirdPersonController.canMove = true;
		if (player2 != null && player2.thirdPersonController != null)
			player2.thirdPersonController.canMove = true;

		if (player1 != null)
			player1.SetGameState(GameState.Stealth);
		if (player2 != null)
			player2.SetGameState(GameState.Stealth);

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
		if (player1 != null)
			UpdateEscalatorOutLine(player1);
		if (player2 != null)
			UpdateEscalatorOutLine(player2);

		moneyText.text = moneyCollected.ToString() + "/" + totalMoney.ToString();

		//if (currentState == GameState.CountdownToStart)
		//{
		//countDownText.text = Mathf.Ceil(countdownToStartTimer).ToString();
		//countdownToStartTimer -= Time.deltaTime;
		//if (countdownToStartTimer < 0f)
		//{
		//    InitiateGameStart();
		//}
		//}

		// Update the stopwatch timer if timing is active
		if (isTiming)
		{
			elapsedTime += Time.deltaTime;
			UpdateStopwatchText();
		}

		// Update the systems override UI text
		int sealedCount = doorsSealedCount();
		systemsOverrideText.text = "Systems To Override: " + sealedCount + "/4";

		if (player1 != null)
		{
			UpdateMusicState(player1);

			// Check if there is no currently selected GameObject
			if (EventSystem.current.currentSelectedGameObject == null && player1.currentState == GameState.Victory)
			{
				// If nothing is selected, select the resume button GameObject
				EventSystem.current.SetSelectedGameObject(winNextLevelButtonGameObject);
			}

			if (EventSystem.current.currentSelectedGameObject == null && player1.currentState == GameState.Defeat)
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
				if (!player1.exposed)
					promptUIManager.ShowSouthButtonEscalatorUI();
			}
		}
	}

	public void UpdateCountdown(float countDown)
	{
		countDownText.text = Mathf.Ceil(countDown).ToString();

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


	public float countdownToStartTimer = 3;





	InputPromptUIManager promptUIManager;

	public string levelName;

	public static event Action OnLevelFinished;

	// Handles player interaction with the escalator

	public void InteractEscalator()
	{
		if (player1 != null)
		{
			if (playerNearEscalator && AreAllObjectivesComplete() && (totalMoney == moneyCollected))
			{
				if (!player1.exposed)
				{
					OnLevelFinished?.Invoke();
					LevelPerformanceManager.Instance.EvaluateLevelPerformance(levelName, elapsedTime);
					player1.SetGameState(GameState.Victory);
					UpdateMusicState(player1);
					StopTimer();
					ShowFloorCompletedUI();
				}

			}
			else if (playerNearEscalator)
			{
				Debug.Log("Not all objectives are complete or your exposed");
			}
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

	public void ClearTargetAll(ThirdPersonController thirdPersonController)
	{
		if (player1 != null && player1.thirdPersonController == thirdPersonController)
			ClearTargetAll(player1);

		if (player2 != null && player2.thirdPersonController == thirdPersonController)
			ClearTargetAll(player2);
	}
	// Clears the target for all guards, useful to reset guard behavior
	public void ClearTargetAll(PlayerState player)
	{
		foreach (var e in allGuards)
		{
			EmeraldAIDetection emeraldAIDetection = e.GetComponent<EmeraldAIDetection>();
			if (emeraldAIDetection.EmeraldComponent.CurrentTarget == player.playerTransform)
			{
				Debug.Log("clear all - guard clear" + player.transform.name);
				e.ClearTarget();
			}

		}

		if (player.currentState != GameState.Defeat)
		{
			player.SetGameState(GameState.Stealth);
		}
	}

	public void UpdateEscalatorOutLine(PlayerState player)
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

			if (!player.exposed)
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
	public void UpdateMusicState(PlayerState player)
	{
		if (player != player1) //just player1 can play music
			return;
		switch (player.currentState)
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
	private bool IsClipPlaying(AudioClip clip)
	{
		return (audioSource1.isPlaying && audioSource1.clip == clip) ||
			   (audioSource2.isPlaying && audioSource2.clip == clip);
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

	public GameState GetCurrentState(ThirdPersonController thirdPersonController)
	{
		if (player1 != null && thirdPersonController == player1.thirdPersonController)
			return player1.currentState;

		if (player2 != null && thirdPersonController == player2.thirdPersonController)
			return player2.currentState;
		return GameState.Stealth;
	}

	public void SetCurrentState(ThirdPersonController thirdPersonController, GameState gameState)
	{
		if (player1 != null && thirdPersonController == player1.thirdPersonController)
			player1.SetGameState(gameState);

		if (player2 != null && thirdPersonController == player2.thirdPersonController)
			player2.SetGameState(gameState);
	}

	[ClientCallback]
	public void StopGuard(NetworkGuard guard)
	{

		NavMeshAgent agent = guard.transform.GetComponent<NavMeshAgent>();

		if(agent != null)
		{
			agent.isStopped = true;
			Debug.Log(" CLIENT stopped guard");
			CmdStopGuard(guard.netId);

		}



	}


	[Command(requiresAuthority = false)]
	public void CmdStopGuard(uint _netId)
	{
		if (NetworkClient.spawned.TryGetValue(_netId, out NetworkIdentity identity))
		{
			if (identity != null)
			{
				NavMeshAgent guard = identity.transform.GetComponent<NavMeshAgent>();
				if (guard != null)
				{
					guard.isStopped = true;
					Debug.Log(" SERVER stopped guard");

				}
			}
		}
	}
	private void OnEnable()
	{
		// Subscribe to the event
		ThirdPersonController.OnLocalPlayerStarted += HandleLocalPlayerStarted;
		GameManager.OnPlayerJoined += HandlePlayerJoined;
		GameManager.OnPlayerExisting += HandlePlayerJoined;
	}

	private void OnDisable()
	{
		// Unsubscribe from the event to avoid memory leaks
		ThirdPersonController.OnLocalPlayerStarted -= HandleLocalPlayerStarted;
		GameManager.OnPlayerJoined -= HandlePlayerJoined;
		GameManager.OnPlayerExisting -= HandlePlayerJoined;

	}
}