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
using Mirror;
using UnityEngine.InputSystem.XR;
using System.Security.Principal;

public class PlayerDamageHandler : NetworkBehaviour
{
	[SerializeField] private int Lives = 1; // Player starts with 1 life


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

	EmeraldAIEventsManager emeraldAIEventsManager = null;


	GameObject loseRestartButtonGameObject;
	Outliner outliner;
	[SerializeField] GameObject moneyGameObject;


	bool stopGuard = false;

	public static event Action OnPlayerCaught;

	private void Initialize()
	{
		// Dynamically find the UI elements (e.g., by name or tag)
		regularCanvas = GameObject.Find("InGameUI");
		if (isLocalPlayer)
		{
			lostCanvas = GameObject.Find("LostUI");
			LivesText = GameObject.Find("LivesText")?.GetComponent<TMP_Text>();
		}
		CapturedText = GameObject.Find("CapturedText")?.GetComponent<TMP_Text>();
		TimerText = GameObject.Find("WaitForXTimerText")?.GetComponent<TMP_Text>();
		PlayerCinemachineCamera = GameObject.Find("PlayerFollowCamera(Regular)")?.GetComponent<CinemachineVirtualCamera>();
		escalatorManager = GameObject.Find("Escalator")?.GetComponent<EscalatorManager>();
		mmFeedbacksCaptured = GameObject.Find("MMFeedbacks(captured)")?.GetComponent<MMFeedbacks>();
		mmFeedbacksBribe = GameObject.Find("MMFeedbacks(bribe)")?.GetComponent<MMFeedbacks>();
		promptUIManager = GameObject.Find("InteractionPrompts").GetComponent<InputPromptUIManager>();


		loseRestartButtonGameObject = GameObject.Find("RestartButton")?.GetComponent<GameObject>();
		outliner = GameObject.Find("MainCamera")?.GetComponent<Outliner>();

		thirdPersonController = GetComponent<ThirdPersonController>();

		myAudioSource = GetComponent<AudioSource>();
		animator = GetComponent<Animator>();
		playerFlash = GetComponent<PlayerFlash>();

		InitializeComponents();

		Debug.Log("canvas initialized!" + regularCanvas.name);

	}

	private void Start()
	{

		if (!isLocalPlayer)
			return;
		Initialize();
	}

	private void InitializeComponents()
	{
		CapturedText.enabled = false;
		TimerText.enabled = false;
		if (isLocalPlayer)
		{
			lostCanvas.SetActive(false);
			LivesText.text = Lives.ToString();
			LivesText.gameObject.SetActive(false);
		}


		if (myAudioSource == null)
		{
			Debug.LogWarning("No AudioSource component found on this GameObject.");
		}
	}
	private void Update()
	{
		if (!isLocalPlayer)
			return;
		if (isWaitingForX)
		{
			HandleTimer();
		}

		if (isServer && emeraldAIEventsManager != null && stopGuard == true)
		{
			GuardStop();
		}
	}
	[ServerCallback]
	private void GuardStop()
	{
		emeraldAIEventsManager.StopMovement();
		Debug.Log("guard stop movement");

	}
	[ClientCallback]
	void ResetAnimations()
	{
		if (thirdPersonController != null)
			thirdPersonController.ResetAttackAnimations();
	}


	[ClientRpc]
	public void RpcSetNetworkGuard(NetworkIdentity networkIdentity)
	{
		var networkGuard = networkIdentity.GetComponent<NetworkGuard>();
		emeraldAIEventsManager = networkGuard.GetComponent<EmeraldAIEventsManager>();

		if (isInvincible || !isLocalPlayer) return;
		ResetAnimations();
		HandlePlayerDamage();
	}



	[ClientCallback]

	private void HandlePlayerDamage()
	{
		Debug.Log("PLayer Damaged");
		PlayTakeDownSound();

		gameObject.layer = LayerMask.NameToLayer("PlayerInvisible");
		CmdPlayerInvisible(netId);

		if (mmFeedbacksCaptured != null)
			mmFeedbacksCaptured.PlayFeedbacks();

		if (GameManager.Instance.GetCurrentGlobalMoney() >= 10)
		{
			StartDamageSequence();
		}
		else
		{
			HandleGameOver();
		}
	}

	[Command]
	private void CmdPlayerInvisible(uint _netId)
	{
		if (NetworkClient.spawned.TryGetValue(_netId, out NetworkIdentity identity))
		{
			if (identity != null)
			{
				identity.gameObject.layer = LayerMask.NameToLayer("PlayerInvisible");

			}
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

	// Manage end of game state.
	private void HandleGameOver()
	{
		if (!isLocalPlayer)
		{
			return;
		}
		regularCanvas.SetActive(false);
		lostCanvas.SetActive(true);
		outliner.enabled = false;

		EscalatorManager.Instance.SetCurrentState(thirdPersonController, EscalatorManager.GameState.Defeat);
		Debug.Log("CURRENT STATE IS: " + EscalatorManager.Instance.GetCurrentState(thirdPersonController));
		// Access the EventSystem and set the selected GameObject
		EventSystem.current.SetSelectedGameObject(null); // Deselect current selection
		EventSystem.current.SetSelectedGameObject(loseRestartButtonGameObject); // Set new selection
																				//Cursor.visible = true;
																				//Cursor.lockState = CursorLockMode.None;

		//escalatorManager.UpdateMusicState();
		//Time.timeScale = DEFAULT_TIME_SCALE;
	}


	// Sequence of events after player takes damage.

	[ClientCallback]
	private void StartDamageSequence()
	{
		OnPlayerCaught?.Invoke();
		stopGuard = true;
		thirdPersonController.SetCapturedState(true);
		EscalatorManager.Instance.ClearTargetAll(thirdPersonController);
		emeraldAIEventsManager.SetIgnoredTarget(thirdPersonController.transform);
		CmdStopGuard(netId);
		PlayerCinemachineCamera.Priority = 9;
		Camera.main.cullingMask &= ~(1 << LayerMask.NameToLayer("Walls"));
		StartTimer();
		waitForXPressCoroutine = StartCoroutine(WaitForXPress());
		Debug.Log("Started damage sequence");
	}

	[Command]
	private void CmdStopGuard(uint _netId)
	{
		if (NetworkClient.spawned.TryGetValue(_netId, out NetworkIdentity identity))
		{
			if (identity != null)
			{
				ThirdPersonController controller = identity.GetComponent<ThirdPersonController>();
				if (controller != null)
				{
					stopGuard = true;
					EscalatorManager.Instance.ClearTargetAll(controller);
					emeraldAIEventsManager.SetIgnoredTarget(controller.transform);
					controller.SetCapturedState(true);

					Debug.Log("guard stop guard");
				}
			}
		}

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
		isInvincible = true;
		gameObject.tag = "PlayerInvisible";		
		CmdSetInvincibleState(netId, true);
		CapturedText.enabled = true;
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
		if (!isInvincible || emeraldAIEventsManager == null || bribed)
		{
			Debug.Log("Warn! n resume play not working " + isInvincible);
			return;

		}

		bribed = true;
		// Rotate player to face the guard
		Vector3 directionToGuard = emeraldAIEventsManager.transform.position - transform.position;
		directionToGuard.y = 0; // Keep rotation in the horizontal plane
		Quaternion rotationToGuard = Quaternion.LookRotation(directionToGuard);
		transform.rotation = rotationToGuard;

		StopXPressCoroutine();

		TryUpdateMoney(GameManager.Instance.GetCurrentGlobalMoney() - 10);
		PopupTextManager.Instance.ShowPopupText("-10");
		moneyGameObject.SetActive(true);
		animator.SetTrigger("GiveMoney");

		promptUIManager.HideNorthButtonUI();

		StartCoroutine(ResumePlayAfterDelay());
	}

	public void PlayBribeFeedback()
	{
		if (mmFeedbacksBribe != null && emeraldAIEventsManager != null)
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

	[ClientCallback]
	public void DisableMoneyGameObject()
	{
		if (thirdPersonController == null || emeraldAIEventsManager == null)
			return;
		//Play sfx (moneywithdrawin)
		moneyGameObject.SetActive(false);
		thirdPersonController.EnableMovement();
		stopGuard = false;
		CmdResumeGuardMovement();
	}

	[Command]
	private void CmdResumeGuardMovement()
	{
		stopGuard = false;
		emeraldAIEventsManager.ResumeMovement();
		emeraldAIEventsManager.ClearIgnoredTarget(this.transform);
		emeraldAIEventsManager = null;
		Debug.Log("resume guard movement");

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
		isInvincible = false;
		gameObject.tag = "Player";
		gameObject.layer = LayerMask.NameToLayer("PlayerVisible");
		CmdSetInvincibleState(netId, false);
	}

	// Toggle player's invincible state.
	[Command]
	private void CmdSetInvincibleState(uint _netId, bool state)
	{
		if (NetworkClient.spawned.TryGetValue(_netId, out NetworkIdentity identity))
		{
			if (identity != null)
			{
				isInvincible = state;
				identity.gameObject.tag = state ? "PlayerInvisible" : "Player";
				if (state == false)
					identity.gameObject.layer = LayerMask.NameToLayer("PlayerVisible");
			}
		}

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
		EscalatorManager.Instance.ClearTargetAll(thirdPersonController);
	}

	public void ResetInvincible()
	{
		isInvincible = false;
		gameObject.tag = "Player";
	}



	public void AddMoney(int amount)
	{
		TryUpdateMoney(GameManager.Instance.GetCurrentGlobalMoney() + amount);
	}


	public void TryUpdateMoney(int value)
	{
		if (isLocalPlayer)
		{
			CmdRequestMoneyUpdate(value); // Send request to server
		}
	}

	[Command]
	private void CmdRequestMoneyUpdate(int value)
	{
		// Ensure server validates or updates the shared variable
		GameManager.Instance.UpdateGlobalMoney(value);
	}


}