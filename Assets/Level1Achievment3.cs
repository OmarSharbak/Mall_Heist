using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level1Achievement3 : MonoBehaviour
{
    public bool hit = false;
    public void Hit()
    {
        hit = true;
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
        if (hit == false)
        {
            LevelPerformanceManager.Instance.UnlockAchievement3();
        }
    }
}
