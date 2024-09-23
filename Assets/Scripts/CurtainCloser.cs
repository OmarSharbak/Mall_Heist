using UnityEngine;

public class CurtainCloser : MonoBehaviour
{
    private Animator animator;
    private BoxCollider boxCollider;
    private Vector3 colliderCenter;
    private bool isOpen = true; // Boolean to track the state of the curtain

    void Start()
    {
        // Get the Animator component attached to this GameObject
        animator = GetComponent<Animator>();

        // Get the BoxCollider component attached to this GameObject
        boxCollider = GetComponent<BoxCollider>();

        // Store the center position of the BoxCollider
        colliderCenter = boxCollider.center;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player") || other.CompareTag("PlayerInvisible"))
        {
            // Calculate the relative position of the other object to this object
            Vector3 relativePosition = transform.InverseTransformPoint(other.transform.position);

            // Check if the object entered from the front or back of the curtain
            if (relativePosition.z > colliderCenter.z)
            {
                // Trigger entered from the front, close the curtain if it's open
                if (isOpen)
                {
                    animator.SetBool("isOpen", false);
                    isOpen = false;
                    EscalatorManager.Instance.ClearTargetAll();
                    EscalatorManager.Instance.CheckExposed();
                    other.gameObject.tag = "PlayerInvisible";
                }
            }
            else
            {
                // Trigger entered from the back, open the curtain if it's closed
                if (!isOpen)
                {
                    animator.SetBool("isOpen", true);
                    isOpen = true;
                    other.gameObject.tag = "Player";

                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") || other.CompareTag("PlayerInvisible"))
        {
            // Calculate the relative position of the other object to this object
            Vector3 relativePosition = transform.InverseTransformPoint(other.transform.position);

            // Check if the object exited towards the outside of the changing room
            if (relativePosition.z > colliderCenter.z)
            {
                // Open the curtain and make the player visible if they exit the changing room
                animator.SetBool("isOpen", true);
                isOpen = true;
                other.gameObject.tag = "Player";
            }
        }
    }

}
