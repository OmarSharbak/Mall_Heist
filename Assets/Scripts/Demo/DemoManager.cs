using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DemoManager : MonoBehaviour
{

	[SerializeField]
	private bool isDemo = false;
	[SerializeField]
	private int levelCap = 3;

	public static DemoManager Instance { get; private set; }

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

	public bool IsLevelCap(int level)
	{
		bool isDemoCap = false;
		if (isDemo)
			isDemoCap = level >= levelCap;

		return isDemoCap;
	}
}
