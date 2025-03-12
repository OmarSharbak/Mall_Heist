using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace RedLabsGames.Tools.QuickAnim
{
    [AddComponentMenu("Quick Anim/Quick Anim Unity Event")]
    [DisallowMultipleComponent]
    public class QuickAnimUnityEvent : MonoBehaviour
    {
        [System.Serializable]
        public class EventData
        {
            public string eventName;
            public UnityEvent onEvent;
        }

        // Editor purpose only
        [SerializeField][UnityEngine.Animations.NotKeyable]
        private int selectedIndex = -1;

        [SerializeField]
        private List<EventData> events = new List<EventData>();


        private void OnDestroy()
        {
            if (selectedIndex == 0) {
                // Workaround to fix variable not used warning...(Let me know if there any fixes)
            }
        }

        /// <summary>
        /// Invoke Event.
        /// </summary>
        /// <param name="_eventName">Event name to invoke</param>
        public void InvokeQuickAnimEvent(string _eventName)
        {
            EventData eventData = events.Find(f => f.eventName == _eventName);

            if (eventData == null) {
                Debug.LogError(_eventName + " event is not found");
                return;
            }

            eventData.onEvent.Invoke();
        }

        /// <summary>
        /// Get Unity Event with event name.
        /// </summary>
        /// <param name="_eventName"></param>
        /// <returns></returns>
        public UnityEvent GetUnityEvent(string _eventName)
        {
            EventData eventData = events.Find(f => f.eventName == _eventName);

            if (eventData == null) {
                Debug.LogError(_eventName + " event is not found");
                return null;
            }

            return eventData.onEvent;
        }

    }
}