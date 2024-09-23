using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;

public class GrassDecorationStealth : MonoBehaviour
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
            Debug.Log("Player Entered");
            playerIsNear = true;
            playerDamageHandler = other.GetComponent<PlayerDamageHandler>();
            thirdPersonController = other.GetComponent<ThirdPersonController>();
            playerPositionHolder = other.GetComponent<PlayerPositionHolder>();
            thirdPersonController.SetNearbyGrass(this);
            positionYBeforeHiding = other.transform.position.y;
            characterController = other.GetComponent<CharacterController>();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") || other.CompareTag("PlayerInvisible"))
        {
            Debug.Log("Player Exited");
            playerIsNear = false;
            playerDamageHandler = null;
            thirdPersonController.ClearNearbyGrass();
            thirdPersonController = null;
        }
    }

    public void Interact()
    {
        if (playerIsNear)
        {
            if (!hiding && canEnterHiding)
            {
                EnterHiding();
            }else if (hiding && canExitHiding)
            {
                StopHiding();
            }
        }
    }
    private void EnterHiding()
    {
        hiding = true;
        EscalatorManager.Instance.ClearTargetAll();
        EscalatorManager.Instance.CheckExposed();
        Debug.Log("Started Hiding playerTransform: " + thirdPersonController.transform.position);
        canEnterHiding = false; // Prevent immediate re-entering
        playerPositionHolder.enabled = false;
        thirdPersonController.transform.position = hidingPosition;
        thirdPersonController.gameObject.tag = "PlayerInvisible";
        thirdPersonController.StopMovement();  
        StartCoroutine(EnableExitAfterDelay());
        characterController.enabled = false;
        
    }
    public void StopHiding()
    {
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
