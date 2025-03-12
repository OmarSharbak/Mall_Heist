using UnityEngine;

namespace JButler
{
    /// <summary>
    /// This is basically placed on a master folder that holds all the persistent singleton gameobjects for organization.
    /// </summary>
    public class DontDestroyOnLoad : MonoBehaviour
    {
        // Initialize.
        void Awake() => DontDestroyOnLoad(this.gameObject);
    }
}