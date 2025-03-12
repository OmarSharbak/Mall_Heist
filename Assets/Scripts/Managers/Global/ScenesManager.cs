using UnityEngine.UI;
using JButler;
using MoreMountains.Tools;
using DG.Tweening;

public class ScenesManager : PersistentSingleton<ScenesManager>
{
    public SceneReference MainMenuScene;
    public Slider progressBar;

    public SceneReference GameRoomScene;


    //----------------------------------------------------------------
    /// <summary>
    /// Update the loading progess bar fill amount.
    /// </summary>
    public void ProgressBarUpdate(float amount)
    {
        if (DOTween.IsTweening(progressBar, true))
        {
            DOTween.Kill(progressBar, true);
            progressBar.DOValue(amount, 1.0f);
        }
        else
            progressBar.DOValue(amount, 1.0f);
    }


    //----------------------------------------------------------------
    public void ProceedToMainMenu()
    {
        // Use this if you want a transition animation in between.
        //MMSceneLoadingManager.LoadScene(MainMenuScene.SceneName);
        // Use this to load directly.
        MMSceneLoadingManager.LoadScene("", MainMenuScene.SceneName);
    }


    //----------------------------------------------------------------
    public void ProceedToGameRoomUSA()
    {
        MMSceneLoadingManager.LoadScene("", GameRoomScene.SceneName);
    }
}