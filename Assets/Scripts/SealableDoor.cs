using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using EPOOutline;
using Mirror;

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
    bool playerIsSealing = false;
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
    }




    void OnTriggerEnter(Collider other)
    {

		// Grab the ThirdPersonController component from the player.
		thirdPersonController = other.GetComponent<ThirdPersonController>();
        if (thirdPersonController != null && thirdPersonController.isLocalPlayer)
        {
            // Check if the triggering object is the player and the door is not sealed.
            if ((other.CompareTag("Player") || other.CompareTag("PlayerInvisible")) && !isSealed)
            {

                promptUIManager = GameObject.Find("InteractionPrompts").GetComponent<InputPromptUIManager>();
                promptUIManager.ShowSouthButtonUI();



                // Update player proximity status.
                playerNearby = true;

                // If the component exists, let the player's controller know about this door.
                    thirdPersonController.SetNearbyDoor(this);

                // Show the sealing progress UI.
                sealProgressBar.gameObject.SetActive(true);
            }
            else if (other.CompareTag("Player") || other.CompareTag("PlayerInvisible"))
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
                if (promptUIManager != null)
                {
                    promptUIManager.HideSouthButtonUI();
                    promptUIManager = null;
                }

                // Reset player status flags.
                playerNearby = false;
                playerIsSealing = false;

                // Clear reference to the door in the player's controller.
                thirdPersonController.ClearNearbyDoor(this);
                thirdPersonController = null;

                // Hide the sealing progress UI.
                audioSource.Stop();
                sealProgressBar.gameObject.SetActive(false);
            }
        }
    }

    [Client]
    public void StartSealing()
    {
        if (thirdPersonController != null && thirdPersonController.isLocalPlayer)
        {
            // Check if the player is near and door isn't sealed.
            if (playerNearby && !isSealed)
            {
                playerIsSealing = true;

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
            else if (!playerNearby)
            {
                StopSealing();
            }
        }
    }
    [Client]
    public void StopSealing()
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

        PopupTextManager.Instance.ShowPopupText("Hacked");

        currentSealTime = 0;
        fillImage.color = gradient.Evaluate(1f);
        outlinable.enabled = false;
        audioSource.Stop();
        audioPlaying = false;

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
