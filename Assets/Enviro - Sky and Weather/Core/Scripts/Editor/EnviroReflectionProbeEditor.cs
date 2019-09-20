using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;


[CustomEditor(typeof(EnviroReflectionProbe))]
public class EnviroReflectionProbeEditor : Editor {

	GUIStyle boxStyle;
	GUIStyle boxStyle2;
	GUIStyle wrapStyle;
    GUIStyle wrapStyle2;
    GUIStyle clearStyle;

    EnviroReflectionProbe myTarget;

	public bool showAudio = false;
	public bool showFog = false;
	public bool showSeason = false;
	public bool showClouds = false;
	public bool showGeneral = false;
    public bool showPostProcessing = false;
    public bool showThirdParty = false;

    SerializedObject serializedObj;
#if ENVIRO_HD
    SerializedProperty customCloudsQuality;
#endif

    void OnEnable()
	{
		myTarget = (EnviroReflectionProbe)target;

		serializedObj = new SerializedObject (myTarget);
#if ENVIRO_HD
        customCloudsQuality = serializedObj.FindProperty("customCloudsQuality");
#endif
    }

    public override void OnInspectorGUI ()
	{

		myTarget = (EnviroReflectionProbe)target;
		#if UNITY_5_6_OR_NEWER
		serializedObj.UpdateIfRequiredOrScript ();
		#else
		serializedObj.UpdateIfDirtyOrScript ();
		#endif
		//Set up the box style
		if (boxStyle == null)
		{
			boxStyle = new GUIStyle(GUI.skin.box);
			boxStyle.normal.textColor = GUI.skin.label.normal.textColor;
			boxStyle.fontStyle = FontStyle.Bold;
			boxStyle.alignment = TextAnchor.UpperLeft;
		}

		if (boxStyle2 == null)
		{
			boxStyle2 = new GUIStyle(GUI.skin.label);
			boxStyle2.normal.textColor = GUI.skin.label.normal.textColor;
			boxStyle2.fontStyle = FontStyle.Bold;
			boxStyle2.alignment = TextAnchor.UpperLeft;
		}

		//Setup the wrap style
		if (wrapStyle == null)
		{
			wrapStyle = new GUIStyle(GUI.skin.label);
			wrapStyle.fontStyle = FontStyle.Bold;
			wrapStyle.wordWrap = true;
		}

        if (wrapStyle2 == null)
        {
            wrapStyle2 = new GUIStyle(GUI.skin.label);
            wrapStyle2.fontStyle = FontStyle.Normal;
            wrapStyle2.wordWrap = true;
        }

        if (clearStyle == null) {
			clearStyle = new GUIStyle(GUI.skin.label);
			clearStyle.normal.textColor = GUI.skin.label.normal.textColor;
			clearStyle.fontStyle = FontStyle.Bold;
			clearStyle.alignment = TextAnchor.UpperRight;
		}


        GUILayout.BeginVertical(" Enviro - Reflection Probe", boxStyle);
        GUILayout.Space(30);
        GUILayout.BeginVertical("Information", boxStyle);
        GUILayout.Space(20);
        EditorGUILayout.LabelField("Use this component to update your realtime reflection probes with Enviro Sky. You also can enable the 'Custom Rendering' to have enviro effects in your reflection probes!", wrapStyle2);
        EditorGUILayout.LabelField("Please enable 'Standalone Probe' if you use this component on your own places reflection probes.", wrapStyle2);

      
        GUILayout.EndVertical();

        GUILayout.BeginVertical("Setup", boxStyle);
        GUILayout.Space(20);
        myTarget.standalone = EditorGUILayout.Toggle("Standalone Probe", myTarget.standalone);
       
        if (myTarget.standalone)
        {
            GUILayout.Space(10);
#if ENVIRO_HD
            GUILayout.BeginVertical("Enviro Effects Rendering", boxStyle);
            GUILayout.Space(20);
            myTarget.customRendering = EditorGUILayout.Toggle("Render Enviro Effects", myTarget.customRendering);

            if(myTarget.customRendering)
            {
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(customCloudsQuality, true, null);
                myTarget.useFog = EditorGUILayout.Toggle("Use Fog", myTarget.useFog);
                if (EditorGUI.EndChangeCheck())
                {
                    serializedObj.ApplyModifiedProperties();
                }
            }
            GUILayout.EndVertical();
#endif
        GUILayout.BeginVertical("Update Settings", boxStyle);
        GUILayout.Space(20);
        myTarget.reflectionsUpdateTreshhold = EditorGUILayout.FloatField("Update Treshold in GameTime Hours", myTarget.reflectionsUpdateTreshhold);
        if (myTarget.customRendering)
        {
            myTarget.useTimeSlicing = EditorGUILayout.Toggle("Use Time-Slicing", myTarget.useTimeSlicing);
        }
        GUILayout.EndVertical();
        }
        GUILayout.EndVertical();
        // END
        EditorGUILayout.EndVertical ();
		EditorUtility.SetDirty (target);
	}
}
