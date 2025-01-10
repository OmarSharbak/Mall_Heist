using HeathenEngineering.SteamworksIntegration;
using Steamworks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using TMPro;
using UnityEngine;

public class LobbyEntryItem : MonoBehaviour
{
	public LobbyData lobbyData;
	public string lobbyName;
	public TextMeshProUGUI lobbyNameText;
	public TextMeshProUGUI pingText;

	private LobbyMenuManager lobbyMenuManager;


	private void Awake()
	{
		lobbyMenuManager= FindAnyObjectByType<LobbyMenuManager>();
	}
	public void SetLobbyData(LobbyData lobbyData)

	{

		this.lobbyData = lobbyData;
		lobbyName = this.lobbyData.Name;

		if (lobbyName == "")
		{
			lobbyNameText.text = "Empty";

		}
		else

		lobbyNameText.text = lobbyName;

		//string ip=lobbyData.GetMemberMetadata("HOSTIP");


		// Create a handler instance
		PingResponseHandler pingHandler = new PingResponseHandler(OnServerRespond,OnServerFailedToRespond);

		HeathenEngineering.SteamworksIntegration.API.Matchmaking.Client.PingServer(lobbyData.GameServer.ipAddress, lobbyData.GameServer.port, pingHandler);

		Debug.Log("Game Server ip:"+lobbyData.GameServer.ipAddress);

	}

	private void OnServerFailedToRespond()
	{
		Debug.LogWarning("Ping Server failed to respond");
	}

	private void OnServerRespond(gameserveritem_t serverInfo)
	{
		if (serverInfo != null)
		{
			pingText.text = serverInfo.m_nPing + "ms";
		}
	}

	public void JoinLobby()
	{
		lobbyMenuManager.JoinLobby(lobbyData);
		lobbyMenuManager.OpenLobby();

	}


//	lobbyData.GetMetadata().TryGetValue("P2PPING", out string value);
//		if(value != null)
//		{
//			Debug.Log(value);

//			int ping = ExtractPing(value, "mad");

//	pingText.text = ping + "ms";

//		}


//public static int ExtractPing(string input, string location)
//{
//	// Split the input string by commas to get each location's data
//	string[] locations = input.Split(',');

//	foreach (var loc in locations)
//	{
//		// Split each location string by '=' to separate the location code and ping data
//		string[] locData = loc.Split('=');
//		if (locData[0] == location)
//		{
//			// Extract the ping value (before the first '+')
//			string pingString = locData[1].Split('+')[0];
//			if (int.TryParse(pingString, out int ping))
//			{
//				return ping;
//			}
//			else
//			{
//				Console.WriteLine("Error parsing ping.");
//				return -1; // Return -1 if parsing fails
//			}
//		}
//	}

//	return -1; // Return -1 if location is not found
//}

}
