// Copyright 2019, Davin Carten, All rights reserved
// This code may only be used in game development, but may not be used in any tools or assets that are sold or made publicly available to other developers.


using UnityEngine;
using System.Collections.Generic;

using emotitron.Compression;
using emotitron;
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

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Animations;
#endif

//#pragma warning disable CS0618 // Supress UNET warmings

namespace emotitron.Networking
{
	[DisallowMultipleComponent]
	public class SimpleSyncAnimator : SyncObject<SimpleSyncAnimator.Frame>,
		ISyncAnimator,
		IOnNetSerialize,
		IOnNetDeserialize,
		IOnSnapshot,
		IApplyOrder,
		IOnInterpolate,
		IOnCaptureCurrentValues
	{
		/// <summary>
		/// Hard coded compression settings for normalized 0-1 range floats
		/// </summary>
		private const int NORM_COMP_BITS = 10; // 12;
		private const float NORM_COMP_ENCODE = 1023; // 4095f;
		private const float NORM_COMP_DECODE = 1 / NORM_COMP_ENCODE;

		#region Inspector Items

		/// Triggers
		///*[HideInInspector]*/ public bool syncTriggers = true;
		///*[HideInInspector]*/ public bool syncCrossfades = true;
		///*[HideInInspector]*/ public bool syncPlays = true;



		#endregion

		#region Shared Cache


		private static Dictionary<int, Dictionary<int, int>> masterSharedTriggHashes = new Dictionary<int, Dictionary<int, int>>();
		private static Dictionary<int, List<int>> masterSharedTriggIndexes = new Dictionary<int, List<int>>();
		[HideInInspector] public List<int> sharedTriggIndexes = new List<int>();
		private Dictionary<int, int> sharedTriggHashes;

		private static Dictionary<int, Dictionary<int, int>> masterSharedStateHashes = new Dictionary<int, Dictionary<int, int>>();
		private static Dictionary<int, List<int>> masterSharedStateIndexes = new Dictionary<int, List<int>>();
		[HideInInspector] public List<int> sharedStateIndexes = new List<int>();
		private Dictionary<int, int> sharedStateHashes;

		private static Dictionary<int, Dictionary<int, TransitionInfo>> masterSharedTransHashes = new Dictionary<int, Dictionary<int, TransitionInfo>>();
		private static Dictionary<int, List<TransitionInfo>> masterSharedTransIndexes = new Dictionary<int, List<TransitionInfo>>();
		[HideInInspector] public List<TransitionInfo> sharedTransIndexes = new List<TransitionInfo>();
		private Dictionary<int, TransitionInfo> sharedTransHashes;

		#endregion

		[HideInInspector] public bool syncPassThrus = true;

		/// States
		[HideInInspector] public bool syncStates = true;
		[HideInInspector] public bool syncTransitions = false;
		[HideInInspector] public bool syncLayers = true;
		[HideInInspector] public bool syncLayerWeights = true;
		[System.NonSerialized] public int layerCount;

		/// Parameters
		[HideInInspector] public bool syncParams = true;
		[HideInInspector] public bool useGlobalParamSettings = true;

		private static Dictionary<int, ParameterDefaults> masterSharedParamDefaults = new Dictionary<int, ParameterDefaults>();
		[HideInInspector] public ParameterDefaults sharedParamDefaults = new ParameterDefaults();

		//[HideInInspector] public int[] paramNameHashes = new int[0];
		private static Dictionary<int, ParameterSettings[]> masterSharedParamSettings = new Dictionary<int, ParameterSettings[]>();
		[HideInInspector] public ParameterSettings[] sharedParamSettings = new ParameterSettings[0];
		[HideInInspector] public int paramCount;


		#region Local Cache

		/// cached stuff
		public Animator animator;
		private int bitsForTriggerIndex;
		private int bitsForStateIndex;
		private int bitsForTransIndex;
		private int bitsForLayerIndex;

		#endregion

		/// History checks
		private int[] lastAnimationHash;
		private uint[] lastLayerWeight;
		private SmartVar[] lastSentParams;

		#region Frame Struct/Queue/Pools

		public class Frame : FrameBase
		{
			public SmartVar[] parameters;
			public int?[] stateHashes;
			public bool[] layerIsInTransition;
			public float[] normalizedTime;
			public float[] layerWeights;
			public Queue<AnimPassThru> passThrus;

			public Frame(SimpleSyncAnimator syncAnimator, int frameId) : base(frameId)
			{
				int layerCount = syncAnimator.layerCount;

				stateHashes = new int?[layerCount];
				layerIsInTransition = new bool[layerCount];
				normalizedTime = new float[layerCount];
				layerWeights = new float[layerCount];
				passThrus = new Queue<AnimPassThru>(2);

				parameters = new SmartVar[syncAnimator.paramCount];
				int paramcnt = syncAnimator.paramCount;
				for (int pid = 0; pid < paramcnt; ++pid)
				{
					parameters[pid] = syncAnimator.sharedParamSettings[pid].defaultValue;
				}

			}

			public override bool Compare(FrameBase frame, FrameBase holdframe)
			{
				throw new System.NotImplementedException();
			}

			public override void CopyFrom(FrameBase sourceFrame)
			{
				Frame frame = sourceFrame as Frame;

				var ps = frame.parameters;
				int paramcnt = ps.Length;
				for (int i = 0; i < paramcnt; ++i)
					parameters[i] = ps[i];


				int lyrCnt = frame.stateHashes.Length;
				for (int i = 0; i < lyrCnt; ++i)
				{
					stateHashes[i] = frame.stateHashes[i];
					layerIsInTransition[i] = frame.layerIsInTransition[i];
					normalizedTime[i] = frame.normalizedTime[i];
					layerWeights[i] = frame.layerWeights[i];
				}

				/// Don't copy triggers - unless I decide otherwise. They are fire once and should not be repeated.
				//triggers = new Queue<TriggerItem>();
				//crossFades = new Queue<TriggerItem>();
			}

			public void Reset()
			{
				passThrus.Clear();

				int lyrCnt = stateHashes.Length;
				for (int i = 0; i < lyrCnt; ++i)
					stateHashes[i] = null;
			}
		}

		#endregion

		#region Unity Timings

#if UNITY_EDITOR

		const double AUTO_REBUILD_RATE = 10f;
		double lastReuildTime;

		/// <summary>
		/// Reindex all of the State and Trigger names in the current AnimatorController. Never hurts to run this (other than haning the editor for a split second).
		/// </summary>
		public void RebuildIndexedNames()
		{
			/// always get new Animator in case it has changed.
			if (animator == null)
				animator = GetComponent<Animator>();

			if (animator && EditorApplication.timeSinceStartup - lastReuildTime > AUTO_REBUILD_RATE)
			{
				//Debug.Log("REBUILD " + GetInstanceID());

				lastReuildTime = EditorApplication.timeSinceStartup;

				AnimatorController ac = animator.GetController();
				if (ac != null)
				{
					ac.GetTriggerNames(sharedTriggIndexes);
					ac.GetStatesNames(sharedStateIndexes);
					ac.GetTransitionNames(sharedTransIndexes);
				}

				EditorUtility.SetDirty(this);
			}
		}
#endif

		protected override void Reset()
		{
			base.Reset();
			/// Default TransformSync to happen before AnimatorSync
			_applyOrder = 5;

			if (animator == null)
				animator = GetComponent<Animator>();

		}

		protected override void Awake()
		{
			base.Awake();

			if (animator == null)
				animator = GetComponent<Animator>();

			Initialize();

			ConnectSharedCaches();

			//Debug.LogError(prefabInstanceId);
		}

		/// Start is just here to give devs the enabled checkbox
		private void Start()
		{

		}

		private void Initialize()
		{
			bitsForTriggerIndex = FloatCrusher.GetBitsForMaxValue((uint)sharedTriggIndexes.Count - 1);
			bitsForStateIndex = FloatCrusher.GetBitsForMaxValue((uint)sharedStateIndexes.Count - 1);
			bitsForTransIndex = FloatCrusher.GetBitsForMaxValue((uint)sharedTransIndexes.Count - 1);


#if UNITY_EDITOR || DEVELOPMENT_BUILD
			if (animator == null)
				Debug.LogError("No Animator component found. Be sure " + name + " has an animator, or remove " + GetType().Name + ".");
#endif
			paramCount = animator.parameters.Length;

			layerCount = animator.layerCount;
			/// we don't use (layercount - 1), because our actual range is -1 to X ... so the extra value is needed for the + 1 shift during serialization.
			bitsForLayerIndex = FloatCrusher.GetBitsForMaxValue((uint)(layerCount));

			lastSentParams = new SmartVar[paramCount];
			/// TODO: these will now have values, check them rather than replace them
			ParameterSettings.RebuildParamSettings(animator, ref sharedParamSettings, ref paramCount, sharedParamDefaults);

			lastAnimationHash = new int[layerCount];
			lastLayerWeight = new uint[layerCount];

			// Cache some of the readonly parameter attributes
			for (int pid = 0; pid < paramCount; ++pid)
			{
				/// Start our lastSent values to the default so our param tests don't require any special checks.
				lastSentParams[pid] = sharedParamSettings[pid].defaultValue; // ; paramDefValue[pid];
			}

			for (int i = 0; i < FRAME_CNT + 1; ++i)
				frames[i] = new Frame(this, i);

		}

		private void ConnectSharedCaches()
		{
			/// Connect sharedTrigger states
			if (!masterSharedTriggHashes.ContainsKey(prefabInstanceId))
			{
				sharedTriggHashes = new Dictionary<int, int>();
				for (int i = 0; i < sharedTriggIndexes.Count; ++i)
					if (sharedTriggHashes.ContainsKey(sharedTriggIndexes[i]))
					{
						Debug.LogError("There appear to be duplicate Trigger names in the animator controller on '" + name + "'. This will break " + GetType().Name + "'s ability to sync triggers.");
					}
					else
						sharedTriggHashes.Add(sharedTriggIndexes[i], i);

				masterSharedTriggHashes.Add(prefabInstanceId, sharedTriggHashes);
				masterSharedTriggIndexes.Add(prefabInstanceId, sharedTriggIndexes);
			}
			else
			{
				sharedTriggHashes = masterSharedTriggHashes[prefabInstanceId];
				sharedTriggIndexes = masterSharedTriggIndexes[prefabInstanceId];
			}

			/// Connect sharedStates
			if (!masterSharedStateHashes.ContainsKey(prefabInstanceId))
			{
				sharedStateHashes = new Dictionary<int, int>();
				for (int i = 0; i < sharedStateIndexes.Count; ++i)
					if (sharedStateHashes.ContainsKey(sharedStateIndexes[i]))
					{
						Debug.LogError("There appear to be duplicate State names in the animator controller on '" + name + "'. This will break " + GetType().Name + "'s ability to sync states.");
					}
					else
						sharedStateHashes.Add(sharedStateIndexes[i], i);

				masterSharedStateHashes.Add(prefabInstanceId, sharedStateHashes);
				masterSharedStateIndexes.Add(prefabInstanceId, sharedStateIndexes);
			}
			else
			{
				sharedStateHashes = masterSharedStateHashes[prefabInstanceId];
				sharedStateIndexes = masterSharedStateIndexes[prefabInstanceId];
			}


			/// Connect sharedTransitions
			if (!masterSharedTransHashes.ContainsKey(prefabInstanceId))
			{
				/// Create dictionary from List
				sharedTransHashes = new Dictionary<int, TransitionInfo>();
				for (int i = 0; i < sharedTransIndexes.Count; ++i)
					if (sharedTransHashes.ContainsKey(sharedTransIndexes[i].hash))
					{
						Debug.LogError("There appear to be duplicate State names in the animator controller on '" + name + "'. This will break " + GetType().Name + "'s ability to sync transitions.");
					}
					else
						sharedTransHashes.Add(sharedTransIndexes[i].hash, sharedTransIndexes[i]);

				/// make the local lookups the shared for this prefab id
				masterSharedTransHashes.Add(prefabInstanceId, sharedTransHashes);
				masterSharedTransIndexes.Add(prefabInstanceId, sharedTransIndexes);
			}
			else
			{
				sharedTransHashes = masterSharedTransHashes[prefabInstanceId];
				sharedTransIndexes = masterSharedTransIndexes[prefabInstanceId];
			}

			ParameterDefaults pd;
			if (masterSharedParamDefaults.TryGetValue(prefabInstanceId, out pd))
				sharedParamDefaults = pd;
			else
				masterSharedParamDefaults.Add(prefabInstanceId, sharedParamDefaults);

			ParameterSettings[] ps;
			if (masterSharedParamSettings.TryGetValue(prefabInstanceId, out ps))
				sharedParamSettings = ps;
			else
				masterSharedParamSettings.Add(prefabInstanceId, sharedParamSettings);
		}

		#endregion

		#region Net Serialization/Deserialization

		/// <summary>
		/// NetObjectLite serialize interface.
		/// </summary>
		public bool OnNetSerialize(int frameId, byte[] buffer, ref int bitposition)
		{

			/// Server side initialization is indicated by the -1
			if (frameId == -1)
			{
				OnCaptureCurrentValues(FRAME_CNT, true, Realm.Primary);
				Frame offtickFrame = frames[FRAME_CNT];

				/// Leading bool is for enabled, which we are ignoring.
				buffer.WriteBool(true, ref bitposition);
				WriteAllToBuffer(offtickFrame, buffer, ref bitposition, true);

				hasInitialization = true;
				return true;
			}

			//if (!na.IsMine)
			//	return false;

			/// Don't transmit data if this component is disabled. Allows for muting components
			/// Simply by disabling them at the authority side.
			if (enabled)
				buffer.WriteBool(true, ref bitposition);
			else
			{
				buffer.WriteBool(false, ref bitposition);
				return false;
			}

			bool isKeyframe = (keyframeRate != 0) && (frameId % keyframeRate) == 0; // IsKey(frameId);
			Frame frame = frames[frameId];

			return WriteAllToBuffer(frame, buffer, ref bitposition, isKeyframe);
		}

		/// <summary>
		/// NetObjectLite deserialize interface.
		/// </summary>
		public void OnNetDeserialize(int sourceFrameId, int originFrameId, int localFrameId, byte[] buffer, ref int bitposition)
		{
			bool isKeyframe = (keyframeRate != 0) && (originFrameId % keyframeRate == 0);

			/// initialization
			if (!hasInitialization || localFrameId == FRAME_CNT)
			{
				var offtickF = frames[localFrameId];

				/// Bool check for component being enabled.
				if (buffer.ReadBool(ref bitposition))
					ReadAllFromBuffer(offtickF, buffer, ref bitposition, isKeyframe);
				else
					offtickF.hasChanged = false;

				hasInitialization = true;
				return;
			}

			var frame = frames[localFrameId];

			/// If hascontent flag is false, we are done here.
			if (!buffer.ReadBool(ref bitposition))
			{
				frame.hasChanged = false;
				return;
			}

			frame.hasChanged = true;

			ReadAllFromBuffer(frame, buffer, ref bitposition, isKeyframe);

		}

		#endregion

		#region Buffer Writer/Readers

		public void OnCaptureCurrentValues(int frameId, bool amActingAuthority, Realm realm)
		{
			Frame frame = frames[frameId];
			CaptureParameters(frame);
			CapturePassThrus(frame);
			CaptureStates(frame);
		}

		private bool WriteAllToBuffer(Frame frame, byte[] buffer, ref int bitposition, bool isKeyframe)
		{
			/// Write Passthrough Trigger and Callback Events
			if (syncPassThrus)
				WritePassThrus(frame, buffer, ref bitposition, isKeyframe);

			if (syncParams)
				WriteParameters(frame, buffer, ref bitposition, isKeyframe);

			if (syncStates)
				WriteStates(frame, buffer, ref bitposition, isKeyframe);

			// Mark as always having content. Can revisit this later.
			return true;
		}

		private void ReadAllFromBuffer(Frame frame, byte[] buffer, ref int bitposition, bool isKeyframe)
		{
			if (syncPassThrus)
				ReadPassThrus(frame, buffer, ref bitposition, isKeyframe);

			if (syncParams)
				ReadParameters(frame, buffer, ref bitposition, isKeyframe);

			if (syncStates)
				ReadStates(frame, buffer, ref bitposition, isKeyframe);
		}

		#region Parameter Handling

		/// <summary>
		/// Serialize frame parameters into buffer
		/// </summary>
		private void WriteParameters(Frame frame, byte[] bitstream, ref int bitposition, bool isKeyframe)
		{
			var paramaters = frame.parameters;

			for (int pid = 0; pid < paramCount; ++pid)
			{
				var ps = sharedParamSettings[pid];

				if (!useGlobalParamSettings && !ps.include)
					continue;

				var type = ps.paramType;
				//int nameHash = paramNameHashes[pid];

				if (type == AnimatorControllerParameterType.Int)
				{
					int val = paramaters[pid];//  animator.GetInteger(nameHash);

					if (isKeyframe || val != lastSentParams[pid])
					{
						if (!isKeyframe)
							bitstream.WriteBool(true, ref bitposition);

						bitstream.WriteSigned(val, ref bitposition, 32);
						lastSentParams[pid] = val;
					}
					else
					{
						if (!isKeyframe)
							bitstream.WriteBool(false, ref bitposition);
					}
				}

				else if (type == AnimatorControllerParameterType.Float)
				{
					SmartVar val = HalfUtilities.Pack(paramaters[pid]);

					//Debug.Log("send " + val + " " + paramaters[pid] + " " + HalfUtilities.Unpack((ushort)val));

					if (isKeyframe || val.UInt != lastSentParams[pid].UInt)
					{

						//Debug.Log("<color=green>half changed</color>" + val + "/" + lastSentParams[pid]
						//		+ " " + isKeyframe + " " /*+ ((ushort)hf == (ushort)lastSentParams[pid])*/);

						if (!isKeyframe)
							bitstream.WriteBool(true, ref bitposition);

						bitstream.Write(val.UInt, ref bitposition, 16);
						//bitstream.WriteHalf(paramaters[pid], ref bitposition);
						lastSentParams[pid] = val;
					}
					else
					{
						//Debug.Log("<color=red>half no change</color>");
						if (!isKeyframe)
							bitstream.WriteBool(false, ref bitposition);
					}

				}

				else if (type == AnimatorControllerParameterType.Bool)
				{
					bool val = paramaters[pid]; // animator.GetBool(nameHash);

					bitstream.WriteBool(val, ref bitposition);
				}

				else if (type == AnimatorControllerParameterType.Trigger)
				{
					//if (!includeTriggers)
					//	continue;

					bool val = paramaters[pid];// animator.GetBool(nameHash);
					bitstream.WriteBool(val, ref bitposition);
				}
			}
		}

		private void CaptureParameters(Frame frame)
		{
			var paramaters = frame.parameters;

			for (int pid = 0; pid < paramCount; ++pid)
			{
				var ps = sharedParamSettings[pid];

				if (!useGlobalParamSettings && !ps.include)
					continue;

				var type = ps.paramType;
				int nameHash = ps.hash;

				switch (type)
				{
					case AnimatorControllerParameterType.Float:
						paramaters[pid] = animator.GetFloat(nameHash);
						break;

					case AnimatorControllerParameterType.Int:
						paramaters[pid] = animator.GetInteger(nameHash);
						break;

					case AnimatorControllerParameterType.Bool:
						paramaters[pid] = animator.GetBool(nameHash);
						break;

					case AnimatorControllerParameterType.Trigger:
						paramaters[pid] = animator.GetBool(nameHash);
						break;

					default:
						break;

				}
			}
		}

		private void ReadParameters(Frame frame, byte[] instream, ref int bitposition, bool isKeyframe)
		{
			SmartVar[] parms = frame.parameters;

			///  Less than ideal check to make sure we don't write None values over our extrapolated values
			///  if this comes in late and is already in use.
			bool frameIsInUse = (ReferenceEquals(frame, targF)) || (ReferenceEquals(frame, snapF));

			for (int pid = 0; pid < paramCount; ++pid)
			{
				var ps = sharedParamSettings[pid];

				if (!useGlobalParamSettings && !ps.include)
					continue;

				var type = ps.paramType;

				if (type == AnimatorControllerParameterType.Int)
				{
					bool used = isKeyframe ? true : instream.ReadBool(ref bitposition);

					if (used)
					{
						int val = instream.ReadSigned(ref bitposition, 32);
						parms[pid] = val;
					}
					else
					{
						if (!frameIsInUse)
							parms[pid] = SmartVar.None;
					}
				}

				else if (type == AnimatorControllerParameterType.Float)
				{
					bool used = isKeyframe ? true : instream.ReadBool(ref bitposition);

					if (used)
					{
						float val = instream.ReadHalf(ref bitposition);
						parms[pid] = val;

						//Debug.Log("RCV " + val);
					}
					else
					{
						if (!frameIsInUse)
							parms[pid] = SmartVar.None;
					}

				}

				/// Always include bools, since the mask of 1 needed to indicate included is the same size as just inccluding them.
				else if (type == AnimatorControllerParameterType.Bool)
				{
					bool val = instream.ReadBool(ref bitposition);
					parms[pid] = val;
				}

				/// Always include triggers, since the mask of 1 needed to indicate included is the same size as just inccluding them.
				else if (type == AnimatorControllerParameterType.Trigger)
				{
					bool val = instream.ReadBool(ref bitposition);
					parms[pid] = val;
				}
			}
		}

		/// <summary>
		/// Many parameters will be SmartVar.None if keyframes are used - meaning they were unchanged.
		/// This method completes the current TargF using values from PrevF
		/// </summary>
		private void CompleteTargetParameters()
		{
			SmartVar[] prevParams = (snapF != null) ? snapF.parameters : targF.parameters;
			SmartVar[] targParams = targF.parameters;
			/// if this smartvar is none, then this value was left out of the update - meaning it was a repeat.
			/// Copy the previous value.
			/// TODO: This should use the extrapolate setting eventually?
			/// TODO: This should be its own loop so it doesn't happen every interpolate? Call in SNapshot?
			for (int pid = 0; pid < paramCount; ++pid)
			{
				SmartVar prevParam = prevParams[pid];
				SmartVar targParam = targParams[pid];
				var psettings = sharedParamSettings[pid];

				if (prevParam.TypeCode == SmartVarTypeCode.None)
				{
					prevParam = psettings.defaultValue;
					prevParams[pid] = prevParam;
				}

				if (targParam.TypeCode == SmartVarTypeCode.None)
				{
					targParam = prevParam;
					targParams[pid] = prevParam;
				}
			}
		}

		/// <summary>
		/// t value of zero will skip any tests and processing for lerps
		/// </summary>
		/// <param name="t"></param>
		private void InterpolateParams(float t)
		{
			SmartVar[] prevParams =/* (snapF != null) ?*/ snapF.parameters /*: targF.parameters*/;
			SmartVar[] targParams = targF.parameters;

			for (int pid = 0; pid < paramCount; ++pid)
			{

				var psettings = sharedParamSettings[pid];
				int hash = psettings.hash;
				if (!useGlobalParamSettings && !psettings.include)
					continue;

				var type = psettings.paramType;

				SmartVar prevParam = prevParams[pid];
				SmartVar targParam = targParams[pid];

				switch (type)
				{
					case AnimatorControllerParameterType.Int:
						{
							if (sharedParamDefaults.includeInts == false)
								continue;

							/// zero t has no interpolation, so skip all the fancy checks and just apply prev
							if (t == 0)
							{
								animator.SetInteger(hash, prevParam);
								continue;
							}

							ParameterInterpolation interpmethod;

							if (useGlobalParamSettings)
								interpmethod = sharedParamDefaults.interpolateInts;
							else
								interpmethod = psettings.interpolate;

							int value =
								(interpmethod == ParameterInterpolation.Hold) ? (int)prevParam :
								(interpmethod == ParameterInterpolation.Advance) ? (int)targParam :
								(interpmethod == ParameterInterpolation.Lerp) ? (int)Mathf.Lerp(prevParam, targParam, t) :
								(int)psettings.defaultValue;

							animator.SetInteger(hash, value);

							break;
						}


					case AnimatorControllerParameterType.Float:
						{
							if (sharedParamDefaults.includeFloats == false)
								continue;


							if (t == 0)
							{
								animator.SetFloat(hash, prevParam);
								continue;
							}

							ParameterInterpolation interpmethod;
							if (useGlobalParamSettings)

								interpmethod = sharedParamDefaults.interpolateFloats;
							else
								interpmethod = psettings.interpolate;

							SmartVar value =
								(interpmethod == ParameterInterpolation.Hold) ? prevParam :
								(interpmethod == ParameterInterpolation.Lerp) ? (SmartVar)(Mathf.Lerp((float)prevParam, (float)targParam, t)) :
								(interpmethod == ParameterInterpolation.Advance) ? targParam :
								psettings.defaultValue;

							animator.SetFloat(hash, value);

							break;
						}


					case AnimatorControllerParameterType.Bool:
						{
							if (!sharedParamDefaults.includeBools)
								continue;

							animator.SetBool(hash, prevParam);
							break;
						}

					case AnimatorControllerParameterType.Trigger:
						{
							if (!sharedParamDefaults.includeTriggers)
								continue;

							if (prevParam)
								animator.SetTrigger(hash);
							break;
						}

					default:
						break;
				}
			}
		}


		/// <summary>
		/// Extrapolate values for tne missing target frame using the previuos. Todo: Use the last two to get a curve.
		/// </summary>
		/// <param name="snap_params"></param>
		/// <param name="targ_params"></param>
		private void ExtrapolateParams(Frame prev, Frame targ, Frame newtarg)
		{
			if (ReferenceEquals(prev, null))
				return;

			var prev_params = prev.parameters;
			var targ_params = targ.parameters;

			/// if next frame from the buffer isn't flagged as valid, it hasn't arrived - Extrapolate
			for (int pid = 0; pid < paramCount; ++pid)
			{
				var ps = sharedParamSettings[pid];
				var type = ps.paramType;

				if (!useGlobalParamSettings && !ps.include)
					continue;

				/// TODO: Actually wire up the Lerps?
				/// TODO: Make this Switch

				// Float lerps back toward default value on lost frames as a loss handling compromise currently.
				if (type == AnimatorControllerParameterType.Float)
				{
					var extrapmethod = (useGlobalParamSettings) ? sharedParamDefaults.extrapolateFloats : ps.extrapolate;

					newtarg.parameters[pid] =
						(extrapmethod == ParameterExtrapolation.Hold) ? targ_params[pid] :
						(extrapmethod == ParameterExtrapolation.Lerp) ? (SmartVar)(targ_params[pid] + ((float)targ_params[pid] - prev_params[pid])) :
						ps.defaultValue;
				}

				else if (type == AnimatorControllerParameterType.Int)
				{
					var extrapmethod = (useGlobalParamSettings) ? sharedParamDefaults.extrapolateInts : ps.extrapolate;

					newtarg.parameters[pid] =
						(extrapmethod == ParameterExtrapolation.Hold) ? targ_params[pid] :
						(extrapmethod == ParameterExtrapolation.Lerp) ? (SmartVar)(targ_params[pid] + (targ_params[pid] - prev_params[pid])) :
						ps.defaultValue;
				}

				else if (type == AnimatorControllerParameterType.Bool)
				{
					var extrapmethod = (useGlobalParamSettings) ? sharedParamDefaults.extrapolateBools : ps.extrapolate;

					newtarg.parameters[pid] =
						(extrapmethod == ParameterExtrapolation.Hold) ? targ_params[pid] : ps.defaultValue;
				}

				/// TODO: this is unfinished
				else /*if (includeTriggers)*/
				{
					var extrapmethod = (useGlobalParamSettings) ? sharedParamDefaults.extrapolateTriggers : ps.extrapolate;

					newtarg.parameters[pid] =
						(extrapmethod == ParameterExtrapolation.Hold) ? targ_params[pid] : ps.defaultValue;
				}
			}
		}


		#endregion

		#region Passhthru Calls

		private readonly Queue<AnimPassThru> passThruQueue = new Queue<AnimPassThru>(2);

		public void SetTrigger(string triggerName, LocalApplyTiming localApplyTiming = LocalApplyTiming.OnSend)
		{
			int hash = Animator.StringToHash(triggerName);
			SetTrigger(hash, localApplyTiming);
		}
		public void SetTrigger(int hash, LocalApplyTiming localApplyTiming = LocalApplyTiming.OnSend)
		{
			passThruQueue.Enqueue(new AnimPassThru(PassThruType.Trigger, hash, -1, localApplyTiming));

			if (localApplyTiming == LocalApplyTiming.Immediately || !syncPassThrus)
				animator.SetTrigger(hash);
		}

		public void ResetTrigger(string triggerName, LocalApplyTiming localApplyTiming = LocalApplyTiming.OnSend)
		{
			int hash = Animator.StringToHash(triggerName);
			ResetTrigger(hash, localApplyTiming);
		}
		public void ResetTrigger(int hash, LocalApplyTiming localApplyTiming = LocalApplyTiming.OnSend)
		{
			passThruQueue.Enqueue(new AnimPassThru(PassThruType.ResetTrigger, hash, -1, localApplyTiming));

			if (localApplyTiming == LocalApplyTiming.Immediately || !syncPassThrus)
				animator.ResetTrigger(hash);
		}

		public void Play(string stateName, int layer = -1, float normTime = 0, LocalApplyTiming localApplyTiming = LocalApplyTiming.OnSend)
		{
			int hash = Animator.StringToHash(stateName);
			Play(hash, layer, normTime, localApplyTiming);
		}
		public void Play(int hash, int layer = -1, float normTime = 0, LocalApplyTiming localApplyTiming = LocalApplyTiming.OnSend)
		{
			passThruQueue.Enqueue(new AnimPassThru(PassThruType.Play, hash, normTime, layer, localApplyTiming));

			if (localApplyTiming == LocalApplyTiming.Immediately || !syncPassThrus)
				animator.Play(hash, layer, normTime);
		}

		public void PlayInFixedTime(string stateName, int layer = -1, float fixedTime = 0, LocalApplyTiming localApplyTiming = LocalApplyTiming.OnSend)
		{
			int hash = Animator.StringToHash(stateName);
			PlayInFixedTime(hash, layer, fixedTime, localApplyTiming);
		}
		public void PlayInFixedTime(int hash, int layer = -1, float fixedTime = 0, LocalApplyTiming localApplyTiming = LocalApplyTiming.OnSend)
		{
			passThruQueue.Enqueue(new AnimPassThru(PassThruType.PlayFixed, hash, layer, fixedTime, localApplyTiming));

			if (localApplyTiming == LocalApplyTiming.Immediately || !syncPassThrus)
				animator.PlayInFixedTime(hash, layer, fixedTime);
		}

		public void CrossFade(string stateName, float duration, int layer = -1, float normalizedTime = 0, LocalApplyTiming localApplyTiming = LocalApplyTiming.OnSend)
		{
			int hash = Animator.StringToHash(stateName);
			CrossFade(hash, duration, layer, normalizedTime, localApplyTiming);
		}
		public void CrossFade(int hash, float duration, int layer = -1, float normalizedTime = 0, LocalApplyTiming localApplyTiming = LocalApplyTiming.OnSend)
		{
			passThruQueue.Enqueue(new AnimPassThru(PassThruType.CrossFade, hash, layer, duration, localApplyTiming));

			if (localApplyTiming == LocalApplyTiming.Immediately || !syncPassThrus)
				animator.CrossFade(hash, duration, layer, normalizedTime);
		}

		public void CrossFadeInFixedTime(string stateName, float duration, int layer = -1, float fixedTime = 0, LocalApplyTiming localApplyTiming = LocalApplyTiming.OnSend)
		{
			int hash = Animator.StringToHash(stateName);
			CrossFadeInFixedTime(hash, duration, layer, fixedTime, localApplyTiming);
		}
		public void CrossFadeInFixedTime(int hash, float duration, int layer = -1, float fixedTime = 0, LocalApplyTiming localApplyTiming = LocalApplyTiming.OnSend)
		{
			passThruQueue.Enqueue(new AnimPassThru(PassThruType.CrossFadeFixed, hash, fixedTime, layer, duration, localApplyTiming));

			if (localApplyTiming == LocalApplyTiming.Immediately || !syncPassThrus)
				animator.CrossFadeInFixedTime(hash, duration, layer, fixedTime);
		}


		#endregion

		#region Passthru Handling


		/// <summary>
		/// Deques and executes frame passthrus
		/// </summary>
		private void ExecutePassThruQueue(Frame frame)
		{
			var passThrus = frame.passThrus;
			while (passThrus.Count > 0)
			{
				var pt = passThrus.Dequeue();
				ExecutePassThru(pt);
			}
		}

		private void ExecutePassThru(AnimPassThru pt)
		{
			int hash = pt.hash;
			switch (pt.passThruType)
			{
				case PassThruType.Trigger:
					animator.SetTrigger(pt.hash);
					break;
				case PassThruType.ResetTrigger:
					animator.ResetTrigger(pt.hash);
					break;
				case PassThruType.Play:
					animator.Play(hash, pt.layer, pt.time);
					break;
				case PassThruType.PlayFixed:
					animator.Play(hash, pt.layer, pt.time);
					break;
				case PassThruType.CrossFade:
					animator.CrossFade(hash, pt.otherTime, pt.layer);
					break;
				case PassThruType.CrossFadeFixed:
					animator.CrossFadeInFixedTime(hash, pt.otherTime, pt.layer, pt.time);
					break;
			}
		}

		private void WritePassThrus(Frame frame, byte[] buffer, ref int bitposition, bool isKeyframe)
		{

			var passThrus = frame.passThrus;

			while (passThrus.Count > 0)
			{
				var pt = passThrus.Dequeue();
				var triggerType = pt.passThruType;
				int hash = pt.hash;

				bool isTrigger = triggerType == PassThruType.Trigger || triggerType == PassThruType.ResetTrigger;
				bool isCrossFade = triggerType == PassThruType.CrossFade || triggerType == PassThruType.CrossFadeFixed;

				if (pt.localApplyTiming == LocalApplyTiming.OnSend)
					ExecutePassThru(pt);

				/// Write first bool for has PassThru
				buffer.WriteBool(true, ref bitposition);

				/// Write TriggerType
				buffer.Write((uint)triggerType, ref bitposition, 3);

				int index;
				bool isIndexed;

				isIndexed =
				(isTrigger) ? sharedTriggHashes.TryGetValue(hash, out index) :
				sharedStateHashes.TryGetValue(pt.hash, out index);


				//bool isIndexed = foundTriggs.Contains(item.hash);
#if UNITY_EDITOR
				if (!isIndexed)
					Debug.LogWarning(GetType().Name +
						" is networking a state/trigger that has not been indexed, resulting in greatly increased size. Be sure to click 'Rebuild Indexes' whenever states/triggers are added or removed.");
#endif
				/// Write IsIndexed bool
				buffer.WriteBool(isIndexed, ref bitposition);

				/// Write Hash
				if (isIndexed)
					buffer.Write((uint)index, ref bitposition, isTrigger ? bitsForTriggerIndex : bitsForStateIndex);
				else
					buffer.WriteSigned(pt.hash, ref bitposition, 32);

				/// Triggers do not use the following - we are done.
				if (isTrigger)
					continue;

				/// Write layer
				if (syncLayers)
					buffer.Write((uint)pt.layer + 1, ref bitposition, bitsForLayerIndex);

				/// Write Time value
				if (pt.time == 0)
					buffer.WriteBool(false, ref bitposition);
				else if (triggerType == PassThruType.PlayFixed || triggerType == PassThruType.CrossFadeFixed)
				{
					buffer.WriteBool(true, ref bitposition);
					buffer.WriteHalf(pt.time, ref bitposition);
				}
				else
				{
					buffer.WriteBool(true, ref bitposition);
					buffer.Write((uint)(pt.time * NORM_COMP_ENCODE), ref bitposition, NORM_COMP_BITS);
				}

				/// Write Crossfade duration
				if (isCrossFade)
					buffer.WriteHalf(pt.otherTime, ref bitposition);
			}

			/// Write End of PassThru marker
			buffer.WriteBool(false, ref bitposition);

		}

		private void CapturePassThrus(Frame frame)
		{
			if (syncPassThrus)
				while (passThruQueue.Count > 0)
					frame.passThrus.Enqueue(passThruQueue.Dequeue());
		}

		private void ReadPassThrus(Frame frame, byte[] buffer, ref int bitposition, bool isKeyframe)
		{

			while (buffer.ReadBool(ref bitposition))
			{
				/// Read type
				PassThruType triggerType = (PassThruType)buffer.Read(ref bitposition, 3);
				bool isTrigger = triggerType == PassThruType.Trigger || triggerType == PassThruType.ResetTrigger;
				bool isCrossFade = triggerType == PassThruType.CrossFade || triggerType == PassThruType.CrossFadeFixed;

				/// Read isIndexed
				bool isIndexed = buffer.ReadBool(ref bitposition);

				/// Read Hash
				int hash;
				if (isIndexed)
				{
					if (isTrigger)
					{
						hash = (int)buffer.Read(ref bitposition, bitsForTriggerIndex);
						hash = sharedTriggIndexes[hash];
					}
					else
					{
						hash = (int)buffer.Read(ref bitposition, bitsForStateIndex);
						hash = sharedStateIndexes[hash];
					}
				}
				else
				{
					hash = buffer.ReadSigned(ref bitposition, 32);
#if UNITY_EDITOR
					Debug.LogWarning(GetType().Name +
						" is networking a state/trigger that has not been indexed, resulting in greatly increased size. Be sure to click 'Rebuild Indexes' whenever states/triggers are added or removed. " + hash);
#endif
				}


				int layer;
				float time;
				if (!isTrigger)
				{
					/// Read Layer
					bool useLayer = syncLayers && (triggerType != PassThruType.Trigger && triggerType != PassThruType.ResetTrigger);
					layer = useLayer ? ((int)buffer.Read(ref bitposition, bitsForLayerIndex) - 1) : -1;

					/// Read include time book, and if true read time
					bool nonZeroTime = buffer.ReadBool(ref bitposition);
					time = (!nonZeroTime) ? 0 :
						(triggerType == PassThruType.PlayFixed || triggerType == PassThruType.CrossFadeFixed) ?
						buffer.ReadHalf(ref bitposition) :
						buffer.Read(ref bitposition, NORM_COMP_BITS) * NORM_COMP_DECODE;
				}
				else
				{
					layer = -1;
					time = 0;
				}

				float dur;
				/// Read duration
				if (isCrossFade)
				{
					bool useDuration = (triggerType == PassThruType.CrossFade || triggerType == PassThruType.CrossFadeFixed);
					dur = (useDuration) ? buffer.ReadHalf(ref bitposition) : -1;
				}
				else
					dur = -1;

				frame.passThrus.Enqueue(new AnimPassThru(triggerType, hash, time, layer, dur));
			}
		}

		#endregion

		#region State Handling

		private void CaptureStates(Frame frame)
		{
			int count = (syncLayers) ? layerCount : 1;
			for (int layer = 0; layer < count; ++layer)
			{
				frame.layerWeights[layer] = animator.GetLayerWeight(layer);

				if (animator.IsInTransition(layer))
				{
					frame.layerIsInTransition[layer] = true;

					if (syncTransitions)
					{
						AnimatorTransitionInfo transInfo = animator.GetAnimatorTransitionInfo(layer);
						frame.stateHashes[layer] = transInfo.fullPathHash;
						frame.normalizedTime[layer] = transInfo.normalizedTime;

						//if (transInfo.fullPathHash == 0)
						//	Debug.LogError(layer + " " + transInfo.anyState +  " [" + transInfo.userNameHash +  "] Cap 0");

					}
				}
				else
				{
					AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(layer);
					frame.layerIsInTransition[layer] = false;
					frame.stateHashes[layer] = stateInfo.fullPathHash;
					frame.normalizedTime[layer] = stateInfo.normalizedTime;

				}

				//Debug.Log(stateInfo.fullPathHash);
			}
		}

		private void WriteStates(Frame frame, byte[] buffer, ref int bitposition, bool isKeyframe)
		{
			var statehashes = frame.stateHashes;
			var normaltimes = frame.normalizedTime;
			var lyerweights = frame.layerWeights;
			var lyerInTrans = frame.layerIsInTransition;
			int count = (syncLayers) ? layerCount : 1;
			for (int layer = 0; layer < count; ++layer)
			{

				int? layerStateHash = statehashes[layer];
				bool stateHasChange = !layerStateHash.HasValue || lastAnimationHash[layer] != layerStateHash.Value;

				/// Write State/NormTime
				if (isKeyframe || stateHasChange)
				{
					/// Write include StateHash bool
					buffer.WriteBool(true, ref bitposition);
					bool isInTransition = lyerInTrans[layer];
					buffer.WriteBool(isInTransition, ref bitposition);

					if (isInTransition)
					{
						if (syncTransitions)
						{
							TransitionInfo ti;

							bool isIndexed = sharedTransHashes.TryGetValue(layerStateHash.Value, out ti);
							int index = (isIndexed) ? ti.index : -1;

							buffer.WriteBool(isIndexed, ref bitposition);

							if (isIndexed)
							{
								buffer.Write((uint)index, ref bitposition, bitsForTransIndex);
							}
							else
							{
#if UNITY_EDITOR
								Debug.LogWarning(GetType().Name +
									" is networking a transition that has not been indexed, resulting in greatly increased size. Be sure to click 'Rebuild Indexes' whenever transitions are added or removed.");
#endif
								Debug.LogError("Unknwn Trans " + layerStateHash.Value);
								buffer.WriteSigned(layerStateHash.Value, ref bitposition, 32);
							}

							/// We don't bother with time for index 0 - indicates an unusable transition
							if (index != 0)
								buffer.Write((uint)(normaltimes[layer] * NORM_COMP_ENCODE), ref bitposition, NORM_COMP_BITS);
						}
					}
					else
					{

						int index = sharedStateIndexes.IndexOf(layerStateHash.Value);
						bool useIndex = index != -1;

						buffer.WriteBool(useIndex, ref bitposition);

						if (useIndex)
							buffer.Write((uint)sharedStateIndexes.IndexOf(layerStateHash.Value), ref bitposition, bitsForStateIndex);
						else
						{
#if UNITY_EDITOR
							Debug.LogWarning(GetType().Name +
								" is networking a state that has not been indexed, resulting in greatly increased size. Be sure to click 'Rebuild Indexes' whenever states are added or removed.");
#endif
							buffer.WriteSigned(layerStateHash.Value, ref bitposition, 32);
						}

						buffer.Write((uint)(normaltimes[layer] * NORM_COMP_ENCODE), ref bitposition, NORM_COMP_BITS);
					}

					lastAnimationHash[layer] = layerStateHash.HasValue ? layerStateHash.Value : 0;
				}
				else
					buffer.WriteBool(false, ref bitposition);

				/// Write LayerWeights
				if (syncLayerWeights && layer != 0)
				{
					uint cLayerWeight = (uint)(lyerweights[layer] * NORM_COMP_ENCODE);

					if (isKeyframe || (lastLayerWeight[layer] != cLayerWeight))
					{
						//Debug.Log("<color=green>weight</color> " + frame.frameId + " " + isKeyframe + " " + cLayerWeight.cvalue + "/ " + lastLayerWeight[layer]);
						buffer.WriteBool(true, ref bitposition);
						buffer.Write((uint)(cLayerWeight * NORM_COMP_ENCODE), ref bitposition, NORM_COMP_BITS);
						lastLayerWeight[layer] = cLayerWeight;

					}
					else
					{
						buffer.WriteBool(false, ref bitposition);
						//Debug.Log("<color=red>weight</color> " + lastLayerWeight[layer]);

					}
				}
			}
		}

		private void ReadStates(Frame frame, byte[] buffer, ref int bitposition, bool isKeyframe)
		{

			var statehashes = frame.stateHashes;
			var normaltimes = frame.normalizedTime;
			var lyerweights = frame.layerWeights;
			var lyerInTrans = frame.layerIsInTransition;

			int count = (syncLayers) ? layerCount : 1;
			for (int layer = 0; layer < count; ++layer)
			{
				bool stateHasChange = buffer.ReadBool(ref bitposition);

				if (stateHasChange)
				{
					bool layerIsInTransition = buffer.ReadBool(ref bitposition);
					lyerInTrans[layer] = layerIsInTransition;

					if (layerIsInTransition)
					{
						if (syncTransitions)
						{
							/// Complex mess that determines if the hash was sent as an index or full hash
							bool isIndexed = buffer.ReadBool(ref bitposition);

							int hash = (isIndexed) ? (int)buffer.Read(ref bitposition, bitsForTransIndex) : buffer.ReadSigned(ref bitposition, 32);
							if (isIndexed)
							{
								hash = sharedTransIndexes[hash].hash;
							}

							statehashes[layer] = hash;
							normaltimes[layer] = (hash != 0) ? buffer.Read(ref bitposition, NORM_COMP_BITS) * NORM_COMP_DECODE : 0;
						}
					}
					else
					{
						bool isIndexed = buffer.ReadBool(ref bitposition);

						int hash = (isIndexed) ? (int)buffer.Read(ref bitposition, bitsForStateIndex) : buffer.ReadSigned(ref bitposition, 32);
						if (isIndexed)
						{
							hash = sharedStateIndexes[hash];
						}

						statehashes[layer] = hash;
						normaltimes[layer] = buffer.Read(ref bitposition, NORM_COMP_BITS) * NORM_COMP_DECODE;
					}
				}

				if (syncLayerWeights && layer != 0)
				{
					bool weightHasChange = buffer.ReadBool(ref bitposition);
					if (weightHasChange)
					{
						lyerweights[layer] = buffer.Read(ref bitposition, NORM_COMP_BITS) * NORM_COMP_DECODE;
					}

				}
			}
		}

		private void ApplyState()
		{

			int count = (syncLayers) ? layerCount : 1;
			for (int layer = 0; layer < count; ++layer)
			{
				/// Set frame/layer as no longer valid (prevents from missed incoming frames finding this value again) 
				/// Really need to add some masks or make these nullable
				pre2F.stateHashes[layer] = null;
				pre2F.normalizedTime[layer] = 0;

				int? statehash = snapF.stateHashes[layer];

				bool isTransition = snapF.layerIsInTransition[layer];

				if (statehash.HasValue)
				{
					if (isTransition)
					{
						if (syncTransitions)
						{
							TransitionInfo ti;
							bool foundIndex = sharedTransHashes.TryGetValue(statehash.Value, out ti);
							int index = (foundIndex) ? ti.index : -1;

							/// index of -1 means non-indexed / index of 0 indicates an unusable transition (crossfade?)
							if (index <= 0)
							{
								if (index == -1)
									Debug.LogWarning("Unknown Transition" + statehash.Value);

								continue;
							}

							/// TODO: duration of 1 is a placeholder
							if (ti.durationIsFixed)
								animator.CrossFadeInFixedTime(ti.destination, ti.duration, layer, snapF.normalizedTime[layer] * ti.duration);
							else
								animator.CrossFade(ti.destination, ti.duration, layer, snapF.normalizedTime[layer]);
						}
					}
					/// TODO: 0 check may not be a thing any more
					else if (statehash.Value > 0)
					{
						//Debug.LogWarning(snapF.frameId + " <b>Play</b> " + statehash.Value + " indexed: " + sharedStateHashes.ContainsKey(statehash.Value) + " " + snapF.normalizedTime[layer]);
						animator.Play(statehash.Value, layer, snapF.normalizedTime[layer]);
					}
				}
			}
		}

		#endregion  // end state handling

		#endregion

		#region Snapshot / Interpolate / Extrapolate

		//private bool hasInitialSnapshot;

		/// <summary>
		/// Advance the buffered state, getting a new target.
		/// </summary>
		public override void OnSnapshot(int newTargetFrameId, bool isActingAuthority, bool initialize)
		{
			if (!enabled)
				return;

			base.OnSnapshot(newTargetFrameId, isActingAuthority, initialize);

			bool snapWasValid = netObj.validFrames.Get(snapF.frameId);

			/// End of Interpolation triggers and Events
			/// Since we are interpolating, to line up timing of all networked objects
			/// The actual occurance of a frame is when it ARRIVES at target.

			/// TODO: don't know if states act like triggers or params yet.

			if (snapWasValid)
			{
				if (syncStates)
					ApplyState();

				/// triggers and crossfades don't extrapolate.
				/// TODO: Add a choice for executing at end of interpolation
				ExecutePassThruQueue(snapF);
			}

			

			CompleteTargetParameters();

			if (!snapF.hasChanged)
				InterpolateParams(0);
		}

		public void OnInterpolate(float t)
		{
			if (!enabled)
				return;

			if (ReferenceEquals(targF, null))
				return;

			if (!targF.hasChanged)
				return;

			if (syncParams)
			{
				InterpolateParams(t);
			}
		}

		///  UNTESTED
		protected override void Interpolate(Frame targFrame, Frame startFrame, Frame endFrame, float t)
		{
			/// TODO: This currently just copies the last value
			targFrame.CopyFrom(endFrame);

			InterpolateState(targFrame, startFrame, endFrame, t);

		}

		protected override void Extrapolate()
		{
			ExtrapolateParams(pre1F, snapF, targF);
			ExtrapolateState();
		}

		private void ExtrapolateState()
		{
			int count = (syncLayers) ? layerCount : 1;
			for (int layer = 0; layer < count; ++layer)
			{
				var pre1hash = pre1F.stateHashes[layer];
				var snaphash = snapF.stateHashes[layer];

				targF.stateHashes[layer] = snaphash;

				float snapTime = snapF.normalizedTime[layer];

				if (pre1hash != snaphash && snapTime != 0)
				{
					float delta = snapTime - pre1F.normalizedTime[layer];
					targF.normalizedTime[layer] = snapF.normalizedTime[layer] + delta;
					//Debug.LogError("<color=green>Good State Extrap</color> " + snapF.normalizedTime[layer]+ " " + targF.normalizedTime[layer]);
				}
				else
				{
					targF.normalizedTime[layer] = snapTime;
					//Debug.LogError("<color=red>Bad State Extrap</color> " + snapTime);
				}
			}
		}

		private void InterpolateState(Frame targFrame, Frame strFrame, Frame endFrame, float t)
		{
			int count = (syncLayers) ? layerCount : 1;
			for (int layer = 0; layer < count; ++layer)
			{
				var strhash = strFrame.stateHashes[layer];
				var endhash = endFrame.stateHashes[layer];

				targFrame.stateHashes[layer] = endhash;

				float strTime = strFrame.normalizedTime[layer];
				float endTime = endFrame.normalizedTime[layer];

				if (strhash != endhash && strTime != 0)
				{
					targFrame.normalizedTime[layer] = Mathf.LerpUnclamped(strTime, endTime, t);
					//Debug.LogError("<color=green>Good State Extrap</color> " + snapF.normalizedTime[layer]+ " " + targF.normalizedTime[layer]);
				}
				else
				{
					targFrame.normalizedTime[layer] = strTime;
					//Debug.LogError("<color=red>Bad State Extrap</color> " + snapTime);
				}
			}
		}
		#endregion
	}
}

//#pragma warning restore CS0618 // Type or member is obsolete

