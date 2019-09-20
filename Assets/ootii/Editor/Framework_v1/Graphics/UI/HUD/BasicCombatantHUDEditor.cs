using com.ootii.Helpers;
using UnityEditor;
using UnityEngine;

namespace com.ootii.UI.Editors
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(BasicCombatantHUD), true)]
    public class BasicCombatantHUDEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            Undo.RecordObject(target, "Inspector");
            serializedObject.Update();

            GUILayout.Space(5);

            EditorHelper.DrawInspectorTitle("ootii Basic Combatant HUD");

            bool lWorldSpace = IsInWorldSpace();
            EditorHelper.DrawInspectorDescription(
                    lWorldSpace
                        ? "Displays a simple health bar in world space."
                        : "Displays a simple health bar in a fixed position on-screen.",
                    MessageType.None);

            GUILayout.Space(5);
           
            try
            {
                EditorGUILayout.LabelField("Character Settings", EditorStyles.boldLabel, GUILayout.Height(16f));
                GUILayout.BeginVertical(EditorHelper.Box);

                SerializedProperty lUsePlayer = serializedObject.FindProperty("_UsePlayer");
                EditorGUILayout.PropertyField(lUsePlayer);
                if (!lUsePlayer.boolValue)
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("_BasicAttributes"));
                }

                EditorGUILayout.PropertyField(serializedObject.FindProperty("_HealthKey"));

                GUILayout.Space(5);

                EditorGUILayout.PropertyField(serializedObject.FindProperty("_HideWhenFull"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("_HideOnDeath"));
            }
            finally
            {                
                GUILayout.EndVertical();
            }

            EditorGUILayout.Separator();

            try
            {
                EditorGUILayout.LabelField("UI Settings", EditorStyles.boldLabel, GUILayout.Height(16f));
                GUILayout.BeginVertical(EditorHelper.Box);                
                
                SerializedProperty lUseEasing = serializedObject.FindProperty("_UseEasing");
                EditorGUILayout.PropertyField(lUseEasing);
                if (lUseEasing.boolValue)
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("_EasingCurve"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("_EasingSpeed"));
                }
                GUILayout.Space(5);

                EditorGUILayout.PropertyField(serializedObject.FindProperty("_HealthBar"));
            }
            finally
            {                
                GUILayout.EndVertical();
            }

            EditorGUILayout.Separator();

            EditorGUILayout.PropertyField(serializedObject.FindProperty("_ShowDebugInfo"));

            serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// Checks if a Canvas with RenderMode == WorldSpace is a child of this gameObject.
        /// </summary>
        /// <returns></returns>
        protected virtual bool IsInWorldSpace()
        {
            bool lWorldSpace = false;

            BasicCombatantHUD lCombatantHUD = (BasicCombatantHUD)target;
            if (lCombatantHUD == null) { return false; }

            Canvas lCanvas = lCombatantHUD.gameObject.GetComponentInChildren<Canvas>();
            if (lCanvas != null)
            {
                lWorldSpace = lCanvas.renderMode == RenderMode.WorldSpace;
            }
            
            return lWorldSpace;
        }
    }
}


