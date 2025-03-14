using HeathenEngineering.SteamworksIntegration;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyEntryItem : MonoBehaviour
{
	public LobbyData lobbyData;
	public string lobbyName;
	public TextMeshProUGUI lobbyNameText;
	public TextMeshProUGUI pingText;
	public GameObject lockedIcon;

	private LobbyMenuManager lobbyMenuManager;

	private UnityEngine.Ping ping;

	private string password = null;

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

		lobbyData.GetMetadata().TryGetValue("HOSTIP", out string ip);
		
		if (ip != null)
		{
			Debug.Log("Game Server ip:" + ip);
			ping= new UnityEngine.Ping(ip);
		}

		lobbyData.GetMetadata().TryGetValue("PASSWORD", out this.password);

		lockedIcon.SetActive(false);

		if (!string.IsNullOrEmpty(password))
		{
			Debug.Log("Password protected:" + password);

			lockedIcon.SetActive(true);
		}

	}

	public void JoinLobby()
	{
		if (!string.IsNullOrEmpty(this.password))
		{
			lobbyMenuManager.AskPassword(this.password,lobbyData);
		}
		else
		{
			lobbyMenuManager.JoinLobby(lobbyData);
			lobbyMenuManager.OpenLobby();
		}

	}

	void Update()
	{
		if (ping !=null && ping.isDone)
		{
			pingText.text = ping.time + "ms";
			ping = null; // Reset ping after it finishes
		}
	}
}
