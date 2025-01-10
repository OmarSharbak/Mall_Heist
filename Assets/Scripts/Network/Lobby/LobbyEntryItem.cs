using HeathenEngineering.SteamworksIntegration;
using Steamworks;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LobbyEntryItem : MonoBehaviour
{
	public LobbyData lobbyData;
	public string lobbyName;
	public TextMeshProUGUI lobbyNameText;

	private LobbyMenuManager lobbyMenuManager;


	private void Awake()
	{
		lobbyMenuManager= FindAnyObjectByType<LobbyMenuManager>();
	}
	public void SetLobbyData()
	{
		if (lobbyName == "")
		{
			lobbyNameText.text = "Empty";

		}
		else

		lobbyNameText.text = lobbyName;

	}
	public void JoinLobby()
	{
		lobbyMenuManager.JoinLobby(lobbyData);
		lobbyMenuManager.OpenLobby();

	}
}
