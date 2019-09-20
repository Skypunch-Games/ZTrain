using System.Collections.Generic;
using UnityEngine;
using com.ootii.Actors.AnimationControllers;
using com.ootii.Helpers;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Animations;
#endif

namespace com.ootii.Setup
{    
    public static class AnimatorSetupHelper 
    {
#if UNITY_EDITOR
       

        public static readonly string DefaultAnimatorControllerPath = DefaultPaths.CustomContent + "Animators/";
        public static readonly string DefaultAnimatorControllerName = "BasicHumanoid";

        /// <summary>
        /// Ensures that an Animator Controller exists and that its layers and parameters are correctly configured        
        /// Currently handles configuring up to 6 layers (Base, Upper Body, Arms Only, Left Hand, Right Hand, Upper Body Additive)
        /// </summary>
        /// <param name="rMotionController"></param>
        /// <param name="rAnimatorPath">Path at which to create the Animator Controller if it does not exist</param>
        /// <param name="rLastLayerIndex">Index of the bottom (highest index) layer to use on the Animator Controller</param>
        /// <returns></returns>
        public static AnimatorController EnsureAnimatorController(MotionController rMotionController, string rAnimatorPath, int rLastLayerIndex = 1)
        {
            // Get the Animator component on the character
            Animator lAnimator = rMotionController.Animator;
            if (lAnimator == null)
            {
                lAnimator = rMotionController.gameObject.GetComponent<Animator>();
                if (lAnimator == null) { return null; }
            }

            AnimatorController lController = null;

            // Create the Animator Controller asset if it does not exist
            if (lAnimator.runtimeAnimatorController == null)
            {
                string lAssetPath = VerifyAnimatorPath(rAnimatorPath);
                //Debug.Log("Creating new animator at: " + lAssetPath);

                lController = AnimatorController.CreateAnimatorControllerAtPath(lAssetPath);
                lAnimator.runtimeAnimatorController = lController;
            }

            lController = lAnimator.runtimeAnimatorController as AnimatorController;

            // Verify that the Animator Controller has the correct layers and parameters
            SetupAnimatorController(lController, rLastLayerIndex);

            return lController;
        }

        /// <summary>
        /// Ensures that an Animator Controller exists and that its layers and parameters are correctly configured        
        /// Currently handles configuring up to 6 layers (Base, Upper Body, Arms Only, Left Hand, Right Hand, Upper Body Additive).
        /// If there is no valid Animator Controller attached to the character, one will be created using the default path and name.
        /// </summary>
        /// <param name="rMotionController"></param>
        /// <param name="rLastLayerIndex"></param>
        /// <returns></returns>
        public static AnimatorController EnsureAnimatorController(MotionController rMotionController, int rLastLayerIndex = 1)
        {
            return EnsureAnimatorController(rMotionController, string.Empty, rLastLayerIndex);
        }        

        /// <summary>
        /// Ensures that an Animator Controller exists and that its layers and parameters are correctly configured. 
        /// </summary>
        /// <param name="rMotionController"></param>
        /// <param name="rUseHandPoses">If true, configures layer 0 through 4; if false, configures only layers 0 and 1</param>
        /// <returns></returns>
        public static AnimatorController EnsureAnimatorController(MotionController rMotionController, bool rUseHandPoses)
        {
            return EnsureAnimatorController(rMotionController,rUseHandPoses ? EnumMotionLayer.RIGHT_HAND : EnumMotionLayer.UPPER_BODY);
        }

        /// <summary>
        /// We we already have a valid reference to an Animator Controller, we can call this overload to quickly verify that the Animator
        /// Controller has been configured correctly and then run the setup if it is not. 
        /// </summary>
        /// <param name="rAnimatorController">Reference to the Animator Controller</param>
        /// <param name="rLastLayerIndex">Highest indexed layer on the Animator Controller</param>
        /// <param name="rForceSetup">Skip verifying the configuration and run the full setup on the Animator Controller</param>
        /// <returns></returns>
        public static AnimatorController EnsureAnimatorController(AnimatorController rAnimatorController, int rLastLayerIndex, bool rForceSetup = false)
        {
            if (rForceSetup || !VerifyAnimatorControllerLayers(rAnimatorController, rLastLayerIndex))
            {
                SetupAnimatorController(rAnimatorController, rLastLayerIndex);
            }

            return rAnimatorController;            
        }

        /// <summary>
        /// Verifies that the Animator Controller has the correct number of layers, that the layer properties are configured, 
        /// and that the animator paramters have been created. This is not an exhaustive check; it checks if layers above the base 
        /// have an avatar mask, and we look for one of the required Motion Controller parameters and one parameter for each layer.
        /// 
        /// Assuming that the Animator Controller was set up using these scripts, then the result of this function should be accurate.
        /// </summary>
        /// <param name="rAnimatorController"></param>
        /// <param name="rLastLayerIndex"></param>
        /// <returns></returns>
        public static bool VerifyAnimatorControllerLayers(AnimatorController rAnimatorController, int rLastLayerIndex)
        {
            // First check if the Animator Controller has the appropriate number of layers
            if (rAnimatorController.layers.Length <= rLastLayerIndex) { return false; }

            // Check animator parameters

            // Check one of the primary MC required parameters 
            if (!CheckParameter(rAnimatorController, "InputMagnitude")) { return false; }

            // Check the layer properties and parameters
            for (int i = 0; i < rAnimatorController.layers.Length; i++)
            {
                // Check if the layer's animator parameters have been created
                if (!CheckParameter(rAnimatorController, "L" + i + "MotionPhase")) { return false; }

                if (i > 0)
                {
                    // Layers above the base require an Avatar Mask
                    if (rAnimatorController.layers[i].avatarMask == null) { return false; }                    
                }               
            }            

            return true;
        }

        /// <summary>
        /// Ensure that the layers on the Animator Controller are configured.
        /// </summary>
        /// <param name="rAnimatorController">The Animator Controller to set up</param>
        /// <param name="rLastLayerIndex">The highest-indexed animator layer</param>        
        public static void SetupAnimatorController(AnimatorController rAnimatorController, int rLastLayerIndex)
        {
            // Ensure that the layer index parameter is within the bounds of what this function handles
            if (rLastLayerIndex < 0) { rLastLayerIndex = 0; }
            else if (rLastLayerIndex > 5) { rLastLayerIndex = 5; }
           
            // First ensure that the base animator layer has been created
            if (rAnimatorController.layers.Length < 1)
            {
                rAnimatorController.AddLayer("Base Layer");  // Index 0
            }           

            // By default, we'll create the Upper Body layer (this can be overridden)
            if (rLastLayerIndex > 0 && rAnimatorController.layers.Length < 2)
            {
                rAnimatorController.AddLayer("Upper Body");  // Index 1                
            }

            // Arms Only layer (currently only used by the Shooter MP)
            if (rLastLayerIndex > 1 && rAnimatorController.layers.Length < 3)
            {
                rAnimatorController.AddLayer("Arms Only");  // Index 2                
            }

            // Left Hand layer, used for hand pose correction
            if (rLastLayerIndex > 2 && rAnimatorController.layers.Length < 4)
            {
                rAnimatorController.AddLayer("Left Hand");  // Index 3                
            }

            // Right Hand layer, used for hand pose correction
            if (rLastLayerIndex > 3 && rAnimatorController.layers.Length < 5)
            {
                rAnimatorController.AddLayer("Right Hand");  // Index 4                
            }

            // Upper Body Additive layer, used by some motion packs for overlaying weapon/shield position
            if (rLastLayerIndex > 4 && rAnimatorController.layers.Length < 6)
            {
                rAnimatorController.AddLayer("Upper Body Additive");  // Index 5                
            }
            
            // Get a copy of the Animator Layers
            AnimatorControllerLayer[] lLayers = rAnimatorController.layers;

            // Set Base Layer IK Pass and Default Weight
            if (lLayers.Length >= 1)
            {
                lLayers[0].iKPass = true;
                lLayers[0].defaultWeight = 1;
            }

            // Set Upper Body Layer IK Pass, Default Weight, and Avatar Mask
            if (lLayers.Length >= 2)
            {
                lLayers[1].iKPass = true;
                lLayers[1].defaultWeight = 1;
                lLayers[1].avatarMask = AssetDatabase.LoadAssetAtPath<AvatarMask>(DefaultPaths.AvatarMasks + "Upper.mask");
            }

            // Set Arms Only Layer IK Pass, Default Weight, and Avatar Mask
            if (lLayers.Length >= 3)
            {
                lLayers[2].iKPass = true;
                lLayers[2].defaultWeight = 1;
                lLayers[2].avatarMask = AssetDatabase.LoadAssetAtPath<AvatarMask>(DefaultPaths.AvatarMasks + "ArmsOnly.mask");
            }

            // Set Left Hand Layer IK Pass, Default Weight, and Avatar Mask
            if (lLayers.Length >= 4)
            {                
                lLayers[3].iKPass = true;
                lLayers[3].defaultWeight = 1;
                lLayers[3].avatarMask = AssetDatabase.LoadAssetAtPath<AvatarMask>(DefaultPaths.AvatarMasks + "LeftHand.mask");
            }

            // Set Right Hand Layer IK Pass, Default Weight, and Avatar Mask
            if (lLayers.Length >= 5)
            {                
                lLayers[4].iKPass = true;
                lLayers[4].defaultWeight = 1;
                lLayers[4].avatarMask = AssetDatabase.LoadAssetAtPath<AvatarMask>(DefaultPaths.AvatarMasks + "RightHand.mask");
            }

            // Upper Body Additive layer
            if (lLayers.Length >= 6)
            {                
                lLayers[5].blendingMode = AnimatorLayerBlendingMode.Additive;
                lLayers[5].iKPass = false;
                lLayers[5].defaultWeight = 1;
                lLayers[5].avatarMask = AssetDatabase.LoadAssetAtPath<AvatarMask>(DefaultPaths.AvatarMasks + "Upper.mask");
            }

            // Set the layer array back into the property after changing the values
            rAnimatorController.layers = lLayers;

            // Ensure that the required animator parameters have been created
            EnsureAnimatorParameters(rAnimatorController);

            // Ensure that each layer's parameters have been created and that each layer above the Base has an Empty state
            for (int i = 0; i < lLayers.Length; i++)
            {
                EnsureLayerParameters(rAnimatorController, i);
                if (i > 0) { CreateEmptyState(rAnimatorController, i) ;}
            }                        
        }


        /// <summary>
        /// Get the Animator Controller attached to the character
        /// </summary>
        /// <param name="rMotionController"></param>
        /// <returns></returns>
        public static AnimatorController GetAnimatorController(MotionController rMotionController)
        {
            Animator lAnimator = rMotionController.Animator;
            if (lAnimator == null)
            {
                lAnimator = rMotionController.gameObject.GetComponent<Animator>();
                if (lAnimator == null) { return null; }
            }

            return lAnimator.runtimeAnimatorController as AnimatorController;
        }

        #region Animator Parameters

        /// <summary>
        /// Creates the animator parameters required by Motion Controller, if they do not exist
        /// </summary>
        /// <param name="rAnimatorController"></param>
        public static void EnsureAnimatorParameters(AnimatorController rAnimatorController)
        {
            CreateAnimatorParameter(rAnimatorController, "IsGrounded", AnimatorControllerParameterType.Bool);
            CreateAnimatorParameter(rAnimatorController, "Stance", AnimatorControllerParameterType.Int);
            CreateAnimatorParameter(rAnimatorController, "InputX", AnimatorControllerParameterType.Float);
            CreateAnimatorParameter(rAnimatorController, "InputY", AnimatorControllerParameterType.Float);
            CreateAnimatorParameter(rAnimatorController, "InputMagnitude", AnimatorControllerParameterType.Float);
            CreateAnimatorParameter(rAnimatorController, "InputMagnitudeAvg", AnimatorControllerParameterType.Float);
            CreateAnimatorParameter(rAnimatorController, "InputAngleFromAvatar", AnimatorControllerParameterType.Float);
            CreateAnimatorParameter(rAnimatorController, "InputAngleFromCamera", AnimatorControllerParameterType.Float);
        }

        /// <summary>
        /// Create the standard Motion Controller parameters for the given layer, if they do not exist
        /// </summary>
        /// <param name="lAnimatorController">The controller we're configuring</param>
        /// <param name="rLayerIndex">Index of the animator layer</param>
        public static void EnsureLayerParameters(AnimatorController lAnimatorController, int rLayerIndex)
        {
            CreateAnimatorParameter(lAnimatorController, "L" + rLayerIndex + "MotionPhase", AnimatorControllerParameterType.Int);
            CreateAnimatorParameter(lAnimatorController, "L" + rLayerIndex + "MotionForm", AnimatorControllerParameterType.Int);
            CreateAnimatorParameter(lAnimatorController, "L" + rLayerIndex + "MotionParameter", AnimatorControllerParameterType.Int);
            CreateAnimatorParameter(lAnimatorController, "L" + rLayerIndex + "MotionStateTime", AnimatorControllerParameterType.Float);
        }

        /// <summary>
        /// Checks if the specified parameter exists on the animator controller
        /// </summary>
        /// <param name="rAnimatorController"></param>
        /// <param name="rName"></param>
        /// <returns></returns>
        public static bool CheckParameter(AnimatorController rAnimatorController, string rName)
        {
            if (rAnimatorController == null) { return false; }

            for (int i = 0; i < rAnimatorController.parameters.Length; i++)
            {
                if (rAnimatorController.parameters[i].name == rName)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Checks if the specified parameter exists on the animator controller
        /// </summary>        
        /// <param name="rAnimator"></param>
        /// <param name="rName"></param>
        /// <returns></returns>
        public static bool CheckParameter(Animator rAnimator, string rName)
        {
            if (rAnimator == null) { return false; }

            for (int i = 0; i < rAnimator.parameterCount; i++)
            {
                if (rAnimator.parameters[i].name == rName) { return true; }
            }

            return false;
        }

        /// <summary>
        /// Creates an animator parameter of the specified type (if it doesn't exist)
        /// </summary>
        /// <param name="rAnimatorController">Controller we're adding the parameter to</param>
        /// <param name="rName">Name of the parameter</param>
        /// <param name="rType">Type of the parameter</param>
        public static void CreateAnimatorParameter(AnimatorController rAnimatorController, string rName, AnimatorControllerParameterType rType)
        {
            if (rAnimatorController == null) { return; }

            for (int i = 0; i < rAnimatorController.parameters.Length; i++)
            {
                if (rAnimatorController.parameters[i].name == rName) { return; }
            }

            rAnimatorController.AddParameter(rName, rType);
        }

        #endregion Parameters


        #region Animator States

        /// <summary>
        /// Get the Animator state machine for the specified animator layer
        /// </summary>
        /// <param name="rMotionController"></param>
        /// <param name="rLayerIndex"></param>
        /// <returns></returns>
        public static AnimatorStateMachine GetLayerStateMachine(MotionController rMotionController, int rLayerIndex)
        {
            // Get the Animator component on the character
            Animator lAnimator = rMotionController.Animator;
            if (lAnimator == null)
            {
                lAnimator = rMotionController.gameObject.GetComponent<Animator>();
                if (lAnimator == null) { return null; }
            }

            AnimatorController lController = lAnimator.runtimeAnimatorController as AnimatorController;            
            if (lController == null) return null;

            AnimatorControllerLayer lLayer = lController.layers[rLayerIndex];
            AnimatorStateMachine lLayerStateMachine = lLayer.stateMachine;

            return lLayerStateMachine;
        }

        /// <summary>
        /// Verify that an Any State transition exists, creating one if it does not.
        /// </summary>
        /// <param name="rFrom"></param>
        /// <param name="rTo"></param>
        /// <param name="rIndex"></param>
        /// <param name="rExists">Indicates if the transition already existed</param>
        /// <returns></returns>
        public static AnimatorStateTransition EnsureAnyStateTransition(
            AnimatorStateMachine rFrom, AnimatorState rTo, int rIndex, out bool rExists)
        {
            var lTransition = MotionControllerMotion.EditorFindAnyStateTransition(rFrom, rTo, rIndex);
            rExists = (lTransition != null);

            if (!rExists)
            {
                lTransition = rFrom.AddAnyStateTransition(rTo);
            }

            return lTransition;
        }

        /// <summary>
        /// Replace an IdlePose state's animation with one from the motion pack being configured. If a different set of 
        /// default Idle/Walk animations are used, then we can replace the default ootii IdlePose animation with one that 
        /// better matches the new default Idle.
        /// </summary>
        /// <param name="rStateMachine">The animator state machine whose Idle Pose we're going to replace</param>
        /// <param name="rMotion">The AnimationClip to replace it with</param>
        /// <param name="rStateName">Name of the state we are replacing (defaults to "IdlePose")</param>
        public static void ReplaceIdlePose(AnimatorStateMachine rStateMachine, Motion rMotion, string rStateName = "IdlePose")
        {
            if (rStateMachine == null || rMotion == null || string.IsNullOrEmpty(rStateName)) { return; }

            AnimatorState lIdlePoseState = MotionControllerMotion.EditorFindState(rStateMachine, rStateName);

            if (lIdlePoseState == null) { return; }
            lIdlePoseState.motion = rMotion;
        }


        /// <summary>
        /// Creates an Empty state on the specified layer
        /// </summary>
        /// <param name="rController"></param>
        /// <param name="rLayerIndex"></param>
        public static void CreateEmptyState(AnimatorController rController, int rLayerIndex)
        {
            if (rController == null) return;

            AnimatorControllerLayer lLayer = rController.layers[rLayerIndex];
            AnimatorStateMachine lLayerStateMachine = lLayer.stateMachine;

            AnimatorStateMachine lEmptySSM = MotionControllerMotion.EditorFindSSM(lLayerStateMachine, "Empty-SM");
            if (lEmptySSM == null) { lEmptySSM = lLayerStateMachine.AddStateMachine("Empty-SM", new Vector3(192, -480, 0)); }

            AnimatorState lEmptyState = MotionControllerMotion.EditorFindState(lEmptySSM, "EmptyPose");
            if (lEmptyState == null) { lEmptyState = lEmptySSM.AddState("EmptyPose", new Vector3(312, 84, 0)); }
            lEmptyState.speed = 1f;
            lEmptyState.mirror = false;
            lEmptyState.tag = "";

            AnimatorStateTransition lAnyTransition_N936152 = MotionControllerMotion.EditorFindAnyStateTransition(lLayerStateMachine, lEmptyState, 0);
            if (lAnyTransition_N936152 == null) { lAnyTransition_N936152 = lLayerStateMachine.AddAnyStateTransition(lEmptyState); }
            lAnyTransition_N936152.isExit = false;
            lAnyTransition_N936152.hasExitTime = false;
            lAnyTransition_N936152.hasFixedDuration = true;
            lAnyTransition_N936152.exitTime = 0.75f;
            lAnyTransition_N936152.duration = 0.15f;
            lAnyTransition_N936152.offset = 0f;
            lAnyTransition_N936152.mute = false;
            lAnyTransition_N936152.solo = false;
            lAnyTransition_N936152.canTransitionToSelf = false;
            lAnyTransition_N936152.orderedInterruption = false;
            lAnyTransition_N936152.interruptionSource = (TransitionInterruptionSource)2;
            for (int i = lAnyTransition_N936152.conditions.Length - 1; i >= 0; i--) { lAnyTransition_N936152.RemoveCondition(lAnyTransition_N936152.conditions[i]); }
            lAnyTransition_N936152.AddCondition(AnimatorConditionMode.Equals, 3010f, "L" + rLayerIndex + "MotionPhase");
            lAnyTransition_N936152.AddCondition(AnimatorConditionMode.Equals, 0f, "L" + rLayerIndex + "MotionForm");
            lAnyTransition_N936152.AddCondition(AnimatorConditionMode.Equals, 0f, "L" + rLayerIndex + "MotionParameter");

            AnimatorStateTransition lAnyTransition_N936154 = MotionControllerMotion.EditorFindAnyStateTransition(lLayerStateMachine, lEmptyState, 1);
            if (lAnyTransition_N936154 == null) { lAnyTransition_N936154 = lLayerStateMachine.AddAnyStateTransition(lEmptyState); }
            lAnyTransition_N936154.isExit = false;
            lAnyTransition_N936154.hasExitTime = false;
            lAnyTransition_N936154.hasFixedDuration = true;
            lAnyTransition_N936154.exitTime = 0.75f;
            lAnyTransition_N936154.duration = 0f;
            lAnyTransition_N936154.offset = 0f;
            lAnyTransition_N936154.mute = false;
            lAnyTransition_N936154.solo = false;
            lAnyTransition_N936154.canTransitionToSelf = false;
            lAnyTransition_N936154.orderedInterruption = false;
            lAnyTransition_N936154.interruptionSource = (TransitionInterruptionSource)2;
            for (int i = lAnyTransition_N936154.conditions.Length - 1; i >= 0; i--) { lAnyTransition_N936154.RemoveCondition(lAnyTransition_N936154.conditions[i]); }
            lAnyTransition_N936154.AddCondition(AnimatorConditionMode.Equals, 3010f, "L" + rLayerIndex + "MotionPhase");
            lAnyTransition_N936154.AddCondition(AnimatorConditionMode.Equals, 0f, "L" + rLayerIndex + "MotionForm");
            lAnyTransition_N936154.AddCondition(AnimatorConditionMode.Equals, 1f, "L" + rLayerIndex + "MotionParameter");
        }

        /// <summary>
        /// Create an animator state and set its fields to the default values
        /// </summary>
        /// <param name="rStateMachine"></param>
        /// <param name="rStateName"></param>
        /// <param name="rPosition"></param>
        /// <returns></returns>
        public static AnimatorState CreateState(AnimatorStateMachine rStateMachine, string rStateName, Vector3 rPosition)
        {
            var lState = MotionControllerMotion.EditorFindState(rStateMachine, rStateName);
            if (lState == null) { lState = rStateMachine.AddState(rStateName, rPosition); }

            lState.speed = 1f;
            lState.mirror = false;
            lState.tag = "";

            return lState;
        }

        /// <summary>
        /// Create an animator state and set its fields to the default values. Also set the animation clip.
        /// </summary>
        /// <param name="rStateMachine"></param>
        /// <param name="rStateName"></param>
        /// <param name="rPosition"></param>
        /// <param name="rAnimationPath"></param>
        /// <param name="rAnimationName"></param>
        /// <returns></returns>
        public static AnimatorState CreateState(AnimatorStateMachine rStateMachine, string rStateName,
            Vector3 rPosition, string rAnimationPath, string rAnimationName = "")
        {
            var lState = CreateState(rStateMachine, rStateName, rPosition);
            if (lState != null)
            {
                // If the name of the animation clip is empty, then use the state name (which is the default when
                // dragging onto the animator canvas)
                lState.motion = MotionControllerMotion.EditorFindAnimationClip(
                    rAnimationPath, string.IsNullOrEmpty(rAnimationName) ? rStateName : rAnimationName);
            }

            return lState;
        }

        /// <summary>
        /// Create an animator state tagged "Exit"
        /// </summary>
        /// <param name="rStateMachine"></param>
        /// <param name="rStateName"></param>
        /// <param name="rPosition"></param>
        /// <returns></returns>
        public static AnimatorState CreateExitState(AnimatorStateMachine rStateMachine, string rStateName, Vector3 rPosition)
        {
            var lState = MotionControllerMotion.EditorFindState(rStateMachine, rStateName);
            if (lState == null) { lState = rStateMachine.AddState(rStateName, rPosition); }

            lState.speed = 1f;
            lState.mirror = false;
            lState.tag = "Exit";

            return lState;
        }

        /// <summary>
        /// Create an animator state tagged "Exit" - also set the animation clip.
        /// </summary>
        /// <param name="rStateMachine"></param>
        /// <param name="rStateName"></param>
        /// <param name="rPosition"></param>
        /// <param name="rAnimationPath"></param>
        /// <param name="rAnimationName"></param>
        /// <returns></returns>
        public static AnimatorState CreateExitState(AnimatorStateMachine rStateMachine, string rStateName,
            Vector3 rPosition, string rAnimationPath, string rAnimationName = "")
        {
            var lState = CreateState(rStateMachine, rStateName, rPosition, rAnimationPath, rAnimationName);
            if (lState != null)
            {
                lState.tag = "Exit";
            }

            return lState;
        }

        #endregion Animator States


        #region Utilities

        /// <summary>
        /// When creating a new animator controller via script, we want to ensure that the path exists and that we're not creating 
        /// a duplicate filename
        /// </summary>
        /// <param name="rPath"></param>
        /// <returns></returns>
        private static string VerifyAnimatorPath(string rPath)
        {
            string lVerifiedPath = string.IsNullOrEmpty(rPath)
                ? DefaultAnimatorControllerPath + DefaultAnimatorControllerName
                : rPath;

            if (!lVerifiedPath.EndsWith(".controller"))
            {
                lVerifiedPath += ".controller";
            }

            return AssetHelper.GetNewAssetPath(lVerifiedPath);
        }

        #endregion Utilities

#endif
    }
}


