using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EPOOutline;

public class InteractableDistructibleItem : InteractableObject
{
    AudioSource audioSource;
    void Start()
    {
        outlinable = GetComponent<Outlinable>();
        audioSource = GetComponent<AudioSource>();
    }

    // Overrides the Interact method in the base InteractableObject class.
    public override void Interact(PlayerInteractionController playerInteractionController)
    {
        playerInteractionController.InteractWith(this);
        if(audioSource != null)
        {
            audioSource.Play();
        }
        
        Destroy(this.gameObject);
    }
}