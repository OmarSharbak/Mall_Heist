using UnityEngine;
using Mirror;
public class CheckSinglePlayer : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        if ((MultiplayerMode.Instance != null && MultiplayerMode.Instance.isSinglePlayer))
        {
            string name = "Level" + MultiplayerMode.Instance.lvlIndex;
            Debug.Log(name);
            CustomNetworkManager customNetworkManager = GetComponent<CustomNetworkManager>();
            customNetworkManager.onlineScene = name;
            NetworkServer.dontListen = true;
            NetworkManager.singleton.StartHost();
        }
        else
        {
            Debug.LogWarning("CheckSingleplayer - MultiplayerMode is null");
        }
    }

}
