using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.Linq;
namespace RedLabsGames.Tools.QuickAnim
{
    [CustomEditor(typeof(QuickAnimUnityEvent))]
    public class QuickAnimEventEditor : Editor
    {

        VisualElement root;
        QuickAnimUnityEvent component;

        SerializedProperty selectedEvent;
        SerializedProperty events;
        Color iconColorPro = new Color(1, 1, 1, 1), iconColorNormal = new Color(0.2924528f, 0.2924528f, 0.2924528f, 1);


        public override VisualElement CreateInspectorGUI()
        {
            component = target as QuickAnimUnityEvent;
            root = new VisualElement()
            {
                style =
            {
                marginTop=10,
                marginBottom=10
            }
            };

            GetProperties();
            ReloadUI();

            Undo.undoRedoPerformed += () =>
            {
                root.schedule.Execute(() =>
                {
                    ReloadUI();
                }).ExecuteLater(10);
            };

            return root;
        }

        void GetProperties()
        {
            selectedEvent = serializedObject.FindProperty("selectedIndex");
            events = serializedObject.FindProperty("events");
        }

        string GetSelectedEventName()
        {
            if (selectedEvent.intValue == -1) {
                return "Add New Event";
            }
            else {
                return events.GetArrayElementAtIndex(selectedEvent.intValue).FindPropertyRelative("eventName").stringValue;
            }
        }


        public void ReloadUI()
        {
            root.Clear();
            var pm = PrefabUtility.GetPropertyModifications(target);
            if (pm == null) {
                pm = new PropertyModification[0];
            }

            events.serializedObject.Update();
            selectedEvent.serializedObject.Update();

            if (events.arraySize != 0) {

                VisualElement headerContainer = new VisualElement()
                {
                    style =
                {
                    flexDirection=FlexDirection.Row
                }
                };


                headerContainer.Add(new Button(() =>
                {
                    GenericMenu menu = new GenericMenu();
                    menu.allowDuplicateNames = true;
                    for (int i = 0; i < events.arraySize; i++) {
                        string eventName = events.GetArrayElementAtIndex(i).FindPropertyRelative("eventName").stringValue;
                        menu.AddItem(new GUIContent(eventName), eventName == GetSelectedEventName(), (o) =>
                        {
                            SelectEvent((int)o);
                        }, i);
                    }

                    menu.ShowAsContext();
                })
                {
                    name = "ToggleEventButton",
                    text = GetSelectedEventName(),
                    tooltip = "Choose event to edit",
                    style =
                {
                    borderRightWidth=0,
                    borderBottomWidth=0,
                    borderLeftWidth=0,
                    borderTopWidth=0,
                    borderTopLeftRadius=0,
                    borderBottomRightRadius=0,
                    borderTopRightRadius=0,
                    borderBottomLeftRadius=0,
                    marginBottom=0,
                    marginLeft=0,
                    marginRight=0,
                    marginTop=0,
                    paddingBottom=0,
                    paddingLeft=10,
                    paddingRight=0,
                    paddingTop=0,
                    fontSize=14,
                    flexGrow=1,
                    unityTextAlign=TextAnchor.MiddleLeft
                }
                });

                headerContainer.Q("ToggleEventButton").Add(new Image()
                {
                    name = "dropdownIcon",
                    image = Resources.Load<Texture>("QuickAnimIcons/Once"),
                    tintColor = EditorGUIUtility.isProSkin ? iconColorPro : iconColorNormal,
                    pickingMode = PickingMode.Ignore,
                    style =
                    {
                        width=20,
                        height=20,
                        marginRight=3,
                        marginBottom=3,
                        left=20,
                        top=1,
                        alignSelf=Align.FlexEnd,
                    }
                });
                headerContainer.Q("dropdownIcon").transform.rotation = Quaternion.Euler(0, 0, 90);


                headerContainer.Add(new Button(() =>
                {
                    AddNewEvent();
                })
                {
                    text = "+",
                    tooltip = "Add New Event",
                    style =
                {
                    borderRightWidth=0,
                    borderBottomWidth=0,
                    borderLeftWidth=1,
                    borderTopWidth=0,
                    borderTopLeftRadius=0,
                    borderBottomRightRadius=0,
                    borderTopRightRadius=0,
                    borderBottomLeftRadius=0,
                    width=32,
                    height=24,
                    marginBottom=0,
                    marginLeft=0,
                    marginRight=0,
                    marginTop=0,
                    alignSelf=Align.FlexEnd,
                    fontSize=25,
                    unityFontStyleAndWeight = FontStyle.Bold
                }
                });
                root.Add(headerContainer);
            }
            else {
                Button AddEventButton = new Button(() =>
                {
                    AddNewEvent();
                })
                {
                    text = "Add New Event",
                    style =
            {
                flexDirection=FlexDirection.Row,
                borderBottomLeftRadius=0,
                borderBottomRightRadius=0,
                borderTopLeftRadius=4,
                borderTopRightRadius=4,
                marginBottom=-1,
                marginLeft=-1,
                marginRight=-1,
                marginTop=-1,
                paddingBottom=0,
                paddingLeft=10,
                paddingRight=0,
                paddingTop=0,
                height=24,
                width=150,
                alignSelf=Align.Center,
                unityTextAlign=TextAnchor.MiddleCenter
            }
                };
                root.Add(AddEventButton);
            }




            VisualElement container = new VisualElement()
            {
                style =
            {
                marginTop=-1,
                borderBottomLeftRadius=4,
                borderBottomRightRadius=4,
                borderLeftWidth=1.5f,
                borderRightWidth=1.5f,
                borderBottomWidth=1.5f,
                borderTopWidth=1.5f,
                paddingBottom=10,
                paddingLeft=10,
                paddingRight=10,
                paddingTop=10,
                //borderTopColor= new Color(1,1,1,0.2f),
                borderRightColor= new Color(1,1,1,0.2f),
                borderLeftColor= new Color(1,1,1,0.2f),
                borderBottomColor= new Color(1,1,1,0.2f),
            }
            };



            if (events.arraySize != 0 && selectedEvent.intValue<events.arraySize) {
                root.Add(container);

                VisualElement box = new VisualElement()
                {
                    style =
                {
                    flexGrow=1,
                    marginBottom=10,
                    flexDirection=FlexDirection.Row
                }
                };

                container.Add(box);

                bool eventNameModified = pm.Any(f => f.propertyPath == "events.Array.data[" + selectedEvent.intValue + "].eventName");
                TextField eventNameField = new TextField("Event Name")
                {
                    style =
                {
                    flexGrow=1,
                    borderLeftColor=new Color(0.05882353f,0.5058824f,0.7450981f),
                    borderLeftWidth=eventNameModified?2:0
                }
                };

                if (eventNameModified)
                    eventNameField.Q<Label>().RegisterCallback((MouseDownEvent e) =>
                    {
                        if (e.button == 1) {
                            GenericMenu menu = new GenericMenu();
                            menu.AddItem(new GUIContent("Apply"), false, () =>
                            {
                                PrefabUtility.ApplyPropertyOverride(events.GetArrayElementAtIndex(selectedEvent.intValue).FindPropertyRelative("eventName"), PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(target), InteractionMode.UserAction);
                                eventNameField.schedule.Execute(() =>
                                {
                                    ReloadUI();
                                }).ExecuteLater(10);
                            });
                            menu.AddItem(new GUIContent("Revert"), false, () =>
                            {
                                PrefabUtility.RevertPropertyOverride(events.GetArrayElementAtIndex(selectedEvent.intValue).FindPropertyRelative("eventName"), InteractionMode.UserAction);
                                eventNameField.schedule.Execute(() =>
                                {
                                    ReloadUI();
                                }).ExecuteLater(10);
                            });
                            menu.ShowAsContext();
                        }
                    });

                eventNameField.value = events.GetArrayElementAtIndex(selectedEvent.intValue).FindPropertyRelative("eventName").stringValue;
                eventNameField.isDelayed = true;
                eventNameField.RegisterValueChangedCallback((e) =>
                {
                    if (GetField<List<QuickAnimUnityEvent.EventData>>(component, "events").Any(c => c.eventName == e.newValue)) {
                        EditorUtility.DisplayDialog("Error", e.newValue + " is already exists.", "OK");
                        ReloadUI();
                    }
                    else {
                        events.GetArrayElementAtIndex(selectedEvent.intValue).FindPropertyRelative("eventName").stringValue = e.newValue;
                        events.GetArrayElementAtIndex(selectedEvent.intValue).FindPropertyRelative("eventName").serializedObject.ApplyModifiedProperties();
                        ReloadUI();
                    }
                });

                box.Add(eventNameField);


                box.Add(new Button(() =>
                {
                    RemoveEvent(selectedEvent.intValue);
                })
                {
                    name = "Remove",
                    tooltip = "Remove",
                });

                box.Q("Remove").Add(new Image()
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

                PropertyField eventField = new PropertyField(events.GetArrayElementAtIndex(selectedEvent.intValue).FindPropertyRelative("onEvent"));
                eventField.Bind(events.serializedObject);
                container.Add(eventField);
            }
        }

        void AddNewEvent()
        {
            events.arraySize++;

            int count = GetField<List<QuickAnimUnityEvent.EventData>>(component, "events").Count(e => e.eventName.StartsWith("NewEvent"));

            events.GetArrayElementAtIndex(events.arraySize - 1).FindPropertyRelative("eventName").stringValue = "NewEvent" + (count != 0 ? count.ToString() : "");
            events.GetArrayElementAtIndex(events.arraySize - 1).FindPropertyRelative("onEvent").FindPropertyRelative("m_PersistentCalls").FindPropertyRelative("m_Calls").arraySize = 0;

            events.serializedObject.ApplyModifiedProperties();

            SelectEvent(events.arraySize - 1);
        }

        void RemoveEvent(int index)
        {
            events.DeleteArrayElementAtIndex(index);
            if (selectedEvent.intValue != 0) {
                selectedEvent.intValue -= 1;
            }

            events.serializedObject.ApplyModifiedProperties();
            selectedEvent.serializedObject.ApplyModifiedProperties();

            ReloadUI();
        }

        void SelectEvent(int index)
        {
            selectedEvent.intValue = index;
            selectedEvent.serializedObject.ApplyModifiedProperties();

            ReloadUI();
        }

        static T GetField<T>(object source, string fieldName)
        {
            return (T)source.GetType().GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(source);
        }

    }
}