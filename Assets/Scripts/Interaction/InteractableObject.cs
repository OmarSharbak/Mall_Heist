using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EPOOutline;

public class InteractableObject : MonoBehaviour
{
    public int pickedUpTimes = 3;

    public Outlinable outlinable;

    public virtual void Interact(PlayerInteractionController playerInteractionController)
    {
        // This method is intended to be overridden by subclasses.
        Debug.Log("Interacting with " + gameObject.name);
    }
}
