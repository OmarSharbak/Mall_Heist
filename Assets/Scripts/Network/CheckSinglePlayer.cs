using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
public class CheckSinglePlayer : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
		MultiplayerMode multiplayerMode = FindObjectOfType<MultiplayerMode>();
		if ((multiplayerMode!=null && multiplayerMode.isSinglePlayer) || multiplayerMode==null)
		{
			NetworkServer.dontListen = true;
			NetworkManager.singleton.StartHost();
		}
	}

}
