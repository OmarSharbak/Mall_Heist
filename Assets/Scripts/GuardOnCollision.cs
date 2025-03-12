using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EmeraldAI.Utility;
using EmeraldAI;

public class GuardOnCollision : MonoBehaviour
{
    [SerializeField] private float closeProximityThreshold = 0.5f; // Example value, adjust as needed
    //[SerializeField] float detectionRadius = 5f; // Radius within which the player is detected. // Edit
    float detectionRadius = 2f;
    [SerializeField] LayerMask playerLayer; // Assign the layer of the player in the Inspector.
    
    public bool canDetect = true;

    EmeraldAIDetection emeraldAIDetection;
    EmeraldAIEventsManager eventsManager;
    Animator animator;

    void Start()
    {
        emeraldAIDetection = GetComponent<EmeraldAIDetection>();
        eventsManager = GetComponent<EmeraldAIEventsManager>();
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (canDetect)
        {
            DetectPlayer();
            DetectPlayerInFront();

            if (playerTransform != null) {
                Vector3 directionToPlayer = playerTransform.position - transform.position;
                float angle = Vector3.Angle(transform.forward, directionToPlayer);
                distanceToPlayer = directionToPlayer.magnitude;
            }

            if (rotating == false)
            {
                eventsManager.CancelRotateAIAwayFromTarget();
            }
        }
            
    }
    void DetectPlayer()
    {
        // Use OverlapSphere to detect the player within the detection radius.
        Collider[] colliders = Physics.OverlapSphere(transform.position, detectionRadius, playerLayer);

        foreach (Collider collider in colliders)
        {
            // Check if the detected object is the player
            if (collider.gameObject.CompareTag("Player"))
            {
                // Set the player as the target
                if (emeraldAIDetection.EmeraldComponent.CurrentTarget == null)
                    emeraldAIDetection.SetDetectedTarget(collider.gameObject.transform);

            }
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red; // Set the color of the Gizmo
        Gizmos.DrawWireSphere(transform.position, detectionRadius); // Draw a wireframe sphere
    }

    float distanceToPlayer;
    Transform playerTransform;
    bool rotating = true;
    public float fieldOfViewAngle = 60f; // Angle for field of view
    public float attackCooldown = 0.3f; // Cooldown time in seconds between attacks

    private float lastAttackTime; // Time when last attack was made

    void DetectPlayerInFront()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, detectionRadius, playerLayer);

        foreach (Collider collider in colliders)
        {
            if (collider.gameObject.CompareTag("Player"))
            {
                playerTransform = collider.gameObject.transform;
                Vector3 directionToPlayer = collider.transform.position - transform.position;
                float angle = Vector3.Angle(transform.forward, directionToPlayer);

                if (angle < fieldOfViewAngle / 2 && Vector3.Distance(transform.position, playerTransform.position) < 1.5f)
                {
                    if(Vector3.Distance(transform.position, playerTransform.position) < 1.0f) //Edit
                    {
                        eventsManager.StopMovement();
                    }
                    

                    if (emeraldAIDetection.EmeraldComponent.CurrentTarget == null)
                    {
                        emeraldAIDetection.SetDetectedTarget(collider.gameObject.transform);
                    }

                    PlayAttackAnimation();
                    break;
                }
            }
        }
    }
    void PlayAttackAnimation()
    {

        if (!animator.GetCurrentAnimatorStateInfo(0).IsName("Run Attack 1") || !animator.GetCurrentAnimatorStateInfo(0).IsName("Attack 1"))
        {
            animator.SetTrigger("Run Attack");
        }

        rotating = false;

        if(playerTransform.GetComponent<PlayerDamageHandler>().isInvincible == false) 
        {
            eventsManager.ResumeMovement();
            rotating = true;
        }
    }
}
