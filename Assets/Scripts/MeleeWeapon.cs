using UnityEngine;
using EmeraldAI;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.AI;
using MoreMountains.Feedbacks;
using System;
using Mirror;

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
    [SyncVar]
    bool hit = false;

    public static event Action OnHitGuitar;

    public static event Action OnGuardHit;

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

    [ClientCallback]
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject != null)
        {
            var _netIdentity = collision.gameObject.GetComponent<NetworkIdentity>();
            if (_netIdentity != null)
            {


                if (canDamage == false || hit)
                {
                    return;
                }
                Debug.Log("Item - collision passed");

                CmdCustomCollisionEnter(_netIdentity.netId);
                Debug.Log("CLIENT - collision with network identity:" + _netIdentity.gameObject.name);

            }
        }

    }

    [Command(requiresAuthority = false)]
    private void CmdCustomCollisionEnter(uint _netId)
    {
        Debug.Log("SERVER - Item - collision enter - cmd called");

        RpcCustomCollisionEnter(_netId);
    }

    [ClientRpc]
    private void RpcCustomCollisionEnter(uint _netId)
    {
        Debug.Log("CLIENT - Item - collision enter - rpc called");

        if (NetworkClient.spawned.TryGetValue(_netId, out NetworkIdentity netIdentity))
        {
            if (netIdentity != null)
            {
                Debug.Log("CLIENT - Item - collision enter:" + netIdentity.gameObject.name);


                if (netIdentity.gameObject.GetComponent<EmeraldAISystem>() != null)
                {
                    // Check for invincibility status on collision target
                    guardInvincibility = netIdentity.gameObject.GetComponent<GuardInvincibility>();
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

                    hit = true;
                    // Play the guard hit sound
                    aiAudioSource = netIdentity.gameObject.GetComponent<AudioSource>();
                    if (aiAudioSource != null && guardHitClip != null)
                    {
                        aiAudioSource.PlayOneShot(guardHitClip);
                    }

                    // Start a coroutine to prevent AI rotation during the hit animation
                    StartCoroutine("HoldRotation");

                    OnHitGuitar?.Invoke();
                    OnGuardHit?.Invoke();


                    // Cache initial AI rotation for animation reset
                    initialPlayerRotation = netIdentity.gameObject.transform.rotation;

                    // Temporarily set AI to invincible during hit animation
                    guardInvincibility.MakeInvincible();

                    // Fetch necessary AI components for subsequent interactions
                    aiSystem = netIdentity.gameObject.GetComponent<EmeraldAISystem>();
                    eventsManager = netIdentity.gameObject.GetComponent<EmeraldAIEventsManager>();
                    aiAnimator = netIdentity.gameObject.GetComponent<Animator>();
                    aiNavMeshAgent = netIdentity.gameObject.GetComponent<NavMeshAgent>();
                    boxCollider = netIdentity.gameObject.GetComponent<BoxCollider>();
                    guardCollision = netIdentity.gameObject.GetComponent<GuardOnCollision>();
                    AIRb = netIdentity.gameObject.GetComponent<Rigidbody>();

                    if (AIRb != null)
                    {
                        AIRb.isKinematic = false;
                        Vector3 forceDirection = (netIdentity.transform.position - this.transform.position).normalized; // Calculate force direction
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
                    if (topParent != null)
                    {
                        GameObject playerArmature = topParent.gameObject;
                        playerArmature.GetComponent<Inventory>().DecreaseHeldItem();
                        // Now you have the playerArmature GameObject
                    }
                    else
                    {
                        Debug.LogError("Player Top Parent not found.");
                    }
                    CmdDeatach();
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
                    hit = true;  // Flag hit to prevent repeat processing

                    ThirdPersonController controller = netIdentity.gameObject.GetComponent<ThirdPersonController>();
                    controller.canMove = false;

                    Transform topParent = FindTopParent(this.transform);
                    if (topParent != null)
                    {
                        GameObject playerArmature = topParent.gameObject;
                        var inventory = playerArmature.GetComponent<Inventory>();

                        if (inventory != null)
                            inventory.DecreaseHeldItem();
                    }
                    else
                    {
                        Debug.LogError("Player Top Parent not found.");
                    }

                    controller.animator.SetTrigger("Hit");

                    // If collision happens at head joint, trigger a stunning visual effect
                    Transform headJoint = netIdentity.transform.Find("Geometry/SimplePeople_Pimp_White/Hips_jnt/Spine_jnt/Spine_jnt 1/Chest_jnt/Neck_jnt/Head_jnt");
                    if (headJoint != null)
                    {
                        StartCoroutine(SpawnEffectAfterDelayPlayer(headJoint));
                    }

                    // After a set delay, reset state post-hit
                    StartCoroutine(WaitAndMovePlayer(controller));

                    CmdDeatach();
                }
                else if (netIdentity.gameObject.TryGetComponent(out BreakableItem breakableItem))
                {
                    hit = true;
                    breakableItem.Hit();
                    CmdDeatach();
                }
            }
            else
            {
                Debug.LogError("CLIENT - item collision enter - has no netidentity");
            }
        }
        else
        {
            Debug.LogError("CLIENT - item collision enter - has no netidentity");
        }
    }
    [Command]
    void CmdDeatach()
    {
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
                rb.useGravity = true;
                rb.constraints = RigidbodyConstraints.None;
            }
            //Collider cd = itemTransform.GetComponent<Collider>();
            //if (cd != null)
            //	cd.isTrigger = true;
        }
    }



    // Coroutine to delay the spawning of a visual effect upon collision
    IEnumerator SpawnEffectAfterDelay(Transform headJoint)
    {
        yield return new WaitForSeconds(1.7f);

        if (AIRb != null)
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
