using System;
using UnityEngine;
using com.ootii.Actors;
using com.ootii.Actors.AnimationControllers;
using com.ootii.Base;
using com.ootii.Helpers;
using com.ootii.Messages;
using UnityEngine.AI;
using com.ootii.Actors.LifeCores;

namespace com.ootii.Reactors
{
    // CDL 07/01/2018 - this reactor temporarily disables the UseTransform option to allow an animation that 
    // uses root motion to move the character
    [Serializable]
    [BaseName("Agent Set Use Transform")]
    [BaseDescription("When placed on an AI controlled character with Use Transform enabled on the Actor Controller, "
        + "this reactor will disable Use Transform when a motion tagged to use root motion data is activated. "
        + "This is intended for use with discrete 'action' motions such as attack, damaged, etc; it is not for walk/run motions that "
        + "use root motion data.")]
    public class AgentSetUseTransform : ReactorAction
    {
        public string _RootMotionTag = DefaultMotionTags.UseRootMotion;
        public string RootMotionTag
        {
            get { return _RootMotionTag; }
            set { _RootMotionTag = value; }
        }

        public bool _ManageNavMeshagent = false;
        public bool ManageNavMeshAgent
        {
            get { return _ManageNavMeshagent; }
            set { _ManageNavMeshagent = value; }
        }

        // We check when initializing what Use Transform was set to; this way we don't enable it on motion deactivation
        // if it was intentionally disabled but this reactor was left active
        protected bool mInitialUseTransform = false;

        // Component references        
        protected ActorController mActorController = null;        
        protected NavMeshAgent mNavMeshAgent = null;       
        protected ActorCore mActorCore = null;

        protected bool mUpdateRotationSetting;
        protected bool mUpdatePositionSetting;
        
        public AgentSetUseTransform()
        {
            _ActivationType = 0;
        }

        public AgentSetUseTransform(GameObject rOwner) : base(rOwner)
        {
            _ActivationType = 0;

            if (mActorCore == null) mActorCore = rOwner.GetComponent<ActorCore>();
            if (mActorController == null) mActorController = rOwner.GetComponent<ActorController>();
            if (mNavMeshAgent == null) mNavMeshAgent = rOwner.GetComponent<NavMeshAgent>();
        }

        public override void Awake()
        {
            if (mOwner != null)
            {
                if (mActorCore == null) mActorCore = mOwner.GetComponent<ActorCore>();
                if (mActorController == null) mActorController = mOwner.GetComponent<ActorController>();
                if (mNavMeshAgent == null) mNavMeshAgent = mOwner.GetComponent<NavMeshAgent>();

                mInitialUseTransform = mActorController.UseTransformPosition;
            }
        }

        /// <summary>
        /// Used to test if the reactor should process
        /// </summary>
        /// <returns></returns>
        public override bool TestActivate(IMessage rMessage)
        {
            // if the ActorController didn't start off set to Use Transform, then do nothing
            if (!mInitialUseTransform) { return false; }
            if (!base.TestActivate(rMessage)) { return false; }
            if (mActorCore == null || !mActorCore.IsAlive) { return false; }

            MotionMessage lMessage = rMessage as MotionMessage;

            // We're only responding to Activated and Deactivated Motion events
            if (lMessage == null || lMessage.Motion == null ||
                (lMessage.ID == EnumMessageID.MSG_MOTION_ACTIVATE
                 && lMessage.ID == EnumMessageID.MSG_MOTION_DEACTIVATE))
            {
                return false;
            }

            // For certain motion types, we want to disable Use Transform
            // on the agent's ActorController so that the root motion data
            // can move the character. Only on layer 0.
            if (lMessage.Motion.MotionLayer.Index != 0) { return false; }

            // Only process motions that are correctly tagged
            if (!lMessage.Motion.TagExists(_RootMotionTag)) { return false; }

            mMessage = lMessage;
            return true;
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
                if (mActorController.UseTransformPosition) SetUseTransform(false);
            }
            // We want to set the hand pose motion when the "Equip" motion deactivates (after the animation is finished) so that 
            // the hand doesn't close too early
            else if (lMessage.ID == EnumMessageID.MSG_MOTION_DEACTIVATE)
            {
                if (!mActorController.UseTransformPosition) SetUseTransform(true);
            }

            // Disable the reactor
            Deactivate();

            // Allow other reactors to continue
            return true;
        }

        protected void SetUseTransform(bool rEnable)
        {          
            mActorController.UseTransformPosition = rEnable;

            if (!_ManageNavMeshagent || mNavMeshAgent == null) { return; }

            //mNavMeshAgent.isStopped = !rEnable;

            // We want to stop the NavMeshAgent from updating position and rotation when playing an animation that uses root motion data 
            // This applies to animations that represent discrete actions: attack, dodge, damage reaction, death, etc. The WalkRunX motions
            // handle applying root motion while pathfinding.
            if (rEnable)
            {
                // Force the NavMeshAgent to the same position as the character, as the character will have moved               
                mNavMeshAgent.nextPosition = mOwner.transform.position;

                // Restore the agent's update setttings as they were before
                mNavMeshAgent.updatePosition = mUpdatePositionSetting;
                mNavMeshAgent.updateRotation = mUpdateRotationSetting;
            }
            else
            {
                mUpdatePositionSetting = mNavMeshAgent.updatePosition;
                mUpdateRotationSetting = mNavMeshAgent.updateRotation;

                mNavMeshAgent.updatePosition = false;
                mNavMeshAgent.updateRotation = false;
            }
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

            if (EditorHelper.TextField("Root Motion Tag", "When a motion with this tag is activated, we will disable Use Transform.", RootMotionTag, rTarget))
            {
                lIsDirty = true;
                RootMotionTag = EditorHelper.FieldStringValue;
            }

            if (EditorHelper.BoolField("Manage NavMesh Agent", "Set NavMeshAgent.isStopped when enabling or disabling Use Transform.", ManageNavMeshAgent, rTarget))
            {
                lIsDirty = true;
                ManageNavMeshAgent = EditorHelper.FieldBoolValue;
            }

            return lIsDirty;
        }

#endif

        #endregion
    }

}