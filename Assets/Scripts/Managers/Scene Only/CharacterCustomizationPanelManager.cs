using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine;
using Spine.Unity;
using JayMountains;

public class CharacterCustomizationPanelManager : MonoBehaviour
{
    [SerializeField] private GameObject SelectMaleButton;
    [SerializeField] private GameObject SelectMaleButtonDisabled;
    [SerializeField] private GameObject SelectFemaleButton;
    [SerializeField] private GameObject SelectFemaleButtonDisabled;

    [SerializeField] private SkeletonGraphic MaleSkeletonAnimation;
    [SerializeField] private SkeletonGraphic FemaleSkeletonAnimation;

    [SerializeField] private AnimationReferenceAsset IdleAnimation;

    private TrackEntry maleIdleTrackEntry;
    private TrackEntry femaleIdleTrackEntry;


    //----------------------------------------------------------------
    private void OnEnable()
    {
        bool isPlayerCharacterMale = CustomizeCharacterManager.Instance.IsMale;

        SelectMaleButton.SetActive(!isPlayerCharacterMale);
        SelectMaleButtonDisabled.SetActive(isPlayerCharacterMale);
        SelectFemaleButton.SetActive(isPlayerCharacterMale);
        SelectFemaleButtonDisabled.SetActive(!isPlayerCharacterMale);

        if (isPlayerCharacterMale)
        {
            StartMaleAnimation();
            StopFemaleAnimation();
        }
        else
        {
            StopMaleAnimation();
            StartFemaleAnimation();
        }
    }


    //----------------------------------------------------------------
    /// <summary>
    /// Button click.
    /// </summary>
    public void SelectCharacterGender(bool isMale)
    {
        CustomizeCharacterManager.Instance.SetGender(isMale);

        SelectMaleButton.SetActive(!isMale);
        SelectMaleButtonDisabled.SetActive(isMale);
        SelectFemaleButton.SetActive(isMale);
        SelectFemaleButtonDisabled.SetActive(!isMale);

        if (isMale)
        {
            StartMaleAnimation();
            StopFemaleAnimation();
        }
        else
        {
            StopMaleAnimation();
            StartFemaleAnimation();
        }
    }


    //----------------------------------------------------------------
    private void StartMaleAnimation()
    {
        MaleSkeletonAnimation.color = new Color32(255, 255, 255, 255);
        maleIdleTrackEntry = MaleSkeletonAnimation.AnimationState.SetAnimation(0, IdleAnimation, true);
        maleIdleTrackEntry.TimeScale = 1;
    }


    //----------------------------------------------------------------
    private void StopMaleAnimation()
    {
        MaleSkeletonAnimation.color = new Color32(80, 80, 80, 255);
        MaleSkeletonAnimation.AnimationState.SetEmptyAnimations(0);
        if (maleIdleTrackEntry != null) maleIdleTrackEntry.TimeScale = 0;
    }


    //----------------------------------------------------------------
    private void StartFemaleAnimation()
    {
        FemaleSkeletonAnimation.color = new Color32(255, 255, 255, 255);
        femaleIdleTrackEntry = FemaleSkeletonAnimation.AnimationState.SetAnimation(0, IdleAnimation, true);
        femaleIdleTrackEntry.TimeScale = 1;
    }


    //----------------------------------------------------------------
    private void StopFemaleAnimation()
    {
        FemaleSkeletonAnimation.color = new Color32(80, 80, 80, 255);
        FemaleSkeletonAnimation.AnimationState.SetEmptyAnimations(0);
        if (femaleIdleTrackEntry != null) femaleIdleTrackEntry.TimeScale = 0;
    }


    //----------------------------------------------------------------
    /// <summary>
    /// Button click.
    /// </summary>
    public void UpdateSkin()
    {
        SpineTDE.Instance.SetSkin();
    }
}