using UnityEngine;
using Mirror;

public class PlayerReady : NetworkBehaviour
{
	[SyncVar] public bool isReady = false;

	public override void OnStartServer()
	{
		base.OnStartServer();
		SetReadyState(true); // Automatically set as ready
	}

	[Server]
	public void SetReadyState(bool ready)
	{
		isReady = ready;
		GameManager.Instance.CheckAllPlayersReady(); // Notify GameManager to check readiness
	}
}
