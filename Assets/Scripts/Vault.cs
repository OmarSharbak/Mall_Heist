using Mirror;
using System;
using System.Linq;
using TMPro;
using UnityEngine;

public class Vault : NetworkBehaviour
{
    [SerializeField] Inventory playerInventory; // Assign this from the inspector

    Animator animator; // Assign this from the inspector

    [SyncVar(hook =nameof(OnVaultOpened))]
    private bool isOpened = false;

	private void OnVaultOpened(bool prevValue,bool newValue)
	{
        if(newValue)
		    animator.SetTrigger("Open");
	}

	private void Start()
    {
        animator = GetComponent<Animator>();
    }

    [ServerCallback]
    public void Update()
    {
        CheckServerHasPassword();
    }

    [ServerCallback]
    public void CheckServerHasPassword()
    {
        string objectiveItem = "Password";
        // Check if the item exists and has a count greater than 0
        bool hasItem = GameManager.Instance.inventory.Any(item => item.itemName == objectiveItem && item.quantity > 0);

        if (hasItem)
			// Send the result back to the client that made the request
			isOpened = true;
	}
}
