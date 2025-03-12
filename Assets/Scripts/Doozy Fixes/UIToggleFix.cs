using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using Doozy.Runtime.UIManager.Components;

/// <summary>
/// Apparently Doozy's UIToggle component don't have the OnRelease functionality.
/// </summary>
[RequireComponent(typeof(UIToggle))]
public class UIToggleFix : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerUpHandler
{
    // Run code on button up.
    [Serializable]
    public class ButtonUpEvent : UnityEvent { }
    [SerializeField]
    public ButtonUpEvent onUp = new ButtonUpEvent();

    private bool pointerEntered = false;
    private UIToggle doozyUIToggle;


    private void Awake()
    {
        doozyUIToggle = transform.GetComponent<UIToggle>();
    }


    // On button enter. =============================================
    public virtual void OnPointerEnter(PointerEventData eventData)
    {
        if (doozyUIToggle.isActiveAndEnabled == false) { return; }
        if (eventData.button != PointerEventData.InputButton.Left) { return; }
        pointerEntered = true;
    }
    // ==============================================================


    // On button exit. ==============================================
    public virtual void OnPointerExit(PointerEventData eventData)
    {
        if (doozyUIToggle.isActiveAndEnabled == false) { return; }
        if (eventData.button != PointerEventData.InputButton.Left) { return; }
        pointerEntered = false;
    }
    // ==============================================================


    // On button up. ================================================
    public virtual void OnPointerUp(PointerEventData eventData)
    {
        if (doozyUIToggle.isActiveAndEnabled == false) { return; }
        if (eventData.button != PointerEventData.InputButton.Left) { return; }
        if (pointerEntered == true)
        {
            onUp.Invoke();
        }
    }
    // ==============================================================
}