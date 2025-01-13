using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;

public class GrassDecorationStealth : NetworkBehaviour
{
    public Collider collider;
    [SerializeField] List<Transform> locations = new List<Transform>();

    Vector3 hidingPosition;
    Vector3 getOutPosition;

    public bool hiding = false;
    bool playerIsNear = false;
    bool canExitHiding = false;
    bool canEnterHiding = true;

    float positionYBeforeHiding;

    private PlayerDamageHandler playerDamageHandler;
    private ThirdPersonController thirdPersonController;
    private PlayerPositionHolder playerPositionHolder;
    private CharacterController characterController;

	public static event Action OnPlayerHidePlants;


	int exitLocationIndex = 0;
    // Start is called before the first frame update
    void Start()
    {
        hidingPosition = transform.position + new Vector3(0, 1.2f, 0);
        getOutPosition = new Vector3(locations[0].position.x, 0.265f ,locations[0].position.z);
        previousLocation = locations[0];
    }

    private void Update()
    {
        if (hiding)
        {
            HandleExitSelection();
        }
    }

    Transform previousLocation;
    private void HandleExitSelection()
    {
        if(thirdPersonController==null)
            return;
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
        getOutPosition = new Vector3(locations[exitLocationIndex].position.x, positionYBeforeHiding, locations[exitLocationIndex].position.z);
    }

    private void OnTriggerEnter(Collider other)
    {
        if ((other.CompareTag("Player") || other.CompareTag("PlayerInvisible")) && !hiding)
        {
            if (!other.GetComponent<ThirdPersonController>().isLocalPlayer)
                return;

			Debug.Log("Player Entered");
			CmdPlayerEntered(other.GetComponent<NetworkIdentity>());

        }
    }

    [Command(requiresAuthority =false)]
    public void CmdPlayerEntered(NetworkIdentity _networkIdentity)
    {
        RpcPlayerEntered(_networkIdentity);

	}

	[ClientRpc]
	public void RpcPlayerEntered(NetworkIdentity _networkIdentity)
	{
		playerIsNear = true;
		playerDamageHandler = _networkIdentity.GetComponent<PlayerDamageHandler>();
		playerPositionHolder = _networkIdentity.GetComponent<PlayerPositionHolder>();
		thirdPersonController = _networkIdentity.GetComponent<ThirdPersonController>();
		thirdPersonController.SetNearbyGrass(this);
		positionYBeforeHiding = _networkIdentity.transform.position.y;
		characterController = _networkIdentity.GetComponent<CharacterController>();
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
        if (thirdPersonController!=null && _networkIdentity == thirdPersonController.netIdentity)
        {
            playerIsNear = false;
            playerDamageHandler = null;
            thirdPersonController.ClearNearbyGrass();
            thirdPersonController = null;
        }
	}


	public void Interact(NetworkIdentity _networkIdentity)
    {
        if (playerIsNear)
        {
            if (!hiding && canEnterHiding)
            {
                CmdEnterHiding(_networkIdentity);
            }else if (hiding && canExitHiding)
            {
				CmdStopHiding(_networkIdentity);
            }
        }
    }
    [Command(requiresAuthority =false)]
    private void CmdEnterHiding(NetworkIdentity _networkIdentity)
    {

        RpcEnterHiding(_networkIdentity);

	}

	[ClientRpc]
	public void RpcEnterHiding(NetworkIdentity _networkIdentity)
	{
		if (thirdPersonController == null)
			return;
		hiding = true;
		EscalatorManager.Instance.ClearTargetAll(thirdPersonController);
		EscalatorManager.Instance.CheckExposed(thirdPersonController);
		Debug.Log("Started Hiding playerTransform: " + thirdPersonController.transform.position);
		canEnterHiding = false; // Prevent immediate re-entering
		playerPositionHolder.enabled = false;
		thirdPersonController.transform.position = hidingPosition;
		thirdPersonController.gameObject.tag = "PlayerInvisible";
		thirdPersonController.StopMovement();
		StartCoroutine(EnableExitAfterDelay());
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
		if (thirdPersonController == null)
			return;
		if (hiding && canExitHiding)
		{
			Debug.Log("Stop Hiding");
			canExitHiding = false; // Reset exit flag
			hiding = false;
			DisableAllLocations();
			Debug.Log("Before exiting playerTransform: " + thirdPersonController.transform.position + " getOut poisiton is: " + getOutPosition);
			thirdPersonController.transform.position = getOutPosition;
			Debug.Log("After exiting playerTransform: " + thirdPersonController.transform.position + " getOut poisiton is: " + getOutPosition);
			thirdPersonController.gameObject.tag = "Player";
			if (!thirdPersonController.captured)
				thirdPersonController.EnableMovement();
			StartCoroutine(EnableEnterAfterDelay());
			characterController.enabled = true;
			playerPositionHolder.enabled = true;
		}
	}

	void DisableAllLocations()
    {
        foreach (var location in locations)
        {
            location.gameObject.active = false;
        }
    }
    private IEnumerator EnableExitAfterDelay()
    {
        yield return new WaitForSeconds(0.1f);
        canExitHiding = true;
    }
    private IEnumerator EnableEnterAfterDelay()
    {
        yield return new WaitForSeconds(0.1f);
        canEnterHiding = true;
        playerPositionHolder.enabled = true;
    }
}
