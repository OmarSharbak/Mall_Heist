#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace RedLabsGames.Tools.QuickAnim
{
    public class FloatingWindow : EditorWindow
    {

        public bool autoClose = false;

        public Vector2 size = new Vector2(200,50);

        public Vector2 pos = new Vector2();

        public System.Action OnClosed;
        public System.Action OnEnterPressed;


        public FloatingWindow(VisualElement vs,bool autoShow=true)
        {
            rootVisualElement.Add(vs);
            if (autoShow)
                ShowWindow();
        }

        public FloatingWindow()
        {
            
        }

        public void ShowWindow(VisualElement vs)
        {
            rootVisualElement.name = "Window";

            rootVisualElement.style.borderBottomWidth = 1;
            rootVisualElement.style.borderLeftWidth = 1;
            rootVisualElement.style.borderTopWidth = 1;
            rootVisualElement.style.borderRightWidth = 1;

            rootVisualElement.style.borderRightColor = new Color(1, 1, 1, 0.2f);
            rootVisualElement.style.borderLeftColor = new Color(1, 1, 1, 0.2f);
            rootVisualElement.style.borderTopColor = new Color(1, 1, 1, 0.2f);
            rootVisualElement.style.borderBottomColor = new Color(1, 1, 1, 0.2f);

            rootVisualElement.style.paddingBottom = 8;
            rootVisualElement.style.paddingTop = 8;
            rootVisualElement.style.paddingLeft = 8;
            rootVisualElement.style.paddingRight = 8;

            rootVisualElement.Add(vs);

            rootVisualElement.RegisterCallback((KeyDownEvent e) =>
            {
                if (e.keyCode == KeyCode.Escape)
                {
                    this.Close();
                }

                if (e.keyCode == KeyCode.Return || e.keyCode == KeyCode.KeypadEnter)
                {
                    OnEnterPressed?.Invoke();
                }

            });

            ShowWindow();
        }

        public void ShowWindow()
        {
            ShowWindow(GUIUtility.GUIToScreenPoint(pos));
        }

        public void ShowWindow(Vector2 pos)
        {
            position = new Rect(pos, size);
            this.maxSize = size;
            this.ShowPopup();
        }

        private void OnLostFocus()
        {
            if (autoClose)
                this.Close();
        }

        private void OnDestroy()
        {
            OnClosed?.Invoke();
        }

    }
}
#endif