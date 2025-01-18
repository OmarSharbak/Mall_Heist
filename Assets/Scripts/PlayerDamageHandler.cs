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
using UnityEngine.AI;
using static UnityEngine.Rendering.DebugUI;
using Unity.VisualScripting;
using I2.Loc;

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
	[SyncVar]

	public bool isInvincible = false;
	[SerializeField] private float InvincibilityTime = 3.0f;
	private const float WaitToPressXTime = 10.0f;
	private float currentWaitToPressXTime;
	[SyncVar]

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

	[SyncVar]
	bool stopGuard = false;

	public static event Action OnPlayerCaught;

	[SyncVar]

	bool bribed = false;

	private GameObject spectatingText = null;
	private GameObject overlay = null;

	private bool damageStarted=false;

	private void Initialize()
	{
		// Dynamically find the UI elements (e.g., by name or tag)
		regularCanvas = GameObject.Find("InGameUI");

		lostCanvas = GameObject.Find("LostUI");
		LivesText = GameObject.Find("LivesText")?.GetComponent<TMP_Text>();

		CapturedText = GameObject.Find("CapturedText")?.GetComponent<TMP_Text>();
		TimerText = GameObject.Find("WaitForXTimerText")?.GetComponent<TMP_Text>();
		PlayerCinemachineCamera = GameObject.Find("PlayerFollowCamera(Regular)")?.GetComponent<CinemachineVirtualCamera>();
		escalatorManager = GameObject.Find("Escalator")?.GetComponent<EscalatorManager>();
		mmFeedbacksCaptured = GameObject.Find("MMFeedbacks(captured)")?.GetComponent<MMFeedbacks>();
		mmFeedbacksBribe = GameObject.Find("MMFeedbacks(bribe)")?.GetComponent<MMFeedbacks>();
		promptUIManager = GameObject.Find("InteractionPrompts").GetComponent<InputPromptUIManager>();


		loseRestartButtonGameObject = GameObject.Find("RestartButton");
		outliner = GameObject.Find("MainCamera")?.GetComponent<Outliner>();

		thirdPersonController = GetComponent<ThirdPersonController>();

		myAudioSource = GetComponent<AudioSource>();
		animator = GetComponent<Animator>();
		playerFlash = GetComponent<PlayerFlash>();

		spectatingText = GameObject.Find("SpectatingText");
		overlay = GameObject.Find("Overlay");


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

		lostCanvas.SetActive(false);
		LivesText.text = Lives.ToString();
		LivesText.gameObject.SetActive(false);


		spectatingText.SetActive(false);
		overlay.SetActive(false);

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

		if (emeraldAIEventsManager != null && stopGuard == true)
		{
			GuardStop();
		}
	}
	public void GuardStop()
	{
		//Debug.Log("CLIENT -guard stopMovement");

		emeraldAIEventsManager.StopMovement();
		CmdStopGuardMovement(emeraldAIEventsManager.transform.GetComponent<NetworkIdentity>().netId);
	}

	[Command(requiresAuthority = false)]
	public void CmdStopGuardMovement(uint _netId)
	{
		if (NetworkClient.spawned.TryGetValue(_netId, out NetworkIdentity identity))
		{
			if (identity != null)
			{
				EmeraldAIEventsManager emerald = identity.transform.GetComponent<EmeraldAIEventsManager>();
				emerald.StopMovement();
				//Debug.Log("SERVER - guard stopMovement");

			}

		}
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
		if (damageStarted)
			return;
		damageStarted = true;
		Debug.Log("PLayer Damaged");
		PlayTakeDownSound();

		gameObject.layer = LayerMask.NameToLayer("PlayerInvisible");
		CmdPlayerInvisible(netId);

		if (mmFeedbacksCaptured != null)
			mmFeedbacksCaptured.PlayFeedbacks();

		if (GameManager.Instance.GetCurrentGlobalMoney() >= 10)
		{
			damageStarted = false;
			StartDamageSequence();
		}
		else
		{
			CmdHandleGameOver(netId);
		}
	}

	[Command]

	public void CmdPlayerInvisible(uint _netId)
	{
		if (NetworkClient.spawned.TryGetValue(_netId, out NetworkIdentity identity))
		{
			if (identity != null)
			{
				identity.gameObject.layer = LayerMask.NameToLayer("PlayerInvisible");
				Debug.Log("SERVER - Player invisible");
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

	[Command(requiresAuthority = false)]
	// Manage end of game state.
	private void CmdHandleGameOver(uint _netId)
	{
		//Debug.Log("Cmd Handle game over " + EscalatorManager.Instance.defeatedPlayersUids.Count);

		if (!EscalatorManager.Instance.defeatedPlayersUids.Contains(_netId))
		{
			EscalatorManager.Instance.defeatedPlayersUids.Add(_netId);
		}

		if ((EscalatorManager.Instance.defeatedPlayersUids.Count == 2) || (MultiplayerMode.Instance != null && MultiplayerMode.Instance.isSinglePlayer))
		{
			AllDefeat();
		}
		//Set camera follow the other player
		else if (NetworkClient.spawned.TryGetValue(_netId, out NetworkIdentity identity))
		{
			if (identity != null)
			{
				ThirdPersonController ownController = identity.transform.GetComponent<ThirdPersonController>();

				foreach (var player in CustomNetworkManager.connectedPlayers)
				{
					ThirdPersonController otherController = player.transform.GetComponent<ThirdPersonController>();
					if (otherController != null)
					{
						Debug.Log("CLIENT - own:" + ownController.transform.name + " other:" + otherController.transform.name);
						if ((otherController != ownController) && (EscalatorManager.Instance.GetCurrentState(otherController) != EscalatorManager.GameState.Defeat))
						{
							Defeat(ownController.netId, otherController.netId);

						}
					}
				}
			}
		}
	}

	[ServerCallback]
	private void AllDefeat()
	{
		foreach (var player in CustomNetworkManager.connectedPlayers)
		{
			PlayerDamageHandler damage = player.transform.GetComponent<PlayerDamageHandler>();
			if (damage != null)
			{
				damage.RpcAllDefeat();
				Debug.Log("SERVER - all defeat" + damage.transform.name);
			}

		}
	}

	[ClientRpc]
	private void RpcAllDefeat()
	{
		if (!isLocalPlayer)
			return;
		lostCanvas.SetActive(true);
		spectatingText.SetActive(false);
		if (MultiplayerMode.Instance != null && MultiplayerMode.Instance.isSinglePlayer)
			EventSystem.current.SetSelectedGameObject(loseRestartButtonGameObject);
		else
			loseRestartButtonGameObject.SetActive(false);
		Debug.Log("CLIENT - all defeat" + transform.name);
		EscalatorManager.Instance.SetCurrentState(thirdPersonController, EscalatorManager.GameState.Defeat);
		Debug.Log("CLIENT CURRENT STATE IS: " + EscalatorManager.Instance.GetCurrentState(thirdPersonController));

	}

	[ServerCallback]
	private void Defeat(uint _netId, uint _otherNetId)
	{
		if (NetworkClient.spawned.TryGetValue(_netId, out NetworkIdentity identity))
		{
			if (identity != null)
			{
				TargetDefeat(identity.connectionToClient, _otherNetId);
				RpcDisableCollider(_netId);
				identity.GetComponent<ThirdPersonController>().defeated = true;


			}
		}
	}

	[TargetRpc]
	private void TargetDefeat(NetworkConnectionToClient target, uint _otherNetId)
	{
		if (NetworkClient.spawned.TryGetValue(_otherNetId, out NetworkIdentity otherIdentity))
		{
			if (otherIdentity != null)
			{
				ThirdPersonController otherController = otherIdentity.transform.GetComponent<ThirdPersonController>();

				//other player otherController
				thirdPersonController.FollowCinemachineCamera.Follow = otherController.CinemachineCameraTarget.transform;
				thirdPersonController.FollowTopCinemachineCamera.Priority = 0;
				thirdPersonController.FollowCinemachineCamera.Priority = 20;
				thirdPersonController.FullMapCinemachineCamera.Priority = 0;
				Debug.Log("CLIENT - camera " + transform.name + " follow other " + otherController.transform.name);

				regularCanvas.SetActive(false);
				outliner.enabled = false;

				spectatingText.SetActive(true);

				// Access the EventSystem and set the selected GameObject
				EventSystem.current.SetSelectedGameObject(null); // Deselect current selection
			}
		}
	}



	[ClientRpc]
	private void RpcDisableCollider(uint _netId)
	{
		if (NetworkClient.spawned.TryGetValue(_netId, out NetworkIdentity identity))
		{
			if (identity != null)
			{
				ThirdPersonController player = identity.transform.GetComponent<ThirdPersonController>();
				if (player != null)
				{
					player.gameObject.layer = LayerMask.NameToLayer("PlayerInvisible");
					player.gameObject.tag = "PlayerInvisible";
					player.ToggleVisibility();

					player.GetComponent<Rigidbody>().isKinematic = true;
					Transform[] ts = player.GetComponentsInChildren<Transform>();


					Inventory inventory = player.GetComponent<Inventory>();
					if (inventory != null && inventory.heldItem != null)
						inventory.CmdDestroyHeldItem(inventory.heldItem.GetComponent<NetworkIdentity>());

					for (int i = 0; i < ts.Length; i++)
					{
						Transform t = ts[i];
						t.gameObject.SetActive(false);
					}


					EscalatorManager.Instance.SetCurrentState(player, EscalatorManager.GameState.Defeat);
					Debug.Log("CLIENT CURRENT STATE IS: " + EscalatorManager.Instance.GetCurrentState(player));

				}
			}
		}


	}


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

	[Command(requiresAuthority = false)]
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

					Debug.Log("SERVER - guard stop guard");
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

		TryUpdateMoney(GameManager.Instance.GetCurrentGlobalMoney(), -10);
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
		LocalizedString locString = "Bribed";
		PopupTextManager.Instance.ShowPopupText(locString);
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
		CmdResumeGuardMovement(emeraldAIEventsManager.transform.GetComponent<NetworkIdentity>().netId);
	}

	[Command(requiresAuthority = false)]
	private void CmdResumeGuardMovement(uint _netId)
	{

		if (NetworkClient.spawned.TryGetValue(_netId, out NetworkIdentity identity))
		{
			if (identity != null)
			{
				EmeraldAIEventsManager emerald = identity.transform.GetComponent<EmeraldAIEventsManager>();
				if (emerald != null)
				{
					stopGuard = false;
					identity.transform.GetComponent<NavMeshAgent>().isStopped = false;
					emerald.ResumeMovement();
					emerald.ClearIgnoredTarget(transform);
					Debug.Log("SERVER - resume guard movement");
				}
			}
		}

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
		Debug.Log("CLIENT - Player Invincible");

	}

	// Toggle player's invincible state.
	[Command]
	private void CmdSetInvincibleState(uint _netId, bool state)
	{
		if (NetworkClient.spawned.TryGetValue(_netId, out NetworkIdentity identity))
		{
			if (identity != null)
			{
				Debug.Log("SERVER -Player Invincible");

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
		TryUpdateMoney(GameManager.Instance.GetCurrentGlobalMoney(), amount);
	}


	public void TryUpdateMoney(int value, int amount)
	{
		if (isLocalPlayer)
		{
			CmdRequestMoneyUpdate(value, amount); // Send request to server
		}
	}

	[Command]
	private void CmdRequestMoneyUpdate(int value, int amount)
	{
		// Ensure server validates or updates the shared variable
		GameManager.Instance.UpdateGlobalMoney(value + amount);

		RpcUpdateClientMoney(amount);
	}

	[ClientRpc]
	private void RpcUpdateClientMoney(int amount)
	{
		if (amount > 0)
		{
			EscalatorManager.Instance.moneyCollected += amount;
			PopupTextManager.Instance.ShowPopupText("+ " + amount);
		}
	}

}