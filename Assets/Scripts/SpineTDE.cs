using UnityEngine;
using Spine;
using Spine.Unity;
using MoreMountains.TopDownEngine;
using MoreMountains.Tools;
using JayMountains;

public class SpineTDE : MMSingleton<SpineTDE>/*, MMEventListener<MMStateChangeEvent<CharacterStates.MovementStates>>*/
{
    public SpineSkinApplicator SpineSkinApplicator => spineSkinApplicator;
    [SerializeField] private SpineSkinApplicator spineSkinApplicator;

    public SkeletonAnimation SkeletonAnimation;
    public Animator Animator;

    public AnimationReferenceAsset IdleAnimation;
    public float IdleAnimationTimeScale = 1;
    public AnimationReferenceAsset WalkAnimation;
    public float WalkAnimationTimeScale = 1;
    public AnimationReferenceAsset AttackAnimation;
    public float AttackAnimationTimeScale = 1;

    private TrackEntry onIdleTrackEntry;
    private TrackEntry onWalkTrackEntry;
    private TrackEntry onAttackTrackEntry;

    private string activeAnimationParameter = "";

    private float taskTransitionDelay = 0;
    private float taskTransitionDelayTimer = 0;


    //----------------------------------------------------------------
    protected override void Awake()
    {
        base.Awake();
        spineSkinApplicator = this.GetComponentInChildren<SpineSkinApplicator>();
    }


    //----------------------------------------------------------------
    private void Update()
    {
        if (taskTransitionDelay == 0 || taskTransitionDelayTimer >= taskTransitionDelay)
        {
            if (Animator.GetBool("Slash1")) { SetAttackAnimation(); }
            else if (Animator.GetBool("Idle") && activeAnimationParameter != "Idle") { SetIdleAnimation(); }
            else if (Animator.GetBool("Walking") && activeAnimationParameter != "Walking") { SetWalkAnimation(); }
        }
        taskTransitionDelayTimer += Time.deltaTime;
    }


    //----------------------------------------------------------------
    private void SetIdleAnimation()
    {
        Debug.Log("Idle");
        activeAnimationParameter = "Idle";

        SkeletonAnimation.state.SetEmptyAnimations(0);
        onIdleTrackEntry = SkeletonAnimation.state.SetAnimation(0, IdleAnimation, true);
        onIdleTrackEntry.TimeScale = IdleAnimationTimeScale;
        onIdleTrackEntry.MixDuration = 0.2f;
    }


    //----------------------------------------------------------------
    private void SetWalkAnimation()
    {
        Debug.Log("Walking");
        activeAnimationParameter = "Walking";

        SkeletonAnimation.state.SetEmptyAnimations(0);
        onWalkTrackEntry = SkeletonAnimation.state.SetAnimation(0, WalkAnimation, true);
        onWalkTrackEntry.TimeScale = WalkAnimationTimeScale;
        onWalkTrackEntry.MixDuration = 0.2f;
    }


    //----------------------------------------------------------------
    private void SetAttackAnimation()
    {
        Debug.Log("Attack");
        activeAnimationParameter = "Slash1";

        SkeletonAnimation.state.SetEmptyAnimations(0);
        onAttackTrackEntry = SkeletonAnimation.state.SetAnimation(0, AttackAnimation, true);
        onAttackTrackEntry.TimeScale = AttackAnimationTimeScale;
        onAttackTrackEntry.MixDuration = 0.1f;

        taskTransitionDelay = SkeletonAnimation.Skeleton.Data.FindAnimation(AttackAnimation.name).Duration;
        taskTransitionDelayTimer = 0;
        Debug.Log(taskTransitionDelay);
    }


    //----------------------------------------------------------------
    public void SetSkin()
    {
        spineSkinApplicator.skinEntries.Clear();
        if (CustomizeCharacterManager.Instance.IsMale)
        {
            spineSkinApplicator.skinEntries.Add($"Full Skin Player");
        }
        else
        {
            spineSkinApplicator.skinEntries.Add($"Full Skin Player F");
        }
        spineSkinApplicator.Activate();
    }


    //----------------------------------------------------------------
    private void OnEnable()
    {
        SetSkin();
    }


    //----------------------------------------------------------------
    /// <summary>
    /// Catch events and do stuff.
    /// </summary>
    //public virtual void OnMMEvent(MMStateChangeEvent<CharacterStates.MovementStates> movementEvent)
    //{
    //    if (movementEvent.Target == this.gameObject)
    //    {
    //        Debug.Log($"state: {movementEvent.NewState}");
    //        switch (movementEvent.NewState)
    //        {
    //            case CharacterStates.MovementStates.Idle:
    //                //Debug.Log("Idle");
    //                SkeletonAnimation.state.SetEmptyAnimations(0);
    //                onIdleTrackEntry = SkeletonAnimation.state.SetAnimation(0, IdleAnimation, true);
    //                onIdleTrackEntry.TimeScale = IdleAnimationTimeScale;
    //                onIdleTrackEntry.MixDuration = 0.2f;
    //                break;

    //            case CharacterStates.MovementStates.Walking:
    //                //Debug.Log("Walk");
    //                SkeletonAnimation.state.SetEmptyAnimations(0);
    //                onWalkTrackEntry = SkeletonAnimation.state.SetAnimation(0, WalkAnimation, true);
    //                onWalkTrackEntry.TimeScale = WalkAnimationTimeScale;
    //                onWalkTrackEntry.MixDuration = 0.2f;
    //                break;
    //        }
    //    }
    //}


    //----------------------------------------------------------------
    /// <summary>
    /// On Enable, we start listening for MMStateChangeEvent<CharacterStates.MovementStates>.
    /// </summary>
    //protected virtual void OnEnable() => this.MMEventStartListening<MMStateChangeEvent<CharacterStates.MovementStates>>();


    //----------------------------------------------------------------
    /// <summary>
    /// On Disable, we stop listening for MMStateChangeEvent<CharacterStates.MovementStates>.
    /// </summary>
    //protected virtual void OnDisable() => this.MMEventStopListening<MMStateChangeEvent<CharacterStates.MovementStates>>();
}