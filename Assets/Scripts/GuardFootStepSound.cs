using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuardFootStepSound : MonoBehaviour
{
    public AudioClip[] FootstepAudioClips;
    public float audibleDistance = 100;
    public Transform playerTransform;
    

    [Range(0, 1)] public float FootstepAudioVolume = 0.5f;

    private void OnFootstep(AnimationEvent animationEvent)
    {
        if (animationEvent.animatorClipInfo.weight > 0.5f)
        {
            /*if (FootstepAudioClips.Length > 0)
            {
                var index = Random.Range(0, FootstepAudioClips.Length);
                float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

                if (distanceToPlayer < audibleDistance) // Replace 'audibleDistance' with the desired value
                {
                    //AudioSource.PlayClipAtPoint(FootstepAudioClips[index], transform.position, FootstepAudioVolume);
                }
            }*/
        }
    }
}
