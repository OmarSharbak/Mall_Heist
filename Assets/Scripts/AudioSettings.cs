using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using TMPro;

public class AudioSettings : MonoBehaviour
{
    [SerializeField] AudioMixer TargetMixer;

    [SerializeField] TMP_Text OverallVolumeLabel;
    [SerializeField] Slider OverallVolume;

    [SerializeField] TMP_Text MusicVolumeLabel;
    [SerializeField] Slider MusicVolume;

    [SerializeField] TMP_Text SFXVolumeLabel;
    [SerializeField] Slider SFXVolume;

    const float AudioVolumeUIOffset = 80f;

    AudioSource audioSource;

    bool firstTime = true;
    void Start()
    {
        audioSource = GetComponent<AudioSource>();

        // Load saved values or default to current mixer settings
        OverallVolume.value = PlayerPrefs.GetFloat("MasterVolume", GetMixerVolume("Master"));
        MusicVolume.value = PlayerPrefs.GetFloat("MusicVolume", GetMixerVolume("Music"));
        SFXVolume.value = PlayerPrefs.GetFloat("SFXVolume", GetMixerVolume("SFX"));

        // Update mixer volumes based on loaded values
        UpdateMixerVolume("Master", OverallVolume.value);
        UpdateMixerVolume("Music", MusicVolume.value);
        UpdateMixerVolume("SFX", SFXVolume.value);
    }

    public void OnOverallVolumeChanged(float newValue)
    {
        UpdateMixerVolume("Master", newValue);
        PlayerPrefs.SetFloat("MasterVolume", newValue);
    }

    public void OnMusicVolumeChanged(float newValue)
    {
        UpdateMixerVolume("Music", newValue);
        PlayerPrefs.SetFloat("MusicVolume", newValue);
    }

    public void OnSFXVolumeChanged(float newValue)
    {
        UpdateMixerVolume("SFX", newValue);
        PlayerPrefs.SetFloat("SFXVolume", newValue);
        if (firstTime == false)
        {
            audioSource.Play();
        }
        else
        {
            firstTime = false;
        }
            
    }

    private float GetMixerVolume(string parameterName)
    {
        float value;
        TargetMixer.GetFloat(parameterName, out value);
        return value;
    }

    private void UpdateMixerVolume(string parameterName, float newValue)
    {
        TargetMixer.SetFloat(parameterName, newValue);

        // Update UI label as well
        switch (parameterName)
        {
            case "Master":
                //OverallVolumeLabel.text = "Overall Volume [" + (AudioVolumeUIOffset + newValue).ToString("00") + "]";
                break;
            case "Music":
                //MusicVolumeLabel.text = "Music Volume [" + (AudioVolumeUIOffset + newValue).ToString("00") + "]";
                break;
            case "SFX":
                //SFXVolumeLabel.text = "Sound Effect Volume [" + (AudioVolumeUIOffset + newValue).ToString("00") + "]";
                break;
        }
    }

}