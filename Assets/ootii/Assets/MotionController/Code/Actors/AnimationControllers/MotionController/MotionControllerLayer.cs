﻿using System;
using System.Collections.Generic;
using UnityEngine;
using com.ootii.Base;
using com.ootii.Data.Serializers;
using com.ootii.Helpers;
using com.ootii.Messages;
using com.ootii.Utilities;

namespace com.ootii.Actors.AnimationControllers
{
    /// <summary>
    /// Controller layers are used to group motions together that
    /// typically don't run at the same time. For example, running,
    /// jumping, and climbing don't happen at the exact same time.
    /// However, someone may be running and swing a sword. Each motion
    /// would be in a seperate layer.
    /// </summary>
    [Serializable]
    public class MotionControllerLayer : BaseObject
    {
        /// <summary>
        /// Index in the list of motion layers that this layer represents.
        /// </summary>
        private int mIndex = 0;
        public int Index
        {
            get { return mIndex; }
            set { mIndex = value; }
        }

        /// <summary>
        /// Controller this motion is tried to
        /// </summary>
        private MotionController mMotionController;
        public MotionController MotionController
        {
            get { return mMotionController; }
            
            set 
            { 
                mMotionController = value;

                for (int i = 0; i < Motions.Count; i++)
                {
                    Motions[i].MotionController = mMotionController;
                }
            }
        }

        /// <summary>
        /// Determines if this layer will ignore motions that try to 
        /// override the later layers.
        /// </summary>
        public bool _IgnoreMotionOverride = false;
        public bool IgnoreMotionOverride
        {
            get { return _IgnoreMotionOverride; }
            set { _IgnoreMotionOverride = value; }
        }

        /// <summary>
        /// Determines the index of the layer in the mechanim animator
        /// where the corresponding layer lies.
        /// </summary>
        public int _AnimatorLayerIndex = 0;
        public int AnimatorLayerIndex
        {
            get { return _AnimatorLayerIndex; }

            set
            {
                _AnimatorLayerIndex = value;

                if (mMotionController != null && mMotionController.Animator != null)
                {
                    mAnimatorLayerName = mMotionController.Animator.GetLayerName(_AnimatorLayerIndex);
                }
            }
        }

        /// <summary>
        /// Current animator state ID that the motion is in.
        /// </summary>
        [NonSerialized]
        public int _AnimatorStateID = 0;
        public int AnimatorStateID
        {
            get { return _AnimatorStateID; }
        }

        /// <summary>
        /// Current animator transition ID that themotion is in.
        /// </summary>
        [NonSerialized]
        public int _AnimatorTransitionID = 0;
        public int AnimatorTransitionID
        {
            get { return _AnimatorTransitionID; }
        }

        /// <summary>
        /// Current animator state ID that the motion is in.
        /// </summary>
        [NonSerialized]
        public float _AnimatorStateNormalizedTime = 0;
        public float AnimatorStateNormalizedTime
        {
            get { return _AnimatorStateNormalizedTime; }
        }

        /// <summary>
        /// Current animator transition ID that themotion is in.
        /// </summary>
        [NonSerialized]
        public float _AnimatorTransitionNormalizedTime = 0;
        public float AnimatorTransitionNormalizedTime
        {
            get { return _AnimatorTransitionNormalizedTime; }
        }

        /// <summary>
        /// List of motions the controller manages
        /// </summary>
        public List<MotionControllerMotion> Motions = new List<MotionControllerMotion>();
        //public List<MotionControllerMotion> Motions
        //{
        //    get { return mMotions; }
        //    set { mMotions = value; }
        //}

        /// <summary>
        /// TODO
        /// </summary>
        public List<string> MotionDefinitions = new List<string>();

        /// <summary>
        /// Name of the animator layer this layer is associated with
        /// </summary>
        private string mAnimatorLayerName = "";
        public string AnimatorLayerName
        {
            get { return mAnimatorLayerName; }
        }

        /// <summary>
        /// The motion that is running and has top priority.
        /// While we could actually be running multiple motions,
        /// this represents the 'primary' one.
        /// </summary>
        private MotionControllerMotion mActiveMotion;
        public MotionControllerMotion ActiveMotion
        {
            get { return mActiveMotion; }
        }

        /// <summary>
        /// The motion that is was running prior to the current active motion.
        /// </summary>
        private MotionControllerMotion mPrevActiveMotion;
        public MotionControllerMotion PrevActiveMotion
        {
            get { return mPrevActiveMotion; }
        }

        /// <summary>
        /// Time in seconds that the active motion has been running for
        /// </summary>
        private float mActiveMotionDuration = 0f;
        public float ActiveMotionDuration
        {
            get { return mActiveMotionDuration; }
        }

        /// <summary>
        /// Returns phase of the motion that is currently
        /// running. While we could actually be running multiple
        /// motions, this represents the primary one.
        /// </summary>
        public int ActiveMotionPhase
        {
            get
            {
                if (mActiveMotion != null) { return mActiveMotion.Phase; }
                return 0;
            }
        }

        /// <summary>
        /// Current velocity caused by the motion. This should be
        /// multiplied by delta-time to create displacement
        /// </summary>
        private Vector3 mVelocity = Vector3.zero;
        public Vector3 Velocity
        {
            get { return mVelocity; }
        }

        /// <summary>
        /// Current movement caused by the motion. This should NOT be
        /// multiplied by delta-time to create displacement
        /// </summary>
        private Vector3 mMovement = Vector3.zero;
        public Vector3 Movement
        {
            get { return mMovement; }
        }

        /// <summary>
        /// Amount of rotation caused by the motion. This should be
        /// multiplied by delta-time to create angular displacement
        /// </summary>
        private Vector3 mAngularVelocity = Vector3.zero;
        public Vector3 AngularVelocity
        {
            get { return mAngularVelocity; }
        }

        /// <summary>
        /// Amount of rotation caused by the motion. This should NOT be
        /// multiplied by delta-time to create angular displacement
        /// </summary>
        private Quaternion mRotation = Quaternion.identity;
        public Quaternion Rotation
        {
            get { return mRotation; }
        }

        /// <summary>
        /// Amount of rotation caused by the motion. This should NOT be
        /// multiplied by delta-time to create angular displacement
        /// </summary>
        private Quaternion mTilt = Quaternion.identity;
        public Quaternion Tilt
        {
            get { return mTilt; }
        }

        ///// <summary>
        ///// Determine if the layer motion is disabling gravity
        ///// </summary>
        //public bool IsGravityEnabled
        //{
        //    get
        //    {
        //        bool lResult = true;
        //        if (mActiveMotion != null) { lResult = mActiveMotion.IsGravityEnabled; }

        //        return lResult;
        //    }
        //}

        /// <summary>
        /// Determines if we use trend data when sending speed data
        /// to the animator
        /// </summary>
        public bool UseTrendData
        {
            get
            {
                bool lResult = true;
                if (mActiveMotion != null) { lResult = mActiveMotion.UseTrendData; }

                return lResult;
            }
        }

        ///// <summary>
        ///// Returns any camera offsets generated by this layer
        ///// </summary>
        //public Vector3 CameraOffset
        //{
        //    get
        //    {
        //        Vector3 lResult = Vector3.zero;
        //        if (mActiveMotion != null) { lResult = mActiveMotion.RootMotionCameraOffset; }

        //        return lResult;
        //    }
        //}

        /// <summary>
        /// Determines if we'll run the DeactivatedUpdate of the previous motion
        /// </summary>
        protected bool mRunDeactivatedUpdate = false;

        /// <summary>
        /// Default constructor
        /// </summary>
        public MotionControllerLayer() 
            : base()
        {
        }

        /// <summary>
        /// Controller constructor
        /// </summary>
        /// <param name="rController">Controller the layer is tied to</param>
        public MotionControllerLayer(MotionController rController) 
            : base()
        {
            mMotionController = rController;
        }

        /// <summary>
        /// Controller constructor
        /// </summary>
        /// <param name="rController">Controller the layer is tied to</param>
        public MotionControllerLayer(string rName, MotionController rController)
            : base()
        {
            _Name = rName;
            mMotionController = rController;
        }

        /// <summary>
        /// Awake is called after all objects are initialized so you can safely speak to other objects. This is where
        /// reference can be associated.
        /// </summary>
        public void Awake()
        {
            if (mMotionController != null && mMotionController.Animator != null)
            {
                mAnimatorLayerName = mMotionController.Animator.GetLayerName(_AnimatorLayerIndex);
            }

            for (int i = 0; i < Motions.Count; i++)
            {
                Motions[i].MotionLayer = this;
                Motions[i].Awake();
            }
        }

        /// <summary>
        /// Adds a motion to the list of motions being managed
        /// </summary>
        /// <param name="rMotion">Motion to add</param>
        public void AddMotion(MotionControllerMotion rMotion)
        {
            if (!Motions.Contains(rMotion))
            {
                rMotion.MotionController = mMotionController;
                rMotion.MotionLayer = this;

                Motions.Add(rMotion);
            }
        }

        /// <summary>
        /// Removes the motion from the list of motions being managed
        /// </summary>
        /// <param name="rMotion">Motion to remove</param>
        public void RemoveMotion(MotionControllerMotion rMotion)
        {
            Motions.Remove(rMotion);

            rMotion.MotionController = null;
            rMotion.MotionLayer = null;
        }

        /// <summary>
        /// Attempt to activate the specified motion and make it the 
        /// active motion.
        /// </summary>
        /// <param name="rMotion">Motion to activate</param>
        /// <returns>Determines if the motion was set as the active motion</returns>
        public bool QueueMotion(MotionControllerMotion rMotion, int rParameter = 0)
        {
            if (!Motions.Contains(rMotion)) { return false; }
            if (mActiveMotion == rMotion && mActiveMotion.IsActive) { return false; }

            rMotion.Parameter = rParameter;
            rMotion.QueueActivation = true;
            return true;
        }

        /// <summary>
        /// Load the animator state and transition IDs
        /// </summary>
        public void LoadAnimatorData()
        {
            // Create the motions to match the defintions
            InstanciateMotions();

            // Allow the motions to load thier data
            int lMotionCount = Motions.Count;
            for (int i = 0; i < lMotionCount; i++)
            {
                Motions[i].LoadAnimatorData();
            }
        }

        /// <summary>
        /// This function is called every fixed framerate frame, if the MonoBehaviour is enabled.
        /// </summary>
        /// <param name="rDeltaTime">Time since the last frame (or fixed update call)</param>
        public void FixedUpdateMotions(float rDeltaTime)
        {
            if (mActiveMotion != null)
            {
                mActiveMotion.FixedUpdate(rDeltaTime);
            }
        }

        /// <summary>
        /// Allows the motion to modify the velocity before it is applied.
        /// </summary>
        /// <param name="rDeltaTime">Time since the last frame (or fixed update call)</param>
        /// <param name="rUpdateIndex">Index of the update to help manage dynamic/fixed updates. [0: Invalid update, >=1: Valid update]</param>
        /// <param name="rMovement">Amount of movement caused by root motion this frame</param>
        /// <param name="rRotation">Amount of rotation caused by root motion this frame</param>
        /// <returns></returns>
        public void UpdateRootMotion(float rDeltaTime, int rUpdateIndex, ref Vector3 rMovement, ref Quaternion rRotation)
        {
            // Check the motions to determine if we should remove the root motion.
            // If even one active motion wants it removed, remove it.
            if (mActiveMotion != null)
            {
                mActiveMotion.UpdateRootMotion(rDeltaTime, rUpdateIndex, ref rMovement, ref rRotation);
            }
        }

        /// <summary>
        /// Updates the motions tied to this layer at the variable
        /// time step (ie Update() not FixedUpdate().
        /// </summary>
        /// <param name="rDeltaTime">Time since the last frame (or fixed update call)</param>
        /// <param name="rUpdateIndex">Index of the update to help manage dynamic/fixed updates. [0: Invalid update, >=1: Valid update]</param>
        public void UpdateMotions(float rDeltaTime, int rUpdateIndex)
        {
            int lPriorityIndex = -1;
            float lPriorityValue = float.MinValue;
            MotionControllerMotion lPrevActiveMotion = mActiveMotion;

            // Grab the current state info for the layer
            _AnimatorStateID = mMotionController.State.AnimatorStates[_AnimatorLayerIndex].StateInfo.fullPathHash;
            _AnimatorStateNormalizedTime = mMotionController.State.AnimatorStates[_AnimatorLayerIndex].StateInfo.normalizedTime;
            _AnimatorTransitionID = mMotionController.State.AnimatorStates[_AnimatorLayerIndex].TransitionInfo.fullPathHash;
            _AnimatorTransitionNormalizedTime = mMotionController.State.AnimatorStates[_AnimatorLayerIndex].TransitionInfo.normalizedTime;

            // Track how long the motion has been playing
            mActiveMotionDuration += Time.deltaTime;

            // Clean up the active motion's flag and ensure it's still valid
            if (mActiveMotion != null)
            {
                mActiveMotion.IsActivatedFrame = false;

                // If we have a current motion, test if it can continue
                if (!mActiveMotion.TestUpdate())
                {
                    mRunDeactivatedUpdate = true;
                    mPrevActiveMotion = mActiveMotion;

                    mActiveMotion.Deactivate();
                    mActiveMotion = null;
                }
            }

            // First, check if our current motion is interruptible.
            // If it's not, we know we are simply running it.
            if (rUpdateIndex == 1 && (mActiveMotion == null || mActiveMotion.IsInterruptible))
            {
                bool lIsQueued = false;

                // Cycle through the motions to determine which ones were not
                // active and should be. We'll take the motion with the highest priority
                for (int i = 0; i < Motions.Count; i++)
                {
                    MotionControllerMotion lMotion = Motions[i];

                    // Clean up the activation flag
                    lMotion.IsActivatedFrame = false;

                    // Don't test if the motion is not enabled
                    if (!lMotion.IsEnabled) { continue; }

                    // Don't reactivate this frame
                    if (Time.time - lMotion.DeactivationTime < 0.001f) { continue; }

                    // If we haven't gone past the reactivation delay, move one
                    if (lMotion.ReactivationDelay > 0f && (lMotion.DeactivationTime + lMotion.ReactivationDelay) > Time.time) { continue; }

                    // If we're to force the motion, don't check others
                    if (lMotion.QueueActivation && (mActiveMotion == null || mActiveMotion.TestInterruption(Motions[i])))
                    {
                        if (_AnimatorTransitionID == 0)
                        {
                            lIsQueued = true;

                            lPriorityIndex = i;
                            lPriorityValue = lMotion.Priority;

                            lMotion.QueueActivation = false;
                        }

                        break;
                    }
                    // If we're dealing with the current motion, test if we continue
                    else if (lMotion == mActiveMotion)
                    {
                        //if (mActiveMotion.TestUpdate())
                        //{
                            if (mActiveMotion.Priority >= lPriorityValue)
                            {
                                lPriorityIndex = i;
                                lPriorityValue = mActiveMotion.Priority;
                            }
                        //}
                        //else
                        //{
                        //    mActiveMotion.Deactivate();
                        //}
                    }
                    // For new motions, check for activation
                    else if (lMotion.IsStartable)
                    {
                        bool lActivate = lMotion.TestActivate();

                        // Test if we should activate the motion
                        if (lActivate && mMotionController.MotionActivatedEvent != null)
                        {
                            MotionMessage lMessage = MotionMessage.Allocate();
                            lMessage.ID = EnumMessageID.MSG_MOTION_TEST;
                            lMessage.Motion = lMotion;
                            lMessage.Continue = true;
                            lMessage.Data = lPrevActiveMotion;
                            mMotionController.MotionTestActivateEvent.Invoke(lMessage);

                            lActivate = lMessage.Continue;

                            MotionMessage.Release(lMessage);
                        }

                        // Test if we should activate the motion
                        if (lActivate && mMotionController._ActorCore != null)
                        {
                            MotionMessage lMessage = MotionMessage.Allocate();
                            lMessage.ID = EnumMessageID.MSG_MOTION_TEST;
                            lMessage.Motion = lMotion;
                            lMessage.Continue = true;
                            lMessage.Data = lPrevActiveMotion;
                            mMotionController._ActorCore.SendMessage(lMessage);

                            lActivate = lMessage.Continue;

                            MotionMessage.Release(lMessage);
                        }

//#if USE_MESSAGE_DISPATCHER || OOTII_MD
//                        // Send the message
//                        if (lActivate)
//                        {
//                            MotionMessage lMessage = MotionMessage.Allocate();
//                            lMessage.ID = EnumMessageID.MSG_MOTION_TEST;
//                            lMessage.Motion = lMotion;
//                            lMessage.Continue = true;
//                            lMessage.Data = lPrevActiveMotion;
//                            MessageDispatcher.SendMessage(lMessage);

//                            lActivate = lMessage.Continue;

//                            MotionMessage.Release(lMessage);
//                        }
//#endif

                        if (lActivate)
                        {
                            if (lMotion.Priority >= lPriorityValue)
                            {
                                lPriorityIndex = i;
                                lPriorityValue = lMotion.Priority;
                            }
                        }
                    }
                }

                // If we have a newly chosen motion, we need to activate it
                if (lPriorityIndex >= 0 && lPriorityIndex < Motions.Count)
                {
                    // Ensure our new motion is valid.
                    if (!lIsQueued && mActiveMotion != null)
                    {
                        // If the "new" motion is the current motion, move on
                        if (mActiveMotion == Motions[lPriorityIndex])
                        {
                            lPriorityIndex = -1;
                        }
                        // If the current motion has a higher priority (lower value), move on
                        else if (mActiveMotion.Priority > lPriorityValue)
                        {
                            lPriorityIndex = -1;
                        }
                        // If we can't interrupt the current motion, move on
                        else if (mActiveMotion.IsActive && !mActiveMotion.TestInterruption(Motions[lPriorityIndex]))
                        {
                            lPriorityIndex = -1;
                        }
                    }

                    // Look to start the new motion (if we have one)
                    if (lPriorityIndex >= 0)
                    {
                        if (mActiveMotion != null && mActiveMotion.IsActive && mActiveMotion != Motions[lPriorityIndex])
                        {
                            mRunDeactivatedUpdate = true;
                            mPrevActiveMotion = mActiveMotion;

                            mActiveMotion.Deactivate();
                        }

                        if (mActiveMotion != Motions[lPriorityIndex]) { Motions[lPriorityIndex].Activate(lPrevActiveMotion); }

                        mActiveMotion = Motions[lPriorityIndex];
                        mActiveMotionDuration = 0f;
                    }
                }
            }

            // Process any shutdown procedures for motions that are deactivated.
            if (mRunDeactivatedUpdate && mPrevActiveMotion != null)
            {
                mRunDeactivatedUpdate = mPrevActiveMotion.DeactivatedUpdate(rDeltaTime, rUpdateIndex);
            }

            // Process any motions that are active. They will die out on thier own
            for (int i = 0; i < Motions.Count; i++)
            {
                if (Motions[i].IsActive)
                {
                    Motions[i].UpdateMotion(rDeltaTime, rUpdateIndex);

                    // As a safetly, test and set the active motion
                    if (mActiveMotion == null && Motions[i].IsActive) 
                    {
                        mActiveMotion = Motions[i];
                        mActiveMotionDuration = 0f;
                    }
                }
            }

            // Check if we've deactivated the current motion. If so, we
            // need to remove our reference to it
            if (mActiveMotion != null && !mActiveMotion.IsActive)
            {
                mActiveMotion = null;
                mActiveMotionDuration = 0f;
            }

            // Calculate the velocities of the active motions
            mAngularVelocity = Vector3.zero;
            mRotation = Quaternion.identity;
            mTilt = Quaternion.identity;
            mVelocity = Vector3.zero;
            mMovement = Vector3.zero;

            for (int i = 0; i < Motions.Count; i++)
            {
                if (Motions[i].IsActive)
                {
                    mAngularVelocity += Motions[i].AngularVelocity;
                    mRotation = mRotation * Motions[i].Rotation;
                    mTilt = mTilt * Motions[i].Tilt;

                    mVelocity += Motions[i].Velocity;
                    mMovement += Motions[i].Movement;
                }
            }
        }

        ///// <summary>
        ///// Updates the motions tied to this layer at the end
        ///// of the update cycles (ie LateUpdate() not Update().
        ///// </summary>
        //public void LateUpdateMotions()
        //{
        //    // Process any motions that are active. They will die out on thier own
        //    for (int i = 0; i < Motions.Count; i++)
        //    {
        //        if (Motions[i].IsActive)
        //        {
        //            Motions[i].LateUpdateMotion();
        //        }
        //    }
        //}

        /// <summary>
        /// Raised when the animator's state has changed
        /// </summary>
        public void OnAnimatorStateChange(int rAnimatorLayer, int rLastStateID, int rNewStateID)
        {
            // Send the state change to all active motions
            for (int i = 0; i < Motions.Count; i++)
            {
                // We allow the motions to interrogate state changes.
                if (Motions[i].IsActive)
                {
                    Motions[i].OnAnimatorStateChange(rLastStateID, rNewStateID);
                }
            }
        }

        /// <summary>
        /// Raised by the animation when an event occurs
        /// </summary>
        public void OnAnimationEvent(AnimationEvent rEvent)
        {
            // Send the event to all active motions
            for (int i = 0; i < Motions.Count; i++)
            {
                if (Motions[i].IsActive)
                {
                    Motions[i].OnAnimationEvent(rEvent);
                }
            }
        }

        /// <summary>
        /// Raised by the controller when a message is received
        /// </summary>
        public void OnMessageReceived(IMessage rMessage)
        {
            // Send the event to all motions
            for (int i = 0; i < Motions.Count; i++)
            {
                if (Motions[i].IsEnabled)
                {
                    Motions[i].OnMessageReceived(rMessage);

                    if (rMessage.IsHandled) { break; }
                }
            }
        }

        /// <summary>
        /// Allow the layer to render debug info
        /// </summary>
        public void OnDrawGizmos()
        {
            //// Send the state change to all active motions
            //for (int i = 0; i < mMotions.Count; i++)
            //{
            //    if (mMotions[i].IsActive)
            //    {
            //        mMotions[i].OnDrawGizmos();
            //    }
            //}
        }

        /// <summary>
        /// Processes the motion definitions and updates the motions to match
        /// the definitions.
        /// </summary>
        public void InstanciateMotions()
        {
            if (mMotionController == null) { return; }

            int lMotionCount = Motions.Count;
            int lMotionDefCount = MotionDefinitions.Count;

            // First, remove any extra motions that may exist
            for (int i = lMotionCount - 1; i >= lMotionDefCount; i--)
            {
                Motions.RemoveAt(i);
            }

            // CDL 07/27/2018 - keep track of any invalid motion definitions            
            List<int> lInvalidDefinitions = new List<int>();

            // Initialize the root object so we can serialize transforms and game objects
            JSONSerializer.RootObject = mMotionController.gameObject;            
            // We need to match the motion definitions to the motions
            for (int i = 0; i < lMotionDefCount; i++)
            {
                string lDefinition = MotionDefinitions[i];
                JSONNode lDefinitionNode = JSONNode.Parse(lDefinition);
                if (lDefinitionNode == null) { continue; }

                MotionControllerMotion lMotion = null;
                
                string lTypeString = lDefinitionNode["Type"].Value;                          
                //Debug.Log("[MotionControllerLayer] Deserializing motion: " + lTypeString);

                // CDL 07/27/2018 - Use AssemblyHelper.ResolveType() intead of Type.GetType() so that 
                // the Type can still be obtained if the Motion was serialized as belonging to a different assembly
                bool lUpdateType;
                Type lType = AssemblyHelper.ResolveType(lTypeString, out lUpdateType);                                
                if (lType == null)
                {
                    // CDL 07/27/2018 - save this definition for removal afterwards, as we can't resolve the Type from any assembly.
                    lInvalidDefinitions.Add(i);                    
                    Debug.Log(string.Format("[MotionControllerLayer] Could not get type for {0}. Definition will be removed.", lTypeString));
                    //Debug.Log(lDefinition);
                    continue;
                }

                float lPriority = 0;
                
                // If don't have a motion matching the type, we need to create one
                if (Motions.Count <= i || lTypeString != Motions[i].GetType().AssemblyQualifiedName)                
                {
                    lMotion = Activator.CreateInstance(lType) as MotionControllerMotion;
                    // CDL 07/27/2018 - added a null check
                    if (lMotion == null) continue;
                    lMotion.MotionController = mMotionController;
                    lMotion.MotionLayer = this;

                    if (Motions.Count <= i)
                    {
                        Motions.Add(lMotion);
                    }
                    else
                    {
                        Motions[i] = lMotion;
                    }
                }
                // Grab the matching motion
                else
                {
                    lMotion = Motions[i];

                    // Track the priority so we can reset it
                    lPriority = lMotion.Priority;
                }

                // Fill the motion with data from the definition
                if (lMotion != null)
                {
                    lMotion.DeserializeMotion(lDefinition);

                    // Reset the priority based on the default
                    if (lPriority > 0) { lMotion.Priority = lPriority; }

                    // CDL 07/27/2018
                    // If we need to update the Type (because the AssemblyQualfiedName needs to be updated), clear it
                    // so that SerializeMotion() gets the type name again
                    if (lUpdateType)
                    {
                        lMotion.Type = "";
                    }
                    // We re-serialize the motion incase there was a change. If the
                    // type changed or some other value, we want the updated definition
                    MotionDefinitions[i] = lMotion.SerializeMotion();
                }
            }

            // CDL 07/27.2018 - Clean up any invalid Motion Definitions
            if (lInvalidDefinitions.Count > 0)
            {
                for (int i = 0; i < lInvalidDefinitions.Count; i++)
                {
                    int lIndex = lInvalidDefinitions[i];
                    Debug.Log("Removing invalid Motion Defintion: " + MotionDefinitions[lIndex]);
                    MotionDefinitions.RemoveAt(lIndex);
                }
            }

            // Clear the root object
            JSONSerializer.RootObject = null;

            // Allow each motion to initialize now that this has been deserialized
            for (int i = 0; i < Motions.Count; i++)
            {
                Motions[i].Awake();
                Motions[i].Initialize();
            }
        }
    }
}
