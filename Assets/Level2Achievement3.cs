using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level2Achievement3 : MonoBehaviour
{
    private void OnEnable()
    {
        TrapItem.OnHitBanana += OnHitBanana1;
    }

    private void OnDisable()
    {
        TrapItem.OnHitBanana -= OnHitBanana1;
    }

    private void OnHitBanana1()
    {
        LevelPerformanceManager.Instance.UnlockAchievement3();
    }
}
