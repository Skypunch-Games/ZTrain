using System;
using UnityEngine;
using System.Collections;
using Devdog.General2.UI;
using UnityEditor;

namespace Devdog.General2.Editors
{
    [CustomEditor(typeof(UIWindow), true)]
    [CanEditMultipleObjects]
    public class UIWindowEditor : Editor
    {
        private static ModuleList<IUIWindowInputHandler> _inputHandlers;

        protected void OnEnable()
        {
            var t = (UIWindow) target;
            _inputHandlers = new ModuleList<IUIWindowInputHandler>(t, this, "Input handler");
            _inputHandlers.description = "Input handlers can be used to make the window respond to keypresses.";
            _inputHandlers.allowDuplicateImplementations = true;
        }

        public override void OnInspectorGUI()
        {
            var t = (UIWindow)target;
            base.OnInspectorGUI();

            if (Application.isPlaying)
            {
                if (t.isVisible)
                {
                    EditorGUILayout.LabelField("Window is Visible");
                    if (GUILayout.Button("OnHide"))
                    {
                        t.Hide();
                    }
                }
                else
                {
                    EditorGUILayout.LabelField("Window is Hidden");
                    if (GUILayout.Button("OnShow"))
                    {
                        t.Show();
                    }
                }
            }

            _inputHandlers.DoLayoutList();
        }
    }
}