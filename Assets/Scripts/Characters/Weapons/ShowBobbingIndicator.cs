using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JayMountains
{
    public class ShowBobbingIndicator : MonoBehaviour
    {
        private IdentifiedTarget _identifiedTarget = null;

        [SerializeField] private GameObject indicator;

        private void Awake()
        {
            _identifiedTarget = this.transform.GetComponent<IdentifiedTarget>();
            if (_identifiedTarget == null)
                Debug.LogError("No IdentifiedTarget component found on the game object.");

            if (indicator == null)
                Debug.LogError("No indicator attached to the game object.");

            HideIndicator();
        }

        private void OnEnable()
        {
            if (_identifiedTarget)
            {
                _identifiedTarget.OnIsTarget += ShowIndicator;
                _identifiedTarget.OnIsNotTarget += HideIndicator;
            }
        }

        private void OnDisable()
        {
            if (_identifiedTarget)
            {
                _identifiedTarget.OnIsTarget -= ShowIndicator;
                _identifiedTarget.OnIsNotTarget -= HideIndicator;
            }
        }

        private void ShowIndicator()
        {
            indicator.SetActive(true);
        }

        private void HideIndicator()
        {
            indicator.SetActive(false);
        }
    }
}