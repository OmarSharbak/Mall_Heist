using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using EmeraldAI.Utility;
using Mirror;

public class ArrowIndicator : NetworkBehaviour
{
	private Transform player;
	public List<Transform> guards = new List<Transform>();
	public Image arrowPrefab;
	public float circleRadius = 100f;  // The radius of the circle around the player on the screen where the arrows are placed.

	private Camera mainCamera;
	private List<Image> arrows = new List<Image>();

	private bool arrowsActive = false;
	private Transform currentGuard = null;

	private void Start()
	{
		mainCamera = Camera.main;
	}

	private void HandleLocalPlayerStarted(ThirdPersonController localPlayer)
	{
		Debug.Log("Arrows - local player started");

		player = localPlayer.transform;
		PlayerDamageHandler.OnPlayerCaught += PlayerDamageHandler_OnPlayerCaught;
		GrassDecorationStealth.OnPlayerHidePlants+= PlayerDamageHandler_OnPlayerCaught;
		ThrowableItem.OnGuardHit += PlayerDamageHandler_OnPlayerCaught;
		TrapItem.OnGuardHit += PlayerDamageHandler_OnPlayerCaught;
		MeleeWeapon.OnGuardHit += PlayerDamageHandler_OnPlayerCaught;
		CreateArrows();
	}

	private void PlayerDamageHandler_OnPlayerCaught()
	{
		CmdDisableArrows(player);
	}

	private void CreateArrows()
	{
		foreach (var guard in guards)
		{
			Image newArrow = Instantiate(arrowPrefab, transform);
			newArrow.enabled = false;
			arrows.Add(newArrow);
			Debug.Log("Arrows - arrow created");

			if (isServer)
			{
				EmeraldAIDetection emeraldAIDetection = guard.GetComponent<EmeraldAIDetection>();
				emeraldAIDetection.EmeraldComponent.OnDetectTargetEvent.AddListener(OnTargetDetected);
				Debug.Log("SERVER Arrows - guard ai listener added");

			}

		}



	}

	public void Update()
	{
		foreach (Image arrow in arrows)
		{
			arrow.enabled = arrowsActive;
			if (arrowsActive)
			{
				PointArrowToGuard(arrow, currentGuard);
			}

		}

	}

	private void OnTargetDetected()
	{
		Debug.Log("Arrows - Target detected");

		if (player == null)
			return;

		for (int i = 0; i < guards.Count; i++)
		{
			Transform _player = IsGuardFollowing(guards[i]);
			if (_player != null)
			{
				CmdEnableArrows(guards[i], _player);
			}

		}

	}

	private Transform IsGuardFollowing(Transform guard)
	{
		EmeraldAIDetection emeraldAIDetection = guard.GetComponent<EmeraldAIDetection>();
		if (emeraldAIDetection.EmeraldComponent.CurrentTarget != null)
			return emeraldAIDetection.EmeraldComponent.CurrentTarget;
		return null;
	}

	[Command(requiresAuthority =false)]
	private void CmdDisableArrows(Transform _player)
	{
		RpcDisableArrows(_player);
		Debug.Log("SERVER - Arrows - disabled");

	}
	[Command(requiresAuthority = false)]
	private void CmdEnableArrows(Transform guard, Transform _player)
	{
		RpcEnableArrows(guard, _player);
		Debug.Log("SERVER - Arrows - enabled");

	}

	[ClientRpc]
	private void RpcDisableArrows(Transform _player)
	{
		if (_player != player)
		{
			Debug.Log("Arrows - Player is not the target player");

			return;
		}
		arrowsActive = false;
		Debug.Log("CLIENT - Arrows - disabled");

	}









	[ClientRpc]
	private void RpcEnableArrows(Transform guard, Transform _player)
	{
		if (_player != player)
		{
			Debug.Log("Arrows - Player is not the target player");

			return;
		}
		arrowsActive = true;

		currentGuard = guard;

		Debug.Log("CLIENT - Arrows - enabled");


	}
	private void PointArrowToGuard(Image arrow, Transform guard)
	{

		Vector3 dirToGuard = guard.position - player.position;
		float angle = GetAngleFromDirection(dirToGuard);
		arrow.rectTransform.rotation = Quaternion.Euler(90, 0, angle);

		Vector2 arrowPosition = CalculatePositionOnCircle(angle, circleRadius);
		arrow.rectTransform.anchoredPosition = arrowPosition;  // This will place the arrow at a specific position around the player.
	}

	private float GetAngleFromDirection(Vector3 dir)
	{
		dir = mainCamera.transform.InverseTransformDirection(dir);
		float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
		return angle - 90;
	}

	private Vector2 CalculatePositionOnCircle(float angle, float radius)
	{
		float x = radius * Mathf.Cos(angle * Mathf.Deg2Rad);
		float y = radius * Mathf.Sin(angle * Mathf.Deg2Rad);
		return new Vector2(x, y);
	}

	private void OnEnable()
	{
		// Subscribe to the event
		ThirdPersonController.OnLocalPlayerStarted += HandleLocalPlayerStarted;
	}

	private void OnDisable()
	{
		// Unsubscribe from the event to avoid memory leaks
		ThirdPersonController.OnLocalPlayerStarted -= HandleLocalPlayerStarted;
        PlayerDamageHandler.OnPlayerCaught -= PlayerDamageHandler_OnPlayerCaught;
        GrassDecorationStealth.OnPlayerHidePlants -= PlayerDamageHandler_OnPlayerCaught;
        ThrowableItem.OnGuardHit -= PlayerDamageHandler_OnPlayerCaught;
        TrapItem.OnGuardHit -= PlayerDamageHandler_OnPlayerCaught;
        MeleeWeapon.OnGuardHit -= PlayerDamageHandler_OnPlayerCaught;
    }
}
