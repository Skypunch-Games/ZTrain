using System;
using System.Collections.Generic;
using com.ootii.Actors.AnimationControllers;
using com.ootii.Base;
using com.ootii.Data.Serializers;
using com.ootii.Helpers;
using UnityEditor;
using UnityEngine;

namespace com.ootii.Setup.Modules
{
    /// <summary>
    /// Base class for a Setup Module.
    /// </summary>
    public abstract class SetupModule : BaseObject, ISetupModule, ISerializableObject
    {
        public const int DefaultPriority = 50;

        /// <summary>
        /// Indicates that the Setup Module has valid settings and can be run
        /// </summary>
        public virtual bool IsValid { get { return true; } }

        /// <summary>
        /// The priority of the module relative to others (when determining the order
        /// in which modules are run during character setup). 
        /// </summary>
        /// <remarks>CURRENTLY UNUSED</remarks>
        public int Priority { get; private set; }

        /// <summary>
        /// Modules can be grouped together by their category name.
        /// </summary>        
        public string Category { get; private set; }

        /// <summary>
        /// A SetupModule can have dependencies on other Modules
        /// </summary>
        /// <remarks>CURRENTLY UNUSED</remarks>
        public List<Type> RequiredModules
        {
            get
            {
                if (mRequiredModules == null && !mCheckedRequirements)
                {
                    mRequiredModules = ModuleRequiresAttribute.GetRequiredTypes(this.GetType());
                    mCheckedRequirements = true;
                }
                return mRequiredModules;
            }
        }
        protected List<Type> mRequiredModules = null;
        private bool mCheckedRequirements = false;

        // The Module Inspector's foldout state, if used
        protected bool mFoldoutState = true;

        // Character references
        protected MotionController mMotionController;
        protected GameObject mCharacterGO;        
        
        protected readonly Type mModuleType;
        protected readonly List<Type> mComponentTypes;

        /// <summary>
        /// Serializes the object into a string
        /// </summary>
        /// <returns>JSON string representing the object</returns>
        public virtual string Serialize()
        {
            return JSONSerializer.Serialize(this, false);
        }

        /// <summary>
        /// Deserialize the object from a string
        /// </summary>
        /// <param name="rDefinition">JSON string</param>
        public virtual void Deserialize(string rDefinition)
        {
            object lThis = this;
            JSONSerializer.DeserializeInto(rDefinition, ref lThis);
        }

        protected SetupModule()
        {
            mModuleType = this.GetType();

            // Try to get the display name of the module from the ModuleName attribute
            _Name = ModuleNameAttribute.GetName(mModuleType);
            Priority = ModulePriorityAttribute.GetPriority(mModuleType);
            Category = ModuleCategoryAttribute.GetCategory(mModuleType);
            mComponentTypes = ModuleUsesComponentAttribute.GetComponentTypes(mModuleType);
        }
       
        /// <summary>
        /// Checks if the Module uses the specified component.
        /// </summary>
        /// <param name="rType"></param>
        /// <returns></returns>
        public virtual bool UsesComponent(Type rType)
        {
            return mComponentTypes.Contains(rType);        
        }

        /// <summary>
        /// Initialize the Module
        /// </summary>
        /// <param name="rUseDefaults"></param>
        public virtual void Initialize(bool rUseDefaults = false)
        {

        }

        /// <summary>
        /// Begin the setup process by setting the reference to the created Motion Controller component        
        /// </summary>
        /// <param name="rMotionController"></param>        
        public virtual void BeginSetup(MotionController rMotionController)
        {
            mMotionController = rMotionController;
            mCharacterGO = rMotionController.gameObject;          
        }

        /// <summary>
        /// Draw the Inspector for the Module
        /// </summary>
        /// <param name="rTarget"></param>
        /// <returns></returns>
        public virtual bool OnInspectorGUI(UnityEngine.Object rTarget)
        {
            return false;
        }

        /// <summary>
        /// Draws a sub-section within the Module's Inspector
        /// </summary>
        /// <param name="rTitle"></param>
        /// <param name="rDrawContentsAction"></param>
        /// <param name="rDisabled"></param>
        protected virtual void DrawModuleSection(string rTitle, Action rDrawContentsAction, bool rDisabled = false)
        {
            EditorGUI.BeginDisabledGroup(rDisabled);
            try
            {
                GUILayout.BeginVertical(EditorHelper.Box);
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(4f);
                EditorGUILayout.LabelField(rTitle, EditorStyles.boldLabel);
                EditorGUILayout.EndHorizontal();

                rDrawContentsAction();
            }
            finally
            {
                GUILayout.EndVertical();
            }
            EditorGUI.EndDisabledGroup();
            
            EditorGUILayout.Separator();
        } 
    }
}

