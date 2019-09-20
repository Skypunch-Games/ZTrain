using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using UnityEngine;
using UnityEditor;
using com.ootii.Base;
using com.ootii.Helpers;

namespace com.ootii.Graphics.NodeGraph
{
    /// <summary>
    /// Generic selection window for types given a specific base type.
    /// </summary>
    public class TypeSelectWindow : EditorWindow
    {
        /// <summary>
        /// Delegate to set the parent list
        /// </summary>
        /// <param name="rParent"></param>
        public delegate void SelectDelegate(Type rEntry, object rExtraData);

        /// <summary>
        /// Base type whose intries will inhert from
        /// </summary>
        private Type mBaseType = null;
        public Type BaseType
        {
            get { return mBaseType; }

            set
            {
                mBaseType = value;
                RefreshList();
            }
        }

        /// <summary>
        /// Search string to initialize
        /// </summary>
        private string mSearchString = "";
        public string SearchString
        {
            get { return mSearchString; }

            set
            {
                mSearchString = value;
                RefreshList();
            }
        }

        /// <summary>
        /// Extra data that is passed back with the callback
        /// </summary>
        private object mUserData = null;
        public object UserData
        {
            get { return mUserData; }
            set { mUserData = value; }
        }

        /// <summary>
        /// Function to call when the parent is selected
        /// </summary>
        public SelectDelegate SelectedEvent = null;

        // Types that correspond to the names
        private List<Type> mTypes = new List<Type>();

        // Names to display
        private List<string> mNames = new List<string>();

        // Descriptions to display
        private List<string> mDescriptions = new List<string>();

        // Track the selected item in the list
        private int mSelectedItemIndex = -1;

        // Editor control
        private Vector2 mScrollPosition = new Vector2();

        /// <summary>
        /// Frame update for GUI objects. Heartbeat of the window that 
        /// allows us to update the UI
        /// </summary>
        public void OnGUI()
        {
            // Render the search box
            GUILayout.BeginHorizontal(GUI.skin.FindStyle("Toolbar"));

            string lOriginalSearch = mSearchString;

            mSearchString = GUILayout.TextField(lOriginalSearch, GUI.skin.FindStyle("ToolbarSeachTextField"));
            if (mSearchString != lOriginalSearch) { RefreshList(); }

            if (GUILayout.Button("", GUI.skin.FindStyle("ToolbarSeachCancelButton")))
            {
                // Remove focus if cleared
                mSearchString = "";
                GUI.FocusControl(null);

                RefreshList();
            }

            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();

            // Render the list
            mScrollPosition = GUILayout.BeginScrollView(mScrollPosition, EditorHelper.ScrollArea);

            if (mNames != null)
            {
                for (int i = 0; i < mNames.Count; i++)
                {
                    GUIStyle lRowStyle = (i == mSelectedItemIndex ? EditorHelper.SelectedLabel : EditorHelper.Label);
                    if (GUILayout.Button(mNames[i], lRowStyle, GUILayout.MinWidth(100)))
                    {
                        mSelectedItemIndex = i;
                    }
                }
            }

            GUILayout.EndScrollView();

            GUILayout.Space(5f);

            GUILayout.BeginVertical();

            string lName = (mSelectedItemIndex < 0 || mSelectedItemIndex >= mNames.Count ? "" : mNames[mSelectedItemIndex]);
            GUILayout.Label(lName, TitleStyle, GUILayout.Width(150f));

            GUILayout.Space(3f);

            string lDescription = (mSelectedItemIndex < 0 || mSelectedItemIndex >= mDescriptions.Count ? "" : mDescriptions[mSelectedItemIndex]);
            GUILayout.Label(lDescription, DescriptionStyle, GUILayout.Width(150f));

            GUILayout.EndVertical();

            GUILayout.EndHorizontal();

            GUILayout.Space(5);

            // Selection button
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Select", GUILayout.Width(70)))
            {
                if (SelectedEvent != null)
                {
                    if (mSelectedItemIndex >= 0 && mSelectedItemIndex < mTypes.Count)
                    {
                        SelectedEvent(mTypes[mSelectedItemIndex], mUserData);
                    }
                }

                mSelectedItemIndex = -1;
                Close();
            }

            GUILayout.Space(5);

            if (GUILayout.Button("Cancel", GUILayout.Width(70)))
            {
                mSelectedItemIndex = -1;
                Close();
            }

            GUILayout.Space(5);

            GUILayout.EndHorizontal();

            GUILayout.Space(10);
        }

        /// <summary>
        /// Grab the types from the assembly
        /// </summary>
        public void RefreshList()
        {
            mTypes.Clear();
            mNames.Clear();
            mDescriptions.Clear();

            mSelectedItemIndex = -1;

            if (mBaseType == null) { return; }

            // CDL 07/04/2018 - this only looks in the assembly containing the base type
            //// Generate the list of motions to display
            //Assembly lAssembly = Assembly.GetAssembly(mBaseType);
            //Type[] lMotionTypes = lAssembly.GetTypes().OrderBy(x => x.Name).ToArray<Type>();
            //for (int i = 0; i < lMotionTypes.Length; i++)
            //{
            //    Type lType = lMotionTypes[i];

            //    if (lType.IsAbstract) { continue; }

            //    if (!mBaseType.IsAssignableFrom(lType)) { continue; }

            //    string lFriendlyName = BaseNameAttribute.GetName(lType);
            //    if (mSearchString.Length > 0)
            //    {
            //        if (lFriendlyName.IndexOf(mSearchString) < 0)
            //        {
            //            if (lType.Name.IndexOf(mSearchString) < 0)
            //            {
            //                continue;
            //            }
            //        }
            //    }

            //    string lDescription = BaseDescriptionAttribute.GetDescription(lType);

            //    mTypes.Add(lType);
            //    mNames.Add(lFriendlyName);
            //    mDescriptions.Add(lDescription);
            //}

            // CDL 07/04/2018 - used the cached found types from all assemblies
            // Create the list to display
            List<Type> lFoundTypes = AssemblyHelper.FoundTypes;
            for (int i = 0; i < lFoundTypes.Count; i++)
            {
                Type lType = lFoundTypes[i];
                if (lType.IsAbstract) { continue; }

                if (!mBaseType.IsAssignableFrom(lType)) { continue; }

                string lFriendlyName = BaseNameAttribute.GetName(lType);
                if (mSearchString.Length > 0)
                {
                    if (lFriendlyName.IndexOf(mSearchString) < 0)
                    {
                        if (lType.Name.IndexOf(mSearchString) < 0)
                        {
                            continue;
                        }
                    }
                }

                string lDescription = BaseDescriptionAttribute.GetDescription(lType);

                mTypes.Add(lType);
                mNames.Add(lFriendlyName);
                mDescriptions.Add(lDescription);
            }
        }

        private static GUIStyle mTitleStyle = null;
        public static GUIStyle TitleStyle
        {
            get
            {
                if (mTitleStyle == null)
                {
                    mTitleStyle = new GUIStyle(GUI.skin.label);
                    mTitleStyle.alignment = TextAnchor.UpperLeft;
                    mTitleStyle.fontSize = 14;
                    mTitleStyle.fontStyle = FontStyle.Bold;
                    mTitleStyle.wordWrap = true;
                }

                return mTitleStyle;
            }
        }

        private static GUIStyle mDescriptionStyle = null;
        public static GUIStyle DescriptionStyle
        {
            get
            {
                if (mDescriptionStyle == null)
                {
                    mDescriptionStyle = new GUIStyle(GUI.skin.label);
                    mDescriptionStyle.alignment = TextAnchor.UpperLeft;
                    mDescriptionStyle.fontSize = 12;
                    mDescriptionStyle.wordWrap = true;
                }

                return mDescriptionStyle;
            }
        }
    }
}

