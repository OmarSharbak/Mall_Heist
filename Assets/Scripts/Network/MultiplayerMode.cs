using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultiplayerMode : MonoBehaviour
{
	public bool isSinglePlayer = true;
	public int lvlIndex = 1;

	public static MultiplayerMode Instance { get; private set; }

	void Start()
	{
		if (Instance != null && Instance != this)
		{
			Destroy(gameObject);
			return;
		}
		Instance = this;
		DontDestroyOnLoad(this);

	}
	public void SetSinglePlayer(bool value)
	{
		isSinglePlayer = value;
	}

	public void SetLevelIndex(int value)
	{
		lvlIndex = value;
	}
}
