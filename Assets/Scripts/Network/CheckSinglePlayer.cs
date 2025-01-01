using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.SceneManagement;
public class CheckSinglePlayer : MonoBehaviour
{
	// Start is called before the first frame update
	void Start()
	{
		MultiplayerMode multiplayerMode = FindObjectOfType<MultiplayerMode>();
		if ((multiplayerMode != null && multiplayerMode.isSinglePlayer) || multiplayerMode == null)
		{
			string name = "Level" + multiplayerMode.lvlIndex;
			Debug.Log(name);
			CustomNetworkManager customNetworkManager = GetComponent<CustomNetworkManager>();
			customNetworkManager.onlineScene = name;
			NetworkServer.dontListen = true;
			NetworkManager.singleton.StartHost();
		}
	}

}
