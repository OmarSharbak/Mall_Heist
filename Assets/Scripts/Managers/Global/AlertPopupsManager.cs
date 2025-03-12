using UnityEngine;
using JButler;

public class AlertPopupsManager : PersistentSingleton<AlertPopupsManager>
{
    [SerializeField] private GameObject connectionErrorNoInternetAlert;
    [SerializeField] private GameObject blockInputLoadingPopup;
    [SerializeField] private GameObject blockInputTransparent;


    //----------------------------------------------------------------
    /// <summary>
    /// 
    /// </summary>
    public void ToggleNetworkErrorAlert(bool toggle)
    {
        connectionErrorNoInternetAlert.SetActive(toggle);
    }


    //----------------------------------------------------------------
    /// <summary>
    /// 
    /// </summary>
    public void BlockInput(bool toggle, bool loading = true)
    {
        if (loading) blockInputLoadingPopup.SetActive(toggle);
        else blockInputTransparent.SetActive(toggle);
    }
}