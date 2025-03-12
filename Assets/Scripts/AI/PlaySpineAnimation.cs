using UnityEngine;
using MoreMountains.TopDownEngine;
using Spine;
using Spine.Unity;

public class PlaySpineAnimation : MonoBehaviour
{
    public AnimationReferenceAsset Animation = null;

    protected SkeletonAnimation skeletonAnimation;

    private TrackEntry trackEntry;


    //----------------------------------------------------------------
    private void Awake()
    {
        skeletonAnimation = this.gameObject.GetComponentInChildren<SkeletonAnimation>();
    }


    //----------------------------------------------------------------
    public void PlayAnimation()
    {
        skeletonAnimation.state.SetEmptyAnimations(0);
        trackEntry = skeletonAnimation.state.SetAnimation(0, Animation, true);
        trackEntry.TimeScale = 1;
        trackEntry.MixDuration = 0.5f;
    }
}