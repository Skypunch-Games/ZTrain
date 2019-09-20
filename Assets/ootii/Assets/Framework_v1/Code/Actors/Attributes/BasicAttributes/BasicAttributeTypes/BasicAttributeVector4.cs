﻿using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using com.ootii.Helpers;
#endif

namespace com.ootii.Actors.Attributes
{
    /// <summary>
    /// Basic class for all Attribute Vector4 items.
    /// </summary>
    public class BasicAttributeVector4 : BasicAttributeTyped<Vector4>
    {

        // **************************************************************************************************
        // Following properties and function only valid while editing
        // **************************************************************************************************

#if UNITY_EDITOR

        /// <summary>
        /// Allows each attribute item to render thier own GUI
        /// </summary>
        /// <param name="rTarget"></param>
        /// <returns></returns>
        public override bool OnInspectorGUI(Rect rRect)
        {
            bool lIsDirty = false;

            Vector4 lNewValue = EditorGUI.Vector4Field(rRect, "", Value);
            if (lNewValue != Value)
            {
                lIsDirty = true;
                Value = lNewValue;
            }

            return lIsDirty;
        }

        /// <summary>
        /// Allows each attribute item to render thier own GUI
        /// </summary>
        /// <param name="rTarget"></param>
        /// <returns></returns>
        public override bool OnInspectorGUI(BasicAttributes rTarget)
        {
            bool lIsDirty = base.OnInspectorGUI(rTarget);

            Vector4 lNewValue = EditorGUILayout.Vector4Field(new GUIContent("Value", "Value of the Attribute"), Value);
            if (lNewValue != Value)
            {
                lIsDirty = true;
                Value = lNewValue;
            }

            return lIsDirty;
        }

#endif

    }
}
