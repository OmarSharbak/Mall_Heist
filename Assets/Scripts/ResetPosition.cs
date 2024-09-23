using UnityEngine;
using EmeraldAI;
using UnityEngine.AI;
using System.Collections;
using EmeraldAI.Utility;

public class ResetPosition : MonoBehaviour
{
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private EmeraldAISystem aiSystem;
    private NavMeshAgent navMeshAgent;
    private EmeraldAIDetection emeraldAIDetection;

    void Start()
    {
        // Store the original position and rotation
        originalPosition = transform.position;
        originalRotation = transform.rotation;

        // Get the Emerald AI and NavMeshAgent components
        aiSystem = GetComponent<EmeraldAISystem>();
        navMeshAgent = GetComponent<NavMeshAgent>();
        emeraldAIDetection = GetComponent<EmeraldAIDetection>();
    }

    // Call this function to reset the AI's position and rotation
    public void ResetToOriginalState()
    {
        if (aiSystem != null && navMeshAgent != null)
        {
            // Stop the AI's current actions

            // Reset the AI's destination
            navMeshAgent.SetDestination(originalPosition);

            if (emeraldAIDetection.EmeraldComponent.CurrentTarget == null)
            {
                // Once the agent reaches the destination, reset the rotation
                StartCoroutine(ResetRotationWhenClose());
            }
           
        }
    }

    IEnumerator ResetRotationWhenClose()
    {
        // Wait until the agent is close to the original position
        while (!navMeshAgent.pathPending && navMeshAgent.remainingDistance > navMeshAgent.stoppingDistance)
        {
            yield return null;
        }

        
        // Reset rotation
        transform.rotation = originalRotation;
    }
}
