using UnityEngine;
using UnityEditor;
using com.ootii.Cameras;
using com.ootii.Helpers;
using com.ootii.Input;

[CanEditMultipleObjects]
[CustomEditor(typeof(FlyRig))]
public class FlyRigEditor : Editor
{
    // Helps us keep track of when the list needs to be saved. This
    // is important since some changes happen in scene.
    private bool mIsDirty;

    // The actual class we're storing
    private FlyRig mTarget;
    private SerializedObject mTargetSO;

    /// <summary>
    /// Called when the object is selected in the editor
    /// </summary>
    private void OnEnable()
    {
        // Grab the serialized objects
        mTarget = (FlyRig)target;
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

        EditorHelper.DrawInspectorTitle("ootii Fly Rig");

        EditorHelper.DrawInspectorDescription("Basic camera rig used primarily for testing. It allows the camera to fly and rotate.", MessageType.None);

        GUILayout.Space(5);

        GameObject lNewInputSourceOwner = EditorHelper.InterfaceOwnerField<IInputSource>(new GUIContent("Input Source", ""), mTarget.InputSourceOwner, true);
        if (lNewInputSourceOwner != mTarget.InputSourceOwner)
        {
            mIsDirty = true;
            mTarget.InputSourceOwner = lNewInputSourceOwner;
        }

        GUILayout.Space(5);

        float lNewMoveSpeed = EditorGUILayout.FloatField(new GUIContent("Move Speed", "Units per second to move."), mTarget.MoveSpeed);
        if (lNewMoveSpeed != mTarget.MoveSpeed)
        {
            mIsDirty = true;
            mTarget.MoveSpeed = lNewMoveSpeed;
        }

        float lNewFastFactor = EditorGUILayout.FloatField(new GUIContent("Fast Factor", "Multiplier when moving fast (left shift)."), mTarget.FastFactor);
        if (lNewFastFactor != mTarget.FastFactor)
        {
            mIsDirty = true;
            mTarget.FastFactor = lNewFastFactor;
        }

        float lNewSlowFactor = EditorGUILayout.FloatField(new GUIContent("Slow Factor", "Multiplier when moving slow (space)."), mTarget.SlowFactor);
        if (lNewSlowFactor != mTarget.SlowFactor)
        {
            mIsDirty = true;
            mTarget.SlowFactor = lNewSlowFactor;
        }

        float lNewScrollFactor = EditorGUILayout.FloatField(new GUIContent("Scroll Factor", "Multiplier when scrolling with the mouse wheel."), mTarget.ScrollFactor);
        if (lNewScrollFactor != mTarget.ScrollFactor)
        {
            mIsDirty = true;
            mTarget.ScrollFactor = lNewScrollFactor;
        }

        GUILayout.Space(5);

        bool lNewInvertPitch = EditorGUILayout.Toggle(new GUIContent("Invert Pitch", "Determines if we invert the mouse pitch."), mTarget.InvertPitch);
        if (lNewInvertPitch != mTarget.InvertPitch)
        {
            mIsDirty = true;
            mTarget.InvertPitch = lNewInvertPitch;
        }

        float lNewRotationSpeed = EditorGUILayout.FloatField(new GUIContent("Rotation Speed", "Degrees per second the camera rotates."), mTarget.RotationSpeed);
        if (lNewRotationSpeed != mTarget.RotationSpeed)
        {
            mIsDirty = true;
            mTarget.RotationSpeed = lNewRotationSpeed;
        }

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
