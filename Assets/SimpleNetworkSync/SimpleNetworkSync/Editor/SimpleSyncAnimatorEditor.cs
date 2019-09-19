// Copyright 2019, Davin Carten, All rights reserved
// This code may only be used in game development, but may not be used in any tools or assets that are sold or made publicly available to other developers.


using UnityEngine;
using System.Collections.Generic;

using emotitron.Compression;
using emotitron.Debugging;
using emotitron.Utilities;
using emotitron.Utilities.Networking;
using emotitron.Utilities.SmartVars;
using emotitron.SyncAnimInternal;
using System.Collections;

#if PUN_2_OR_NEWER
using Photon.Pun;
#elif MIRROR
using Mirror;
#elif ENABLE_UNET
using UnityEngine.Networking;
#endif

using UnityEditor;

namespace emotitron.Networking
{

	[CustomEditor(typeof(SimpleSyncAnimator))]
	[CanEditMultipleObjects]
	public class SimpleSyncAnimatorEditor : SyncObjectTFrameBase
	{
		
		SimpleSyncAnimator t;
		Animator a;

		float qtrwidth;

		private static readonly GUIContent passthruLabel = new GUIContent(
			"Sync Pass Thru Methods",
			"'this.SetTrigger()', 'this.Play()', 'this.CrossFadeInFixedTime()', etc methods are provided by this class, and will pass through the same named commands to the Animator. " +
			"When enabled, these methods will be sent and triggered over the network. Disabing this allows these methods to act as if they were called directly on the Animator without any networking, which is convenient for testing.");

		private static readonly GUIContent statesLabel = new GUIContent(
			"Sync States",
			"When enabled, changes in the animator current state are transmitted.");

		private static readonly GUIContent label_syncTrans = new GUIContent(
			"Sync Transitions",
			"Transition syncs are included.");

		private static readonly GUIContent weightsLabel = new GUIContent(
			"Sync Layer Weights",
			"When enabled, changes in the layer weights are transmitted.");

		private static readonly GUIContent paramsLabel = new GUIContent(
			"Sync Parameters",
			"When enabled, animator parameters will be networked and synced.");

		//private static readonly GUIContent label_indexedStateNames = new GUIContent(
		//	"Indexed State Names",
		//	"Any names added here will be indexed, and that index will be used for Trigger and Crossfade RPCs rather than 32 bit hashes.");

		private static readonly GUIContent label_syncLayers = new GUIContent(
			"Sync Layers",
			"State syncs for all layers, rather than just the root layer.");

		
		private static readonly GUIContent label_Interp = new GUIContent(
			"Interp",
			"Interpolation enables lerping(tweening) of values on clients between network updates.");

		private static readonly GUIContent label_Extrap = new GUIContent(
			"Extrap",
			"Extrapolation replicates previous values if new values from the network fail to arrive in time. When disabled, values default to default value for that parameter as defined in the Animator");

		private static readonly GUIContent label_Default = new GUIContent(
			"Defs",
			"Default value used for initial values and extrapolation.");

		private static readonly GUIContent index_Default = new GUIContent(
			"Index Animator Names",
			"Polls Animator Controller for all State, Trigger and Transition hashes. Indexed values require a tiny fraction of bandwidth to send compared to raw 32 hashes. This happens often automatically, but it never hurts to press this button after making changes to your Animator Controller.");

		public override void OnEnable()
		{
			base.OnEnable();
			textTexture = (Texture2D)Resources.Load<Texture2D>("Header/EditorHeaderAnimator");

			t = (SimpleSyncAnimator)target;
			if (t)
				a = t.GetComponent<Animator>();

			t.RebuildIndexedNames();

		}

		private bool showSummary;
		
		public override void OnInspectorGUI()
		{
			//halfwidth = EditorGUIUtility.currentViewWidth * .5f;
			qtrwidth = EditorGUIUtility.currentViewWidth * .25f;
			//quatwidth = EditorGUIUtility.currentViewWidth * .25f;

			base.OnInspectorGUI();

			serializedObject.Update();
			EditorGUI.BeginChangeCheck();

			EditorGUILayout.Space();



			Rect r = EditorGUILayout.GetControlRect();
			if (GUI.Button(r, index_Default))
			{
				t.RebuildIndexedNames();
				showSummary = true;
			}

			r = GetIndentedControlRect(12);
			showSummary = EditorGUI.Foldout(r, showSummary, "Indexed Name Summary", (GUIStyle)"Foldout");
			if (showSummary)
			{
				sb.Length = 0;
				sb.Append((uint)t.sharedTriggIndexes.Count).Append(" Triggers found.\n")
					.Append(FloatCrusher.GetBitsForMaxValue((uint)t.sharedTriggIndexes.Count - 1) + 1).Append(" bits per indexed Trigger.\n33 bits per non-indexed Trigger.\n\n");

				sb.Append((uint)t.sharedStateIndexes.Count).Append(" States found.\n")
					.Append(FloatCrusher.GetBitsForMaxValue((uint)t.sharedStateIndexes.Count - 1) + 1).Append(" bits per indexed State.\n33 bits per non-indexed State.\n\n");

				sb.Append((uint)t.sharedTransIndexes.Count).Append(" Transitions found.\n")
					.Append(FloatCrusher.GetBitsForMaxValue((uint)t.sharedTransIndexes.Count - 1) + 1).Append(" bits per indexed Transition.\n33 bits per non-indexed Transitions.");
				EditorGUILayout.HelpBox(sb.ToString(), MessageType.None);
			}
			EditorGUILayout.HelpBox("Network Animator commands with:\nthis.Play()\nthis.CrossfadeFixedTime()\nthis.SetTrigger()", MessageType.None);

			Divider();

			/// Passthrus
			t.syncPassThrus = EditorGUILayout.BeginToggleGroup(passthruLabel, t.syncPassThrus);
			EditorGUILayout.EndToggleGroup();

			Divider();

			/// States
			t.syncStates = EditorGUILayout.BeginToggleGroup(statesLabel, t.syncStates);
			if (t.syncStates)
				StatesSection();
			EditorGUILayout.EndToggleGroup();

			Divider();
			
			/// Parameters
			t.syncParams = EditorGUILayout.BeginToggleGroup(paramsLabel, t.syncParams);
			if (t.syncParams)
				ParamSection();
			EditorGUILayout.EndToggleGroup();

			EditorGUILayout.Space();

			if (EditorGUI.EndChangeCheck())
				serializedObject.ApplyModifiedProperties();
		}

		private void StatesSection()
		{
			MiniToggle(EditorGUILayout.GetControlRect(), label_syncTrans, ref t.syncTransitions);
			MiniToggle(EditorGUILayout.GetControlRect(), label_syncLayers, ref t.syncLayers);
			if (t.syncLayers)
			{
				MiniToggle(EditorGUILayout.GetControlRect(), weightsLabel, ref t.syncLayerWeights);
				//if (t.syncLayerWeights)
				//	EditorGUILayout.PropertyField(serializedObject.FindProperty("layerWeightCrusher"));
			}
		}


#region Parameters

		private void ParamSection()
		{
			t.useGlobalParamSettings = EditorGUILayout.ToggleLeft("Use Global Settings", t.useGlobalParamSettings);

			var r = EditorGUILayout.GetControlRect();

			r.xMin += qtrwidth;
			r.width = qtrwidth;
			EditorGUI.LabelField(r, label_Interp);

			r.xMin += qtrwidth;
			r.width = qtrwidth;
			EditorGUI.LabelField(r, label_Extrap);

			r.xMin += qtrwidth;
			r.width = qtrwidth;
			EditorGUI.LabelField(r, label_Default);


			if (t.useGlobalParamSettings)
			{
				r = ColLabel("Integers", ref t.sharedParamDefaults.includeInts);
				ColInterp(r, AnimatorControllerParameterType.Int, ref t.sharedParamDefaults.interpolateInts);
				ColExtrap(r, AnimatorControllerParameterType.Int, ref t.sharedParamDefaults.extrapolateInts);
				ColDefaults(r, AnimatorControllerParameterType.Int, ref t.sharedParamDefaults.defaultInt);

				r = ColLabel("Floats", ref t.sharedParamDefaults.includeFloats);
				ColInterp(r, AnimatorControllerParameterType.Float, ref t.sharedParamDefaults.interpolateFloats);
				ColExtrap(r, AnimatorControllerParameterType.Float, ref t.sharedParamDefaults.extrapolateFloats);
				ColDefaults(r, AnimatorControllerParameterType.Float, ref t.sharedParamDefaults.defaultFloat);

				r = ColLabel("Bools", ref t.sharedParamDefaults.includeBools);
				ColExtrap(r, AnimatorControllerParameterType.Bool, ref t.sharedParamDefaults.extrapolateBools);
				ColDefaults(r, AnimatorControllerParameterType.Bool, ref t.sharedParamDefaults.defaultBool);

				r = ColLabel("Triggers", ref t.sharedParamDefaults.includeTriggers);
				ColExtrap(r, AnimatorControllerParameterType.Trigger, ref t.sharedParamDefaults.extrapolateTriggers);
				ColDefaults(r, AnimatorControllerParameterType.Trigger, ref t.sharedParamDefaults.defaultTrigger);
			}
			else
			{
				var names = ParameterSettings.RebuildParamSettings(a, ref t.sharedParamSettings, ref t.paramCount, t.sharedParamDefaults);

				var pms = t.sharedParamSettings;
				for (int i = 0; i < t.paramCount; ++i)
				{
					var pm = pms[i];
					r = ColLabel(names[i], ref pm.include);
					ColInterp(r, pm.paramType, ref pm.interpolate);
					ColExtrap(r, pm.paramType, ref pm.extrapolate);
					ColDefaults(r, pm.paramType, ref pm.defaultValue);
				}
			}
		}

		private Rect ColLabel(string label, ref bool use)
		{
			Rect r = ColLabel(label, false);
			Rect toggleR = r;
			toggleR.width = 16;
			use = GUI.Toggle(toggleR, use, "", (GUIStyle)"OL Toggle");

			return r;
		}
		private Rect ColLabel(string label, bool lockedOn = true)
		{
			Rect r = EditorGUILayout.GetControlRect();
			r.xMin -= 2;
			Rect labelrect = new Rect(r.xMin + 16, r.yMin, r.width, r.height);
			EditorGUI.LabelField(labelrect, label, (GUIStyle)"MiniLabel");

			if (lockedOn)
			{
				EditorGUI.BeginDisabledGroup(true);
				GUI.Toggle(r, true, "", (GUIStyle)"OL Toggle");
				EditorGUI.EndDisabledGroup();
			}
			return r;
		}
		private void ColInterp(Rect r, AnimatorControllerParameterType type, ref ParameterInterpolation i)
		{
			if (type == AnimatorControllerParameterType.Bool || type == AnimatorControllerParameterType.Trigger)
				return;

			r.xMin += qtrwidth;
			r.width = qtrwidth;
			i = (ParameterInterpolation)EditorGUI.EnumPopup(r, "", i, (GUIStyle)"MiniPopup");
		}

		private void ColExtrap(Rect r, AnimatorControllerParameterType type, ref ParameterExtrapolation e)
		{
			r.xMin += qtrwidth * 2;
			r.width = qtrwidth;
			if (type == AnimatorControllerParameterType.Bool || type == AnimatorControllerParameterType.Trigger)
				e = (ParameterExtrapolation)EditorGUI.EnumPopup(r, "", (ParameterMissingHold)e, (GUIStyle)"MiniPopup");
			else
				e = (ParameterExtrapolation)EditorGUI.EnumPopup(r, "", e, (GUIStyle)"MiniPopup");
		}

		private void ColDefaults(Rect r, AnimatorControllerParameterType type, ref SmartVar v)
		{
			r.xMin += qtrwidth * 3;
			r.width = qtrwidth - 32;

			if (type == AnimatorControllerParameterType.Float)
			{
				v = EditorGUI.FloatField(r, v.Float);
			}
			else if (type == AnimatorControllerParameterType.Int)
			{
				v = EditorGUI.IntField(r, v.Int);
			}
			else
			{
				v = EditorGUI.Toggle(r, v.Bool);
			}
		}

		#endregion



		#region Utilities

		//private void KnownNameEditor(ref List<string> names)
		//{
		//	{
		//		int bttnwidth = 16;
		//		Rect r;

		//		for (int i = 0; i < names.Count; ++i)
		//		{
		//			r = EditorGUILayout.GetControlRect();
		//			names[i] = EditorGUI.TextField(new Rect(r.xMin, r.yMin, (r.width - bttnwidth) * .66f, r.height), names[i]);
		//			EditorGUI.LabelField(new Rect(r.xMin + (r.width - bttnwidth) * .66f, r.yMin, (r.width - bttnwidth) * .33f, r.height), Animator.StringToHash(names[i]).ToString());
		//			if (GUI.Button(new Rect(r.xMax - bttnwidth, r.yMin, bttnwidth, r.height), "X"))
		//			{
		//				names.RemoveAt(i);
		//				break;
		//			}
		//		}
		//		r = EditorGUILayout.GetControlRect();
		//		if (GUI.Button(r, "Add Name"))
		//			names.Add("ENTER NAME");
		//	}
		//}

		/// <summary>
		/// Draw left mini-toggle.
		/// </summary>
		/// <returns>Returns if toggle has changed.</returns>
		private bool MiniToggle(Rect r, GUIContent label, ref bool b, bool lockedOn = false)
		{
			EditorGUI.LabelField(new Rect(r.xMin + 16, r.yMin, r.width - 16, r.height), label, (GUIStyle)"MiniLabel");

			if (lockedOn)
				EditorGUI.BeginDisabledGroup(true);

			bool newb = GUI.Toggle(new Rect(r.xMin, r.yMin, 32, r.height), b, "", (GUIStyle)"OL Toggle");

			if (lockedOn)
				EditorGUI.EndDisabledGroup();

			bool haschanged = b != newb;
			if (haschanged)
			{
				Undo.RecordObject(t, "Sync Animator Toggle");
				b = newb;
			}

			return haschanged;
		}

		private Rect GetIndentedControlRect(int indent)
		{
			Rect r = EditorGUILayout.GetControlRect();
			r.xMin = r.xMin + indent;
			r.width = r.width - indent;
			return r;
		}

#endregion
	}



}

