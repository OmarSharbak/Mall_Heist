using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultiplayerMode : MonoBehaviour
{
	public bool isSinglePlayer = true;

	void Start()
	{
		DontDestroyOnLoad(this);

	}
	public void SetSinglePlayer(bool value)
	{
		isSinglePlayer = value;
	}
}
