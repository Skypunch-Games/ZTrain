using UnityEngine;
using System.Collections;
using com.ootii.Helpers;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace com.ootii.Setup
{
    public class AnimationPathManager
    {
#if UNITY_EDITOR
        public string AnimationPath
        {
            get { return mAnimationPath; }            
        }
        protected string mAnimationPath;

        public bool IsVerified { get { return VerifyAnimationFile(); } }        

        // The EditorPrefs key used to store the animation path
        protected string mPrefsKey;

        // The default path to the animations (used if no stored path)
        protected string mDefaultPath;

        // The file name to check when verifying if the animations are present at the specified path
        protected string mFileName;

        // The title (Motion Pack Name, etc) to display in the label fields
        protected string mTitle;

        public AnimationPathManager(string rDefaultPath, string rFileName, string rPrefsKey, string rTitle)
        {
            mDefaultPath = rDefaultPath;            
            mFileName = rFileName;
            mPrefsKey = rPrefsKey;
            mTitle = rTitle;

            mAnimationPath = LoadPath();            
            VerifyAnimationFile();
        }

        protected string LoadPath()
        {
            string lPath = AssetHelper.GetStoredPath(mPrefsKey, mDefaultPath);            
            // We don't want a trailing "/" character on the animator paths
            return lPath.EndsWith("/") ? lPath.Substring(0, lPath.Length - 2) :lPath;
        }

        protected bool VerifyAnimationFile()
        {            
            return AssetHelper.VerifyFilePath(mAnimationPath + "/" + mFileName);            
        }

        public virtual bool OnInspectorGUI()
        {             
            if (VerifyAnimationFile())
            {
                if (!string.IsNullOrEmpty(mTitle))
                {
                    EditorGUILayout.LabelField("Path to " + mTitle + " animations verified.");
                }
                else
                {
                    EditorGUILayout.LabelField("Animation path verified.");
                }
            }
            else
            {
                // Display message to user
                EditorGUILayout.HelpBox((string.IsNullOrEmpty(mTitle) ? "The" : mTitle) + " animations could not be located. Enter the path to the animation .fbx files.", MessageType.Warning);
                EditorGUILayout.Space();

                bool lPathChanged = false;
                string lLabelText = string.IsNullOrEmpty(mTitle)
                    ? "Path to Animations"
                    : "Path to " + mTitle + " Animations";
                string lAnimationPath = EditorHelper.FolderSelect(
                        new GUIContent(lLabelText, "Path to the folder containing the animation files."),
                        mAnimationPath,
                        out lPathChanged);

                if (lPathChanged)
                {                    
                    mAnimationPath = lAnimationPath;                    
                    if (VerifyAnimationFile())
                    {
                        // If the path is valid and we can verify the animation file, then store the path in EditorPrefs
                        EditorPrefs.SetString(mPrefsKey, mAnimationPath);
                    }
                }
            }

            return true;
        }
#endif
    }
}
