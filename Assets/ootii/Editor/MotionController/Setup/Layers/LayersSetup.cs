using System.Collections.Generic;
using com.ootii.Base;
using com.ootii.Helpers;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace com.ootii.Setup
{

    public class LayersSetup : ToolWindow<LayersSetup>
    {
        #region Serialized Values

        // Layers
        [Tooltip("Update the Unity project layers using the referenced Layer Definition Set(s)?")]
        public bool SetProjectLayers = true;

        [Tooltip("Overwrite existing layers?")]
        public bool OverwriteExistingLayers = true;

        public List<LayerDefinitionSet> LayerDefinitionSets = new List<LayerDefinitionSet>();


        #endregion Serialized Values
      
        private ReorderableList mLayerDefinitionsList = null;
        private SerializedProperty mLayerDefinitionSetsProperty = null;       

        private LayerDefinitionSet mDefaultLayerDefinitionSet = null;

        protected const float mMinWidth = 600f;
        protected const float mMinHeight = 600f;
        protected const float mMaxWidth = 800f;
        protected const float mMaxHeight = 1000f;


        [MenuItem("Window/ootii Tools/Layers Setup", false, 3)]
        public static void ShowWindow()
        {
            LayersSetup lWindow = EditorWindow.GetWindow<LayersSetup>(true, "ootii Layers Setup");
            lWindow.minSize = new Vector2(mMinWidth, mMinHeight);
            lWindow.maxSize = new Vector2(mMaxWidth, mMaxHeight);           
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            
            mDefaultLayerDefinitionSet = AssetDatabase.LoadAssetAtPath<LayerDefinitionSet>(
                "Assets/ootii/Assets/MotionController/Content/Default Motion Controller Layers.asset");            

            mLayerDefinitionSetsProperty = FindProperty(x => x.LayerDefinitionSets);

            if (!LayerDefinitionSets.Contains(mDefaultLayerDefinitionSet))
            {
                LayerDefinitionSets.Add(mDefaultLayerDefinitionSet);
            }
            
            SetupDefinitionsList();
        }

        #region Display UI

        private void OnGUI()
        {
            ShowHeader();
            bool lCanBuild = ShowGUI();

            if (!lCanBuild) return;
            if (GUILayout.Button("Build"))
            {
                SetLayers();
            }
        }

        protected void ShowHeader()
        {
            GUILayout.Space(5);
            EditorHelper.DrawInspectorTitle("ootii Project Layers Setup");
            EditorHelper.DrawInspectorDescription("The Layers Setup tool can set the Unity project layers using the designated Layer Definition Sets", MessageType.None);
            GUILayout.Space(5);
        }

        protected bool ShowGUI()
        {
            mSerializedObject.Update();
            

            DrawProjectSettings();
            

            mSerializedObject.ApplyModifiedProperties();

            return true;
        }

        protected void DrawProjectSettings()
        {
            EditorGUILayout.LabelField("Project Settings", EditorStyles.boldLabel, GUILayout.Height(16f));
            try
            {
                GUILayout.BeginVertical(EditorHelper.Box);

                // Use the "Climb" layer at index 9 to test if the default layers have been set up.                
                if (!LayersSetupHelper.LayerExistsAt("Climb", DefaultLayers.Climb))
                {
                    EditorHelper.DrawInspectorDescription("The default Motion Controller layers have not been set up.", MessageType.Warning);
                }
                EditorGUILayout.PropertyField(FindProperty(x => x.SetProjectLayers));
               
                if (SetProjectLayers)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(FindProperty(x => x.OverwriteExistingLayers), new GUIContent("Overwrite Existing"));
                    EditorGUI.indentLevel--;
                    GUILayout.Space(5f);
                    mLayerDefinitionsList.DoLayoutList();
                }                            
            }
            finally
            {
                GUILayout.EndVertical();
                EditorGUILayout.Separator();
            }
        }

       


        #endregion Display UI


        

        protected void SetLayers()
        {
            for (int i = 0; i < mLayerDefinitionSetsProperty.arraySize; i++)
            {
                SerializedProperty lCurrentSet = mLayerDefinitionSetsProperty.GetArrayElementAtIndex(i);
                if (lCurrentSet == null) { continue; }

                LayerDefinitionSet lDefinitionSet = (LayerDefinitionSet) lCurrentSet.objectReferenceValue;
                if (lDefinitionSet == null) { continue; }

                LayersSetupHelper.AddLayers(lDefinitionSet, OverwriteExistingLayers);
            }
        }


        

        #region Layer Definitions List

        private void SetupDefinitionsList()
        {
            mLayerDefinitionsList = new ReorderableList(mSerializedObject, mLayerDefinitionSetsProperty, true, true, true, true);
            mLayerDefinitionsList.drawHeaderCallback += rect => {
                EditorGUI.LabelField(new Rect(12 + rect.x, rect.y, rect.width, rect.height), "Layer Definition Sets");
            };
            mLayerDefinitionsList.drawElementCallback += DrawLayerDefinitionElementCallback;
            
        }

        private void DrawLayerDefinitionElementCallback(Rect rRect, int rIndex, bool rIsActive, bool rIsFocused)
        {
            if (mLayerDefinitionSetsProperty == null || !(0 <= rIndex && rIndex < mLayerDefinitionSetsProperty.arraySize)) {return; }

            SerializedProperty lCurrentSet = mLayerDefinitionSetsProperty.GetArrayElementAtIndex(rIndex);
            if (lCurrentSet == null) { return; }

            EditorGUI.PropertyField(
                new Rect(rRect.x, rRect.y, rRect.width, EditorGUIUtility.singleLineHeight),
                lCurrentSet, GUIContent.none, true);
        }

        #endregion Layer Definitions List
    }
}
