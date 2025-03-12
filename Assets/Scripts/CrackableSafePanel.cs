using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class CrackableSafePanel : MonoBehaviour
{
    [SerializeField] private Transform _dial;
    [SerializeField] private Transform _locksHolder;
    [SerializeField] private Transform _lockPrefab;
    [SerializeField] private Sprite _lockSprite;
    [SerializeField] private Sprite _unlockSprite;

    public Transform Dial { get => _dial; }

    public void CreateLocks(int count)
    {
        _locksHolder.ClearChildren(1);
        for (int i = 0; i < count; i++)
        {
            Transform safeLock = Instantiate(_lockPrefab, _locksHolder);
            safeLock.SetGameObjectActive(true);
        }
    }

    public void Unlock(int index)
    {
        _locksHolder.GetChild(index + 1).GetChild(0).GetComponent<Image>().sprite = _unlockSprite;
    }

    public void StartShakeTween(int index, float time = 1)
    {
        _locksHolder.GetChild(index + 1).GetChild(0).DOShakePosition(time, Vector3.right * 4);
    }

    public void StopShakeTween(int index)
    {
        _locksHolder.GetChild(index + 1).GetChild(0).DOComplete();
    }
}
