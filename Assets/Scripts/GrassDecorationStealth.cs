using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Cinemachine.CinemachineOrbitalTransposer;

public class GrassDecorationStealth : NetworkBehaviour
{
	public Collider collider;
	[SerializeField] List<Transform> locations = new List<Transform>();

	Vector3 hidingPosition;
	Vector3 getOutPosition;

	//public bool hiding = false;
	//bool playerIsNear = false;
	//bool canExitHiding = false;
	//bool canEnterHiding = true;

	//float positionYBeforeHiding;

	//private PlayerDamageHandler playerDamageHandler;
	//private ThirdPersonController thirdPersonController;
	//private PlayerPositionHolder playerPositionHolder;
	//private CharacterController characterController;

	public static event Action OnPlayerHidePlants;

	private List<NetworkIdentity> playersNear = new List<NetworkIdentity>();


	int exitLocationIndex = 0;
	// Start is called before the first frame update
	void Start()
	{
		hidingPosition = transform.position + new Vector3(0, 1.2f, 0);
		getOutPosition = new Vector3(locations[0].position.x, 0.265f, locations[0].position.z);
		previousLocation = locations[0];
	}

	private void Update()
	{
		foreach (var player in playersNear)
		{
			if (player.GetComponent<ThirdPersonController>().hiding)
			{
				HandleExitSelection(player);
			}
		}
	}

	Transform previousLocation;
	private void HandleExitSelection(NetworkIdentity _networkIdentity)
	{
		if (!playersNear.Contains(_networkIdentity))
			return;
		ThirdPersonController thirdPersonController = _networkIdentity.GetComponent<ThirdPersonController>();

		Vector3 inputDirection = thirdPersonController.inputDirection; // Accessing input direction


		// Assuming Forward = 0, Right = 1, Backward = 2, Left = 3
		if (inputDirection.z > 0) // Moving Forward
		{
			exitLocationIndex = 1;
		}
		else if (inputDirection.x > 0) // Moving Right
		{
			exitLocationIndex = 0;
		}
		else if (inputDirection.z < 0) // Moving Backward
		{
			exitLocationIndex = 3;
		}
		else if (inputDirection.x < 0) // Moving Left
		{
			exitLocationIndex = 2;
		}

		if (previousLocation != locations[exitLocationIndex])
		{
			previousLocation.gameObject.SetActive(false);
			previousLocation = locations[exitLocationIndex];
		}

		locations[exitLocationIndex].gameObject.SetActive(true);
		if (locations[exitLocationIndex].gameObject.GetComponent<ParticleSystem>().isPlaying == false)
		{
			locations[exitLocationIndex].gameObject.GetComponent<ParticleSystem>().Play();
		}
		getOutPosition = new Vector3(locations[exitLocationIndex].position.x, thirdPersonController.positionYBeforeHiding, locations[exitLocationIndex].position.z);
	}

	private void OnTriggerEnter(Collider other)
	{
		if ((other.CompareTag("Player") || other.CompareTag("PlayerInvisible")) && !other.GetComponent<ThirdPersonController>().hiding)
		{
			if (!other.GetComponent<ThirdPersonController>().isLocalPlayer)
				return;

			Debug.Log("Player Entered");
			CmdPlayerEntered(other.GetComponent<NetworkIdentity>());

		}
	}

	[Command(requiresAuthority = false)]
	public void CmdPlayerEntered(NetworkIdentity _networkIdentity)
	{
		RpcPlayerEntered(_networkIdentity);

	}

	[ClientRpc]
	public void RpcPlayerEntered(NetworkIdentity _networkIdentity)
	{
		if (!playersNear.Contains(_networkIdentity))
		{
			playersNear.Add(_networkIdentity);
			_networkIdentity.GetComponent<ThirdPersonController>().SetNearbyGrass(this, _networkIdentity.transform.position.y);

		}

		//playerIsNear = true;
		//playerDamageHandler = _networkIdentity.GetComponent<PlayerDamageHandler>();
		//playerPositionHolder = _networkIdentity.GetComponent<PlayerPositionHolder>();
		//characterController = _networkIdentity.GetComponent<CharacterController>();
	}


	private void OnTriggerExit(Collider other)
	{
		if (other.CompareTag("Player") || other.CompareTag("PlayerInvisible"))
		{
			if (!other.GetComponent<ThirdPersonController>().isLocalPlayer)
				return;
			Debug.Log("Player Exited");
			CmdPlayerExited(other.GetComponent<NetworkIdentity>());

		}
	}

	[Command(requiresAuthority = false)]
	public void CmdPlayerExited(NetworkIdentity _networkIdentity)
	{
		RpcPlayerExited(_networkIdentity);
	}

	[ClientRpc]
	public void RpcPlayerExited(NetworkIdentity _networkIdentity)
	{
		//if (thirdPersonController!=null && _networkIdentity == thirdPersonController.netIdentity)
		//{
		if (playersNear.Contains(_networkIdentity))
		{
			playersNear.Remove(_networkIdentity);
			_networkIdentity.GetComponent<ThirdPersonController>().ClearNearbyGrass();
		}
		//playerIsNear = false;
		//         playerDamageHandler = null;
		//         thirdPersonController = null;
	}



	public void Interact(NetworkIdentity _networkIdentity)
	{
		Debug.Log("Interact grass "+playersNear.Contains(_networkIdentity));
		if (playersNear.Contains(_networkIdentity))
		{
			ThirdPersonController thirdPersonController = _networkIdentity.GetComponent<ThirdPersonController>();

			Debug.Log(thirdPersonController.hiding +" "+ thirdPersonController.canEnterHiding);

			if (!thirdPersonController.hiding && thirdPersonController.canEnterHiding)
			{
				CmdEnterHiding(_networkIdentity);
			}
			else if (thirdPersonController.hiding && thirdPersonController.canExitHiding)
			{
				CmdStopHiding(_networkIdentity);
			}
		}
	}
	[Command(requiresAuthority = false)]
	private void CmdEnterHiding(NetworkIdentity _networkIdentity)
	{

		RpcEnterHiding(_networkIdentity);

	}

	[ClientRpc]
	public void RpcEnterHiding(NetworkIdentity _networkIdentity)
	{
		Debug.Log("enter hidding" + playersNear.Contains(_networkIdentity));

		if (!playersNear.Contains(_networkIdentity))
			return;
		ThirdPersonController thirdPersonController = _networkIdentity.GetComponent<ThirdPersonController>();
		CharacterController characterController = _networkIdentity.GetComponent<CharacterController>();


		thirdPersonController.hiding = true;
		EscalatorManager.Instance.ClearTargetAll(thirdPersonController);
		EscalatorManager.Instance.CheckExposed(thirdPersonController);
		Debug.Log("Started Hiding playerTransform: " + thirdPersonController.transform.position);
		thirdPersonController.canEnterHiding = false; // Prevent immediate re-entering
		thirdPersonController.EnablePlayerPositionHolder(false);
		thirdPersonController.transform.position = hidingPosition;
		thirdPersonController.gameObject.tag = "PlayerInvisible";
		thirdPersonController.StopMovement();
		StartCoroutine(EnableExitAfterDelay(thirdPersonController));
		characterController.enabled = false;
		OnPlayerHidePlants?.Invoke();
	}

	[Command(requiresAuthority = false)]
	public void CmdStopHiding(NetworkIdentity _networkIdentity)
	{

		RpcStopHiding(_networkIdentity);
	}

	[ClientRpc]
	public void RpcStopHiding(NetworkIdentity _networkIdentity)
	{
		if (!playersNear.Contains(_networkIdentity))
			return;
		ThirdPersonController thirdPersonController = _networkIdentity.GetComponent<ThirdPersonController>();
		CharacterController characterController = _networkIdentity.GetComponent<CharacterController>();


		if (thirdPersonController.hiding && thirdPersonController.canExitHiding)
		{
			Debug.Log("Stop Hiding");
			thirdPersonController.canExitHiding = false; // Reset exit flag
			thirdPersonController.hiding = false;
			DisableAllLocations();
			Debug.Log("Before exiting playerTransform: " + thirdPersonController.transform.position + " getOut poisiton is: " + getOutPosition);
			thirdPersonController.transform.position = getOutPosition;
			Debug.Log("After exiting playerTransform: " + thirdPersonController.transform.position + " getOut poisiton is: " + getOutPosition);
			thirdPersonController.gameObject.tag = "Player";
			if (!thirdPersonController.captured)
				thirdPersonController.EnableMovement();
			StartCoroutine(EnableEnterAfterDelay(thirdPersonController));
			characterController.enabled = true;
			thirdPersonController.EnablePlayerPositionHolder(true);
		}
	}

	void DisableAllLocations()
	{
		foreach (var location in locations)
		{
			location.gameObject.active = false;
		}
	}
	private IEnumerator EnableExitAfterDelay(ThirdPersonController thirdPersonController)
	{
		yield return new WaitForSeconds(0.1f);
		thirdPersonController.canExitHiding = true;
	}
	private IEnumerator EnableEnterAfterDelay(ThirdPersonController thirdPersonController)
	{
		yield return new WaitForSeconds(0.1f);
		thirdPersonController.canEnterHiding = true;
		thirdPersonController.EnablePlayerPositionHolder(true);
	}
}
