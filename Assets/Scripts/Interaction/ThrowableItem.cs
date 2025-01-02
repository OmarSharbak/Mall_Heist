using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using EmeraldAI;
using MoreMountains.Feedbacks;
using System;
using Mirror;

public class ThrowableItem : InventoryItem
{
	public bool isThrowable;                   // Specifies whether the item can be thrown by the player
	public int damageAmount = 2;               // Damage inflicted by this item
	public GameObject stunningFXPrefab;        // Visual effect shown when AI is stunned
	MMFeedbacks mmFeedbacks;

	[SyncVar]
	private bool hit = false;                  // State flag indicating if the item has struck its target

	public AudioClip guardHitClip;
	public AudioClip guardStunClip;

	Quaternion initialAIRotation;          // Store initial rotation of AI for animation purposes

	// References to various AI components for interactions and animations
	EmeraldAISystem aiSystem;
	EmeraldAIEventsManager eventsManager;
	Animator aiAnimator;
	NavMeshAgent aiNavMeshAgent;
	GuardInvincibility guardInvincibility;
	GuardOnCollision guardCollision;
	BoxCollider boxCollider;
	AudioSource aiAudioSource;
	Rigidbody rb;

	[SerializeField] float noiseRadius = 7f; // Radius of the noise
	[SerializeField] GameObject noiseIndicatorPrefab; // Prefab of the noise indicator (Sphere or Quad)
	MeshRenderer noiseIndicatorRenderer;

	private GameObject noiseIndicator; // Instance of the noise indicator

	[SyncVar]
	bool noiseShown = false;

	public static event Action OnGuardHit;

	private void ShowNoiseRadius()
	{
		noiseIndicator.SetActive(true);
		noiseIndicator.transform.position = transform.position;
		noiseShown = true;
	}

	private void HideNoiseRadius()
	{
		noiseIndicator.SetActive(false);
	}

	void Start()
	{
		GameObject mmFeedbacksGO = GameObject.Find("MMFeedbacks(hit)");
		if (mmFeedbacksGO)
		{
			mmFeedbacks = mmFeedbacksGO.GetComponent<MMFeedbacks>();
		}

		rb = GetComponent<Rigidbody>();

		if (noiseIndicatorPrefab != null)
		{
			// Instantiate and initialize the noise indicator
			noiseIndicator = Instantiate(noiseIndicatorPrefab, transform.position, Quaternion.identity);
			noiseIndicator.transform.localScale = new Vector3(noiseRadius * 2, noiseRadius * 2, noiseRadius * 2);
			// Set the rotation of the noise indicator to x: 90, y: 0, z: 0
			noiseIndicator.transform.rotation = Quaternion.Euler(90, 0, 0);
			noiseIndicatorRenderer = noiseIndicator.GetComponent<MeshRenderer>();

			noiseIndicator.SetActive(false);
		}
	}

	void Update()
	{
		//base.Update(); // Call the base class Update method

		// ThrowableItem specific Update logic here

		// Pause AI movement for hit animations if required components are available
		if (aiSystem != null && eventsManager != null)
		{
			if (aiSystem.CurrentHealth > 0)
				eventsManager.StopMovement();

			if (aiAnimator != null)
			{
				aiAnimator.SetFloat("Direction", 0f);
				aiAnimator.SetFloat("Speed", 0f);
			}

			if (healthBarCanvasTransform != null)
			{
				healthBarCanvasTransform.gameObject.SetActive(true);
				healthBarCanvasTransform.gameObject.GetComponent<Canvas>().enabled = true;
			}
		}


	}

	Transform healthBarCanvasTransform;
	[SerializeField] bool isContainer = false;
	void OnCollisionEnter(Collision collision)
	{
		if (collision.gameObject != null)
		{

			Debug.Log("ITEM collision enter!");
			// Your existing OnCollisionEnter code here
			if (noiseIndicator != null && !isContainer && noiseShown == false)
			{
				CmdShowNoiseRadius();
			}

			bool isFloor = (collision.gameObject.name == "Floor");
			if (isFloor)
				CmdSetHit();
			// Skip processing if item isn't throwable or has already hit its target
			if (!isThrowable || isFloor || (rb != null && rb.velocity.magnitude < 0.25f))
			{
				Debug.Log("ITEM collision returned hit:");
				return;

			}
			var _netIdentity = collision.gameObject.GetComponent<NetworkIdentity>();

			CmdCustomCollisionEnter(_netIdentity);


		}
	}

	[Command(requiresAuthority = false)]
	private void CmdSetHit()
	{
		hit = true;

	}
	[Command(requiresAuthority = false)]
	private void CmdCustomCollisionEnter(NetworkIdentity _netIdentity)
	{
		Debug.Log("ITEM collision enter passed ");

		RpcCustomCollisionEnter(_netIdentity);
	}
	[Command(requiresAuthority = false)]
	private void CmdShowNoiseRadius()
	{
		//Debug.Log("SERVER - collision - noise show");

		noiseShown = true;
		RpcShowNoiseRadius();
	}

	[ClientRpc]
	private void RpcShowNoiseRadius()
	{
		Debug.Log("CLIENT - collision - noise show");

		ShowNoiseRadius();
		StartCoroutine(RemoveNoiseIndicator());
	}


	[ClientRpc]
	private void RpcCustomCollisionEnter(NetworkIdentity netIdentity)
	{
		Debug.Log("CLIENT - Item - collision enter - rpc called");

		if (netIdentity != null)
		{
			Debug.Log("CLIENT - Item - collision enter:" + netIdentity.gameObject.name);


			// Check if collided object is an EmeraldAI system (i.e., AI opponent)
			if (netIdentity.gameObject.GetComponent<EmeraldAISystem>() != null)
			{
				Debug.Log("ITEM collision guard hit");

				// Check for invincibility status on netIdentity target
				guardInvincibility = netIdentity.gameObject.GetComponent<GuardInvincibility>();
				if (guardInvincibility != null && guardInvincibility.IsInvincible())
				{
					return; // Skip processing for invincible targets
				}

				mmFeedbacks.PlayFeedbacks();
				// Find the "AI Health Bar Canvas" within the "HealthBarParent" of the collided GameObject.
				healthBarCanvasTransform = netIdentity.transform.Find("HealthBarParent/AI Health Bar Canvas");

				// Check if the "AI Health Bar Canvas" was found.
				if (healthBarCanvasTransform != null)
				{
					// Ensure the health bar canvas GameObject is active.
					healthBarCanvasTransform.gameObject.SetActive(true);
				}
				else
				{
					// Log an error if the "AI Health Bar Canvas" was not found.
					Debug.LogError("AI Health Bar Canvas not found on the collided GameObject.");
				}

				CmdSetHit();  // Flag hit to prevent repeat processing

				// Play the guard hit sound
				aiAudioSource = netIdentity.gameObject.GetComponent<AudioSource>();
				if (aiAudioSource != null && guardHitClip != null)
				{
					aiAudioSource.PlayOneShot(guardHitClip);
				}

				// Start a coroutine to prevent AI rotation during the hit animation
				StartCoroutine("HoldRotation");

				// Cache initial AI rotation for animation reset
				initialAIRotation = netIdentity.gameObject.transform.rotation;

				// Temporarily set AI to invincible during hit animation
				guardInvincibility.MakeInvincible();

				// Fetch necessary AI components for subsequent interactions
				aiSystem = netIdentity.gameObject.GetComponent<EmeraldAISystem>();
				eventsManager = netIdentity.gameObject.GetComponent<EmeraldAIEventsManager>();
				aiAnimator = netIdentity.gameObject.GetComponent<Animator>();
				aiNavMeshAgent = netIdentity.gameObject.GetComponent<NavMeshAgent>();
				boxCollider = netIdentity.gameObject.GetComponent<BoxCollider>();
				guardCollision = netIdentity.gameObject.GetComponent<GuardOnCollision>();

				// Apply damage and update AI state
				guardCollision.canDetect = false;
				aiSystem.DetectionRadius = 0;
				eventsManager.ClearTarget();
				aiSystem.Damage(damageAmount, EmeraldAI.EmeraldAISystem.TargetType.Player, playerTransform, 1);

				OnGuardHit?.Invoke();

				eventsManager.CancelAttackAnimation();

				if (aiAnimator != null)
				{
					// Trigger hit animation for the AI
					aiAnimator.SetTrigger("Hit");
				}

				// Debug code (can be omitted in production)
				AnimatorStateInfo stateInfo = aiAnimator.GetCurrentAnimatorStateInfo(0);
				if (stateInfo.IsName("Death_01(2)"))
				{
					Debug.Log("Hit animation is currently playing");
				}
				else
				{
					aiAnimator.SetTrigger("Hit");
				}

				// Temporarily disable AI's collider
				boxCollider.enabled = false;

				// If collision happens at head joint, trigger a stunning visual effect
				Transform headJoint = netIdentity.transform.Find("Hips_jnt/Spine_jnt/Spine_jnt 1/Chest_jnt/Neck_jnt/Head_jnt");
				if (headJoint != null)
				{
					StartCoroutine(SpawnEffectAfterDelay(headJoint));
				}

				// After a set delay, reset AI state post-hit
				StartCoroutine(WaitAndMove());
			}
			else if (netIdentity.gameObject.GetComponent<ThirdPersonController>() != null)//collision other player
			{
				Debug.Log("Item - Player Hit");
				mmFeedbacks.PlayFeedbacks();
				CmdSetHit();  // Flag hit to prevent repeat processing

				ThirdPersonController controller = netIdentity.gameObject.GetComponent<ThirdPersonController>();
				controller.canMove = false;

				controller.animator.SetTrigger("Hit");

				// If collision happens at head joint, trigger a stunning visual effect
				Transform headJoint = netIdentity.transform.Find("Geometry/SimplePeople_Pimp_White/Hips_jnt/Spine_jnt/Spine_jnt 1/Chest_jnt/Neck_jnt/Head_jnt");
				if (headJoint != null)
				{
					StartCoroutine(SpawnEffectAfterDelayPlayer(headJoint));
				}

				// After a set delay, reset state post-hit
				StartCoroutine(WaitAndMovePlayer(controller));

			}


		}
		else
		{
			Debug.Log("Item Collision Enter: Net Identity null!!!");
		}
	}

	// Coroutine to delay the spawning of a visual effect upon collision
	IEnumerator SpawnEffectAfterDelay(Transform headJoint)
	{
		yield return new WaitForSeconds(1.7f);

		if (headJoint != null)
		{
			if (aiSystem.CurrentHealth > 0)
			{
				GameObject spawnedFX = Instantiate(stunningFXPrefab, headJoint.position + new Vector3(0, 1.5f, 0), Quaternion.Euler(-90, 0, 0));

				yield return new WaitForSeconds(0.2f);  // Delay for SFX

				if (aiAudioSource != null && guardStunClip != null)
				{
					aiAudioSource.PlayOneShot(guardStunClip);
				}
				Destroy(spawnedFX, 4.6f); // Clean up the effect after its duration
			}
		}
	}

	// Coroutine to delay the spawning of a visual effect upon collision
	IEnumerator SpawnEffectAfterDelayPlayer(Transform headJoint)
	{
		yield return new WaitForSeconds(1.7f);

		if (headJoint != null)
		{

			GameObject spawnedFX = Instantiate(stunningFXPrefab, headJoint.position + new Vector3(0, 1.5f, 0), Quaternion.Euler(-90, 0, 0));

			yield return new WaitForSeconds(0.2f);  // Delay for SFX

			Destroy(spawnedFX, 4.6f); // Clean up the effect after its duration

		}
	}

	IEnumerator RemoveNoiseIndicator()
	{
		float startTime = Time.time;  // Store the start time
		float duration = 2f;  // Set the desired duration

		while (Time.time - startTime < duration)  // Keep running while the duration is not exceeded
		{
			// Pulsating effect
			float noiseLevel = Mathf.PingPong(Time.time, 0.3f);  // Adjust values based on your needs

			// Get the original color of the material
			Color originalColor = noiseIndicatorRenderer.material.color;

			// Set the color of the material, keeping the original RGB values and only modifying the alpha
			noiseIndicatorRenderer.material.SetColor("_BaseColor", new Color(originalColor.r, originalColor.g, originalColor.b, noiseLevel));

			yield return null;  // Wait for the next frame
		}

		HideNoiseRadius();  // Hide the noise radius after the loop ends
	}

	// Coroutine to reset AI state after being hit
	IEnumerator WaitAndMovePlayer(ThirdPersonController controller)
	{
		yield return new WaitForSeconds(7.7f);
		controller.canMove = true;

	}


	// Coroutine to reset AI state after being hit
	IEnumerator WaitAndMove()
	{
		eventsManager.ClearTarget();
		yield return new WaitForSeconds(7.7f);

		// Reset AI rotation and movement post-animation
		if (aiSystem != null)
		{
			aiSystem.transform.rotation = initialAIRotation;
		}

		// Stop rotation hold coroutine and reset AI state
		StopCoroutine("HoldRotation");



		// Set AI back to a vulnerable state
		if (aiSystem != null && aiSystem.CurrentHealth > 0)
		{
			eventsManager.ClearTarget();
			aiSystem.DetectionRadius = 8;
			eventsManager.ResumeMovement();
			aiSystem.GetComponent<GuardInvincibility>().MakeVulnerable();

			// Re-enable AI's collider
			boxCollider.enabled = true;

			guardCollision.canDetect = true;
		}


		// Release references to free memory and prevent unintended actions
		aiSystem = null;
		eventsManager = null;
		aiAnimator = null;
		aiNavMeshAgent = null;
		guardInvincibility = null;
		boxCollider = null;
		aiAudioSource = null;
		guardCollision = null;
		healthBarCanvasTransform = null;
	}

	// Coroutine to continuously hold AI's rotation and position for hit animation correctness
	IEnumerator HoldRotation()
	{
		while (true)
		{
			if (aiSystem != null) // If AI is still active
			{
				aiSystem.transform.rotation = initialAIRotation;
			}

			yield return null; // Pause until next frame
		}
	}

	[Command]
	public void CmdDeatach()
	{
		transform.SetParent(null); // Detach it from the player's hand

		// Enable Rigidbody for physics-based throwing
		Rigidbody rb = GetComponent<Rigidbody>();
		if (rb != null)
		{
			rb.isKinematic = false;
		}

		// Update all clients
		RpcDeatach(GetComponent<NetworkIdentity>().netId);
	}
	[ClientRpc]
	void RpcDeatach(uint _netId)
	{
		if (NetworkClient.spawned.TryGetValue(_netId, out var item))
		{
			Transform itemTransform = item.transform;
			itemTransform.SetParent(null); // Detach it on all clients

			// Enable physics locally
			Rigidbody rb = itemTransform.GetComponent<Rigidbody>();
			if (rb != null)
			{
				rb.isKinematic = false;
			}
			Collider cd = itemTransform.GetComponent<Collider>();
			if (cd != null)
				cd.isTrigger = true;
		}
	}

}
