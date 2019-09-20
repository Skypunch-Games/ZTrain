using com.ootii.Setup.Modules;
using com.ootii.Utilities;
using UnityEditor;


namespace com.ootii.Setup.Modules
{
    /// <summary>
    /// When a Module cannot be created using the serialized type string, an Undefined Module will
    /// be instantiated as a placeholder.
    /// 
    /// This will happen if a Character Wizard Profile is copied from one project to another and the 
    /// destination project does not contain the Module type serialized in the Profile asset.
    /// </summary>
    [ModuleName("Undefined Module"), HideModule(true)]    
    [ModuleDescription("The Character Wizard Module can not be found in the project.")]
    public sealed class UndefinedSetupModule : SetupModule
    {
        // Store the original module definition
        private readonly string mModuleDefinition = null;

        public UndefinedSetupModule(string rModuleDefinition)
        {
            mModuleDefinition = rModuleDefinition;
            var lNode = JSONNode.Parse(rModuleDefinition);
            _Name = lNode["_Name"].Value;
        }

        /// <summary>
        /// Return the original serialized module definition to preserve the original data
        /// </summary>
        /// <returns></returns>
        public override string Serialize()
        {
            return mModuleDefinition;
        }

        /// <summary>
        /// Do nothing when deserializing
        /// </summary>
        /// <param name="rDefinition"></param>
        public override void Deserialize(string rDefinition)
        {
            // Do nothing
        }       

        public override bool OnInspectorGUI(UnityEngine.Object rTarget)
        {
            EditorGUILayout.Separator();
            //EditorGUI.BeginDisabledGroup(true);
            //string lHelpText = "The setup module is not present in the project.";
            //EditorHelper.DrawInspectorBlock(Name + "(Undefined)" , lHelpText, EditorGUILayout.Separator, ref mFoldoutState, false);
            //EditorGUI.EndDisabledGroup();

            return false;
        }
    }

}

