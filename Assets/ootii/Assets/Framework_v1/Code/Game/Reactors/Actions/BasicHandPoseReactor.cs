using System;
using UnityEngine;
using com.ootii.Actors.AnimationControllers;
using com.ootii.Actors.LifeCores;
using com.ootii.Base;
using com.ootii.Helpers;
using com.ootii.Messages;
using com.ootii.MotionControllerPacks;

namespace com.ootii.Reactors
{
    // CDL - 07/25/2018 - added the Basic Hand Pose motion and reactor from my motion packs. These allow the character's hand(s)
    // to stay closed around a weapon's grip when playing animations that aren't part of that weapon set (Jump, Climb, etc)
    [Serializable]
    [BaseName("Hand Pose Reactor")]
    [BaseDescription("Ensures the character has the correct hand pose as weapons are equipped and stored")]
    public class BasicHandPoseReactor : ReactorAction
    {        
        public string _RightHandPoseName = "Right Hand Pose";
        public string RightHandPoseName
        {
            get { return _RightHandPoseName; }
            set { _RightHandPoseName = value; }
        }

        public string _LeftHandPoseName = "Left Hand Pose";
        public string LeftHandPoseName
        {
            get { return _LeftHandPoseName; }
            set { _LeftHandPoseName = value; }
        }

        public string _EquipMotionTag = DefaultMotionTags.EquipMotion;
        public string EquipMotionTag
        {
            get { return _EquipMotionTag; }
            set { _EquipMotionTag = value; }
        }

        public string _StoreMotionTag = DefaultMotionTags.StoreMotion;
        public string StoreMotionTag
        {
            get { return _StoreMotionTag; }
            set { _StoreMotionTag = value; }
        }



        // The Hand Pose motions
        [NonSerialized]
        protected BasicHandPose mRightHandPose = null;

        [NonSerialized]
        protected BasicHandPose mLeftHandPose = null;

        // Component references
        [NonSerialized]
        protected ActorCore mActorCore = null;

        [NonSerialized]
        protected MotionController mMotionController = null;


        public BasicHandPoseReactor()
        {
            _ActivationType = 0;
        }

        public BasicHandPoseReactor(GameObject rOwner) : base(rOwner)
        {
            _ActivationType = 0;
            mActorCore = rOwner.GetComponent<ActorCore>();
            mMotionController = rOwner.GetComponent<MotionController>();
        }

        /// <summary>
        /// Initialize the reactor
        /// </summary>
        public override void Awake()
        {
            if (mOwner != null)
            {
                mActorCore = mOwner.GetComponent<ActorCore>();
                mMotionController = mOwner.GetComponent<MotionController>();
            }
        }

        /// <summary>
        /// Used to test if the reactor should process
        /// </summary>
        /// <returns></returns>
        public override bool TestActivate(IMessage rMessage)
        {
            if (!base.TestActivate(rMessage)) { return false; }
            if (mActorCore == null || !mActorCore.IsAlive) { return false; }

            MotionMessage lMessage = rMessage as MotionMessage;
            if (lMessage == null || lMessage.Motion == null) { return false; }

            // We want to activate or deactivate the hand pose animator states is response to
            // motions being activated or deactivated
            if (lMessage.ID == EnumMessageID.MSG_MOTION_ACTIVATE
                || lMessage.ID == EnumMessageID.MSG_MOTION_DEACTIVATE)
            {
                mMessage = lMessage;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Called when the reactor is first activated
        /// </summary>
        /// <returns>Determines if other reactors should process.</returns>
        public override bool Activate()
        {
            base.Activate();

            MotionMessage lMessage = (MotionMessage)mMessage;

            // We want to release the hand pose motion when the "Store" motion is activated (otherwise the hand will stay closed
            // until the animation has completed)
            if (lMessage.ID == EnumMessageID.MSG_MOTION_ACTIVATE)
            {
                if (lMessage.Motion is IStoreMotion || lMessage.Motion.TagExists(_StoreMotionTag))
                {
                    if (mRightHandPose == null) mRightHandPose = mMotionController.GetMotion(EnumMotionLayer.RIGHT_HAND, RightHandPoseName) as BasicHandPose;
                    if (mLeftHandPose == null) mLeftHandPose = mMotionController.GetMotion(EnumMotionLayer.LEFT_HAND, LeftHandPoseName) as BasicHandPose;

                    ReleaseHandPose(mRightHandPose);
                    ReleaseHandPose(mLeftHandPose);
                }
            }
            // We want to set the hand pose motion when the "Equip" motion deactivates (after the animation is finished) so that 
            // the hand doesn't close too early
            else if (lMessage.ID == EnumMessageID.MSG_MOTION_DEACTIVATE)
            {
                if (lMessage.Motion is IEquipMotion || lMessage.Motion.TagExists(_EquipMotionTag))
                {
                    if (mRightHandPose == null) mRightHandPose = mMotionController.GetMotion(EnumMotionLayer.RIGHT_HAND, RightHandPoseName) as BasicHandPose;
                    if (mLeftHandPose == null) mLeftHandPose = mMotionController.GetMotion(EnumMotionLayer.LEFT_HAND, LeftHandPoseName) as BasicHandPose;

                    // TODO: check for two-handed weapon, as we don't want the left hand to stay closed ... or the LH pose state may simply be Empty
                    // May need to use a ReleaseOffHandPose tag for motions like Jump where one hand will come off the weapon. 
                    SetHandPose(mRightHandPose);
                    SetHandPose(mLeftHandPose);
                }
            }


            // Disable the reactor
            Deactivate();

            // Allow other reactors to continue
            return true;
        }

        /// <summary>
        /// Called when the reactor is meant to be deactivated
        /// </summary>
        public override void Deactivate()
        {
            base.Deactivate();

            mMessage = null;
        }

        /// <summary>
        /// Activate the hand pose motion
        /// </summary>
        /// <param name="rHandPose"></param>
        protected void SetHandPose(BasicHandPose rHandPose)
        {
            if (rHandPose == null) return;

            //Debug.Log("[BasicHandPoseReactor] Setting " + rHandPose.Name);
            mMotionController.ActivateMotion(rHandPose);
        }

        /// <summary>
        /// Deactivate the hand pose motion
        /// </summary>
        /// <param name="rHandPose"></param>
        protected void ReleaseHandPose(BasicHandPose rHandPose)
        {
            if (rHandPose == null || !rHandPose.IsActive) return;

            //Debug.Log("[BasicHandPoseReactor] Releasing " + rHandPose.Name);
            rHandPose.Deactivate();
        }

        #region Editor Functions

#if UNITY_EDITOR

        /// <summary>
        /// Called when the inspector needs to draw
        /// </summary>
        public override bool OnInspectorGUI(UnityEditor.SerializedObject rTargetSO, UnityEngine.Object rTarget)
        {
            _EditorShowActivationType = false;
            bool lIsDirty = base.OnInspectorGUI(rTargetSO, rTarget);

            if (EditorHelper.TextField("Right Hand Pose Name", "The name of the BasicHandPose motion for the right hand.", RightHandPoseName, rTarget))
            {
                lIsDirty = true;
                RightHandPoseName = EditorHelper.FieldStringValue;
            }

            if (EditorHelper.TextField("Left Hand Pose Name", "The name of the BasicHandPose motion for the left hand.", LeftHandPoseName, rTarget))
            {
                lIsDirty = true;
                LeftHandPoseName = EditorHelper.FieldStringValue;
            }

            GUILayout.Space(5f);

            if (EditorHelper.TextField("Equip Motion Tag", "Set the hand pose when a motion with this tag is activated.", EquipMotionTag, rTarget))
            {
                lIsDirty = true;
                EquipMotionTag = EditorHelper.FieldStringValue;
            }

            if (EditorHelper.TextField("Store Motion Tag", "Release the hand pose when a motion with this tag is activated.", StoreMotionTag, rTarget))
            {
                lIsDirty = true;
                StoreMotionTag = EditorHelper.FieldStringValue;
            }

            return lIsDirty;
        }

#endif

        #endregion
    }
}
