using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EPOOutline;
using MoreMountains.Feedbacks;
using Mirror;

public class Register : NetworkBehaviour
{
	PlayerDamageHandler playerDamageHandler;
	ThirdPersonController thirdPersonController;
	AudioSource audioSource;
	Outlinable outlinable;

	public int moneyAmount = 10;

	MMFeedbacks mmFeedbacksSteal;

	bool playerIsNear = false;

	[SyncVar(hook = nameof(OnRegisterInteracted))]
	public bool interacted = false;

	private void Start()
	{
		audioSource = GetComponent<AudioSource>();
		outlinable = GetComponent<Outlinable>();
		mmFeedbacksSteal = GameObject.Find("MMFeedbacks(steal)").GetComponent<MMFeedbacks>();
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag("Player") || other.CompareTag("PlayerInvisible"))
		{
			playerIsNear = true;
			playerDamageHandler = other.GetComponent<PlayerDamageHandler>();
			thirdPersonController = other.GetComponent<ThirdPersonController>();
			thirdPersonController.SetNearbyRegister(this);
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (other.CompareTag("Player") || other.CompareTag("PlayerInvisible"))
		{
			playerIsNear = false;
			thirdPersonController.ClearNearbyRegister();
			thirdPersonController = null;
		}
	}

	public void Interact()
	{
		if (!interacted && playerIsNear)
		{

			CmdSetInteracted(true);


		}
	}
	private void OnRegisterInteracted(bool _oldValue, bool _newValue)
	{
		Debug.Log("CLIENT - register interacted");

		if (audioSource != null)
		{
			Debug.Log("Money added.");
			audioSource.Play();
		}

		if (mmFeedbacksSteal != null)
			mmFeedbacksSteal.PlayFeedbacks();

		if (playerDamageHandler != null)
		{
			playerDamageHandler.AddMoney(moneyAmount);
			playerDamageHandler = null;

		}
		else
		{
			Debug.LogWarning("Money not added because the playerDamageHandler is null");
		}
		if (outlinable != null)
			outlinable.enabled = false;

	}

	[Command(requiresAuthority = false)]
	public void CmdSetInteracted(bool _newValue)
	{
		Debug.Log("SERVER - register interacted");

		interacted = _newValue;

	}
}
