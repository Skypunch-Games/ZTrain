using UnityEngine;
using UnityEditor;
using com.ootii.Helpers;
using com.ootii.Networking;

#if UNITY5 || UNITY_2017 || UNITY_2018

[CanEditMultipleObjects]
[CustomEditor(typeof(BasicActorNetworkSync))]
public class BasicActorNetworkSyncEditor : Editor
{
    // Helps us keep track of when the list needs to be saved. This
    // is important since some changes happen in scene.
    private bool mIsDirty;

    // The actual class we're storing
    private BasicActorNetworkSync mTarget;
    private SerializedObject mTargetSO;

    /// <summary>
    /// Called when the object is selected in the editor
    /// </summary>
    private void OnEnable()
    {
        // Grab the serialized objects
        mTarget = (BasicActorNetworkSync)target;
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

        EditorHelper.DrawInspectorTitle("ootii Basic Network Sync (UNet)");

        EditorHelper.DrawInspectorDescription("Basic solution for synchronizing actors across a UNet network..", MessageType.None);

        EditorHelper.DrawInspectorDescription("This component is an EXAMPLE of how to use the AC and MC with Unet (or other networking solutions). It is very basic and has simple smoothing, no prection, etc.", MessageType.Warning);

        GUILayout.Space(5);
        
        if (EditorHelper.IntField("Network Send Rate", "Network updates to send per second.", mTarget.NetworkSendRate, mTarget))
        {
            mIsDirty = true;
            mTarget.NetworkSendRate = EditorHelper.FieldIntValue;
        }

        GUILayout.Space(5f);

        EditorGUILayout.LabelField("Transform", EditorStyles.boldLabel, GUILayout.Height(16f));
        GUILayout.BeginVertical(EditorHelper.Box);

        if (EditorHelper.BoolField("Sync Position", "Determines if we sync the position during network updates.", mTarget.SyncPosition, mTarget))
        {
            mIsDirty = true;
            mTarget.SyncPosition = EditorHelper.FieldBoolValue;
        }

        if (EditorHelper.BoolField("Sync Rotation", "Determines if we sync the rotation during network updates.", mTarget.SyncRotation, mTarget))
        {
            mIsDirty = true;
            mTarget.SyncRotation = EditorHelper.FieldBoolValue;
        }

        if (EditorHelper.FloatField("Sync Factor", "Lerp time for transform sync.", mTarget.SyncFactor, mTarget))
        {
            mIsDirty = true;
            mTarget.SyncFactor = EditorHelper.FieldFloatValue;
        }

        GUILayout.EndVertical();

        GUILayout.Space(5f);

        EditorGUILayout.LabelField("Animator Parameters", EditorStyles.boldLabel, GUILayout.Height(16f));
        GUILayout.BeginVertical(EditorHelper.Box);

        if (EditorHelper.BoolField("Sync Motion Phase", "Determines if we sync the motion phase during network updates.", mTarget.SyncMotionPhase, mTarget))
        {
            mIsDirty = true;
            mTarget.SyncMotionPhase = EditorHelper.FieldBoolValue;
        }

        if (EditorHelper.BoolField("Sync Parameters", "Determines if we sync the animator parameters during network updates.", mTarget.SyncAnimatorParams, mTarget))
        {
            mIsDirty = true;
            mTarget.SyncAnimatorParams = EditorHelper.FieldBoolValue;
        }

        if (EditorHelper.FloatField("Phase Change Delay", "Time in seconds before changing the motion phase.", mTarget.PhaseChangeDelay, mTarget))
        {
            mIsDirty = true;
            mTarget.PhaseChangeDelay = EditorHelper.FieldFloatValue;
        }

        GUILayout.EndVertical();

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

            // Clear out the dirty flag
            mIsDirty = false;
        }
    }
}

#endif
