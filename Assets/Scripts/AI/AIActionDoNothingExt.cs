using UnityEngine;
using MoreMountains.TopDownEngine;
using Spine;
using Spine.Unity;

public class AIActionDoNothingExt : AIActionDoNothing
{
    //------------------------------
    // Spine -----------------------
    public AnimationReferenceAsset IdleAnimation = null;

    protected SkeletonAnimation skeletonAnimation;

    private TrackEntry idleTrackEntry;
    //------------------------------
    //------------------------------


    //----------------------------------------------------------------
    public override void Initialization()
    {
        base.Initialization();

        skeletonAnimation = this.gameObject.GetComponentInChildren<SkeletonAnimation>();
    }


    //----------------------------------------------------------------
    protected virtual void SetAnimation()
    {
        if (IdleAnimation == null) return;

        skeletonAnimation.state.SetEmptyAnimations(0);
        idleTrackEntry = skeletonAnimation.state.SetAnimation(0, IdleAnimation, true);
        idleTrackEntry.TimeScale = 1;
        idleTrackEntry.MixDuration = 0.5f;
    }


    //----------------------------------------------------------------
    public override void OnEnterState()
    {
        base.OnEnterState();
        SetAnimation();
    }
}