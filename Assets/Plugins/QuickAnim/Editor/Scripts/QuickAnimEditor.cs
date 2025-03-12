using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

using UnityEngine.UIElements;
using UnityEditor;

namespace RedLabsGames.Tools.QuickAnim
{
    [CustomEditor(typeof(QuickAnimComponent))]
    public class QuickAnimEditor : Editor
    {
        public static AnimationClip currentEditingClip;
        public static System.Action update;
        static Animation anim;
        static QuickAnimComponent component;
        static Vector2 mousePos;
        static QuickAnimEditor editor;
        Color iconColorPro = new Color(1, 1, 1, 1), iconColorNormal = new Color(0.2924528f, 0.2924528f, 0.2924528f, 1);
        VisualElement root;

        SerializedObject serialized;

        public override VisualElement CreateInspectorGUI()
        {
            Undo.undoRedoPerformed += ()=>
            {
                ReloadUI();
                if (GetField<int>(component, "lastEditedClipIndex") != -1) {
                    AnimationClip clip = component.GetAnimationClip(GetField<string>(component, "lastEditedClip"));

                    if(clip!=null && currentEditingClip != null) {
                        if (!CompareClips(currentEditingClip, clip)) {
                            EditClip(GetField<int>(component, "lastEditedClipIndex"));
                        }
                    }
                }

            };

            root = new VisualElement();

            root.RegisterCallback((MouseMoveEvent e) =>
            {
                mousePos = e.mousePosition;
            });

            ReloadUI();
            return root;
        }

        bool CompareClips(AnimationClip clipA,AnimationClip clipB)
        {
            EditorCurveBinding[] curveBindingsA = AnimationUtility.GetCurveBindings(clipA);
            EditorCurveBinding[] curveBindingsB = AnimationUtility.GetCurveBindings(clipB);

            if (curveBindingsA.Length != curveBindingsB.Length) {
                return false;
            }

            for (int i = 0; i < curveBindingsA.Length; i++) {
                if (curveBindingsA[i].propertyName != curveBindingsB[i].propertyName) {
                    return false;
                }
            }

            return true;
        }

        static object GetField(object source, string fieldName)
        {
            return source.GetType().GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(source);
        }

        static T GetField<T>(object source, string fieldName)
        {
            return (T)source.GetType().GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(source);
        }

        static void SetField(object source, string fieldName, object value)
        {
            source.GetType().GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(source, value);
        }

        static void UpdateClip()
        {
           

            if (component == null) {
                EditorApplication.update -= UpdateClip;
                return;
            }
            else {
                //Debug.Log("Refreshing : " + component.name);
            }

            if (component.enabled == false || component.gameObject.activeSelf == false) {
                return;
            }

            if (component == null || anim == null) {
                EditorApplication.update -= UpdateClip;
                return;
            }

            update?.Invoke();

            string clipName = GetField<string>(component, "lastEditedClip");

            if (clipName == "") {
                //Debug.Log("Null");
                return;
            }

            currentEditingClip = anim.GetClip(clipName);
            SerializedObject serializedObject=null;

            //Debug.Log(EditorUtility.IsDirty(currentEditingClip));

            if (currentEditingClip != null && EditorUtility.IsDirty(currentEditingClip) == true) {

                if (serializedObject == null) {
                    serializedObject = new SerializedObject(component);
                }

                List<QuickAnimComponent.AnimData> anims = GetField<List<QuickAnimComponent.AnimData>>(component, "anims");


               // Debug.Log("Save");
                EditorUtility.ClearDirty(currentEditingClip);
                List<QuickAnimComponent.AnimData.Binding> bindings = new List<QuickAnimComponent.AnimData.Binding>();

                EditorCurveBinding[] curveBindings = AnimationUtility.GetCurveBindings(currentEditingClip);

                serializedObject.FindProperty("anims").GetArrayElementAtIndex(GetField<int>(component, "lastEditedClipIndex")).FindPropertyRelative("bindings").arraySize = curveBindings.Length;

                for (int i = 0; i < curveBindings.Length; i++) {
                    AnimationCurve curve = AnimationUtility.GetEditorCurve(currentEditingClip, curveBindings[i]);
                    AnimationCurve curveNormal = AnimationUtility.GetEditorCurve(currentEditingClip, curveBindings[i]);
                    AnimationUtility.ConstrainToPolynomialCurve(curveNormal);
                    serializedObject.FindProperty("anims").GetArrayElementAtIndex(GetField<int>(component, "lastEditedClipIndex")).FindPropertyRelative("bindings").GetArrayElementAtIndex(i).FindPropertyRelative("path").stringValue = curveBindings[i].path;
                    serializedObject.FindProperty("anims").GetArrayElementAtIndex(GetField<int>(component, "lastEditedClipIndex")).FindPropertyRelative("bindings").GetArrayElementAtIndex(i).FindPropertyRelative("propName").stringValue = curveBindings[i].propertyName;
                    serializedObject.FindProperty("anims").GetArrayElementAtIndex(GetField<int>(component, "lastEditedClipIndex")).FindPropertyRelative("bindings").GetArrayElementAtIndex(i).FindPropertyRelative("type").stringValue = curveBindings[i].type.AssemblyQualifiedName;
                    serializedObject.FindProperty("anims").GetArrayElementAtIndex(GetField<int>(component, "lastEditedClipIndex")).FindPropertyRelative("bindings").GetArrayElementAtIndex(i).FindPropertyRelative("curve").animationCurveValue = curve;
                }

                List<QuickAnimComponent.AnimData.Event> events = new List<QuickAnimComponent.AnimData.Event>();

                AnimationEvent[] animEvents = AnimationUtility.GetAnimationEvents(currentEditingClip);
                serializedObject.FindProperty("anims").GetArrayElementAtIndex(GetField<int>(component, "lastEditedClipIndex")).FindPropertyRelative("events").arraySize = animEvents.Length;

                for (int i = 0; i < animEvents.Length; i++) {
                    serializedObject.FindProperty("anims").GetArrayElementAtIndex(GetField<int>(component, "lastEditedClipIndex")).FindPropertyRelative("events").GetArrayElementAtIndex(i).FindPropertyRelative("time").floatValue = animEvents[i].time;
                    serializedObject.FindProperty("anims").GetArrayElementAtIndex(GetField<int>(component, "lastEditedClipIndex")).FindPropertyRelative("events").GetArrayElementAtIndex(i).FindPropertyRelative("funtionName").stringValue = animEvents[i].functionName;
                    serializedObject.FindProperty("anims").GetArrayElementAtIndex(GetField<int>(component, "lastEditedClipIndex")).FindPropertyRelative("events").GetArrayElementAtIndex(i).FindPropertyRelative("stringParam").stringValue = animEvents[i].stringParameter;
                    serializedObject.FindProperty("anims").GetArrayElementAtIndex(GetField<int>(component, "lastEditedClipIndex")).FindPropertyRelative("events").GetArrayElementAtIndex(i).FindPropertyRelative("intParam").intValue = animEvents[i].intParameter;
                    serializedObject.FindProperty("anims").GetArrayElementAtIndex(GetField<int>(component, "lastEditedClipIndex")).FindPropertyRelative("events").GetArrayElementAtIndex(i).FindPropertyRelative("floatParam").floatValue = animEvents[i].floatParameter;
                    serializedObject.FindProperty("anims").GetArrayElementAtIndex(GetField<int>(component, "lastEditedClipIndex")).FindPropertyRelative("events").GetArrayElementAtIndex(i).FindPropertyRelative("objRefParam").objectReferenceValue = animEvents[i].objectReferenceParameter;
                }
                serializedObject.ApplyModifiedProperties();
                editor?.ReloadUI();
            }
        }

        void AddClip(string name)
        {
            serializedObject.FindProperty("anims").arraySize++;
            serializedObject.FindProperty("anims").GetArrayElementAtIndex(serializedObject.FindProperty("anims").arraySize - 1).FindPropertyRelative("name").stringValue = name;

            serializedObject.FindProperty("anims").GetArrayElementAtIndex(serializedObject.FindProperty("anims").arraySize - 1).FindPropertyRelative("bindings").arraySize = 0;
            serializedObject.FindProperty("anims").GetArrayElementAtIndex(serializedObject.FindProperty("anims").arraySize - 1).FindPropertyRelative("events").arraySize = 0;
            serializedObject.FindProperty("anims").GetArrayElementAtIndex(serializedObject.FindProperty("anims").arraySize - 1).FindPropertyRelative("speed").floatValue = 1;
            serializedObject.FindProperty("anims").GetArrayElementAtIndex(serializedObject.FindProperty("anims").arraySize - 1).FindPropertyRelative("type").enumValueIndex = 0;

            serializedObject.ApplyModifiedProperties();
        }

        void ClearClips()
        {
            //  Debug.Log("Clearing Clips");
            AnimationClip[] animClips = AnimationUtility.GetAnimationClips(anim.gameObject);

            for (int i = 0; i < animClips.Length; i++) {
                anim.RemoveClip(animClips[i].name);
            }
            EditorApplication.RepaintAnimationWindow();
        }

        void EditClip(int index)
        {
            List<QuickAnimComponent.AnimData> anims = GetField<List<QuickAnimComponent.AnimData>>(component, "anims");


            AnimationClip[] clips = AnimationUtility.GetAnimationClips(anim.gameObject);

            for (int i = 0; i < clips.Length; i++) {
                anim.RemoveClip(clips[i].name);
            }

            if (index == -1) {
                EditorApplication.ExecuteMenuItem("Window/Animation/Animation");
                return;
            }

            SetField(component, "lastEditedClipIndex", index);

            currentEditingClip = anims[index].GetClip();
            anim.AddClip(anims[index].GetClip(), anims[index].name);
            SetField(component, "lastEditedClip", currentEditingClip.name);
            EditorApplication.ExecuteMenuItem("Window/Animation/Animation");
        }

        void MakeDef(int index)
        {
            serializedObject.FindProperty("first").intValue = index;
            serializedObject.ApplyModifiedProperties();
        }

        private void OnDestroy()
        {
            if (component == null) {
                if (anim != null && !Application.isPlaying) {
                    DestroyImmediate(anim);
                    EditorApplication.RepaintAnimationWindow();
                    return;
                }
            }
        }

        private void OnDisable()
        {
            //EditorApplication.update -= UpdateClip;
        }

        SerializedObject GetSerializedObject()
        {
            int i = serializedObject.FindProperty("first").intValue;
            return serializedObject;
        }

        private void OnEnable()
        {

            component = (QuickAnimComponent)target;
            editor = this;

            anim = component.GetComponent<Animation>();

            if (anim == null)
            {
                anim = component.gameObject.AddComponent<Animation>();
            }


            anim.hideFlags = HideFlags.HideInInspector;

            update = null;


            EditorApplication.update -= UpdateClip;

            EditorApplication.update += UpdateClip;
            System.Action resetEvent = GetField<System.Action>(component, "OnResetEvent");
            resetEvent += () =>
            {
                AnimationClip[] animClips = AnimationUtility.GetAnimationClips(anim.gameObject);

                for (int i = 0; i < animClips.Length; i++)
                {
                    anim.RemoveClip(animClips[i].name);
                }

                EditorApplication.RepaintAnimationWindow();
                ReloadUI();
            };

        }


        public void ReloadUI()
        {
            root.Clear();

            var pm = PrefabUtility.GetPropertyModifications(target);
            if (pm == null) {
                pm = new PropertyModification[0];
            }

            List<QuickAnimComponent.AnimData> anims = GetField<List<QuickAnimComponent.AnimData>>(component, "anims");


            VisualElement header = new VisualElement()
            {
                style =
                {
                    flexDirection=FlexDirection.Row
                }
            };

            header.Add(new Label("Clips : ")
            {
                style =
                    {
                        unityTextAlign=TextAnchor.MiddleLeft,
                        flexGrow=1,
                        fontSize=20,
                        marginLeft=4,
                        marginTop=5,
                        marginBottom=8
                    }
            });

            root.Add(header);

            for (int i = 0; i < anims.Count; i++)
            {

                bool clipModified = pm.Any(f => f.propertyPath.StartsWith("anims.Array.data[" + i + "].bindings"));


                VisualElement vs = new VisualElement()
                {
                    userData = i,
                    style =
                    {
                        flexDirection = FlexDirection.Row,
                        backgroundColor = new Color(1,1,1,0.12f),
                        borderTopColor= new Color(1,1,1,0.2f),
                        borderBottomColor= new Color(1,1,1,0.1f),
                        borderBottomWidth=2,
                        borderTopWidth=2,
                        marginBottom=5,
                        paddingTop=4,
                        paddingBottom=4,
                        overflow = Overflow.Hidden,
                       
                    }
                };

                if (clipModified) {
                    VisualElement clipModifiedIndicator = new VisualElement()
                    {
                        style =
                        {
                            marginTop=-4,
                            marginBottom=-4,
                            marginRight=-5,
                            width=18,
                            borderLeftColor=new Color(0.05882353f,0.5058824f,0.7450981f),
                            borderLeftWidth=3
                        }
                    };
                    vs.Add(clipModifiedIndicator);
                    clipModifiedIndicator.RegisterCallback((MouseDownEvent e) =>
                    {
                        if (e.button == 1 && clipModified) {
                            GenericMenu menu = new GenericMenu();
                            menu.AddItem(new GUIContent("Apply Clip"), false, () =>
                            {
                                try {
                                    PrefabUtility.ApplyPropertyOverride(GetSerializedObject().FindProperty("anims").GetArrayElementAtIndex((int)(vs.userData)).FindPropertyRelative("bindings"), PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(target), InteractionMode.UserAction);
                                }
                                catch (System.Exception) {
                                }
                                EditClip((int)(vs.userData));
                                ReloadUI();
                            });
                            menu.AddItem(new GUIContent("Revert Clip"), false, () =>
                            {
                                PrefabUtility.RevertPropertyOverride(GetSerializedObject().FindProperty("anims").GetArrayElementAtIndex((int)(vs.userData)).FindPropertyRelative("bindings"), InteractionMode.UserAction);
                                EditClip((int)(vs.userData));
                                ReloadUI();
                            });
                            menu.ShowAsContext();
                        }
                    });
                }


                Color borderColor = EditorGUIUtility.isProSkin ? new Color(0.1882353f, 0.1882353f, 0.1882353f) : new Color(0.8313726f, 0.8313726f, 0.8313726f);


                string indicatorTooltip = "";

                indicatorTooltip = GetField<int>(component, "first") == i ? anims[(int)(vs.userData)].name + " will be played at the begining" : "Select to play " + anims[(int)(vs.userData)].name + " at the begining";

                vs.Add(new Button(() =>
                {
                    if (Application.isPlaying)
                    {
                        if(!string.IsNullOrEmpty(component.gameObject.scene.name)){
                            component.Play(anims[(int)(vs.userData)].name);
                            ReloadUI();
                        }
                    }
                    else
                    {
                        MakeDef((int)(vs.userData));
                        ReloadUI();
                    }
                })
                {
                    name = "indicator" + i,
                    tooltip = indicatorTooltip,
                    style =
                    {
                        width=16, height=16, alignSelf=Align.Center, marginLeft=5, borderBottomLeftRadius=30, borderBottomRightRadius=30, borderTopLeftRadius=30, borderTopRightRadius=30, borderLeftWidth=1, borderRightWidth=1, borderBottomWidth=1, borderTopWidth=1, borderLeftColor=borderColor, borderRightColor=borderColor, borderTopColor=borderColor, borderBottomColor=borderColor, backgroundColor = GetField<int>(component,"first") == i ? (EditorGUIUtility.isProSkin?new Color(0.5943396f, 0.5943396f, 0.5943396f):new Color(0.95f,0.95f,0.95f)) : (EditorGUIUtility.isProSkin? new Color(0.3647059f, 0.3647059f, 0.3647059f):new Color(0.7f,0.7f,0.7f))
                    }
                });



                if (Application.isPlaying)
                {
                    update += () =>
                    {
                        try
                        {
                            Color c = component.CurrentlyPlayingClipName == anims[(int)(vs.userData)].name ? Color.green : new Color(0.3647059f, 0.3647059f, 0.3647059f);

                            vs.Q("indicator" + (vs.userData).ToString()).style.backgroundColor = c;
                        }
                        catch (System.Exception)
                        {
                        }
                        
                    };
                }

                bool clipNameModified = pm.Any(f => f.propertyPath == "anims.Array.data[" + i + "].name");

                Label clipLabel = new Label(anims[i].name)
                {
                    name = "ClipName",
                    style =
                    {
                        unityTextAlign=TextAnchor.MiddleLeft,
                        flexGrow=Application.isPlaying?0:1,
                        marginLeft=1,
                        marginRight =Application.isPlaying?8:0,
                        paddingLeft=clipNameModified?2:0,
                        borderLeftColor=new Color(0.05882353f,0.5058824f,0.7450981f),
                        borderLeftWidth=clipNameModified?2:0,
                    }
                };

                vs.Add(clipLabel);



                clipLabel.RegisterCallback((MouseDownEvent e) =>
                {
                    if (e.button == 0)
                    {
                        if (Application.isPlaying) {
                            if (!string.IsNullOrEmpty(component.gameObject.scene.name)) {
                                component.Play(anims[(int)(vs.userData)].name);
                                ReloadUI();
                            }
                        }
                        else {
                            MakeDef((int)(vs.userData));
                            ReloadUI();
                        }
                    }

                    if (e.button == 1 && clipNameModified) {
                        GenericMenu menu = new GenericMenu();
                        menu.AddItem(new GUIContent("Apply Clip Name"), false, () =>
                        {
                            try {
                                PrefabUtility.ApplyPropertyOverride(GetSerializedObject().FindProperty("anims").GetArrayElementAtIndex((int)(vs.userData)).FindPropertyRelative("name"), PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(target), InteractionMode.UserAction);
                                ReloadUI();
                            }
                            catch (System.Exception) {
                            }
                        });
                        menu.AddItem(new GUIContent("Revert Clip Name"), false, () =>
                        {
                            PrefabUtility.RevertPropertyOverride(GetSerializedObject().FindProperty("anims").GetArrayElementAtIndex((int)(vs.userData)).FindPropertyRelative("name"), InteractionMode.UserAction);
                            ReloadUI();
                        });
                        menu.ShowAsContext();
                    }
                });

                if (Application.isPlaying)
                {
                    UnityEngine.UIElements.ProgressBar bar = new UnityEngine.UIElements.ProgressBar();

                    bar.style.flexGrow = 1;
                    bar.ElementAt(0).style.flexGrow = 1;
                    bar.ElementAt(0).ElementAt(0).style.flexGrow = 1;

                    int times = 0;

                    update += () =>
                    {
                        if (anim[anims[(int)(vs.userData)].name] != null)
                        {
                            float value = anim[anims[(int)(vs.userData)].name].normalizedTime * 100;

                            times = (int)(value / 100);

                            value -= 100 * times;

                            if (value < 0)
                            {
                                value = 100 + value;
                            }

                            bar.title = Mathf.Round(Mathf.Abs(value)).ToString() + " %";
                            bar.SetValueWithoutNotify(value);
                        }
                    };

                    vs.Add(bar);

                    if (!string.IsNullOrEmpty(component.gameObject.scene.name))
                    bar.RegisterCallback((MouseDownEvent m) =>
                    {
                        float currentSpeed = anims[(int)(vs.userData)].speed;


                        FloatingWindow window = ScriptableObject.CreateInstance<FloatingWindow>();

                        window.autoClose = true;
                        window.size = new Vector2(300, 70);
                        window.pos = mousePos;

                        VisualElement windowVS = new VisualElement();

                        UnityEngine.UIElements.Slider slider = new UnityEngine.UIElements.Slider(0, 0.99999f);
                        slider.value = anim[anims[(int)(vs.userData)].name].normalizedTime;
                        slider.value -= times;

                        slider.RegisterValueChangedCallback((ChangeEvent<float> e) =>
                        {
                            anim[anims[(int)(vs.userData)].name].normalizedTime = e.newValue;
                        });

                        windowVS.Add(slider);

                        windowVS.Add(new Label("Slide to control the animation")
                        {
                            name = "tile",
                            style =
                            {
                                flexGrow=1,
                                height=30,
                                unityTextAlign=TextAnchor.MiddleCenter
                            }
                        });

                        window.ShowWindow(windowVS);
                        anims[(int)(vs.userData)].speed = 0;

                        window.OnClosed += () =>
                        {
                            anims[(int)(vs.userData)].speed = currentSpeed;
                        };
                    });
                }


                bool speedModified = pm.Any(f => f.propertyPath == "anims.Array.data[" + i + "].speed");

                UnityEngine.UIElements.FloatField speedField = new UnityEngine.UIElements.FloatField(" ")
                {
                    tooltip = "Playback Speed",
                    isDelayed=true,
                    style =
                    {
                        marginRight=5
                    }
                };


                speedField.ElementAt(0).Add(new Image()
                {
                    name="speedIcon",
                    image = Resources.Load<Texture>("QuickAnimIcons/Time"),
                    tintColor = EditorGUIUtility.isProSkin ? iconColorPro : iconColorNormal,
                    pickingMode =PickingMode.Ignore,
                    style =
                    {
                        width=20,
                        height=20,
                        marginRight=3,
                        marginBottom=3,
                        alignSelf=Align.Center,
                        borderLeftColor=new Color(0.05882353f,0.5058824f,0.7450981f),
                        borderLeftWidth=speedModified?2:0,
                    }
                });

                speedField.Q(null,"unity-label").RegisterCallback((MouseDownEvent e) =>
                {
                    if (e.button == 1 && speedModified) {
                        GenericMenu menu = new GenericMenu();
                        menu.AddItem(new GUIContent("Apply Speed"), false, () =>
                        {
                            try {
                                PrefabUtility.ApplyPropertyOverride(GetSerializedObject().FindProperty("anims").GetArrayElementAtIndex((int)(vs.userData)).FindPropertyRelative("speed"), PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(target), InteractionMode.UserAction);
                                ReloadUI();
                            }
                            catch (System.Exception) {
                            }
                        });
                        menu.AddItem(new GUIContent("Revert Speed"), false, () =>
                        {
                            PrefabUtility.RevertPropertyOverride(GetSerializedObject().FindProperty("anims").GetArrayElementAtIndex((int)(vs.userData)).FindPropertyRelative("speed"), InteractionMode.UserAction);
                            ReloadUI();
                        });
                        menu.ShowAsContext();
                    }
                });

                speedField.ElementAt(0).style.minWidth=20;

                speedField.ElementAt(1).style.unityTextAlign = TextAnchor.MiddleCenter;

                speedField.ElementAt(1).style.minWidth = 30;
                speedField.ElementAt(1).style.maxWidth = 30;


                speedField.value = anims[i].speed;

                speedField.RegisterValueChangedCallback((ChangeEvent<float> e) =>
                {
                    serializedObject.FindProperty("anims").GetArrayElementAtIndex((int)(vs.userData)).FindPropertyRelative("speed").floatValue = e.newValue;
                    serializedObject.ApplyModifiedProperties();
                    ReloadUI();
                });


                vs.Add(speedField);

                bool modeModified = pm.Any(f => f.propertyPath == "anims.Array.data[" + i + "].type");

                vs.Add(new Image()
                {
                    name="modeIcon",
                    image = Resources.Load<Texture>("QuickAnimIcons/" + anims[(int)(vs.userData)].type.ToString()),
                    tintColor = EditorGUIUtility.isProSkin ? iconColorPro : iconColorNormal,
                    tooltip = "Playback Mode : " + anims[(int)(vs.userData)].type.ToString(),
                    style =
                    {
                        flexShrink=0,
                        width=20,
                        height=20,
                        alignSelf=Align.Center,
                        borderLeftColor=new Color(0.05882353f,0.5058824f,0.7450981f),
                        borderLeftWidth=modeModified?2:0,
                    }
                });

                vs.Q("modeIcon").RegisterCallback((MouseDownEvent e) =>
                {
                    if (e.button == 1 && modeModified) {
                        GenericMenu menu = new GenericMenu();
                        menu.AddItem(new GUIContent("Apply Playback Mode"), false, () =>
                        {
                            try {
                                PrefabUtility.ApplyPropertyOverride(GetSerializedObject().FindProperty("anims").GetArrayElementAtIndex((int)(vs.userData)).FindPropertyRelative("type"), PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(target), InteractionMode.UserAction);
                            }
                            catch (System.Exception) {
                            }

                            ReloadUI();

                        });
                        menu.AddItem(new GUIContent("Revert Playback Mode"), false, () =>
                        {
                            PrefabUtility.RevertPropertyOverride(GetSerializedObject().FindProperty("anims").GetArrayElementAtIndex((int)(vs.userData)).FindPropertyRelative("type"), InteractionMode.UserAction);
                            ReloadUI();
                        });
                        menu.ShowAsContext();
                    }
                });

                UnityEngine.UIElements.EnumField animType = new UnityEngine.UIElements.EnumField(anims[i].type)
                {
                    tooltip = "Playback Mode",
                };
                animType.style.minWidth = 90;
                animType.style.maxWidth = 90;


                animType.RegisterValueChangedCallback((ChangeEvent<System.Enum> e) =>
                {
                    serializedObject.FindProperty("anims").GetArrayElementAtIndex((int)(vs.userData)).FindPropertyRelative("type").enumValueIndex = (int)((PlaybackMode)e.newValue);
                    serializedObject.ApplyModifiedProperties();

                    if (Application.isPlaying)
                    {
                        if (component.CurrentlyPlayingClipName == anims[(int)(vs.userData)].name)
                        {
                            component.Play(anims[(int)(vs.userData)].name);
                            anim[anims[(int)(vs.userData)].name].normalizedTime = 0;
                        }
                    }
                    ReloadUI();
                });


                vs.Add(animType);

                if (!Application.isPlaying)
                {
                    vs.Add(new Button(() =>
                    {
                        EditClip((int)(vs.userData));
                    })
                    {
                        name="Edit",
                        tooltip="Edit Clip"
                    });

                    vs.Q("Edit").Add(new Image()
                    {
                        image = Resources.Load<Texture>("QuickAnimIcons/Edit"),
                        tintColor = EditorGUIUtility.isProSkin ? iconColorPro : iconColorNormal,
                        pickingMode = PickingMode.Ignore,
                        style =
                        {
                            flexGrow=1,
                            width=18,
                            height=18,
                            alignSelf=Align.Center,
                        }
                    });

                    vs.Add(new Button(() =>
                    {

                        GenericMenu menu = new GenericMenu();

                        menu.AddItem(new GUIContent("Rename"), false, () =>
                        {
                            FloatingWindow window = ScriptableObject.CreateInstance<FloatingWindow>();
                            window.autoClose = true;
                            window.size = new Vector2(300, 70);
                            window.pos = mousePos;

                            VisualElement windowVS = new VisualElement();


                            TextField textField = new TextField("Clip Name");

                            textField.value = anims[(int)(vs.userData)].name;

                            windowVS.Add(textField);

                            window.ShowWindow(windowVS);

                            textField.Focus();
                            textField.Q("unity-text-input").Focus();


                            textField.RegisterCallback((KeyDownEvent e) =>
                            {
                                if (e.keyCode == KeyCode.Return || e.keyCode == KeyCode.KeypadEnter) {
                                    window.Close();
                                    RenameClip((int)(vs.userData), textField.text);
                                    ReloadUI();
                                }
                            });

                            windowVS.Add(new Button(() =>
                            {
                                window.Close();
                                RenameClip((int)(vs.userData), textField.text);
                                ReloadUI();
                            })
                            {
                                text = "Rename",
                                style =
                        {
                            marginTop=10
                        }
                            });
                        });


                        menu.AddItem(new GUIContent("Save Clip"), false, () =>
                        {
                            SaveClip((int)(vs.userData));
                        });

                        menu.AddItem(new GUIContent("Load Clip"), false, () =>
                        {
                            LoadClip((int)(vs.userData));
                        });

                        menu.ShowAsContext();
                        
                    })
                    {
                        name="Options",
                        tooltip = "Options",
                    });

                    vs.Q("Options").Add(new Image()
                    {
                        image = Resources.Load<Texture>("QuickAnimIcons/Rename"),
                        tintColor = EditorGUIUtility.isProSkin ? iconColorPro : iconColorNormal,
                        pickingMode = PickingMode.Ignore,
                        style =
                        {
                            flexGrow=1,
                            width=18,
                            height=18,
                            alignSelf=Align.Center,
                        }
                    });

                    vs.Add(new Button(() =>
                    {
                        RemoveClip((int)(vs.userData));
                        ReloadUI();
                    })
                    {
                        name="Remove",
                        tooltip = "Remove",
                    });

                    vs.Q("Remove").Add(new Image()
                    {
                        image = Resources.Load<Texture>("QuickAnimIcons/Delete"),
                        tintColor = EditorGUIUtility.isProSkin ? iconColorPro : iconColorNormal,
                        pickingMode = PickingMode.Ignore,
                        style =
                        {
                            flexGrow=1,
                            width=18,
                            height=18,
                            alignSelf=Align.Center,
                        }
                    });

                }

                root.Add(vs);
            }

            if (!Application.isPlaying)
            root.Add(new Button(() =>
            {

                FloatingWindow window = ScriptableObject.CreateInstance<FloatingWindow>();

                window.autoClose = true;
                window.size = new Vector2(300, 70);
                window.pos = mousePos;

                VisualElement vs = new VisualElement();


                TextField textField = new TextField("Clip Name");

                textField.value = "Clip" + anims.Count;

                vs.Add(textField);

                window.ShowWindow(vs);

                textField.Focus();
                textField.Q("unity-text-input").Focus();


                textField.RegisterCallback((KeyDownEvent e) =>
                {
                    if (e.keyCode == KeyCode.Return || e.keyCode == KeyCode.KeypadEnter)
                    {
                        window.Close();
                        AddClip(textField.text);
                        ReloadUI();
                    }
                });


                vs.Add(new Button(() =>
                {
                    window.Close();
                    AddClip(textField.text);
                    ReloadUI();
                })
                {
                    text = "Add",
                    style =
                {
                    marginTop=10
                }
                });
            })
            {
                text = "Add Clip",
                style =
                {
                    alignSelf = Align.FlexEnd,
                    height=30,
                    width=100,
                }
            });

        }

        void SaveClip(int index)
        {
            List<QuickAnimComponent.AnimData> anims = GetField<List<QuickAnimComponent.AnimData>>(component, "anims");
            AnimationClip clip = anims[index].GetClip();

            string path = EditorUtility.SaveFilePanelInProject("Save Animation Clip", clip.name, "anim", "Save Clip");

            if (!string.IsNullOrEmpty(path)) {
                AssetDatabase.CreateAsset(clip, path);
                AssetDatabase.Refresh();
            }
        }

        void LoadClip(int index)
        {
            string path = EditorUtility.OpenFilePanel("Load Animation Clip","", "anim");

            if (!string.IsNullOrEmpty(path)) {
                path = path.Replace(Application.dataPath.Replace("Assets",""), "");
                AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);

                if (clip != null) {
                    EditClip(index);


                    EditorCurveBinding[] curveBindings = AnimationUtility.GetCurveBindings(clip);

                    serializedObject.FindProperty("anims").GetArrayElementAtIndex(GetField<int>(component, "lastEditedClipIndex")).FindPropertyRelative("bindings").arraySize = curveBindings.Length;

                    for (int i = 0; i < curveBindings.Length; i++) {
                        AnimationCurve curve = AnimationUtility.GetEditorCurve(clip, curveBindings[i]);
                        AnimationCurve curveNormal = AnimationUtility.GetEditorCurve(clip, curveBindings[i]);
                        AnimationUtility.ConstrainToPolynomialCurve(curveNormal);
                        serializedObject.FindProperty("anims").GetArrayElementAtIndex(GetField<int>(component, "lastEditedClipIndex")).FindPropertyRelative("bindings").GetArrayElementAtIndex(i).FindPropertyRelative("path").stringValue = curveBindings[i].path;
                        serializedObject.FindProperty("anims").GetArrayElementAtIndex(GetField<int>(component, "lastEditedClipIndex")).FindPropertyRelative("bindings").GetArrayElementAtIndex(i).FindPropertyRelative("propName").stringValue = curveBindings[i].propertyName;
                        serializedObject.FindProperty("anims").GetArrayElementAtIndex(GetField<int>(component, "lastEditedClipIndex")).FindPropertyRelative("bindings").GetArrayElementAtIndex(i).FindPropertyRelative("type").stringValue = curveBindings[i].type.AssemblyQualifiedName;
                        serializedObject.FindProperty("anims").GetArrayElementAtIndex(GetField<int>(component, "lastEditedClipIndex")).FindPropertyRelative("bindings").GetArrayElementAtIndex(i).FindPropertyRelative("curve").animationCurveValue = curve;
                    }

                    List<QuickAnimComponent.AnimData.Event> events = new List<QuickAnimComponent.AnimData.Event>();

                    AnimationEvent[] animEvents = AnimationUtility.GetAnimationEvents(clip);
                    serializedObject.FindProperty("anims").GetArrayElementAtIndex(GetField<int>(component, "lastEditedClipIndex")).FindPropertyRelative("events").arraySize = animEvents.Length;

                    for (int i = 0; i < animEvents.Length; i++) {
                        serializedObject.FindProperty("anims").GetArrayElementAtIndex(GetField<int>(component, "lastEditedClipIndex")).FindPropertyRelative("events").GetArrayElementAtIndex(i).FindPropertyRelative("time").floatValue = animEvents[i].time;
                        serializedObject.FindProperty("anims").GetArrayElementAtIndex(GetField<int>(component, "lastEditedClipIndex")).FindPropertyRelative("events").GetArrayElementAtIndex(i).FindPropertyRelative("funtionName").stringValue = animEvents[i].functionName;
                        serializedObject.FindProperty("anims").GetArrayElementAtIndex(GetField<int>(component, "lastEditedClipIndex")).FindPropertyRelative("events").GetArrayElementAtIndex(i).FindPropertyRelative("stringParam").stringValue = animEvents[i].stringParameter;
                        serializedObject.FindProperty("anims").GetArrayElementAtIndex(GetField<int>(component, "lastEditedClipIndex")).FindPropertyRelative("events").GetArrayElementAtIndex(i).FindPropertyRelative("intParam").intValue = animEvents[i].intParameter;
                        serializedObject.FindProperty("anims").GetArrayElementAtIndex(GetField<int>(component, "lastEditedClipIndex")).FindPropertyRelative("events").GetArrayElementAtIndex(i).FindPropertyRelative("floatParam").floatValue = animEvents[i].floatParameter;
                        serializedObject.FindProperty("anims").GetArrayElementAtIndex(GetField<int>(component, "lastEditedClipIndex")).FindPropertyRelative("events").GetArrayElementAtIndex(i).FindPropertyRelative("objRefParam").objectReferenceValue = animEvents[i].objectReferenceParameter;
                    }
                    serializedObject.ApplyModifiedProperties();
                    ReloadUI();
                    EditClip(index);
                }   
            }
        }

        void RemoveClip(int index)
        {
            List<QuickAnimComponent.AnimData> anims = GetField<List<QuickAnimComponent.AnimData>>(component, "anims");

            string clipName = GetField<string>(component, "lastEditedClip");
            if (clipName == anims[index].name)
            {
                if (anim[clipName] != null)
                {
                    anim.RemoveClip(clipName);
                    SetField(component, "lastEditedClip", "");
                    EditorApplication.RepaintAnimationWindow();
                }
            }

            serializedObject.FindProperty("anims").DeleteArrayElementAtIndex(index);
            serializedObject.ApplyModifiedProperties();
        }

        void RenameClip(int index, string newName)
        {
            serializedObject.FindProperty("anims").GetArrayElementAtIndex(index).FindPropertyRelative("name").stringValue = newName;
            serializedObject.ApplyModifiedProperties();


            if (Resources.FindObjectsOfTypeAll<EditorWindow>().Where(w=>w.titleContent.text=="Animation").FirstOrDefault()!=null)
            if (GetField<int>(component, "lastEditedClipIndex") == index)
            {
                EditClip(index);
            }
        }
    }
}
