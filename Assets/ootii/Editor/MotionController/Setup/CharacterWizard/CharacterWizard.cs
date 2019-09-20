using System.Collections;
using com.ootii.Helpers;
using com.ootii.Actors.AnimationControllers;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using com.ootii.Base;
using com.ootii.Geometry;
using com.ootii.Utilities;

namespace com.ootii.Setup
{
    public class CharacterWizard : ToolWindow<CharacterWizard>
    {
        /// <summary>
        /// EditorPrefs keys for the Character Wizard
        /// </summary>
        public static class PrefsKey
        {
            public static readonly string ActiveProfile = "ootii_CharacterWizard_ActiveProfile";
            public static readonly string AnimatorPath = "ootii_CharacterWizard_AnimatorPath";
            public static readonly string AnimatorName = "ootii_CharacterWizard_AnimatorName";
            public static readonly string Mode = "ootii_CharacterWizard_DisplayMode";
        }

        public enum CharacterRole
        {
            Player,
            NPC
        }

        public enum WizardStatus
        {
            Configuring,
            Success,
            Failed
        }

        public static readonly string DefaultProfileFolder = DefaultPaths.MotionControllerContent + "Data/Setup/Profiles/";
        public static readonly string DefaultCustomProfileFolder = DefaultPaths.CustomContent + "Data/Setup/Profiles/";
        public static readonly string DefaultAnimatorFolder = DefaultPaths.CustomContent + "Animators/";        
        public static readonly string DefaultAnimatorName = "BasicHumanoid";

        public static readonly string DefaultProfileName = "Basic Motions (Default).asset";
        public static readonly string NewProfileName = "Custom Character Profile.asset";
        
        private static readonly string WindowTitle = "ootii Character Wizard";

        private const float ProfileButtonSize = 100;
        private const float ProfileOptionWidth = 180;
        private const float ProfileOptionHeight = 160;
        private const float ProfileOptionMargin = 25;

        // Flag that determines when the character build state is valid
        public bool CanBuild
        {
            get
            {
                // Must have both a character model and a profile assigned
                if (Character == null || ActiveProfile == null)
                {
                    return false;
                }

                // The profile must either be set to create a new animator controller or must have an existing
                // one assigned
                if (!ActiveProfile.CreateNewAnimator && ActiveProfile.AnimatorController == null)
                {
                    return false;
                }

                return true;                
            }
        }
        public bool IsPlayer { get { return CharacterType == CharacterRole.Player; } }
        

        private readonly WaitForSeconds mShortWait = new WaitForSeconds(0.25f);
       

        #region GUI Styles

        private GUIStyle mProfileOptionBoxStyle;
        private GUIStyle ProfileOptionBoxStyle
        {
            get
            {
                if (mProfileOptionBoxStyle == null)
                {
                    mProfileOptionBoxStyle = new GUIStyle(GUI.skin.box)
                    {                        
                        fixedWidth = ProfileOptionWidth,
                        fixedHeight = ProfileOptionHeight,                        
                    };

                    mProfileOptionBoxStyle.normal.background = null;

                }
                return mProfileOptionBoxStyle;
            }
        }

        private GUIStyle mProfileButtonStyle;
        private GUIStyle ProfileButtonStyle
        {
            get
            {
                if (mProfileButtonStyle == null)
                {
                    mProfileButtonStyle = new GUIStyle(GUI.skin.button)
                    {
                        fixedHeight = ProfileButtonSize,
                        fixedWidth = ProfileButtonSize
                    };
                }

                return mProfileButtonStyle;                
            }
        }

        private GUIStyle mProfileLabelStyle;
        private GUIStyle ProfileLabelStyle
        {
            get
            {
                if (mProfileLabelStyle == null)
                {
                    mProfileLabelStyle = new GUIStyle(GUI.skin.label)
                    {
                        fontStyle = FontStyle.Bold,
                        alignment = TextAnchor.MiddleCenter,
                        wordWrap = true,
                        stretchHeight = true
                    };
                }
                return mProfileLabelStyle;
            }
        }


        private GUIStyle mSmallButtonStyle;
        private GUIStyle SmallButtonStyle
        {
            get
            {
                if (mSmallButtonStyle == null)
                {
                    mSmallButtonStyle = new GUIStyle(EditorHelper.TinyButton)
                    {
                        margin = new RectOffset(0, 0, 2, 0)
                    };

                }

                return mSmallButtonStyle;
            }
        }

        #endregion GUI Styles


        #region Serialized Fields

        [Tooltip("The Character Wizard Profile to load.")]
        public CharacterWizardProfile ActiveProfile;
        
        [Tooltip("GameObject to use for this character.")]
        public GameObject Character = null;               

        [Tooltip("The path to the folder where a new Animator Controller will be created.")]
        public string AnimatorPath = string.Empty;

        [Tooltip("Is the character a Player or an AI-controlled NPC?")]
        public CharacterRole CharacterType = CharacterRole.Player;
            
        [Tooltip("Name of the created character.")]
        public string Name = string.Empty;

        [Tooltip("Ovewrite existing layers when applying a Layer Definition Set?")]
        public bool OverwriteExistingLayers = true;

        #endregion Serialized Fields


        #region Private Fields
        
        private readonly List<CharacterWizardProfile> mBasicModeProfiles = new List<CharacterWizardProfile>();

        // The custom inspector for the active Profile. We use this to inject its OnInspectorGUI() code into
        // the editor GUI that we're assembling here
        private CharacterWizardProfileEditor mActiveProfileEditor = null;                
        
        // The title to display in the header
        private string mHeaderTitle = WindowTitle;

        // The current ToolMode of the Character Wizard window
        private ToolMode mToolMode = ToolMode.Basic;

        private bool mDebugEnabled = false;

        // ===================================================
        // Foldout states
        private bool mDebugFoldout = false;
        private bool mCharacterFoldout = true;
        private bool mProfileSelectFoldout = true;

        // ===================================================
        // Window dimensions
        private const float MinWidth = 800f;
        private const float MinHeight = 800f;
        private const float MaxWidth = 800f;
        private const float MaxHeight = 1200f;

        #endregion Private Fields              
               

        #region Menu Commands

        /// <summary>
        /// Opens the Character Wizard window
        /// </summary>
        [MenuItem("Window/ootii Tools/Character Wizard", false, 1)]
        public static void ShowWindow()
        {
            CharacterWizard lWindow = GetWindow<CharacterWizard>(WindowTitle, true);
            lWindow.minSize = new Vector2(MinWidth, MinHeight);
            lWindow.maxSize = new Vector2(MaxWidth, MaxHeight);
        }

        /// <summary>
        /// Menu command extension for running the Character Wizard when a character model or prefab is selected.
        /// Once the window opens, the character model fields are pre-set based on the selected object.
        /// </summary>
        [MenuItem("Assets/ootii Character Wizard")]
        private static void RunWizard()
        {
            GameObject lGameObject = Selection.activeObject as GameObject;
            if (lGameObject == null) { return; }

            CharacterWizard lWindow = GetWindow<CharacterWizard>(WindowTitle, true);
            lWindow.minSize = new Vector2(MinWidth, MinHeight);
            lWindow.maxSize = new Vector2(MaxWidth, MaxHeight);
            lWindow.Character = lGameObject;

            //Animator lAnimator = lGameObject.GetComponent<Animator>();
            //if (lAnimator == null) { return; }

            //lWindow.ModelType = lAnimator.isHuman ? RigType.Humanoid : RigType.Generic;

            Debug.Log(string.Format("Running {0} on {1}", WindowTitle, lGameObject.name));
        }

        /// <summary>
        /// Ensures that the menu command is only enabled when a valid object is selected in either the project view
        /// or scene heirarchy.
        /// </summary>
        /// <returns></returns>
        [MenuItem("Assets/Build Character (ootii QuickStart)", true)]
        private static bool ValidateRunWizard()
        {
            GameObject lGameObject = Selection.activeObject as GameObject;
            if (lGameObject == null) { return false; }

            // Check for presence of an Animator. This doesn't guarantee that we're running on a character model,
            // but it removes the option from models which definitely aren't.
            Animator lAnimator = lGameObject.GetComponent<Animator>();
            if (lAnimator == null) { return false; }

            return true;
        }

        #endregion Menu Commands
      

        #region Initialization

        /// <summary>
        /// Perform required setup when the window is first activated
        /// </summary>
        protected override void OnEnable()
        {
            base.OnEnable();
            
            // Restore saved settings
            AnimatorPath = EditorPrefs.GetString(PrefsKey.AnimatorPath, DefaultAnimatorFolder);

            // Restore the saved tool mode, or default to Basic
            //mToolMode = (ToolMode) EditorPrefs.GetInt(PrefsKey.Mode, 0);            
            SetToolMode((ToolMode)EditorPrefs.GetInt(PrefsKey.Mode, 0));          
        }

        #endregion Initialization


        #region Profile and Tool Mode Management
        
        
        private void SetToolMode(ToolMode rToolMode, bool rUserInitiated = false)
        {
            if (rToolMode == ToolMode.Basic)
            {
                // Ensure that the Basic mode profiles have been loaded
                LoadBasicModeProfiles(false);
            }

            // If the user clicked an editor tab and changed the mode:
            if (rUserInitiated && mToolMode != rToolMode)
            {
                // Advanced to Basic
                if (rToolMode == ToolMode.Basic)
                {
                    // Store the path of the current active profile (if it is a user-created one) so that it can be reloaded
                    if (ActiveProfile != null && ActiveProfile.Priority == CharacterWizardProfile.PriorityStatus.User)
                    {
                        ActiveProfile.StorePath(PrefsKey.ActiveProfile);
                    }
                }
                // Basic to Advanced
                else if (rToolMode == ToolMode.Advanced)
                {                    
                    //if (ActiveProfile != null)
                    //{
                    //    // If a profile was selected in Basic mode, create a new copy of it in the user's folder
                    //    UseCopyOfCurrentProfile();
                    //}
                    //else
                    //{
                    //    // Use the last selected profile
                    //    UseStoredProfile();
                    //}                    
                }

                // Save the new tool mode when user-initiated
                EditorPrefs.SetInt(PrefsKey.Mode, (int)rToolMode);                
            }
            // Called from OnEnable()
            else if (!rUserInitiated)
            {
                // When starting in Advanced, load the last stored profile
                if (rToolMode == ToolMode.Advanced)
                {
                    UseStoredProfile();
                }
            }


            mToolMode = rToolMode;          
        }
        
        /// <summary>
        /// Load all of the Basic mode profiles (located in the Default Profile Folder)
        /// </summary>
        /// <param name="rSetActive"></param>
        private void LoadBasicModeProfiles(bool rSetActive)
        {
            mBasicModeProfiles.Clear();
            var lProfiles = AssetHelper.LoadAssets<CharacterWizardProfile>(DefaultProfileFolder)
                .OrderBy(x => x.Priority);
                       
            foreach (var lProfile in lProfiles)
            {
                // Check required Motion Packs before adding to list
                if (lProfile.CheckRequiredMotionPacks())
                {
                    mBasicModeProfiles.Add(lProfile);
                }                                        
            }      
            
            if (rSetActive)
            {
                ActiveProfile = mBasicModeProfiles.FirstOrDefault();
            }
        }               
       
        /// <summary>
        /// Load a stored profile and set it to the Active Profile
        /// </summary>        
        private void UseStoredProfile()
        {
            ActiveProfile = CharacterWizardProfile.LoadFromStoredPath(PrefsKey.ActiveProfile);            
            OnActiveProfileChanged(true);
        }  
       
        /// <summary>
        /// Called when the ActiveProfile is changed, whether from script or user-initiated
        /// </summary>
        /// <param name="rUpdatePrefs">Update EditorPrefs?</param>
        private void OnActiveProfileChanged(bool rUpdatePrefs)
        {
            // Destroy the current Editor instance
            mActiveProfileEditor = null;
            if (ActiveProfile == null)
            {
                mHeaderTitle = WindowTitle;
                return;
            }

            ActiveProfile.UpdateRequirements();

            mHeaderTitle = WindowTitle + " - " + ActiveProfile.name;

            //Debug.Log("Active Profile Changed: " + ActiveProfile.name + " ToolMode = " + mToolMode.ToString());

            // Get the Editor for the Active Profile
            mActiveProfileEditor = Editor.CreateEditor(ActiveProfile) as CharacterWizardProfileEditor;
            if (mActiveProfileEditor != null)
            {
                mActiveProfileEditor.IsInjectedView = true;
            }
            
            if (rUpdatePrefs)
            {
                // Store this profile as the current selection
                ActiveProfile.StorePath(PrefsKey.ActiveProfile);                
            }
        }

        #endregion Profile and Tool Mode Management

        public IEnumerator CreateCharacter()
        {                        
            if (IsPlayer)
            {
                // If there are already any objects tagged "Player" in the scene, disable them so that there is no conflict
                // with the newly created character
                var lExistingPlayers = GameObject.FindGameObjectsWithTag("Player");
                if (lExistingPlayers != null)
                {
                    foreach (var lPlayer in lExistingPlayers)
                    {
                        if (lPlayer.GetComponent<MotionController>() == null) continue;
                        lPlayer.gameObject.SetActive(false);
                    }
                }
            }

            // Instantiate the character
            if (!string.IsNullOrEmpty(AssetDatabase.GetAssetPath(Character)))
            {
                // Use the entered Name (if not blank); default to the character model/prefab name
                string lName = string.IsNullOrEmpty(Name) ? Character.name : Name;
                Character = GameObject.Instantiate(Character) as GameObject;
                Character.name = lName;                
                yield return mShortWait;
            }            

            Character.SetActive(true);

            // Add the Motion Controller component
            MotionController lMotionController = CharacterSetupHelper.CreateMotionController(Character, IsPlayer);            
            yield return mShortWait;


            this.StartCoroutine(ActiveProfile.CreateCharacter(lMotionController, AnimatorPath, Name, IsPlayer));
            
            // Wait for the profile to finish
            while (ActiveProfile.IsWorking)
            {
                yield return mShortWait;
            }

            yield return mShortWait;

            EditorUtility.ClearProgressBar();
            Debug.Log("Character Wizard: Completed");

            yield return mShortWait;

            this.Close();
        }

        

        #region Draw GUI

        /// <summary>
        /// The main editor GUI function
        /// </summary>
        private void OnGUI()
        {
            mSerializedObject.Update();

            EditorHelper.DrawWindowHeader(mHeaderTitle, "The Character Wizard allows you to quickly set up a new character, using " +
                "sensible default values that you can modify later.\n\n" +
                "In Basic Mode, you select from a set of pre-created profiles.\n" +
                "In Advanced Mode, you can create and edit your own setup profiles.\n\n" +
                "IMPORTANT: Note that some configurations will take 30 seconds or more to complete, during which time the " +
                "Unity Editor may appear unresponsive. This is common when creating a new Animator Controller when multiple " +
                "Motion Packs are being applied.");   
            DrawEditorTabs();         

            if (mToolMode == ToolMode.Advanced)
            {
                OnAdvancedGUI();
            }
            else
            {
                OnBasicGUI();
            }            

            mSerializedObject.ApplyModifiedProperties();
        }        

        /// <summary>
        /// Draw the Basic and Advanced Mode tabs
        /// </summary>
        private void DrawEditorTabs()
        {
            EditorGUILayout.BeginHorizontal();

            //GUILayout.FlexibleSpace();
            GUILayout.Space(10);
            EditorGUILayout.BeginHorizontal();
                GUILayout.Label("", EditorHelper.BasicIcon, GUILayout.Width(16), GUILayout.Height(16));
                if (GUILayout.Button("Basic", EditorStyles.miniButton, GUILayout.Width(70)))
                {
                    SetToolMode(ToolMode.Basic, true);
                }
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(20);

            EditorGUILayout.BeginHorizontal();
                GUILayout.Label("", EditorHelper.AdvancedIcon, GUILayout.Width(16), GUILayout.Height(16));
                if (GUILayout.Button("Advanced", EditorStyles.miniButton, GUILayout.Width(70)))
                {
                    SetToolMode(ToolMode.Advanced, true);
                }
            EditorGUILayout.EndHorizontal();

            GUILayout.FlexibleSpace();
            
            //if (GUILayout.Button("", EditorHelper.OrangeGearButton, GUILayout.Width(16), GUILayout.Height(16)))
            //{                
            //    SetToolMode(ToolMode.Configuration, true);
            //}
            //GUILayout.Space(20);
            //if (GUILayout.Button("", EditorHelper.DebugIcon, GUILayout.Width(16), GUILayout.Height(16)))
            //{
            //    mDebugEnabled = !mDebugEnabled;
            //}

            GUILayout.Space(20);

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Separator();
            EditorHelper.DrawLine();
            EditorGUILayout.Separator();
        }
              
        /// <summary>
        /// Draw the project-wide settings UI (Layers, etc)
        /// </summary>
        private void DrawProjectSettings()
        {
            if (LayersSetupHelper.AreDefaultLayersSet()) { return; }

            EditorHelper.DrawInspectorBlock("Project Settings", "", () =>
            {
                EditorHelper.DrawInspectorDescription("The default Motion Controller layers have not been set up.\n\n" +
                "Press the 'Set Layers' button to add the default layer definitions.", MessageType.Warning);
                EditorGUILayout.Separator();
                EditorGUILayout.PropertyField(FindProperty(x => x.OverwriteExistingLayers));

                if (GUILayout.Button("Set Layers"))
                {
                    LayersSetupHelper.ApplyDefaultLayers(OverwriteExistingLayers);
                }
            });           
        }

        /// <summary>
        /// Draw the character model settings UI
        /// </summary>
        private void DrawCharacterSettings()
        {
            EditorGUILayout.Separator();

            if (Character == null)
            {
                EditorGUILayout.HelpBox("Select a character model or prefab.", MessageType.Error);
                GUILayout.Space(5);                
            }
            else
            {
                if (string.IsNullOrEmpty(Name))
                {
                    Name = Character.name;
                }
            }
            EditorGUILayout.PropertyField(FindProperty(x => x.Character));            

            EditorGUILayout.Separator();
            EditorGUILayout.PropertyField(FindProperty(x => x.Name));

            EditorGUILayout.Separator();
            EditorGUILayout.PropertyField(FindProperty(x => x.CharacterType), GUILayout.Width(300f));
        }

        /// <summary>
        /// Draw the footer buttons (Create)
        /// </summary>
        private void DrawAdvancedFooterButtons()
        {
            if (GUILayout.Button("Create " + CharacterType, CommandButtonStyle)) { this.StartCoroutine(CreateCharacter()); }
        }
       

        #endregion Draw GUI


        #region Draw GUI (Advanced)

        /// <summary>
        /// Draw the Advanced Mode GUI
        /// </summary>
        /// <returns></returns>
        private void OnAdvancedGUI()
        {
            try
            {
                mScrollPosition = EditorGUILayout.BeginScrollView(mScrollPosition, ScrollStyle);
                DrawDebugSettings();
                DrawProjectSettings();

                EditorHelper.DrawInspectorBlock("Character Model", "Select a character model and name your character.",
                    DrawCharacterSettings, ref mCharacterFoldout);

                DrawProfileSection();

                // Inject the inspector GUI from the selected profile into the EditorWindow's GUI
                if (mActiveProfileEditor != null)
                {
                    mActiveProfileEditor.OnInspectorGUI();
                }
            }
            finally
            {
                EditorGUILayout.EndScrollView();
            }
            
            EditorHelper.DrawWindowFooter(DrawAdvancedFooterButtons, !CanBuild, TextAlignment.Center);
        }   

        /// <summary>
        /// Draws the Profile section UI
        /// </summary>
        private void DrawProfileSection()
        {
            EditorGUILayout.Separator();
            EditorHelper.DrawLine();
            EditorGUILayout.Separator();

            EditorHelper.DrawInspectorBlock("Profile", "Select a profile to use for character setup.",
                DrawProfileSelect, ref mProfileSelectFoldout);

            if (ActiveProfile != null)
            {
                GUILayout.FlexibleSpace();
                //EditorHelper.DrawLine();
                EditorGUILayout.Separator();
            }
        }

        /// <summary>
        /// Draw the Profile selection controls within the Profile section UI
        /// </summary>
        private void DrawProfileSelect()
        {
            if (ActiveProfile != null && ActiveProfile.ReadOnly)
            {
                EditorHelper.DrawInspectorDescription("The selected profile does not allow editing. Press the 'Copy' button" + 
                    " to make a duplicate for editing");
            }

            if (ActiveProfile != null && !ActiveProfile.HasRequiredPacks)
            {
                EditorHelper.DrawInspectorDescription("One or more of the Motion Packs used by this profile are missing. " +
                    "Select the profile's asset directly in the Project View to edit these requirements.\n\n" +
                    "You may still proceed with setting up a character without the missing Motion Packs.", MessageType.Warning);
            }

            try
            {
                EditorGUILayout.BeginHorizontal();

                if (EditorHelper.ScriptableObjectField("Profile",
                    "The Character Wizard Profile to use for creating this character.", ActiveProfile,
                    typeof (CharacterWizardProfile), this))
                {
                    ActiveProfile = (CharacterWizardProfile) EditorHelper.FieldObjectValue;
                    OnActiveProfileChanged(true);
                }
               
                GUILayout.Space(10);

                EditorGUI.BeginDisabledGroup(ActiveProfile == null);
                if (GUILayout.Button("Copy", SmallButtonStyle, GUILayout.Width(60)))
                {                    
                    // Create a copy of the current profile
                    if (ActiveProfile != null)
                    {
                        ActiveProfile = (ActiveProfile.Priority == CharacterWizardProfile.PriorityStatus.User)
                            ? ActiveProfile.Copy()
                            : ActiveProfile.Copy(DefaultCustomProfileFolder + ActiveProfile.FileName);
                        OnActiveProfileChanged(true);
                    }                   
                }
                EditorGUI.EndDisabledGroup();

                GUILayout.Space(10);

                if (GUILayout.Button("New", SmallButtonStyle, GUILayout.Width(60)))
                {
                    // Create a new profile
                    ActiveProfile = CharacterWizardProfile.Create(DefaultCustomProfileFolder + NewProfileName);
                    OnActiveProfileChanged(true);
                }
                GUILayout.Space(10);
            }
            finally
            {
                EditorGUILayout.EndHorizontal();
            }
           

            if (ActiveProfile != null && !ActiveProfile.ReadOnly)
            {
                EditorGUILayout.HelpBox(
                    "Changing the profile name will also rename the asset file in your project. " +
                    "The change will not take effect until you press Enter or focus on another input field.",
                    MessageType.Info);
                GUILayout.Space(5);
                if (EditorHelper.DelayedTextField("Profile Name",
                    "Enter a new name for the profile. This will rename the profile's .asset file.",
                    ActiveProfile.name, this))
                {
                    if (!string.IsNullOrEmpty(EditorHelper.FieldStringValue))
                    {
                        ActiveProfile.Rename(EditorHelper.FieldStringValue, true);
                        OnActiveProfileChanged(true);
                    }
                }
            }
        }

        #endregion Draw GUI (Advanced)


        #region Draw GUI (Basic)

        /// <summary>
        /// Draw the Basic Mode GUI
        /// </summary>
        /// <returns></returns>
        private void OnBasicGUI()
        {
            try
            {
                mScrollPosition = EditorGUILayout.BeginScrollView(mScrollPosition, ScrollStyle);
                DrawDebugSettings();
                DrawProjectSettings();

                GUILayout.BeginVertical(EditorHelper.GroupBox);

                try
                {
                    GUILayout.BeginVertical(EditorHelper.Box);

                    DrawCharacterSettings();
                    GUILayout.Space(30f);
                    DrawBasicProfiles();
                    GUILayout.FlexibleSpace();
                }
                finally
                {
                    GUILayout.EndVertical();
                }

                GUILayout.EndVertical();
                EditorGUILayout.Separator();
            }
            finally
            {
                EditorGUILayout.EndScrollView();
            }           
        }   

        private void DrawBasicProfiles()
        {
            const int ElementsInRow = 3;
            try
            {
                EditorGUILayout.BeginVertical();                
                EditorGUI.BeginDisabledGroup(Character == null);

                for (int i = 0; i < mBasicModeProfiles.Count; i++)
                {
                    if (i % ElementsInRow == 0)
                    {
                        EditorGUILayout.BeginHorizontal();
                        GUILayout.FlexibleSpace();
                    }

                    DrawBasicProfileOption(mBasicModeProfiles[i]);
                    GUILayout.Space(ProfileOptionMargin);

                    if (((i + 1) % ElementsInRow == 0) || i == mBasicModeProfiles.Count -1)
                    {
                        GUILayout.FlexibleSpace();
                        EditorGUILayout.EndHorizontal();
                    }
                }
            }
            finally
            {
                EditorGUI.EndDisabledGroup();
                EditorGUILayout.EndVertical();
            }          
        }

        private void DrawBasicProfileOption(CharacterWizardProfile lProfile)
        {            
            if (lProfile == null) { return; }
                       
            try
            {                
                EditorGUILayout.BeginVertical(ProfileOptionBoxStyle);

                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();                
                if (GUILayout.Button(new GUIContent(lProfile.Image, lProfile.Description), ProfileButtonStyle))
                {
                    ActiveProfile = lProfile;
                    if (CanBuild) { this.StartCoroutine(CreateCharacter()); }
                }
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();

                GUILayout.Space(5);

                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();                
                EditorGUILayout.LabelField(lProfile.DisplayText.Trim(), ProfileLabelStyle);
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
                
                GUILayout.FlexibleSpace();        
            }
            finally
            {
                EditorGUILayout.EndVertical();
            }            
        }     


        #endregion Draw GUI (Basic)


        #region Draw GUI (Debug)

        private void DrawDebugSettings()
        {
            if (!mDebugEnabled) { return; }
            
            EditorHelper.DrawInspectorBlock("Debug", "Some debug settings", () =>
            {
                EditorGUILayout.LabelField("Reset Editor Prefs", EditorStyles.boldLabel, GUILayout.Height(16f));
                GUILayout.Space(5);

                DrawEditorPrefControls("ActiveProfile", PrefsKey.ActiveProfile);
                DrawEditorPrefControls("AnimatorName", PrefsKey.AnimatorName);
                DrawEditorPrefControls("AnimatorPath", PrefsKey.AnimatorPath);
            }, 
            ref mDebugFoldout);
        }

        private void DrawEditorPrefControls(string rName, string rPrefsKey)
        {
            EditorGUILayout.LabelField(rPrefsKey, GUILayout.Height(16f));

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Value = ", GUILayout.Height(16f), GUILayout.Width(60f));
            EditorGUILayout.LabelField(EditorPrefs.GetString(rPrefsKey, string.Empty), GUILayout.Height(16f), GUILayout.ExpandWidth(true));

            //GUILayout.FlexibleSpace();

            if (GUILayout.Button("Remove", GUILayout.Width(60f)))
            {
                EditorPrefs.DeleteKey(rPrefsKey);
            }
            GUILayout.EndHorizontal();

            EditorGUILayout.Separator();
        }


        #endregion Draw GUI (Debug)

    }
}
