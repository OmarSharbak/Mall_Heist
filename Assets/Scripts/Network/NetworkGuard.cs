using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using EmeraldAI;
using EmeraldAI.Utility;
using System.Threading;
public class NetworkGuard : NetworkBehaviour
{

    ThirdPersonController[] playersJoined= new ThirdPersonController[2];

    int count = 0;
	public override void OnStartClient()
    {
        base.OnStartClient();

        if (!isServer)
        {
            EmeraldAISystem emeraldAISystem = GetComponent<EmeraldAISystem>();
            emeraldAISystem.enabled = false;
        }
        else
        {
			//Example - It is recomended that EmeraldComponent is cached somewhere
			EmeraldAISystem emeraldAISystem = GetComponent<EmeraldAISystem>();

			emeraldAISystem.OnDoDamageEvent.AddListener(() => CheckPlayersDamaged());
			Debug.Log("PLayer Joined added to guards AI");
		}
    }
    private void HandlePlayerJoined(ThirdPersonController playerJoined)
    {

        if (isServer)
        {
            playersJoined[count]= playerJoined;
            count++;
			Debug.Log("plyer joined" + " ### " + playerJoined.name);

		}
	}

    [ServerCallback]
    private void CheckPlayersDamaged()
    {
		EmeraldAIEventsManager _emeraldAIEventsManager = transform.GetComponent<EmeraldAIEventsManager>();

		EmeraldAISystem emeraldAISystem = transform.GetComponent<EmeraldAISystem>();

		Transform combatTarget = emeraldAISystem.PlayerDamageComponent.transform;

        int i = 0;
		foreach (var player in playersJoined)
        {
           // Debug.LogError(player.name + " ### " + i);
            i++;

			if (combatTarget != null)
            {


                if (combatTarget.GetComponent<NetworkIdentity>() == player.GetComponent<NetworkIdentity>())
                {
					PlayerDamageHandler playerDamageHandler = player.GetComponent<PlayerDamageHandler>();

					playerDamageHandler.RpcSetNetworkGuard(GetComponent<NetworkIdentity>());


					//Debug.LogError("COMBAT MATCH! " + combatTarget.name + " ### " + player.name);
					return;
                }
                //Debug.LogError("COMBAT TARGET DON'T MATCH! " + combatTarget.name +" ### " +player.name);
			}
            //Debug.LogError("COMBAT TARGET DON'T MATCH!  null" + " ### " + player.name);

		}
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

