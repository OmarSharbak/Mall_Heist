using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace RedLabsGames.Tools.QuickAnim
{

    public enum PlaybackMode
    {
        Loop, PingPong, Once
    }

    [AddComponentMenu("Quick Anim/Quick Anim Component")]
    [DisallowMultipleComponent]
   // [ExecuteInEditMode]
    public class QuickAnimComponent : MonoBehaviour
    {

        [System.Serializable]
        public class AnimData
        {
            public string name;
            public PlaybackMode type;
            public float speed = 1;

            private AnimationClip clip;
            public string clipData;

            [System.Serializable]
            public class Binding
            {
                public string path, propName, type;
                public AnimationCurve curve;
            }

            [System.Serializable]
            public class Event
            {
                public float time;
                public string funtionName;
                public string stringParam;
                public Object objRefParam;
                public float floatParam;
                public int intParam;
            }


            public List<Binding> bindings = new List<Binding>();

            public List<Event> events = new List<Event>();

            public AnimationClip GetClip()
            {
                clip = new AnimationClip();
                clip.name = name;
                clip.legacy = true;

                for (int i = 0; i < bindings.Count; i++)
                {
                    clip.SetCurve(bindings[i].path, System.Type.GetType(bindings[i].type), bindings[i].propName, bindings[i].curve);
                }

                List<AnimationEvent> animEvents = new List<AnimationEvent>();
                for (int i = 0; i < events.Count; i++)
                {
                    animEvents.Add(new AnimationEvent()
                    {
                        time = events[i].time,
                        floatParameter = events[i].floatParam,
                        intParameter = events[i].intParam,
                        stringParameter = events[i].stringParam,
                        objectReferenceParameter = events[i].objRefParam,
                        functionName = events[i].funtionName
                    });
                }


#if UNITY_EDITOR
                UnityEditor.AnimationUtility.SetAnimationEvents(clip, animEvents.ToArray());
#else
                clip.events = animEvents.ToArray();
#endif
                return clip;
            }

        }

        Animation anim;

        // Editor purpose only
        [SerializeField]
        private string lastEditedClip = "";

        // Editor purpose only
        [SerializeField][UnityEngine.Animations.NotKeyable]
        private int lastEditedClipIndex = -1;

        // Editor purpose only
        [SerializeField][UnityEngine.Animations.NotKeyable]
        private int first = 0;

       /// <summary>
       /// Overall speed multiplier for all clips.
       /// </summary>
        public float SpeedMultiplier { get => speedMultiplier; set => speedMultiplier = value; }

        /// <summary>
        /// Default CrossFade duration.
        /// </summary>
        public float DefaultCrossFadeDuration { get => defaultCrossFadeDuration; set => defaultCrossFadeDuration = value; }


        /// <summary>
        /// Name of the currently playing clip.
        /// </summary>
        public string CurrentlyPlayingClipName => GetPlayingClipName();

        /// <summary>
        /// Speed of the currently playing clip.
        /// </summary>
        public float CurrentlyPlayingClipSpeed { get => GetPlayingClipSpeed(); set => SetPlayingClipSpeed(value); }

        /// <summary>
        /// Mode of the currently playing clip (Eg: Loop, PingPong, Once).
        /// </summary>
        public PlaybackMode CurrentlyPlayingClipMode { get => GetPlayingClipMode(); set => SetPlayingClipMode(value); }

        /// <summary>
        /// Returns true if an animation is being played.
        /// </summary>
        public bool IsPlaying { get => (anim.isPlaying); }

        [SerializeField]
        private List<AnimData> anims = new List<AnimData>();

        private AnimData currentPlaying;

        [SerializeField]
        private float speedMultiplier=1;
        [SerializeField]
        private float defaultCrossFadeDuration=0.25f;

        private System.Action OnResetEvent =null;


        private void Awake()
        {

            anim = GetComponent<Animation>();

            if (anim == null) {
                anim = gameObject.AddComponent<Animation>();
            }
            anim.hideFlags = HideFlags.HideInInspector;


            for (int i = 0; i < anims.Count; i++) {
                anim.AddClip(anims[i].GetClip(), anims[i].name);
            }

        }

        void Start()
        {
            if (!Application.isPlaying)
            {
                return;
            }


            if (lastEditedClip=="" && lastEditedClipIndex == -1)
            {
                // Workaround to fix variable not used warning...(Let me know if there any fixes)
            }

            if (anims.Count > 0)
            {
                anim.clip = anims[first].GetClip();
                Play(anims[first].name);
            }
        }


        private void Reset()
        {
            OnResetEvent?.Invoke();
        }

        private void OnEnable()
        {
            anim = GetComponent<Animation>();
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                if (anim == null)
                {
                    anim = gameObject.AddComponent<Animation>();
                    anim.hideFlags = HideFlags.HideInInspector;
                }

                AnimationClip[] animClips = AnimationUtility.GetAnimationClips(anim.gameObject);

                for (int i = 0; i < animClips.Length; i++)
                {
                    anim.RemoveClip(animClips[i].name);
                }
                EditorApplication.RepaintAnimationWindow();
            }
#endif

            if (anim != null && Application.isPlaying)
            {
                anim.enabled = true;
                if (anims.Count > 0)
                {
                    if (anim[anims[first].name] != null)
                    {
                        Play(anims[first].name);
                    }
                }
            }
        }


        private void OnDisable()
        {
            currentPlaying = null;

            if (anim != null && Application.isPlaying)
            {
                anim.enabled = false;
            }
        }


        /// <summary>
        /// Returns the animation clip with name.
        /// </summary>
        /// <param name="clipName"></param>
        /// <returns></returns>
        public AnimationClip GetAnimationClip(string clipName)
        {
            AnimData data = anims.Where(a => a.name == clipName).FirstOrDefault();

            if (data == null)
            {
                Debug.LogError("Unable to find clip with name " + clipName, gameObject);
                return null;
            }

            return data.GetClip();
        }

        /// <summary>
        /// CrossFade to given clip name with default crossfade duration.
        /// </summary>
        /// <param name="clipName">Clip name to CrossFade</param>
        public void CrossFade(string clipName)
        {
           if (!IsValid()) {
                return;
            }

            AnimData data = anims.Where(a => a.name == clipName).FirstOrDefault();

            if (data == null) {
                Debug.LogError("Unable to find clip with name " + clipName, gameObject);
                return;
            }


            currentPlaying = data;
            anim.CrossFade(clipName, DefaultCrossFadeDuration);
        }


        /// <summary>
        /// CrossFade to given clip name with given duration.
        /// </summary>
        /// <param name="clipName">Clip Name to CrossFade.</param>
        /// <param name="time">Duration to CrossFade.</param>
        public void CrossFade(string clipName,float time)
        {
            if (!IsValid()) {
                return;
            }

            AnimData data = anims.Where(a => a.name == clipName).FirstOrDefault();

            if (data == null) {
                Debug.LogError("Unable to find clip with name " + clipName, gameObject);
                return;
            }

            currentPlaying = data;
            anim.CrossFade(clipName, time);
        }

        /// <summary>
        /// Play with clip name.
        /// </summary>
        /// <param name="clipName">Clip Name</param>
        public void Play(string clipName)
        {
            if (!IsValid()) {
                return;
            }

            AnimData data = anims.Where(a => a.name == clipName).FirstOrDefault();

            if (data == null) {
                Debug.LogError("Unable to find clip with name " + clipName, gameObject);
                return;
            }

            currentPlaying = data;
            anim.Play(clipName);
        }


        bool IsValid()
        {
            if (gameObject.activeSelf == false) {
                Debug.LogError("Quick Anim cannot be played when gameObject is not active.", gameObject);
                return false;
            }

            if (enabled == false) {
                Debug.LogError("Quick Anim cannot be played when component is not active.", gameObject);
                return false;
            }

            return true;
        }


        /// <summary>
        /// Returns name of the currently playing clip.
        /// </summary>
        /// <returns></returns>
        private string GetPlayingClipName()
        {
            AnimData data = currentPlaying;

            if (data == null)
            {
                throw new System.Exception("No animation is currently playing");
            }

            return data.name;
        }

        
        /// <summary>
        /// Set Speed of a clip with name.
        /// </summary>
        /// <param name="clipName"></param>
        /// <param name="speed"></param>
        public void SetClipSpeed(string clipName,float speed)
        {
            AnimData data = anims.Where(a => a.name == clipName).FirstOrDefault();

            if (data == null)
            {
                Debug.LogError("Unable to find clip with name " + clipName, gameObject);
                return;
            }

            data.speed = speed;
        }


        /// <summary>
        /// Returns Speed of a clip with name.
        /// </summary>
        /// <param name="clipName"></param>
        /// <param name="speed"></param>
        public float GetClipSpeed(string clipName)
        {
            AnimData data = anims.Where(a => a.name == clipName).FirstOrDefault();

            if (data == null)
            {
                Debug.LogError("Unable to find clip with name " + clipName, gameObject);
                return default;
            }

            return data.speed;
        }

        /// <summary>
        /// Set Speed of currently playing clip.
        /// </summary>
        /// <param name="clipName"></param>
        /// <param name="speed"></param>
        private void SetPlayingClipSpeed( float speed)
        {
            AnimData data = currentPlaying;

            if (data == null)
            {
                Debug.LogError("No animation is currently playing", gameObject);
                return;
            }

            data.speed = speed;
        }

        /// <summary>
        /// Returns Speed of currently playing clip.
        /// </summary>
        /// <param name="clipName"></param>
        /// <param name="speed"></param>
        private float GetPlayingClipSpeed()
        {
            AnimData data = currentPlaying;

            if (data == null)
            {
                Debug.LogError("No animation is currently playing", gameObject);
                return default;
            }

            return data.speed;
        }

        /// <summary>
        /// Set Mode of a clip with name (Eg: Loop, PingPong, Once).
        /// </summary>
        /// <param name="clipName"></param>
        /// <param name="speed"></param>
        public void SetClipMode(string clipName, PlaybackMode mode)
        {
            AnimData data = anims.Where(a => a.name == clipName).FirstOrDefault();

            if (data == null)
            {
                Debug.LogError("Unable to find clip with name " + clipName, gameObject);
                return;
            }

            data.type = mode;

            if (currentPlaying == data)
            {
                Play(currentPlaying.name);
            }
        }

        /// <summary>
        /// Returns Mode of a clip with name (Eg: Loop, PingPong, Once). 
        /// </summary>
        /// <param name="clipName"></param>
        /// <param name="speed"></param>
        public PlaybackMode GetClipMode(string clipName)
        {
            AnimData data = anims.Where(a => a.name == clipName).FirstOrDefault();

            if (data == null)
            {
                Debug.LogError("Unable to find clip with name " + clipName, gameObject);
                return default;
            }

            return data.type;
        }

        /// <summary>
        /// Set Mode of currently playing clip (Eg: Loop, PingPong, Once).
        /// </summary>
        /// <param name="clipName"></param>
        /// <param name="speed"></param>
        private void SetPlayingClipMode(PlaybackMode mode)
        {
            AnimData data = currentPlaying;

            if (data == null)
            {
                Debug.LogError("No animation is currently playing", gameObject);
                return;
            }

            data.type = mode;

            Play(currentPlaying.name);
        }


        /// <summary>
        /// Returns Mode of currently playing clip (Eg: Loop, PingPong, Once).
        /// </summary>
        /// <param name="clipName"></param>
        /// <param name="speed"></param>
        private PlaybackMode GetPlayingClipMode()
        {
            AnimData data = currentPlaying;

            if (data == null)
            {
                Debug.LogError("No animation is currently playing", gameObject);
                return default;
            }

            return data.type;
        }


        private void Update()
        {
            if (!Application.isPlaying)
            {
                return;
            }


            foreach (AnimationState item in anim)
            {
                AnimData data = anims.Where(a => a.name == item.name).FirstOrDefault();
                item.speed = data.speed * SpeedMultiplier;

                PlaybackMode type = data.type;

                switch (type)
                {
                    case PlaybackMode.Once:
                        item.wrapMode = WrapMode.Once;
                        break;
                    case PlaybackMode.Loop:
                        item.wrapMode = WrapMode.Loop;
                        break;
                    case PlaybackMode.PingPong:
                        item.wrapMode = WrapMode.PingPong;
                        break;
                    default:
                        break;
                }
            }
        }

    }

}
