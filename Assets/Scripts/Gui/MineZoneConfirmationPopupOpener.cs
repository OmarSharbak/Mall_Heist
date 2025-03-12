using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MineZoneConfirmationPopupOpener : MonoBehaviour
{
    [SerializeField] private string region;
    [SerializeField] private MineZoneConfirmationPopup mineZoneConfirmationPopup;

    public void OpenPopup()
    {
        mineZoneConfirmationPopup.OpenConfirmation(region);
        mineZoneConfirmationPopup.gameObject.SetActive(true);
    }
}