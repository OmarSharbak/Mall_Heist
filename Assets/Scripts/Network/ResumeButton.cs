using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResumeButton : MonoBehaviour
{
	private ThirdPersonController thirdPersonController;
	private void HandleLocalPlayerStarted(ThirdPersonController localPlayer)
	{
		thirdPersonController = localPlayer;
	}

	public void Resume()
	{
		thirdPersonController.Resume();
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
	}


}
