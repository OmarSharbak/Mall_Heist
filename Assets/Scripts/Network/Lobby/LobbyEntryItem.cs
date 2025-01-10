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

		// Create a handler instance
		PingResponseHandler pingHandler = new PingResponseHandler(OnServerRespond,OnServerFailedToRespond);

		HeathenEngineering.SteamworksIntegration.API.Matchmaking.Client.PingServer(lobbyData.GameServer.ipAddress, lobbyData.GameServer.port, pingHandler);



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

}
