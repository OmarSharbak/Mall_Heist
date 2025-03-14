using UnityEngine;
using Mirror;
using System.Collections;
using TMPro;
using System;
using HeathenEngineering.SteamworksIntegration;

public class GameManager : NetworkBehaviour
{

	[Serializable]
	public class ServerItem
	{
		public string itemName;
		public int quantity;
	}
	public static GameManager Instance { get; private set; }

	[SyncVar] private bool gameStarted = false;
	private float countdownTime = 3f;
	private int requiredPlayers = 2; // Fixed number of players

	// Define the SyncVar with a hook
	[SyncVar(hook = nameof(OnGlobalMoneyChanged))]
	private int globalMoney;

	// Create a SyncList for the global inventory

	public SyncList<ServerItem> inventory = new SyncList<ServerItem>();

	[SerializeField]
	private TMP_Text moneyText; // Text to display money

	// Declare a static event
	public static event Action<ThirdPersonController, int> OnPlayerJoined;
	public static event Action<ThirdPersonController, int> OnPlayerExisting;

	public Material player2BlueMaterial; // Assign this in the inspector 

	private void Awake()
	{
		if (Instance != null && Instance != this)
		{
			Destroy(gameObject);
			return;
		}
		Instance = this;

		if ((MultiplayerMode.Instance != null && MultiplayerMode.Instance.isSinglePlayer) || MultiplayerMode.Instance == null)
			requiredPlayers = 1;


		moneyText.text = GameManager.Instance.GetCurrentGlobalMoney().ToString(); // Initialize money text

		inventory.OnChange += OnInventoryChanged;

	}

	private void Start()
	{
		if ((MultiplayerMode.Instance != null && MultiplayerMode.Instance.isSinglePlayer) || MultiplayerMode.Instance == null)

			EscalatorManager.Instance.countDownText.text="";

	}
	[Command(requiresAuthority = false)]
	public void CmdCheckAllPlayersReady()
	{
		if (gameStarted) return;

		//// Check if there are exactly two players connected
		//if (NetworkServer.connections.Count != requiredPlayers) return;

		// Ensure all players are ready
		bool allReady = true;
		foreach (NetworkConnectionToClient conn in NetworkServer.connections.Values)
		{
			if (conn.identity == null) continue;

			PlayerState player = conn.identity.GetComponent<PlayerState>();
			if (player == null || !player.ready)
			{
				allReady = false;
				break;
			}
		}
		Debug.Log("Server - all ready " + allReady);
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

	[ClientRpc]
	public void RpcNotifyPlayerJoined(uint playerNetId, uint[] existingNetId, int numPlayers)
	{
		Debug.Log($"RpcNotifyPlayerJoined called on remote client for netId={playerNetId}");

		if (NetworkClient.spawned.TryGetValue(playerNetId, out NetworkIdentity identity))
		{
			if (identity != null)
			{
				ThirdPersonController controller = identity.GetComponent<ThirdPersonController>();
				if (controller != null)
				{
					controller.transform.name = "Player_" + numPlayers;
					OnPlayerJoined?.Invoke(controller, numPlayers);

					//change name and color
					if (numPlayers == 2 && player2BlueMaterial != null)
					{
						CustomNetworkManager nm = FindAnyObjectByType<CustomNetworkManager>();
						string name = nm != null ? nm.playerName2 : "";
						Color color = Color.blue;
						color.a = 0.3f;
						// Get the Renderer component of the object
						Renderer renderer = controller.modelRenderer;
						if (renderer != null)
						{
							// Change the material
							renderer.material = player2BlueMaterial;

						}
						controller.SetupPlayer(name, color);
					}
					else
					{
						CustomNetworkManager nm = FindAnyObjectByType<CustomNetworkManager>();

						string name = nm != null ? nm.playerName1 : "";
						Color color = Color.red;
						color.a = 0.3f;


						controller.SetupPlayer(name, color);

					}

					if (isServer && numPlayers == 2)
					{
						EscalatorManager.Instance.playerRemote = controller.GetComponent<PlayerState>();

						EscalatorManager.Instance.Initialize(EscalatorManager.Instance.playerRemote);
						EscalatorManager.Instance.Initialize(EscalatorManager.Instance.playerLocal);


						Debug.Log("Player remote State correct!");
					}
					if (MultiplayerMode.Instance != null && MultiplayerMode.Instance.isSinglePlayer)
					{
						EscalatorManager.Instance.Initialize(EscalatorManager.Instance.playerLocal);
						controller.SetupPlayer("", Color.black);
					}

					Debug.Log($"Player {controller.gameObject.name} has spawned on the remote client!");
					foreach (var netId in existingNetId)
					{
						if (NetworkClient.spawned.TryGetValue(netId, out NetworkIdentity identityExisting))
						{
							if (identityExisting != null)
							{
								ThirdPersonController controllerExisting = identityExisting.GetComponent<ThirdPersonController>();
								if (controllerExisting != null)
								{
									controllerExisting.transform.name = "Player_1";
									OnPlayerExisting?.Invoke(controllerExisting, numPlayers);
									Debug.Log($"Existing Player {controllerExisting.gameObject.name} has spawned on the remote client!");

									CustomNetworkManager nm = FindAnyObjectByType<CustomNetworkManager>();

									string name = nm != null ? nm.playerName1 : "";
									Color color = Color.red;
									color.a = 0.3f;


									controllerExisting.SetupPlayer(name, color);

									if (!isServer && numPlayers == 2)
									{
										EscalatorManager.Instance.playerRemote = controllerExisting.GetComponent<PlayerState>();

										EscalatorManager.Instance.Initialize(EscalatorManager.Instance.playerRemote);
										EscalatorManager.Instance.Initialize(EscalatorManager.Instance.playerLocal);


										Debug.Log("Player remote State correct!");
									}
								}
							}
						}

						//notify existing players
					}
				}
			}
			else
			{
				Debug.LogError($"NetworkIdentity for player with netId {playerNetId} is null.");
			}
		}
		else
		{
			Debug.LogWarning($"Player with netId {playerNetId} not found in remote client spawn list.");
		}
	}


	// Example method to add an item to the inventory
	[Server]
	public void AddItem(string itemName, int quantity)
	{
		ServerItem existingItem = inventory.Find(item => item.itemName == itemName);
		if (existingItem != null)
		{

			existingItem.quantity += quantity;
			Debug.Log("SERVER - Add Item increased quantity");
		}
		else
		{
			inventory.Add(new ServerItem { itemName = itemName, quantity = quantity });
		}
	}

	// Example method to remove an item from the inventory
	[Server]
	public void RemoveItem(string itemName, int quantity)
	{
		ServerItem existingItem = inventory.Find(item => item.itemName == itemName);
		if (existingItem != null)
		{
			existingItem.quantity -= quantity;
			if (existingItem.quantity <= 0)
			{
				inventory.Remove(existingItem);
			}
		}
	}

	private void OnInventoryChanged(SyncList<ServerItem>.Operation op, int index, ServerItem item)
	{
		switch (op)
		{
			case SyncList<ServerItem>.Operation.OP_ADD:
				//Debug.Log($"Item Added: {item.itemName} ({item.quantity})");
				break;
			case SyncList<ServerItem>.Operation.OP_REMOVEAT:
				//Debug.Log($"Item Removed: {item.itemName}");
				break;
			case SyncList<ServerItem>.Operation.OP_SET:
				//Debug.Log($"Item Updated: {item.itemName} ({item.quantity})");
				break;
		}
	}
}
