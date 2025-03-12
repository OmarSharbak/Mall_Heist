using EPOOutline;
using Mirror;
using MoreMountains.Feedbacks;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LockDoor : NetworkBehaviour
{
	[SerializeField] List<SealableDoor> sealableDoors;
	[SerializeField] TMP_Text doorOpenedText;
	[SerializeField] TMP_Text sealablesText;

	Outlinable outlinable;
	Animator animator;
	MMFeedbacks mmFeedbacksOpenDoor;


	[SyncVar(hook = nameof(OnDoorStateChanged))]
	private bool isOpened = false;

	private int totalSeals = 0;

	private void Start()
	{
		outlinable = GetComponent<Outlinable>();
		animator = GetComponent<Animator>();
		doorOpenedText.enabled = false; // Ensure the text is initially disabled
		mmFeedbacksOpenDoor = GameObject.Find("MMFeedbacks(opendoor)").GetComponent<MMFeedbacks>();
		totalSeals = sealableDoors.Count;
		sealablesText.color = outlinable.FrontParameters.Color;
		if(totalSeals==0)
			sealablesText.text = "";


	}

	void Update()
	{
		if (!isOpened && AreAllDoorsSealed())
		{
			OpenDoor();
		}
		else if (!isOpened)
		{
			int sealedDoors = 0;
			foreach (SealableDoor door in sealableDoors)
			{
				if (door.isSealed)
					sealedDoors++;
			}
			sealablesText.text = sealedDoors + "/" + totalSeals;
		}
		else
		{
			sealablesText.text = "";
		}
	}
	private void OnDoorStateChanged(bool oldState, bool newState)
	{
		if (newState)
		{
			OpenDoorClient(); // Trigger animations and effects for all clients
		}
	}

	[Client]
	private void OpenDoor()
	{
		CmdOpenDoor();
	}

	// Called on the server when a player tries to seal the door
	[Command(requiresAuthority = false)]
	public void CmdOpenDoor()
	{
		if (!isOpened) // Ensure door is not already sealed
		{
			isOpened = true; // SyncVar will automatically update clients
		}
	}

	[Client]
	private void OpenDoorClient()
	{
		animator.SetTrigger("OpenDoor");
		outlinable.FrontParameters.Color = Color.green;
		outlinable.BackParameters.Color = Color.green;

		StartCoroutine(ShowDoorOpenedText());
	}

	IEnumerator ShowDoorOpenedText()
	{
		if (mmFeedbacksOpenDoor != null)
			mmFeedbacksOpenDoor.PlayFeedbacks();

		doorOpenedText.enabled = true; // Enable the text
		yield return new WaitForSeconds(3); // Wait for 2 seconds
		doorOpenedText.enabled = false; // Disable the text
	}

	private bool AreAllDoorsSealed()
	{
		foreach (SealableDoor door in sealableDoors)
		{
			if (!door.isSealed)
				return false;
		}
		return true;
	}
}
