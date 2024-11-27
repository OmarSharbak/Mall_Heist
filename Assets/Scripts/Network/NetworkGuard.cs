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
            
            PlayerDamageHandler playerDamageHandler = playerJoined.GetComponent<PlayerDamageHandler>();
            emeraldAISystem.OnDoDamageEvent.AddListener(() => OnDamageByAI(playerDamageHandler));
            Debug.Log("PLayer Joined added to guards AI");
        }
    }


    private void OnDamageByAI(PlayerDamageHandler playerDamageHandler)
    {
        playerDamageHandler.CmdDamageByAI(this);
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

