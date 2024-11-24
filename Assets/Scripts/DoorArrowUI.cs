using UnityEngine;

public class DoorArrowUI : MonoBehaviour
{
    public RectTransform arrowUI; // UI element representing the arrow
    public Transform doorTransform; // Door's Transform in the world
    public Transform playerTransform; // Player's Transform
    public float proximityThreshold = 3f; // Distance threshold to turn off the arrow

    void Update()
    {
        if (EscalatorManager.Instance != null && doorTransform != null && playerTransform != null)
        {
            // Check if the door is open
            if (EscalatorManager.Instance.doorOpen == false)
            {
                arrowUI.gameObject.SetActive(false);
                return;
            }

            // Calculate distance between player and door
            float distanceToDoor = Vector3.Distance(playerTransform.position, doorTransform.position);
            if (distanceToDoor <= proximityThreshold)
            {
                arrowUI.gameObject.SetActive(false);
                return;
            }
            else
            {
                arrowUI.gameObject.SetActive(true);
            }

            // Calculate direction from player to door
            Vector3 directionToDoor = doorTransform.position - playerTransform.position;
            directionToDoor.y = 0; // Ensure calculation is on a flat plane if needed

            // Calculate angle between forward vector and direction to the door
            float angle = Mathf.Atan2(directionToDoor.x, directionToDoor.z) * Mathf.Rad2Deg;

            // Rotate the arrow UI to point towards the door
            arrowUI.localRotation = Quaternion.Euler(0, 0, -angle);

            arrowUI.gameObject.SetActive(true); // Ensure the arrow is visible
        }
    }
}
