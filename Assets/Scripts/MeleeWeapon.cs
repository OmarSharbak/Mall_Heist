using UnityEngine;
using EmeraldAI;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.AI;
using MoreMountains.Feedbacks;
using System;

public class MeleeWeapon : InventoryItem
{
    public bool isThrowable = false;
    public int damageAmount = 4; // Damage inflicted by this weapon
    public AudioClip guardHitClip;
    public AudioClip guardStunClip;
    private AudioSource audioSource;
    public GameObject stunningFXPrefab;        // Visual effect shown when AI is stunned
    MMFeedbacks mmFeedbacks;

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
    Rigidbody AIRb;

    public bool isMelee = true;
    public bool canDamage = false;
    bool hit = false;

    public static event Action OnHitGuitar;

    public void SetCanDamage(bool damage)
    {
        canDamage = damage;
    }
    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        mmFeedbacks = GameObject.Find("MMFeedbacks(hit)").GetComponent<MMFeedbacks>();
    }

    private void Update()
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

    Transform FindTopParent(Transform currentTransform)
    {
        while (currentTransform.parent != null)
        {
            currentTransform = currentTransform.parent;
        }
        return currentTransform;
    }

    Transform healthBarCanvasTransform;
    void OnCollisionEnter(Collision collision)
    {
        // Check for invincibility status on collision target
        guardInvincibility = collision.gameObject.GetComponent<GuardInvincibility>();
        if (guardInvincibility != null && guardInvincibility.IsInvincible())
        {
            return; // Skip processing for invincible targets
        }

        if(canDamage == false || hit)
        {
            return;
        }

        if (collision.gameObject.GetComponent<EmeraldAISystem>() != null)
        {

            if(mmFeedbacks != null)
            {
                Debug.Log("Feedbacks Played.");
                mmFeedbacks.PlayFeedbacks();
            }

            // Find the "AI Health Bar Canvas" within the "HealthBarParent" of the collided GameObject.
            healthBarCanvasTransform = collision.transform.Find("HealthBarParent/AI Health Bar Canvas");

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

            hit = true;
            // Play the guard hit sound
            aiAudioSource = collision.gameObject.GetComponent<AudioSource>();
            if (aiAudioSource != null && guardHitClip != null)
            {
                aiAudioSource.PlayOneShot(guardHitClip);
            }

            // Start a coroutine to prevent AI rotation during the hit animation
            StartCoroutine("HoldRotation");

            OnHitGuitar?.Invoke();

            // Cache initial AI rotation for animation reset
            initialPlayerRotation = collision.gameObject.transform.rotation;

            // Temporarily set AI to invincible during hit animation
            guardInvincibility.MakeInvincible();

            // Fetch necessary AI components for subsequent interactions
            aiSystem = collision.gameObject.GetComponent<EmeraldAISystem>();
            eventsManager = collision.gameObject.GetComponent<EmeraldAIEventsManager>();
            aiAnimator = collision.gameObject.GetComponent<Animator>();
            aiNavMeshAgent = collision.gameObject.GetComponent<NavMeshAgent>();
            boxCollider = collision.gameObject.GetComponent<BoxCollider>();
            guardCollision = collision.gameObject.GetComponent<GuardOnCollision>();
            AIRb = collision.gameObject.GetComponent<Rigidbody>();

            if (AIRb != null)
            {
                AIRb.isKinematic = false;
                Vector3 forceDirection = (collision.transform.position - this.transform.position).normalized; // Calculate force direction
                float forceMagnitude = 14.75f; // Adjust the force magnitude as needed
                
                AIRb.AddForce(forceDirection * forceMagnitude, ForceMode.Impulse); // Apply an impulse force
                
            }

            // Apply damage and update AI state
            guardCollision.canDetect = false;
            aiSystem.DetectionRadius = 0;
            eventsManager.ClearTarget();
            aiSystem.Damage(damageAmount, EmeraldAI.EmeraldAISystem.TargetType.Player, playerTransform, 1);
            Rigidbody rb = this.transform.GetComponent<Rigidbody>();
            rb.useGravity = true;
            rb.constraints = RigidbodyConstraints.None;
            
            Transform topParent = FindTopParent(this.transform);
            Transform playerArmatureTransform = topParent.Find("PlayerArmature");
            if (playerArmatureTransform != null)
            {
                GameObject playerArmature = playerArmatureTransform.gameObject;
                playerArmature.GetComponent<Inventory>().DecreaseHeldItem();
                // Now you have the playerArmature GameObject
            }
            else
            {
                Debug.LogError("PlayerArmature not found.");
            }
            this.transform.SetParent(null);
            //inventory.DecreaseHeldItem();

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
            Transform headJoint = collision.transform.Find("Hips_jnt/Spine_jnt/Spine_jnt 1/Chest_jnt/Neck_jnt/Head_jnt");
            if (headJoint != null)
            {
                StartCoroutine(SpawnEffectAfterDelay(headJoint));
            }

            // After a set delay, reset AI state post-hit
            StartCoroutine(WaitAndMove());
        }
    }

    // Coroutine to delay the spawning of a visual effect upon collision
    IEnumerator SpawnEffectAfterDelay(Transform headJoint)
    {
        yield return new WaitForSeconds(1.7f);

        if(AIRb != null)
            AIRb.isKinematic = true;

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

}
