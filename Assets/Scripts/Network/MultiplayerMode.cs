using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultiplayerMode : MonoBehaviour
{
	public bool isSinglePlayer = true;
	public int lvlIndex = 1;

	void Start()
	{
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
