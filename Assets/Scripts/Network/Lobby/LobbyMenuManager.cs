using HeathenEngineering.SteamworksIntegration;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LobbyMenuManager : MonoBehaviour
{
	[Header("Main")]
	[SerializeField] private GameObject mainMenuObject;
	[Header("Lobby")]
	[SerializeField] private GameObject lobbyObject;
	[SerializeField] private TextMeshProUGUI lobbyTitle;
	[SerializeField] private LobbyManager lobbyManager;

	private void Awake()
	{
		OpenMainMenu();
		HeathenEngineering.SteamworksIntegration.API.Overlay.Client.EventGameLobbyJoinRequested.AddListener(OverlayJoinButton);
	}

	public void OnLobbyCreated(LobbyData lobbyData)
	{
		lobbyData.Name= UserData.Me.Name+ "'s Lobby";
		lobbyTitle.text= UserData.Me.Name+ "'s Lobby";
		OpenLobby();

	}

	public void OnLobbyJoined(LobbyData lobbyData)
	{
		lobbyData.Name = lobbyData.Name;
		lobbyTitle.text = lobbyData.Name;
		OpenLobby();

	}

	private void OverlayJoinButton(LobbyData lobbyData, UserData userData)
	{
		throw new NotImplementedException();
	}

	public void OpenMainMenu()
	{
		CloseScreens();
		mainMenuObject.SetActive(true);
	}

	public void OpenLobby()
	{
		CloseScreens();
		lobbyObject.SetActive(true);
	}

	private void CloseScreens()
	{
		mainMenuObject.SetActive(false);
		lobbyObject.SetActive(false);
	}

}
