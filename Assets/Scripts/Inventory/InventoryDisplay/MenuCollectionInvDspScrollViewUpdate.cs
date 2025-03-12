using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// Script idea from: https://forum.unity.com/threads/content-size-fitter-callback.292032/#post-3319787
/// </summary>
public class MenuCollectionInvDspScrollViewUpdate : UIBehaviour
{
    [SerializeField] private RectTransform targetParent;
    [SerializeField] private ContentSizeFitter scrollViewContentSizeFitter;

    protected override void Start()
    {
        StartCoroutine(RefreshScrollView());
    }

    protected override void OnRectTransformDimensionsChange()
    {
        base.OnRectTransformDimensionsChange();

        RectTransform rectTransform = this.GetComponent<RectTransform>();
        targetParent.sizeDelta = new Vector2(rectTransform.rect.width, rectTransform.rect.height);

        if (this.gameObject.activeInHierarchy)
            StartCoroutine(RefreshScrollView());
    }

    IEnumerator RefreshScrollView()
    {
        scrollViewContentSizeFitter.enabled = false;
        yield return 0;
        scrollViewContentSizeFitter.enabled = true;
    }
}