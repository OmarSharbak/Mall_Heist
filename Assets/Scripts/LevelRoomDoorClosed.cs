using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class LevelRoomDoorClosed : MonoBehaviour
{
    public GameObject[] ClosedDoors;
    public GameObject[] OpenedDoors;

    [Button]
    public void SetDoorsClosed()
    {
        for (int i = 0; i < ClosedDoors.Length; i++)
        {
            ClosedDoors[i].SetActive(true);
        }
        for (int i = 0; i < OpenedDoors.Length; i++)
        {
            OpenedDoors[i].SetActive(false);
        }
    }

    [Button]
    public void SetDoorsOpened()
    {
        for (int i = 0; i < OpenedDoors.Length; i++)
        {
            OpenedDoors[i].SetActive(true);
        }
        for (int i = 0; i < ClosedDoors.Length; i++)
        {
            ClosedDoors[i].SetActive(false);
        }
    }
}