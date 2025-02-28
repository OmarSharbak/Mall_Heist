using System;
using UnityEngine;

public class AimLine: MonoBehaviour
{
	public LineRenderer lineRenderer; // Reference to the LineRenderer
	private Transform playerTransform; // Reference to the player’s transform
	public float detectionRadius = 15f;
	public string[] layers = new string[] { "Guards" };
	public float aimAssistAngle = 35f;

	private Inventory inventory;

	private bool tutorialActive = false;

	public float parabolaHeight = 10f; // Height the parabola reaches above the starting point
	public int pointCount = 30; // Number of points to display the parabola

	InputPromptUIManager promptUIManager;


	void OnEnable()
	{
		ThirdPersonController.OnLocalPlayerStarted += StartLocal;
		PlayerDamageHandler.OnPlayerCaught += PlayerCaught;
		PlayerDamageHandler.OnGuardBribed += GuardBribed;

	}

	private void PlayerCaught()
	{
		tutorialActive = false;
		lineRenderer.enabled = false;
		promptUIManager.HideWestButtonUI();

	}

	void OnDisable()
	{
		ThirdPersonController.OnLocalPlayerStarted -= StartLocal;
		PlayerDamageHandler.OnPlayerCaught -= PlayerCaught;
		PlayerDamageHandler.OnGuardBribed -= GuardBribed;		
	}

	private void GuardBribed()
	{
		tutorialActive = true;
	}

	private void StartLocal(ThirdPersonController controller)
	{
		playerTransform = controller.transform;

		if (lineRenderer == null)
		{
			lineRenderer = GetComponent<LineRenderer>(); // Find LineRenderer if not set
		}
		inventory = GetComponent<Inventory>();

		if (EscalatorManager.Instance.levelName == "Level1")
		{
			tutorialActive = true;
		}

		promptUIManager = GameObject.Find("InteractionPrompts").GetComponent<InputPromptUIManager>();

	}

	Vector3 AimAssist(Vector3 itemPosition)
	{
		LayerMask hitLayer = LayerMask.GetMask(layers); // Adjust the layer mask as needed

		// Get all colliders in the detection range
		Collider[] hits = Physics.OverlapSphere(itemPosition, detectionRadius, hitLayer);
		Transform closestTarget = null;
		float closestAngle = Mathf.Infinity;

		// Look for the closest guard within the aim assist angle
		foreach (var hit in hits)
		{
			if (hit.transform == transform) continue; // Skip self
			Vector3 directionToTarget = (hit.transform.position - itemPosition).normalized;
			float angle = Vector3.Angle(playerTransform.forward, directionToTarget);

			if (angle < aimAssistAngle && angle < closestAngle)
			{
				closestAngle = angle;
				closestTarget = hit.transform;
				Debug.Log("Assisted");
			}
		}

		if (closestTarget != null)
		{
			// Show the parabolic trajectory from the hand to the guard
			DrawParabolicPath(itemPosition, closestTarget.position);

			return (closestTarget.position - itemPosition).normalized;
		}

		// If no target found, hide the LineRenderer
		lineRenderer.enabled = false;
		promptUIManager.HideWestButtonUI();
		return Vector3.zero; // Return zero vector if no aim assist is applied
	}

	// Function to calculate and draw the parabolic path
	void DrawParabolicPath(Vector3 startPosition, Vector3 targetPosition)
	{
		float distance = Vector3.Distance(startPosition, targetPosition);
		int pointCount = Mathf.Max(30, Mathf.CeilToInt(distance / 0.1f)); // Ensure we have enough points for the distance

		// Set up the LineRenderer
		lineRenderer.positionCount = pointCount;

		// Calculate the parabolic path points
		for (int i = 0; i < pointCount; i++)
		{
			float t = i / (float)(pointCount - 1); // Time step from 0 to 1
			Vector3 point = CalculateParabolicPoint(startPosition, targetPosition, t);
			lineRenderer.SetPosition(i, point);
		}

		lineRenderer.enabled = true; // Enable the LineRenderer to show the path
		promptUIManager.ShowWestButtonUI();

	}

	// Function to calculate a point on the parabolic trajectory
	Vector3 CalculateParabolicPoint(Vector3 startPosition, Vector3 targetPosition, float t)
	{
		// Calculate the horizontal direction and distance
		Vector3 direction = targetPosition - startPosition;
		float distance = direction.magnitude;
		Vector3 horizontalDirection = direction.normalized;

		// Calculate the midpoint height (to create the parabolic curve)
		Vector3 midpoint = startPosition + horizontalDirection * (distance * 0.5f);

		// Set the peak of the parabola to be `parabolaHeight` above the start position
		float heightAtMidpoint = parabolaHeight;

		// Interpolate the vertical position
		float height = Mathf.Lerp(startPosition.y, targetPosition.y, t);
		height += Mathf.Sin(Mathf.PI * t) * heightAtMidpoint; // Create the upward curve

		// Calculate the horizontal position
		Vector3 horizontalPosition = startPosition + horizontalDirection * (distance * t);

		// Combine horizontal and vertical positions
		return new Vector3(horizontalPosition.x, height, horizontalPosition.z);
	}
	private void Update()
	{
		if (tutorialActive)
			if (inventory != null)
				if(inventory.heldItem == null)
				{
					promptUIManager.HideWestButtonUI();
					lineRenderer.enabled = false;

				}
				else 
					AimAssist(inventory.heldItem.transform.position);
	}
}
