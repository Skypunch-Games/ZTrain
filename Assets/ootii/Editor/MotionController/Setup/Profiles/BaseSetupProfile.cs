using System;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using com.ootii.Helpers;
using com.ootii.Setup.Modules;
using com.ootii.Utilities;
using UnityEditor;


namespace com.ootii.Setup
{
    /// <summary>
    /// Base class for a Setup Profile that contains a list of active Setup Modules
    /// </summary>
    public abstract class BaseSetupProfile : ScriptableObject
    {
        /// <summary>
        /// The list of Setup Modules which have been added to the profile
        /// </summary>
        public List<SetupModule> Modules = new List<SetupModule>();

        /// <summary>
        /// The serialized string definitions which correspond to the list of Modules
        /// </summary>
        public List<string> _ModuleDefinitions = new List<string>();

        /// <summary>
        /// The currently selected Module in the Inspector's ReorderableList. This prevents us from 
        /// having the selection reset every time Unity reserializes the asset.
        /// </summary>
        public int EditorModuleIndex = -1;

        // This is set to true once it has been renamed in the inspector. This is checked if we only want to change
        // something the first time a user manually renames the asset.
        public bool _Renamed = false;


        /// <summary>
        /// Instantiate the list of Setup Modules
        /// </summary>
        public virtual void InitializeModules()
        {
            int lDefCount = _ModuleDefinitions.Count;
            //Debug.Log(string.Format("Initializing Modules ({0}).", lDefCount));

            // First remove any extras beyond what we have definitions for
            for (int i = Modules.Count - 1; i >= lDefCount; i--) { Modules.RemoveAt(i); }
           
            for (int i = 0; i < lDefCount; i++)
            {
                var lDefinition = _ModuleDefinitions[i];
                var lNode = JSONNode.Parse(lDefinition);
                if (lNode == null) continue;

                var lTypeString = lNode["__Type"].Value;

                bool lUpdateType;
                Type lType = AssemblyHelper.ResolveType(lTypeString, out lUpdateType);
                if (lType == null)
                {
                    // Add an Undefined Module as a placeholder, but don't serialize it
                    var lUndefinedModule = new UndefinedSetupModule(lDefinition);
                    if (Modules.Count <= i)
                    {
                        Modules.Add(lUndefinedModule);
                    }
                    else
                    {
                        Modules[i] = lUndefinedModule;
                    }
                    continue;
                }

                SetupModule lModule = null;

                // If no module matching the type, we need to instantiate one                
                if (Modules.Count <= i || lType != Modules[i].GetType())
                {
                    lModule = Activator.CreateInstance(lType) as SetupModule;                    

                    if (Modules.Count <= i)
                    {
                        Modules.Add(lModule);
                    }
                    else
                    {
                        Modules[i] = lModule;
                    }
                }
                // Assign the module if it matches
                else
                {
                    lModule = Modules[i];
                }

                // Fill the module with data from the definition
                if (lModule != null)
                {
                    lModule.Deserialize(lDefinition);

                    // Update the serialized Type if necessary
                    if (lUpdateType)
                    {
                        _ModuleDefinitions[i] = lModule.Serialize();
                    }
                }
            }

            // Allow each item to initialize now that it has been deserialized
            foreach (var lModule in Modules)
            {
                lModule.Initialize();
            }
        }
       

        #region AssetManagement



        /// <summary>
        /// Provides the path to the asset
        /// </summary>
        public string AssetPath
        {
            get { return AssetDatabase.GetAssetPath(this); }
        }

        /// <summary>
        /// Provides only the filename of the asset
        /// </summary>
        public string FileName
        {
            get { return Path.GetFileName(AssetPath); }
        }

        /// <summary>
        /// Rename this profile (changes the .asset filename)
        /// </summary>
        /// <param name="rName"></param>
        /// <param name="rSetRenamed"></param>
        public void Rename(string rName, bool rSetRenamed)
        {
            if (string.IsNullOrEmpty(rName)) return;

            try
            {
                string lNewName = rName.EndsWith(".asset") ? rName : rName + ".asset";
                AssetDatabase.RenameAsset(AssetPath, lNewName);
               
                if (!_Renamed) { OnFirstRenamed(rSetRenamed); }
            }
            catch (IOException ex)
            {
                Debug.LogException(ex);
            }
        }

        protected virtual void OnFirstRenamed(bool rSetRenamed)
        {
            if (rSetRenamed) { _Renamed = true; }
        }
        #endregion AssetManagement

    }

}

