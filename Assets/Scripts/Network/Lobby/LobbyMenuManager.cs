using HeathenEngineering.SteamworksIntegration;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class LobbyMenuManager : MonoBehaviour
{
	[Header("Main")]
	[SerializeField] private GameObject mainMenuObject;
	[Header("Lobby")]
	[SerializeField] private GameObject lobbyObject;
	[SerializeField] private TextMeshProUGUI lobbyTitle;
	[SerializeField] private LobbyManager lobbyManager;


	[Header("User lobby setup")]
	[SerializeField] private LobbyUserPanel lobbyUserPanel;
	[SerializeField] private Transform lobbyUserHolder;


	private Dictionary<UserData, LobbyUserPanel> _lobbyUserPanels = new();
	private void Awake()
	{
		OpenMainMenu();
		HeathenEngineering.SteamworksIntegration.API.Overlay.Client.EventGameLobbyJoinRequested.AddListener(OverlayJoinButton);
	}

	public void OnLobbyCreated(LobbyData lobbyData)
	{
		ClearCards();
		lobbyData.Name= UserData.Me.Name+ "'s Lobby";
		lobbyTitle.text= UserData.Me.Name+ "'s Lobby";
		OpenLobby();

		SetupCard(UserData.Me);

	}

	public void OnLobbyJoined(LobbyData lobbyData)
	{
		ClearCards();
		lobbyTitle.text = lobbyData.Name;
		OpenLobby();

		foreach(var member in lobbyData.Members)
			SetupCard(member.user);
	}

	private void OverlayJoinButton(LobbyData lobbyData, UserData userData)
	{
		lobbyManager.Join(lobbyData);
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

	public void OnUserJoin(UserData userData)
	{
		SetupCard(userData);
	}
	public void OnUserleft(UserLobbyLeaveData userLeaveData)
	{
		if(!_lobbyUserPanels.TryGetValue(userLeaveData.user, out var panel))
		{
		}
		Destroy(panel.gameObject);
		_lobbyUserPanels.Remove(userLeaveData.user);
	}

	private void CloseScreens()
	{
		mainMenuObject.SetActive(false);
		lobbyObject.SetActive(false);
	}

	private void ClearCards()
	{
		foreach(Transform child in lobbyUserHolder)
			Destroy(child.gameObject);

		_lobbyUserPanels.Clear();
	}
	private void SetupCard(UserData userData)
	{
		var userPanel = Instantiate(lobbyUserPanel, lobbyUserHolder);
		userPanel.Initialize(userData);
		_lobbyUserPanels.TryAdd(userData,userPanel);
	}

}
