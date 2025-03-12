using System.Reflection;
using UnityEngine;
using JayMountains;
using MoreMountains.Feedbacks;

public class PlayButtonSFX : MonoBehaviour
{
    [SerializeField] private string MMFPlayerName;

    public void PlayButtonSound()
    {
        ((MMF_Player)SoundManager.Instance.GetType().GetField(MMFPlayerName).GetValue(SoundManager.Instance)).PlayFeedbacks();
    }
}