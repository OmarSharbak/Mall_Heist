using Mirror;
using UnityEngine;

public class Objective : NetworkBehaviour
{
	public bool startActive = false;
	public Color startColor;

	private Transform player;            // Reference to the player transform
	public GameObject arrowPrefab;      // Prefab of the UI arrow
	public GameObject vfxPrefab;
	public Vector3 vfxOffset;
	private GameObject arrowInstance;   // Instance of the UI arrow
	private GameObject vfxInstance;
	public Camera mainCamera;           // Reference to the main camera
	private float distanceFromPlayer = 3f;  // Threshold distance to deactivate the arrow
	private bool isObjectiveActive = false;
	private MeshRenderer arrowRenderer;
	private float nearDistanceThreshold = 5f;  // Threshold distance to deactivate the arrow

	[HideInInspector]
	public bool isComplete=false;


	void OnEnable()
	{
		ThirdPersonController.OnLocalPlayerStarted+=StartLocal;
	}
	void OnDisable()
	{
		ThirdPersonController.OnLocalPlayerStarted -= StartLocal;
	}
	private void StartLocal(ThirdPersonController controller)
	{
		// Instantiate the arrow prefab and set it as a child of the canvas
		arrowInstance = Instantiate(arrowPrefab, transform);
		arrowInstance?.SetActive(false);
		arrowRenderer = arrowInstance?.GetComponent<MeshRenderer>();


		vfxInstance = Instantiate(vfxPrefab, transform);
		vfxInstance?.SetActive(false);
		vfxInstance.transform.position += vfxOffset;

		// Get reference to the main camera if not assigned
		if (mainCamera == null)
		{
			mainCamera = Camera.main;
		}


		player = controller.transform;

		Debug.Log("Objective: " + mainCamera + " " + player);

		setActiveGameObjects(startActive);

	}

	public void CompleteObjective()
	{
		isComplete = true;

		Destroy(arrowInstance);
		Destroy(vfxInstance);


	}

	public void setActiveGameObjects(bool value)
	{
		if (isComplete)
			return;

		arrowInstance.SetActive(value);
		vfxInstance?.SetActive(value);

		arrowRenderer.material = new Material(arrowRenderer.material);
		arrowRenderer.material.SetColor("_Color", startColor); // Change the color dynamically

		// Get all ParticleSystem components in this GameObject
		ParticleSystem[] particleSystems = vfxInstance.GetComponentsInChildren<ParticleSystem>();

		// Loop through each particle system
		foreach (ParticleSystem ps in particleSystems)
		{
			// Get the main module of the particle system
			var main = ps.main;

			// Change the start color of the particle syste
			main.startColor = new Color(startColor.r,startColor.g,startColor.b,0.15f);
		}

		isObjectiveActive = value;



	}

	void Update()
	{
		if (isObjectiveActive)
		{

			if (player != null && arrowInstance != null)
			{

				// Calculate the distance between the camera and the objective
				float distanceToObjective = Vector3.Distance(player.transform.position, transform.position);

				// If the camera is too near, deactivate the arrow; otherwise, activate it
				if (distanceToObjective < nearDistanceThreshold)
				{
					arrowInstance.SetActive(false);
					return; // Skip further processing if the arrow is inactive
				}
				else
				{
					arrowInstance.SetActive(true);
				}

				Vector3 playerPos = player.position;
				playerPos.y += 1.75f;
				// Calculate direction from player to the target
				Vector3 directionToObjective = (transform.position - playerPos).normalized;

				// Set the arrow's position 10 units away from the player in the direction of the target
				arrowInstance.transform.position = playerPos + directionToObjective * distanceFromPlayer;

				// Make the arrow point toward the target
				arrowInstance.transform.LookAt(transform);

				// Rotate the arrow 90 degrees around the Y-axis
				arrowInstance.transform.rotation *= Quaternion.Euler(90, 0, 0);
			}
		}
	}
}