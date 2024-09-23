using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level2Achievement2 : MonoBehaviour
{
    int hitCount = 0;
    private void OnEnable()
    {
        MeleeWeapon.OnHitGuitar += IncreaseHitCount;
    }

    private void OnDisable()
    {
        MeleeWeapon.OnHitGuitar -= IncreaseHitCount;
    }

    private void IncreaseHitCount()
    {
        hitCount++;
        if(hitCount >= 3)
            LevelPerformanceManager.Instance.UnlockAchievement2();
    }
}
