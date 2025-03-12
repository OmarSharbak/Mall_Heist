using Spine;
using Spine.Unity;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// By Nick (https://esotericsoftware.com/forum/d/17077-suggestion-make-default-skin-support-combination/6)
/// </summary>
[ExecuteInEditMode]
public class SpineSkinApplicator : MonoBehaviour
{
    public SkeletonAnimation targetSkeletonAnimation;

    public SkeletonGraphic targetSkeletonGraphic;

    public bool ignoreSkinNotFoundError = false;

    public Skeleton Skeleton => targetSkeletonGraphic != null
                        ? targetSkeletonGraphic.Skeleton
                        : targetSkeletonAnimation?.Skeleton;


#if UNITY_EDITOR
    public bool EditorSkipSkinSync
    {
        get => targetSkeletonAnimation != null ? targetSkeletonAnimation.EditorSkipSkinSync : false;
        set
        {
            if (targetSkeletonAnimation != null)
                targetSkeletonAnimation.EditorSkipSkinSync = value;

            // note: targetSkeletonGraphic Don't have EditorSkipSkinSync property.
        }
    }
#else
    public bool EditorSkipSkinSync { get; set; } = false;
#endif

    public enum AutomaticApplicationOption { None, OnStart }
    public AutomaticApplicationOption automaticApplyOption = AutomaticApplicationOption.OnStart;

    public enum SkinApplicationType { ReplaceSkin, AddToSkin }

    public SkinApplicationType applicationType;

    public bool replaceSkinApplyInEditMode = true;

    // note: attritbute disabled to support both SkeletonAnimation and SkeletonGraphic
    //[SpineSkin(dataField = "targetSkeletonAnimation")]
    public List<string> skinEntries = new List<string>(1);

    void Start()
    {
        var activate = Application.isPlaying
            ? automaticApplyOption == AutomaticApplicationOption.OnStart
            : applicationType == SkinApplicationType.ReplaceSkin && replaceSkinApplyInEditMode;

        if (activate)
        {
            Activate();
        }
    }


    private void OnValidate()
    {

        if (Application.isPlaying) { return; }

        BindSpineComponent();

        if (IsTargetReady)
        {
            if (applicationType == SkinApplicationType.ReplaceSkin && replaceSkinApplyInEditMode)
            {
                EditorSkipSkinSync = true;
                Activate();
            }
            else
            {
                EditorSkipSkinSync = false;
            }
        }
    }

    public void BindSpineComponent()
    {
        if (!IsTargetReady)
        {
            targetSkeletonAnimation = GetComponent<SkeletonAnimation>();

            if (!IsTargetReady)
                targetSkeletonGraphic = GetComponent<SkeletonGraphic>();
        }
    }

    private void OnDestroy()
    {
        EditorSkipSkinSync = false;
    }


    public void Activate()
    {
        if (IsTargetAndSkeletonValid)
        {
            switch (applicationType)
            {
                case SkinApplicationType.ReplaceSkin:
                    ApplyAsReplace();
                    break;
                case SkinApplicationType.AddToSkin:
                    ApplyAsAddToSkin();
                    break;
            }
        }
    }


    public void ApplyAsReplace()
    {
        Skin buildSkin = new Skin("buildSkin");

        foreach (var skinName in skinEntries)
        {
            var skinCur = Skeleton.Data.FindSkin(skinName);
            if (skinCur != null)
            {
                buildSkin.AddSkin(skinCur);
                //buildSkin.AddAttachments(skinCur); // note: old version API?
            }
            else
            {
                if (ignoreSkinNotFoundError == false)
                {
                    Debug.LogError("Error: skin not found, skinName: " + skinName, gameObject);
                }
            }
        }

        Skeleton.SetSkin(buildSkin);
        Skeleton.SetSlotsToSetupPose();
    }


    public void ApplyAsAddToSkin()
    {

        foreach (string skinName in skinEntries)
        {

            Skin newSkin = Skeleton.Data.FindSkin(skinName);
            if (newSkin != null)
            {
                Skin currentSkin = Skeleton.Skin;
                Skin combinedSkin = new Skin("combinedSkin");
                combinedSkin.AddSkin(currentSkin);
                combinedSkin.AddSkin(newSkin);
                Skeleton.SetSkin(combinedSkin);
                Skeleton.SetSlotsToSetupPose();
            }
            else
            {
                if (ignoreSkinNotFoundError == false)
                {
                    Debug.LogError("Error: skin not found, skinName: " + skinName, gameObject);
                }
            }
        }

    }

    public bool IsTargetReady => targetSkeletonAnimation != null || targetSkeletonGraphic != null;
    public bool IsTargetAndSkeletonValid => Skeleton != null;

}


#region [ ========== Inspector ========== ]
#if UNITY_EDITOR

[CustomEditor(typeof(SpineSkinApplicator))]
public class SpineSkinApplicator_Inspector : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        var inst = (SpineSkinApplicator)target;

        GUILayout.BeginVertical();
        {
            GUILayout.BeginHorizontal("box");
            {
                GUILayout.Label("Utility");

                GUILayout.FlexibleSpace();

                if (GUILayout.Button("Bind Spine Skeleton Component"))
                {
                    inst.BindSpineComponent();
                }
            }
            GUILayout.EndHorizontal();

            DrawSkinList(inst);
        }
        GUILayout.EndVertical();
    }


    enum SkinOp { None, Add, Remove }

    Vector2 scrollOffset = Vector2.zero;

    string skinOptionSearchPattern = string.Empty;
    int visibleSkinOptionsCount = 0;

    void DrawSkinList(SpineSkinApplicator inst)
    {
        if (!inst.IsTargetReady)
            return;

        GUILayout.BeginVertical("box");
        {
            GUILayout.BeginHorizontal("box");
            {
                var total = inst.Skeleton.Data.Skins.Count - 1;// -1 for "default" skin.
                GUILayout.Label($"Skins {visibleSkinOptionsCount} / {total}");
                GUILayout.FlexibleSpace();
                GUILayout.Label("Search");
                skinOptionSearchPattern = GUILayout.TextField(skinOptionSearchPattern, GUILayout.MinWidth(120));
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(4);

            scrollOffset = GUILayout.BeginScrollView(scrollOffset);
            {
                var skinOp = SkinOp.None;

                string targetSkin = string.Empty;

                visibleSkinOptionsCount = 0;

                foreach (var skin in inst.Skeleton.Data.Skins)
                {
                    if (skin.Name == "default")
                        continue;

                    if (skin.Name.IndexOf(skinOptionSearchPattern, System.StringComparison.OrdinalIgnoreCase) <= -1 && skinOptionSearchPattern.Length > 0)
                        continue;

                    visibleSkinOptionsCount++;

                    GUILayout.BeginHorizontal();
                    {
                        var selected = inst.skinEntries.Contains(skin.Name);

                        var newSelected = GUILayout.Toggle(selected, skin.Name);
                        if (newSelected != selected)
                        {
                            skinOp = newSelected ? SkinOp.Add : SkinOp.Remove;
                            targetSkin = skin.Name;
                        }
                    }
                    GUILayout.EndHorizontal();
                }

                switch (skinOp)
                {
                    case SkinOp.Add: inst.skinEntries.Add(targetSkin); break;
                    case SkinOp.Remove: inst.skinEntries.Remove(targetSkin); break;
                }

                EditorUtility.SetDirty(inst.gameObject);
                Undo.RecordObject(inst.gameObject, "Spine Skin Combination Updated");
            }
            GUILayout.EndScrollView();
        }
        GUILayout.EndVertical();
    }
}
#endif
#endregion [ ========== Inspector (End) ========== ]