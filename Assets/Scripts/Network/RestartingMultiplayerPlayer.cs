using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RestartingMultiplayer : MonoBehaviour
{
    void Start()
    {
        Time.timeScale = 1.0f;
        Invoke(nameof(LoadMulti), 1f);
    }

    private void LoadMulti()
    {
        Debug.Log("load Multiplayer");
		if (MultiplayerMode.Instance != null)
		{
			string name = "Level" + MultiplayerMode.Instance.lvlIndex;
			Debug.Log(name);
			CustomNetworkManager customNetworkManager = GetComponent<CustomNetworkManager>();
			customNetworkManager.onlineScene = name;
			if (MultiplayerMode.Instance.isHost)
				NetworkManager.singleton.StartHost();
			else
				Invoke(nameof(StartClient), 3f);

		}
		else
		{
			Debug.LogWarning("Multi - MultiplayerMode is null");
		}
	}

	private void StartClient()
	{
		NetworkManager.singleton.StartClient();
	}
}
