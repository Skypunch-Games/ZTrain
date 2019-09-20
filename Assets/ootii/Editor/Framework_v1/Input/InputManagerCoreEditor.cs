#if OOTII_EI
using System;
using UnityEngine;
using UnityEditor;
using com.ootii.Input;

[CanEditMultipleObjects]
[CustomEditor(typeof(InputManagerCore))]
public class InputManagerCoreEditor : Editor
{
    // Helps us keep track of when the things need to be saved.
    private bool mIsDirty;

    // The actual class we're storing
    private InputManagerCore mInputManagerCore;
    private SerializedObject mInputManagerCoreSO;

    // Activators that can be selected
    private int[] mViewActivatorIDs = new int[] { 0, EnumInput.MOUSE_LEFT_BUTTON, EnumInput.MOUSE_RIGHT_BUTTON, EnumInput.MOUSE_MIDDLE_BUTTON, EnumInput.LEFT_SHIFT };
    private string[] mViewActivatorNames = new string[] { "None", "Left Mouse Button", "Right Mouse Button", "Middle Mouse Button", "Left Shift" };

    /// <summary>
    /// Called when the script object is loaded
    /// </summary>
    void OnEnable()
    {
        // Grab the serialized objects
        mInputManagerCore = (InputManagerCore)target;
        mInputManagerCoreSO = new SerializedObject(target);

        // Start up the input manager and set the stub
        InputManager.Core = mInputManagerCore;
    }

    /// <summary>
    /// Called when the inspector needs to draw
    /// </summary>
    public override void OnInspectorGUI()
    {
        // Pulls variables from runtime so we have the latest values.
        mInputManagerCoreSO.Update();

        EditorGUILayout.HelpBox("This component is now obsolete. Please use the Easy Input Source component instead.", MessageType.Error);

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(new GUIContent("Enable Xbox Controller", "Determines if we look for input from an Xbox controller"), GUILayout.Width(150));
        bool lNewIsXboxControllerEnabled = EditorGUILayout.Toggle(mInputManagerCore.IsXboxControllerEnabled);
        if (lNewIsXboxControllerEnabled != mInputManagerCore.IsXboxControllerEnabled)
        {
            mIsDirty = true;
            mInputManagerCore.IsXboxControllerEnabled = lNewIsXboxControllerEnabled;
        }
        EditorGUILayout.EndHorizontal();

        //EditorGUILayout.BeginHorizontal();
        //EditorGUILayout.LabelField(new GUIContent("Move Activator", "Key or button to press to enable movement."), GUILayout.Width(150));
        //int lNewMoveActivator = EditorGUILayout.Popup(mInputManagerCore.MoveActivator, mActivators);
        //if (lNewMoveActivator != mInputManagerCore.MoveActivator)
        //{
        //    mIsDirty = true;
        //    mInputManagerCore.MoveActivator = lNewMoveActivator;
        //}
        //EditorGUILayout.EndHorizontal();

        // Find the current view activator (if it exists)
        int lViewActivatorIndex = 0;
        for (int i = 0; i < mViewActivatorIDs.Length; i++)
        {
            if (mViewActivatorIDs[i] == mInputManagerCore.ViewActivator)
            {
                lViewActivatorIndex = i;
                break;
            }
        }

        EditorGUILayout.BeginHorizontal();

        // Let the user change it
        EditorGUILayout.LabelField(new GUIContent("View Activator", "Determines what button enables viewing."), GUILayout.Width(EditorGUIUtility.labelWidth - 3));
        int lNewViewActivatorIndex = EditorGUILayout.Popup(lViewActivatorIndex, mViewActivatorNames);
        if (lNewViewActivatorIndex != lViewActivatorIndex)
        {
            mIsDirty = true;
            mInputManagerCore.ViewActivator = mViewActivatorIDs[lNewViewActivatorIndex];
        }

        EditorGUILayout.EndHorizontal();

        // If there is a change... update.
        if (mIsDirty)
        {
            // Flag the object as needing to be saved
            EditorUtility.SetDirty(mInputManagerCore);

            // Pushes the values back to the runtime so it has the changes
            mInputManagerCoreSO.ApplyModifiedProperties();

            // Clear out the dirty flag
            mIsDirty = false;
        }
    }
}
#endif