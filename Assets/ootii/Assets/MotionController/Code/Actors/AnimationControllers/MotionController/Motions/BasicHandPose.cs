using UnityEngine;

namespace com.ootii.Actors.AnimationControllers
{
    // CDL 07/25/2018 - I figure that others might find this useful to correct the hands when using any of the weapon motion packs 
    // with the standard Jump, Climb, etc motions which will otherwise open the hands
    [MotionName("Basic Hand Pose")]
    [MotionDescription("Simple motion that sets the hand pose; usually to keep the hand closed around a weapon. Requires a masked layer for each hand.")]
    public class BasicHandPose : MotionControllerMotion
    {
        /// <summary>
        /// Trigger values for the motion
        /// </summary>
        public int PHASE_UNKNOWN = 0;
        public int PHASE_START = 3660;
        public int PHASE_EXIT = 3661;
        public int PHASE_EMPTY = 3010;

        /// <summary>
        /// Determines if we're using the IsInMotion() function to verify that
        /// the transition in the animator has occurred for this motion.
        /// </summary>
        public override bool VerifyTransition
        {
            get { return false; }
        }

        // Used to force a change if needed
        protected int mActiveForm = 0;

        /// <summary>
        /// Default constructor
        /// </summary>
        public BasicHandPose() : base()
        {
            _Category = EnumMotionCategories.IDLE;

            _Priority = 10;

#if UNITY_EDITOR
            if (_EditorAnimatorSMName.Length == 0)
            {
                _EditorAnimatorSMName = "BasicHandPose-SM";
            }
#endif
        }

        /// <summary>
        /// Controller constructor
        /// </summary>
        /// <param name="rController">Controller the motion belongs to</param>
        public BasicHandPose(MotionController rController) : base(rController)
        {
            _Category = EnumMotionCategories.IDLE;

            _Priority = 10;

#if UNITY_EDITOR
            if (_EditorAnimatorSMName.Length == 0)
            {
                _EditorAnimatorSMName = "BasicHandPose-SM";
            }
#endif
        }

        /// <summary>
        /// Tests if this motion should be started. However, the motion
        /// isn't actually started.
        /// </summary>
        /// <returns></returns>
        public override bool TestActivate()
        {
            // This motion does not self-activate. It will be activated or deactivated via UnityEvents
            return false;
        }

        /// <summary>
        /// Tests if the motion should continue. If it shouldn't, the motion
        /// is typically disabled
        /// </summary>
        /// <returns></returns>
        public override bool TestUpdate()
        {
            if (mMotionController.State.AnimatorStates[mMotionLayer._AnimatorLayerIndex].StateInfo.IsTag("Exit"))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Called to start the specific motion. If the motion
        /// were something like 'jump', this would start the jumping process
        /// </summary>
        /// <param name="rPrevMotion">Motion that this motion is taking over from</param>
        public override bool Activate(MotionControllerMotion rPrevMotion)
        {
            // Trigger the transition
            mActiveForm = (_Form > 0 ? _Form : mMotionController.CurrentForm);
            mMotionController.SetAnimatorMotionPhase(mMotionLayer.AnimatorLayerIndex, PHASE_START, mActiveForm, mParameter, true);

            // Finalize the activation
            return base.Activate(rPrevMotion);
        }

        public override void Deactivate()
        {
            mMotionController.SetAnimatorMotionPhase(mMotionLayer.AnimatorLayerIndex, PHASE_EMPTY, 0, 0, true);
            base.Deactivate();
        }

        /// <summary>
        /// Allows the motion to modify the velocity before it is applied.
        /// </summary>
        /// <param name="rDeltaTime">Time since the last frame (or fixed update call)</param>
        /// <param name="rUpdateIndex">Index of the update to help manage dynamic/fixed updates. [0: Invalid update, >=1: Valid update]</param>
        /// <param name="rMovement">Amount of movement caused by root motion this frame</param>
        /// <param name="rRotation">Amount of rotation caused by root motion this frame</param>
        /// <returns></returns>
        public override void UpdateRootMotion(float rDeltaTime, int rUpdateIndex, ref Vector3 rMovement, ref Quaternion rRotation)
        {
            rMovement = Vector3.zero;
            rRotation = Quaternion.identity;
        }

        #region Auto-Generated
        // ************************************ START AUTO GENERATED ************************************

        /// <summary>
        /// These declarations go inside the class so you can test for which state
        /// and transitions are active. Testing hash values is much faster than strings.
        /// </summary>
        public int STATE_Empty = -1;
        public int STATE_DefaultHandPose = -1;
        public int TRANS_AnyState_DefaultHandPose = -1;
        public int TRANS_EntryState_DefaultHandPose = -1;
        public int TRANS_DefaultHandPose_Empty = -1;

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
                    if (lStateID == STATE_Empty) { return true; }
                    if (lStateID == STATE_DefaultHandPose) { return true; }
                }

                if (lTransitionID == TRANS_AnyState_DefaultHandPose) { return true; }
                if (lTransitionID == TRANS_EntryState_DefaultHandPose) { return true; }
                if (lTransitionID == TRANS_DefaultHandPose_Empty) { return true; }
                return false;
            }
        }

        /// <summary>
        /// Used to determine if the actor is in one of the states for this motion
        /// </summary>
        /// <returns></returns>
        public override bool IsMotionState(int rStateID)
        {
            if (rStateID == STATE_Empty) { return true; }
            if (rStateID == STATE_DefaultHandPose) { return true; }
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
                if (rStateID == STATE_Empty) { return true; }
                if (rStateID == STATE_DefaultHandPose) { return true; }
            }

            if (rTransitionID == TRANS_AnyState_DefaultHandPose) { return true; }
            if (rTransitionID == TRANS_EntryState_DefaultHandPose) { return true; }
            if (rTransitionID == TRANS_DefaultHandPose_Empty) { return true; }
            return false;
        }

        /// <summary>
        /// Preprocess any animator data so the motion can use it later
        /// </summary>
        public override void LoadAnimatorData()
        {
            string lLayer = mMotionController.Animator.GetLayerName(mMotionLayer._AnimatorLayerIndex);
            TRANS_AnyState_DefaultHandPose = mMotionController.AddAnimatorName("AnyState -> " + lLayer + ".BasicHandPose-SM.Default HandPose");
            TRANS_EntryState_DefaultHandPose = mMotionController.AddAnimatorName("Entry -> " + lLayer + ".BasicHandPose-SM.Default HandPose");
            STATE_Empty = mMotionController.AddAnimatorName("" + lLayer + ".BasicHandPose-SM.Empty");
            STATE_DefaultHandPose = mMotionController.AddAnimatorName("" + lLayer + ".BasicHandPose-SM.Default HandPose");
            TRANS_DefaultHandPose_Empty = mMotionController.AddAnimatorName("" + lLayer + ".BasicHandPose-SM.Default HandPose -> " + lLayer + ".BasicHandPose-SM.Empty");
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

            UnityEditor.Animations.AnimatorStateMachine lSSM_N2076296 = MotionControllerMotion.EditorFindSSM(lLayerStateMachine, "BasicHandPose-SM");
            if (lSSM_N2076296 == null) { lSSM_N2076296 = lLayerStateMachine.AddStateMachine("BasicHandPose-SM", new Vector3(324, 0, 0)); }

            UnityEditor.Animations.AnimatorState lState_N2076298 = MotionControllerMotion.EditorFindState(lSSM_N2076296, "Empty");
            if (lState_N2076298 == null) { lState_N2076298 = lSSM_N2076296.AddState("Empty", new Vector3(648, -144, 0)); }
            lState_N2076298.speed = 1f;
            lState_N2076298.mirror = false;
            lState_N2076298.tag = "Exit";

            UnityEditor.Animations.AnimatorState lState_N2076300 = MotionControllerMotion.EditorFindState(lSSM_N2076296, "Default HandPose");
            if (lState_N2076300 == null) { lState_N2076300 = lSSM_N2076296.AddState("Default HandPose", new Vector3(348, -144, 0)); }
            lState_N2076300.speed = 1f;
            lState_N2076300.mirror = false;
            lState_N2076300.tag = "";

            UnityEditor.Animations.AnimatorStateTransition lAnyTransition_N2076302 = MotionControllerMotion.EditorFindAnyStateTransition(lLayerStateMachine, lState_N2076300, 0);
            if (lAnyTransition_N2076302 == null) { lAnyTransition_N2076302 = lLayerStateMachine.AddAnyStateTransition(lState_N2076300); }
            lAnyTransition_N2076302.isExit = false;
            lAnyTransition_N2076302.hasExitTime = false;
            lAnyTransition_N2076302.hasFixedDuration = true;
            lAnyTransition_N2076302.exitTime = 0.75f;
            lAnyTransition_N2076302.duration = 0.25f;
            lAnyTransition_N2076302.offset = 0f;
            lAnyTransition_N2076302.mute = false;
            lAnyTransition_N2076302.solo = false;
            lAnyTransition_N2076302.canTransitionToSelf = true;
            lAnyTransition_N2076302.orderedInterruption = true;
            lAnyTransition_N2076302.interruptionSource = (UnityEditor.Animations.TransitionInterruptionSource)0;
            for (int i = lAnyTransition_N2076302.conditions.Length - 1; i >= 0; i--) { lAnyTransition_N2076302.RemoveCondition(lAnyTransition_N2076302.conditions[i]); }
            lAnyTransition_N2076302.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 3660f, "L" + rLayerIndex + "MotionPhase");
            lAnyTransition_N2076302.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 0f, "L" + rLayerIndex + "MotionForm");

            UnityEditor.Animations.AnimatorStateTransition lTransition_N2076304 = MotionControllerMotion.EditorFindTransition(lState_N2076300, lState_N2076298, 0);
            if (lTransition_N2076304 == null) { lTransition_N2076304 = lState_N2076300.AddTransition(lState_N2076298); }
            lTransition_N2076304.isExit = false;
            lTransition_N2076304.hasExitTime = false;
            lTransition_N2076304.hasFixedDuration = true;
            lTransition_N2076304.exitTime = 0.75f;
            lTransition_N2076304.duration = 0.25f;
            lTransition_N2076304.offset = 0f;
            lTransition_N2076304.mute = false;
            lTransition_N2076304.solo = false;
            lTransition_N2076304.canTransitionToSelf = true;
            lTransition_N2076304.orderedInterruption = true;
            lTransition_N2076304.interruptionSource = (UnityEditor.Animations.TransitionInterruptionSource)0;
            for (int i = lTransition_N2076304.conditions.Length - 1; i >= 0; i--) { lTransition_N2076304.RemoveCondition(lTransition_N2076304.conditions[i]); }
            lTransition_N2076304.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 3661f, "L" + rLayerIndex + "MotionPhase");


            // Run any post processing after creating the state machine
            OnStateMachineCreated();
        }

#endif

        // ************************************ END AUTO GENERATED ************************************
        #endregion

    }
}
