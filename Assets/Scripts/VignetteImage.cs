using UnityEngine;
using UnityEngine.UI;
using MoreMountains.Tools;
using MoreMountains.TopDownEngine;
using Sirenix.OdinInspector;
using JayMountains;

public class VignetteImage : MMSingleton<VignetteImage>, MMEventListener<MMCameraEvent>
{
    public float SCALE = 100;

    [SerializeField] private Image image;
    [SerializeField] private GameObject mask;

    [Button]
    public void SetVignette(bool toggle)
    {
        image.gameObject.SetActive(toggle);

        SCALE = 600 + (CollectionPerksBoostManager.Instance.TotalRadiusMineralHelmValue * 5);
        ExpandImage();
    }

    [Button]
    public void ExpandImage()
    {
        if (image.rectTransform.rect.width > image.rectTransform.rect.height)
        {
            float aspectRatio = image.rectTransform.rect.height / image.rectTransform.rect.width;
            float newHeight = SCALE * aspectRatio;
            image.rectTransform.sizeDelta = new Vector2(SCALE, newHeight);
        }
        else
        {
            float aspectRatio = image.rectTransform.rect.width / image.rectTransform.rect.height;
            float newWidth = SCALE * aspectRatio;
            image.rectTransform.sizeDelta = new Vector2(newWidth, SCALE);
        }

        mask.transform.localScale = new Vector3(CalculateMaskScale(SCALE), CalculateMaskScale(SCALE), 0);
    }

    //private float CalculateMaskScale(float vignetteScale)
    //{
    //    // The ratio constants.
    //    float vignetteScaleValue = 600f;
    //    float maskScaleValue = 300f;

    //    // Calculate the mask scale based on the ratio.
    //    float maskScale = (vignetteScale / vignetteScaleValue) * maskScaleValue;
    //    return maskScale;
    //}

    private float CalculateMaskScale(float vignetteScale)
    {
        // Ratio 1: [600 of 'a' is 300 of 'b']
        float vignetteScale1 = 600f;
        float maskScale1 = 300f;

        // Ratio 2: [1500 of 'a' is 800 of 'b']
        float vignetteScale2 = 1500f;
        float maskScale2 = 800f;

        // Calculate the interpolation factor (t) between the two ratios.
        float t = Mathf.InverseLerp(vignetteScale1, vignetteScale2, vignetteScale);

        // Interpolate between the two ratios.
        float maskScale = Mathf.Lerp(
            (vignetteScale / vignetteScale1) * maskScale1,
            (vignetteScale / vignetteScale2) * maskScale2,
            t
        );

        return maskScale;
    }

    public virtual void OnMMEvent(MMCameraEvent cameraEvent)
    {
        switch (cameraEvent.EventType)
        {
            case MMCameraEventTypes.SetTargetCharacter:
                this.GetComponent<MMFollowTarget>().Target = cameraEvent.TargetCharacter.transform;
                break;
        }
    }

    protected virtual void OnEnable()
    {
        this.MMEventStartListening<MMCameraEvent>();
        //SetVignette(true);
    }

    protected virtual void OnDisable()
    {
        this.MMEventStopListening<MMCameraEvent>();
    }
}