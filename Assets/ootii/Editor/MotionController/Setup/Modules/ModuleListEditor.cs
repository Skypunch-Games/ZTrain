using System;
using System.Collections.Generic;
using System.Linq;
using com.ootii.Helpers;
using com.ootii.Utilities.Debug;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace com.ootii.Setup.Modules
{
    /// <summary>
    /// Manages the list of Character Wizard Setup modules on a Profile
    /// </summary>
    public class ModuleListEditor
    {
        // Indicates if the list has modified the target's values
        private bool mIsDirty = false;

        // The target Setup Profile containing the serialied fields
        private readonly BaseSetupProfile mTarget;
        
        private ReorderableList mModuleList;
        private int mModuleTypeIndex = 0;        
        private readonly List<Type> mModuleTypes = new List<Type>();
        private readonly List<string> mModuleNames = new List<string>();

        //private readonly T mTarget;

        public ModuleListEditor(BaseSetupProfile rTarget)
        {
            mTarget = rTarget;
            Initialize();
        }
        
        public bool OnInspectorGUI()
        {
            if (mModuleList.count != mTarget.Modules.Count)
            {
                InstantiateModuleList();
            }

            EditorGUILayout.LabelField("Modules", EditorStyles.boldLabel, GUILayout.Height(16f));
            GUILayout.BeginVertical(EditorHelper.GroupBox);

            //var listDebug = string.Format("ModuleList.index = {0}, mTarget._Modules.Count = {1}",
            //        mModuleList.index, mTarget.Modules.Count);

            EditorHelper.DrawInspectorDescription("Modules extend the functionality of the Character Wizard, such as setup options for Motion Packs or integration packages.", MessageType.None);

            EditorGUI.BeginDisabledGroup(mModuleTypes.Count < 1);
            mModuleList.DoLayoutList();
            if (mModuleList.index >= 0)
            {
                GUILayout.Space(5f);
                GUILayout.BeginVertical(EditorHelper.Box);


                if (mModuleList.index < mTarget.Modules.Count)
                {
                    bool lListIsDirty = DrawModuleDetailItem(mTarget.Modules[mModuleList.index]);
                    if (lListIsDirty) { mIsDirty = true; }
                }
                else
                {
                    mModuleList.index = -1;
                }

                GUILayout.EndVertical();
            }
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.EndVertical();

            GUILayout.Space(5f);

            // return the current IsDirty value and reset the class-level field
            bool lIsDirty = mIsDirty;
            mIsDirty = false;
            return lIsDirty;
        }


        private void Initialize()
        {
            FindModuleTypes();
            InstantiateModuleList();
        }

        /// <summary>
        /// Scan all assemblies for Character Wizard Modules
        /// </summary>
        private void FindModuleTypes()
        {
            var lFoundTypes = AssemblyHelper.FoundTypes;
            foreach (Type lType in lFoundTypes)
            {
                if (lType.IsAbstract) { continue; }
                if (typeof(SetupModule).IsAssignableFrom(lType)
                    && !HideModuleAttribute.GetHidden(lType))
                {
                    mModuleTypes.Add(lType);
                    mModuleNames.Add(ModuleNameAttribute.GetName(lType));
                }
            }
        }

        private void InstantiateModuleList()
        {
            mModuleList = new ReorderableList(mTarget.Modules, typeof(SetupModule), true, true, true, true)
            {
                drawHeaderCallback = DrawModuleListHeader,
                drawFooterCallback = DrawModuleListFooter,
                drawElementCallback = DrawModuleListItem,
                onAddCallback = OnModuleListItemAdd,
                onRemoveCallback = OnModuleListItemRemove,
                onSelectCallback = OnModuleListItemSelect,
                onReorderCallback = OnModuleListReorder,
                footerHeight = 17f
            };

            if (mTarget.EditorModuleIndex >= 0 && mTarget.EditorModuleIndex < mModuleList.count)
            {
                mModuleList.index = mTarget.EditorModuleIndex;
            }
        }
        
        /// <summary>
        /// Draws the list header
        /// </summary>
        /// <param name="rRect"></param>
        private void DrawModuleListHeader(Rect rRect)
        {
            EditorGUI.LabelField(rRect, "Modules");

            Rect lNoteRect = new Rect(rRect.width + 12f, rRect.y, 11f, rRect.height);
            EditorGUI.LabelField(lNoteRect, "-", EditorStyles.miniLabel);

            if (GUI.Button(rRect, "", EditorStyles.label))
            {
                mModuleList.index = -1;
                OnModuleListItemSelect(mModuleList);
            }
        }

        /// <summary>
        /// Draws the list footer adn buttons
        /// </summary>
        /// <param name="rRect"></param>
        private void DrawModuleListFooter(Rect rRect)
        {
            if (mModuleTypes.Count == 0) { return; }

            Rect lModuleRect = new Rect(rRect.x, rRect.y + 1, rRect.width - 4 - 28 - 28, 16);
            mModuleTypeIndex = EditorGUI.Popup(lModuleRect, mModuleTypeIndex, mModuleNames.ToArray());

            // Don't allow a duplicate module type to be added
            EditorGUI.BeginDisabledGroup(HasModuleType(mModuleTypes[mModuleTypeIndex]));
            Rect lAddRect = new Rect(rRect.x + rRect.width - 28 - 28 - 1, rRect.y + 1, 28, 15);
            if (GUI.Button(lAddRect, new GUIContent("+", "Add Module."), EditorStyles.miniButtonLeft)) { OnModuleListItemAdd(mModuleList); }
            EditorGUI.EndDisabledGroup();

            Rect lDeleteRect = new Rect(lAddRect.x + lAddRect.width, lAddRect.y, 28, 15);
            if (GUI.Button(lDeleteRect, new GUIContent("-", "Delete Module."), EditorStyles.miniButtonRight)) { OnModuleListItemRemove(mModuleList); };
        }

        /// <summary>
        /// Draw a module in the list
        /// </summary>
        /// <param name="rRect"></param>
        /// <param name="rIndex"></param>
        /// <param name="rIsActive"></param>
        /// <param name="rIsFocused"></param>
        private void DrawModuleListItem(Rect rRect, int rIndex, bool rIsActive, bool rIsFocused)
        {
            if (rIndex >= mTarget.Modules.Count) { return; }

            var lItem = mTarget.Modules[rIndex];
            if (lItem == null)
            {
                EditorGUI.LabelField(rRect, "NULL");
                return;
            }

            rRect.y += 2;            
            float lHSpace = 5f;
            float lFlexVSpace = rRect.width - lHSpace - lHSpace - 40f - lHSpace - 16f;
            
            // Grey out any Undefined modules
            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginDisabledGroup(lItem.GetType() == typeof(UndefinedSetupModule));
            Rect lTypeRect = new Rect(rRect.x, rRect.y, lFlexVSpace, EditorGUIUtility.singleLineHeight);
            EditorGUI.LabelField(lTypeRect, lItem.Name);
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// Update the Profile when a module is added to the list
        /// </summary>
        /// <param name="rList"></param>
        private void OnModuleListItemAdd(ReorderableList rList)
        {
            if (mModuleTypeIndex >= mModuleTypes.Count) { return; }

            try
            {
                Type lType = mModuleTypes[mModuleTypeIndex];
                var lModule = Activator.CreateInstance(lType) as SetupModule;
                if (lModule == null)
                {
                    Log.ConsoleWrite("Could not create instance of Character Wizard Module: " + lType.FullName);
                    return;
                }

                if (lModule.GetType() != lType)
                {
                    Log.ConsoleWrite(string.Format("Module type doesn't match. Expected {0}, Actual {1}", lModule.GetType(), lType));
                    return;
                }

                // Check if any other modules are required
                var lRequired = ModuleRequiresAttribute.GetRequiredTypes(lType);
                if (lRequired != null)
                {
                    foreach (Type t in lRequired)
                    {
                        Debug.Log("Required Module: " + t.FullName);
                    }
                }

                mTarget.Modules.Add(lModule);
                mTarget._ModuleDefinitions.Add(lModule.Serialize());
                lModule.Initialize(true);

                rList.index = mTarget.Modules.Count - 1;
                OnModuleListItemSelect(rList);

                mIsDirty = true;
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        /// <summary>
        /// Update the Profile when a module is removed from the list
        /// </summary>
        /// <param name="rList"></param>
        private void OnModuleListItemRemove(ReorderableList rList)
        {
            if (!EditorUtility.DisplayDialog("Warning!", "Are you sure you want to remove the module?", "Yes", "No"))
                return;

            int rIndex = rList.index;
            rList.index--;

            // Remove the item
            mTarget.Modules.RemoveAt(rIndex);
            mTarget._ModuleDefinitions.RemoveAt(rIndex);

            // Set the new index
            OnModuleListItemSelect(rList);

            mIsDirty = true;
        }

        /// <summary>
        /// Update the Profile when the module list is reordered
        /// </summary>
        /// <param name="rList"></param>
        private void OnModuleListReorder(ReorderableList rList)
        {
            // Update the module definitions
            mTarget._ModuleDefinitions.Clear();

            foreach (var lModule in mTarget.Modules)
            {
                mTarget._ModuleDefinitions.Add(lModule.Serialize());
            }
            mIsDirty = true;
        }

        /// <summary>
        /// Set the selected module index on the target object
        /// </summary>
        /// <param name="rList"></param>
        private void OnModuleListItemSelect(ReorderableList rList)
        {
            //Debug.Log("Module selected: " + rList.index);
            mTarget.EditorModuleIndex = rList.index;
        }

        /// <summary>
        /// Draw the details of the selected module (below the list)
        /// </summary>
        /// <param name="rItem"></param>
        /// <returns></returns>
        private bool DrawModuleDetailItem(SetupModule rItem)
        {
            bool lIsDirty = false;
            if (rItem == null)
            {
                EditorGUILayout.LabelField("NULL");
                return false;
            }

            Type lType = rItem.GetType();
            EditorGUI.BeginDisabledGroup(lType == typeof(UndefinedSetupModule));
            if (rItem.Name.Length > 0)
            {
                EditorHelper.DrawSmallTitle(rItem.Name.Length > 0 ? rItem.Name : "Module");
            }
            else
            {
                string lName = ModuleNameAttribute.GetName(rItem.GetType());
                EditorHelper.DrawSmallTitle(lName.Length > 0 ? lName : "Module");
            }

            string lDescription = ModuleDescriptionAttribute.GetDescription(rItem.GetType());
            if (lDescription.Length > 0)
            {
                EditorHelper.DrawInspectorDescription(lDescription, MessageType.None);
            }            

            // Draw the Inspector for the individual module
            bool lModuleDirty = rItem.OnInspectorGUI(mTarget);
            if (lModuleDirty) { lIsDirty = true; }

            EditorGUI.EndDisabledGroup();

            if (lIsDirty)
            {
                UpdateModuleDefinition(mModuleList.index, rItem);
            }

            return lIsDirty;
        }

        /// <summary>
        /// Update the serialized module definition
        /// </summary>
        /// <param name="rIndex"></param>
        /// <param name="rModule"></param>
        private void UpdateModuleDefinition(int rIndex, SetupModule rModule)
        {
            if (!IsValidIndex(rIndex)) { return; }
            try
            {
                var lSerialized = rModule.Serialize();
                //Debug.Log("Updating Module Definition: " + lSerialized);
                mTarget._ModuleDefinitions[rIndex] = lSerialized;
            }
            catch (IndexOutOfRangeException ex)
            {
                Debug.LogError("Invalid index for mTarget._ModuleDefinitions: " + rIndex);
                Debug.LogException(ex);
            }

        }

        /// <summary>
        /// Check if the Profile already contains the specified module
        /// </summary>
        /// <param name="rType"></param>
        /// <returns></returns>
        private bool HasModuleType(Type rType)
        {
            // Allowed to have more than one Undefined module            
            if (rType == typeof (UndefinedSetupModule)) { return false; }
            return (mTarget.Modules.Any(x => x.GetType() == rType));
        }

        /// <summary>
        /// Checks if the specified list index is valid
        /// </summary>
        /// <param name="rIndex"></param>
        /// <returns></returns>
        private bool IsValidIndex(int rIndex)
        {
            return (rIndex >= 0 && rIndex < mModuleList.count);
        }
    }
}

