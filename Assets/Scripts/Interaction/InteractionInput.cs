using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractionInput: MonoBehaviour
{
    public float interactionRange = 1.0f;  // The range within which the player can interact with objects.
    public LayerMask interactableLayer;  // The layer on which interactable objects reside.

    /*void Update()
    {
        // Check for the interaction input.
        if (Input.GetKeyDown(KeyCode.E))  // Replace with your actual interaction input.
        {
            // Cast a ray forward from the player's position.
            if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, interactionRange, interactableLayer))
            {
                // Check if the ray hit an interactable object.
                InteractableObject interactableObject = hit.transform.GetComponent<InteractableObject>();
                if (interactableObject != null)
                {
                    // Interact with the object.
                    interactableObject.Interact(GetComponent<PlayerInteractionController>());
                }
            }
        }
    }*/
}

