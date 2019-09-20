using System;
using System.Collections.Generic;
using System.Reflection;
using com.ootii.Actors.AnimationControllers;
using com.ootii.Helpers;

namespace com.ootii.Setup
{
    /// <summary>
    /// Class containing the relevant info used in setting up Motion Packs via script. The fields are populated
    /// via reflection.
    /// </summary>
    [Serializable]
    public class MotionPackInfo
    {
        /// <summary>
        /// Name of the motion pack
        /// </summary>
        public string Name;

        /// <summary>
        /// A string representing the type of the Motion Pack Definition file; this is used for serialization
        /// </summary>
        public string TypeString;

        /// <summary>
        /// Description of the motion pack (optional)
        /// </summary>
        public string Description;

        /// <summary>
        /// Type of the Motion Pack Definition files (used to get methods and properties via reflection)
        /// </summary>
        public Type Type
        {
            get
            {
                if (mType == null)
                {
                    if (TypeString == null) { return null; }
                    mType = AssemblyHelper.ResolveType(TypeString);
                }

                return mType;
            }
        }
        private Type mType = null;
    }

    /// <summary>
    /// Helper functions for use when setting up Motion Packs
    /// </summary>
    public class MotionPackSetupHelper 
    {
#if UNITY_EDITOR
        /// <summary>
        /// Cached list of Motion Pack types detected in the project
        /// </summary>        
        public static List<Type> PackTypes
        {
            get
            {
                if (mPackTypes == null)
                {
                    InitializePackLists();
                }
                return mPackTypes;
            }
        }
        private static List<Type> mPackTypes;

        /// <summary>
        /// Cached list of Motion Pack names 
        /// </summary>
        public static List<string> PackNames
        {
            get
            {
                if (mPackNames == null)
                {
                    InitializePackLists();
                }
                return mPackNames;
            }
        }
        private static List<string> mPackNames;         

        public static List<MotionPackInfo> GetPackInfo()
        {          
            var lItems = new List<MotionPackInfo>();
            for (int i = 0; i < PackTypes.Count; i++)
            {
                var lInfo = new MotionPackInfo
                {
                    Name = PackNames[i],
                    TypeString = PackTypes[i].FullName                    
                };

                lItems.Add(lInfo);
            }

            return lItems;
        }
       
        /// <summary>
        /// Create the cached lists of pack names and types 
        /// </summary>
        private static void InitializePackLists()
        {
            mPackTypes = new List<Type>();
            mPackNames = new List<string>();

            // Use the cached found types to build a list of motion packs present
            List<Type> lFoundTypes = AssemblyHelper.FoundTypes;
            foreach (Type lType in lFoundTypes)
            {
                // We just want concrete implementations of MotionPackDefinition
                if (lType.IsAbstract) continue;
                if (!typeof(MotionPackDefinition).IsAssignableFrom(lType)) continue;

                PropertyInfo[] lStaticMethods = lType.GetProperties(BindingFlags.Static | BindingFlags.Public);
                MethodInfo lSetupMethod = lType.GetMethod("SetupPack", BindingFlags.Static | BindingFlags.Public);
                if (lSetupMethod == null) continue;

                string lPackName = "";
                //bool? lHidePack = false;                

                // Get the static properties that we need to check
                foreach (PropertyInfo lPropertyInfo in lStaticMethods)
                {
                    switch (lPropertyInfo.Name)
                    {
                        case "PackName":
                            lPackName = lPropertyInfo.GetValue(null, null) as string;
                            break;
                        //case "HidePack":
                        //    lHidePack = lPropertyInfo.GetValue(null, null) as bool?;
                        //    break;                        
                    }
                }

                // Assign default value if lHidePack is null.
                //if (!lHidePack.HasValue) { lHidePack = false; }

                //if ((bool) lHidePack || string.IsNullOrEmpty(lPackName) || mPackNames.IndexOf(lPackName) >= 0) continue;
                if (string.IsNullOrEmpty(lPackName) || mPackNames.IndexOf(lPackName) >= 0) continue;

                mPackNames.Add(lPackName);
                mPackTypes.Add(lType);                
            }
        }
#endif
    }
}
