using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EPOOutline;


public class InteractableThrowableItem : InteractableObject
{


    void Start()
    {
        outlinable = GetComponent<Outlinable>();
    }

    // Overrides the Interact method in the base InteractableObject class.
    public override void Interact(PlayerInteractionController playerInteractionController)
    {
        playerInteractionController.InteractWith(this);
    }
}