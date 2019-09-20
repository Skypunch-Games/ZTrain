using com.ootii.Helpers;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace com.ootii.Setup
{
    // CDL 07/01/2018 - custom editor for LayerDefinitionSet
    [CanEditMultipleObjects]
    [CustomEditor(typeof(LayerDefinitionSet))]
    public class LayerDefinitionSetEditor : Editor
    {
        private const float TypeColumnWidth = 80;
        private const float ListPadding = 12;
        private const float ColumnPadding = 6;

        private ReorderableList mList = null;
        private LayerDefinitionSet mLayerDefinitionSet = null;
        private SerializedProperty mLayerDefinitionsProperty = null;
  
        private SerializedProperty mOverwriteExistingLayers = null;

        void OnGUI()
        {

            //EditorUtility.DisplayDialog("MyTool", "Do It in C# !", "OK", "");
            GUI.skin.box = null;
            GUI.skin.window = null;
            GUI.skin.label = null;
            GUI.skin.font = null;

            Debug.Log("Reset some GUI.skin values");

        }

        private void OnEnable()
        {           
            mLayerDefinitionSet = (LayerDefinitionSet)target;
            mLayerDefinitionsProperty = serializedObject.FindProperty("_LayerDefinitions");
            mOverwriteExistingLayers = serializedObject.FindProperty("_OverwriteExistingLayers");
            SetupList();
        }

        public override void OnInspectorGUI()
        {
            Undo.RecordObject(target, "Inspector");
            serializedObject.Update();

            GUILayout.Space(5);

            EditorHelper.DrawInspectorTitle("ootii Layer Definition Set");
            EditorHelper.DrawInspectorDescription("The Layer Definitions included in this set can be use to set Unity's layers.", MessageType.None);

            GUILayout.Space(5);

            EditorGUILayout.LabelField(serializedObject.targetObject.name, EditorStyles.boldLabel, GUILayout.Height(16f));

            try
            {
                GUILayout.BeginVertical(EditorHelper.Box);

                EditorGUILayout.PropertyField(serializedObject.FindProperty("_Description"), new GUIContent("Help Text"));
                EditorHelper.DrawInspectorDescription("This is a reorderable list, but the actual order doesn't matter.", MessageType.None);
                EditorGUILayout.Separator();
                mList.DoLayoutList();
               
                EditorGUILayout.Separator();

                EditorHelper.DrawInspectorDescription("By default, this operation will only add layer names where no existing names have been defined. " +
                    "When overwriting existing layers, only the layer indexes present in the Layer Definition Set will be affected. ", MessageType.Warning);
                EditorGUILayout.PropertyField(mOverwriteExistingLayers);
                               
                if (GUILayout.Button("Set Layers", EditorStyles.miniButton))
                {
                    SetupLayers(mLayerDefinitionSet);
                }
            }
            finally
            {
                EditorGUILayout.Separator();
                GUILayout.EndVertical();
            }

            

            serializedObject.ApplyModifiedProperties();
        }

        private void SetupList()
        {
            mList = new ReorderableList(serializedObject, mLayerDefinitionsProperty, true, true, true, true);
            mList.drawHeaderCallback += OnDrawHeader;
            mList.drawElementCallback += OnDrawElement;           
            mList.onAddCallback += OnAddLayer;
        }

        private void OnDrawHeader(Rect rRect)
        {
            float lIndexWidth = 50;
            float lFieldWidth = (rRect.width - lIndexWidth - ListPadding);
            

            EditorGUI.LabelField(new Rect(ListPadding + rRect.x, rRect.y, lIndexWidth, rRect.height), "Index");
            EditorGUI.LabelField(new Rect(ListPadding + rRect.x + lIndexWidth + ColumnPadding, rRect.y, lFieldWidth, rRect.height), "Name");            
        }

        private void OnDrawElement(Rect rRect, int rIndex, bool rIsActive, bool rIsFocused)
        {
            float lIndexWidth = 50;
            float lFieldWidth = (rRect.width - lIndexWidth - 4);

            var layerProperty = mLayerDefinitionsProperty.GetArrayElementAtIndex(rIndex);
            if (layerProperty == null) { return; }

            SerializedProperty lIndex = layerProperty.FindPropertyRelative("_Index");
            SerializedProperty lName = layerProperty.FindPropertyRelative("_Name");

            EditorGUI.PropertyField(new Rect(rRect.x, rRect.y +2, lIndexWidth, rRect.height -6), lIndex, GUIContent.none);
            EditorGUI.PropertyField(new Rect(rRect.x + lIndexWidth + ColumnPadding, rRect.y + 2, lFieldWidth, rRect.height -6), lName, GUIContent.none);
        }
       
        private void SetupLayers(LayerDefinitionSet rLayerDefinitionSet)
        {
            LayersSetupHelper.AddLayers(rLayerDefinitionSet, mOverwriteExistingLayers.boolValue);
         
            EditorUtility.DisplayDialog("Layers Set Up", rLayerDefinitionSet.name + " set.", "Ok");
        }       

        private void OnAddLayer(ReorderableList rList)
        {
            SerializedProperty lSelectedLayer = rList.serializedProperty.GetArrayElementAtIndex(rList.index);

            int lIndex = rList.serializedProperty.arraySize;
            rList.serializedProperty.arraySize++;
            rList.index = lIndex;

            
            SerializedProperty lLayer = rList.serializedProperty.GetArrayElementAtIndex(lIndex);
            if (lSelectedLayer != null)
            {
                int lLayerIndex = lSelectedLayer.FindPropertyRelative("_Index").intValue;
                lLayer.FindPropertyRelative("_Index").intValue = lLayerIndex +1;
                lLayer.FindPropertyRelative("_Name").stringValue = string.Empty;
            }
        }
       
    }
}
