// Copyright 2019, Davin Carten, All rights reserved
// This code may only be used in game development, but may not be used in any tools or assets that are sold or made publicly available to other developers.



using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using emotitron.Compression;
using emotitron.Utilities.Networking;
using emotitron.Utilities;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace emotitron.Networking
{

	[RequireComponent(typeof(NetObjAdapter))]
	public class NetObjectLite : MonoBehaviour
		, INetObject
	{
		// Bits used in byte[] stream to indicate how many bits of data were written.
		private const int BITS_FOR_DATA_SIZE = 8;
		public const int MAX_ORDER_VAL = 9;

		public const int TARG_BUFF_CNT = 2;
		public const int MAX_BUFF_CNT = 3;
		public const int MIN_BUFF_CNT = 1;
		public const int TICKS_BEFORE_CORRECTION = 5;

		public const int FRAME_CNT = NetMasterLite.NET_FRAME_COUNT;

		#region Static NetObject Lookups and Pools

		public static Dictionary<uint, NetObjectLite> netObjLookup = new Dictionary<uint, NetObjectLite>();
		public static List<NetObjectLite> activeNetObjs = new List<NetObjectLite>();
		public static List<NetObjectLite> netObjs = new List<NetObjectLite>();

		public BitArray validFrames = new BitArray(FRAME_CNT);

		#endregion

#if UNITY_EDITOR
		private void Reset()
		{
			UnityEditorInternal.InternalEditorUtility.SetIsInspectorExpanded(this, false);

			var noa = GetComponent<NetObjAdapter>();
			if (noa == null)
				transform.root.gameObject.AddComponent<NetObjAdapter>();
		}
#endif

		[System.NonSerialized] public NetObjAdapter noa;

		[System.NonSerialized] public int currTargFrameId;

		#region Callback Interfaces

		private static List<Component> reusableFindsyncObjects = new List<Component>();

		private readonly List<IOnNetSerialize> iOnNetSerialize = new List<IOnNetSerialize>();
		private readonly List<IOnNetDeserialize> iOnNetDeserialize = new List<IOnNetDeserialize>();
		private readonly List<IOnSnapshot> iOnSnapshot = new List<IOnSnapshot>();
		private readonly List<IOnQuantize> iOnQuantize = new List<IOnQuantize>();
		private readonly List<IOnInterpolate> iOnInterpolate = new List<IOnInterpolate>();
		private readonly List<IOnCaptureCurrentValues> iOnCaptureCurrentValues = new List<IOnCaptureCurrentValues>();
		private readonly List<IOnChangeAuthority> iOnChangeAuthority = new List<IOnChangeAuthority>();

		private void CollectInterfaces()
		{
			CollectAndReorderInterfaces();
		}

		private void CollectAndReorderInterfaces()
		{
			GetComponentsInChildren(reusableFindsyncObjects);

			int cnt = reusableFindsyncObjects.Count;
			for (int order = 0; order <= MAX_ORDER_VAL; ++order)
			{
				for (int index = 0; index < cnt; ++index)
				{
					var comp = reusableFindsyncObjects[index];
					var iApplyOrder = comp as IApplyOrder;

					/// Apply any objects without IApplyOrder to the middle timing of 5
					if (ReferenceEquals(iApplyOrder, null))
					{
						if (order == 5)
						{
							//Debug.Log("Unordered " + comp.name);
							AddInterfaceToList(comp, iOnNetSerialize);
							AddInterfaceToList(comp, iOnNetDeserialize);
							AddInterfaceToList(comp, iOnQuantize);
							AddInterfaceToList(comp, iOnCaptureCurrentValues);
							AddInterfaceToList(comp, iOnSnapshot);
							AddInterfaceToList(comp, iOnInterpolate);
							AddInterfaceToList(comp, iOnChangeAuthority);
						}
					}
					else if (iApplyOrder.ApplyOrder == order)
					{
						AddInterfaceToList(comp, iOnNetSerialize);
						AddInterfaceToList(comp, iOnNetDeserialize);
						AddInterfaceToList(comp, iOnQuantize);
						AddInterfaceToList(comp, iOnCaptureCurrentValues);
						AddInterfaceToList(comp, iOnSnapshot);
						AddInterfaceToList(comp, iOnInterpolate);
						AddInterfaceToList(comp, iOnChangeAuthority);
					}
				}
			}
		}

		private void AddInterfaceToList<T>(Component comp, List<T> list) where T : class
		{
			T i1 = comp as T;
			if (!ReferenceEquals(i1, null))
				list.Add(comp as T);
		}

		#endregion

		#region Inspector Fields

		[System.NonSerialized] public Authority authority = Authority.Auto;

		#endregion

		protected void Awake()
		{
			noa = GetComponent<NetObjAdapter>();
			if (noa == null)
				noa = transform.root.gameObject.AddComponent<NetObjAdapter>();

			netObjs.Add(this);

			CollectInterfaces();
		}

		private void OnEnable()
		{
			activeNetObjs.Add(this);
		}

		private void OnDisable()
		{
			activeNetObjs.Remove(this);
		}

		private void OnDestroy()
		{
			netObjs.Remove(this);
		}

		/// Virtual console logging
#if TICKS_TO_UICONSOLE && (DEBUG || UNITY_EDITOR || DEVELOPMENT_BUILD)
		
		private void Update()
		{
			Debugging.UIConsole.Clear();
		}

		private void LateUpdate()
		{
			if (!noa.IsMine)
			{
				Debugging.UIConsole.Log(noa.NetId + " " + validFrames.PrintMask(currTargFrameId, -1).ToString());
			}
		}

#endif

		#region Initialization Message Handler

		/// <summary>
		/// Specifically here to accect the OnSerialize initialization data from UNET, but can be used as the entry point for any netlibs initialization serialization
		/// </summary>
		public bool SendInitialization(out byte[] buffer, out int bytecount)
		{
			buffer = NetMsgSends.reusableOutgoingBuffer;
			bytecount = 0;

			bool hascontent = false;
			int bitposition = 0;

			int cnt = iOnNetSerialize.Count;
			for (int i = 0; i < cnt; ++i)
				hascontent |= iOnNetSerialize[i].OnNetSerialize(-1, buffer, ref bitposition);

			if (!hascontent)
				return false;

			bytecount = (bitposition + 7) >> 3;

			return true;
		}

		/// <summary>
		/// Specifically here to accect the OnSerialize initialization data from UNET, but can be used as the entry point for any netlibs initialization serialization
		/// </summary>
		public void ReceiveInitialization(byte[] buffer, int bytecount)
		{
			int bitposition = 0;

			int cnt = iOnNetDeserialize.Count;
			for (int i = 0; i < cnt; ++i)
				iOnNetDeserialize[i].OnNetDeserialize(FRAME_CNT, FRAME_CNT, FRAME_CNT, buffer, ref bitposition);

		}

		#endregion

		#region Static ToAll Methods

		public static void SerializeAllNetObjs(int frameId)
		{

			var buffer = NetMsgSends.reusableOutgoingBuffer;

			int cnt = activeNetObjs.Count;
			for (int i = 0; i < cnt; ++i)
			{
				var netObj = activeNetObjs[i];

				if (!netObj.noa.IsMine)
					continue;

				netObj.OnCaptureCurrentValues(frameId);

				int bitposition = 0;
				/// Write frameId
				buffer.Write((uint)frameId, ref bitposition, NetMasterLite.NET_FRAME_BITS);

				/// Write netid
				buffer.WritePackedBytes((uint)netObj.noa.NetId, ref bitposition, 32);

				/// BEGIN CONTENT
				activeNetObjs[i].OnSerialize(frameId, buffer, ref bitposition);

				NetMsgSends.Send(buffer, bitposition, netObj.gameObject, ReceiveGroup.Others);
			}
		}


		public static void Deserialize(object conn, int connId, byte[] buffer)
		{
			int bitposition = 0;

			/// Read frameId
			int frameId = (int)buffer.Read(ref bitposition, NetMasterLite.NET_FRAME_BITS);
			
			/// Read netid
			uint netid = buffer.ReadPackedBytes(ref bitposition, 32);

			var netobj = netid.FindComponentByNetId<NetObjectLite>();

			if (netobj && !netobj.noa.IsMine)
				netobj.OnDeserialize(frameId, frameId, frameId, buffer, ref bitposition);

		}

		public static void SnapshotAllNetObjs(int frameId)
		{
			int cnt = activeNetObjs.Count;
			for (int i = 0; i < cnt; ++i)
			{
				var netObj = activeNetObjs[i];
				if (!netObj.noa.IsMine)
					netObj.OnSnapshot(frameId);
			}
		}

		public static void InterpolateAllNetObjs(float t)
		{
			int cnt = activeNetObjs.Count;
			for (int i = 0; i < cnt; ++i)
			{
				var netObj = activeNetObjs[i];
				if (!netObj.noa.IsMine)
					netObj.OnInterpolate(t);
			}
		}

		#endregion

		public void OnCaptureCurrentValues(int frameId)
		{
			int cnt = iOnCaptureCurrentValues.Count;
			for (int i = 0; i < cnt; ++i)
				iOnCaptureCurrentValues[i].OnCaptureCurrentValues(frameId, noa.IsMine, Realm.Primary);
		}

		public bool OnSerialize(int frameId, byte[] buffer, ref int bitposition)
		{
			bool hascontent = false;

			int cnt = iOnNetSerialize.Count;
			for (int i = 0; i < cnt; ++i)
				hascontent |= iOnNetSerialize[i].OnNetSerialize(frameId, buffer, ref bitposition);

			return hascontent;
		}

		//public int targetBufferCount = 3;

		//private bool offsetHasBeenEstablished;
		bool processedInitialBacklog;
		float firstDeserializeTime;
		int backlogCount;

		public void OnDeserialize(int sourceFrameId, int originFrameId, int localframeId, byte[] buffer, ref int bitposition)
		{
			/// Startup handling. Set the starting guess for currTargFrameId, and burn through excess update backlog, 
			if (!processedInitialBacklog || !hadInitialSnapshot)
			{
				//Debug.Log("<b>Initial Deserialize</b> " + localframeId);
				if (firstDeserializeTime == 0)
				{
					/// First incoming, we establish a currTargFrameId
					firstDeserializeTime = Time.time; ;
					currTargFrameId = localframeId - TARG_BUFF_CNT;
					if (currTargFrameId < 0) currTargFrameId += FRAME_CNT;
				}
				else if (Time.time > firstDeserializeTime)
				{
					/// This is a new set of incoming, we are no longer processing a startup backlog
					processedInitialBacklog = true;

					/// Keep incrementing until we start snapshotting. Getting through a backlog of starup frames
					/// Even though this is a new Update time, we still may have more backlog to get through.
					if (!hadInitialSnapshot)
					{
						/// if there was a backlog and we have more than one frame to deserialize in the queue, advance the snapshot
						/// to maintain our buffer offset.
						currTargFrameId = localframeId - TARG_BUFF_CNT;
						validFrames.ClearBitsBefore(currTargFrameId, 4);
						if (currTargFrameId < 0) currTargFrameId += FRAME_CNT;

					}
				}
			}

			int cnt = iOnNetDeserialize.Count;
			for (int i = 0; i < cnt; ++i)
				iOnNetDeserialize[i].OnNetDeserialize(sourceFrameId, originFrameId, localframeId, buffer, ref bitposition);

			/// Flag frame as valid if it is still in the future
			int frameOffsetFromCurrent = localframeId - currTargFrameId;
			bool frameIsInFuture = frameOffsetFromCurrent > 0 || frameOffsetFromCurrent < (-FRAME_CNT / 2);
			validFrames.Set(localframeId, frameIsInFuture);

			if (!frameIsInFuture)
			{
				frameArrivedTooLate = true;
			}

#if !PUN_2_OR_NEWER
			if (MasterNetAdapter.ServerIsActive)
				NetMsgSends.Send(buffer, bitposition, gameObject, ReceiveGroup.Others);
#endif
		}

		bool hadInitialSnapshot;
		int numOfSequentialFramesWithTooSmallBuffer = 0;
		int numOfSequentialFramesWithTooLargeBuffer = 0;
		bool frameArrivedTooLate;

		public void OnSnapshot(int frameId)
		{
			if (!processedInitialBacklog)
				return;

			//if (noa.IsMine)
			//	return;

			if (!noa.enabled)
				return;

			//Debug.Log(currTargFrameId + " " + validFrames.PrintMask(currTargFrameId, null));

			/// TODO: May be able to reduce this in the future to a less aggressive look ahead, and cache this once its settled
			int validCount = validFrames.CountValidRange(currTargFrameId, (FRAME_CNT / 4));

//			if (validCount == 0)
//			{
//#if SNS_WARNINGS
//				Debug.LogWarning("<b>SNS Buffer Empty - Holding frame</b> " + currTargFrameId + " buffsze: " + validCount);
//#endif
//				numOfSequentialFramesWithTooLargeBuffer = 0;
//				numOfSequentialFramesWithTooSmallBuffer++;
//				return;
//			}

			int advanceCount;

			if (frameArrivedTooLate || validCount < MIN_BUFF_CNT)
			{

				numOfSequentialFramesWithTooLargeBuffer = 0;
				numOfSequentialFramesWithTooSmallBuffer++;
				frameArrivedTooLate = false;

				if (!hadInitialSnapshot || numOfSequentialFramesWithTooSmallBuffer >= TICKS_BEFORE_CORRECTION)
				{
#if SNS_WARNINGS
					Debug.LogWarning("<b>Buffer Low</b> - Holding frame " + currTargFrameId + " buffsze: " + validCount);
#endif
					return;
				}
				else
					advanceCount = 1;
			}
			else 
			if (validCount > MAX_BUFF_CNT)
			{
				numOfSequentialFramesWithTooLargeBuffer++;
				numOfSequentialFramesWithTooSmallBuffer = 0;
				if (numOfSequentialFramesWithTooLargeBuffer > TICKS_BEFORE_CORRECTION)
				{
#if SNS_WARNINGS
					Debug.LogWarning("<b>Trimming Oversized Buffer </b>" + currTargFrameId + " count: " + validCount);
#endif
					advanceCount = 2;
				}
				else
					advanceCount = 1;
			}
			else
			{
				numOfSequentialFramesWithTooLargeBuffer = 0;
				numOfSequentialFramesWithTooSmallBuffer = 0;
				advanceCount = 1;

			}

			while ((advanceCount-- >= 1)) 
			{

				int outdatedFrame = currTargFrameId - 4;
				if (outdatedFrame < 0) outdatedFrame += FRAME_CNT;
				validFrames.Set(outdatedFrame, false);

				currTargFrameId++;
				if (currTargFrameId >= FRAME_CNT) currTargFrameId -= FRAME_CNT;

				int cnt = iOnSnapshot.Count;
				for (int i = 0; i < cnt; ++i)
					iOnSnapshot[i].OnSnapshot(currTargFrameId, false, false);

			}

			hadInitialSnapshot = true;
		}

		public void OnInterpolate(float t)
		{
			int cnt = iOnInterpolate.Count;
			for (int i = 0; i < cnt; ++i)
				iOnInterpolate[i].OnInterpolate(t);
		}
	}


#if UNITY_EDITOR
	[CustomEditor(typeof(NetObjectLite))]
	public class NetObjectLiteEditor : HeaderEditorBase
	{
		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();
			EditorGUILayout.HelpBox("Network Entity manager used by Simple Network Sync. Collects all networking interfaces from child components," +
				" and relays network callbacks, serialization, and events between the NetMaster and synced components.", MessageType.None);
		}
	}

#endif
}
