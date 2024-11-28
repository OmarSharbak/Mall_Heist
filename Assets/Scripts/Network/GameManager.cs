using UnityEngine;
using Mirror;
using System.Collections;
using TMPro;

public class GameManager : NetworkBehaviour
{
	public static GameManager Instance { get; private set; }

	[SyncVar] private bool gameStarted = false;
	private float countdownTime = 3f;
	private int requiredPlayers = 2; // Fixed number of players

	// Define the SyncVar with a hook
	[SyncVar(hook = nameof(OnGlobalMoneyChanged))]
	private int globalMoney;
	[SerializeField]
	private TMP_Text moneyText; // Text to display money

	private void Awake()
	{
		if (Instance != null && Instance != this)
		{
			Destroy(gameObject);
			return;
		}
		Instance = this;
		MultiplayerMode multiplayerMode = FindObjectOfType<MultiplayerMode>();
		if ((multiplayerMode != null && multiplayerMode.isSinglePlayer) || multiplayerMode == null)
			requiredPlayers = 1;

		moneyText.text = GameManager.Instance.GetCurrentGlobalMoney().ToString(); // Initialize money text

	}

	public void CheckAllPlayersReady()
	{
		if (!isServer || gameStarted) return;

		// Check if there are exactly two players connected
		if (NetworkServer.connections.Count != requiredPlayers) return;

		// Ensure all players are ready
		bool allReady = true;
		foreach (NetworkConnectionToClient conn in NetworkServer.connections.Values)
		{
			if (conn.identity == null) continue;

			PlayerReady player = conn.identity.GetComponent<PlayerReady>();
			if (player == null || !player.isReady)
			{
				allReady = false;
				break;
			}
		}

		// Start the countdown if all players are ready
		if (allReady)
		{
			StartCoroutine(StartCountdown());
		}
	}

	[Server]
	private IEnumerator StartCountdown()
	{
		if (gameStarted) yield break;

		gameStarted = true;

		for (float time = countdownTime; time > 0; time--)
		{
			RpcUpdateCountdown((int)time);
			yield return new WaitForSeconds(1f);
		}

		RpcStartGame();
	}

	[ClientRpc]
	private void RpcUpdateCountdown(int timeRemaining)
	{
		Debug.Log($"Countdown: {timeRemaining}");
		// Update UI countdown display here
		EscalatorManager.Instance.UpdateCountdown(timeRemaining);
	}

	[ClientRpc]
	private void RpcStartGame()
	{
		Debug.Log("Game Started!");
		// Trigger game start logic here
		EscalatorManager.Instance.InitiateGameStart();

	}

	// Method to update the SyncVar value (server-side only)
	[Server]
	public void UpdateGlobalMoney(int newValue)
	{
		globalMoney = newValue; // Updates are automatically synced to all clients
	}

	// Hook method called on clients when globalScore changes
	private void OnGlobalMoneyChanged(int oldValue, int newValue)
	{
		Debug.Log($"Global Score updated: {oldValue} -> {newValue}");
		UpdateMoneyUI();
	}

	private void UpdateMoneyUI()
	{
		moneyText.text = GameManager.Instance.GetCurrentGlobalMoney().ToString();
	}

	public int GetCurrentGlobalMoney()
	{
		return globalMoney;
	}
}
