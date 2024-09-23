using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level1Achievement2 : MonoBehaviour
{
    public bool caught = false;

    private void OnEnable()
    {
        PlayerDamageHandler.OnPlayerCaught += HandlePlayerCaught;
        EscalatorManager.OnLevelFinished += HandleLevelFinished;
    }

    private void OnDisable()
    {
        PlayerDamageHandler.OnPlayerCaught -= HandlePlayerCaught;
        EscalatorManager.OnLevelFinished -= HandleLevelFinished;
    }

    private void HandlePlayerCaught()
    {
        caught = true;
    }

    private void HandleLevelFinished()
    {
        if (caught == false)
        {
            LevelPerformanceManager.Instance.UnlockAchievement2();
        }
    }
}
