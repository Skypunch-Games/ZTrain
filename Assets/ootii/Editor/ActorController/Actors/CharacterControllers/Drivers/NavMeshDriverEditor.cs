﻿using UnityEngine;
using UnityEditor;
using com.ootii.Actors;
using com.ootii.Helpers;

[CanEditMultipleObjects]
[CustomEditor(typeof(NavMeshDriver))]
public class NavMeshDriverEditor : Editor
{
    // Helps us keep track of when the list needs to be saved. This
    // is important since some changes happen in scene.
    private bool mIsDirty;

    // The actual class we're storing
    private NavMeshDriver mTarget;
    private SerializedObject mTargetSO;

    /// <summary>
    /// Called when the object is selected in the editor
    /// </summary>
    private void OnEnable()
    {
        // Grab the serialized objects
        mTarget = (NavMeshDriver)target;
        mTargetSO = new SerializedObject(target);
    }

    /// <summary>
    /// This function is called when the scriptable object goes out of scope.
    /// </summary>
    private void OnDisable()
    {
    }

    /// <summary>
    /// Called when the inspector needs to draw
    /// </summary>
    public override void OnInspectorGUI()
    {
        // Pulls variables from runtime so we have the latest values.
        mTargetSO.Update();

        GUILayout.Space(5);

        EditorHelper.DrawInspectorTitle("ootii Nav Mesh Driver");

        EditorHelper.DrawInspectorDescription("Actor driver that uses a Nav Mesh Agent (and Mecanim Animator) to tell the Actor Controller what to do.", MessageType.None);

        GUILayout.Space(5);

        if (EditorHelper.BoolField("Is Enabled", "Determines if the driver is actively controlling the actor.", mTarget.IsEnabled, mTarget))
        {
            mIsDirty = true;
            mTarget.IsEnabled = EditorHelper.FieldBoolValue;
        }

        if (EditorHelper.BoolField("Use Nav Position", "Determines if we'll use the nav mesh position directly or calculate our own.", mTarget.UseNavMeshPosition, mTarget))
        {
            mIsDirty = true;
            mTarget.UseNavMeshPosition = EditorHelper.FieldBoolValue;
        }

        if (EditorHelper.FloatField("Movement Speed", "Meters per second the actor moves.", mTarget.MovementSpeed, mTarget))
        {
            mIsDirty = true;
            mTarget.MovementSpeed = EditorHelper.FieldFloatValue;
        }

        if (EditorHelper.FloatField("Rotation Speed", "Degrees per second the actor rotates.", mTarget.RotationSpeed, mTarget))
        {
            mIsDirty = true;
            mTarget.RotationSpeed = EditorHelper.FieldFloatValue;
        }

        GUILayout.Space(5);

        if (EditorHelper.ObjectField<Transform>("Target", "Transform that we'll use the Nav Mesh Agent to follow.", mTarget.Target, mTarget))
        {
            mIsDirty = true;
            mTarget.Target = EditorHelper.FieldObjectValue as Transform;
        }

        if (EditorHelper.Vector3Field("Target Position", "Specific position the Nav Mesh Agent will head to.", mTarget.TargetPosition, mTarget))
        {
            mIsDirty = true;
            mTarget.TargetPosition = EditorHelper.FieldVector3Value;
        }

        if (EditorHelper.BoolField("Clear Target On Stop", "Determine if we clear the target once it's reached.", mTarget.ClearTargetOnStop, mTarget))
        {
            mIsDirty = true;
            mTarget.ClearTargetOnStop = EditorHelper.FieldBoolValue;
        }

        GUILayout.Space(5);

        if (EditorHelper.FloatField("Stop Distance", "Range in which we consider ourselves as arriving.", mTarget.StopDistance, mTarget))
        {
            mIsDirty = true;
            mTarget.StopDistance = EditorHelper.FieldFloatValue;
        }

        EditorGUILayout.BeginHorizontal();

        if (EditorHelper.FloatField("Slow Distance", "Range in which start slowing down.", mTarget.SlowDistance, mTarget))
        {
            mIsDirty = true;
            mTarget.SlowDistance = EditorHelper.FieldFloatValue;
        }

        GUILayout.Space(5);

        if (EditorHelper.FloatField(mTarget.SlowFactor, "Slow Speed", mTarget, 45))
        {
            mIsDirty = true;
            mTarget.SlowFactor = EditorHelper.FieldFloatValue;
        }

        GUILayout.FlexibleSpace();

        EditorGUILayout.EndHorizontal();

        GUILayout.Space(5);

        if (EditorHelper.FloatField("Path Height", "Nav Mesh height that is added by Unity. This allows us to fix our height.", mTarget.PathHeight, mTarget))
        {
            mIsDirty = true;
            mTarget.PathHeight = EditorHelper.FieldFloatValue;
        }

        GUILayout.Space(5);

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

            // Clear out the dirty flag
            mIsDirty = false;
        }
    }
}
