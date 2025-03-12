using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JayMountains
{
    /// <summary>
    /// Works together with the IdentifyTarget component. <br />
    /// <i>Used with third party asset store packages:</i> <b>TopDown Engine</b>
    /// </summary>
    public class IdentifiedTarget : MonoBehaviour
    {
        // Is target delegate.
        public delegate void OnIsTargetDelegate();
        public OnIsTargetDelegate OnIsTarget;

        // Is not target delegate.
        public delegate void OnIsNotTargetDelegate();
        public OnIsNotTargetDelegate OnIsNotTarget;

        public void IsTarget()
        {
            Debug.Log($"{this.gameObject.name} is a target now.");
            OnIsTarget?.Invoke();
        }

        public void IsNotTarget()
        {
            Debug.Log($"{this.gameObject.name} is no longer a target.");
            OnIsNotTarget?.Invoke();
        }
    }
}