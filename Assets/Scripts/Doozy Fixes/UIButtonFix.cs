using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using Doozy.Runtime.UIManager.Components;
using Sirenix.OdinInspector;

/// <summary>
/// Apparently Doozy's UIButton component don't have the OnRelease functionality.
/// </summary>
[RequireComponent(typeof(UIButton))]
public class UIButtonFix : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerUpHandler, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    // Run code on button up.
    [Serializable]
    public class ButtonUpEvent : UnityEvent { }
    [SerializeField]
    public ButtonUpEvent onUp = new ButtonUpEvent();

    [InfoBox("Image needs to have Read/Write enabled and Mesh Type as Full Rect.")]
    [SerializeField] bool ignoreTransparent = false;
    [SerializeField] Image[] imagesTransparency;

    private bool pointerEntered = false;
    private UIButton doozyUIButton;

    private ScrollRect scrollRectParent;
    private bool dragging = false;

    private void Awake() => Initialize();


    private void OnEnable() => Initialize();


    private void Initialize()
    {
        doozyUIButton = transform.GetComponent<UIButton>();

        scrollRectParent = GetComponentInParent<ScrollRect>();

        if (ignoreTransparent)
        {
            if (imagesTransparency.Length > 0)
            {
                for (int i = 0; i < imagesTransparency.Length; i++)
                {
                    imagesTransparency[i].alphaHitTestMinimumThreshold = 0.5f;
                }
            }
        }
    }


    // On button enter. =============================================
    public virtual void OnPointerEnter(PointerEventData eventData)
    {
        if (doozyUIButton.isActiveAndEnabled == false || doozyUIButton.interactable == false) { return; }
        if (eventData.button != PointerEventData.InputButton.Left) { return; }
        pointerEntered = true;
    }
    // ==============================================================


    // On button exit. ==============================================
    public virtual void OnPointerExit(PointerEventData eventData)
    {
        if (doozyUIButton.isActiveAndEnabled == false || doozyUIButton.interactable == false) { return; }
        if (eventData.button != PointerEventData.InputButton.Left) { return; }
        pointerEntered = false;
    }
    // ==============================================================


    // On button up. ================================================
    public virtual void OnPointerUp(PointerEventData eventData)
    {
        if (doozyUIButton.isActiveAndEnabled == false || doozyUIButton.interactable == false) { return; }
        if (eventData.button != PointerEventData.InputButton.Left) { return; }
        if (pointerEntered == true && !dragging)
        {
            onUp.Invoke();
        }
    }
    // ==============================================================


    // On drag. =====================================================
    public virtual void OnDrag(PointerEventData eventData)
    {
        if (scrollRectParent != null) scrollRectParent.OnDrag(eventData);
    }
    // ==============================================================


    // On begin drag. ===============================================
    public virtual void OnBeginDrag(PointerEventData eventData)
    {
        if (scrollRectParent != null)
        {
            scrollRectParent.OnBeginDrag(eventData);
            dragging = true;
        }
    }
    // ==============================================================


    // On end drag. =================================================
    public virtual void OnEndDrag(PointerEventData eventData)
    {
        if (scrollRectParent != null)
        {
            scrollRectParent.OnEndDrag(eventData);
            dragging = false;
        }
    }
    // ==============================================================
}