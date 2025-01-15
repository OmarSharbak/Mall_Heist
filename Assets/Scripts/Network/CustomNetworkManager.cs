using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomNetworkManager : NetworkManager
{

	// A list to track connected players
	public static readonly List<NetworkIdentity> connectedPlayers = new List<NetworkIdentity>();
	private int _numPlayers = 0;
	[HideInInspector]
	public string playerName1 = "";
	[HideInInspector]
	public string playerName2 = "";
	
	public override void OnStartServer()
	{
		base.OnStartServer();
		connectedPlayers.Clear();

	}


	public override void OnServerAddPlayer(NetworkConnectionToClient conn)
	{
		base.OnServerAddPlayer(conn);
		Debug.Log("A player has connected to the server");

		// Track the new player
		NetworkIdentity newPlayerIdentity = conn.identity;
		connectedPlayers.Add(newPlayerIdentity);
		_numPlayers++;

		// Delay access to GameManager to ensure it has been initialized
		StartCoroutine(NotifyPlayerJoined(conn, _numPlayers)); // Call the method after 1 second



	}


	[ServerCallback]
	IEnumerator NotifyPlayerJoined(NetworkConnectionToClient conn, int numPlayers)
	{
		yield return new WaitForSeconds(1);

		// Notify the new player about all previously connected players
		List<uint> existingPlayerIds = new List<uint>();
		foreach (var player in connectedPlayers)
		{
			if (player != conn.identity) // Exclude the new player from the list
				existingPlayerIds.Add(player.netId);
		}

		// Get the player GameObject and the PlayerManager component
		if (GameManager.Instance != null)
			// Notify all clients that a new player has joined
			GameManager.Instance.RpcNotifyPlayerJoined(conn.identity.netId, existingPlayerIds.ToArray(), numPlayers);
		else
			Debug.Log("Custom Network Manager: gameManager null!");

	}

}
