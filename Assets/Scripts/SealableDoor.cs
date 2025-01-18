using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using EPOOutline;
using Mirror;
using I2.Loc;
using static UnityEditor.Progress;

public class SealableDoor : NetworkBehaviour
{
	[SerializeField]
	// UI element to display the progress of sealing the door.
	Slider sealProgressBar;

	[SerializeField]
	// UI Image that will show the fill of the progress bar.
	Image fillImage;

	[SerializeField]
	// Gradient to modify fill color based on sealing progress.
	Gradient gradient;

	InputPromptUIManager promptUIManager;

	[SyncVar(hook = nameof(OnSealedStateChanged))]
	public bool isSealed = false; // Syncs the sealed state across clients


	// Time duration to complete the sealing process.
	public float sealDuration = 5.0f;

	// Timer to keep track of how long the door has been sealed.
	float currentSealTime = 0.0f;

	// Flags to determine player's proximity and action status.
	bool playerNearby = false;
	[SyncVar]
	public bool playerIsSealing = false;
	bool audioPlaying = false;

	// Reference to player's controller script.
	ThirdPersonController thirdPersonController;

	AudioManager audioManager;
	AudioSource audioSource;

	Outlinable outlinable;

	void Start()
	{
		audioManager = GameObject.Find("Player/Sounds").GetComponent<AudioManager>();
		outlinable = GetComponent<Outlinable>();
		audioSource = GetComponent<AudioSource>();

	}

	void Update()
	{
		// Check if player is not sealing and currentSealTime is above zero.
		if (!playerIsSealing && currentSealTime > 0)
		{
			// Decrease the seal timer at the same rate it increases.
			currentSealTime -= Time.deltaTime;

			// Ensure the timer doesn't drop below zero.
			currentSealTime = Mathf.Max(currentSealTime, 0);

			// Update the progress UI.
			sealProgressBar.value = Mathf.Clamp01(currentSealTime / sealDuration);
			fillImage.color = gradient.Evaluate(sealProgressBar.value);
		}
		else if (!isSealed && playerIsSealing)
		{
			// Increment the seal timer.
			currentSealTime += Time.deltaTime;

			if (audioPlaying == false)
			{
				audioSource.Play();
				audioPlaying = true;
			}

			// Update the progress UI.
			sealProgressBar.value = Mathf.Clamp01(currentSealTime / sealDuration);
			fillImage.color = gradient.Evaluate(sealProgressBar.value);

			// If sealing is complete.
			if (currentSealTime >= sealDuration)
			{
				CmdSealDoor();
			}
		}
	}




	void OnTriggerEnter(Collider other)
	{
		//Debug.Log("Client: Sealed door - On trigger enter");

		// Grab the ThirdPersonController component from the player.
		thirdPersonController = other.GetComponent<ThirdPersonController>();
		if (thirdPersonController != null && thirdPersonController.isLocalPlayer)
		{
			// Check if the triggering object is the player and the door is not sealed.
			if ((other.CompareTag("Player") || other.CompareTag("PlayerInvisible")) && !isSealed)
			{

				CmdOnEnter(thirdPersonController);
			}
			else if (other.CompareTag("Player") || other.CompareTag("PlayerInvisible"))
				CmdOnEnterAndSealed();
		}
	}
	[Command(requiresAuthority = false)]

	private void CmdOnEnterAndSealed()
	{
		RpcOnEnterAndSealed();
	}

	[ClientRpc]
	private void RpcOnEnterAndSealed()
	{
		sealProgressBar.gameObject.SetActive(true);
	}
	[Command(requiresAuthority = false)]

	private void CmdOnEnter(ThirdPersonController controller)
	{
		//Debug.Log("Server: Sealed door - On enter");

		RpcOnEnter(controller);
	}

	[ClientRpc]
	private void RpcOnEnter(ThirdPersonController controller)
	{
		//Debug.Log("Client RPC: Sealed door - On enter");


		if (controller != null)
		{
			thirdPersonController = controller;

			if (controller.isLocalPlayer)
			{
				var intP = GameObject.Find("InteractionPrompts");
				if (intP != null)
				{
					promptUIManager = intP.GetComponent<InputPromptUIManager>();
					if (promptUIManager != null)
						promptUIManager.ShowSouthButtonUI();
				}
			}

			//Debug.Log("Client RPC: Sealed door - passed");


			// Update player proximity status.
			playerNearby = true;

			// If the component exists, let the player's controller know about this door.
			controller.SetNearbyDoor(this);

			// Show the sealing progress UI.
			sealProgressBar.gameObject.SetActive(true);
		}
	}

	void OnTriggerExit(Collider other)
	{
		// Grab the ThirdPersonController component from the player.
		thirdPersonController = other.GetComponent<ThirdPersonController>();
		if (thirdPersonController != null && thirdPersonController.isLocalPlayer)
		{
			// Check if the exiting object is the player.
			if (other.CompareTag("Player") || other.CompareTag("PlayerInvisible"))
			{

				CmdOnExit(thirdPersonController);
			}
		}
	}
	[Command(requiresAuthority = false)]

	private void CmdOnExit(ThirdPersonController controller)
	{
		RpcOnExit(controller);
	}

	[ClientRpc]
	private void RpcOnExit(ThirdPersonController controller)
	{
		if (thirdPersonController != null && thirdPersonController.isLocalPlayer && promptUIManager != null)
		{
			promptUIManager.HideSouthButtonUI();
			promptUIManager = null;
		}

		// Reset player status flags.
		playerNearby = false;
		playerIsSealing = false;

		// Clear reference to the door in the player's controller.
		controller.ClearNearbyDoor(this);
		thirdPersonController = null;

		// Hide the sealing progress UI.
		audioSource.Stop();
		sealProgressBar.gameObject.SetActive(false);
	}


	[Command(requiresAuthority = false)]
	public void CmdStartSealing()
	{
		//Debug.Log("Server CMD: Sealed door - start sealing");

		RpcStartSealing();
	}

	[ClientRpc]
	public void RpcStartSealing()
	{
		//Debug.Log("Client RPC : Sealed door - start sealing");

		if (thirdPersonController != null)
		{
			// Check if the player is near and door isn't sealed.
			if (playerNearby && !isSealed)
			{
				CmdSetSealing();

			}
			else if (!playerNearby)
			{
				//Debug.Log("Client RPC : Sealed door - start sealing - player not near")	;

				StopSealing();

			}
		}
		else
		{
			//Debug.Log("Client RPC : Sealed door - start sealing - controller null");
		}
	}

	[Command(requiresAuthority = false)]
	public void CmdSetSealing()
	{
		//Debug.Log("Server CMD: Sealed door - set sealing");

		playerIsSealing = true;
	}

	[Command(requiresAuthority = false)]
	public void CmdStopSealing()
	{
		RpcStopSealing();
	}

	[ClientRpc]
	public void RpcStopSealing()
	{
		StopSealing();
	}
	[Client]
	private void StopSealing()
	{
		// Reset the flag when the player stops sealing.
		playerIsSealing = false;
		if (!isSealed)
		{
			audioSource.Stop();
			audioPlaying = false;
		}
	}
	// Hook to update visuals when `isSealed` changes
	private void OnSealedStateChanged(bool oldValue, bool newValue)
	{
		UpdateDoorVisual();
	}

	[Client]
	void UpdateDoorVisual()
	{
		// Mark the door as sealed and reset the seal timer.
		if (promptUIManager != null)
		{
			promptUIManager.HideSouthButtonUI();
			promptUIManager = null;
		}

		LocalizedString locString = "Hacked";
		PopupTextManager.Instance.ShowPopupText(locString);

		currentSealTime = 0;
		fillImage.color = gradient.Evaluate(1f);
		outlinable.enabled = false;
		audioSource.Stop();
		audioPlaying = false;

		// Update the progress UI.
		sealProgressBar.value = 1f;
		fillImage.color = gradient.Evaluate(sealProgressBar.value);

		// Use a coroutine to delay playing of the "ObjectiveComplete" sound
		StartCoroutine(PlayObjectiveCompleteSound());
	}

	IEnumerator PlayObjectiveCompleteSound()
	{
		yield return new WaitForSeconds(0.1f); // Delay of 0.1 seconds. Adjust as needed.
		audioManager.PlayAudio("ObjectiveComplete");
	}

	// Called on the server when a player tries to seal the door
	[Command(requiresAuthority = false)]
	public void CmdSealDoor()
	{
		if (!isSealed) // Ensure door is not already sealed
		{
			isSealed = true; // SyncVar will automatically update clients
		}
	}
}
