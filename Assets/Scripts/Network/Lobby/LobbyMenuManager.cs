using HeathenEngineering.SteamworksIntegration;
using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class LobbyMenuManager : MonoBehaviour
{
	[Header("Main")]
	[SerializeField] private GameObject mainMenuObject;
	[Header("Lobby")]
	[SerializeField] private GameObject lobbyObject;
	[SerializeField] private TextMeshProUGUI lobbyTitle;
	[SerializeField] private LobbyManager lobbyManager;
	[Header("Lobbies")]
	[SerializeField] private GameObject lobbiesMenuObject;
	[SerializeField] private GameObject askPasswordObject;
	[SerializeField] private TextMeshProUGUI passwordJoinObject;
	[SerializeField] private GameObject incorrectPasswordObject;
	[SerializeField] private GameObject lobbyDataItemPrefab;
	[SerializeField] private GameObject lobbyListContent;

	[Header("User lobby setup")]
	[SerializeField] private LobbyUserPanel lobbyUserPanel;
	[SerializeField] private Transform lobbyUserHolder;


	private Dictionary<UserData, LobbyUserPanel> _lobbyUserPanels = new();

	private string _steamID64;

	private List<GameObject> _listOfLobbies = new List<GameObject>();

	private const string ipCheckUrl = "https://api.ipify.org";

	private string publicIP=null;
	private string tempPass=null;
	private LobbyData tempLobbyData=null;

	private void Awake()
	{
		OpenMainMenu();
		HeathenEngineering.SteamworksIntegration.API.Overlay.Client.EventGameLobbyJoinRequested.AddListener(OverlayJoinButton);
		StartCoroutine(GetPublicIPAddress());

	}


	IEnumerator GetPublicIPAddress()
	{
		using (UnityWebRequest webRequest = UnityWebRequest.Get(ipCheckUrl))
		{
			yield return webRequest.SendWebRequest();

			if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
			{
				Debug.LogError("Error: " + webRequest.error);
			}
			else
			{
				publicIP = webRequest.downloadHandler.text;

			}
		}
	}


	public void OnLobbyCreated(LobbyData lobbyData)
	{
		ClearCards();
		lobbyData.Name = UserData.Me.Name + "'s Lobby";
		lobbyTitle.text = UserData.Me.Name + "'s Lobby";
		OpenLobby();

		SetupCard(UserData.Me);

		lobbyManager.SetLobbyData("HOSTIP", publicIP);

		Debug.Log("Public IP: " + publicIP);


	}

	public void OnLobbyJoined(LobbyData lobbyData)
	{
		ClearCards();
		lobbyTitle.text = lobbyData.Name;
		OpenLobby();

		foreach (var member in lobbyData.Members)
			SetupCard(member.user);

		var hostUser = lobbyData.Owner.user;
		if (!hostUser.IsValid)
		{
			Debug.LogError("HostUser is not valid");
			return;
		}
		_steamID64 = hostUser.SteamId.ToString();

		if (lobbyManager.Full)
		{
			if (!lobbyManager.IsPlayerOwner)
			{
				Invoke(nameof(StartClient), 3f);
			}

		}

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
	public void OpenLobbies()
	{
		CloseScreens();
		lobbiesMenuObject.SetActive(true);
		askPasswordObject.SetActive(false);
		incorrectPasswordObject.SetActive(false);
	}
	public void OpenLobby()
	{
		CloseScreens();
		lobbyObject.SetActive(true);
	}

	public void OnUserJoin(UserData userData)
	{
		SetupCard(userData);

		if (lobbyManager.Full)
		{
			if (lobbyManager.IsPlayerOwner)
			{
				StartHost();
			}

		}
	}
	public void OnUserleft(UserLobbyLeaveData userLeaveData)
	{
		if (!_lobbyUserPanels.TryGetValue(userLeaveData.user, out var panel))
		{
		}
		Destroy(panel.gameObject);
		_lobbyUserPanels.Remove(userLeaveData.user);
	}

	private void CloseScreens()
	{
		mainMenuObject.SetActive(false);
		lobbyObject.SetActive(false);
		lobbiesMenuObject.SetActive(false);
	}

	private void ClearCards()
	{
		foreach (Transform child in lobbyUserHolder)
			Destroy(child.gameObject);

		_lobbyUserPanels.Clear();
	}
	private void SetupCard(UserData userData)
	{
		var userPanel = Instantiate(lobbyUserPanel, lobbyUserHolder);
		userPanel.Initialize(userData);
		_lobbyUserPanels.TryAdd(userData, userPanel);
	}

	private void StartHost()
	{
		//start mirror host/server
		NetworkManager.singleton.StartHost();
	}

	private void StartClient()
	{
		//start mirror client
		NetworkManager.singleton.networkAddress = _steamID64;
		NetworkManager.singleton.StartClient();
	}

	public void OnLevelSelected(int value)
	{
		NetworkManager.singleton.onlineScene = "Level" + (value + 1);
		Debug.Log("changed online scene: " + NetworkManager.singleton.onlineScene);
	}

	public void BackToMenu()
	{
		Destroy(NetworkManager.singleton.gameObject);
		SceneManager.LoadScene(0);
	}

	private void DisplayLobbies(LobbyData[] lobbies)
	{
		for (int i = 0; i < lobbies.Length; i++)
		{

			GameObject createdItem = Instantiate(lobbyDataItemPrefab);
			LobbyEntryItem lei = createdItem.GetComponent<LobbyEntryItem>();
			if (lei != null)
			{
				lei.SetLobbyData(lobbies[i]);
				lei.transform.SetParent(lobbyListContent.transform);
				lei.transform.localScale = Vector3.one;
			}

			_listOfLobbies.Add(createdItem);

		}
	}


	private void DestroyListOfLobbies()
	{
		foreach (GameObject lobbyItem in _listOfLobbies)
		{
			Destroy(lobbyItem);
		}
		_listOfLobbies.Clear();
	}

	public void GetListOfLobbies()
	{
		HeathenEngineering.SteamworksIntegration.API.Matchmaking.Client.AddRequestLobbyListResultCountFilter(60);
		HeathenEngineering.SteamworksIntegration.API.Matchmaking.Client.AddRequestLobbyListResultCountFilter(60);

		HeathenEngineering.SteamworksIntegration.API.Matchmaking.Client.RequestLobbyList(OnListRequested);

	}

	private void OnListRequested(LobbyData[] lobbyDatas, bool error)
	{
		if (error)
		{
			Debug.LogWarning(error.ToString());
		}
		else
		{
			DestroyListOfLobbies();
			DisplayLobbies(lobbyDatas);
		}
	}

	public void JoinLobby(LobbyData lobbyData)
	{
		lobbyManager.Join(lobbyData);
	}

	public void SetPassword(string value)
	{
		if (!string.IsNullOrEmpty(value))
		{
			lobbyManager.SetLobbyData("PASSWORD", value);

			Debug.Log("Password: " + value);
		}

	}

	public void AskPassword(string pass,LobbyData lobbyData)
	{
		this.tempPass = pass;
		this.tempLobbyData= lobbyData;
		askPasswordObject.SetActive(true);
	}

	public void OnEnterPassword() {
		incorrectPasswordObject.SetActive(false);
		string pass = passwordJoinObject.text;
		if (!string.IsNullOrEmpty(pass))
		{
			if(pass == this.tempPass)
			{
				askPasswordObject.SetActive(false);
				JoinLobby(this.tempLobbyData);
				OpenLobby();
				return;
			}
		}
		incorrectPasswordObject.SetActive(true);
	} 
}
