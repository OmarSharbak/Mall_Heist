using UnityEngine;
using MoreMountains.TopDownEngine;
using Spine;
using Spine.Unity;

public class AIActionMoveTowardsTarget2DExt : AIActionMoveTowardsTarget2D
{
    //------------------------------
    // Spine -----------------------
    public AnimationReferenceAsset WalkAnimation = null;
    public float WalkAnimationSpeed = 1;

    protected SkeletonAnimation skeletonAnimation;

    private TrackEntry walkTrackEntry;
    //------------------------------
    //------------------------------

    //------------------------------
    // Character Movement ----------
    public bool UpdateMoveSpeed = false;
    public float MoveSpeed = 1;
    public bool RevertMoveSpeedOnExit = true;

    protected float initWalkSpeed;
    protected float initMovementSpeed;
    //------------------------------
    //------------------------------


    //----------------------------------------------------------------
    public override void Initialization()
    {
        base.Initialization();

        skeletonAnimation = this.gameObject.GetComponentInChildren<SkeletonAnimation>();

        initWalkSpeed = _characterMovement.WalkSpeed;
        initMovementSpeed = _characterMovement.MovementSpeed;
    }


    //----------------------------------------------------------------
    protected virtual void SetAnimation()
    {
        if (WalkAnimation == null) return;

        skeletonAnimation.state.SetEmptyAnimations(0);
        walkTrackEntry = skeletonAnimation.state.SetAnimation(0, WalkAnimation, true);
        walkTrackEntry.TimeScale = WalkAnimationSpeed;
        walkTrackEntry.MixDuration = 0.5f;
    }


    //----------------------------------------------------------------
    protected virtual void SetMoveSpeed()
    {
        if (!UpdateMoveSpeed) return;

        _characterMovement.WalkSpeed = MoveSpeed;
        _characterMovement.MovementSpeed = MoveSpeed;
    }


    //----------------------------------------------------------------
    protected virtual void RevertMoveSpeed()
    {
        if (!UpdateMoveSpeed) return;
        if (!RevertMoveSpeedOnExit) return;

        _characterMovement.WalkSpeed = initWalkSpeed;
        _characterMovement.MovementSpeed = initMovementSpeed;
    }


    //----------------------------------------------------------------
    public override void OnEnterState()
    {
        base.OnEnterState();
        SetAnimation();
        SetMoveSpeed();
    }


    //----------------------------------------------------------------
    public override void OnExitState()
    {
        base.OnExitState();
        RevertMoveSpeed();
    }
}