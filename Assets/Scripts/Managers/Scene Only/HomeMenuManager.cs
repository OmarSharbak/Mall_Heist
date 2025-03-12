using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JButler;

public class HomeMenuManager : Singleton<HomeMenuManager>
{
    // Panels from Right Buttons
    [SerializeField] private GameObject CharacterCustomizePanel;

    // Panels from Left Buttons
    [SerializeField] private GameObject SettingsPanel;
    [SerializeField] private GameObject BagInventoryPanel;
    [SerializeField] private GameObject GemsCollectionsPanel;
    [SerializeField] private GameObject QuestsPanel;

    // Panels from Hub Interactions
    [SerializeField] private GameObject ShopPanel;
    [SerializeField] private GameObject WorldMapPanel;

    /// <summary>
    /// Button click.
    /// </summary>
    public void ToggleBagInventoryPanel(bool toggle) => BagInventoryPanel.SetActive(toggle);

    /// <summary>
    /// Button click.
    /// </summary>
    public void ToggleCharacterCustomizePanel(bool toggle) => CharacterCustomizePanel.SetActive(toggle);

    /// <summary>
    /// Button click.
    /// </summary>
    public void ToggleShopPanel(bool toggle) => ShopPanel.SetActive(toggle);

    /// <summary>
    /// Button click.
    /// </summary>
    public void ToggleWorldMapPanel(bool toggle) => WorldMapPanel.SetActive(toggle);

    /// <summary>
    /// Button click.
    /// </summary>
    public void ToggleGemsCollectionsPanel(bool toggle) => GemsCollectionsPanel.SetActive(toggle);

    /// <summary>
    /// Button click.
    /// </summary>
    public void ToggleQuestsPanel(bool toggle) => QuestsPanel.SetActive(toggle);

    /// <summary>
    /// Button click.
    /// </summary>
    public void ToggleSettingsPanel(bool toggle) => SettingsPanel.SetActive(toggle);

    /// <summary>
    /// Button click.
    /// </summary>
    public void EnterGameRoomUSA() => ScenesManager.Instance.ProceedToGameRoomUSA();

    /// <summary>
    /// Button click.
    /// </summary>
    public void DebugPressed() => Debug.Log("Pressed");
}