// Uncomment this directive to display the checkbox for the ReadOnly property
//#define ALLOW_EDIT_DEFAULT_PROFILES

using System;
using System.Collections.Generic;
using System.Linq;
using com.ootii.Helpers;
using com.ootii.Actors.Attributes;
using com.ootii.Actors.Combat;
using com.ootii.Actors.Inventory;
using com.ootii.Base;
using com.ootii.Setup.Modules;
using UnityEditor;
using UnityEngine;

namespace com.ootii.Setup
{
    [CustomEditor(typeof(CharacterWizardProfile), true), CanEditMultipleObjects]
    public class CharacterWizardProfileEditor : BaseInspector<CharacterWizardProfile>
    {
        // Width for fields that look weird when expanded to the full width of the window
        private const float mFieldWidth = 300f;

        /// <summary>
        /// Is this inspector's view injected into an EditorWindow or another inspector?
        /// </summary>
        public bool IsInjectedView { get; set; }

        #region Protected Fields      

        // ===================================================
        // Serialized Properties
        protected SerializedProperty mReadOnly = null;
        protected SerializedProperty mPriority = null;
        protected SerializedProperty mCharacterType = null;        

        protected SerializedProperty mCreateNewAnimator = null;
        protected SerializedProperty mConfigureAnimator = null;
        protected SerializedProperty mAnimatorController = null;
        protected SerializedProperty mAnimatorName = null;
        protected SerializedProperty mAnimatorUsesProfileName = null;
        protected SerializedProperty mAnimatorUsesCharacterName = null;

        protected SerializedProperty mViewActivator = null;
        protected SerializedProperty mAddGameCore = null;
        protected SerializedProperty mUseCameraPrefab = null;        
        protected SerializedProperty mUseCameraAnchor = null;
        
        protected SerializedProperty mAddBasicAttributes = null;
        protected SerializedProperty mAddBasicInventory = null;
        protected SerializedProperty mAddRigidbody = null;
        protected SerializedProperty mAddCombatant = null;

        protected SerializedProperty mAddNavMeshAgent = null;

        protected SerializedProperty mUseReticle = null;
        protected SerializedProperty mUsePlayerHealthBar = null;
        protected SerializedProperty mUseNPCHealthBar = null;
        protected SerializedProperty mPlayerHealthBarPrefab = null;
        protected SerializedProperty mCombatantHealthBarPrefab = null;

        protected SerializedProperty mAddBasicMotions = null;
        protected SerializedProperty mAddInteraction = null;
        protected SerializedProperty mAddClimbMotions = null;

        protected SerializedProperty mPlayerLayer = null;
        protected SerializedProperty mNPCLayer = null;

        // ===================================================
        // Foldout states        
        protected bool mProfileSettingsFoldout = true;
        protected bool mRequiredPacksFoldout = true;
        protected bool mAnimatorFoldout = true;
        protected bool mPlayerFoldout = true;
        protected bool mNPCFoldout = true;
        protected bool mMotionsFoldout = true;
        protected bool mComponentsFoldout = true;        
        protected bool mUIFoldout = true;                       

        protected ModuleListEditor mNewModuleList;

        
        // ===================================================
        // Motion Pack list        
        // The packs selected as Required
        protected readonly Dictionary<int, bool> mRequiredMotionPacks = new Dictionary<int, bool>();
        
        #endregion Protected Fields              


        #region Initialization

        protected override void Initialize()
        {
            try
            {                                        
                // Determine which required Motion Packs should be checked
                mRequiredMotionPacks.Clear();
                for (int i = 0; i < MotionPackSetupHelper.PackNames.Count; i++)
                {
                    bool lPackEnabled = mTarget._RequiredMotionPacks.Contains(MotionPackSetupHelper.PackNames[i]);
                    mRequiredMotionPacks.Add(i, lPackEnabled);
                }

                InitializeProperties();

                if (!Application.isPlaying)
                {
                    mTarget.InitializeModules();
                }

                mNewModuleList = new ModuleListEditor(mTarget);                              
            }
            catch (NullReferenceException)
            {
                // Swallow the exception. 
                // A NullReferenceException usually gets thrown in InitiailzeProperties() if the asset is 
                // selected when Unity recompiles            
            }
        }

        /// <summary>
        /// Find and obtain serialized properties that are used repeatedly. 
        /// We won't worry about doing this for ones that are only used once
        /// in an EditorGUILayout.PropertyField() function call
        /// </summary>
        protected virtual void InitializeProperties()
        {                       
            mReadOnly = FindProperty(x => x.ReadOnly);
            mPriority = FindProperty(x => x.Priority);
                                   
            mCreateNewAnimator = FindProperty(x => x.CreateNewAnimator);
            mConfigureAnimator = FindProperty(x => x.ConfigureAnimator);
            mAnimatorController = FindProperty(x => x.AnimatorController);
            mAnimatorName = FindProperty(x => x.AnimatorName);
            mAnimatorUsesProfileName = FindProperty(x => x.AnimatorUsesProfileName);
            mAnimatorUsesCharacterName = FindProperty(x => x.AnimatorUsesCharacterName);

            mUseCameraPrefab = FindProperty(x => x.UseCameraPrefab);
            mUseCameraAnchor = FindProperty(x => x.UseCameraAnchor);
            mViewActivator = FindProperty(x => x.ViewActivator);
            mAddGameCore = FindProperty(x => x.AddGameCore);
                     
            mAddBasicAttributes = FindProperty(x => x.AddBasicAttributes);
            mAddBasicInventory = FindProperty(x => x.AddBasicInventory);
            mAddRigidbody = FindProperty(x => x.AddRigidbody);
            mAddCombatant = FindProperty(x => x.AddCombatant);

            mAddNavMeshAgent = FindProperty(x => x.AddNavMeshAgent);

            mUseReticle = FindProperty(x => x.UseReticle);
            mUsePlayerHealthBar = FindProperty(x => x.UsePlayerHUD);
            mUseNPCHealthBar = FindProperty(x => x.UseCombatantHUD);
            mPlayerHealthBarPrefab = FindProperty(x => x.PlayerHealthBarPrefab);
            mCombatantHealthBarPrefab = FindProperty(x => x.CombatantHealthBarPrefab);

            mAddBasicMotions = FindProperty(x => x.AddBasicMotions);
            mAddClimbMotions = FindProperty(x => x.AddClimbMotions);
            mAddInteraction = FindProperty(x => x.AddInteraction);

            mPlayerLayer = FindProperty(x => x.PlayerLayer);
            mNPCLayer = FindProperty(x => x.NPCLayer);
        }

        #endregion Initialization


        protected override void Draw()
        {            
            if (!IsInjectedView)
            {
                EditorHelper.DrawInspectorBlock("Profile Settings", "", DrawProfileSettings, ref mProfileSettingsFoldout);                
            }           

            EditorHelper.DrawInspectorBlock("Animator Controller", "Create a new animator controller or use an existing one.",
                DrawAnimatorSettings, ref mAnimatorFoldout, mReadOnly.boolValue);           

            EditorHelper.DrawInspectorBlock("Player Settings",
                "Options to use when the character is configured as a Player.",
                DrawPlayerSettings, ref mPlayerFoldout, mReadOnly.boolValue);

            EditorHelper.DrawInspectorBlock("NPC Settings",
               "Options to use when the character is configured as an AI-controlled NPC.",
               DrawNPCSettings, ref mNPCFoldout, mReadOnly.boolValue);

            EditorHelper.DrawInspectorBlock("Motions", "Add and configure common motions on the character.",
                DrawMotionConfiguration, ref mMotionsFoldout, mReadOnly.boolValue);

            EditorHelper.DrawInspectorBlock("Component Settings", "Select common components to add and configure on the character.",
                DrawComponentSettings, ref mComponentsFoldout, mReadOnly.boolValue);
                                
            EditorGUI.BeginDisabledGroup(mReadOnly.boolValue);     
            if (mNewModuleList.OnInspectorGUI())            
            {
                mIsDirty = true;                
            }         
            EditorGUI.EndDisabledGroup();

            if (mIsDirty) { mSaveChanges = true; }            
        }
       

        #region Display GUI Sections

          
        /// <summary>
        /// Profile settings -- should be edited directly in the asset's inspector, as this section
        /// won't display in the Wizard (the Wizard does have a field to rename the profile, however)
        /// </summary>
        protected virtual void DrawProfileSettings()
        {           
#if ALLOW_EDIT_DEFAULT_PROFILES
            EditorGUILayout.PropertyField(mReadOnly);
            EditorGUILayout.PropertyField(FindProperty(x => x._Renamed));
            EditorGUILayout.PropertyField(mPriority);
            EditorGUILayout.Separator();
#endif
            
            if (EditorHelper.DelayedTextField("Profile Name",
                           "Enter a new name for the profile. This will rename the profile's .asset file.",
                           mTarget.name, mTarget))
            {
                if (!string.IsNullOrEmpty(EditorHelper.FieldStringValue))
                {
                    mTarget.Rename(EditorHelper.FieldStringValue, true);
                }
            }

            EditorGUILayout.PropertyField(FindProperty(x => x.DisplayText));

            EditorGUILayout.Separator();
            EditorGUILayout.PropertyField(FindProperty(x => x.Description));
            EditorGUILayout.Separator();
            EditorGUILayout.PropertyField(FindProperty(x => x.Image));


            if (mPriority.enumValueIndex != (int) CharacterWizardProfile.PriorityStatus.User)
            {
                EditorGUILayout.Separator();

                string lHelpText = "Select any Motion Packs required by this profile.\n\n" +
                                   "If any of the required Motion Packs are not present, then the profile will be " +
                                   "hidden in Basic Mode";

                EditorHelper.DrawInspectorBlock("Required Motion Packs", lHelpText,
                    DrawRequiredMotionPacks, ref mRequiredPacksFoldout);
            }
        }

        /// <summary>
        /// Draws the list for selecting required Motion Packs
        /// </summary>
        protected virtual void DrawRequiredMotionPacks()
        {            
            if (MotionPackSetupHelper.PackNames != null)
            {
                for (int i = 0; i < MotionPackSetupHelper.PackNames.Count; i++)
                {
                    string lPackName = MotionPackSetupHelper.PackNames[i];
                    mRequiredMotionPacks[i] = EditorGUILayout.ToggleLeft(new GUIContent("  " + lPackName),
                        mRequiredMotionPacks[i], GUILayout.ExpandWidth(true));

                    if (mRequiredMotionPacks[i])
                    {
                        if (!mTarget._RequiredMotionPacks.Contains(lPackName))
                        {
                            mTarget._RequiredMotionPacks.Add(lPackName);                            
                            mIsDirty = true;
                        }
                    }
                    else
                    {
                        if (mTarget._RequiredMotionPacks.Contains(lPackName))
                        {
                            mTarget._RequiredMotionPacks.Remove(lPackName);                            
                            mIsDirty = true;
                        }
                    }
                }

                string[] lMissingPacks = mTarget._RequiredMotionPacks.Where(x => !MotionPackSetupHelper.PackNames.Contains(x)).ToArray();
                if (lMissingPacks.Any())
                {
                    EditorGUILayout.Separator();
                    EditorHelper.DrawInspectorDescription("This profile requires Motion Packs that are not currently installed.\n\n" +
                        "Pressing 'Clear All' will erase  ");

                    EditorGUI.BeginDisabledGroup(true);
                    foreach (string lPackName in lMissingPacks)
                    {
                        EditorGUILayout.ToggleLeft(new GUIContent("  " + lPackName), true, GUILayout.ExpandWidth(true));
                    }
                    EditorGUI.EndDisabledGroup();
                }                

                EditorGUILayout.Separator();

                EditorHelper.DrawQuickClearButton((() => {
                    // Update the editor list
                    for (int i = 0; i < mRequiredMotionPacks.Count; i++)
                    {
                        mRequiredMotionPacks[i] = false;
                    }

                    // Update the serialized list of string on the target
                    mTarget._RequiredMotionPacks.Clear();
                    mTarget.UpdateRequirements();
                    mIsDirty = true;
                }));                
                
            }
            else
            {
                EditorGUILayout.LabelField("There are no Motion Packs installed.");
            }
        }        

        /// <summary>
        /// Draw the settings to use when the character is configured as a Player
        /// </summary>
        protected virtual void DrawPlayerSettings()
        {            
            EditorGUILayout.HelpBox("When a player character is created, the camera, UI, and input options can also be set up.", MessageType.None);
            GUILayout.Space(5);
          
            mPlayerLayer.intValue = EditorGUILayout.LayerField(new GUIContent("Assign to Layer"), mPlayerLayer.intValue);

            EditorGUILayout.Separator();
            EditorGUILayout.LabelField("Camera Settings", EditorStyles.boldLabel, GUILayout.Height(16f));
            //EditorGUILayout.BeginHorizontal();

            //EditorGUILayout.BeginVertical();
            EditorGUILayout.PropertyField(mUseCameraPrefab);
            if (mUseCameraPrefab.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(FindProperty(x => x.CameraPrefab));
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.PropertyField(mUseCameraAnchor);
            if (mUseCameraAnchor.boolValue)
            {
                EditorGUI.indentLevel++;
                
                EditorGUILayout.PropertyField(FindProperty(x => x.AnchorTargetOffset), new GUIContent("Target Offset"), GUILayout.Width(mFieldWidth));
                
                EditorGUILayout.PropertyField(FindProperty(x => x.AnchorRotatesWithTarget), new GUIContent("Rotate with Target"));
                EditorGUILayout.PropertyField(FindProperty(x => x.AnchorMovementLerp), new GUIContent("Movement Lerp"), GUILayout.Width(mFieldWidth));
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Separator();
            EditorGUILayout.PropertyField(mAddGameCore);
            EditorGUILayout.Separator();

            EditorGUILayout.LabelField("Input Settings", EditorStyles.boldLabel, GUILayout.Height(16f));
            if (!InputSetupHelper.HasDefaultInputSettings())
            {
                EditorGUILayout.HelpBox(
                    "The default Unity Input Manager values for Motion Controller are missing." +
                    "\n\nMark the checkbox below to set them up.", MessageType.Info);
                GUILayout.Space(5);
                EditorGUILayout.PropertyField(FindProperty(x => x.UpdateInput),
                    new GUIContent("Setup Input Entries"));
            }

            mViewActivator.intValue = EditorGUILayout.Popup("View Activator", mViewActivator.intValue,
                InputSetupHelper.Activators, GUILayout.Width(mFieldWidth));

            EditorGUILayout.Separator();
            EditorGUILayout.LabelField("UI Settings", EditorStyles.boldLabel, GUILayout.Height(16f));
            EditorGUILayout.PropertyField(mUseReticle);
            if (mUseReticle.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(FindProperty(x => x.ReticlePrefab));
                EditorGUILayout.PropertyField(FindProperty(x => x.ReticleStartVisible),
                    new GUIContent("Visible at Start"));
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.PropertyField(mUsePlayerHealthBar, new GUIContent("Use HUD"));
            if (mUsePlayerHealthBar.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(mPlayerHealthBarPrefab, new GUIContent("HUD Prefab"));
                EditorGUILayout.PropertyField(FindProperty(x => x.PlayerHealthBarHideWhenFull), new GUIContent("Hide When Full"));
                EditorGUI.indentLevel--;
            }
            //EditorGUILayout.EndVertical();
            //GUILayout.FlexibleSpace();
            //EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// Draw the settings to use when the character is configured as an NPC
        /// </summary>
        protected virtual void DrawNPCSettings()
        {
            EditorGUILayout.HelpBox("When an NPC is created, a NavMesh component and health bar display can be added." + 
                "In the future, this may include some basic AI components or behaviour trees to get you started.", MessageType.None);
            GUILayout.Space(5);
            
            mNPCLayer.intValue = EditorGUILayout.LayerField(new GUIContent("Assign to Layer"), mNPCLayer.intValue);
            EditorGUILayout.PropertyField(FindProperty(x => x.UseTransformPosition));

            EditorGUILayout.PropertyField(mAddNavMeshAgent);
            if (mAddNavMeshAgent.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(FindProperty(x => x.NavMeshAngularSpeed),
                    new GUIContent("Angular Velocity"), GUILayout.Width(mFieldWidth));
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Separator();
            EditorGUILayout.LabelField("UI Settings", EditorStyles.boldLabel, GUILayout.Height(16f));

            bool lIsWorldSpacePrefab = false;
            EditorGUILayout.PropertyField(mUseNPCHealthBar, new GUIContent("Use HUD"));
            if (mUseNPCHealthBar.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(mCombatantHealthBarPrefab, new GUIContent("HUD Prefab"));
                if (mTarget.CombatantHealthBarPrefab != null)
                {
                    lIsWorldSpacePrefab = mTarget.CombatantHealthBarPrefab.DisplayInWorldSpace;
                }

                if (lIsWorldSpacePrefab)
                {
                    EditorGUILayout.PropertyField(FindProperty(x => x.WorldSpaceHUDPosition), new GUIContent("World Space Y Pos."), GUILayout.Width(mFieldWidth));
                }
                EditorGUI.indentLevel--;

                if (lIsWorldSpacePrefab)
                {
                    GUILayout.Space(5);
                    EditorHelper.DrawInspectorDescription(
                        "This HUD prefab contains a Canvas element and is thus intended for use in World Space." +
                        "\n\nWhen the character is built, an instance of the HUD prefab will be added to the character.", MessageType.Info);
                    GUILayout.Space(5);
                }
            }
        }
   
        /// <summary>
        /// Draw the Animator Controller settings UI
        /// </summary>
        protected virtual void DrawAnimatorSettings()
        {            
            EditorGUILayout.PropertyField(mCreateNewAnimator);
           
            if (mCreateNewAnimator.boolValue)
            {
                // Clear reference on the target
                mTarget.AnimatorController = null;               

                EditorGUI.BeginDisabledGroup(mAnimatorUsesProfileName.boolValue || mAnimatorUsesCharacterName.boolValue);
                EditorGUILayout.PropertyField(mAnimatorName);
                EditorGUI.EndDisabledGroup();
                EditorGUI.indentLevel++;

                EditorGUILayout.BeginHorizontal();
                
                EditorGUILayout.LabelField(
                    new GUIContent("Use Profile Name",
                        "The animator controller will be created with the same name as the profile."),
                    GUILayout.Width(120));

                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(mAnimatorUsesProfileName, new GUIContent(""),
                    GUILayout.Width(40));

                if (EditorGUI.EndChangeCheck())
                {
                    if (mAnimatorUsesProfileName.boolValue)
                    {
                        mAnimatorUsesCharacterName.boolValue = false;                        
                    }                   
                }
                
                GUILayout.Space(15f);

                EditorGUILayout.LabelField(
                    new GUIContent("Use Character Name",
                        "The animator controller will be created with the same name as the character."),
                    GUILayout.Width(140));

                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(mAnimatorUsesCharacterName, new GUIContent(""),
                    GUILayout.Width(40));

                if (EditorGUI.EndChangeCheck())
                {
                    if (mAnimatorUsesCharacterName.boolValue)
                    {
                        mAnimatorUsesProfileName.boolValue = false;                        
                    }
                }

                EditorGUILayout.EndHorizontal();
                EditorGUI.indentLevel--;                              
            }
            else
            {
                try
                {
                    if (mAnimatorController.objectReferenceValue != null)
                    {
                        string lConfigureSelected =
                            "This process will likely make significant changes to your Animator Controller. " +
                            "\n\nIf you are using an Animator Controller configured outside of this tool, " +
                            "it is best to first make a duplicate (CTRL-D), rename it, and use this instead.";

                        string lConfigureNotSelected =
                            "No changes to the assigned Animator Controller will be made. Unless your Animator " +
                            "Controller has been set up previously using the same profile settings, some animator " +
                            "states may be missing.";
                        EditorGUILayout.HelpBox(mConfigureAnimator.boolValue ? lConfigureSelected : lConfigureNotSelected,
                            MessageType.Warning);

                        GUILayout.Space(15);
                    }

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(mAnimatorController);
                    GUILayout.Space(5f);

                    EditorGUILayout.LabelField(
                        new GUIContent("Configure",
                            "Allows the Character Wizard to make changes to the animator controller."),
                        GUILayout.Width(80));
                    
                    EditorGUILayout.PropertyField(mConfigureAnimator, new GUIContent(""),
                        GUILayout.Width(40));
                }
                finally
                {
                    EditorGUILayout.EndHorizontal();
                }
            }
        }
              
        /// <summary>
        /// Draw add & configure component settings UI
        /// </summary>
        protected virtual void DrawComponentSettings()
        {
            // TODO: allow for substitutes that implement the same interfaces
            // Check if any Modules use these components
            bool lOverrideAttributes = false;
            bool lOverrideCombatant = false;
            bool lOverrideInventory = false;

            foreach (var lModule in mTarget.Modules)
            {
                if (lModule.UsesComponent(typeof(BasicAttributes))) { lOverrideAttributes = true; }
                if (lModule.UsesComponent(typeof(BasicInventory))) { lOverrideInventory = true; }

#if OOTII_SSMP || OOTII_AYMP || OOTII_SCMP
                if (lModule.UsesComponent(typeof(Combatant))) { lOverrideCombatant = true; }
#endif
            }

            if (lOverrideAttributes) { mAddBasicAttributes.boolValue = true; }
            if (lOverrideInventory) { mAddBasicInventory.boolValue = true; }
            if (lOverrideCombatant) { mAddCombatant.boolValue = true; }

            EditorGUI.BeginDisabledGroup(lOverrideAttributes);
            EditorGUILayout.PropertyField(mAddBasicAttributes);
            EditorGUI.EndDisabledGroup();
            if (mAddBasicAttributes.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(FindProperty(x => x.HealthValue), GUILayout.Width(mFieldWidth));
                EditorGUI.indentLevel--;
            }

            EditorGUI.BeginDisabledGroup(lOverrideInventory);
            EditorGUILayout.PropertyField(mAddBasicInventory);
            EditorGUI.EndDisabledGroup();
            if (mAddBasicInventory.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(FindProperty(x => x.CreateDefaultSlots));
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.PropertyField(mAddRigidbody);
            if (mAddRigidbody.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(FindProperty(x => x.RigidbodyMass), GUILayout.Width(mFieldWidth));
                EditorGUI.indentLevel--;
            }

#if OOTII_SSMP || OOTII_AYMP || OOTII_SCMP                      
            EditorGUI.BeginDisabledGroup(lOverrideCombatant);
            EditorGUILayout.PropertyField(mAddCombatant);
            EditorGUI.EndDisabledGroup();
            if (mAddCombatant.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(FindProperty(x => x.MaxTargetLockDistance), new GUIContent("Max Lock Distance"), GUILayout.Width(mFieldWidth));
                EditorGUI.indentLevel--;
            }
#endif                        
            EditorHelper.DrawQuickSetButtons(mTarget.SetComponentOptions);
        }
       
        /// <summary>
        /// Draw the motion configuration UI
        /// </summary>
        protected virtual void DrawMotionConfiguration()
        {            
            EditorHelper.DrawInspectorDescription("Adds the set of Basic motions that use the Motion Form parameter " +
                "to determine which animation to play.");
            
            EditorGUILayout.PropertyField(FindProperty(x => x.AddFullBodyReactions));
            EditorGUILayout.PropertyField(FindProperty(x => x.AddUtilityMotions));
            EditorGUILayout.PropertyField(mAddInteraction);
            if (mAddInteraction.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(FindProperty(x => x.InteractionLayers));
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Separator();

            EditorHelper.DrawInspectorDescription("Adds the Jump and Fall motions.");
            EditorGUILayout.PropertyField(FindProperty(x => x.AddJumpMotions));

            EditorGUILayout.Separator();
                            
            EditorHelper.DrawInspectorDescription("Adds the Climb 0.5m, Climb 1.0m, Climb 1.8m, Climb 2.5m, Climb Ladder, and Vault motions.\n\n"
                + "Climb Ladder and Vault will only be added to a Player, as they do not currently support NPC movement.");
            EditorGUILayout.PropertyField(mAddClimbMotions);

            if (mAddClimbMotions.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(FindProperty(x => x.ClimbLayers));                
                EditorGUILayout.PropertyField(FindProperty(x => x.ClimbLadderLayers));
                EditorGUILayout.PropertyField(FindProperty(x => x.VaultLayers));                
                EditorGUI.indentLevel--;
            }

            EditorHelper.DrawQuickSetButtons(mTarget.SetMotionOptions);                                            
        }
      
        #endregion Display GUI Sections
    }
}
