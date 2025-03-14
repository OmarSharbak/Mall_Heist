using Edgegap;
using HeathenEngineering.SteamworksIntegration;
using I2.Loc;
using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LobbyMenuManager : MonoBehaviour
{
	[Header("Main")]
	[SerializeField] private GameObject mainMenuObject;
	[SerializeField] private GameObject mainMenuHostObject;
	[Header("Lobby")]
	[SerializeField] private GameObject lobbyObject;
	[SerializeField] private GameObject lobbyLeaveObject;
	[SerializeField] private TextMeshProUGUI lobbyTitle;
	[SerializeField] private LobbyManager lobbyManager;
	[SerializeField] private Button lobbyStartButton;
	[SerializeField] private Toggle lobbyReadyButton;
	[SerializeField] private TMP_Dropdown lobbyDropdown;

	[Header("Lobbies")]
	[SerializeField] private GameObject lobbiesMenuObject;
	[SerializeField] private GameObject askPasswordObject;
	[SerializeField] private TMP_InputField passwordJoinObject;
	[SerializeField] private GameObject incorrectPasswordObject;
	[SerializeField] private GameObject lobbyDataItemPrefab;
	[SerializeField] private GameObject lobbyListContent;
	[SerializeField] private GameObject lobbiesBackObject;


	[Header("User lobby setup")]
	[SerializeField] private LobbyUserPanel lobbyUserPanel;
	[SerializeField] private Transform lobbyUserHolder;

	[Header("Level Setup")]
	[SerializeField] private int totalLevels;


	private Dictionary<UserData, LobbyUserPanel> _lobbyUserPanels = new();

	private string _steamID64;

	private List<GameObject> _listOfLobbies = new List<GameObject>();

	private const string ipCheckUrl = "https://api.ipify.org";

	private string publicIP = null;
	private string password = null;
	private string tempPass = null;
	private LobbyData tempLobbyData = null;

	public void Start()
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

		lobbyManager.SetLobbyData("PASSWORD", password);

		Debug.Log("Password: " + password);
		
		var lobby = lobbyManager.Lobby;
		lobby.IsReady = true;
		FindAnyObjectByType<CustomNetworkManager>().playerName1 = UserData.Me.Nickname;

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
				lobbyReadyButton.interactable = true;

				FindAnyObjectByType<CustomNetworkManager>().playerName1 = lobbyManager.Lobby.Owner.user.Nickname;
				FindAnyObjectByType<CustomNetworkManager>().playerName2 = UserData.Me.Nickname;
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
		EventSystem.current.SetSelectedGameObject(mainMenuHostObject);


		lobbyDropdown.options.Clear();
		for (int i = 1; i <= totalLevels; i++)
		{
			bool isDemoCap = DemoManager.Instance!=null && DemoManager.Instance.IsLevelCap(i-1);
			if (isDemoCap)
				continue;
			LocalizedString level = "Level " + i;
			level.mRTL_IgnoreArabicFix = true;
			lobbyDropdown.options.Add(new TMP_Dropdown.OptionData(level));
		}
	}
	public void OpenLobbies()
	{
		CloseScreens();
		lobbiesMenuObject.SetActive(true);
		askPasswordObject.SetActive(false);
		incorrectPasswordObject.SetActive(false);
		EventSystem.current.SetSelectedGameObject(lobbiesBackObject);


	}
	public void OpenLobby()
	{
		CloseScreens();
		lobbyObject.SetActive(true);
		EventSystem.current.SetSelectedGameObject(lobbyLeaveObject);
	}

	public void OnUserJoin(UserData userData)
	{
		SetupCard(userData);
		if(userData!=lobbyManager.Lobby.Owner.user)
			FindAnyObjectByType<CustomNetworkManager>().playerName2 = userData.Nickname;

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
		lobbyStartButton.interactable = false;
		lobbyReadyButton.interactable = false;

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

	public void StartHost()
	{
		//start mirror host/server
		NetworkManager.singleton.StartHost();
		lobbyManager.SetLobbyData("STARTED", "true");
		lobbyStartButton.interactable = false;



	}

	public void StartClient()
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
		if (NetworkManager.singleton != null)
			Destroy(NetworkManager.singleton.gameObject);

		if (MultiplayerMode.Instance != null)
			Destroy(MultiplayerMode.Instance.gameObject);

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
			password = value;
		}

	}

	public void AskPassword(string pass, LobbyData lobbyData)
	{
		this.tempPass = pass;
		this.tempLobbyData = lobbyData;
		askPasswordObject.SetActive(true);
	}

	public void OnEnterPassword()
	{
		incorrectPasswordObject.SetActive(false);
		string pass = passwordJoinObject.text;
		if (!string.IsNullOrEmpty(pass))
		{
			if (pass == this.tempPass)
			{
				askPasswordObject.SetActive(false);
				JoinLobby(this.tempLobbyData);
				OpenLobby();
				return;
			}
		}
		incorrectPasswordObject.SetActive(true);
	}

	public void OnReady(bool value)
	{
		if (!lobbyManager.IsPlayerOwner)
		{
			lobbyReadyButton.interactable = false;
			var lobby = lobbyManager.Lobby;
			lobby.IsReady = true;			
		}
	}
	private void Update()
	{
		if (lobbyManager.IsPlayerOwner && lobbyManager.AllPlayersReady && lobbyManager.MemberCount==2)
		{
			lobbyStartButton.interactable = true;
		}else if (!lobbyManager.IsPlayerOwner)
		{
			string started = lobbyManager.GetLobbyData("STARTED");
			if (started == "true")
			{
				StartClient();
			}
		}
	}
}
