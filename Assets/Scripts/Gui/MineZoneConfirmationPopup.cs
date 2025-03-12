using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MineZoneConfirmationPopup : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI description;
    [SerializeField] private UIButtonFix button;

    public void OpenConfirmation(string region)
    {
        description.text = $"Enter the caves of {region}?";
        //button.onUp.AddListener(delegate
        //{
        //    button.onUp.RemoveAllListeners();
        //    button.onUp.AddListener(() => ScenesManager.Instance.ProceedToGameRoomUSA());
        //});
        button.onUp.RemoveAllListeners();
        button.onUp.AddListener(() => ScenesManager.Instance.ProceedToGameRoomUSA());
    }

    /// <summary>
    /// Button click.
    /// </summary>
    public void CloseConfirmation()
    {
        this.gameObject.SetActive(false);
    }
}