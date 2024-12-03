using Mirror;
using System.Linq;
using TMPro;
using UnityEngine;

public class Vault : NetworkBehaviour
{
    [SerializeField] Inventory playerInventory; // Assign this from the inspector

    Animator animator; // Assign this from the inspector

    [SyncVar]
    private bool isOpened = false;

    private void Start()
    {
        animator = GetComponent<Animator>();
    }

    public void Update()
    {
        if (!isOpened)
        {
            OpenVault();
        }
    }
    public void OpenVault()
    {
        CmdCheckServerHasPassword();
    }

    [Command]
    public void CmdCheckServerHasPassword()
    {
        string objectiveItem = "Password";
        // Check if the item exists and has a count greater than 0
        bool hasItem = GameManager.Instance.inventory.Any(item => item.itemName == objectiveItem && item.quantity > 0);

        if (hasItem)
            // Send the result back to the client that made the request
            RpcSendHasPasswordResult();
    }
    // TargetRpc to send the result back to the client
    [ClientRpc]
    private void RpcSendHasPasswordResult()
    {
        isOpened = true;
        animator.SetTrigger("Open");
    }
}
