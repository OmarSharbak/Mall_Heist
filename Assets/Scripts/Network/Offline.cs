using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Offline : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

		if (NetworkManager.singleton != null)
		{
			NetworkManager.singleton.StopHost();

			Destroy(NetworkManager.singleton.gameObject);

		}

		if(MultiplayerMode.Instance!=null)
			Destroy(MultiplayerMode.Instance.gameObject);

		SceneManager.LoadScene("MenuScene2");

	}
}
