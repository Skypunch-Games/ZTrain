// Copyright 2019, Davin Carten, All rights reserved
// This code may be used for game development, but may not be used in any tools or assets that are sold to other developers.


using UnityEngine;
using System.Collections;


#if UNITY_EDITOR
using emotitron.Utilities;
using UnityEditor;
#endif

namespace emotitron.Networking
{

	/// <summary>
	/// SyncObject base class with handling for frames and frame history.
	/// </summary>
	/// <typeparam name="TFrame">The derived FrameBase class to be used as the frame.</typeparam>
	public abstract class SyncObject<TFrame> : SyncObject
		, IOnSnapshot
		where TFrame : FrameBase
	{
		public const int FRAME_CNT = NetMasterLite.NET_FRAME_COUNT;

		[System.NonSerialized] public readonly TFrame[] frames = new TFrame[FRAME_CNT + 1];

		[Tooltip("Every X Net Tick this object will serialize a full update, regardless of having changed or not. Primarily needed for PUN2. For UNet/Mirror this can be left at 0.")]
		[Range(0, FRAME_CNT)]
		public int keyframeRate = Utilities.Networking.NetObjAdapter.ADAPTER_NAME == "PUN2" ? 1 : 0;

		/// TODO: Make keyframe offset. AUTO will offset it by netid % keyframeRate
		//[Range(0, NetMasterLite.NET_FRAME_COUNT)]
		//public int keyframeOffset = 0;

		/// Runtime vars
		protected TFrame pre2F, pre1F, snapF, targF, nextF;
		protected bool hadInitialSnapshot;

		/// <summary>
		/// When overriding, but sure to keep base.Awake(). Also, frames are created and given indexes, but any other Initialization will still need to be
		/// explictly called in the derived Awake().
		/// </summary>
		protected override void Awake()
		{
			base.Awake();
		}

		protected virtual void OnEnable()
		{
			///TEST
			hadInitialSnapshot = false;
		}


		public virtual void OnSnapshot(int frameId, bool isActingAuthority, bool initialize)
		{
			TFrame frame = frames[frameId];
			/// Our initial Snapshot needs to initialize
			if (!hadInitialSnapshot || initialize)
			{

				int prevprevId = (frameId - 3);
				if (prevprevId < 0) prevprevId += FRAME_CNT;

				int prevId = (frameId - 2);
				if (prevId < 0) prevId += FRAME_CNT;

				int snapId = (frameId - 1);
				if (snapId < 0) snapId += FRAME_CNT;

				int nextId = (frameId + 1);
				if (nextId >= FRAME_CNT) nextId -= FRAME_CNT;

				pre2F = frames[prevprevId];
				pre1F = frames[prevId];
				snapF = frames[snapId];
				targF = frame;
				nextF = frames[nextId];

				/// Initialize indicates that we have stored a specific initialization from UNET and its stored in the offtick frame
				/// PUN2 has no initialization options, so its first incoming frame has to be treated as the initialization.
				TFrame initframe = (initialize) ? frames[FRAME_CNT] : frame;
				{
					pre2F.CopyFrom(initframe);
					pre1F.CopyFrom(initframe);
					snapF.CopyFrom(initframe);
					targF.CopyFrom(initframe);
					nextF.CopyFrom(initframe);
				}

				hadInitialSnapshot = true;
			}
			else
			{
				pre2F = pre1F;
				pre1F = snapF;
				snapF = targF;
				targF = frame;

				int nextId = (frameId + 1);
				if (nextId >= FRAME_CNT)
					nextId -= FRAME_CNT;

				nextF = frames[nextId];
				
				/// Non authority connections reconstruct this frame if it's missing.
				if (!isActingAuthority && !noa.IsMine)
				{
					if (!netObj.validFrames[frameId])
						ConstructMissingFrame(frameId);
					else if (targF.hasChanged == false)
						targF.CopyFrom(snapF);
				}
			}

			snapshotTime = Time.fixedTime;
		}

		protected virtual void ConstructMissingFrame(int frameId)
		{
			/// Mark as changed so Interpolate will respect this as valid.
			targF.hasChanged = true;

			//Debug.LogWarning("Constructing Missing (Packetloss or disconnected player)" + frameId);
			/// Look forward to see if we have any valid frames in the future we can interpolate our missing frame with

			const int MAX_LOOKAHEAD = 3;
			for (int i = 2; i <= MAX_LOOKAHEAD; ++i)
			{
				int futureFid = frameId + i;
				if (futureFid >= FRAME_CNT)
					futureFid -= FRAME_CNT;

				if (netObj.validFrames[futureFid] && frames[futureFid].hasChanged) //  ((validMask & (ulong)1 << futureFid) != 0)
				{
					float t = 1f / i;
					Interpolate(targF, snapF, frames[futureFid], t);
					return;
				}
			}

			/// No future valid frame found, just do a regular extrapolation
			Extrapolate();
		}

		protected abstract void Interpolate(TFrame targ, TFrame start, TFrame end, float t);
		protected abstract void Extrapolate();
	}

#if UNITY_EDITOR
	[CustomEditor(typeof(SyncObject<>))]
	[CanEditMultipleObjects]
	public class SyncObjectTFrameBase : SyncObjectBase
	{
		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();
		}
	}
#endif
}

