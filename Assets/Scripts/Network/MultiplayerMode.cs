using UnityEngine;

public class MultiplayerMode : MonoBehaviour
{
	public bool isSinglePlayer = true;
	public int lvlIndex = 1;
	public bool isHost= true;
	public static MultiplayerMode Instance { get; private set; }

	public void Start()
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

	public void SetHost(bool value)
	{
		isHost = value;
	}
}
