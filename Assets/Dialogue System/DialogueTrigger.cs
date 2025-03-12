using UnityEngine;
using System.Collections;

public class DialogueTrigger : MonoBehaviour
{

    [SerializeField] Dialogue dialogue;
    private bool hasInteracted = false; // Flag to check if interaction has already occurred

    private void Start()
    {
        hasInteracted = false;
    }
    void Interact()
    {
        if (hasInteracted) return; // Exit if already interacted

        TriggerDialogue();
        hasInteracted = true; // Set the flag after interaction
    }

    void TriggerDialogue()
    {
        DialogueManager.Instance.StartDialogue(dialogue);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            Interact();
        }
    }
}