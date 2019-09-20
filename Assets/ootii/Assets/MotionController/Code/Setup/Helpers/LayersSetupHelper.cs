using UnityEngine;
using com.ootii.Helpers;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace com.ootii.Setup
{
    /// <summary>
    /// The default defined layers used by the Motion Controller demos
    /// Also includes the built-in Unity layers and a few common settings
    /// </summary>
    public static class DefaultLayers
    {
        // The built-in Unity layers
        public static int Default = 0;
        public static int TransparentFX = 1;
        public static int IgnoreRaycast = 2;
        public static int Water = 4;
        public static int UI = 5;

        // Not built-in, but the default for the PostProcessing Stack v2
        public static int PostProcessing = 8;

        // Custom layers for Motion Controller (based on the demo scenes)
        public static int Climb = 9;
        public static int Ladder = 10;
        public static int ScalableWall = 11;
        public static int ScalableLedge = 12;
        public static int BalanceWalk = 13;
        public static int Interaction = 14;

        // Additional layers

        public static int PhysicsObjects = 16;
        //public static int Player = 20;
        //public static int NPC = 21;

        public static string[] Names =
        {
                "Default",          // 0
                "TransparentFX",    // 1
                "Ignore Raycast",   // 2
                "",                 // 3
                "Water",            // 4
                "UI",               // 5
                "",                 // 6
                "",                 // 7
                "PostProcessing",   // 8
                "Climb",            // 9
                "Ladder",           // 10
                "Scalable Wall",    // 11
                "Scalable Ledge",   // 12
                "Balance Walk",     // 13
                "Interaction",      // 14
                "",                 // 15
                "Physics Objects",  // 16
                "",                 // 17
                "",
                "",
                "",                 // 20 
                "",                 // 21
                "",
                "",
                ""                  // 24
            };
    }

    /// <summary>
    /// Helper functions for setting up Unity Layers
    /// </summary>
    public static class LayersSetupHelper
    {        
        /// <summary>
        /// Test if there is a defined layer with the specified name
        /// </summary>
        /// <param name="rName"></param>
        /// <returns></returns>
        public static bool LayerExists(string rName)
        {
            int lLayer = LayerMask.NameToLayer(rName);
            return lLayer > -1;
        }

        /// <summary>
        /// Test if there is a defined layer with the specified name at the designated index
        /// </summary>
        /// <param name="rName"></param>
        /// <param name="rIndex"></param>
        /// <returns></returns>
        public static bool LayerExistsAt(string rName, int rIndex)
        {
            int lLayer = LayerMask.NameToLayer(rName);            
            return lLayer == rIndex;
        }

        #region Editor Functions

#if UNITY_EDITOR

        public static readonly string DefaultLayerDefinitionSetPath =
            DefaultPaths.MotionControllerContent + "Data/Setup/Default Motion Controller Layers.asset";

        /// <summary>
        /// Check if the default set of Motion Controller layers have been created
        /// </summary>
        /// <returns></returns>
        public static bool AreDefaultLayersSet()
        {
            // Iterate through the default layers used by the standard motions. If any are not set,
            // then return false. These are the essential layers for basic functionality.
            for (int i = DefaultLayers.Climb; i <= DefaultLayers.Interaction; i++)
            {
                if (!LayerExistsAt(DefaultLayers.Names[i], i))
                {                    
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Provides access to the "layers" property of the Tag Manager
        /// </summary>
        private static SerializedProperty mLayersProperty = null;
        private static SerializedProperty LayersProperty
        {
            get
            {         
                if (mLayersProperty == null)       
                    mLayersProperty = TagManager.FindProperty("layers");

                return mLayersProperty;                          
            }
        }

        /// <summary>
        /// Provides access to the (Unity) Tag Manager
        /// </summary>
        private static SerializedObject mTagManager = null;
        private static SerializedObject TagManager
        {
            get
            {
                if (mTagManager == null)
                    mTagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
                                
                return mTagManager;                
            }
        }

        /// <summary>
        /// Load the set of default layer definitions from the file system; if it does not exist, then
        /// the default asset will be re-created
        /// </summary>
        /// <returns></returns>
        public static LayerDefinitionSet LoadDefaultLayerDefinitionSet()
        {
            LayerDefinitionSet lDefinitionSet =
                AssetDatabase.LoadAssetAtPath<LayerDefinitionSet>(DefaultLayerDefinitionSetPath);

            if (lDefinitionSet == null)
            {
                // Create the asset
                lDefinitionSet = AssetHelper.GetOrCreateAsset<LayerDefinitionSet>(DefaultLayerDefinitionSetPath);

                // Add the defaut layer definitions
                for (int i = 0; i < DefaultLayers.Names.Length; i++)
                {
                    if (!(string.IsNullOrEmpty(DefaultLayers.Names[i])))
                    {
                        lDefinitionSet.LayerDefinitions.Add(new LayerDefinition
                        {
                            Index = i,
                            Name = DefaultLayers.Names[i]
                        });
                    }
                }

                // Ensure the changes are saved
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            return lDefinitionSet;
        }

        /// <summary>
        /// Apply the default layer definition set; optionally overwrite existing layers.
        /// </summary>
        /// <param name="rOverwriteExisting"></param>
        public static void ApplyDefaultLayers(bool rOverwriteExisting)
        {
            LayerDefinitionSet lLayers = LoadDefaultLayerDefinitionSet();
            if (lLayers == null) { return; }

            AddLayers(lLayers, rOverwriteExisting);
        }

        /// <summary>
        /// Add a defined Layer (optional: overwrite an existing layer if already defined).
        /// This updates the Tag Manager properties after adding the layer.
        /// </summary>
        /// <param name="rIndex"></param>
        /// <param name="rName"></param>
        /// <param name="rOverwriteExisting"></param>
        public static void AddLayer(int rIndex, string rName, bool rOverwriteExisting)
        {
            AddLayer_Internal(rIndex, rName, rOverwriteExisting);

            TagManager.ApplyModifiedProperties();
        }

        /// <summary>
        /// Add a defined Layer (optional: overwrite an existing layer if already defined).
        /// This updates the Tag Manager properties after adding the layer.
        /// </summary>
        /// <param name="rLayerDefinition"></param>
        /// <param name="rOverwriteExisting"></param>
        public static void AddLayer(LayerDefinition rLayerDefinition, bool rOverwriteExisting)
        {
            AddLayer_Internal(rLayerDefinition.Index, rLayerDefinition.Name, rOverwriteExisting);

            TagManager.ApplyModifiedProperties();
        }

        /// <summary>
        /// Add the layers contained within the designated Layer Definition Set.
        /// The Tag Manager properties are updated after the set of layers has been updated.
        /// </summary>
        /// <param name="rLayerDefinitionSet"></param>
        /// <param name="rOverwriteExisting"></param>
        public static void AddLayers(LayerDefinitionSet rLayerDefinitionSet, bool rOverwriteExisting)
        {
            if (rLayerDefinitionSet == null) { return; }            

            foreach (LayerDefinition lLayer in rLayerDefinitionSet.LayerDefinitions)
            {
                AddLayer_Internal(lLayer, rOverwriteExisting);
            }

            TagManager.ApplyModifiedProperties();
        }

        /// <summary>
        /// Add a defined layer. Does not apply modified properties on the Tag Manager.
        /// </summary>
        /// <param name="rIndex"></param>
        /// <param name="rName"></param>
        /// <param name="rOverwriteExisting"></param>
        private static void AddLayer_Internal(int rIndex, string rName, bool rOverwriteExisting)
        {
            SerializedProperty lLayer = LayersProperty.GetArrayElementAtIndex(rIndex);
            if (string.IsNullOrEmpty(lLayer.stringValue) || rOverwriteExisting)
            {
                lLayer.stringValue = rName;
                Debug.Log(string.Format("Set Layer [{0}] = {1}\n", rIndex, rName));
            }
        }

        /// <summary>
        /// Add a defined layer. Does not apply modified properties on the Tag Manager.
        /// </summary>
        /// <param name="rLayerDefinition"></param>
        /// <param name="rOverwriteExisting"></param>
        private static void AddLayer_Internal(LayerDefinition rLayerDefinition, bool rOverwriteExisting)
        {
            AddLayer_Internal(rLayerDefinition.Index, rLayerDefinition.Name, rOverwriteExisting);
        }

#endif

        #endregion Editor Functions
    }

    
}
