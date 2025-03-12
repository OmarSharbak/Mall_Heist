using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using JButler;
using JayMountains;

public class GameplayMenuManager : Singleton<GameplayMenuManager>
{
    [SerializeField] private GameObject BagInventoryPanel;

    [SerializeField] private GameObject ButtonShoot;
    [SerializeField] private GameObject ButtonInteract;
    [SerializeField] private GameObject ButtonInteractIconCave;

    private void Start()
    {
        if (ScenesManager.Instance.MainMenuScene.SceneName == SceneManager.GetActiveScene().name)
            SoundManager.Instance.PlayBGM(SoundManager.Instance.MenuBGM);

        if (ScenesManager.Instance.GameRoomScene.SceneName == SceneManager.GetActiveScene().name)
            SoundManager.Instance.PlayBGM(SoundManager.Instance.GameplayBGM);
    }

    /// <summary>
    /// Button click.
    /// </summary>
    public void ToggleBagInventoryPanel(bool toggle) => BagInventoryPanel.SetActive(toggle);

    /// <summary>
    /// Button click.
    /// </summary>
    public void SetShootButton()
    {
        ToggleOffAllActionButtons();
        ButtonShoot.SetActive(true);
        Debug.Log("Shoot");
    }

    /// <summary>
    /// Button click.
    /// </summary>
    public void SetInteractButton()
    {
        ToggleOffAllActionButtons();
        ButtonInteract.SetActive(true);
        Debug.Log("Interact");
    }

    /// <summary>
    /// Button click.
    /// </summary>
    public void SetInteractIconCaveButton()
    {
        ToggleOffAllActionButtons();
        ButtonInteractIconCave.SetActive(true);
        Debug.Log("Interact");
    }

    private void ToggleOffAllActionButtons()
    {
        if (ButtonShoot) ButtonShoot.SetActive(false);
        if (ButtonInteract) ButtonInteract.SetActive(false);
        if (ButtonInteractIconCave) ButtonInteractIconCave.SetActive(false);
    }

    /// <summary>
    /// Button click.
    /// </summary>
    public void GoBackMenu()
    {
        ScenesManager.Instance.ProceedToMainMenu();
    }
}