//Copyright 2019, Davin Carten, All rights reserved

using UnityEngine;
using emotitron.Compression;

using emotitron.Utilities.Networking;
using emotitron.Utilities.CallbackUtils;
using System;
using System.Collections;
using System.Collections.Generic;

#if UNITY_EDITOR
using emotitron.Utilities;
using UnityEditor;
#endif

namespace emotitron.Networking
{
	public class NetMasterLite : MonoBehaviour
	{
		public static NetMasterLite single;

		/// Developer configurables
		public const int NET_FRAME_COUNT = 60;
		public const int NET_FRAME_BITS = 6;

		[Tooltip("Simple Network Sync components all run on FixedUpdate as the simulation. However you may want a network rate that is less than the physics rate. " +
			"You can reduce the rate with this value. A value one 1 will send an update every FixedUpdate. A value of 3 will send every 3rd.")]
		[Range(1, 6)]
		public int _sendEveryX = 1;
		public static int sendEveryX = 1;

		float lastFixedDeltaTimeTick;

		#region Properties

		private static int _currFrameId, _currSubFrameId, _prevFrameId, _prevSubFrameId;
		public static int CurrentFrameId { get { return _currFrameId; } }
		public static int CurrentSubFrameId { get { return _currSubFrameId; } }
		public static int PreviousFrameId { get { return _prevFrameId; } }
		public static int PreviousSubFrameId { get { return _prevSubFrameId; } }

		#endregion

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
#if UNITY_EDITOR
		[MenuItem("Simple Network Sync/Add Net Master")]
#endif
		public static void EnsureExistsInScene()
		{
			if (!single)
			{
				GameObject go;
				single = FindObjectOfType<NetMasterLite>();
				if (single)
				{
					go = single.gameObject;
				}
				else
				{
					go = new GameObject("Net Master");
					single = go.AddComponent<NetMasterLite>();
				}

				if (Application.isPlaying)
				{
					DontDestroyOnLoad(go);
					NetMsgCallbacks.RegisterCallback(NetObjectLite.Deserialize);
				}

				if (single)
					sendEveryX = single._sendEveryX;

#if UNITY_EDITOR
				if (!Application.isPlaying && go)
					EditorGUIUtility.PingObject(go);
#endif
			}
		}

		#region NetMaster callbacks

		public interface IOnIncrementFrame { void OnIncrementFrame(int newFrameId, int newSubFrameId, int previousFrameId, int prevSubFrameId); }
		public static List<IOnIncrementFrame> iOnIncrementFrame = new List<IOnIncrementFrame>();
		public static void OnIncrementFrameTrigger()
		{
			int cnt = iOnIncrementFrame.Count;
			for (int i = 0; i < iOnPreSimulate.Count; ++i)
				iOnIncrementFrame[i].OnIncrementFrame(_currFrameId, _currSubFrameId, _prevFrameId, _prevSubFrameId);
		}

		public interface IOnPreSimulate { void OnPreSimulate(int frameId, int subFrameId); }
		public static List<IOnPreSimulate> iOnPreSimulate = new List<IOnPreSimulate>();
		public static void OnPreSimulateTrigger()
		{
			int cnt = iOnPreSimulate.Count;
			for (int i = 0; i < iOnPreSimulate.Count; ++i)
				iOnPreSimulate[i].OnPreSimulate(_currFrameId, _currSubFrameId);
		}

		public interface IOnPostSimulate { void OnPostSimulate(int frameId, int subFrameId); }
		public static List<IOnPostSimulate> iOnPostSimulate = new List<IOnPostSimulate>();
		public static void OnPostSimulateTrigger()
		{
			int cnt = iOnPostSimulate.Count;
			for (int i = 0; i < iOnPostSimulate.Count; ++i)
				iOnPostSimulate[i].OnPostSimulate(_currFrameId, _currSubFrameId);
		}

		public static void RegisterCallbackInterfaces(object c, bool register = true)
		{
			CallbackUtilities.RegisterInterface(iOnPreSimulate, c, register);
			CallbackUtilities.RegisterInterface(iOnPostSimulate, c, register);
		}

		#endregion

		private void Awake()
		{
			if (single && single != this)
			{
				/// If a singleton already exists, destroy the new one, but copy over its SendEveryX setting, so that new scenes have the ability to change this setting.
				single._sendEveryX = this._sendEveryX;
				sendEveryX = this._sendEveryX;
				Destroy(this);
				return;
			}
			sendEveryX = _sendEveryX;
			_prevFrameId = NET_FRAME_COUNT - 1;
			_prevSubFrameId = sendEveryX - 1;
		}

	private bool simulationHasRun = false;

		private void FixedUpdate()
		{
#if !PUN_2_OR_NEWER
			/// Brute force ensure that handlers have registered before any incoming messages start arriving (Thanks UNET)
			NetMsgCallbacks.RegisterDefaultHandler();
#endif
			/// Halt everything if networking isn't ready.
			if (!NetMsgSends.ReadyToSend)
				return;

			if (simulationHasRun)
				PostSimulate();

			OnPreSimulateTrigger();

			simulationHasRun = true;
		}

		void Update()
		{
#if !PUN_2_OR_NEWER
			/// Brute force ensure that handlers have registered before any incoming messages start arriving (Thanks UNET)
			NetMsgCallbacks.RegisterDefaultHandler();
#endif

			if (simulationHasRun)
				PostSimulate();

			NetObjectLite.InterpolateAllNetObjs((Time.time - lastFixedDeltaTimeTick) / (Time.fixedDeltaTime * sendEveryX));
		}

		void PostSimulate()
		{
			OnPostSimulateTrigger();

			if (_currSubFrameId == 0)
				SerializeAndSend();

			IncrementFrameId();

			simulationHasRun = false;
		}

		private void SerializeAndSend()
		{
			NetObjectLite.SerializeAllNetObjs(_currFrameId);
		}

		private void IncrementFrameId()
		{

			_prevSubFrameId = _currSubFrameId;
			_currSubFrameId++;
			if (_currSubFrameId >= sendEveryX)
			{
				_currSubFrameId = 0;
				_prevFrameId = _currFrameId;

				_currFrameId++;
				if (_currFrameId >= NET_FRAME_COUNT)
					_currFrameId = 0;

				NetObjectLite.SnapshotAllNetObjs(_currFrameId);

				lastFixedDeltaTimeTick = Time.fixedTime;
			}

			OnIncrementFrameTrigger();

		}
	}

#if UNITY_EDITOR
	[CustomEditor(typeof(NetMasterLite))]
	public class NetMasterLiteEditor : HeaderEditorBase
	{
		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();
			EditorGUILayout.HelpBox("Timing singleton used by all Simple Network Sync components. Effectively a tiny networking specific Update Manager. Will be added automatically at runtime if one does not exist in your scene.", MessageType.None);
		}
	}

#endif
}

