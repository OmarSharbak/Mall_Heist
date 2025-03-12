using UnityEngine;

public class AudioManager : MonoBehaviour
{
    // Audio source component to play the sounds
    public AudioSource audioSource;

    // AudioClip references for different in-game events
    public AudioClip itemPickup;
    public AudioClip overridingSystems;
    public AudioClip itemThrow;
    public AudioClip objectiveComplete;
    // ... Add other audio clips as needed

    // Initialize the AudioSource component
    private void Start()
    {
        //audioSource = GetComponent<AudioSource>();
    }

    // Play the audio clip based on the event name
    public void PlayAudio(string eventName)
    {
        switch (eventName)
        {
            case "ItemPickup":
                PlayClipOnce(itemPickup);
                break;
            case "OverridingSystems":
                if (!audioSource.isPlaying || audioSource.clip != overridingSystems)
                {
                    PlayClipLooped(overridingSystems);
                }
                break;
            case "ItemThrow":
                PlayClipOnce(itemThrow);
                break;
            case "ObjectiveComplete":
                PlayClipOnce(objectiveComplete);
                break;
            default:
                Debug.LogWarning("Unknown audio event: " + eventName);
                break;
        }
    }

    // Play a given clip just once
    private bool isCurrentlyPlayingFootstep = false; // Flag to keep track of whether a footstep is currently playing

    public void PlayClipOnce(AudioClip clip, bool isFootstep = false)
    {
        if (audioSource != null)
        {
            // If it's a footstep sound and either no sound is playing or another footstep was playing
            if (isFootstep && (!audioSource.isPlaying || isCurrentlyPlayingFootstep))
            {
                audioSource.clip = clip;
                audioSource.Play();
                isCurrentlyPlayingFootstep = true; // Set the flag since a footstep is playing
            }
            else if (!isFootstep)
            {
                audioSource.Stop(); // Stop any currently playing sound, footstep or otherwise
                audioSource.clip = clip;
                audioSource.Play();
                isCurrentlyPlayingFootstep = false; // Reset the flag since a non-footstep sound is playing
            }
        }
    }

    // Play a given clip in a loop
    private void PlayClipLooped(AudioClip clip)
    {
        audioSource.loop = true;
        audioSource.clip = clip;
        audioSource.Play();
    }

    // Stop the currently playing audio
    public void StopAudio()
    {
        audioSource.Stop();
    }
}
