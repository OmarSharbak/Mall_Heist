using System.Collections;
using UnityEngine;
using MoreMountains.Tools;
using MoreMountains.Feedbacks;

namespace JayMountains
{
    public class SoundManager : MMSingleton<SoundManager>
    {
        public AudioClip MenuBGM;
        public int MenuBGMId;
        public AudioClip GameplayBGM;
        public int GameplayBGMId;

        public MMF_Player ButtonClickNormal;
        public MMF_Player ButtonClickClose;

        private AudioClip currentBGM;

        private void Start()
        {
            LoadMusic();
            LoadSFX();
        }

        public void LoadMusic()
        {
            int isMusicOn = PlayerPrefs.GetInt("MusicToggle", 1);
            if (isMusicOn == 1) MMSoundManagerTrackEvent.Trigger(MMSoundManagerTrackEventTypes.UnmuteTrack, MMSoundManager.MMSoundManagerTracks.Music);
            if (isMusicOn == 0) MMSoundManagerTrackEvent.Trigger(MMSoundManagerTrackEventTypes.MuteTrack, MMSoundManager.MMSoundManagerTracks.Music);
        }

        public void LoadSFX()
        {
            int isSFXOn = PlayerPrefs.GetInt("SFXToggle", 1);
            if (isSFXOn == 1) MMSoundManagerTrackEvent.Trigger(MMSoundManagerTrackEventTypes.UnmuteTrack, MMSoundManager.MMSoundManagerTracks.Sfx);
            if (isSFXOn == 0) MMSoundManagerTrackEvent.Trigger(MMSoundManagerTrackEventTypes.MuteTrack, MMSoundManager.MMSoundManagerTracks.Sfx);
        }

        public void PlayBGM(AudioClip audioClip)
        {
            if (currentBGM == null)
            {
                MMSoundManagerSoundPlayEvent.Trigger(audioClip, MMSoundManager.MMSoundManagerTracks.Music, this.transform.position, persistent: true, loop: true, ID: GetID(audioClip));
                currentBGM = audioClip;
            }
            else
            {
                MMSoundManagerSoundFadeEvent.Trigger(MMSoundManagerSoundFadeEvent.Modes.PlayFade, GetID(currentBGM), 1f, 0f, new MMTweenType(MMTween.MMTweenCurve.LinearTween));
                StartCoroutine(FreeSound(GetID(currentBGM), 1f));
                MMSoundManagerSoundPlayEvent.Trigger(audioClip, MMSoundManager.MMSoundManagerTracks.Music, this.transform.position, persistent: true, loop: true, ID: GetID(audioClip),
                    fade: true,
                    fadeDuration: 1f,
                    fadeInitialVolume: 0f,
                    volume: 1,
                    fadeTween: new MMTweenType(MMTween.MMTweenCurve.LinearTween));
                currentBGM = audioClip;
            }
        }

        private int GetID(AudioClip audioClip)
        {
            if (audioClip == MenuBGM) return MenuBGMId;
            if (audioClip == GameplayBGM) return GameplayBGMId;
            return -1;
        }

        private IEnumerator FreeSound(int soundID, float fadeDuration)
        {
            // Note: Have to use unscale as when leaving game room it pauses the game setting the time scale to 0 hence normal coroutines will not run.
            // https://discord.com/channels/504379597794574339/507151835388313600/1090669975569703035
            yield return MMCoroutine.WaitForUnscaled(fadeDuration);
            MMSoundManagerSoundControlEvent.Trigger(MMSoundManagerSoundControlEventTypes.Free, soundID);
        }
    }
}