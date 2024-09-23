using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThrowingTutorialDialogue : MonoBehaviour
{
    [SerializeField] Dialogue dialogue;

    InteractableObject interactableObject;
    bool hasInteracted = false;

    // Start is called before the first frame update
    void Start()
    {
        interactableObject = GetComponent<InteractableObject>();    
    }

    // Update is called once per frame
    void Update()
    {
        if (interactableObject.pickedUpTimes == 2 && hasInteracted == false)
        {
            hasInteracted = true;
            DialogueManager.Instance.StartDialogue(dialogue);
        }
    }
}