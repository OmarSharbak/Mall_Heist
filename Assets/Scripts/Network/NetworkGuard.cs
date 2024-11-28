using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using EmeraldAI;
using EmeraldAI.Utility;
public class NetworkGuard : NetworkBehaviour
{

    public override void OnStartClient()
    {
        base.OnStartClient();

        if (!isServer)
        {
            EmeraldAISystem emeraldAISystem = GetComponent<EmeraldAISystem>();
            emeraldAISystem.enabled = false;
        }
    }
    private void HandlePlayerJoined(ThirdPersonController playerJoined)
    {

        if (isServer)
        {
            //Example - It is recomended that EmeraldComponent is cached somewhere
            EmeraldAISystem emeraldAISystem = GetComponent<EmeraldAISystem>();

            emeraldAISystem.OnDoDamageEvent.AddListener(() => CheckPlayerDamaged(playerJoined));
            Debug.Log("PLayer Joined added to guards AI");
        }
    }

    [ServerCallback]
    private void CheckPlayerDamaged(ThirdPersonController playerJoined)
    {
		PlayerDamageHandler playerDamageHandler = playerJoined.GetComponent<PlayerDamageHandler>();
		EmeraldAIEventsManager _emeraldAIEventsManager = GetComponent<EmeraldAIEventsManager>();

		EmeraldAISystem emeraldAISystem = GetComponent<EmeraldAISystem>();

		Transform combatTarget = emeraldAISystem.PlayerDamageComponent.transform;

		if (combatTarget != null)
        {
            if (combatTarget == playerDamageHandler.transform)
            {
				playerDamageHandler.playerStatus = 1;

				playerDamageHandler.RpcSetNetworkGuard(this);

                return;
            }
			Debug.LogError("COMBAT TARGET DON'T MATCH! " + combatTarget.name);
		}
		Debug.LogError("COMBAT TARGET DON'T MATCH!  null");
	}


	private void OnEnable()
    {
        // Subscribe to the event
        ThirdPersonController.OnPlayerJoined += HandlePlayerJoined;
    }

    private void OnDisable()
    {
        // Unsubscribe from the event to avoid memory leaks
        ThirdPersonController.OnPlayerJoined -= HandlePlayerJoined;
    }
}

