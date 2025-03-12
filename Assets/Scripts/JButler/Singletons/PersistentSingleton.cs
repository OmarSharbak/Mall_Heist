using UnityEngine;

namespace JButler
{
    public class PersistentSingleton<T> : MonoBehaviour where T : Component
    {
        protected static T _inst;
        protected bool _enabled;

        public bool AutomaticallyUnparentOnAwake = true;

        /// <summary>
        /// Singleton design pattern.
        /// </summary>
        public static T Instance
        {
            get
            {
                if (_inst == null)
                {
                    _inst = FindObjectOfType<T>();
                    if (_inst == null)
                    {
                        GameObject obj = new GameObject();
                        _inst = obj.AddComponent<T>();
                    }
                }
                return _inst;
            }
        }

        /// <summary>
        /// On awake, we check if there's already a copy of the object in the scene. If there's one, we destroy it.
        /// </summary>
        protected virtual void Awake()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            if (AutomaticallyUnparentOnAwake)
            {
                this.transform.SetParent(null);
            }

            if (_inst == null)
            {
                // If I am the first instance, make me the Singleton.
                _inst = this as T;
                DontDestroyOnLoad(transform.gameObject);
                _enabled = true;
            }
            else
            {
                // If a Singleton already exists and you find another reference in scene, destroy it!
                if (this != _inst)
                {
                    Destroy(this.gameObject);
                }
            }
        }
    }
}