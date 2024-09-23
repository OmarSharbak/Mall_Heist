using UnityEngine;

public class Vault : MonoBehaviour
{
    [SerializeField] Inventory playerInventory; // Assign this from the inspector
    
    Animator animator; // Assign this from the inspector

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
        if (playerInventory.HasObjectiveItem("Password"))
        {
            isOpened = true;
            animator.SetTrigger("Open");
        }
    }
}
