using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlassInteractableThrowItem : InteractableThrowableItem
{
    public override void Interact(PlayerInteractionController playerInteractionController)
    {
        base.Interact(playerInteractionController);
        transform.parent.gameObject.SetActive(false);
    }
}
