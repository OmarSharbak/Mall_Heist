using UnityEngine;
using UnityEngine.EventSystems; // Required for event handling
using Febucci.UI; // Required if using Febucci Text Animator

public class TextAnimatorControl : MonoBehaviour, ISelectHandler, IDeselectHandler
{
    public TextAnimator_TMP textAnimator; // Assign your Text Animator Player component

    void Start()
    {
        textAnimator = GetComponent<TextAnimator_TMP>();
    }

    public void OnSelect(BaseEventData eventData)
    {
        // Enable the Text Animator when the button is selected
        textAnimator.Animate(Time.deltaTime);
    }

    public void OnDeselect(BaseEventData eventData)
    {
        // Disable the Text Animator when the button is deselected
        textAnimator.enabled = false;
    }
}
