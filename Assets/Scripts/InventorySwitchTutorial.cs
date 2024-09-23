using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventorySwitchTutorial : MonoBehaviour
{
    [SerializeField] Dialogue dialogue;

    Inventory inventory;
    bool hasInteracted = false;

    // Start is called before the first frame update
    void Start()
    {
        inventory = GetComponent<Inventory>();
    }

    // Update is called once per frame
    void Update()
    {
        if (hasInteracted == false && inventory.CountTotalItems() >= 2)
        {
            hasInteracted = true;
            DialogueManager.Instance.StartDialogue(dialogue);
        }
    }
}