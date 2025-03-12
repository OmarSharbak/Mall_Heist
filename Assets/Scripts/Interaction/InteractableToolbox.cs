using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractableToolbox : InteractableObject
{
    // Overrides the Interact method in the base InteractableObject class.
    public override void Interact(PlayerInteractionController playerInteractionController)
    {
        // Get the PlayerInteractionController from the player and call InteractWith, passing this toolbox.
        //PlayerInteractionController player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerInteractionController>();
        playerInteractionController.InteractWith(this);

        //Debug.Log("Picking up wrench.");
    }
}
