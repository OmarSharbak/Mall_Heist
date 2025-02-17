using UnityEngine;

public class FPSLimiter : MonoBehaviour
{
    public int maxFPS = 90;

	public static FPSLimiter Instance { get; private set; }

	[System.Obsolete]
    void Awake()
    {
		if (Instance != null && Instance != this)
		{
			Destroy(gameObject);
			return;
		}
		Instance = this;

		DontDestroyOnLoad(gameObject);
        Application.targetFrameRate = Screen.currentResolution.refreshRate;
        QualitySettings.vSyncCount = 0;
    }
}
