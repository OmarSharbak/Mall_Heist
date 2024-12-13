using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using EmeraldAI;
using EPOOutline;
using MoreMountains.Feedbacks;
using System;

public class TrapItem : InventoryItem
{
    public bool isPlaceable = true;
    public int damageAmount = 2; // Damage inflicted by this trap
    public GameObject stunningFXPrefab; // Visual effect shown when AI is stunned
    public AudioClip guardStunClip; // Audio clip to play when the guard is stunned
    public AudioClip guardHitClip;

    public bool isPlaced = false; // Flag to check if the trap is placed down
    private bool hit = false;

    Quaternion initialPlayerRotation;          // Store initial rotation of AI for animation purposes

    // References to various AI components for interactions and animations
    EmeraldAISystem aiSystem;
    EmeraldAIEventsManager eventsManager;
    Animator aiAnimator;
    NavMeshAgent aiNavMeshAgent;
    GuardInvincibility guardInvincibility;
    GuardOnCollision guardCollision;
    BoxCollider boxCollider;
    AudioSource aiAudioSource;
    Rigidbody aiRigidbody;

    Outlinable outlinable;
    private MMFeedbacks mmFeedbacks;

    void Start()
    {
        outlinable = GetComponent<Outlinable>();
        mmFeedbacks = GameObject.Find("MMFeedbacks(hit)").GetComponent<MMFeedbacks>();
    }

    void Update()
    {
        // base.Update(); // Call the base class Update method

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

    public static event Action OnHitBanana;

    Transform healthBarCanvasTransform;
    void OnTriggerEnter(Collider other)
    {

        // Skip processing if item isn't throwable or has already hit its target
        if (!isPlaceable || hit || !isPlaced) 
            return;

        // Check if collided object is an EmeraldAI system (i.e., AI opponent)
        if (other.gameObject.GetComponent<EmeraldAISystem>() != null)
        {
			// Check for invincibility status on other target
			guardInvincibility = other.gameObject.GetComponent<GuardInvincibility>();

			if (guardInvincibility != null && guardInvincibility.IsInvincible())
			{
				return; // Skip processing for invincible targets
			}

			if (mmFeedbacks != null)
            {
                Debug.Log("Feedbacks Played.");
                mmFeedbacks.PlayFeedbacks();
            }
            // Find the "AI Health Bar Canvas" within the "HealthBarParent" of the collided GameObject.
            healthBarCanvasTransform = other.transform.Find("HealthBarParent/AI Health Bar Canvas");

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

            hit = true;  // Flag hit to prevent repeat processing

            // Play the guard hit sound
            aiAudioSource = other.gameObject.GetComponent<AudioSource>();
            if (aiAudioSource != null && guardHitClip != null)
            {
                aiAudioSource.PlayOneShot(guardHitClip);
            }

            // Start a coroutine to prevent AI rotation during the hit animation
            StartCoroutine("HoldRotation");

            // Cache initial AI rotation for animation reset
            initialPlayerRotation = other.gameObject.transform.rotation;

            // Temporarily set AI to invincible during hit animation
            guardInvincibility.MakeInvincible();

            // Fetch necessary AI components for subsequent interactions
            aiSystem = other.gameObject.GetComponent<EmeraldAISystem>();
            eventsManager = other.gameObject.GetComponent<EmeraldAIEventsManager>();
            aiAnimator = other.gameObject.GetComponent<Animator>();
            aiNavMeshAgent = other.gameObject.GetComponent<NavMeshAgent>();
            boxCollider = other.gameObject.GetComponent<BoxCollider>();
            guardCollision = other.gameObject.GetComponent<GuardOnCollision>();
            aiRigidbody = other.gameObject.GetComponent<Rigidbody>();

            // Apply damage and update AI state
            guardCollision.canDetect = false;
            aiSystem.DetectionRadius = 0;
            eventsManager.ClearTarget();
            aiSystem.Damage(damageAmount, EmeraldAI.EmeraldAISystem.TargetType.Player, playerTransform, 1);

            outlinable.enabled = false;

            eventsManager.CancelAttackAnimation();

            // Invoke event 
            OnHitBanana?.Invoke();

            if (aiAnimator != null)
            {
                // Trigger hit animation for the AI
                aiAnimator.SetTrigger("Hit");
            }
            eventsManager.ClearTarget();
            // Debug code (can be omitted in production)
            AnimatorStateInfo stateInfo = aiAnimator.GetCurrentAnimatorStateInfo(0);
            if (stateInfo.IsName("Death_01(2)"))
            {
                Debug.Log("Hit animation is currently playing");
            }

            // Temporarily disable AI's collider
            boxCollider.enabled = false;

            // If other happens at head joint, trigger a stunning visual effect
            Transform headJoint = other.transform.Find("Hips_jnt/Spine_jnt/Spine_jnt 1/Chest_jnt/Neck_jnt/Head_jnt");
            if (headJoint != null)
            {
                StartCoroutine(SpawnEffectAfterDelay(headJoint));
            }

            // Move the guard and the banana
            StartCoroutine(MoveGuardAndBanana(other.transform));
            
            // After a set delay, reset AI state post-hit
            StartCoroutine(WaitAndMove());
        }
		else if (other.gameObject.GetComponent<ThirdPersonController>() != null)//collision other player
		{
			Debug.Log("Item - Player Hit");
			mmFeedbacks.PlayFeedbacks();
			hit = true;  // Flag hit to prevent repeat processing

			ThirdPersonController controller = other.gameObject.GetComponent<ThirdPersonController>();
			controller.canMove = false;

			controller.animator.SetTrigger("Hit");

			// If collision happens at head joint, trigger a stunning visual effect
			Transform headJoint = other.transform.Find("Geometry/SimplePeople_Pimp_White/Hips_jnt/Spine_jnt/Spine_jnt 1/Chest_jnt/Neck_jnt/Head_jnt");
			if (headJoint != null)
			{
				StartCoroutine(SpawnEffectAfterDelayPlayer(headJoint));
			}
			StartCoroutine(MoveGuardAndBanana(other.transform));


			// After a set delay, reset state post-hit
			StartCoroutine(WaitAndMovePlayer(controller));

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
            aiSystem.transform.rotation = initialPlayerRotation;
        }

        // Stop rotation hold coroutine and reset AI state
        StopCoroutine("HoldRotation");

        // Set AI back to a vulnerable state
        if (aiSystem != null && aiSystem.CurrentHealth > 0)
        {
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
        aiRigidbody = null;
    }

    // Coroutine to continuously hold AI's rotation and position for hit animation correctness
    IEnumerator HoldRotation()
    {
        while (true)
        {
            if (aiSystem != null) // If AI is still active
            {
                aiSystem.transform.rotation = initialPlayerRotation;
            }

            yield return null; // Pause until next frame
        }
    }

    // Coroutine to move the guard and the banana
    IEnumerator MoveGuardAndBanana(Transform guardTransform)
    {
        float duration = 0.5f;
        float elapsedTime = 0;
        Vector3 initialPosition = guardTransform.position;
        Vector3 targetPosition = initialPosition + guardTransform.forward * 2f; // Move the guard forward

        while (elapsedTime < duration)
        {
            guardTransform.position = Vector3.Lerp(initialPosition, targetPosition, elapsedTime / duration);
            transform.position = Vector3.Lerp(transform.position, transform.position + guardTransform.forward * 0.05f, elapsedTime / duration); // Move the banana forward
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        guardTransform.position = targetPosition;
        //transform.position = transform.position + guardTransform.forward * 2f; // Ensure the final position of the banana
    }
}
