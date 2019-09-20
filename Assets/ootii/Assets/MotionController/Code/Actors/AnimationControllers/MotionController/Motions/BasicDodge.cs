using UnityEngine;
using com.ootii.Actors.AnimationControllers;
using com.ootii.Geometry;
using com.ootii.Helpers;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace com.ootii.MotionControllerPacks
{    
    /// <summary>
    /// Combat dodges, for both melee and ranged animation sets (adapted from Bow_Dodge)
    /// </summary>
    [MotionName("Basic Dodge")]
    [MotionDescription("Allows the character to do a quick dodge in the specified direction.")]
    public class BasicDodge : MotionControllerMotion
    {
        // Enum values for the motion
        public const int PHASE_UNKNOWN = 0;
        public const int PHASE_START = 3370;

        /// <summary>
        /// Determines if we dodge based on camera direction
        /// </summary>
        public bool _UseCameraDirection = true;
        public bool UseCameraDirection
        {
            get { return _UseCameraDirection; }
            set { _UseCameraDirection = value; }
        }

        /// <summary>
        /// Only use the camera direction when in WalkRunPivot
        /// </summary>
        public bool _CameraDirectionOnlyInPivot = true;
        public bool CameraDirectionOnlyInPivot
        {
            get { return _CameraDirectionOnlyInPivot; }
            set { _CameraDirectionOnlyInPivot = value; }
        }

        /// <summary>
        /// Enable a dodge forward; otherwise, dodges will only be left, right, and backwards
        /// </summary>
        public bool _AllowForwardDodge = true;
        public bool AllowForwardDodge
        {
            get { return _AllowForwardDodge; }
            set { _AllowForwardDodge = value; }
        }

        /// <summary>
        /// Determines if we're using the IsInMotion() function to verify that
        /// the transition in the animator has occurred for this motion.
        /// </summary>
        public override bool VerifyTransition
        {
            get { return false; }
        }

        /// <summary>
        /// Track the angle we have from the input
        /// </summary>
        protected Vector3 mStoredInputForward = Vector3.forward;

        /// <summary>
        /// Track the angle we'll use to force the direction
        /// </summary>
        protected float mStoredAngle = 0f;

        /// <summary>
        /// Default constructor
        /// </summary>
        public BasicDodge() : base()
        {
            _Category = EnumMotionCategories.DODGE;

            _Priority = 13;
            _ActionAlias = "Dodge";

#if UNITY_EDITOR
            if (_EditorAnimatorSMName.Length == 0) { _EditorAnimatorSMName = "BasicDodge-SM"; }
#endif
        }

        /// <summary>
        /// Controller constructor
        /// </summary>
        /// <param name="rController">Controller the motion belongs to</param>
        public BasicDodge(MotionController rController)
            : base(rController)
        {
            _Category = EnumMotionCategories.DODGE;

            _Priority = 13;
            _ActionAlias = "Dodge";

#if UNITY_EDITOR
            if (_EditorAnimatorSMName.Length == 0) { _EditorAnimatorSMName = "BasicDodge-SM"; }
#endif
        }


        /// <summary>
        /// Tests if this motion should be started. However, the motion
        /// isn't actually started.
        /// </summary>
        /// <returns></returns>
        public override bool TestActivate()
        {
            if (!mIsStartable) { return false; }
            if (!mMotionController.IsGrounded) { return false; }
            if (mActorController.State.Stance != EnumControllerStance.COMBAT_MELEE
                && mActorController.State.Stance != EnumControllerStance.COMBAT_RANGED) { return false; }

            // Check if the input occured
            if (mMotionController._InputSource != null && mMotionController._InputSource.IsJustPressed(_ActionAlias))
            {
                return true;
            }

            // Stop
            return false;
        }

        /// <summary>
        /// Tests if the motion should continue. If it shouldn't, the motion is typically disabled
        /// </summary>
        /// <returns>Boolean that determines if the motion continues</returns>
        public override bool TestUpdate()
        {
            if (mIsActivatedFrame) { return true; }
            if (!mMotionController.IsGrounded) { return false; }
            if (mActorController.State.Stance != EnumControllerStance.COMBAT_MELEE
                && mActorController.State.Stance != EnumControllerStance.COMBAT_RANGED) { return false; }


            // If we've reached the exit state, leave. The delay is to ensure that we're not in an old motion's exit state
            if (mMotionController.State.AnimatorStates[mMotionLayer._AnimatorLayerIndex].StateInfo.IsTag("Exit") && mMotionLayer._AnimatorStateNormalizedTime > 0.2f)
            {
                if (mMotionLayer._AnimatorTransitionID == 0) { return false; }
            }

            // Stay in
            return true;
        }

        /// <summary>
        /// Raised when a motion is being interrupted by another motion
        /// </summary>
        /// <param name="rMotion">Motion doing the interruption</param>
        /// <returns>Boolean determining if it can be interrupted</returns>
        public override bool TestInterruption(MotionControllerMotion rMotion)
        {
            return true;
        }

        /// <summary>
        /// Called to start the specific motion. If the motion
        /// were something like 'jump', this would start the jumping process
        /// </summary>
        /// <param name="rPrevMotion">Motion that this motion is taking over from</param>
        public override bool Activate(MotionControllerMotion rPrevMotion)
        {
            bool lUseCamera = _UseCameraDirection && _CameraDirectionOnlyInPivot && rPrevMotion is BasicWalkRunPivot;
            if (lUseCamera)
            {
                Vector3 lCameraForward = (Quaternion.Inverse(mActorController.Tilt) * mMotionController._CameraTransform.rotation).Forward();
                Vector3 lActorForward = mActorController._Transform.forward;
                mStoredAngle = Vector3Ext.SignedAngle(lActorForward, lCameraForward);
                //Debug.Log("BasicDodge activating using camera direction: " + mStoredAngle);
            }
            else
            {
                if (rPrevMotion is BasicWalkRunPivot)
                {
                    mStoredAngle = 0f;
                }
                else
                {
                    mStoredAngle = mMotionController.State.InputFromAvatarAngle;
                }
                //Debug.Log("BasicDodge activating: " + mStoredAngle);
            }

            // Activate the motion
            int lForm = (_Form > 0 ? _Form : mMotionController.CurrentForm);
            int lParameter = (_AllowForwardDodge ? 0 : 1);
            mMotionController.SetAnimatorMotionPhase(mMotionLayer._AnimatorLayerIndex, PHASE_START, lForm, lParameter, true);
            //Debug.Log(string.Format("Form {0}, Parameter {1}", lForm, Parameter));

            // Return
            return base.Activate(rPrevMotion);
        }       

        /// <summary>
        /// Allows the motion to modify the root-motion velocities before they are applied. 
        /// 
        /// NOTE:
        /// Be careful when removing rotations as some transitions will want rotations even 
        /// if the state they are transitioning from don't.
        /// </summary>
        /// <param name="rDeltaTime">Time since the last frame (or fixed update call)</param>
        /// <param name="rUpdateIndex">Index of the update to help manage dynamic/fixed updates. [0: Invalid update, >=1: Valid update]</param>
        /// <param name="rVelocityDelta">Root-motion linear velocity relative to the actor's forward</param>
        /// <param name="rRotationDelta">Root-motion rotational velocity</param>
        /// <returns></returns>
        public override void UpdateRootMotion(float rDeltaTime, int rUpdateIndex, ref Vector3 rMovement, ref Quaternion rRotation)
        {
        }

        /// <summary>
        /// Updates the motion over time. This is called by the controller
        /// every update cycle so animations and stages can be updated.
        /// </summary>
        /// <param name="rDeltaTime">Time since the last frame (or fixed update call)</param>
        /// <param name="rUpdateIndex">Index of the update to help manage dynamic/fixed updates. [0: Invalid update, >=1: Valid update]</param>
        public override void Update(float rDeltaTime, int rUpdateIndex)
        {
            // Modify the input value to be what we stored
            MotionState lState = mMotionController.State;
            lState.InputFromAvatarAngle = mStoredAngle;
            mMotionController.State = lState;
        }

        // **************************************************************************************************
        // Following properties and function only valid while editing
        // **************************************************************************************************

#if UNITY_EDITOR

        /// <summary>
        /// Allow the motion to render it's own GUI
        /// </summary>
        public override bool OnInspectorGUI()
        {
            bool lIsDirty = false;

            if (EditorHelper.TextField("Action Alias", "Determines what input triggers the dodge.", ActionAlias, mMotionController))
            {
                lIsDirty = true;
                ActionAlias = EditorHelper.FieldStringValue;
            }

            if (EditorHelper.BoolField("Use Camera Direction", "Determines if dodge direction is based on the camera direction or input.", UseCameraDirection, mMotionController))
            {
                lIsDirty = true;
                UseCameraDirection = EditorHelper.FieldBoolValue;
            }

            if (UseCameraDirection)
            {
                EditorGUI.indentLevel++;
                if (EditorHelper.BoolField("Only in Adventure Style", "Only use the camera direction when in WalkRunPivot (Adventure style movement).", CameraDirectionOnlyInPivot, mMotionController))
                {
                    lIsDirty = true;
                    CameraDirectionOnlyInPivot = EditorHelper.FieldBoolValue;
                }
                EditorGUI.indentLevel--;
            }

            if (EditorHelper.BoolField("Allow Forward Dodge", "Determines if dodging forward is enabled, or if the character can only dodge backwards, left, and right (dodging while moving forward will dodge backward).", AllowForwardDodge, mMotionController))
            {
                lIsDirty = true;
                AllowForwardDodge = EditorHelper.FieldBoolValue;
            }

            return lIsDirty;
        }
        
#endif

        #region Auto-Generated
        // ************************************ START AUTO GENERATED ************************************

        /// <summary>
        /// These declarations go inside the class so you can test for which state
        /// and transitions are active. Testing hash values is much faster than strings.
        /// </summary>
        public int STATE_Start = -1;
        public int STATE_Empty = -1;

        /// <summary>
        /// Determines if we're using auto-generated code
        /// </summary>
        public override bool HasAutoGeneratedCode
        {
            get { return true; }
        }

        /// <summary>
        /// Used to determine if the actor is in one of the states for this motion
        /// </summary>
        /// <returns></returns>
        public override bool IsInMotionState
        {
            get
            {
                int lStateID = mMotionLayer._AnimatorStateID;
                int lTransitionID = mMotionLayer._AnimatorTransitionID;

                if (lTransitionID == 0)
                {
                    if (lStateID == STATE_Start) { return true; }
                    if (lStateID == STATE_Empty) { return true; }
                }

                return false;
            }
        }

        /// <summary>
        /// Used to determine if the actor is in one of the states for this motion
        /// </summary>
        /// <returns></returns>
        public override bool IsMotionState(int rStateID)
        {
            if (rStateID == STATE_Start) { return true; }
            if (rStateID == STATE_Empty) { return true; }
            return false;
        }

        /// <summary>
        /// Used to determine if the actor is in one of the states for this motion
        /// </summary>
        /// <returns></returns>
        public override bool IsMotionState(int rStateID, int rTransitionID)
        {
            if (rTransitionID == 0)
            {
                if (rStateID == STATE_Start) { return true; }
                if (rStateID == STATE_Empty) { return true; }
            }

            return false;
        }

        /// <summary>
        /// Preprocess any animator data so the motion can use it later
        /// </summary>
        public override void LoadAnimatorData()
        {
            string lLayer = mMotionController.Animator.GetLayerName(mMotionLayer._AnimatorLayerIndex);
            STATE_Start = mMotionController.AddAnimatorName("" + lLayer + ".Start");
            STATE_Empty = mMotionController.AddAnimatorName("" + lLayer + ".BasicDodge-SM.Empty");
        }

#if UNITY_EDITOR

        /// <summary>
        /// New way to create sub-state machines without destroying what exists first.
        /// </summary>
        protected override void CreateStateMachine()
        {
            int rLayerIndex = mMotionLayer._AnimatorLayerIndex;
            MotionController rMotionController = mMotionController;

            UnityEditor.Animations.AnimatorController lController = null;

            Animator lAnimator = rMotionController.Animator;
            if (lAnimator == null) { lAnimator = rMotionController.gameObject.GetComponent<Animator>(); }
            if (lAnimator != null) { lController = lAnimator.runtimeAnimatorController as UnityEditor.Animations.AnimatorController; }
            if (lController == null) { return; }

            while (lController.layers.Length <= rLayerIndex)
            {
                UnityEditor.Animations.AnimatorControllerLayer lNewLayer = new UnityEditor.Animations.AnimatorControllerLayer();
                lNewLayer.name = "Layer " + (lController.layers.Length + 1);
                lNewLayer.stateMachine = new UnityEditor.Animations.AnimatorStateMachine();
                lController.AddLayer(lNewLayer);
            }

            UnityEditor.Animations.AnimatorControllerLayer lLayer = lController.layers[rLayerIndex];

            UnityEditor.Animations.AnimatorStateMachine lLayerStateMachine = lLayer.stateMachine;

            UnityEditor.Animations.AnimatorStateMachine lSSM_N2076316 = MotionControllerMotion.EditorFindSSM(lLayerStateMachine, "BasicDodge-SM");
            if (lSSM_N2076316 == null) { lSSM_N2076316 = lLayerStateMachine.AddStateMachine("BasicDodge-SM", new Vector3(624, -912, 0)); }

            UnityEditor.Animations.AnimatorState lState_N2076318 = MotionControllerMotion.EditorFindState(lSSM_N2076316, "Empty");
            if (lState_N2076318 == null) { lState_N2076318 = lSSM_N2076316.AddState("Empty", new Vector3(360, -96, 0)); }
            lState_N2076318.speed = 1f;
            lState_N2076318.mirror = false;
            lState_N2076318.tag = "Exit";


            // Run any post processing after creating the state machine
            OnStateMachineCreated();
        }

#endif

        // ************************************ END AUTO GENERATED ************************************
        #endregion
    }
}
