using UnityEngine;

namespace JButler
{
    public class Singleton<T> : MonoBehaviour where T : Component
    {
        protected static T _inst;

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
        /// On awake, we initialize our instance. Make sure to call base.Awake() in override if you need awake.
        /// </summary>
        protected virtual void Awake()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            _inst = this as T;
        }
    }
}