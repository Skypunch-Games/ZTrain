﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditorInternal;
using com.ootii.Actors;
using com.ootii.Actors.AnimationControllers;
using com.ootii.Base;
using com.ootii.Cameras;
using com.ootii.Helpers;
using com.ootii.Input;

[CanEditMultipleObjects]
[CustomEditor(typeof(MotionController))]
public class MotionControllerEditor : Editor
{
    // Helps us keep track of when the list needs to be saved. This
    // is important since some changes happen in scene.
    private bool mIsDirty;

    // The actual class we're stroing
    private MotionController mTarget;
    private SerializedObject mTargetSO;

    // List object for our layers
    private ReorderableList mLayerList;

    // List object for our layer motions
    private ReorderableList mMotionList;

    // List object for our motion tags
    private string mTagValue = "";
    private ReorderableList mMotionTagList;
    private List<string> mMotionTags = new List<string>();

    // Filtered list of motions to be used by mMotionList
    private List<MotionControllerMotion> mFilteredMotions = new List<MotionControllerMotion>();

    //private int mMotionIndex = 0;
    private List<Type> mMotionTypes = new List<Type>();
    private List<String> mMotionNames = new List<string>();

    // List object for our packs
    private ReorderableList mPackList;

    private List<Type> mPackTypes = new List<Type>();
    private List<string> mPackNames = new List<string>();
    //private List<string> mGroupNamesX = new List<string>();

    // Determines if we show the layers and motions
    //private bool mShowLayers = false;
    private bool mShowSettings = false;

    // Store the actor controller so we can look at it
    private ActorController mActorController = null;

    /// <summary>
    /// Quick test to determine if we are filtering motions
    /// </summary>
    private bool IsFiltering
    {
        get
        {
            if (mTarget == null) { return false; }
            if (mTarget.EditorFilterText.Length > 0) { return true; }
            if (mTarget.EditorFilterGroupIndex > 0) { return true; }

            return false;
        }
    }

    /// <summary>
    /// Called when the script object is loaded
    /// </summary>
    void OnEnable()
    {
        // Grab the serialized objects
        mTarget = (MotionController)target;
        mTargetSO = new SerializedObject(target);

        mActorController = mTarget._ActorController;
        if (mActorController == null) { mActorController = mTarget.gameObject.GetComponent<ActorController>(); }

        // Update the motion controller layers so they can update with the definitions.
        // TRT 3/29/2016: Added so we don't reset changes at run-time when the MC
        // is selected in the editor.
        if (!UnityEngine.Application.isPlaying)
        {
            for (int i = 0; i < mTarget.MotionLayers.Count; i++)
            {
                mTarget.MotionLayers[i].AnimatorLayerIndex = i;
                mTarget.MotionLayers[i].MotionController = mTarget;
                mTarget.MotionLayers[i].InstanciateMotions();
            }
        }

        // Dropdown values
        mMotionTypes.Clear();
        mMotionNames.Clear();

        mPackTypes.Clear();
        mPackNames.Clear();

        // CDL 06/28/2018 - this only scans the "ootii" assembly
        // Generate the list of motions to display
        //Assembly lAssembly = Assembly.GetAssembly(typeof(MotionController));
        //Type[] lMotionTypes = lAssembly.GetTypes().OrderBy(x => x.Name).ToArray<Type>();
        //for (int i = 0; i < lMotionTypes.Length; i++)
        //{
        //    Type lType = lMotionTypes[i];
        //    if (lType.IsAbstract) { continue; }
        //    if (typeof(MotionControllerMotion).IsAssignableFrom(lType))
        //    {
        //        mMotionTypes.Add(lType);
        //        mMotionNames.Add(GetFriendlyName(lType));
        //    }
        //}

        // CDL 07/04/2018 - scan all assemblies for Motions and Motion Pack Definitions  
        // Use the cached found types    
        List<Type> lFoundTypes = AssemblyHelper.FoundTypes;

        // First generate the list of motions in the project
        for (int i = 0; i < lFoundTypes.Count; i++)
        {
            Type lType = lFoundTypes[i];
            if (lType.IsAbstract) { continue; }            

            if (typeof(MotionControllerMotion).IsAssignableFrom(lType))
            {                
                mMotionTypes.Add(lType);
                mMotionNames.Add(GetFriendlyName(lType));               
            }
        }
        
        mPackTypes.Add(null);
        mPackNames.Add("All");

        // CDL 07/04/2018       
        // Next we'll register all of the motion packs.
        // First look for motion pack definitions
        for (int i = 0; i < lFoundTypes.Count; i++)
        {
            Type lType = lFoundTypes[i];
            if (lType.IsAbstract) { continue; }

            if (typeof(MotionPackDefinition).IsAssignableFrom(lType))
            {
                PropertyInfo[] lStaticMethods = lType.GetProperties(BindingFlags.Static | BindingFlags.Public);
                if (lStaticMethods != null)
                {
                    for (int j = 0; j < lStaticMethods.Length; j++)
                    {
                        if (lStaticMethods[j].Name == "PackName")
                        {                            
                            string lPackName = lStaticMethods[j].GetValue(null, null) as string;
                            if (mPackNames.IndexOf(lPackName) < 0)
                            {
                                mPackTypes.Add(lType);
                                mPackNames.Add(lPackName);
                            }
                        }
                    }
                }
            }
        }

        // CDL 07/04/2018               
        // Finally, we'll look for static methods named "RegisterPack" on motions; this was the old method of defining motion packs        
        // Generate the list of pack definitions and the list of motions
        for (int i = 0; i < lFoundTypes.Count; i++)
        {
            Type lType = lFoundTypes[i];
            if (lType.IsAbstract) { continue; }
            
            if (typeof(MotionControllerMotion).IsAssignableFrom(lType))
            {
                // Old method; pack setup on Idle motion (of the official ootii Motion Packs, only Swimming still uses this)
                MethodInfo[] lStaticMethods = lType.GetMethods(BindingFlags.Static | BindingFlags.Public);
                if (lStaticMethods != null)
                {
                    for (int j = 0; j < lStaticMethods.Length; j++)
                    {
                        if (lStaticMethods[j].Name == "RegisterPack")
                        {
                            string lPackName = lStaticMethods[j].Invoke(null, null) as string;
                            if (mPackNames.IndexOf(lPackName) < 0)
                            {
                                mPackTypes.Add(lType);
                                mPackNames.Add(lPackName);
                            }
                        }
                    }
                }
            }            
        }

        // CDL 06/28/2018 - using the cached list of found types instead
        //Type[] lPackTypes = lAssembly.GetTypes().OrderBy(x => x.Name).ToArray<Type>();
        //for (int i = 0; i < lPackTypes.Length; i++)
        //{
        //    Type lType = lPackTypes[i];
        //    if (lType.IsAbstract) { continue; }
        //    if (typeof(MotionPackDefinition).IsAssignableFrom(lType))
        //    {
        //        PropertyInfo[] lStaticMethods = lType.GetProperties(BindingFlags.Static | BindingFlags.Public);
        //        if (lStaticMethods != null)
        //        {
        //            for (int j = 0; j < lStaticMethods.Length; j++)
        //            {
        //                if (lStaticMethods[j].Name == "PackName")
        //                {
        //                    string lPackName = lStaticMethods[j].GetValue(null, null) as string;
        //                    if (mPackNames.IndexOf(lPackName) < 0)
        //                    {
        //                        mPackTypes.Add(lType);
        //                        mPackNames.Add(lPackName);
        //                    }
        //                }
        //            }
        //        }
        //    }
        //}

        // CDL 06/28/2018 - can move this above as well, but if this is outdated, then maybe it can be left out since this is a major/breaking update
        //// Generate the list of packs from the motions (old way)
        //for (int i = 0; i < lMotionTypes.Length; i++)
        //{
        //    Type lType = lMotionTypes[i];
        //    if (lType.IsAbstract) { continue; }
        //    if (typeof(MotionControllerMotion).IsAssignableFrom(lType))
        //    {
        //        MethodInfo[] lStaticMethods = lType.GetMethods(BindingFlags.Static | BindingFlags.Public);
        //        if (lStaticMethods != null)
        //        {
        //            for (int j = 0; j < lStaticMethods.Length; j++)
        //            {
        //                if (lStaticMethods[j].Name == "RegisterPack")
        //                {
        //                    string lPackName = lStaticMethods[j].Invoke(null, null) as string;
        //                    if (mPackNames.IndexOf(lPackName) < 0)
        //                    {
        //                        mPackTypes.Add(lType);
        //                        mPackNames.Add(lPackName);
        //                    }
        //                }
        //            }
        //        }
        //    }
        //}

        // Create the list of layers
        InstanciateLayerList();
        if (mTarget.EditorLayerIndex < mLayerList.count)
        {
            mLayerList.index = mTarget.EditorLayerIndex;
            OnLayerListItemSelect(mLayerList);
        }

        InstanciatePackList();
    }

    /// <summary>
    /// Called when the inspector needs to draw
    /// </summary>
    public override void OnInspectorGUI()
    {
        // Pulls variables from runtime so we have the latest values.
        mTargetSO.Update();

        GUILayout.Space(5);

        EditorHelper.DrawInspectorTitle("ootii Motion Controller");

        EditorHelper.DrawInspectorDescription("Used to manage actor movement and animations.", MessageType.None);

        GUILayout.Space(5);

        if (mActorController != null && mActorController.UseTransformPosition)
        {
            EditorGUILayout.BeginVertical(EditorHelper.RedBox);
            EditorHelper.DrawInspectorDescription("The Actor Controller is set to follow its transform and not move based on input or motions. Movement input values will be simulated.", MessageType.Info);
            EditorGUILayout.EndVertical();

            GUILayout.Space(5);
        }

        if (EditorHelper.BoolField("Is Enabled", "Determines if the MC will pocess animations. While disabled, the AC could still be managing movement.", mTarget.IsEnabled, mTarget))
        {
            mIsDirty = true;
            mTarget.IsEnabled = EditorHelper.FieldBoolValue;
        }

        if (EditorHelper.BoolField("Smooth Time", "Determines if average delta-time over several frames in order to smooth movement due to delta-time spikes.", mTarget.IsTimeSmoothingEnabled, mTarget))
        {
            mIsDirty = true;
            mTarget.IsTimeSmoothingEnabled = EditorHelper.FieldBoolValue;
        }

        GUILayout.Space(5f);

        if (EditorHelper.ObjectField<Animator>("Animator", "Defines the animator is that controlling the character we want to manage.", mTarget.Animator, mTarget))
        {
            mIsDirty = true;
            mTarget.Animator = EditorHelper.FieldObjectValue as Animator;
        }

        EditorGUILayout.BeginHorizontal();

        GameObject lNewInputSourceOwner = EditorHelper.InterfaceOwnerField<IInputSource>(new GUIContent("Input Source", "Input source we'll use to get key presses, mouse movement, etc. This GameObject should have a component implementing the IInputSource interface."), mTarget.InputSourceOwner, true);
        if (lNewInputSourceOwner != mTarget.InputSourceOwner)
        {
            mIsDirty = true;
            mTarget.InputSourceOwner = lNewInputSourceOwner;
        }

        GUILayout.Space(5);

        EditorGUILayout.LabelField(new GUIContent("Find", "Determines if we attempt to automatically find the input source at startup if one isn't set."), GUILayout.Width(30));

        bool lNewAutoFindInputSource = EditorGUILayout.Toggle(mTarget.AutoFindInputSource, GUILayout.Width(16));
        if (lNewAutoFindInputSource != mTarget.AutoFindInputSource)
        {
            mIsDirty = true;
            mTarget.AutoFindInputSource = lNewAutoFindInputSource;
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();

        Transform lNewCameraTransform = EditorGUILayout.ObjectField(new GUIContent("Camera Rig", "Camera rig (or camera) transform that some motions may use to help rotations."), mTarget.CameraTransform, typeof(Transform), true) as Transform;
        if (lNewCameraTransform != mTarget.CameraTransform)
        {
            mIsDirty = true;
            mTarget.CameraTransform = lNewCameraTransform;
        }

        GUILayout.Space(5);

        EditorGUILayout.LabelField(new GUIContent("Find", "Determines if we attempt to automatically find the camera at startup if one isn't set."), GUILayout.Width(30));

        bool lNewAutoFindCameraTransform = EditorGUILayout.Toggle(mTarget.AutoFindCameraTransform, GUILayout.Width(16));
        if (lNewAutoFindCameraTransform != mTarget.AutoFindCameraTransform)
        {
            mIsDirty = true;
            mTarget.AutoFindCameraTransform = lNewAutoFindCameraTransform;
        }

        EditorGUILayout.EndHorizontal();

        if (!InputManagerHelper.IsDefined("ActivateRotation"))
        {
            GUILayout.Space(5);

            EditorGUILayout.BeginVertical(EditorHelper.Box);

            EditorGUILayout.HelpBox("Missing Unity Input Manager values. Press the button below to set them up.", MessageType.Warning);

            if (GUILayout.Button("Setup Input Entries", EditorStyles.miniButton))
            {
                CreateInputManagerSettings();
            }

            EditorGUILayout.EndVertical();
        }

        EditorHelper.DrawLine();

        EditorGUILayout.BeginHorizontal();

        EditorGUILayout.BeginHorizontal();

        GUILayout.Label("", BasicIcon, GUILayout.Width(16), GUILayout.Height(16));

        if (GUILayout.Button("Basic", EditorStyles.miniButton, GUILayout.Width(60)))
        {
            mTarget.EditorTabIndex = 0;
            mIsDirty = true;
        }

        EditorGUILayout.EndHorizontal();

        GUILayout.Space(1f);

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Advanced", EditorStyles.miniButton, GUILayout.Width(65)))
        {
            mTarget.EditorTabIndex = 1;
            mIsDirty = true;
        }

        EditorGUILayout.EndHorizontal();

        GUILayout.FlexibleSpace();

        EditorGUILayout.BeginHorizontal();

        GUILayout.Label("", AdvancedIcon, GUILayout.Width(16), GUILayout.Height(16));

        if (GUILayout.Button("Packs", EditorStyles.miniButton, GUILayout.Width(60)))
        {
            mTarget.EditorTabIndex = 2;
            mIsDirty = true;
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndHorizontal();

        GUILayout.Space(5);

        if (mTarget.EditorTabIndex == 1)
        {
            bool lIsDirty = OnAdvancedInspector();
            if (lIsDirty) { mIsDirty = true; }
        }
        else if (mTarget.EditorTabIndex == 2)
        {
            bool lIsDirty = OnPackInspector();
            if (lIsDirty) { mIsDirty = true; }
        }
        else
        {
            bool lIsDirty = OnBasicInspector();
            if (lIsDirty) { mIsDirty = true; }
        }

        GUILayout.Space(5f);

        // If there is a change... update.
        if (mIsDirty)
        {
            // Flag the object as needing to be saved
            EditorUtility.SetDirty(mTarget);

#if UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_4 || UNITY_4_5 || UNITY_4_6 || UNITY_5_0 || UNITY_5_1 || UNITY_5_2
            EditorApplication.MarkSceneDirty();
#else
            if (!EditorApplication.isPlaying)
            {
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
            }
#endif

            // Pushes the values back to the runtime so it has the changes
            mTargetSO.ApplyModifiedProperties();

            // Update the motion controller layers so they can update with the definitions.
            for (int i = 0; i < mTarget.MotionLayers.Count; i++)
            {
                mTarget.MotionLayers[i].InstanciateMotions();
                mTarget.MotionLayers[i].MotionController = mTarget;
            }

            // Clear out the dirty flag
            mIsDirty = false;
        }
    }

    /// <summary>
    /// Draws the advanced verions of the GUI
    /// </summary>
    /// <returns></returns>
    private bool OnBasicInspector()
    {
        bool lIsDirty = false;

        EditorGUILayout.BeginVertical(Box);

        EditorHelper.DrawSmallTitle("Movement Style");

        EditorHelper.DrawInspectorDescription("Click to setup the motions, input, and camera behavior to mimic the secified style. This will add motions and create the camera rig and input source as needed.", MessageType.None);

        EditorGUILayout.BeginHorizontal();

        GUILayout.Space(5);

        if (GUILayout.Button(new GUIContent("", "Setup MMO Style."), MMOIcon, GUILayout.Width(32f), GUILayout.Height(32f)))
        {
            lIsDirty = true;
            EnableMMOStyle();
        }

        EditorGUILayout.LabelField("MMO Style", OptionText, GUILayout.MinWidth(50), GUILayout.ExpandWidth(true));

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();

        GUILayout.Space(5);

        if (GUILayout.Button(new GUIContent("", "Setup Adventure Style."), AdventureIcon, GUILayout.Width(32f), GUILayout.Height(32f)))
        {
            lIsDirty = true;
            EnableAdventureStyle();
        }

        EditorGUILayout.LabelField("Adventure Style", OptionText, GUILayout.MinWidth(50), GUILayout.ExpandWidth(true));

        EditorGUILayout.EndHorizontal();


        EditorGUILayout.BeginHorizontal();

        GUILayout.Space(5);

        if (GUILayout.Button(new GUIContent("", "Setup Shooter Style."), ShooterIcon, GUILayout.Width(32f), GUILayout.Height(32f)))
        {
            lIsDirty = true;
            EnableShooterStyle();
        }

        EditorGUILayout.LabelField("Shooter Style", OptionText, GUILayout.MinWidth(50), GUILayout.ExpandWidth(true));

        EditorGUILayout.EndHorizontal();

        GUILayout.Space(5);

        EditorGUILayout.EndVertical();

        GUILayout.Space(5);

        EditorGUILayout.BeginVertical(Box);

        EditorHelper.DrawSmallTitle("Options");

        EditorHelper.DrawInspectorDescription("Select the desired options for your character. This will add motions as needed.", MessageType.None);
        GUILayout.Space(5);

        // Jumping
        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(10);

        MotionControllerMotion lMotion = mTarget.GetMotion<Jump>();
        bool lIsMotionEnabled = (lMotion != null && lMotion.IsEnabled);

        bool lNewIsMotionEnabled = EditorGUILayout.Toggle(lIsMotionEnabled, OptionToggle, GUILayout.Width(16));
        if (lNewIsMotionEnabled != lIsMotionEnabled)
        {
            lIsDirty = true;
            EnableJumping(lNewIsMotionEnabled);
        }

        EditorGUILayout.LabelField("Jump and running jump", GUILayout.MinWidth(50), GUILayout.ExpandWidth(true));
        EditorGUILayout.EndHorizontal();
        GUILayout.Space(5);

        // Climbing
        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(10);

        lMotion = mTarget.GetMotion<Climb_1m>();
        lIsMotionEnabled = (lMotion != null && lMotion.IsEnabled);

        lNewIsMotionEnabled = EditorGUILayout.Toggle(lIsMotionEnabled, OptionToggle, GUILayout.Width(16));
        if (lNewIsMotionEnabled != lIsMotionEnabled)
        {
            lIsDirty = true;
            EnableClimbing(lNewIsMotionEnabled);
        }

        EditorGUILayout.LabelField("Climb obstacles", GUILayout.MinWidth(50), GUILayout.ExpandWidth(true));

        if (lNewIsMotionEnabled)
        {
            Climb_0_5m lClimb1 = mTarget.GetMotion<Climb_0_5m>();
            if (lClimb1 != null)
            {
                int lNewClimbableLayers = EditorHelper.LayerMaskField(new GUIContent("", "Layers that identies objects that can be climbed."), lClimb1.ClimbableLayers, GUILayout.Width(100));
                if (lNewClimbableLayers != lClimb1.ClimbableLayers)
                {
                    lIsDirty = true;
                    lClimb1.ClimbableLayers = lNewClimbableLayers;

                    Climb_1m lClimb2 = mTarget.GetMotion<Climb_1m>();
                    if (lClimb2 != null) { lClimb2.ClimbableLayers = lNewClimbableLayers; }

                    Climb_1_8m lClimb3 = mTarget.GetMotion<Climb_1_8m>();
                    if (lClimb3 != null) { lClimb3.ClimbableLayers = lNewClimbableLayers; }

                    Climb_2_5m lClimb4 = mTarget.GetMotion<Climb_2_5m>();
                    if (lClimb4 != null) { lClimb4.ClimbableLayers = lNewClimbableLayers; }

                    SerializeMotion(lClimb1);
                    SerializeMotion(lClimb2);
                    SerializeMotion(lClimb3);
                    SerializeMotion(lClimb4);
                }
            }
        }

        EditorGUILayout.EndHorizontal();
        GUILayout.Space(5);

        // Climbing ladders
        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(10);

        lMotion = mTarget.GetMotion<ClimbLadder>();
        lIsMotionEnabled = (lMotion != null && lMotion.IsEnabled);

        lNewIsMotionEnabled = EditorGUILayout.Toggle(lIsMotionEnabled, OptionToggle, GUILayout.Width(16));
        if (lNewIsMotionEnabled != lIsMotionEnabled)
        {
            lIsDirty = true;
            EnableLaddering(lNewIsMotionEnabled);
        }

        EditorGUILayout.LabelField("Climb ladders", GUILayout.MinWidth(50), GUILayout.ExpandWidth(true));

        if (lNewIsMotionEnabled)
        {
            ClimbLadder lClimb = mTarget.GetMotion<ClimbLadder>();
            if (lClimb != null)
            {
                int lNewClimbableLayers = EditorHelper.LayerMaskField(new GUIContent("", "Layers that identies objects that can be climbed."), lClimb.ClimbableLayers, GUILayout.Width(100));
                if (lNewClimbableLayers != lClimb.ClimbableLayers)
                {
                    lIsDirty = true;
                    lClimb.ClimbableLayers = lNewClimbableLayers;

                    SerializeMotion(lClimb);
                }
            }
        }

        EditorGUILayout.EndHorizontal();
        GUILayout.Space(5);

        // Scaling walls
        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(10);

        lMotion = mTarget.GetMotion<ClimbWall>();
        lIsMotionEnabled = (lMotion != null && lMotion.IsEnabled);

        lNewIsMotionEnabled = EditorGUILayout.Toggle(lIsMotionEnabled, OptionToggle, GUILayout.Width(16));
        if (lNewIsMotionEnabled != lIsMotionEnabled)
        {
            lIsDirty = true;
            EnableScaling(lNewIsMotionEnabled);
        }

        EditorGUILayout.LabelField("Scale walls", GUILayout.MinWidth(50), GUILayout.ExpandWidth(true));

        if (lNewIsMotionEnabled)
        {
            ClimbWall lClimb = mTarget.GetMotion<ClimbWall>();
            if (lClimb != null)
            {
                int lNewClimbableLayers = EditorHelper.LayerMaskField(new GUIContent("", "Layers that identies objects that can be climbed."), lClimb.ClimbableLayers, GUILayout.Width(100));
                if (lNewClimbableLayers != lClimb.ClimbableLayers)
                {
                    lIsDirty = true;
                    lClimb.ClimbableLayers = lNewClimbableLayers;

                    SerializeMotion(lClimb);
                }
            }
        }

        EditorGUILayout.EndHorizontal();
        GUILayout.Space(5);

        // Sneaking
        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(10);

        lMotion = mTarget.GetMotion<Sneak_v2>();
        lIsMotionEnabled = (lMotion != null && lMotion.IsEnabled);

        lNewIsMotionEnabled = EditorGUILayout.Toggle(lIsMotionEnabled, OptionToggle, GUILayout.Width(16));
        if (lNewIsMotionEnabled != lIsMotionEnabled)
        {
            lIsDirty = true;
            EnableSneaking(lNewIsMotionEnabled);
        }

        EditorGUILayout.LabelField("Sneak", GUILayout.MinWidth(50), GUILayout.ExpandWidth(true));
        EditorGUILayout.EndHorizontal();
        GUILayout.Space(5);

        EditorGUILayout.EndVertical();

        return lIsDirty;
    }

    /// <summary>
    /// Draws the advanced verions of the GUI
    /// </summary>
    /// <returns></returns>
    private bool OnAdvancedInspector()
    {
        bool lIsDirty = false;

        // Show the Layers
        EditorGUILayout.LabelField("Motion Layers", EditorStyles.boldLabel, GUILayout.Height(16f));

        GUILayout.BeginVertical(EditorHelper.GroupBox);
        EditorHelper.DrawInspectorDescription("Motions Layers sync with the animator's layers and contain motions that work on that layer.", MessageType.None);

        mLayerList.DoLayoutList();

        if (mLayerList.index >= 0 && mLayerList.index < mTarget.MotionLayers.Count)
        {
            GUILayout.Space(5f);
            GUILayout.BeginVertical(EditorHelper.Box);

            bool lLayerListIsDirty = DrawLayerDetailItem(mTarget.MotionLayers[mLayerList.index]);
            if (lLayerListIsDirty) { lIsDirty = true; }

            GUILayout.EndVertical();
        //}

        GUILayout.EndVertical();

        //if (mLayerList.index >= 0 && mLayerList.index < mTarget.MotionLayers.Count)
        //{
            MotionControllerLayer lLayer = mTarget.MotionLayers[mLayerList.index];

            GUILayout.Space(5f);

            EditorGUILayout.LabelField("Motions", EditorStyles.boldLabel, GUILayout.Height(16f));
            GUILayout.BeginVertical(EditorHelper.GroupBox);
            EditorHelper.DrawInspectorDescription("Motions determine how your actor will move, rotate, and animate.", MessageType.None);

            // Filter area
            GUILayout.BeginVertical();

            GUILayout.BeginHorizontal();

            string lNewEditorFilterText = EditorGUILayout.TextField(mTarget.EditorFilterText, GUI.skin.FindStyle("ToolbarSeachTextField"), GUILayout.ExpandWidth(true));
            if (lNewEditorFilterText != mTarget.EditorFilterText)
            {
                mTarget.EditorFilterText = lNewEditorFilterText;
                InstanciateMotionList(mLayerList);
            }

            if (GUILayout.Button("", GUI.skin.FindStyle("ToolbarSeachCancelButton")))
            {
                if (mTarget.EditorFilterText.Length > 0)
                {
                    mTarget.EditorFilterText = "";
                    InstanciateMotionList(mLayerList);
                }

                GUI.FocusControl(null);
            }

            //int lNewEditorFilterGroupIndex = EditorGUILayout.Popup(mTarget.EditorFilterGroupIndex, mPackNames.ToArray(), GUILayout.Width(100));
            //if (lNewEditorFilterGroupIndex != mTarget.EditorFilterGroupIndex)
            //{
            //    mTarget.EditorFilterGroupIndex = lNewEditorFilterGroupIndex;
            //    InstanciateMotionList(mLayerList);
            //}

            GUILayout.EndHorizontal();

            GUILayout.EndVertical();

            // Motion List
            bool lListIsDirty = DrawLayerMotionList(mTarget.MotionLayers[mLayerList.index]);
            if (lListIsDirty) { lIsDirty = true; }

            if (IsFiltering)
            {
                if (mMotionList != null && mMotionList.index >= 0 && mMotionList.index < mFilteredMotions.Count)
                {
                    GUILayout.Space(5f);
                    GUILayout.BeginVertical(EditorHelper.Box);

                    bool lMotionIsDirty = DrawMotionDetailItem(mMotionList.index, mFilteredMotions[mMotionList.index]);
                    if (lMotionIsDirty) { lIsDirty = true; }

                    GUILayout.EndVertical();
                }
            }
            else
            {
                if (mMotionList != null && mMotionList.index >= 0 && mMotionList.index < lLayer.Motions.Count)
                {
                    GUILayout.Space(5f);
                    GUILayout.BeginVertical(EditorHelper.Box);

                    bool lMotionIsDirty = DrawMotionDetailItem(mMotionList.index, lLayer.Motions[mMotionList.index]);
                    if (lMotionIsDirty) { lIsDirty = true; }

                    GUILayout.EndVertical();
                }
            }
        }

        GUILayout.EndVertical();

        //GUILayout.Space(5f);

        //// Customize the animator clearing
        //EditorGUILayout.LabelField("Animator Clear", EditorStyles.boldLabel, GUILayout.Height(16f));

        //GUILayout.BeginVertical(EditorHelper.GroupBox);
        //EditorHelper.DrawInspectorDescription("To help support multiplayer networking, these setting will modify the Motion Controller's behavior. Only use if needed.", MessageType.None);

        //GUILayout.BeginVertical(EditorHelper.Box);

        //if (EditorHelper.PopUpField("Clear Mode", "Determines when the motion phase parameter is cleared. Only change this if you're using a networking solution that can't handle fast changing animator parameters.", mTarget._AnimatorClearType, mAnimatorClearTypes, mTarget))
        //{
        //    lIsDirty = true;
        //    mTarget.AnimatorClearType = EditorHelper.FieldIntValue;
        //}

        //if (mTarget.AnimatorClearType == 1)
        //{
        //    GUILayout.Space(5f);
        //    EditorHelper.DrawInspectorDescription("This option is useful if your networking solution can't handle fast changing animator parameters. However, long delays may cause stuttering as animation states re-enter due to not clearing the motion phase in a timely manner.", MessageType.None);

        //    if (EditorHelper.FloatField("Clear Delay", "Time in seconds to delay before clearing the motion phase in the animator.", mTarget.AnimatorClearDelay, mTarget))
        //    {
        //        lIsDirty = true;
        //        mTarget.AnimatorClearDelay = EditorHelper.FieldFloatValue;
        //    }
        //}

        //GUILayout.EndVertical();

        //GUILayout.EndVertical();

        GUILayout.Space(5f);

        // Show the events
        GUILayout.BeginHorizontal();

        if (GUILayout.Button(new GUIContent("Events"), EditorStyles.boldLabel))
        {
            mTarget.EditorShowEvents = !mTarget.EditorShowEvents;
        }

        GUILayout.FlexibleSpace();

        if (GUILayout.Button(new GUIContent(mTarget.EditorShowEvents ? "-" : "+"), EditorStyles.boldLabel))
        {
            mTarget.EditorShowEvents = !mTarget.EditorShowEvents;
        }

        GUILayout.EndHorizontal();

        GUILayout.BeginVertical(EditorHelper.GroupBox);
        EditorHelper.DrawInspectorDescription("Assign functions to be called when specific events take place.", MessageType.None);

        if (mTarget.EditorShowEvents)
        {
            GUILayout.BeginVertical(EditorHelper.Box);

            SerializedProperty lActivatedEvent = mTargetSO.FindProperty("AnimatorChangedEvent");
            if (lActivatedEvent != null)
            {
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(lActivatedEvent);
                if (EditorGUI.EndChangeCheck())
                {
                    mIsDirty = true;
                }
            }

            lActivatedEvent = mTargetSO.FindProperty("MotionTestActivateEvent");
            if (lActivatedEvent != null)
            {
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(lActivatedEvent);
                if (EditorGUI.EndChangeCheck())
                {
                    mIsDirty = true;
                }
            }

            lActivatedEvent = mTargetSO.FindProperty("MotionActivatedEvent");
            if (lActivatedEvent != null)
            {
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(lActivatedEvent);
                if (EditorGUI.EndChangeCheck())
                {
                    mIsDirty = true;
                }
            }

            SerializedProperty lDeactivatedEvent = mTargetSO.FindProperty("MotionDeactivatedEvent");
            if (lDeactivatedEvent != null)
            {
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(lDeactivatedEvent);
                if (EditorGUI.EndChangeCheck())
                {
                    mIsDirty = true;
                }
            }

            SerializedProperty lCustomEvent = mTargetSO.FindProperty("ActionTriggeredEvent");
            if (lCustomEvent != null)
            {
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(lCustomEvent);
                if (EditorGUI.EndChangeCheck())
                {
                    mIsDirty = true;
                }
            }

            GUILayout.EndVertical();
        }

        GUILayout.EndVertical();

        GUILayout.Space(5);

        // Show the debug
        GUILayout.BeginHorizontal();

        if (GUILayout.Button(new GUIContent("Debug"), EditorStyles.boldLabel))
        {
            mTarget.EditorShowDebug = !mTarget.EditorShowDebug;
        }

        GUILayout.FlexibleSpace();

        if (GUILayout.Button(new GUIContent(mTarget.EditorShowDebug ? "-" : "+"), EditorStyles.boldLabel))
        {
            mTarget.EditorShowDebug = !mTarget.EditorShowDebug;
        }

        GUILayout.EndHorizontal();
        GUILayout.BeginVertical(EditorHelper.GroupBox);
        EditorHelper.DrawInspectorDescription("Determines if we'll render debug information. We can do this motion-by-motion or for all.", MessageType.None);

        if (mTarget.EditorShowDebug)
        {
            GUILayout.BeginVertical(EditorHelper.Box);

            if (EditorHelper.BoolField("Show Debug Info", "Determines if the MC will render debug information at all.", mTarget.ShowDebug, mTarget))
            {
                lIsDirty = true;
                mTarget.ShowDebug = EditorHelper.FieldBoolValue;
            }

            if (mTarget.ShowDebug)
            {
                if (EditorHelper.BoolField("   For All Motions", "Determines if the MC will force all motions to show debug info.", mTarget.ShowDebugForAllMotions, mTarget))
                {
                    lIsDirty = true;
                    mTarget.ShowDebugForAllMotions = EditorHelper.FieldBoolValue;
                }
            }

            GUILayout.EndVertical();
        }

        GUILayout.EndVertical();

        return lIsDirty;
    }

    /// <summary>
    /// Draws the advanced verions of the GUI
    /// </summary>
    /// <returns></returns>
    private bool OnPackInspector()
    {
        bool lIsDirty = false;

        // Show the Layers
        GUILayout.BeginVertical(EditorHelper.GroupBox);
        mPackList.DoLayoutList();

        if (mPackList.index >= 1)
        {
            GUILayout.BeginVertical(EditorHelper.Box);

            bool lListIsDirty = DrawPackDetailItem(mPackList.index);
            if (lListIsDirty) { lIsDirty = true; }

            GUILayout.EndVertical();
        }

        GUILayout.EndVertical();

        return lIsDirty;
    }

    /// <summary>
    /// Allows us to render objects in the scene itself. This
    /// is only called when the scene window has focus
    /// </summary>
    private void OnSceneGUI()
    {
        if (Event.current.type.Equals(EventType.Repaint))
        {
            // Allow the motors to render to the scene or edit scene objects
            if (mLayerList != null && mLayerList.index >= 0 && mLayerList.index < mTarget.MotionLayers.Count)
            {
                MotionControllerLayer lLayer = mTarget.MotionLayers[mLayerList.index];

                if (IsFiltering)
                {
                    if (mMotionList != null && mMotionList.index >= 0 && mMotionList.index < mFilteredMotions.Count)
                    {
                        MotionControllerMotion lMotion = mFilteredMotions[mMotionList.index];
                        if (lMotion != null) { lMotion.OnSceneGUI(); }
                    }
                }
                else
                {
                    if (mMotionList != null && mMotionList.index >= 0 && mMotionList.index < lLayer.Motions.Count)
                    {
                        MotionControllerMotion lMotion = lLayer.Motions[mMotionList.index];
                        if (lMotion != null) { lMotion.OnSceneGUI(); }
                    }
                }
            }
        }
    }

    #region Layer List

    /// <summary>
    /// Create the reorderable list
    /// </summary>
    private void InstanciateLayerList()
    {
        SerializedProperty lMotionLayers = mTargetSO.FindProperty("MotionLayers");
        mLayerList = new ReorderableList(mTargetSO, lMotionLayers, true, true, true, true);
        mLayerList.drawHeaderCallback = DrawLayerListHeader;
        mLayerList.drawFooterCallback = DrawLayerListFooter;
        mLayerList.drawElementCallback = DrawLayerListItem;
        mLayerList.onAddCallback = OnLayerListItemAdd;
        mLayerList.onRemoveCallback = OnLayerListItemRemove;
        mLayerList.onSelectCallback = OnLayerListItemSelect;
        mLayerList.onReorderCallback = OnLayerListReorder;
        mLayerList.footerHeight = 17f;
    }

    /// <summary>
    /// Header for the list
    /// </summary>
    /// <param name="rRect"></param>
    private void DrawLayerListHeader(Rect rRect)
    {
        EditorGUI.LabelField(rRect, "Motion Layers");
    }

    /// <summary>
    /// Allows us to draw each item in the list
    /// </summary>
    /// <param name="rRect"></param>
    /// <param name="rIndex"></param>
    /// <param name="rIsActive"></param>
    /// <param name="rIsFocused"></param>
    private void DrawLayerListItem(Rect rRect, int rIndex, bool rIsActive, bool rIsFocused)
    {
        if (rIndex < mTarget.MotionLayers.Count)
        {
            Rect lNameRect = new Rect(rRect.x, rRect.y + 1, rRect.width, EditorGUIUtility.singleLineHeight);
            string lNewLayerName = EditorGUI.TextField(lNameRect, mTarget.MotionLayers[rIndex].Name);
            if (lNewLayerName != mTarget.MotionLayers[rIndex].Name)
            {
                mIsDirty = true;
                mTarget.MotionLayers[rIndex].Name = lNewLayerName;
            }
        }
    }

    /// <summary>
    /// Footer for the list
    /// </summary>
    /// <param name="rRect"></param>
    private void DrawLayerListFooter(Rect rRect)
    {
        Rect lAddRect = new Rect(rRect.x + rRect.width - 28 - 28, rRect.y + 1, 28, 15);
        if (GUI.Button(lAddRect, new GUIContent("+", "Add Layer."), EditorStyles.miniButtonLeft)) { OnLayerListItemAdd(mLayerList); }

        Rect lDeleteRect = new Rect(lAddRect.x + lAddRect.width, lAddRect.y, 28, 15);
        if (GUI.Button(lDeleteRect, new GUIContent("-", "Delete Layer."), EditorStyles.miniButtonRight)) { OnLayerListItemRemove(mLayerList); };
    }

    /// <summary>
    /// Allows us to add to a list
    /// </summary>
    /// <param name="rList"></param>
    private void OnLayerListItemAdd(ReorderableList rList)
    {
        MotionControllerLayer lLayer = Activator.CreateInstance<MotionControllerLayer>();
        lLayer.MotionController = mTarget;
        lLayer.AnimatorLayerIndex = mTarget.MotionLayers.Count;
        mTarget.MotionLayers.Add(lLayer);

        mLayerList.index = mTarget.MotionLayers.Count - 1;
        OnLayerListItemSelect(rList);

        mIsDirty = true;
    }

    /// <summary>
    /// Allows us process when a list is selected
    /// </summary>
    /// <param name="rList"></param>
    private void OnLayerListItemSelect(ReorderableList rList)
    {
        mTarget.EditorLayerIndex = rList.index;
        if (mTarget.EditorLayerIndex == -1) { return; }

        InstanciateMotionList(rList);
    }

    /// <summary>
    /// Allows us to stop before removing the item
    /// </summary>
    /// <param name="rList"></param>
    private void OnLayerListItemRemove(ReorderableList rList)
    {
        if (EditorUtility.DisplayDialog("Warning!", "Are you sure you want to delete the item?", "Yes", "No"))
        {
            int rIndex = rList.index;

            rList.index--;

            mTarget.MotionLayers.RemoveAt(rIndex);
            OnLayerListItemSelect(rList);

            mIsDirty = true;
        }
    }

    /// <summary>
    /// Allows us to process after the motions are reordered
    /// </summary>
    /// <param name="rList"></param>
    private void OnLayerListReorder(ReorderableList rList)
    {
        mIsDirty = true;
    }

    /// <summary>
    /// Renders the currently selected step
    /// </summary>
    /// <param name="rStep"></param>
    private bool DrawLayerDetailItem(MotionControllerLayer rLayer)
    {
        if (rLayer == null) { return false; }

        bool lIsDirty = false;

        EditorHelper.DrawSmallTitle(rLayer.Name.Length > 0 ? rLayer.Name : "Motion Layer");

        if (EditorHelper.TextField("Name", "Friendly name of the layer.", rLayer.Name, mTarget))
        {
            lIsDirty = true;
            rLayer.Name = EditorHelper.FieldStringValue;
        }

        if (EditorHelper.BoolField("Ignore Motion Override", "Determines if this layer will ignore 'Override Layer' that is set on motions in previous layers.", rLayer.IgnoreMotionOverride, mTarget))
        {
            lIsDirty = true;
            rLayer.IgnoreMotionOverride = EditorHelper.FieldBoolValue;
        }

        return lIsDirty;
    }

    /// <summary>
    /// Renders the currently selected step
    /// </summary>
    /// <param name="rStep"></param>
    private bool DrawLayerMotionList(MotionControllerLayer rLayer)
    {
        if (rLayer == null) { return false; }

        bool lIsDirty = false;

        if (mMotionList != null)
        {
            mMotionList.DoLayoutList();
        }

        return lIsDirty;
    }

    #endregion

    #region Motion List

    /// <summary>
    /// Instanciates the motion list from the specified layer list
    /// </summary>
    /// <param name="rList"></param>
    private void InstanciateMotionList(ReorderableList rLayerList)
    {
        string[] lSearchItems = mTarget.EditorFilterText.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

        // Parse out the search values
        List<string> lSearchText = new List<string>();
        List<string> lSearchTags = new List<string>();
        for (int i = 0; i < lSearchItems.Length; i++)
        {
            string lText = lSearchItems[i];

            if (lText.Length > 4 && lText.IndexOf("tag:", StringComparison.OrdinalIgnoreCase) == 0)
            {
                lText = lText.Substring(4);
                if (lText.Length > 0 && !lSearchTags.Contains(lText, StringComparer.OrdinalIgnoreCase)) { lSearchTags.Add(lText); }
            }
            else
            {
                if (lText.Length > 0 && !lSearchText.Contains(lText, StringComparer.OrdinalIgnoreCase)) { lSearchText.Add(lText); }
            }
        }

        if (IsFiltering)
        {
            //string lFilterText = mTarget.EditorFilterText.ToLower();
            //string lPackText = mPackNames[mTarget.EditorFilterGroupIndex].ToLower();

            mFilteredMotions.Clear();
            for (int i = 0; i < mTarget.MotionLayers[rLayerList.index].Motions.Count; i++)
            {
                MotionControllerMotion lMotion = mTarget.MotionLayers[rLayerList.index].Motions[i];

                if (IsMotionFiltered(lMotion, lSearchText, lSearchTags)) { continue; }

                //if (lFilterText.Length > 0 && lMotion._Name.ToLower().IndexOf(lFilterText) < 0)
                //{
                //    if (lMotion.GetType().Name.ToLower().IndexOf(lFilterText) < 0)
                //    {
                //        continue;
                //    }
                //}

                //if (mTarget.EditorFilterGroupIndex > 0 && lMotion._Pack.ToLower() != lPackText) { continue; }

                mFilteredMotions.Add(lMotion);
            }

            mMotionList = new ReorderableList(mFilteredMotions, typeof(MotionControllerMotion), false, true, true, true);
        }
        else
        {
            mMotionList = new ReorderableList(mTarget.MotionLayers[rLayerList.index].Motions, typeof(MotionControllerMotion), true, true, true, true);
        }

        mMotionList.drawHeaderCallback = DrawMotionListHeader;
        mMotionList.drawFooterCallback = DrawMotionListFooter;
        mMotionList.drawElementCallback = DrawMotionListItem;
        mMotionList.onSelectCallback = OnMotionListItemSelect;
        mMotionList.onAddCallback = OnMotionListItemAdd;
        mMotionList.onRemoveCallback = OnMotionListItemRemove;
        mMotionList.onReorderCallback = OnMotionListReorder;
        mMotionList.footerHeight = 17;

        if (mTarget.EditorMotionIndex < mMotionList.count)
        {
            mMotionList.index = mTarget.EditorMotionIndex;
            OnMotionListItemSelect(mMotionList);
        }
    }

    /// <summary>
    /// Header for the list
    /// </summary>
    /// <param name="rRect"></param>
    private void DrawMotionListHeader(Rect rRect)
    {
        EditorGUI.LabelField(rRect, "Motions");

        if (GUI.Button(rRect, "", EditorStyles.label))
        {
            mMotionList.index = -1;
            OnMotionListItemSelect(mMotionList);
        }
    }

    /// <summary>
    /// Allows us to draw each item in the list
    /// </summary>
    /// <param name="rRect"></param>
    /// <param name="rIndex"></param>
    /// <param name="rIsActive"></param>
    /// <param name="rIsFocused"></param>
    private void DrawMotionListItem(Rect rRect, int rIndex, bool rIsActive, bool rIsFocused)
    {
        MotionControllerMotion lMotion = null;

        if (IsFiltering)
        {
            if (rIndex < mFilteredMotions.Count)
            {
                lMotion = mFilteredMotions[rIndex];
            }
        }
        else
        {
            if (rIndex < mTarget.MotionLayers[mLayerList.index].Motions.Count)
            {
                lMotion = mTarget.MotionLayers[mLayerList.index].Motions[rIndex];
            }
        }

        if (lMotion != null)
        {
            bool lIsDirty = false;

            rRect.y += 2;

            float lHSpace = 5f;
            float lFlexVSpace = rRect.width - lHSpace - lHSpace - 40f - lHSpace - 16f;

            //bool lIsEnabled = (lMotion != null ? lMotion.IsEnabled : false);
            string lType = (lMotion != null ? GetFriendlyName(lMotion.GetType()) : "null");
            //string lName = (lMotion != null ? lMotion.Name : "null");
            //string lPriority = (lMotion != null ? lMotion.Priority.ToString() : "");

            EditorGUILayout.BeginHorizontal();

            Rect lTypeRect = new Rect(rRect.x, rRect.y, lFlexVSpace / 2f, EditorGUIUtility.singleLineHeight);
            EditorGUI.LabelField(lTypeRect, lType);

            Rect lNameRect = new Rect(lTypeRect.x + lTypeRect.width + lHSpace, lTypeRect.y, lFlexVSpace / 2f, EditorGUIUtility.singleLineHeight);
            string lMotionName = EditorGUI.TextField(lNameRect, lMotion.Name);
            if (lMotionName != lMotion.Name)
            {
                lIsDirty = true;
                lMotion.Name = lMotionName;
            }

            Rect lPriorityRect = new Rect(lNameRect.x + lNameRect.width + lHSpace, lNameRect.y, 40f, EditorGUIUtility.singleLineHeight);
            float lNewMotionPriority = EditorGUI.FloatField(lPriorityRect, lMotion.Priority);
            if (lNewMotionPriority != lMotion.Priority)
            {
                lIsDirty = true;
                lMotion.Priority = lNewMotionPriority;
            }

            Rect lIsEnabledRect = new Rect(lPriorityRect.x + lPriorityRect.width + lHSpace, lPriorityRect.y, 16f, 16f);
            bool lNewIsEnabled = EditorGUI.Toggle(lIsEnabledRect, lMotion.IsEnabled);
            if (lNewIsEnabled != lMotion.IsEnabled)
            {
                lIsDirty = true;
                lMotion.IsEnabled = lNewIsEnabled;
            }

            EditorGUILayout.EndHorizontal();

            // Update the motion if there's a change
            if (lIsDirty)
            {
                mIsDirty = true;
                SerializeMotion(lMotion);
            }
        }
    }

    /// <summary>
    /// Footer for the list
    /// </summary>
    /// <param name="rRect"></param>
    private void DrawMotionListFooter(Rect rRect)
    {
        Rect lMotionRect = new Rect(rRect.x, rRect.y + 1, rRect.width - 4 - 28 - 28, 16);
        //mMotionIndex = EditorGUI.Popup(lMotionRect, mMotionIndex, mMotionNames.ToArray());

        Rect lAddRect = new Rect(lMotionRect.x + lMotionRect.width + 4, lMotionRect.y, 28, 15);
        if (GUI.Button(lAddRect, new GUIContent("+", "Add Motion."), EditorStyles.miniButtonLeft)) { OnMotionListItemAdd(mMotionList); }

        Rect lDeleteRect = new Rect(lAddRect.x + lAddRect.width, lAddRect.y, 28, 15);
        if (GUI.Button(lDeleteRect, new GUIContent("-", "Delete Motion."), EditorStyles.miniButtonRight)) { OnMotionListItemRemove(mMotionList); };
    }

    /// <summary>
    /// Allows us to add to a list
    /// </summary>
    /// <param name="rList"></param>
    private void OnMotionListItemAdd(ReorderableList rList)
    {
        MotionSelectWindow lWindow = EditorWindow.GetWindow(typeof(MotionSelectWindow), false, "Select Types", true) as MotionSelectWindow;
        lWindow.minSize = new Vector2(350f, 200f);
        lWindow.OnSelectedEvent = OnMotionsSelected;
        //lWindow.Awake();

        //if (mMotionIndex >= mMotionTypes.Count) { return; }

        //MotionControllerMotion lMotion = Activator.CreateInstance(mMotionTypes[mMotionIndex]) as MotionControllerMotion;
        //lMotion.MotionController = mTarget;
        //lMotion.MotionLayer = mTarget.MotionLayers[mLayerList.index];
        //mTarget.MotionLayers[mLayerList.index].Motions.Add(lMotion);

        //lMotion.Reset();

        //mTarget.MotionLayers[mLayerList.index].MotionDefinitions.Add(lMotion.SerializeMotion());

        //if (IsFiltering)
        //{
        //    InstanciateMotionList(mLayerList);
        //    mMotionList.index = mFilteredMotions.Count - 1;
        //}
        //else
        //{
        //    mMotionList.index = mTarget.MotionLayers[mLayerList.index].Motions.Count - 1;
        //}

        //OnMotionListItemSelect(rList);
        //mIsDirty = true;
    }

    /// <summary>
    /// Store the currently selection motion
    /// </summary>
    /// <param name="rList"></param>
    private void OnMotionListItemSelect(ReorderableList rList)
    {
        mTarget.EditorMotionIndex = rList.index;

        InstantiateMotionTagList();
    }

    /// <summary>
    /// Allows us to stop before removing the item
    /// </summary>
    /// <param name="rList"></param>
    private void OnMotionListItemRemove(ReorderableList rList)
    {
        if (EditorUtility.DisplayDialog("Motion Controller", "Are you sure you want to delete the item?", "Yes", "No"))
        {
            int lIndex = rList.index;

            if (IsFiltering)
            {
                lIndex = mTarget.MotionLayers[mLayerList.index].Motions.IndexOf(mFilteredMotions[lIndex]);
                if (lIndex < 0) { return; }
            }

            rList.index--;

            mTarget.MotionLayers[mLayerList.index].Motions[lIndex].Clear();
            mTarget.MotionLayers[mLayerList.index].Motions.RemoveAt(lIndex);
            mTarget.MotionLayers[mLayerList.index].MotionDefinitions.RemoveAt(lIndex);

            if (IsFiltering)
            {
                InstanciateMotionList(mLayerList);
            }

            OnMotionListItemSelect(rList);

            mIsDirty = true;
        }
    }

    /// <summary>
    /// Allows us to process after the motions are reordered
    /// </summary>
    /// <param name="rList"></param>
    private void OnMotionListReorder(ReorderableList rList)
    {
        if (IsFiltering) { return; }

        // We need to update the motion defintions
        MotionControllerLayer lLayer = mTarget.MotionLayers[mLayerList.index];
        lLayer.MotionDefinitions.Clear();

        for (int i = 0; i < lLayer.Motions.Count; i++)
        {
            lLayer.MotionDefinitions.Add(lLayer.Motions[i].SerializeMotion());
        }

        mIsDirty = true;
    }

    /// <summary>
    /// Renders the currently selected motion details
    /// </summary>
    /// <param name="rStep"></param>
    private bool DrawMotionDetailItem(int rMotionIndex, MotionControllerMotion rMotion)
    {
        // Grab the motion
        if (rMotion == null) { return false; }

        bool lIsDirty = false;
        Type lMotionType = rMotion.GetType();
        string lMotionTypeName = GetFriendlyName(lMotionType);

        EditorHelper.DrawSmallTitle(rMotion.Name.Length > 0 ? rMotion.Name : lMotionTypeName);

        object[] lMotionAttributes = lMotionType.GetCustomAttributes(typeof(MotionDescriptionAttribute), true);
        foreach (MotionDescriptionAttribute lAttribute in lMotionAttributes)
        {
            EditorHelper.DrawInspectorDescription(lAttribute.Value, MessageType.None);
        }

        GUILayout.Space(2f);

        EditorGUILayout.BeginVertical(EditorHelper.Box);

        EditorGUILayout.BeginHorizontal();

        GUILayout.Space(4f);

        if (GUILayout.Button(new GUIContent("", "Details"), EditorHelper.OrangeGearButton, GUILayout.Width(16), GUILayout.Height(16)))
        {
            mTarget.EditorShowSettings = !mTarget.EditorShowSettings;
        }

        if (GUILayout.Button(new GUIContent((mTarget.EditorShowSettings ? "Hide" : "Show") + " Details", "Click to show/hide low level details of the motion."), GUI.skin.label, GUILayout.MinWidth(50f)))
        {
            mTarget.EditorShowSettings = !mTarget.EditorShowSettings;
        }

        EditorGUILayout.EndHorizontal();

        if (mTarget.EditorShowSettings)
        {
            GUILayout.Space(5f);

            EditorGUILayout.LabelField(new GUIContent("Namespace", "Specifies the container the motion belongs to."), new GUIContent(rMotion.GetType().Namespace, rMotion.GetType().Namespace));

            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField(new GUIContent("Type", "Identifies the type of motion."), new GUIContent(lMotionType.Name), GUILayout.MinWidth(30));

            if (GUILayout.Button(new GUIContent("", "Animator Settings"), EditorHelper.BlueGearButton, GUILayout.Width(16), GUILayout.Height(16)))
            {
                mShowSettings = !mShowSettings;
            }

            GUILayout.Space(2);

            EditorGUILayout.EndHorizontal();

            // Animator API
            if (mShowSettings)
            {
                EditorGUILayout.BeginVertical(GUI.skin.box);

                GUILayout.Space(2);

                // Key
                string lMotionKey = EditorGUILayout.TextField(new GUIContent("Key", "Unique key to associate with the motion type"), rMotion.Key);
                if (lMotionKey != rMotion.Key)
                {
                    lIsDirty = true;
                    rMotion._Key = lMotionKey;
                }

                // Animator sub-state machine
                rMotion._EditorAnimatorController = EditorGUILayout.ObjectField(new GUIContent("Controller", "Animator controller we are editing."), rMotion._EditorAnimatorController, typeof(UnityEditor.Animations.AnimatorController), true, null) as UnityEditor.Animations.AnimatorController;
                if (rMotion._EditorAnimatorController == null)
                {
                    Animator lAnimator = mTarget.Animator;
                    if (lAnimator == null) { lAnimator = mTarget.gameObject.GetComponent<Animator>(); }
                    if (lAnimator == null) { lAnimator = mTarget.gameObject.GetComponentInChildren<Animator>(); }
                    if (lAnimator != null) { rMotion._EditorAnimatorController = lAnimator.runtimeAnimatorController as UnityEditor.Animations.AnimatorController; }
                }

                rMotion._EditorAnimatorSMName = EditorGUILayout.TextField(new GUIContent("SM Name", "State machine name."), rMotion._EditorAnimatorSMName);

                rMotion._EditorAttachBehaviour = EditorGUILayout.Toggle(new GUIContent("Attach Behaviour", "Determines if we attach an animator state behaviour to the state machine."), rMotion._EditorAttachBehaviour);

                rMotion.OnSettingsGUI();

                GUILayout.Space(2);
                EditorGUILayout.EndVertical();

                GUILayout.Space(5);
            }

            // Force the name at the top
            string lMotionName = EditorGUILayout.TextField(new GUIContent("Name", "Friendly name of the motion that can be searched for."), rMotion.Name);
            if (lMotionName != rMotion.Name)
            {
                lIsDirty = true;
                rMotion.Name = lMotionName;
            }

            // Priority to determine which motion runs
            float lNewMotionPriority = EditorGUILayout.FloatField(new GUIContent("Priority", "Higher priorities will run over lower priorities."), rMotion.Priority);
            if (lNewMotionPriority != rMotion.Priority)
            {
                lIsDirty = true;
                rMotion.Priority = lNewMotionPriority;
            }

            if (EditorHelper.BoolField("Override Layers", "Determines if this motion forces later layers to go the empty state.", rMotion.OverrideLayers, mTarget))
            {
                lIsDirty = true;
                rMotion.OverrideLayers = EditorHelper.FieldBoolValue;
            }

            // Reactivation delay
            float lReactivationDelay = EditorGUILayout.FloatField(new GUIContent("React. Delay", "Once deactivated, seconds before activation can occur again."), rMotion.ReactivationDelay);
            if (lReactivationDelay != rMotion.ReactivationDelay)
            {
                lIsDirty = true;
                rMotion.ReactivationDelay = lReactivationDelay;
            }

            if (EditorHelper.BoolField("Show Debug", "Determines if the motion will render debug information (it is has any).", rMotion._ShowDebug, mTarget))
            {
                lIsDirty = true;
                rMotion._ShowDebug = EditorHelper.FieldBoolValue;
            }
        }

        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical(EditorHelper.Box);

        EditorGUILayout.BeginHorizontal();

        GUILayout.Space(4f);

        if (GUILayout.Button(new GUIContent("", "Tags"), MotionControllerEditor.OrangeTagsButton, GUILayout.Width(16), GUILayout.Height(16)))
        {
            mTarget.EditorShowTags = !mTarget.EditorShowTags;
        }

        if (GUILayout.Button(new GUIContent((mTarget.EditorShowTags ? "Hide" : "Show") + " Tags", "Click to show/hide tags for each motion."), GUI.skin.label, GUILayout.MinWidth(50f)))
        {
            mTarget.EditorShowTags = !mTarget.EditorShowTags;
        }

        EditorGUILayout.EndHorizontal();

        if (mTarget.EditorShowTags)
        {
            GUILayout.Space(5f);

            EditorGUILayout.BeginVertical(EditorHelper.GroupBox);

            mMotionTagList.DoLayoutList();

            EditorGUILayout.EndVertical();
        }

        EditorGUILayout.EndVertical();

        GUILayout.Space(5f);

        if (rMotion.ActionAlias.Length > 0 && !InputManagerHelper.IsDefined(rMotion.ActionAlias))
        {
            //if (!ReflectionHelper.IsTypeValid("com.ootii.Input.EasyInput, " + AssemblyHelper.AssemblyInfo))
            //{
            //    EditorGUILayout.BeginVertical(EditorHelper.Box);

            //    EditorGUILayout.HelpBox("Missing Unity Input Manager values. Press the button below to set them up.", MessageType.Warning);

            //    if (GUILayout.Button("Setup Input Entries", EditorStyles.miniButton))
            //    {
            //        CreateInputManagerSettings();
            //    }

            //    EditorGUILayout.EndVertical();

            //    GUILayout.Space(5);
            //}
        }

        // Allow the motion to render it's own UI
        bool lIsMotionDirty = rMotion.OnInspectorGUI();
        if (lIsMotionDirty) { lIsDirty = true; }

        // Update the motion if there's a change
        if (lIsDirty)
        {
            SerializeMotion(rMotion);
        }

        return lIsDirty;
    }

    /// <summary>
    /// Callback for when motions are selected
    /// </summary>
    /// <param name="rMotions"></param>
    private void OnMotionsSelected(List<Type> rTypes)
    {
        for (int i = 0; i < rTypes.Count; i++)
        {
            MotionControllerMotion lMotion = Activator.CreateInstance(rTypes[i]) as MotionControllerMotion;
            lMotion.MotionController = mTarget;
            lMotion.MotionLayer = mTarget.MotionLayers[mLayerList.index];
            mTarget.MotionLayers[mLayerList.index].Motions.Add(lMotion);

            lMotion.Reset();

            mTarget.MotionLayers[mLayerList.index].MotionDefinitions.Add(lMotion.SerializeMotion());
        }

        if (rTypes.Count > 0)
        { 
            if (IsFiltering)
            {
                InstanciateMotionList(mLayerList);
                mMotionList.index = mFilteredMotions.Count - 1;
            }
            else
            {
                mMotionList.index = mTarget.MotionLayers[mLayerList.index].Motions.Count - 1;
            }

            OnMotionListItemSelect(mMotionList);
            mIsDirty = true;
        }
    }

    #endregion

    #region Tag List

    /// <summary>
    /// Create the reorderable list
    /// </summary>
    private void InstantiateMotionTagList()
    {
        mMotionTags.Clear();

        MotionControllerMotion lMotion = null;
        if (mLayerList.index >= 0 && mMotionList.index >= 0)
        {
            MotionControllerLayer lLayer = mTarget.MotionLayers[mLayerList.index];
            lMotion = lLayer.Motions[mMotionList.index];
            if (lMotion.Tags != null)
            {
                string[] lTags = lMotion.Tags;
                Array.Sort(lTags, StringComparer.OrdinalIgnoreCase);
                mMotionTags.AddRange(lTags);
            }
        }

        mMotionTagList = new ReorderableList(mMotionTags, typeof(string), false, true, true, true);
        mMotionTagList.drawHeaderCallback = DrawMotionTagListHeader;
        mMotionTagList.drawFooterCallback = DrawMotionTagListFooter;
        mMotionTagList.drawElementCallback = DrawMotionTagListItem;
        mMotionTagList.onAddCallback = OnMotionTagListItemAdd;
        mMotionTagList.onRemoveCallback = OnMotionTagListItemRemove;
        mMotionTagList.onSelectCallback = OnMotionTagListItemSelect;
        mMotionTagList.footerHeight = 17f;

        if (lMotion != null)
        {
            if (lMotion.EditorTagIndex >= 0 && lMotion.EditorTagIndex < mMotionTagList.count)
            {
                mMotionTagList.index = lMotion.EditorTagIndex;
            }
        }
    }

    /// <summary>
    /// Header for the list
    /// </summary>
    /// <param name="rRect"></param>
    private void DrawMotionTagListHeader(Rect rRect)
    {
        EditorGUI.LabelField(rRect, "Tags");
    }

    /// <summary>
    /// Allows us to draw each item in the list
    /// </summary>
    /// <param name="rRect"></param>
    /// <param name="rIndex"></param>
    /// <param name="rIsActive"></param>
    /// <param name="rIsFocused"></param>
    private void DrawMotionTagListItem(Rect rRect, int rIndex, bool rIsActive, bool rIsFocused)
    {
        if (rIndex < mMotionTags.Count)
        {
            rRect.y += 2;

            float lWidth = (rRect.width - 5f) * 0.5f;

            Rect lValueRect = new Rect(rRect.x, rRect.y, lWidth, EditorGUIUtility.singleLineHeight);
            EditorGUI.LabelField(lValueRect, mMotionTags[rIndex]);
        }
    }

    /// <summary>
    /// Footer for the list
    /// </summary>
    /// <param name="rRect"></param>
    private void DrawMotionTagListFooter(Rect rRect)
    {
        Rect lTextRect = new Rect(rRect.x, rRect.y + 1, rRect.width - 4 - 28 - 28, 16);
        mTagValue = EditorGUI.TextField(lTextRect, mTagValue);

        Rect lAddRect = new Rect(rRect.x + rRect.width - 28 - 28 - 1, rRect.y + 1, 28, 15);
        if (GUI.Button(lAddRect, new GUIContent("+", "Add Tag."), EditorStyles.miniButtonLeft)) { OnMotionTagListItemAdd(mMotionTagList); }

        Rect lDeleteRect = new Rect(lAddRect.x + lAddRect.width, lAddRect.y, 28, 15);
        if (GUI.Button(lDeleteRect, new GUIContent("-", "Delete Tag."), EditorStyles.miniButtonRight)) { OnMotionTagListItemRemove(mMotionTagList); };
    }

    /// <summary>
    /// Allows us to add to a list
    /// </summary>
    /// <param name="rList"></param>
    private void OnMotionTagListItemAdd(ReorderableList rList)
    {
        if (mTagValue.Length == 0) { return; }
        if (((MotionControllerMotion)mMotionList.list[mMotionList.index]).TagExists(mTagValue)) { return; }

        MotionControllerMotion lMotion = mMotionList.list[mMotionList.index] as MotionControllerMotion;
        lMotion.AddTag(mTagValue);

        string[] lTags = lMotion.Tags;
        Array.Sort(lTags, StringComparer.OrdinalIgnoreCase);

        mMotionTagList.list.Clear();

        int lIndex = 0;
        for (int i = 0; i < lTags.Length; i++)
        {
            mMotionTagList.list.Add(lTags[i]);
            if (lTags[i] == mTagValue) { lIndex = i; }
        }

        mMotionTagList.index = lIndex;
        OnMotionTagListItemSelect(rList);

        mTagValue = "";

        // Update the motion if there's a change
        SerializeMotion(lMotion);

        mIsDirty = true;
    }

    /// <summary>
    /// Allows us process when a list is selected
    /// </summary>
    /// <param name="rList"></param>
    private void OnMotionTagListItemSelect(ReorderableList rList)
    {
        ((MotionControllerMotion)mMotionList.list[mMotionList.index]).EditorTagIndex = rList.index;
    }

    /// <summary>
    /// Allows us to stop before removing the item
    /// </summary>
    /// <param name="rList"></param>
    private void OnMotionTagListItemRemove(ReorderableList rList)
    {
        if (EditorUtility.DisplayDialog("Warning!", "Are you sure you want to delete the tag?", "Yes", "No"))
        {
            int lIndex = rList.index;
            string lItem = mMotionTags[lIndex];

            MotionControllerMotion lMotion = mMotionList.list[mMotionList.index] as MotionControllerMotion;
            lMotion.RemoveTag(lItem);

            rList.index--;

            mMotionTags.RemoveAt(lIndex);

            OnMotionTagListItemSelect(rList);

            // Update the motion if there's a change
            SerializeMotion(lMotion);

            mIsDirty = true;
        }
    }

    #endregion

    #region Pack List

    /// <summary>
    /// Create the reorderable list
    /// </summary>
    private void InstanciatePackList()
    {
        mPackList = new ReorderableList(mPackTypes, typeof(Type), false, true, true, true);
        mPackList.drawHeaderCallback = DrawPackListHeader;
        mPackList.drawFooterCallback = DrawPackListFooter;
        mPackList.drawElementCallback = DrawPackListItem;
        mPackList.onAddCallback = OnPackListItemAdd;
        mPackList.onRemoveCallback = OnPackListItemRemove;
        mPackList.onSelectCallback = OnPackListItemSelect;
        mPackList.onReorderCallback = OnPackListReorder;
        mPackList.footerHeight = 5f;

        if (mTarget.EditorPackIndex >= 0 && mTarget.EditorPackIndex < mPackList.count)
        {
            mPackList.index = mTarget.EditorPackIndex;
            OnPackListItemSelect(mPackList);
        }
    }

    /// <summary>
    /// Header for the list
    /// </summary>
    /// <param name="rRect"></param>
    private void DrawPackListHeader(Rect rRect)
    {
        EditorGUI.LabelField(rRect, "Motion Packs");
    }

    /// <summary>
    /// Allows us to draw each item in the list
    /// </summary>
    /// <param name="rRect"></param>
    /// <param name="rIndex"></param>
    /// <param name="rIsActive"></param>
    /// <param name="rIsFocused"></param>
    private void DrawPackListItem(Rect rRect, int rIndex, bool rIsActive, bool rIsFocused)
    {
        if (rIndex < mPackTypes.Count)
        {
            Rect lNameRect = new Rect(rRect.x, rRect.y + 1, rRect.width, EditorGUIUtility.singleLineHeight);
            EditorGUI.LabelField(lNameRect, (rIndex == 0 ? "[Default Motions]" : mPackNames[rIndex]));
        }
    }

    /// <summary>
    /// Footer for the list
    /// </summary>
    /// <param name="rRect"></param>
    private void DrawPackListFooter(Rect rRect)
    {
    }

    /// <summary>
    /// Allows us to add to a list
    /// </summary>
    /// <param name="rList"></param>
    private void OnPackListItemAdd(ReorderableList rList)
    {
    }

    /// <summary>
    /// Allows us process when a list is selected
    /// </summary>
    /// <param name="rList"></param>
    private void OnPackListItemSelect(ReorderableList rList)
    {
        mTarget.EditorPackIndex = rList.index;
        if (mTarget.EditorPackIndex == -1) { return; }
    }

    /// <summary>
    /// Allows us to stop before removing the item
    /// </summary>
    /// <param name="rList"></param>
    private void OnPackListItemRemove(ReorderableList rList)
    {
    }

    /// <summary>
    /// Allows us to process after the motions are reordered
    /// </summary>
    /// <param name="rList"></param>
    private void OnPackListReorder(ReorderableList rList)
    {
    }

    /// <summary>
    /// Renders the currently selected step
    /// </summary>
    /// <param name="rStep"></param>
    private bool DrawPackDetailItem(int rIndex)
    {
        bool lIsDirty = false;

        if (mPackTypes[rIndex] != null)
        {
            MethodInfo lInspector = mPackTypes[rIndex].GetMethod("OnPackInspector", BindingFlags.Static | BindingFlags.Public);
            if (lInspector != null) { lIsDirty = (bool)lInspector.Invoke(null, new object[] { mTarget }); }
        }

        return lIsDirty;
    }

    #endregion

    #region Support Functions

    /// <summary>
    /// Sets up the Style of walk/run
    /// </summary>
    private void EnableMMOStyle()
    {
        // Assign the Animator Controller
        AssignAnimatorController();

        // CDL 07/03/2018 - obtain the assembly name from AssemblyHelper.AssemblyInfo
        // Get or create the input source        
        IInputSource lInputSource = CreateInputSource("com.ootii.Input.EasyInputSource, " + AssemblyHelper.AssemblyInfo);
        if (lInputSource == null) { lInputSource = CreateInputSource<UnityInputSource>(); }
        lInputSource.IsEnabled = true;
        ReflectionHelper.SetProperty(lInputSource, "ViewActivator", 3);

        GameObject lInputSourceGO = null;
        if (lInputSource is MonoBehaviour)
        {
            lInputSourceGO = ((MonoBehaviour)lInputSource).gameObject;
        }

        mTarget.InputSourceOwner = lInputSourceGO;

        // Get or create the camera
        //bool lIsAdventureRigAvailable = ReflectionHelper.IsTypeValid("com.ootii.Cameras.CameraController, " + AssemblyHelper.AssemblyInfo);

        // CDL 07/03/2018 - obtain the assembly name from AssemblyHelper.AssemblyInfo
        BaseCameraRig lCameraRig = CreateCameraRig("com.ootii.Cameras.CameraController, " + AssemblyHelper.AssemblyInfo);
        if (lCameraRig == null) { lCameraRig = CreateCameraRig("com.ootii.Cameras.OrbitRig, " + AssemblyHelper.AssemblyInfo); }
         if (lCameraRig == null) { return; }

        lCameraRig.IsFixedUpdateEnabled = false;
        lCameraRig.IsInternalUpdateEnabled = false;
        lCameraRig.Anchor = mTarget.transform;
        ReflectionHelper.SetProperty(lCameraRig, "InputSourceOwner", lInputSourceGO);
        ReflectionHelper.SetProperty(lCameraRig, "AnchorOrbitsCamera", false);

        mTarget.CameraTransform = lCameraRig.gameObject.transform;

        // Ensure we have the right number of layers
        if (mTarget.MotionLayers.Count <= 0)
        {
            mTarget.MotionLayers.Add(new MotionControllerLayer("Locomotion", mTarget));
        }

        // Test if the right motions exist. If not, create them
        Idle lIdleMotion = CreateMotion<Idle>(0);
        //lIdleMotion.RotateWithViewInputX = true;
        lIdleMotion.RotateWithCamera = true;
        lIdleMotion.RotateWithInput = false;
        lIdleMotion.RotateWithMovementInputX = true;
        lIdleMotion.ForceViewOnInput = true;
        SerializeMotion(lIdleMotion);

        Fall lFallMotion = CreateMotion<Fall>(0);
        SerializeMotion(lFallMotion);

        WalkRunRotate_v2 lWalkMotion = CreateMotion<WalkRunRotate_v2>(0);
        //lWalkMotion.RotateWithViewInputX = true;
        //lWalkMotion.RotateWithMovementInputX = true;
        //lWalkMotion.ForceViewOnInput = true;
        SerializeMotion(lWalkMotion);

        //if (lIsAdventureRigAvailable)
        //{
        //    WalkRunStrafe lWalkMotionStrafe = CreateMotion<WalkRunStrafe>(0);
        //    lWalkMotionStrafe.Priority = lWalkMotion.Priority + 1;
        //    //lWalkMotionStrafe.ActivateWithAltCameraMode = true;
        //    //lWalkMotionStrafe.ForceViewOnInput = false;
        //    SerializeMotion(lWalkMotionStrafe);
        //}
        //else
        //{
            DisableMotion<WalkRunStrafe_v2>(0);
        //}

        DisableMotion<WalkRunPivot_v2>(0);

        EditorUtility.DisplayDialog("Motion Controller", "The new style was setup", "Close");
    }

    /// <summary>
    /// Sets up the Style of walk/run
    /// </summary>
    private void EnableAdventureStyle()
    {
        // Assign the Animator Controller
        AssignAnimatorController();

        // CDL 07/03/2018 - obtain the assembly name from AssemblyHelper.AssemblyInfo
        // Get or create the input source
        IInputSource lInputSource = CreateInputSource("com.ootii.Input.EasyInputSource, " + AssemblyHelper.AssemblyInfo);
        if (lInputSource == null) { lInputSource = CreateInputSource<UnityInputSource>(); }
        lInputSource.IsEnabled = true;
        ReflectionHelper.SetProperty(lInputSource, "ViewActivator", 2);

        GameObject lInputSourceGO = null;
        if (lInputSource is MonoBehaviour)
        {
            lInputSourceGO = ((MonoBehaviour)lInputSource).gameObject;
        }

        mTarget.InputSourceOwner = lInputSourceGO;

        // Get or create the camera
        //bool lIsAdventureRigAvailable = ReflectionHelper.IsTypeValid("com.ootii.Cameras.CameraController, " + AssemblyHelper.AssemblyInfo);

        // CDL 07/03/2018 - obtain the assembly name from AssemblyHelper.AssemblyInfo
        BaseCameraRig lCameraRig = CreateCameraRig("com.ootii.Cameras.CameraController, " + AssemblyHelper.AssemblyInfo);
        if (lCameraRig == null) { lCameraRig = CreateCameraRig("com.ootii.Cameras.OrbitRig, " + AssemblyHelper.AssemblyInfo); }
        if (lCameraRig == null) { return; }

        lCameraRig.IsFixedUpdateEnabled = false;
        lCameraRig.IsInternalUpdateEnabled = false;
        lCameraRig.Anchor = mTarget.transform;
        ReflectionHelper.SetProperty(lCameraRig, "InputSourceOwner", lInputSourceGO);
        ReflectionHelper.SetProperty(lCameraRig, "AnchorOrbitsCamera", true);

        mTarget.CameraTransform = lCameraRig.gameObject.transform;

        // Ensure we have the right number of layers
        if (mTarget.MotionLayers.Count <= 0)
        {
            mTarget.MotionLayers.Add(new MotionControllerLayer("Locomotion", mTarget));
        }

        // Test if the right motions exist. If not, create them
        Idle lIdleMotion = CreateMotion<Idle>(0);
        //lIdleMotion.RotateWithViewInputX = false;
        lIdleMotion.RotateWithCamera = false;
        lIdleMotion.RotateWithInput = false;
        lIdleMotion.RotateWithMovementInputX = false;
        lIdleMotion.ForceViewOnInput = false;
        SerializeMotion(lIdleMotion);

        Fall lFallMotion = CreateMotion<Fall>(0);
        SerializeMotion(lFallMotion);

        WalkRunPivot_v2 lWalkMotion = CreateMotion<WalkRunPivot_v2>(0);
        SerializeMotion(lWalkMotion);

        //if (lIsAdventureRigAvailable)
        //{
        //    WalkRunStrafe lWalkMotionStrafe = CreateMotion<WalkRunStrafe>(0);
        //    lWalkMotionStrafe.Priority = lWalkMotion.Priority + 1;
        //    //lWalkMotionStrafe.ActivateWithAltCameraMode = true;
        //    //lWalkMotionStrafe.ForceViewOnInput = false;
        //    SerializeMotion(lWalkMotionStrafe);
        //}
        //else
        //{
            DisableMotion<WalkRunStrafe_v2>(0);
        //}

        DisableMotion<WalkRunRotate_v2>(0);

        EditorUtility.DisplayDialog("Motion Controller", "The new style was setup", "Close");
    }

    /// <summary>
    /// Sets up the Style of walk/run
    /// </summary>
    private void EnableShooterStyle()
    {
        // Assign the Animator Controller
        AssignAnimatorController();

        // CDL 07/03/2018 - obtain the assembly name from AssemblyHelper.AssemblyInfo
        // Get or create the input source
        IInputSource lInputSource = CreateInputSource("com.ootii.Input.EasyInputSource, " + AssemblyHelper.AssemblyInfo);
        if (lInputSource == null) { lInputSource = CreateInputSource<UnityInputSource>(); }
        lInputSource.IsEnabled = true;
        ReflectionHelper.SetProperty(lInputSource, "ViewActivator", 2);

        GameObject lInputSourceGO = null;
        if (lInputSource is MonoBehaviour)
        {
            lInputSourceGO = ((MonoBehaviour)lInputSource).gameObject;
        }

        mTarget.InputSourceOwner = lInputSourceGO;

        // CDL 07/03/2018 - obtain the assembly name from AssemblyHelper.AssemblyInfo
        // Get or create the camera
        BaseCameraRig lCameraRig = CreateCameraRig("com.ootii.Cameras.CameraController, " + AssemblyHelper.AssemblyInfo);
        if (lCameraRig == null) { lCameraRig = CreateCameraRig("com.ootii.Cameras.FollowRig, " + AssemblyHelper.AssemblyInfo); }
        if (lCameraRig == null) { return; }

        lCameraRig.IsFixedUpdateEnabled = false;
        lCameraRig.IsInternalUpdateEnabled = false;
        lCameraRig.Anchor = mTarget.transform;
        ReflectionHelper.SetProperty(lCameraRig, "InputSourceOwner", lInputSourceGO);
        ReflectionHelper.SetProperty(lCameraRig, "AnchorOrbitsCamera", false);

        mTarget.CameraTransform = lCameraRig.gameObject.transform;

        // Ensure we have the right number of layers
        if (mTarget.MotionLayers.Count <= 0)
        {
            mTarget.MotionLayers.Add(new MotionControllerLayer("Locomotion", mTarget));
        }

        // Test if the right motions exist. If not, create them
        Idle lIdleMotion = CreateMotion<Idle>(0);
        //lIdleMotion.RotateWithViewInputX = true;
        lIdleMotion.RotateWithCamera = true;
        lIdleMotion.RotateWithInput = false;
        lIdleMotion.RotateWithMovementInputX = false;
        lIdleMotion.ForceViewOnInput = true;
        SerializeMotion(lIdleMotion);

        Fall lFallMotion = CreateMotion<Fall>(0);
        SerializeMotion(lFallMotion);

        WalkRunStrafe_v2 lWalkMotion = CreateMotion<WalkRunStrafe_v2>(0);
        //lWalkMotion.ActivateWithAltCameraMode = false;
        //lWalkMotion.ForceViewOnInput = true;
        SerializeMotion(lWalkMotion);

        DisableMotion<WalkRunRotate_v2>(0);
        DisableMotion<WalkRunPivot_v2>(0);

        EditorUtility.DisplayDialog("Motion Controller", "The new style was setup", "Close");
    }

    /// <summary>
    /// Sets up the option
    /// </summary>
    private void EnableJumping(bool rEnable)
    {
        MotionControllerMotion lMotion = CreateMotion<Jump>(0);
        lMotion.IsEnabled = rEnable;
        SerializeMotion(lMotion);

        lMotion = CreateMotion<RunningJump>(0);
        lMotion.IsEnabled = rEnable;
        SerializeMotion(lMotion);
    }

    /// <summary>
    /// Sets up the option
    /// </summary>
    private void EnableClimbing(bool rEnable)
    {
        MotionControllerMotion lMotion = CreateMotion<Climb_0_5m>(0);
        lMotion.IsEnabled = rEnable;
        SerializeMotion(lMotion);

        lMotion = CreateMotion<Climb_1m>(0);
        lMotion.IsEnabled = rEnable;
        SerializeMotion(lMotion);

        lMotion = CreateMotion<Climb_1_8m>(0);
        lMotion.IsEnabled = rEnable;
        SerializeMotion(lMotion);

        lMotion = CreateMotion<Climb_2_5m>(0);
        lMotion.IsEnabled = rEnable;
        SerializeMotion(lMotion);
    }

    /// <summary>
    /// Sets up the option
    /// </summary>
    private void EnableLaddering(bool rEnable)
    {
        MotionControllerMotion lMotion = CreateMotion<ClimbLadder>(0);
        lMotion.IsEnabled = rEnable;
        SerializeMotion(lMotion);
    }

    /// <summary>
    /// Sets up the option
    /// </summary>
    private void EnableScaling(bool rEnable)
    {
        MotionControllerMotion lMotion = CreateMotion<ClimbWall>(0);
        lMotion.IsEnabled = rEnable;
        SerializeMotion(lMotion);
    }

    /// <summary>
    /// Sets up the option
    /// </summary>
    private void EnableSneaking(bool rEnable)
    {
        MotionControllerMotion lMotion = CreateMotion<Sneak_v2>(0);
        lMotion.IsEnabled = rEnable;
        SerializeMotion(lMotion);
    }

    /// <summary>
    /// Creates the camera rig if need and returns the GO
    /// </summary>
    /// <returns></returns>
    private void AssignAnimatorController()
    {
        Animator lAnimator = mTarget.Animator;
        if (lAnimator == null) { lAnimator = mTarget.gameObject.GetComponent<Animator>(); }
        if (lAnimator == null) { lAnimator = mTarget.gameObject.GetComponentInChildren<Animator>(); }
        if (lAnimator == null) { lAnimator = mTarget.gameObject.AddComponent<Animator>(); }

        if (lAnimator.runtimeAnimatorController == null)
        {
            RuntimeAnimatorController lController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/ootii/Assets/MotionController/Content/Animations/Humanoid/Humanoid.controller");
            lAnimator.runtimeAnimatorController = lController;
        }
    }

    /// <summary>
    /// Creates the camera rig if need and returns the GO
    /// </summary>
    /// <returns></returns>
    private IInputSource CreateInputSource<T>() where T : IInputSource
    {
        IInputSource[] lInputSources = InterfaceHelper.GetComponents<IInputSource>();
        if (lInputSources != null && lInputSources.Length > 0) { return lInputSources[0]; }

        // Create the input source
        GameObject lInputSourceGO = new GameObject("Input Source");
        T lInputSource = (T)((object)lInputSourceGO.AddComponent(typeof(T)));

        return lInputSource;
    }

    /// <summary>
    /// Creates the camera rig if need and returns the GO
    /// </summary>
    /// <returns></returns>
    private IInputSource CreateInputSource(string rType)
    {
        if (!ReflectionHelper.IsTypeValid(rType)) { return null; }

        Type lType = Type.GetType(rType);

        IInputSource[] lInputSources = InterfaceHelper.GetComponents<IInputSource>();
        if (lInputSources != null && lInputSources.Length > 0) { return lInputSources[0]; }

        // Create the input source
        GameObject lInputSourceGO = new GameObject("Input Source");
        IInputSource lInputSource = lInputSourceGO.AddComponent(lType) as IInputSource; 

        return lInputSource;
    }

    /// <summary>
    /// Creates the camera rig if need and returns the GO
    /// </summary>
    /// <returns></returns>
    private T CreateCameraRig<T>() where T : BaseCameraRig
    {
        GameObject lCameraRigGO = null;

        // Check if the camera rig is setup
        GameObject lCameraGO = GameObject.FindGameObjectWithTag("MainCamera");
        if (lCameraGO == null)
        {
            Camera lCamera = Component.FindObjectOfType<Camera>();
            if (lCamera != null) { lCameraGO = lCamera.gameObject; }
        }

        if (lCameraGO == null)
        {
            EditorUtility.DisplayDialog("Warning!", "Unable to create the camera as no 'MainCamera' was found.", "Close");
            return default(T);
        }

        // Grab the camera's parent
        if (lCameraGO.transform.parent != null)
        {
            lCameraRigGO = lCameraGO.transform.parent.gameObject;
        }

        // Check if we need to create a rig
        if (lCameraRigGO == null)
        {
            lCameraRigGO = new GameObject("Camera Rig");
            lCameraRigGO.transform.position = lCameraGO.transform.position;
            lCameraRigGO.transform.rotation = lCameraGO.transform.rotation;

            lCameraGO.transform.parent = lCameraRigGO.transform;
        }

        // Disable any rigs that currently exist
        BaseCameraRig[] lCameraRigs = lCameraRigGO.GetComponents<BaseCameraRig>();
        for (int i = 0; i < lCameraRigs.Length; i++)
        {
            lCameraRigs[i].enabled = false;
        }

        // Check if the orbit rig is assigned
        T lCameraRig = lCameraRigGO.GetComponent(typeof(T)) as T;
        if (lCameraRig == null) { lCameraRig = lCameraRigGO.AddComponent(typeof(T)) as T; }

        lCameraRig.enabled = true;

        // Return the rig
        return lCameraRig;
    }

    /// <summary>
    /// Creates the camera rig if need and returns the GO
    /// </summary>
    /// <returns></returns>
    private BaseCameraRig CreateCameraRig(string rType)
    {
        if (!ReflectionHelper.IsTypeValid(rType)) { return null; }

        Type lType = Type.GetType(rType);

        GameObject lCameraRigGO = null;

        // Find the camera
        Camera lCamera = Component.FindObjectOfType<Camera>();
        if (lCamera != null) { lCamera.nearClipPlane = 0.1f; }

        // Check if the camera rig is setup
        GameObject lCameraGO = GameObject.FindGameObjectWithTag("MainCamera");
        if (lCameraGO == null && lCamera != null)
        {
            lCameraGO = lCamera.gameObject;
        }

        if (lCameraGO == null)
        {
            EditorUtility.DisplayDialog("Warning!", "Unable to create the camera as no 'MainCamera' was found.", "Close");
            return null;
        }

        // Grab the camera's parent
        if (lCameraGO.transform.parent != null)
        {
            lCameraRigGO = lCameraGO.transform.parent.gameObject;
        }

        // Check if we need to create a rig
        if (lCameraRigGO == null)
        {
            lCameraRigGO = new GameObject("Camera Rig");
            lCameraRigGO.transform.position = lCameraGO.transform.position;
            lCameraRigGO.transform.rotation = lCameraGO.transform.rotation;

            lCameraGO.transform.parent = lCameraRigGO.transform;
        }

        // Disable any rigs that currently exist
        BaseCameraRig[] lCameraRigs = lCameraRigGO.GetComponents<BaseCameraRig>();
        for (int i = 0; i < lCameraRigs.Length; i++)
        {
            lCameraRigs[i].enabled = false;
        }

        // Check if the orbit rig is assigned
        BaseCameraRig lCameraRig = lCameraRigGO.GetComponent(lType) as BaseCameraRig;
        if (lCameraRig == null) { lCameraRig = lCameraRigGO.AddComponent(lType) as BaseCameraRig; }

        lCameraRig.enabled = true;

        // Return the rig
        return lCameraRig;
    }

    /// <summary>
    /// Get or create the specified motion
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    private void DisableMotion<T>(int rLayerIndex) where T : MotionControllerMotion
    {
        if (rLayerIndex >= mTarget.MotionLayers.Count) { return; }

        MotionControllerMotion lMotion = mTarget.GetMotion(typeof(T)) as T;
        if (lMotion == null) { return; }

        lMotion.IsEnabled = false;

        SerializeMotion(lMotion);
    }

    /// <summary>
    /// Serialize the motion and update it's defintion
    /// </summary>
    /// <param name="rMotion"></param>
    private void SerializeMotion(MotionControllerMotion rMotion)
    {
        if (rMotion == null) { return; }

        int lIndex = rMotion.MotionLayer.Motions.IndexOf(rMotion);
        if (lIndex >= 0)
        {
            rMotion.MotionLayer.MotionDefinitions[lIndex] = rMotion.SerializeMotion();
        }
    }

    /// <summary>
    /// Get or create the specified motion
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    private T CreateMotion<T>(int rLayerIndex) where T : MotionControllerMotion
    {
        while (mTarget.MotionLayers.Count <= rLayerIndex)
        {
            mTarget.MotionLayers.Add(new MotionControllerLayer());
        }

        T lMotion = mTarget.GetMotion(typeof(T)) as T;
        if (lMotion == null)
        {
            lMotion = Activator.CreateInstance(typeof(T)) as T;
            mTarget.MotionLayers[rLayerIndex].AddMotion(lMotion);
            mTarget.MotionLayers[rLayerIndex].MotionDefinitions.Add(lMotion.SerializeMotion());

            if (EditorApplication.isPlaying)
            {
                lMotion.Awake();
                lMotion.Initialize();
                lMotion.LoadAnimatorData();
                lMotion.CreateInputManagerSettings();
            }
        }

        lMotion.IsEnabled = true;

        return lMotion;
    }

    /// <summary>
    /// Returns a friendly name for the type
    /// </summary>
    /// <param name="rType"></param>
    /// <returns></returns>
    private string GetFriendlyName(Type rType)
    {
        string lTypeName = rType.Name;
        object[] lMotionAttributes = rType.GetCustomAttributes(typeof(MotionNameAttribute), true);
        if (lMotionAttributes != null && lMotionAttributes.Length > 0) { lTypeName = ((MotionNameAttribute)lMotionAttributes[0]).Value; }

        return lTypeName;
    }

    /// <summary>
    /// If the input manager entries don't exist, create them
    /// </summary>
    private void CreateInputManagerSettings()
    {
        if (!InputManagerHelper.IsDefined("ChangeStance"))
        {
            InputManagerEntry lEntry = new InputManagerEntry();
            lEntry.Name = "ChangeStance";
            lEntry.PositiveButton = "t";
            lEntry.Gravity = 1000;
            lEntry.Dead = 0.001f;
            lEntry.Sensitivity = 1000;
            lEntry.Type = InputManagerEntryType.KEY_MOUSE_BUTTON;
            lEntry.Axis = 0;
            lEntry.JoyNum = 0;

            InputManagerHelper.AddEntry(lEntry);
        }

        if (!InputManagerHelper.IsDefined("Run"))
        {
            InputManagerEntry lEntry = new InputManagerEntry();
            lEntry.Name = "Run";
            lEntry.PositiveButton = "left shift";
            lEntry.Gravity = 1000;
            lEntry.Dead = 0.001f;
            lEntry.Sensitivity = 1000;
            lEntry.Type = InputManagerEntryType.KEY_MOUSE_BUTTON;
            lEntry.Axis = 0;
            lEntry.JoyNum = 0;

            InputManagerHelper.AddEntry(lEntry, true);

#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX

            lEntry = new InputManagerEntry();
            lEntry.Name = "Run";
            lEntry.PositiveButton = "";
            lEntry.Gravity = 1;
            lEntry.Dead = 0.3f;
            lEntry.Sensitivity = 1;
            lEntry.Type = InputManagerEntryType.JOYSTICK_AXIS;
            lEntry.Axis = 5;
            lEntry.JoyNum = 0;

            InputManagerHelper.AddEntry(lEntry, true);

#else

            lEntry = new InputManagerEntry();
            lEntry.Name = "Run";
            lEntry.PositiveButton = "";
            lEntry.Gravity = 1;
            lEntry.Dead = 0.3f;
            lEntry.Sensitivity = 1;
            lEntry.Type = InputManagerEntryType.JOYSTICK_AXIS;
            lEntry.Axis = 9;
            lEntry.JoyNum = 0;

            InputManagerHelper.AddEntry(lEntry, true);

#endif
        }

        if (!InputManagerHelper.IsDefined("Interact"))
        {
            InputManagerEntry lEntry = new InputManagerEntry();
            lEntry.Name = "Interact";
            lEntry.PositiveButton = "f";
            lEntry.Gravity = 1000;
            lEntry.Dead = 0.001f;
            lEntry.Sensitivity = 1000;
            lEntry.Type = InputManagerEntryType.KEY_MOUSE_BUTTON;
            lEntry.Axis = 0;
            lEntry.JoyNum = 0;
            InputManagerHelper.AddEntry(lEntry, true);
        }

        if (!InputManagerHelper.IsDefined("StrafeLeft"))
        {
            InputManagerEntry lEntry = new InputManagerEntry();
            lEntry.Name = "StrafeLeft";
            lEntry.PositiveButton = "q";
            lEntry.Gravity = 1000;
            lEntry.Dead = 0.001f;
            lEntry.Sensitivity = 1000;
            lEntry.Type = InputManagerEntryType.KEY_MOUSE_BUTTON;
            lEntry.Axis = 0;
            lEntry.JoyNum = 0;

            InputManagerHelper.AddEntry(lEntry, true);
        }

        if (!InputManagerHelper.IsDefined("StrafeRight"))
        {
            InputManagerEntry lEntry = new InputManagerEntry();
            lEntry.Name = "StrafeRight";
            lEntry.PositiveButton = "e";
            lEntry.Gravity = 1000;
            lEntry.Dead = 0.001f;
            lEntry.Sensitivity = 1000;
            lEntry.Type = InputManagerEntryType.KEY_MOUSE_BUTTON;
            lEntry.Axis = 0;
            lEntry.JoyNum = 0;

            InputManagerHelper.AddEntry(lEntry, true);
        }

        if (!InputManagerHelper.IsDefined("ActivateRotation"))
        {
            InputManagerEntry lEntry = new InputManagerEntry();
            lEntry.Name = "ActivateRotation";
            lEntry.PositiveButton = "mouse 1";
            lEntry.Gravity = 1000;
            lEntry.Dead = 0.001f;
            lEntry.Sensitivity = 1000;
            lEntry.Type = InputManagerEntryType.KEY_MOUSE_BUTTON;
            lEntry.Axis = 1;
            lEntry.JoyNum = 0;

            InputManagerHelper.AddEntry(lEntry, true);

#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX

            lEntry = new InputManagerEntry();
            lEntry.Name = "ActivateRotation";
            lEntry.PositiveButton = "";
            lEntry.Gravity = 1;
            lEntry.Dead = 0.3f;
            lEntry.Sensitivity = 1;
            lEntry.Type = InputManagerEntryType.JOYSTICK_AXIS;
            lEntry.Axis = 3;
            lEntry.JoyNum = 0;

            InputManagerHelper.AddEntry(lEntry, true);

#else

            lEntry = new InputManagerEntry();
            lEntry.Name = "ActivateRotation";
            lEntry.PositiveButton = "";
            lEntry.Gravity = 1;
            lEntry.Dead = 0.3f;
            lEntry.Sensitivity = 1;
            lEntry.Type = InputManagerEntryType.JOYSTICK_AXIS;
            lEntry.Axis = 4;
            lEntry.JoyNum = 0;

            InputManagerHelper.AddEntry(lEntry, true);

#endif
        }

        if (!InputManagerHelper.IsDefined("Cover Toggle"))
        {
            InputManagerEntry lEntry = new InputManagerEntry();
            lEntry.Name = "Cover Toggle";
            lEntry.PositiveButton = "m";
            lEntry.Gravity = 1000;
            lEntry.Dead = 0.001f;
            lEntry.Sensitivity = 1000;
            lEntry.Type = InputManagerEntryType.KEY_MOUSE_BUTTON;
            lEntry.Axis = 0;
            lEntry.JoyNum = 0;

            InputManagerHelper.AddEntry(lEntry, true);
        }

        for (int i = 0; i < mTarget.MotionLayers.Count; i++)
        {
            for (int j = 0; j < mTarget.MotionLayers[i].Motions.Count; j++)
            {
                mTarget.MotionLayers[i].Motions[j].CreateInputManagerSettings();
            }
        }
    }

    /// <summary>
    /// Determines if we filter this motion out based on the search criteria
    /// </summary>
    /// <param name="rMotion">Motion test</param>
    /// <returns>True if the motion should not be shown</returns>
    private bool IsMotionFiltered(MotionControllerMotion rMotion, List<string> rSearchText, List<string> rSearchTags)
    {
        Type lType = rMotion.GetType();

        // First, exclude all items that don't match the tag
        if (rSearchTags.Count > 0)
        {
            bool lIsMotionTagValid = true;
            for (int i = 0; i < rSearchTags.Count; i++)
            {
                if (!rMotion.TagExists(rSearchTags[i]))
                {
                    lIsMotionTagValid = false;
                    break;
                }
            }

            bool lIsMotionTypeTagValid = true;
            for (int i = 0; i < rSearchTags.Count; i++)
            {
                if (!MotionTypeTagsAttribute.Contains(lType, rSearchTags[i]))
                {
                    lIsMotionTypeTagValid = false;
                    break;
                }
            }

            if (!lIsMotionTagValid && !lIsMotionTypeTagValid) { return true; }
        }

        // Next, exclude all items that don't match the search
        if (rSearchText.Count > 0)
        {
            for (int i = 0; i < rSearchText.Count; i++)
            {
                string lName = rMotion.Name;
                if (lName == null || lName.Length == 0) { lName = MotionNameAttribute.GetName(lType); }
                if (lName == null || lName.Length == 0) { lName = BaseNameAttribute.GetName(lType); }

                if (!lName.Contains(rSearchText[i], StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
        }

        return false;
    }

    #endregion

    #region Styles

    /// <summary>
    /// Box used to group standard GUI elements
    /// </summary>
    private static GUIStyle mMMOIcon = null;
    private static GUIStyle MMOIcon
    {
        get
        {
            if (mMMOIcon == null)
            {
                Texture2D lTexture = Resources.Load<Texture2D>(EditorGUIUtility.isProSkin ? "MMOIcon" : "MMOIcon");

                mMMOIcon = new GUIStyle(GUI.skin.box);
                mMMOIcon.normal.background = lTexture;
                mMMOIcon.padding = new RectOffset(0, 0, 0, 0);
                mMMOIcon.border = new RectOffset(0, 0, 0, 0);
            }

            return mMMOIcon;
        }
    }

    /// <summary>
    /// Box used to group standard GUI elements
    /// </summary>
    private static GUIStyle mAdventureIcon = null;
    private static GUIStyle AdventureIcon
    {
        get
        {
            if (mAdventureIcon == null)
            {
                Texture2D lTexture = Resources.Load<Texture2D>(EditorGUIUtility.isProSkin ? "AdventureIcon" : "AdventureIcon");

                mAdventureIcon = new GUIStyle(GUI.skin.box);
                mAdventureIcon.normal.background = lTexture;
                mAdventureIcon.padding = new RectOffset(0, 0, 0, 0);
                mAdventureIcon.border = new RectOffset(0, 0, 0, 0);
            }

            return mAdventureIcon;
        }
    }

    /// <summary>
    /// Box used to group standard GUI elements
    /// </summary>
    private static GUIStyle mShooterIcon = null;
    private static GUIStyle ShooterIcon
    {
        get
        {
            if (mShooterIcon == null)
            {
                Texture2D lTexture = Resources.Load<Texture2D>(EditorGUIUtility.isProSkin ? "ShooterIcon" : "ShooterIcon");

                mShooterIcon = new GUIStyle(GUI.skin.box);
                mShooterIcon.normal.background = lTexture;
                mShooterIcon.padding = new RectOffset(0, 0, 0, 0);
                mShooterIcon.border = new RectOffset(0, 0, 0, 0);
            }

            return mShooterIcon;
        }
    }

    /// <summary>
    /// Label
    /// </summary>
    private static GUIStyle mOptionText = null;
    private static GUIStyle OptionText
    {
        get
        {
            if (mOptionText == null)
            {
                mOptionText = new GUIStyle(GUI.skin.label);
                mOptionText.wordWrap = true;
                mOptionText.padding.top = 11;
            }

            return mOptionText;
        }
    }

    /// <summary>
    /// Label
    /// </summary>
    private static GUIStyle mOptionToggle = null;
    private static GUIStyle OptionToggle
    {
        get
        {
            if (mOptionToggle == null)
            {
                mOptionToggle = new GUIStyle(GUI.skin.toggle);
            }

            return mOptionToggle;
        }
    }

    /// <summary>
    /// Box used to group standard GUI elements
    /// </summary>
    private static GUIStyle mBasicIcon = null;
    private static GUIStyle BasicIcon
    {
        get
        {
            if (mBasicIcon == null)
            {
                Texture2D lTexture = Resources.Load<Texture2D>(EditorGUIUtility.isProSkin ? "BasicIcon_pro" : "BasicIcon");

                mBasicIcon = new GUIStyle(GUI.skin.button);
                mBasicIcon.normal.background = lTexture;
                mBasicIcon.padding = new RectOffset(0, 0, 0, 0);
                mBasicIcon.margin = new RectOffset(0, 0, 1, 0);
                mBasicIcon.border = new RectOffset(0, 0, 0, 0);
                mBasicIcon.stretchHeight = false;
                mBasicIcon.stretchWidth = false;

            }

            return mBasicIcon;
        }
    }

    /// <summary>
    /// Box used to group standard GUI elements
    /// </summary>
    private static GUIStyle mAdvancedIcon = null;
    private static GUIStyle AdvancedIcon
    {
        get
        {
            if (mAdvancedIcon == null)
            {
                Texture2D lTexture = Resources.Load<Texture2D>(EditorGUIUtility.isProSkin ? "AdvancedIcon_pro" : "AdvancedIcon");

                mAdvancedIcon = new GUIStyle(GUI.skin.button);
                mAdvancedIcon.normal.background = lTexture;
                mAdvancedIcon.padding = new RectOffset(0, 0, 0, 0);
                mAdvancedIcon.margin = new RectOffset(0, 0, 1, 0);
                mAdvancedIcon.border = new RectOffset(0, 0, 0, 0);
                mAdvancedIcon.stretchHeight = false;
                mAdvancedIcon.stretchWidth = false;

            }

            return mAdvancedIcon;
        }
    }

    /// <summary>
    /// Box used to group standard GUI elements
    /// </summary>
    private static GUIStyle mBox = null;
    private static GUIStyle Box
    {
        get
        {
            if (mBox == null)
            {
                //Texture2D lTexture = Resources.Load<Texture2D>(EditorGUIUtility.isProSkin ? "Editor/GroupBox_pro" : "Editor/OrangeGrayBox");
                Texture2D lTexture = Resources.Load<Texture2D>(EditorGUIUtility.isProSkin ? "GroupBox_pro" : "OrangeGrayBox");
                mBox = new GUIStyle(GUI.skin.box);
                mBox.normal.background = lTexture;
                mBox.padding = new RectOffset(0, 0, 0, 0);
                mBox.margin = new RectOffset(0, 0, 0, 0);
            }

            return mBox;
        }
    }

    /// <summary>
    /// Gray select target button
    /// </summary>
    private static GUIStyle mOrangeTagsButton = null;
    public static GUIStyle OrangeTagsButton
    {
        get
        {
            if (mOrangeTagsButton == null)
            {
                mOrangeTagsButton = new GUIStyle();
                //mOrangeTagsButton.normal.background = Resources.Load<Texture2D>(EditorGUIUtility.isProSkin ? "Editor/TagsButtonOrange_pro" : "Editor/TagsButtonOrange");
                mOrangeTagsButton.normal.background = Resources.Load<Texture2D>(EditorGUIUtility.isProSkin ? "TagsButtonOrange_pro" : "TagsButtonOrange");
                mOrangeTagsButton.margin = new RectOffset(0, 0, 2, 0);
            }

            return mOrangeTagsButton;
        }
    }

    #endregion
}
