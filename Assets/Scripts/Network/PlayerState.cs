using Mirror;
using System;
using UnityEngine;

public class PlayerState : NetworkBehaviour
{

	public bool exposed = false; // Whether this player is exposed

	[SyncVar(hook =nameof(OnPlayerReady))]
	public bool ready = false; // Whether this player is exposed


	public Transform playerTransform; // Reference to the player's transform	
									  //

	public EscalatorManager.GameState currentState { get; set; } = EscalatorManager.GameState.WaitingToStart;// Current game state

	public ThirdPersonController thirdPersonController = null;

	// Determines if player is near the escalator for interaction
	[SyncVar]
	public bool playerNearEscalator = false;

	[Command(requiresAuthority =false)]
	public void CmdSetGameState(EscalatorManager.GameState newState)
	{
		SetState(newState);

		RpcSetGameState(newState);
	}
	// Set the current game state and handle related behavior
	[ClientRpc]
	public void RpcSetGameState(EscalatorManager.GameState newState)
	{
		SetState(newState);
	}

	public void SetState(EscalatorManager.GameState newState)
	{
		currentState = newState;

		switch (currentState)
		{
			case EscalatorManager.GameState.WaitingToStart:
				// Handle logic related to normal gameplay here, e.g., resume gameplay
				Time.timeScale = 1.0f;
				// ... any other logic
				break;

			case EscalatorManager.GameState.Pause:
				// Handle game pause logic here, e.g., stop gameplay
				Time.timeScale = 0.0f;
				// ... any other logic
				EscalatorManager.Instance.UpdateMusicState(this);
				break;

			case EscalatorManager.GameState.Defeat:
				// Handle logic related to player's defeat here
				// ... e.g., display defeat screen, play defeat music, etc.
				//Cursor.visible = true;
				//Cursor.lockState = CursorLockMode.None;
				thirdPersonController.canMove = false;
				thirdPersonController.StopMovement();
				//thirdPersonController.ToggleVisibility();
				//CmdSetGameStateDefeat(netId);
				//Time.timeScale = 0.0f;
				break;

			case EscalatorManager.GameState.Victory:
				// Handle logic related to player's defeat here
				// ... e.g., display defeat screen, play defeat music, etc.
				//Cursor.visible = true;
				//Cursor.lockState = CursorLockMode.None;
				thirdPersonController.canMove = false;
				thirdPersonController.StopMovement();
				Time.timeScale = 0.0f;
				break;

			case EscalatorManager.GameState.CountdownToStart:
				// Handle logic related to player's victory here
				// ... e.g., display victory screen, play victory music, etc.
				Time.timeScale = 1.0f;
				break;

			case EscalatorManager.GameState.Stealth:
				// Handle logic related to player's victory here
				// ... e.g., display victory screen, play victory music, etc.
				Time.timeScale = 1.0f;
				EscalatorManager.Instance.UpdateMusicState(this);
				break;

			case EscalatorManager.GameState.Chase:
				// Handle logic related to player's victory here
				// ... e.g., display victory screen, play victory music, etc.
				Time.timeScale = 1.0f;
				EscalatorManager.Instance.UpdateMusicState(this);
				break;

			default:
				Debug.LogWarning("Unknown game state set: " + newState);
				break;
		}
	}

	[Command(requiresAuthority = false)]
	public void CmdSetPlayerReady()
	{
		Debug.Log("Server - player ready");
		ready = true;
	}

	private void OnPlayerReady(bool _oldValue,bool _newVAlue)
	{
		Debug.Log("client - on player ready");

		GameManager.Instance.CmdCheckAllPlayersReady();
	}

}


