using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using EmeraldAI;
using System.Collections;
using TMPro;
using Cinemachine;
using EPOOutline;
using MoreMountains.Feedbacks;
using System;
using MK.Toon;

public class PlayerDamageHandler : MonoBehaviour
{
	[SerializeField] private int Lives = 1; // Player starts with 1 life
	[SerializeField] int moneyCount = 0; // Money count
	private TMP_Text moneyText; // Text to display money

	// UI References
	private GameObject regularCanvas;
	private GameObject lostCanvas;
	private TMP_Text CapturedText;
	private TMP_Text LivesText;
	private TMP_Text TimerText;

	// Cameras
	private CinemachineVirtualCamera PlayerCinemachineCamera;

	// Manager references
	private EscalatorManager escalatorManager;

	// Sounds
	[SerializeField] private AudioClip takeDownClip;
	[SerializeField] private AudioClip defeatClip;
	private AudioSource myAudioSource;

	// Gameplay states and values
	private const float DEFAULT_TIME_SCALE = 0.0f;
	public bool isInvincible = false;
	[SerializeField] private float InvincibilityTime = 3.0f;
	private const float WaitToPressXTime = 10.0f;
	private float currentWaitToPressXTime;
	private bool isWaitingForX = false;

	private Animator animator;
	private PlayerFlash playerFlash;

	private MMFeedbacks mmFeedbacksCaptured;
	private MMFeedbacks mmFeedbacksBribe;

	// References
	private ThirdPersonController thirdPersonController;
	private Coroutine waitForXPressCoroutine; // Reference to coroutine

	InputPromptUIManager promptUIManager;

	EmeraldAIEventsManager emeraldAIEventsManager;
	private void Start()
	{
		// Dynamically find the UI elements (e.g., by name or tag)
		regularCanvas = GameObject.Find("InGameUI");
		lostCanvas = GameObject.Find("LostUI");
		CapturedText = GameObject.Find("CapturedText")?.GetComponent<TMP_Text>();
		LivesText = GameObject.Find("LivesText")?.GetComponent<TMP_Text>();
		TimerText = GameObject.Find("WaitForXTimerText")?.GetComponent<TMP_Text>();
		PlayerCinemachineCamera = GameObject.Find("PlayerFollowCamera(Regular)")?.GetComponent<CinemachineVirtualCamera>();
		escalatorManager = GameObject.Find("Escalator")?.GetComponent<EscalatorManager>();
		moneyText = GameObject.Find("MoneyText")?.GetComponent<TMP_Text>();
		mmFeedbacksCaptured = GameObject.Find("MMFeedbacks(captured)")?.GetComponent<MMFeedbacks>();
		mmFeedbacksBribe = GameObject.Find("MMFeedbacks(bribe)")?.GetComponent<MMFeedbacks>();
		promptUIManager = GameObject.Find("InteractionPrompts").GetComponent<InputPromptUIManager>();


		loseRestartButtonGameObject = GameObject.Find("RestartButton")?.GetComponent<GameObject>();
		outliner = GameObject.Find("MainCamera")?.GetComponent<Outliner>();
		InitializeComponents();
	}

	private void InitializeComponents()
	{
		CapturedText.enabled = false;
		TimerText.enabled = false;
		//LivesText.text = Lives.ToString();
		//LivesText.gameObject.SetActive(false);
		moneyText.text = moneyCount.ToString(); // Initialize money text
		thirdPersonController = GetComponent<ThirdPersonController>();
		myAudioSource = GetComponent<AudioSource>();
		animator = GetComponent<Animator>();
		playerFlash = GetComponent<PlayerFlash>();

		if (myAudioSource == null)
		{
			Debug.LogWarning("No AudioSource component found on this GameObject.");
		}
	}

	private void Update()
	{
		if (isWaitingForX)
		{
			HandleTimer();
		}

		if (emeraldAIEventsManager != null && stopGuard == true)
		{
			emeraldAIEventsManager.StopMovement();
		}
	}

	void ResetAnimations()
	{
		thirdPersonController.ResetAttackAnimations();
	}
	public void OnDamagedByAI(EmeraldAIEventsManager EventsManager)
	{

		emeraldAIEventsManager = EventsManager;
		if (isInvincible) return;
		ResetAnimations();
		HandlePlayerDamage();
	}

	private void HandlePlayerDamage()
	{

		PlayTakeDownSound();

		this.gameObject.layer = LayerMask.NameToLayer("PlayerInvisible");


		if (mmFeedbacksCaptured != null)
			mmFeedbacksCaptured.PlayFeedbacks();

		if (moneyCount >= 10)
		{
			StartDamageSequence();
		}
		else
		{
			HandleGameOver();
		}
	}

	// Play the takedown sound.
	private void PlayTakeDownSound()
	{
		if (myAudioSource != null && takeDownClip != null)
		{
			myAudioSource.PlayOneShot(takeDownClip);
		}
		else
		{
			Debug.LogWarning("Missing AudioSource or TakeDown clip.");
		}
	}

	GameObject loseRestartButtonGameObject;
	Outliner outliner;
	[SerializeField] GameObject moneyGameObject;

	// Manage end of game state.
	private void HandleGameOver()
	{
		regularCanvas.SetActive(false);
		lostCanvas.SetActive(true);
		outliner.enabled = false;

		EscalatorManager.Instance.SetGameState(EscalatorManager.GameState.Defeat);
		Debug.Log("CURRENT STATE IS: " + EscalatorManager.Instance.currentState);
		// Access the EventSystem and set the selected GameObject
		EventSystem.current.SetSelectedGameObject(null); // Deselect current selection
		EventSystem.current.SetSelectedGameObject(loseRestartButtonGameObject); // Set new selection
																				//Cursor.visible = true;
																				//Cursor.lockState = CursorLockMode.None;

		//escalatorManager.UpdateMusicState();
		//Time.timeScale = DEFAULT_TIME_SCALE;
	}

	bool stopGuard = false;

	public static event Action OnPlayerCaught;
	// Sequence of events after player takes damage.
	private void StartDamageSequence()
	{
		OnPlayerCaught?.Invoke();
		escalatorManager.ClearTargetAll();
		stopGuard = true;
		emeraldAIEventsManager.SetIgnoredTarget(this.transform);
		thirdPersonController.SetCapturedState(true);
		PlayerCinemachineCamera.Priority = 9;
		Camera.main.cullingMask &= ~(1 << LayerMask.NameToLayer("Walls"));
		StartTimer();
		waitForXPressCoroutine = StartCoroutine(WaitForXPress());
	}

	// Start the countdown timer.
	private void StartTimer()
	{
		currentWaitToPressXTime = WaitToPressXTime;
		isWaitingForX = true;
		TimerText.enabled = true;
	}

	// Handle countdown for player's action.
	private void HandleTimer()
	{
		currentWaitToPressXTime -= Time.deltaTime;
		TimerText.text = Mathf.CeilToInt(currentWaitToPressXTime).ToString();

		if (currentWaitToPressXTime <= 0)
		{
			isWaitingForX = false;
			TimerText.enabled = false;
		}
	}

	// Wait for player's input while immobile.
	private IEnumerator WaitForXPress()
	{
		thirdPersonController.StopMovement();
		SetInvincibleState(true);
		CapturedText.enabled = true;
		gameObject.tag = "PlayerInvisible";
		promptUIManager.ShowNorthButtonUI();

		float elapsedTime = 0f;
		while (elapsedTime < WaitToPressXTime)
		{
			elapsedTime += Time.deltaTime;
			yield return null;
		}

		ResumePlay();
	}

	bool bribed = false;

	// Resume game after player's input.
	public void ResumePlay()
	{
		if (!isInvincible || emeraldAIEventsManager == null || bribed) return;

		bribed = true;
		// Rotate player to face the guard
		Vector3 directionToGuard = emeraldAIEventsManager.transform.position - transform.position;
		directionToGuard.y = 0; // Keep rotation in the horizontal plane
		Quaternion rotationToGuard = Quaternion.LookRotation(directionToGuard);
		transform.rotation = rotationToGuard;

		StopXPressCoroutine();

		moneyCount -= 10;
		PopupTextManager.Instance.ShowPopupText("-10");
		UpdateMoneyUI();
		moneyGameObject.SetActive(true);
		animator.SetTrigger("GiveMoney");

		promptUIManager.HideNorthButtonUI();

		StartCoroutine(ResumePlayAfterDelay());
	}

	public void PlayBribeFeedback()
	{
		if (mmFeedbacksBribe != null)
			mmFeedbacksBribe.PlayFeedbacks();
	}
	private IEnumerator ResumePlayAfterDelay()
	{
		yield return new WaitForSeconds(1f); // Wait for 1 second
		PopupTextManager.Instance.ShowPopupText("Bribed");
		bribed = false;
		PlayerCinemachineCamera.Priority = 11;
		Camera.main.cullingMask |= 1 << LayerMask.NameToLayer("Walls");
		thirdPersonController.SetCapturedState(false);
		thirdPersonController.EnableMovement();
		CapturedText.enabled = false;
		isWaitingForX = false;
		TimerText.enabled = false;
		StartCoroutine(HandleInvincibility());
	}

	public void DisableMoneyGameObject()
	{
		//Play sfx (moneywithdrawin)
		moneyGameObject.SetActive(false);
		thirdPersonController.EnableMovement();
		stopGuard = false;
		emeraldAIEventsManager.ResumeMovement();
		emeraldAIEventsManager.ClearIgnoredTarget(this.transform);
		emeraldAIEventsManager = null;
	}
	private void StopXPressCoroutine()
	{
		if (waitForXPressCoroutine != null)
		{
			StopCoroutine(waitForXPressCoroutine);
			waitForXPressCoroutine = null;
		}
	}

	// Set player to be invincible for a duration after damage.
	private IEnumerator HandleInvincibility()
	{
		//animator.SetTrigger("Damage");
		playerFlash.TriggerFlash();
		yield return new WaitForSeconds(InvincibilityTime);
		SetInvincibleState(false);
	}

	// Toggle player's invincible state.
	private void SetInvincibleState(bool state)
	{
		isInvincible = state;
		gameObject.tag = state ? "PlayerInvisible" : "Player";
		if (state == false)
			this.gameObject.layer = LayerMask.NameToLayer("PlayerVisible");
	}

	// Update the player's remaining lives on UI.
	private void UpdateLivesUI()
	{
		LivesText.text = Lives.ToString();
	}

	public void SetInvincible()
	{
		isInvincible = true;
		gameObject.tag = "PlayerInvisible";
		EscalatorManager.Instance.ClearTargetAll();
	}

	public void ResetInvincible()
	{
		isInvincible = false;
		gameObject.tag = "Player";
	}

	private void UpdateMoneyUI()
	{
		moneyText.text = moneyCount.ToString();
	}

	public void AddMoney(int amount)
	{
		moneyCount += amount;
		UpdateMoneyUI();
	}
}