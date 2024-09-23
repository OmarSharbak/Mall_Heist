using UnityEngine;

public class FPSLimiter : MonoBehaviour
{
    public int maxFPS = 90;

    [System.Obsolete]
    void Awake()
    {
        DontDestroyOnLoad(gameObject);
        Application.targetFrameRate = Screen.currentResolution.refreshRate * 2;
        QualitySettings.vSyncCount = 0;
    }
}
