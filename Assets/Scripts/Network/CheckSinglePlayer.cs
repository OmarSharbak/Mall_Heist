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
		
		if ((MultiplayerMode.Instance != null && MultiplayerMode.Instance.isSinglePlayer) || MultiplayerMode.Instance == null)
		{
			string name = "Level" + MultiplayerMode.Instance.lvlIndex;
			Debug.Log(name);
			CustomNetworkManager customNetworkManager = GetComponent<CustomNetworkManager>();
			customNetworkManager.onlineScene = name;
			NetworkServer.dontListen = true;
			NetworkManager.singleton.StartHost();
		}
	}

}
