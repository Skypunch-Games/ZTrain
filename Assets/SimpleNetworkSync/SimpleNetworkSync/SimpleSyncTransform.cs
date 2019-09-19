// Copyright 2019, Davin Carten, All rights reserved
// This code may only be used in game development, but may not be used in any tools or assets that are sold or made publicly available to other developers.


using UnityEngine;
using System.Collections.Generic;

using emotitron.Compression;
using emotitron.Utilities;
using emotitron.Utilities.Networking;
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
#endif

#pragma warning disable CS0618 // Supress UNET warnings

namespace emotitron.Networking
{
	[DisallowMultipleComponent]
	public class SimpleSyncTransform : SyncObject<SimpleSyncTransform.Frame>
		, ISyncTransform
		, IOnNetSerialize
		, IOnNetDeserialize
		, IApplyOrder
		, IOnSnapshot
		, IOnInterpolate
		, IOnCaptureCurrentValues
		, IOnChangeAuthority
	{

		#region Inspector Fields

		[Tooltip("If the distance delta between snapshots exceeds this amount, object will move to new location without lerping.")]
		public float teleportThreshold = 5f;
		private float teleportThresholdSqrMag;
		
		[Tooltip("Not Implemented yet")]
		public Interpolation interpolation = Interpolation.Linear;

		[Tooltip("Percentage of extrapolation from previous values. [0 = No Extrapolation] [.5 = 50% extrapolation] [1 = Undampened]. This allows for gradual slowing down of motion when the buffer runs dry.")]
		[Range(0f, 1f)]
		public float extrapolateRatio = .5f;
		protected int extrapolationCount;

		[Tooltip("Will set all non-authority instances of rigidbodies to kinematic, which is how they need to be set for these syncs to work correctly. Also enables rb interpolation.")]
		public bool autoKinematic = true;

		public Dictionary<int, TransformCrusher> masterSharedCrushers = new Dictionary<int, TransformCrusher>();
		public TransformCrusher transformCrusher = new TransformCrusher()
		{

			PosCrusher = new ElementCrusher(TRSType.Position, false)
			{
				hideFieldName = true,
				XCrusher = new FloatCrusher(Axis.X, TRSType.Position, true) { BitsDeterminedBy = BitsDeterminedBy.HalfFloat },
				YCrusher = new FloatCrusher(Axis.Y, TRSType.Position, true) { BitsDeterminedBy = BitsDeterminedBy.HalfFloat },
				ZCrusher = new FloatCrusher(Axis.Z, TRSType.Position, true) { BitsDeterminedBy = BitsDeterminedBy.HalfFloat }
			},
			RotCrusher = new ElementCrusher(TRSType.Quaternion, false)
			{
				hideFieldName = true,
				XCrusher = new FloatCrusher(Axis.X, TRSType.Quaternion, true) { Bits = 12 },
				YCrusher = new FloatCrusher(Axis.Y, TRSType.Euler, true) { Bits = 12 },
				ZCrusher = new FloatCrusher(Axis.Z, TRSType.Euler, true) { Bits = 12 },
				QCrusher = new QuatCrusher(44, true)
				//QCrusher = new QuatCrusher(CompressLevel.uint64Hi, false, false)
			},
			SclCrusher = new ElementCrusher(TRSType.Scale, false)
			{
				hideFieldName = true,
				uniformAxes = ElementCrusher.UniformAxes.NonUniform,
				//UCrusher = new FloatCrusher(Axis.Uniform, TRSType.Scale, true) { BitsDeterminedBy = BitsDeterminedBy.HalfFloat, axis = Axis.Uniform, TRSType = TRSType.Scale }
				XCrusher = new FloatCrusher(BitPresets.Bits8, -1, 1, Axis.X, TRSType.Scale, true) { TRSType = TRSType.Scale, AccurateCenter = true, BitsDeterminedBy = BitsDeterminedBy.HalfFloat },
				YCrusher = new FloatCrusher(BitPresets.Bits8, -1, 1, Axis.Y, TRSType.Scale, true) { TRSType = TRSType.Scale, AccurateCenter = true, BitsDeterminedBy = BitsDeterminedBy.HalfFloat },
				ZCrusher = new FloatCrusher(BitPresets.Bits8, -1, 1, Axis.Z, TRSType.Scale, true) { TRSType = TRSType.Scale, AccurateCenter = true, BitsDeterminedBy = BitsDeterminedBy.HalfFloat }
			}
		};

		#endregion

		private bool hasTeleported;
		public bool HasTeleported { set { hasTeleported = value; } }

		private Rigidbody rb;

		protected override void Reset()
		{
			base.Reset();
			/// Default TransformSync to happen before AnimatorSync
			_applyOrder = 3;
			/// Set the crushers local setting based on if this is root or a child
			bool isChild = transform.parent != null;
			transformCrusher.PosCrusher.local = (isChild);
			transformCrusher.RotCrusher.local = (isChild);
			transformCrusher.RotCrusher.TRSType = TRSType.Euler;
			transformCrusher.SclCrusher.local = true;
		}

		protected override void Awake()
		{
			base.Awake();
			rb = GetComponent<Rigidbody>();

			for (int i = 0; i < FRAME_CNT + 1; ++i)
				frames[i] = new Frame(this, i);

			teleportThresholdSqrMag = teleportThreshold * teleportThreshold;

			ConnectSharedCaches();
		}

		private void ConnectSharedCaches()
		{
			if (masterSharedCrushers.ContainsKey(prefabInstanceId))
				transformCrusher = masterSharedCrushers[prefabInstanceId];
			else
				masterSharedCrushers.Add(prefabInstanceId, transformCrusher);
		}

		/// Start is here to give devs the enabled checkbox
		private void Start()
		{
			if (autoKinematic && rb)
			{
				rbDefaultKinematic = rb.isKinematic;
				rbDefaultInterp = rb.interpolation;
				OnChangeAuthority(noa.IsMine);
			}
		}

		private bool rbDefaultKinematic;
		private RigidbodyInterpolation rbDefaultInterp;

		public void OnChangeAuthority(bool isMine)
		{
			if (autoKinematic && rb)
			{
				rb.isKinematic = (isMine) ? rbDefaultKinematic : true;
				rb.interpolation = (isMine) ? rbDefaultInterp : RigidbodyInterpolation.Interpolate;
			}
		}

		public class Frame : FrameBase
		{
			public bool hasTeleported;
			public Matrix m;
			public CompressedMatrix cm;
			public SimpleSyncTransform owner;
			//public Vector3 velocity;
			//public CompressedElement cVelocity = new CompressedElement();
			//public Vector3 anglrVel;
			//public CompressedElement cAnglrVel = new CompressedElement();

			public Frame(SimpleSyncTransform sst, int frameId) : base(frameId)
			{
				m = new Matrix();
				cm = new CompressedMatrix();
				sst.transformCrusher.Capture(sst.transform, cm, m);
			}

			public override void CopyFrom(FrameBase sourceFrame)
			{
				Frame src = sourceFrame as Frame;
				m.CopyFrom(src.m);
				cm.CopyFrom(src.cm);
				hasTeleported = src.hasTeleported;
				//velocity = src.velocity;
				//anglrVel = src.anglrVel;
				//cVelocity.CopyFrom(src.cVelocity);
				//cAnglrVel.CopyFrom(src.cAnglrVel);
			}

			public override bool Compare(FrameBase frame, FrameBase holdframe)
			{
				throw new System.NotImplementedException();
			}

			//static readonly StringBuilder strb = new StringBuilder();
			/// <summary>
			/// Compares only the compressed values for equality
			/// </summary>
			public bool FastCompareCompressed(Frame other)
			{
				bool match = cm.Equals(other.cm)
					 //&& cVelocity.Equals(other.cVelocity)
					 // && cAnglrVel.Equals(other.cAnglrVel)
					 ;

				if (match)
					return true;

				return false;
			}
			/// <summary>
			/// Compares only the compressed values for equality
			/// </summary>
			public bool FastCompareUncompressed(Frame other)
			{
				return
					m.position == other.m.position &&
					m.rotation == other.m.rotation &&
					m.scale == other.m.scale
					//&& velocity == other.velocity
					//&& anglrVel == other.anglrVel
					;
			}

			public override string ToString()
			{
				return "[" + frameId + " " + m.position + " / " + m.rotation + "]";
			}
		}

		public bool OnNetSerialize(int frameId, byte[] buffer, ref int bitposition)
		{
			/// initialization
			if (frameId == -1)
			{
				var offtickFrame = frames[FRAME_CNT];
				OnCaptureCurrentValues(FRAME_CNT, true, Realm.Primary);

				buffer.WriteBool(true, ref bitposition);
				buffer.WriteBool(offtickFrame.hasTeleported, ref bitposition);
				transformCrusher.Write(offtickFrame.cm, buffer, ref bitposition);
				transformCrusher.Decompress(offtickFrame.m, offtickFrame.cm);

				prevSentFrame = offtickFrame;
				hasInitialization = true;
				return true;
			}

			/// Don't transmit data if this component is disabled. Allows for muting components
			/// Simply by disabling them at the authority side.
			if (!enabled)
			{
				buffer.WriteBool(false, ref bitposition);
				return false;
			}
			
			Frame frame = frames[frameId];
			bool iskeyframe = (keyframeRate != 0) && (frameId % keyframeRate == 0);

			bool hascontent;

			/// Only check for changes if we aren't forced to send by a keyframe.
			if (!iskeyframe)
			{
				hascontent = prevSentFrame == null || !frame.cm.Equals(prevSentFrame.cm);
				if (!hascontent)
				{
					buffer.WriteBool(false, ref bitposition);
					return false;
				}
			}

			buffer.WriteBool(true, ref bitposition);
			buffer.WriteBool(frame.hasTeleported, ref bitposition);
			transformCrusher.Write(frame.cm, buffer, ref bitposition);
			transformCrusher.Decompress(frame.m, frame.cm);
			prevSentFrame = frame;
			return true;
		}

		public Frame prevSentFrame;

		public void OnNetDeserialize(int sourceFrameId, int originFrameId, int localFrameId, byte[] buffer, ref int bitposition)
		{
			/// initialization
			if (!hasInitialization || localFrameId == FRAME_CNT)
			{

				if (buffer.ReadBool(ref bitposition))
				{
					var offsetF = frames[localFrameId];
					offsetF.hasTeleported = buffer.ReadBool(ref bitposition);
					transformCrusher.Read(offsetF.cm, buffer, ref bitposition);
					transformCrusher.Decompress(offsetF.m, offsetF.cm);
					transformCrusher.Apply(transform, offsetF.m);
					offsetF.hasChanged = true;
					hasInitialization = true;
				}
				return;
			}

			Frame frame = frames[localFrameId];

			/// If enabled flag is false, we are done here.
			if (!buffer.ReadBool(ref bitposition))
			{
				frame.hasChanged = false;
				return;
			}

			frame.hasChanged = true;
			frame.hasTeleported = buffer.ReadBool(ref bitposition);
			transformCrusher.Read(frame.cm, buffer, ref bitposition);
			transformCrusher.Decompress(frame.m, frame.cm);
		}
		
		public void OnCaptureCurrentValues(int frameId, bool amActingAuthority, Realm realm)
		{
			Frame frame = frames[frameId];

			if (rb)
			{
				//Rigidbody realmRb = realm == Realm.Primary ? rb : null;
				transformCrusher.Capture(rb, frame.cm, frame.m);
			}
			else
			{
				transformCrusher.Capture(transform, frame.cm, frame.m);
			}

			frame.hasTeleported = hasTeleported;
			hasTeleported = false;
		}

		public override void OnSnapshot(int newTargetFrameId, bool isActingAuthority, bool initialize)
		{
			if (!enabled /*|| !hasInitialization*/)
				return;

			base.OnSnapshot(newTargetFrameId, isActingAuthority, initialize);

			/// Was this an authority side teleport?
			if (targF.hasTeleported)
			{
				hasTeleported = true;
			}
			/// is new target far enough from snapshot to force a teleport?
			else if (Vector3.SqrMagnitude(targF.m.position - snapF.m.position) > teleportThresholdSqrMag)
			{
				hasTeleported = true;
#if UNITY_EDITOR
				Debug.LogWarning(name + " teleportThreshold distance exceeded. Teleport Distance: " + Vector3.Distance(targF.m.position, snapF.m.position) + " / " + teleportThreshold
					+ "  sqrmag: " + Vector3.SqrMagnitude(targF.m.position - snapF.m.position) + " / " + teleportThresholdSqrMag + " " + targF.m.position + " " + snapF.m.position);
#endif
			}
			else
			{
				hasTeleported = false;
			}

			/// Apply teleport if called for.
			if (hasTeleported)
			{
				if (rb)
					transformCrusher.Set(rb, targF.m);
				else
					transformCrusher.Apply(transform, targF.m);
			}
			else
			{
				if (rb)
					transformCrusher.Set(rb, snapF.m);
				else
					transformCrusher.Apply(transform, snapF.m);
			}
		}

		public void OnInterpolate(float t)
		{
			if (!enabled || !hasInitialization)
				return;

			if (hasTeleported)
				return;
			
			if (interpolation == Interpolation.None)
				return;

			//if (rb)
			//	return;

			if (ReferenceEquals(targF, null))
				return;

			if (!targF.hasChanged)
				return;

			if (targF.hasTeleported)
				return;

			if (interpolation == Interpolation.Linear)
				Matrix.Lerp(Matrix.reusable, snapF.m, targF.m, t);
			else
				Matrix.CatmullRomLerpUnclamped(Matrix.reusable, pre1F.m, snapF.m, targF.m, t);

			transformCrusher.Apply(transform, Matrix.reusable);
		}

		protected override void Interpolate(Frame targ, Frame start, Frame end, float t)
		{
			Matrix.Lerp(targ.m, start.m, end.m, t);
		}

		protected override void Extrapolate()
		{
			Matrix.LerpUnclamped(targF.m, pre1F.m, snapF.m, 1 + extrapolateRatio);
			transformCrusher.Compress(targF.cm, targF.m);
		}
	}
}

