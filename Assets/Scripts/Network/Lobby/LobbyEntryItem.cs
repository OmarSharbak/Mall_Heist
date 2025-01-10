using HeathenEngineering.SteamworksIntegration;
using TMPro;
using UnityEngine;

public class LobbyEntryItem : MonoBehaviour
{
	public LobbyData lobbyData;
	public string lobbyName;
	public TextMeshProUGUI lobbyNameText;
	public TextMeshProUGUI pingText;

	private LobbyMenuManager lobbyMenuManager;

	private UnityEngine.Ping ping;

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

	}

	public void JoinLobby()
	{
		lobbyMenuManager.JoinLobby(lobbyData);
		lobbyMenuManager.OpenLobby();

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
