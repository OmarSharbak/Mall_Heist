using UnityEngine;

public class DoorArrowUI : MonoBehaviour
{
    public RectTransform arrowUI; // UI element representing the arrow
    public Transform doorTransform; // Door's Transform in the world
    private Transform playerTransform; // Player's Transform
    public float proximityThreshold = 3f; // Distance threshold to turn off the arrow

	private void HandleLocalPlayerStarted(ThirdPersonController localPlayer)
	{


		playerTransform = localPlayer.transform;

	}

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
        else if (EscalatorManager.Instance == null)
        {
            Debug.LogWarning("DoorArrow - EscalatorManager null");
        }

		else if (doorTransform == null)
		{
			Debug.LogWarning("DoorArrow - door null");
		}

		else if (playerTransform == null)
		{
			Debug.LogWarning("DoorArrow - player transform null");
		}
	}

	private void OnEnable()
	{
		// Subscribe to the event
		ThirdPersonController.OnLocalPlayerStarted += HandleLocalPlayerStarted;
	}

	private void OnDisable()
	{
		// Unsubscribe from the event to avoid memory leaks
		ThirdPersonController.OnLocalPlayerStarted -= HandleLocalPlayerStarted;
	}
}
