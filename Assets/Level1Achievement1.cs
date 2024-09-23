using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level1Achievement1 : MonoBehaviour
{
    public bool detected = false;
    public void Detected()
    {
        detected = true;
    }

    private void OnEnable()
    {
        EscalatorManager.OnLevelFinished += HandleLevelFinished;
    }

    private void OnDisable()
    {
        EscalatorManager.OnLevelFinished -= HandleLevelFinished;
    }

    private void HandleLevelFinished()
    {
        if(detected == false)
        {
            LevelPerformanceManager.Instance.UnlockAchievement1();
        }
    }
}
