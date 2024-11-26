using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using EmeraldAI;
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
	}
