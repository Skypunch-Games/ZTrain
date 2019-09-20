using com.ootii.Actors.LifeCores;
using com.ootii.Cameras;
using com.ootii.Geometry;
using com.ootii.MotionControllerPacks;
using com.ootii.Reactors;
using com.ootii.Setup;
using UnityEngine;
using com.ootii.Utilities.Debug;

#if UNITY_EDITOR
using UnityEditor.Animations;
#endif

namespace com.ootii.Actors.AnimationControllers
{   
    /// <summary>
    /// Base definition class for Motion Packs which are built around extending the set of "Basic" motions. Contains all of
    /// the code necessary to create and extend the animator states for these motions.
    /// </summary>
    public abstract class BasicMotionPackDefinition : MotionPackDefinition
    {
#if UNITY_EDITOR                       
        /// <summary>
        /// Get the default IdlePose animation included with Motion Controller and used with Form 0
        /// </summary>
        /// Use "public new static AnimationClip GetDefaultIdlePose()" to override this in classes that inherit from this
        /// <returns></returns>
        public static AnimationClip GetDefaultIdlePose()
        {
            return MotionControllerMotion.EditorFindAnimationClip(DefaultPaths.StandardAnimations + "Idling/unity_Idle_IdleToIdlesR.fbx", "IdlePose");
        }

        #region Create Basic Motions

        /// <summary>
        /// Disable the default Idle, WalkRunPivot_v2, etc motions, as we don't want them conflicting with the Basic versions
        /// </summary>
        /// <param name="rMotionController"></param>
        public static void DisableDefaultLocomotion(MotionController rMotionController)
        {
            rMotionController.DisableMotion<Idle>(0);
            rMotionController.DisableMotion<WalkRunPivot_v2>(0);
            rMotionController.DisableMotion<WalkRunRotate_v2>(0);
            rMotionController.DisableMotion<WalkRunStrafe_v2>(0);
        }

        /// <summary>
        /// Create and configure the basic locomotion motions: BasicIdle, BasicWalkRunPivot, BasicWalkRunStrafe
        /// </summary>
        /// <param name="rMotionController"></param>
        /// <param name="rDefaultToRun"></param>
        /// <param name="rMovementStyle"></param>
        public static void CreateBasicLocomotion(MotionController rMotionController, bool rDefaultToRun = false, int rMovementStyle = 0)
        {
            // Disable the older Idle and WalkRunX motions
            DisableDefaultLocomotion(rMotionController);

            IBaseCameraRig lCameraRig = rMotionController.CameraRig;
            if (lCameraRig == null) { lCameraRig = rMotionController.ExtractCameraRig(rMotionController.CameraTransform); }

            BasicIdle lIdle = rMotionController.GetMotion<BasicIdle>(0, true);
            if (lIdle == null) { lIdle = rMotionController.CreateMotion<BasicIdle>(0); }

            BasicWalkRunPivot lPivot = rMotionController.GetMotion<BasicWalkRunPivot>(0, true);
            if (lPivot == null) { lPivot = rMotionController.CreateMotion<BasicWalkRunPivot>(0); }

            BasicWalkRunStrafe lStrafe = rMotionController.GetMotion<BasicWalkRunStrafe>(0, true);
            if (lStrafe == null) { lStrafe = rMotionController.CreateMotion<BasicWalkRunStrafe>(0); }
            
            
            lIdle.RotateWithInput = (rMovementStyle != EnumMovementStyle.Adventure) && (lCameraRig == null);
            lIdle.RotateWithCamera = (rMovementStyle != EnumMovementStyle.Adventure) && (lCameraRig != null);
            rMotionController.SerializeMotion(lIdle);

            // WalkRunPivot is only enabled for Adventure style movement
            lPivot.IsEnabled = rMovementStyle == EnumMovementStyle.Adventure;
            // We set DefaultToRun to true for NPCs or when using analog control            
            lPivot.DefaultToRun = rDefaultToRun;
            rMotionController.SerializeMotion(lPivot);

            // We want to set up the "default" WalkRunStrafe that is activated when aiming or locking onto a target,
            // not the "Controlled Walk" one that is used by BasicInteraction to move to an interaction
            bool lIsBasicStrafe = lStrafe.Name.Trim() != "Controlled Walk";
            if (lIsBasicStrafe)
            {
                lStrafe.IsEnabled = true;
                // Require target and use input alias only in Adventure style movement (Player only)
                lStrafe.RequireTarget = rMovementStyle == EnumMovementStyle.Adventure;
                lStrafe.ActivationAlias = (rMovementStyle == EnumMovementStyle.Adventure) ? "Camera Aim" : "";
                lStrafe.DefaultToRun = rDefaultToRun;
                lStrafe.RotateWithInput = (lCameraRig == null);
                lStrafe.RotateWithCamera = (lCameraRig != null);
                rMotionController.SerializeMotion(lStrafe);
            }                     
        }

        public static void CreateBasicNPCLocomotion(MotionController rMotionController, bool rDefaultToRun = false)
        {
            // Disable the older Idle and WalkRunX motions
            DisableDefaultLocomotion(rMotionController);

            //IBaseCameraRig lCameraRig = rMotionController.CameraRig;
            //if (lCameraRig == null) { lCameraRig = rMotionController.ExtractCameraRig(rMotionController.CameraTransform); }

            BasicIdle lIdle = rMotionController.GetMotion<BasicIdle>(0, true);
            if (lIdle == null) { lIdle = rMotionController.CreateMotion<BasicIdle>(0); }

            //BasicWalkRunPivot lPivot = rMotionController.GetMotion<BasicWalkRunPivot>(0, true);
            //if (lPivot == null) { lPivot = rMotionController.CreateMotion<BasicWalkRunPivot>(0); }

            BasicWalkRunStrafe lStrafe = rMotionController.GetMotion<BasicWalkRunStrafe>(0, true);
            if (lStrafe == null) { lStrafe = rMotionController.CreateMotion<BasicWalkRunStrafe>(0); }


            lIdle.RotateWithInput = true;
            lIdle.RotateWithCamera = false;
            rMotionController.SerializeMotion(lIdle);

            //// WalkRunPivot is only enabled for Adventure style movement
            //lPivot.IsEnabled = true;
            //// We set DefaultToRun to true for NPCs or when using analog control            
            //lPivot.DefaultToRun = rDefaultToRun;
            //rMotionController.SerializeMotion(lPivot);

            // We want to set up the "default" WalkRunStrafe that is activated when aiming or locking onto a target,
            // not the "Controlled Walk" one that is used by BasicInteraction to move to an interaction
            bool lIsBasicStrafe = lStrafe.Name.Trim() != "Controlled Walk";
            if (lIsBasicStrafe)
            {
                lStrafe.IsEnabled = true;                
                lStrafe.RequireTarget = false;
                lStrafe.ActivationAlias = "";
                lStrafe.DefaultToRun = rDefaultToRun;
                lStrafe.RotateWithInput = true;
                lStrafe.RotateWithCamera = false;
                rMotionController.SerializeMotion(lStrafe);
            }
        }

        /// <summary>
        /// Create and configure the BasicItemEquip and BasicItemStore Motions
        /// </summary>
        /// <param name="rMotionController"></param>
        /// <param name="rLayerIndex"></param>
        /// <param name="rAddBodyShape"></param>
        public static void CreateBasicEquipMotions(MotionController rMotionController, int rLayerIndex, bool rAddBodyShape = false)
        {
            string rLayerText = rLayerIndex > 0 ? rLayerIndex.ToString() : "";
            BasicItemEquip lEquip = rMotionController.GetMotion<BasicItemEquip>(rLayerIndex);
            if (lEquip == null)
            {
                lEquip = rMotionController.CreateMotion<BasicItemEquip>(rLayerIndex, "BasicItemEquip" + rLayerText);
            }

            BasicItemStore lStore = rMotionController.GetMotion<BasicItemStore>(rLayerIndex);
            if (lStore == null)
            {
                lStore = rMotionController.CreateMotion<BasicItemStore>(rLayerIndex, "BasicItemStore" + rLayerText);
            }

            lEquip.ItemID = "";
            // Additive blended animations will mess with the Equip animation
            lEquip.AddTag(DefaultMotionTags.DisableAdditive);

            // If using a two-handed weapon, we'll want to enable left hand IK (if in use) after equipping the weapon
            lEquip.AddTag(DefaultMotionTags.EnableLeftHandIK);

            // character often has trouble walking up stairs if this is enabled
            lEquip.AddCombatantBodyShape = rAddBodyShape;
            rMotionController.SerializeMotion(lEquip);

            // Additive blended animations will mess with the Store animation
            lStore.AddTag(DefaultMotionTags.DisableAdditive);

            // If using a two-handed weapon, we'll want to disable the left hand IK (if in use) when storing the weapon
            lStore.AddTag(DefaultMotionTags.DisableLeftHandIK);
            rMotionController.SerializeMotion(lStore);
        }

        /// <summary>
        /// Create the BasicHandPose motions for both for left and right hands
        /// </summary>
        /// <param name="rMotionController"></param>
        public static void CreateBasicHandPoseMotions(MotionController rMotionController)
        {
            BasicHandPose lLeftHand = rMotionController.GetMotion<BasicHandPose>(EnumMotionLayer.LEFT_HAND, true);
            if (lLeftHand == null) { rMotionController.CreateMotion<BasicHandPose>(EnumMotionLayer.LEFT_HAND, "Left Hand Pose"); }

            BasicHandPose lRightHand = rMotionController.GetMotion<BasicHandPose>(EnumMotionLayer.RIGHT_HAND, true);
            if (lRightHand == null) { rMotionController.CreateMotion<BasicHandPose>(EnumMotionLayer.RIGHT_HAND, "Right Hand Pose"); }

            // Add the reactor that activates and deactivates the hand pose motions
            ActorCore lCore = rMotionController.GetOrAddComponent<ActorCore>();
            lCore.GetOrAddReactor<BasicHandPoseReactor>();
        }


        /// <summary>
        /// Create the BasicDodge motion
        /// </summary>
        /// <param name="rMotionController"></param>
        /// <param name="rUseRootMotion"></param>
        public static void CreateBasicMeleeDodgeMotion(MotionController rMotionController, bool rUseRootMotion)
        {
            BasicDodge lDodge = rMotionController.GetMotion<BasicDodge>(0, true);
            if (lDodge == null) { lDodge = rMotionController.CreateMotion<BasicDodge>(0); }

            // Dodge animations may move the character using root motion data; this tag is checked for
            // by the AgentUseTransform reactor so that it can disable Use Transform during the animation
            if (rUseRootMotion) { lDodge.AddTag(DefaultMotionTags.UseRootMotion); }

            // Dodge animations when holding a two-handed weapon may move the left hand off of the weapon handle            
            lDodge.AddTag(DefaultMotionTags.DisableLeftHandIK);
            rMotionController.SerializeMotion(lDodge);
        }

        /// <summary>
        /// Create the BasicInteraction motion as well as the "Controlled Walk" strafe motion that it uses for positioning
        /// </summary>
        /// <param name="rMotionController"></param>
        public static void CreateInteractions(MotionController rMotionController)
        {
            BasicInteraction lInteraction = rMotionController.CreateMotion<BasicInteraction>(0);
            lInteraction.InteractableLayers = 1 << DefaultLayers.Interaction;
            rMotionController.SerializeMotion(lInteraction);

//#pragma warning disable 0219
            BasicWalkRunStrafe lControlledWalk = null;
//#pragma warning restore 0219

            // Try and find a BasicWalkRunStrafe motion named "Controlled Walk"
            foreach (var motion in rMotionController.MotionLayers[0].Motions)
            {
                if (motion.Name.Trim() == "Controlled Walk")
                {
                    lControlledWalk = motion as BasicWalkRunStrafe;                    
                    break;
                }
            }

            if (lControlledWalk == null)
            {
                //Debug.Log("Controlled Walk not found; creating...");                
                //lControlledWalk = CreateMotion<BasicWalkRunStrafe>(rMotionController.MotionLayers[0], "Controlled Walk", false);
                lControlledWalk = rMotionController.CreateMotionInstance<BasicWalkRunStrafe>(0, "Controlled Walk", false);
            }

            if (lControlledWalk != null)
            {
                lControlledWalk.Priority = 10;
                lControlledWalk.RequireTarget = false;
                lControlledWalk.RotateWithCamera = false;
                lControlledWalk.RotateWithInput = false;
                lControlledWalk.ActivationAlias = string.Empty;
                rMotionController.SerializeMotion(lControlledWalk);
            }
        }

        /// <summary>
        /// Create and configure the full-body BasicDamaged reaction motion
        /// </summary>
        /// <param name="rMotionController"></param>
        /// <param name="rUseRootMotion"></param>
        public static void CreateBasicDamagedMotion(MotionController rMotionController, bool rUseRootMotion)
        {
            BasicDamaged lDamaged = rMotionController.GetMotion<BasicDamaged>(0, true);
            if (lDamaged == null) { lDamaged = rMotionController.CreateMotion<BasicDamaged>(0); }

            // Damaged animations may move the character using root motion data; this tag is checked for
            // by the AgentUseTransform reactor so that it can disable Use Transform during the animation
            if (rUseRootMotion) { lDamaged.AddTag(DefaultMotionTags.UseRootMotion); }

            // When using a two-handed weapon, the damaged animation may move the character's left hand off 
            // of the weapon handle
            lDamaged.AddTag(DefaultMotionTags.DisableLeftHandIK);

            rMotionController.SerializeMotion(lDamaged);
        }

        /// <summary>
        /// Create and configure the full-body BasicDeath motion
        /// </summary>
        /// <param name="rMotionController"></param>
        /// <param name="rUseRootMotion"></param>
        public static void CreateBasicDeathMotion(MotionController rMotionController, bool rUseRootMotion)
        {
            BasicDeath lDeath = rMotionController.GetMotion<BasicDeath>(0, true);
            if (lDeath == null) { lDeath = rMotionController.CreateMotion<BasicDeath>(0); }

            // Death animations may move the character using root motion data; this tag is checked for
            // by the AgentUseTransform reactor so that it can disable Use Transform during the animation
            if (rUseRootMotion) { lDeath.AddTag(DefaultMotionTags.UseRootMotion); }

            // When using a two-handed weapon, the death animation almost always moves the character's left hand off 
            // of the weapon handle
            lDeath.AddTag(DefaultMotionTags.DisableLeftHandIK);

            // An additive blended animation will mess with death animation
            lDeath.AddTag(DefaultMotionTags.DisableAdditive);

            rMotionController.SerializeMotion(lDeath);
        }

        #endregion Create Basic Motions


        #region Create Standard Motions

        public static string[] DefaultClimbMotionTags = 
            { DefaultMotionTags.DisableAdditive, DefaultMotionTags.DisableFootIK, DefaultMotionTags.DisableLeftHandIK };

        /// <summary>
        /// Create the set of climbing motions (0.5m, 1.0m, 1.8m, 2.5m), plus the climb ladder motion
        /// These can be added to NPCs, as the OffMeshLinkDriver can use them to move the character
        /// </summary>
        /// <param name="rMotionController"></param>        
        /// <param name="rLayerMask"></param>        
        public static void CreateClimbMotions(MotionController rMotionController, LayerMask rLayerMask)
        {
            Climb_0_5m lClimb0_5m = rMotionController.CreateMotion<Climb_0_5m>(0, DefaultClimbMotionTags);
            lClimb0_5m.ClimbableLayers = rLayerMask;
            rMotionController.SerializeMotion(lClimb0_5m);

            Climb_1m lClimb_1m = rMotionController.CreateMotion<Climb_1m>(0, DefaultClimbMotionTags);
            lClimb_1m.ClimbableLayers = rLayerMask;
            rMotionController.SerializeMotion(lClimb_1m);

            Climb_1_8m lClimb1_8m = rMotionController.CreateMotion<Climb_1_8m>(0, DefaultClimbMotionTags);
            lClimb1_8m.ClimbableLayers = rLayerMask;
            rMotionController.SerializeMotion(lClimb1_8m);

            Climb_2_5m lClimb2_5m = rMotionController.CreateMotion<Climb_2_5m>(0, DefaultClimbMotionTags);
            lClimb2_5m.ClimbableLayers = rLayerMask;
            rMotionController.SerializeMotion(lClimb2_5m);
        }

        /// <summary>
        /// Create the standard Climb Ladder motion; this won't be added to NPCs as there's no vertical navmesh to direct movement
        /// </summary>
        /// <param name="rMotionController"></param>
        /// <param name="rLayerMask"></param>
        public static void CreateClimbLadderMotion(MotionController rMotionController, LayerMask rLayerMask)
        {
            ClimbLadder lClimbLadder = rMotionController.CreateMotion<ClimbLadder>(0, DefaultClimbMotionTags);
            lClimbLadder.ClimbableLayers = rLayerMask;
            rMotionController.SerializeMotion(lClimbLadder);
        }
        
        /// <summary>
        /// Create the Climb Wall motion; this won't be added to NPCs as there's no vertical navmesh to direct movement
        /// </summary>
        /// <param name="rMotionController"></param>
        /// <param name="rLayerMask"></param>
        public static void CreateWallClimbMotion(MotionController rMotionController, LayerMask rLayerMask)
        {
            ClimbWall lClimbWall = rMotionController.CreateMotion<ClimbWall>(0, DefaultClimbMotionTags);
            lClimbWall.ClimbableLayers = rLayerMask;
            rMotionController.SerializeMotion(lClimbWall);
        }

        /// <summary>
        /// Create the Climb Ledge; this won't be added to NPCs as there's no vertical navmesh to direct movement
        /// </summary>
        /// <param name="rMotionController"></param>
        /// <param name="rLayerMask"></param>
        public static void CreateLedgeClimbMotion(MotionController rMotionController, LayerMask rLayerMask)
        {
            ClimbCrouch lClimbCrouch = rMotionController.CreateMotion<ClimbCrouch>(0, DefaultClimbMotionTags);
            lClimbCrouch.ClimbableLayers = rLayerMask;
            rMotionController.SerializeMotion(lClimbCrouch);
        }

        /// <summary>
        /// Craete and configure jumping and falling motions
        /// </summary>
        /// <param name="rMotionController"></param>
        /// <param name="rUseBasicJump"></param>
        public static void CreateJumpMotions(MotionController rMotionController, bool rUseBasicJump = false)
        {
            string[] defaultTags = DefaultClimbMotionTags;
            // Add the standard set of motions used in the ootii Demos
            Jump lJump = rMotionController.CreateMotion<Jump>(0, defaultTags);
            lJump.IsEnabled = !rUseBasicJump;
            rMotionController.SerializeMotion(lJump);

            rMotionController.CreateMotion<Fall>(0, defaultTags);

            if (rUseBasicJump)
            {
                // Create the BasicJump, which uses the current Motion Form to select which animation to play
                BasicJump lBasicJump = rMotionController.GetMotion<BasicJump>(0, true);
                if (lBasicJump == null) { lBasicJump = rMotionController.CreateMotion<BasicJump>(0); }
                lBasicJump.IsMomentumEnabled = true;
                lBasicJump.IsControlEnabled = true;
                lBasicJump.AllowSliding = false;
                lBasicJump.AddTag(DefaultMotionTags.DisableAdditive);
                rMotionController.SerializeMotion(lBasicJump);
            }
        }
       
        /// <summary>
        /// Create the standard Vault 1m motion.
        /// </summary>
        /// <param name="rMotionController"></param>
        /// <param name="rLayerMask"></param>
        /// <param name="rAllowRunning"></param>
        public static void CreateVaultMotion(MotionController rMotionController, LayerMask rLayerMask, bool rAllowRunning)
        {
            Vault_1m lVault = rMotionController.CreateMotion<Vault_1m>(0, DefaultClimbMotionTags);
            lVault.ClimbableLayers = rLayerMask;
            lVault.AllowRunningVault = rAllowRunning;
            rMotionController.SerializeMotion(lVault);
        }

        /// <summary>
        /// Create the standard Balance Walk motion
        /// </summary>
        /// <param name="rMotionController"></param>
        /// <param name="rLayerMask"></param>
        /// <param name="rUseLayers"></param>
        /// <param name="rUseRaycasts"></param>
        public static void CreateBalanceWalkMotion(MotionController rMotionController, LayerMask rLayerMask, bool rUseLayers, bool rUseRaycasts)
        {
            BalanceWalk lBalanceWalk = rMotionController.CreateMotion<BalanceWalk>(0, DefaultClimbMotionTags);
            lBalanceWalk.ActivateUsingLayers = rUseLayers;
            lBalanceWalk.ActivateUsingRaycasts = rUseRaycasts;
            lBalanceWalk.BalanceLayers = rLayerMask;
            rMotionController.SerializeMotion(lBalanceWalk);
        }
       
        /// <summary>
        /// Create the motions which use the Utility-SM animations: Stunned, Frozen, Sleep, PushedBack, etc.
        /// These are primarily reactions which were introduced with the Spellcasting Motion Pack
        /// </summary>
        /// <param name="rMotionController"></param>
        /// <param name="rUseRootMotion"></param>
        public static void CreateUtilityMotions(MotionController rMotionController, bool rUseRootMotion = false)
        {
#pragma warning disable CS0219     
            Stunned lStunned = rMotionController.CreateMotion<Stunned>(0);
            Frozen lFrozen = rMotionController.CreateMotion<Frozen>(0);
            Cower lCower = rMotionController.CreateMotion<Cower>(0);
            KnockedDown lKnockedDown = rMotionController.CreateMotion<KnockedDown>(0);
            Sleep lSleep = rMotionController.CreateMotion<Sleep>(0);
            PushedBack lPushed = rMotionController.CreateMotion<PushedBack>(0);
            Levitate lLevitate = rMotionController.CreateMotion<Levitate>(0);
#pragma warning restore 0219

            if (rUseRootMotion)
            {
                lKnockedDown.AddTag(DefaultMotionTags.UseRootMotion);
                lSleep.AddTag(DefaultMotionTags.UseRootMotion);
            }
        }

        public static void CreateSneakMotion(MotionController rMotionController)
        {
            Sneak_v2 lSneak = rMotionController.CreateMotion<Sneak_v2>(0);
            rMotionController.SerializeMotion(lSneak);
        }

        public static void CreateRunningJumpMotion(MotionController rMotionController)
        {            
            RunningJump lRunningJump = rMotionController.CreateMotion<RunningJump>(0, DefaultClimbMotionTags);
            rMotionController.SerializeMotion(lRunningJump);
        }

        public void CreateSlideMotion(MotionController rMotionController)
        {
            Slide lSlide = rMotionController.CreateMotion<Slide>(0);
            rMotionController.SerializeMotion(lSlide);
        }

        #endregion Create Standard Motions


        #region Verify Basic Animator States

        /// <summary>
        /// Verifies that the Start state (Idle Pose) has been created on the Base Layer and that it is set to be the default
        /// layer state.
        /// </summary>
        /// <param name="rController"></param>
        /// <returns></returns>
        public static AnimatorStateMachine EnsureBaseLayerStartState(AnimatorController rController)
        {            
            if (rController == null) return null;
            
            AnimatorStateMachine lLayerStateMachine = rController.layers[0].stateMachine;

            AnimatorState lDefaultState = MotionControllerMotion.EditorFindState(lLayerStateMachine, "Start");
            if (lDefaultState == null) { lDefaultState = lLayerStateMachine.AddState("Start", new Vector3(30, 200, 0)); }
            lDefaultState.speed = 1;
            lDefaultState.mirror = false;
            lDefaultState.tag = "";
            lDefaultState.motion = MotionControllerMotion.EditorFindAnimationClip(DefaultPaths.StandardAnimations + "Idling/unity_Idle_IdleToIdlesR.fbx", "IdlePose");

            lLayerStateMachine.defaultState = lDefaultState;

            return lLayerStateMachine;
        }
      
        /// <summary>
        /// Verifies that the BasicIdle SSM has been created and has a default animation state
        /// </summary>
        /// <param name="rController"></param>
        /// <param name="rLayerIndex"></param>
        /// <param name="rCreateStates"></param>
        public static AnimatorStateMachine EnsureBasicIdleSSM(AnimatorController rController, int rLayerIndex, bool rCreateStates = true)
        {            
            if (rController == null) return null;
            //Log.ConsoleWrite("EnsureBasicIdleStates: Begin");
            
            AnimatorStateMachine lLayerStateMachine = rController.layers[rLayerIndex].stateMachine;
            
            // Create the state machine
            AnimatorStateMachine lIdleSSM = MotionControllerMotion.EditorFindSSM(lLayerStateMachine, "BasicIdle-SM");
            if (lIdleSSM == null) { lIdleSSM = lLayerStateMachine.AddStateMachine("BasicIdle-SM", new Vector3(192, -1056, 0)); }

            if (!rCreateStates) return lIdleSSM;
           
            // Create the animation state
            AnimatorState lIdleState = MotionControllerMotion.EditorFindState(lIdleSSM, "Unarmed Idle Pose");
            if (lIdleState == null) { lIdleState = lIdleSSM.AddState("Unarmed Idle Pose", new Vector3(312, 84, 0)); }
            lIdleState.speed = 1f;
            lIdleState.mirror = false;
            lIdleState.tag = "";
            lIdleState.motion = MotionControllerMotion.EditorFindAnimationClip(DefaultPaths.StandardAnimations + "Idling/unity_Idle_IdleToIdlesR.fbx", "IdlePose");

            // Add the Any State transitons
            AnimatorStateTransition lAnyTransition_38106 = MotionControllerMotion.EditorFindAnyStateTransition(lLayerStateMachine, lIdleState, 0);
            if (lAnyTransition_38106 == null) { lAnyTransition_38106 = lLayerStateMachine.AddAnyStateTransition(lIdleState); }
            lAnyTransition_38106.isExit = false;
            lAnyTransition_38106.hasExitTime = false;
            lAnyTransition_38106.hasFixedDuration = true;
            lAnyTransition_38106.exitTime = 0.75f;
            lAnyTransition_38106.duration = 0.1f;
            lAnyTransition_38106.offset = 0f;
            lAnyTransition_38106.mute = false;
            lAnyTransition_38106.solo = false;
            lAnyTransition_38106.canTransitionToSelf = true;
            lAnyTransition_38106.interruptionSource = (TransitionInterruptionSource)0;
            for (int i = lAnyTransition_38106.conditions.Length - 1; i >= 0; i--) { lAnyTransition_38106.RemoveCondition(lAnyTransition_38106.conditions[i]); }
            lAnyTransition_38106.AddCondition(AnimatorConditionMode.Equals, 3000f, "L" + rLayerIndex + "MotionPhase");
            lAnyTransition_38106.AddCondition(AnimatorConditionMode.Equals, 0f, "L" + rLayerIndex + "MotionForm");
            lAnyTransition_38106.AddCondition(AnimatorConditionMode.Equals, 0f, "L" + rLayerIndex + "MotionParameter");

            AnimatorStateTransition lAnyTransition_38108 = MotionControllerMotion.EditorFindAnyStateTransition(lLayerStateMachine, lIdleState, 1);
            if (lAnyTransition_38108 == null) { lAnyTransition_38108 = lLayerStateMachine.AddAnyStateTransition(lIdleState); }
            lAnyTransition_38108.isExit = false;
            lAnyTransition_38108.hasExitTime = false;
            lAnyTransition_38108.hasFixedDuration = true;
            lAnyTransition_38108.exitTime = 0.75f;
            lAnyTransition_38108.duration = 0f;
            lAnyTransition_38108.offset = 0f;
            lAnyTransition_38108.mute = false;
            lAnyTransition_38108.solo = false;
            lAnyTransition_38108.canTransitionToSelf = true;
            lAnyTransition_38108.interruptionSource = (TransitionInterruptionSource)0;
            for (int i = lAnyTransition_38108.conditions.Length - 1; i >= 0; i--) { lAnyTransition_38108.RemoveCondition(lAnyTransition_38108.conditions[i]); }
            lAnyTransition_38108.AddCondition(AnimatorConditionMode.Equals, 3000f, "L" + rLayerIndex + "MotionPhase");
            lAnyTransition_38108.AddCondition(AnimatorConditionMode.Equals, 0f, "L" + rLayerIndex + "MotionForm");
            lAnyTransition_38108.AddCondition(AnimatorConditionMode.Equals, 1f, "L" + rLayerIndex + "MotionParameter");


            //Log.ConsoleWrite("EnsureBasicIdleStates: End");
            return lIdleSSM;
        }

        /// <summary>
        /// Verifies that the BasicWalkRunPivot SSM has been created and has a default animation state
        /// </summary>
        /// <param name="rController"></param>
        /// <param name="rLayerIndex"></param>
        /// <param name="rCreateStates"></param>
        public static AnimatorStateMachine EnsureBasicWalkRunPivotSSM(AnimatorController rController, int rLayerIndex, bool rCreateStates = true)
        {            
            if (rController == null) return null;

            AnimatorStateMachine lLayerStateMachine = rController.layers[rLayerIndex].stateMachine;

            AnimatorStateMachine lWalkRunPivotSSM = MotionControllerMotion.EditorFindSSM(lLayerStateMachine, "BasicWalkRunPivot-SM");
            if (lWalkRunPivotSSM == null) { lWalkRunPivotSSM = lLayerStateMachine.AddStateMachine("BasicWalkRunPivot-SM", new Vector3(408, -1056, 0)); }

            if (!rCreateStates)return lWalkRunPivotSSM; 

            AnimatorState lBaseState = MotionControllerMotion.EditorFindState(lWalkRunPivotSSM, "Unarmed BlendTree");
            if (lBaseState == null) { lBaseState = lWalkRunPivotSSM.AddState("Unarmed BlendTree", new Vector3(312, 72, 0)); }
            lBaseState.speed = 1f;
            lBaseState.mirror = false;
            lBaseState.tag = "";

            BlendTree lPivotTree = MotionControllerMotion.EditorCreateBlendTree("Blend Tree", rController, rLayerIndex);
            lPivotTree.blendType = BlendTreeType.Simple1D;
            lPivotTree.blendParameter = "InputMagnitude";
            lPivotTree.blendParameterY = "InputX";
#if !(UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_4 || UNITY_4_5 || UNITY_4_6 || UNITY_5_0 || UNITY_5_1 || UNITY_5_2 || UNITY_5_3)
            lPivotTree.useAutomaticThresholds = true;
#endif
            lPivotTree.AddChild(MotionControllerMotion.EditorFindAnimationClip(DefaultPaths.StandardAnimations + "Idling/unity_Idle_IdleToIdlesR.fbx", "IdlePose"), 0f);
            lPivotTree.AddChild(MotionControllerMotion.EditorFindAnimationClip(DefaultPaths.StandardAnimations + "Walking/unity_WalkFWD_v2.fbx", "WalkForward"), 0.5f);
            lPivotTree.AddChild(MotionControllerMotion.EditorFindAnimationClip(DefaultPaths.StandardAnimations + "Running/RunForward_v2.fbx", "RunForward"), 1f);
            lBaseState.motion = lPivotTree;

            AnimatorStateTransition lAnyTransition_32133 = MotionControllerMotion.EditorFindAnyStateTransition(lLayerStateMachine, lBaseState, 0);
            if (lAnyTransition_32133 == null) { lAnyTransition_32133 = lLayerStateMachine.AddAnyStateTransition(lBaseState); }
            lAnyTransition_32133.isExit = false;
            lAnyTransition_32133.hasExitTime = false;
            lAnyTransition_32133.hasFixedDuration = true;
            lAnyTransition_32133.exitTime = 0.75f;
            lAnyTransition_32133.duration = 0.25f;
            lAnyTransition_32133.offset = 0.5f;
            lAnyTransition_32133.mute = false;
            lAnyTransition_32133.solo = false;
            lAnyTransition_32133.canTransitionToSelf = true;
            lAnyTransition_32133.interruptionSource = (TransitionInterruptionSource)0;
            for (int i = lAnyTransition_32133.conditions.Length - 1; i >= 0; i--) { lAnyTransition_32133.RemoveCondition(lAnyTransition_32133.conditions[i]); }
            lAnyTransition_32133.AddCondition(AnimatorConditionMode.Equals, 3050f, "L" + rLayerIndex + "MotionPhase");
            lAnyTransition_32133.AddCondition(AnimatorConditionMode.Equals, 0f, "L" + rLayerIndex + "MotionForm");
            lAnyTransition_32133.AddCondition(AnimatorConditionMode.Equals, 1f, "L" + rLayerIndex + "MotionParameter");

            AnimatorStateTransition lAnyTransition_32132 = MotionControllerMotion.EditorFindAnyStateTransition(lLayerStateMachine, lBaseState, 1);
            if (lAnyTransition_32132 == null) { lAnyTransition_32132 = lLayerStateMachine.AddAnyStateTransition(lBaseState); }
            lAnyTransition_32132.isExit = false;
            lAnyTransition_32132.hasExitTime = false;
            lAnyTransition_32132.hasFixedDuration = true;
            lAnyTransition_32132.exitTime = 0.75f;
            lAnyTransition_32132.duration = 0.25f;
            lAnyTransition_32132.offset = 0f;
            lAnyTransition_32132.mute = false;
            lAnyTransition_32132.solo = false;
            lAnyTransition_32132.canTransitionToSelf = true;
            lAnyTransition_32132.interruptionSource = (TransitionInterruptionSource)0;
            for (int i = lAnyTransition_32132.conditions.Length - 1; i >= 0; i--) { lAnyTransition_32132.RemoveCondition(lAnyTransition_32132.conditions[i]); }
            lAnyTransition_32132.AddCondition(AnimatorConditionMode.Equals, 3050f, "L" + rLayerIndex + "MotionPhase");
            lAnyTransition_32132.AddCondition(AnimatorConditionMode.Equals, 0f, "L" + rLayerIndex + "MotionForm");
            lAnyTransition_32132.AddCondition(AnimatorConditionMode.Equals, 0f, "L" + rLayerIndex + "MotionParameter");

            return lWalkRunPivotSSM;
        }

        /// <summary>
        /// Verifies that the BasicWalkRunStrafe SSM has been created and has a default animation state
        /// </summary>
        /// <param name="rController"></param>
        /// <param name="rLayerIndex"></param>
        /// <param name="rCreateStates"></param>
        public static AnimatorStateMachine EnsureBasicWalkRunStrafeSSM(AnimatorController rController, int rLayerIndex, bool rCreateStates = true)
        {            
            if (rController == null) return null;

            AnimatorStateMachine lLayerStateMachine = rController.layers[rLayerIndex].stateMachine;

            AnimatorStateMachine lWalkRunStrafeSSM = MotionControllerMotion.EditorFindSSM(lLayerStateMachine, "BasicWalkRunStrafe-SM");
            if (lWalkRunStrafeSSM == null) { lWalkRunStrafeSSM = lLayerStateMachine.AddStateMachine("BasicWalkRunStrafe-SM", new Vector3(408, -1008, 0)); }

            if (!rCreateStates) return lWalkRunStrafeSSM;

            AnimatorState lBaseState = MotionControllerMotion.EditorFindState(lWalkRunStrafeSSM, "Unarmed BlendTree");
            if (lBaseState == null) { lBaseState = lWalkRunStrafeSSM.AddState("Unarmed BlendTree", new Vector3(336, 24, 0)); }
            lBaseState.speed = 1f;
            lBaseState.mirror = false;
            lBaseState.tag = "";

            BlendTree lRootTree = MotionControllerMotion.EditorCreateBlendTree("Move Blend Tree", rController, rLayerIndex);
            lRootTree.blendType = BlendTreeType.Simple1D;
            lRootTree.blendParameter = "InputMagnitude";
            lRootTree.blendParameterY = "InputX";
#if !(UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_4 || UNITY_4_5 || UNITY_4_6 || UNITY_5_0 || UNITY_5_1 || UNITY_5_2 || UNITY_5_3)
            lRootTree.useAutomaticThresholds = true;
#endif
            lRootTree.AddChild(MotionControllerMotion.EditorFindAnimationClip(DefaultPaths.StandardAnimations + "Idling/unity_Idle_IdleToIdlesR.fbx", "IdlePose"), 0f);

            BlendTree lWalkTree = MotionControllerMotion.EditorCreateBlendTree("WalkTree", rController, rLayerIndex);
            lWalkTree.blendType = BlendTreeType.SimpleDirectional2D;
            lWalkTree.blendParameter = "InputX";
            lWalkTree.blendParameterY = "InputY";
#if !(UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_4 || UNITY_4_5 || UNITY_4_6 || UNITY_5_0 || UNITY_5_1 || UNITY_5_2 || UNITY_5_3)
            lWalkTree.useAutomaticThresholds = true;
#endif
            lWalkTree.AddChild(MotionControllerMotion.EditorFindAnimationClip(DefaultPaths.StandardAnimations + "Walking/unity_WalkFWD_v2.fbx", "WalkForward"), new Vector2(0f, 0.35f));
            ChildMotion[] lM_28552_0_Children = lWalkTree.children;
            lM_28552_0_Children[lM_28552_0_Children.Length - 1].mirror = false;
            lM_28552_0_Children[lM_28552_0_Children.Length - 1].timeScale = 1.1f;
            lWalkTree.children = lM_28552_0_Children;

            lWalkTree.AddChild(MotionControllerMotion.EditorFindAnimationClip(DefaultPaths.StandardAnimations + "Walking/unity_SWalk_v2.fbx", "SWalkForwardRight"), new Vector2(0.35f, 0.35f));
            ChildMotion[] lM_28552_1_Children = lWalkTree.children;
            lM_28552_1_Children[lM_28552_1_Children.Length - 1].mirror = false;
            lM_28552_1_Children[lM_28552_1_Children.Length - 1].timeScale = 1.2f;
            lWalkTree.children = lM_28552_1_Children;

            lWalkTree.AddChild(MotionControllerMotion.EditorFindAnimationClip(DefaultPaths.StandardAnimations + "Walking/unity_SWalk_v2.fbx", "SWalkForwardLeft"), new Vector2(-0.35f, 0.35f));
            ChildMotion[] lM_28552_2_Children = lWalkTree.children;
            lM_28552_2_Children[lM_28552_2_Children.Length - 1].mirror = false;
            lM_28552_2_Children[lM_28552_2_Children.Length - 1].timeScale = 1.2f;
            lWalkTree.children = lM_28552_2_Children;

            lWalkTree.AddChild(MotionControllerMotion.EditorFindAnimationClip(DefaultPaths.StandardAnimations + "Walking/unity_SWalk_v2.fbx", "SWalkLeft"), new Vector2(-0.35f, 0f));
            ChildMotion[] lM_28552_3_Children = lWalkTree.children;
            lM_28552_3_Children[lM_28552_3_Children.Length - 1].mirror = false;
            lM_28552_3_Children[lM_28552_3_Children.Length - 1].timeScale = 1.2f;
            lWalkTree.children = lM_28552_3_Children;

            lWalkTree.AddChild(MotionControllerMotion.EditorFindAnimationClip(DefaultPaths.StandardAnimations + "Walking/unity_SWalk_v2.fbx", "SWalkRight"), new Vector2(0.35f, 0f));
            ChildMotion[] lM_28552_4_Children = lWalkTree.children;
            lM_28552_4_Children[lM_28552_4_Children.Length - 1].mirror = false;
            lM_28552_4_Children[lM_28552_4_Children.Length - 1].timeScale = 1.2f;
            lWalkTree.children = lM_28552_4_Children;

            lWalkTree.AddChild(MotionControllerMotion.EditorFindAnimationClip(DefaultPaths.StandardAnimations + "Walking/unity_Idle2Strafe_AllAngles.fbx", "WalkStrafeBackwardsLeft"), new Vector2(-0.35f, -0.35f));
            ChildMotion[] lM_28552_5_Children = lWalkTree.children;
            lM_28552_5_Children[lM_28552_5_Children.Length - 1].mirror = false;
            lM_28552_5_Children[lM_28552_5_Children.Length - 1].timeScale = 1.1f;
            lWalkTree.children = lM_28552_5_Children;

            lWalkTree.AddChild(MotionControllerMotion.EditorFindAnimationClip(DefaultPaths.StandardAnimations + "Walking/unity_Idle2Strafe_AllAngles.fbx", "WalkStrafeBackwardsRight"), new Vector2(0.35f, -0.35f));
            ChildMotion[] lM_28552_6_Children = lWalkTree.children;
            lM_28552_6_Children[lM_28552_6_Children.Length - 1].mirror = false;
            lM_28552_6_Children[lM_28552_6_Children.Length - 1].timeScale = 1.1f;
            lWalkTree.children = lM_28552_6_Children;

            lWalkTree.AddChild(MotionControllerMotion.EditorFindAnimationClip(DefaultPaths.StandardAnimations + "Walking/unity_BWalk.fbx", "WalkBackwards"), new Vector2(0f, -0.35f));
            ChildMotion[] lM_28552_7_Children = lWalkTree.children;
            lM_28552_7_Children[lM_28552_7_Children.Length - 1].mirror = false;
            lM_28552_7_Children[lM_28552_7_Children.Length - 1].timeScale = 1f;
            lWalkTree.children = lM_28552_7_Children;

            lRootTree.AddChild(lWalkTree, 0.5f);

            BlendTree lRunTree = MotionControllerMotion.EditorCreateBlendTree("RunTree", rController, rLayerIndex);
            lRunTree.blendType = BlendTreeType.SimpleDirectional2D;
            lRunTree.blendParameter = "InputX";
            lRunTree.blendParameterY = "InputY";
#if !(UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_4 || UNITY_4_5 || UNITY_4_6 || UNITY_5_0 || UNITY_5_1 || UNITY_5_2 || UNITY_5_3)
            lRunTree.useAutomaticThresholds = true;
#endif
            lRunTree.AddChild(MotionControllerMotion.EditorFindAnimationClip(DefaultPaths.StandardAnimations + "Running/RunForward_v2.fbx", "RunForward"), new Vector2(0f, 0.7f));
            ChildMotion[] lM_28544_0_Children = lRunTree.children;
            lM_28544_0_Children[lM_28544_0_Children.Length - 1].mirror = false;
            lM_28544_0_Children[lM_28544_0_Children.Length - 1].timeScale = 1f;
            lRunTree.children = lM_28544_0_Children;

            lRunTree.AddChild(MotionControllerMotion.EditorFindAnimationClip(DefaultPaths.StandardAnimations + "Running/RunStrafe.fbx", "RunStrafeForwardRight"), new Vector2(0.7f, 0.7f));
            ChildMotion[] lM_28544_1_Children = lRunTree.children;
            lM_28544_1_Children[lM_28544_1_Children.Length - 1].mirror = false;
            lM_28544_1_Children[lM_28544_1_Children.Length - 1].timeScale = 1.1f;
            lRunTree.children = lM_28544_1_Children;

            lRunTree.AddChild(MotionControllerMotion.EditorFindAnimationClip(DefaultPaths.StandardAnimations + "Running/RunStrafe.fbx", "RunStrafeForwardLeft"), new Vector2(-0.7f, 0.7f));
            ChildMotion[] lM_28544_2_Children = lRunTree.children;
            lM_28544_2_Children[lM_28544_2_Children.Length - 1].mirror = false;
            lM_28544_2_Children[lM_28544_2_Children.Length - 1].timeScale = 1.1f;
            lRunTree.children = lM_28544_2_Children;

            lRunTree.AddChild(MotionControllerMotion.EditorFindAnimationClip(DefaultPaths.StandardAnimations + "Running/RunStrafe.fbx", "RunStrafeLeft"), new Vector2(-0.7f, 0f));
            ChildMotion[] lM_28544_3_Children = lRunTree.children;
            lM_28544_3_Children[lM_28544_3_Children.Length - 1].mirror = false;
            lM_28544_3_Children[lM_28544_3_Children.Length - 1].timeScale = 1f;
            lRunTree.children = lM_28544_3_Children;

            lRunTree.AddChild(MotionControllerMotion.EditorFindAnimationClip(DefaultPaths.StandardAnimations + "Running/RunStrafe.fbx", "RunStrafeRight"), new Vector2(0.7f, 0f));
            ChildMotion[] lM_28544_4_Children = lRunTree.children;
            lM_28544_4_Children[lM_28544_4_Children.Length - 1].mirror = false;
            lM_28544_4_Children[lM_28544_4_Children.Length - 1].timeScale = 1f;
            lRunTree.children = lM_28544_4_Children;

            lRunTree.AddChild(MotionControllerMotion.EditorFindAnimationClip(DefaultPaths.StandardAnimations + "Running/RunStrafe.fbx", "RunStrafeBackwardLeft"), new Vector2(-0.7f, -0.7f));
            ChildMotion[] lM_28544_5_Children = lRunTree.children;
            lM_28544_5_Children[lM_28544_5_Children.Length - 1].mirror = false;
            lM_28544_5_Children[lM_28544_5_Children.Length - 1].timeScale = 1.1f;
            lRunTree.children = lM_28544_5_Children;

            lRunTree.AddChild(MotionControllerMotion.EditorFindAnimationClip(DefaultPaths.StandardAnimations + "Running/RunStrafe.fbx", "RunStrafeBackwardRight"), new Vector2(0.7f, -0.7f));
            ChildMotion[] lM_28544_6_Children = lRunTree.children;
            lM_28544_6_Children[lM_28544_6_Children.Length - 1].mirror = false;
            lM_28544_6_Children[lM_28544_6_Children.Length - 1].timeScale = 1.1f;
            lRunTree.children = lM_28544_6_Children;

            lRunTree.AddChild(MotionControllerMotion.EditorFindAnimationClip(DefaultPaths.StandardAnimations + "Running/RunBackward.fbx", "RunBackwards"), new Vector2(0f, -0.7f));
            ChildMotion[] lM_28544_7_Children = lRunTree.children;
            lM_28544_7_Children[lM_28544_7_Children.Length - 1].mirror = false;
            lM_28544_7_Children[lM_28544_7_Children.Length - 1].timeScale = 1f;
            lRunTree.children = lM_28544_7_Children;

            lRootTree.AddChild(lRunTree, 1f);
            lBaseState.motion = lRootTree;

            AnimatorStateTransition lAnyTransition_32133 = MotionControllerMotion.EditorFindAnyStateTransition(lLayerStateMachine, lBaseState, 0);
            if (lAnyTransition_32133 == null) { lAnyTransition_32133 = lLayerStateMachine.AddAnyStateTransition(lBaseState); }
            lAnyTransition_32133.isExit = false;
            lAnyTransition_32133.hasExitTime = false;
            lAnyTransition_32133.hasFixedDuration = true;
            lAnyTransition_32133.exitTime = 0.75f;
            lAnyTransition_32133.duration = 0.25f;
            lAnyTransition_32133.offset = 0.5f;
            lAnyTransition_32133.mute = false;
            lAnyTransition_32133.solo = false;
            lAnyTransition_32133.canTransitionToSelf = true;
            lAnyTransition_32133.interruptionSource = (TransitionInterruptionSource)0;
            for (int i = lAnyTransition_32133.conditions.Length - 1; i >= 0; i--) { lAnyTransition_32133.RemoveCondition(lAnyTransition_32133.conditions[i]); }
            lAnyTransition_32133.AddCondition(AnimatorConditionMode.Equals, 3100f, "L" + rLayerIndex + "MotionPhase");
            lAnyTransition_32133.AddCondition(AnimatorConditionMode.Equals, 0f, "L" + rLayerIndex + "MotionForm");
            lAnyTransition_32133.AddCondition(AnimatorConditionMode.Equals, 1f, "L" + rLayerIndex + "MotionParameter");

            AnimatorStateTransition lAnyTransition_32134 = MotionControllerMotion.EditorFindAnyStateTransition(lLayerStateMachine, lBaseState, 1);
            if (lAnyTransition_32134 == null) { lAnyTransition_32134 = lLayerStateMachine.AddAnyStateTransition(lBaseState); }
            lAnyTransition_32134.isExit = false;
            lAnyTransition_32134.hasExitTime = false;
            lAnyTransition_32134.hasFixedDuration = true;
            lAnyTransition_32134.exitTime = 0.9f;
            lAnyTransition_32134.duration = 0.25f;
            lAnyTransition_32134.offset = 0f;
            lAnyTransition_32134.mute = false;
            lAnyTransition_32134.solo = false;
            lAnyTransition_32134.canTransitionToSelf = true;
            lAnyTransition_32134.interruptionSource = (TransitionInterruptionSource)0;
            for (int i = lAnyTransition_32134.conditions.Length - 1; i >= 0; i--) { lAnyTransition_32134.RemoveCondition(lAnyTransition_32134.conditions[i]); }
            lAnyTransition_32134.AddCondition(AnimatorConditionMode.Equals, 3100f, "L" + rLayerIndex + "MotionPhase");
            lAnyTransition_32134.AddCondition(AnimatorConditionMode.Equals, 0f, "L" + rLayerIndex + "MotionForm");
            lAnyTransition_32134.AddCondition(AnimatorConditionMode.Equals, 0f, "L" + rLayerIndex + "MotionParameter");
            
            
            return lWalkRunStrafeSSM;
        }

        /// <summary>
        /// Verifies that the BasicJump SSM has been created and has a default animation state
        /// </summary>
        /// <param name="rController"></param>
        /// <param name="rLayerIndex"></param>
        /// <param name="rCreateStates"></param>
        public static AnimatorStateMachine EnsureBasicJumpSSM(AnimatorController rController, int rLayerIndex, bool rCreateStates = true)
        {            
            if (rController == null) return null;

            AnimatorStateMachine lLayerStateMachine = rController.layers[rLayerIndex].stateMachine;

            AnimatorStateMachine lJumpSSM = MotionControllerMotion.EditorFindSSM(lLayerStateMachine, "BasicJump-SM");
            if (lJumpSSM == null) { lJumpSSM = lLayerStateMachine.AddStateMachine("BasicJump-SM", new Vector3(192, -864, 0)); }

            if (!rCreateStates) return lJumpSSM;

            AnimatorState lJumpState = MotionControllerMotion.EditorFindState(lJumpSSM, "Unarmed Jump");
            if (lJumpState == null) { lJumpState = lJumpSSM.AddState("Unarmed Jump", new Vector3(360, -60, 0)); }
            lJumpState.speed = 1.1f;
            lJumpState.mirror = false;
            lJumpState.tag = "";
            lJumpState.motion = MotionControllerMotion.EditorFindAnimationClip(DefaultPaths.StandardAnimations + "Jumping/ootii_StandingJump.fbx", "Jump");

            AnimatorState lExitState = MotionControllerMotion.EditorFindState(lJumpSSM, "IdlePose");
            if (lExitState == null) { lExitState = lJumpSSM.AddState("IdlePose", new Vector3(600, -60, 0)); }
            lExitState.speed = 1f;
            lExitState.mirror = false;
            lExitState.tag = "Exit";
            lExitState.motion = MotionControllerMotion.EditorFindAnimationClip(DefaultPaths.StandardAnimations + "Idling/unity_Idle_IdleToIdlesR.fbx", "IdlePose");

            AnimatorStateTransition lAnyTransition_302716 = MotionControllerMotion.EditorFindAnyStateTransition(lLayerStateMachine, lJumpState, 0);
            if (lAnyTransition_302716 == null) { lAnyTransition_302716 = lLayerStateMachine.AddAnyStateTransition(lJumpState); }
            lAnyTransition_302716.isExit = false;
            lAnyTransition_302716.hasExitTime = false;
            lAnyTransition_302716.hasFixedDuration = true;
            lAnyTransition_302716.exitTime = 0.75f;
            lAnyTransition_302716.duration = 0.25f;
            lAnyTransition_302716.offset = 0f;
            lAnyTransition_302716.mute = false;
            lAnyTransition_302716.solo = false;
            lAnyTransition_302716.canTransitionToSelf = true;
            lAnyTransition_302716.interruptionSource = (TransitionInterruptionSource)0;
            for (int i = lAnyTransition_302716.conditions.Length - 1; i >= 0; i--) { lAnyTransition_302716.RemoveCondition(lAnyTransition_302716.conditions[i]); }
            lAnyTransition_302716.AddCondition(AnimatorConditionMode.Equals, 3400f, "L" + rLayerIndex + "MotionPhase");
            lAnyTransition_302716.AddCondition(AnimatorConditionMode.Equals, 0f, "L" + rLayerIndex + "MotionForm");
            lAnyTransition_302716.AddCondition(AnimatorConditionMode.Equals, 0f, "L" + rLayerIndex + "MotionParameter");

            AnimatorStateTransition lTransition_297782 = MotionControllerMotion.EditorFindTransition(lJumpState, lExitState, 0);
            if (lTransition_297782 == null) { lTransition_297782 = lJumpState.AddTransition(lExitState); }
            lTransition_297782.isExit = false;
            lTransition_297782.hasExitTime = true;
            lTransition_297782.hasFixedDuration = true;
            lTransition_297782.exitTime = 0.9f;
            lTransition_297782.duration = 0.1f;
            lTransition_297782.offset = 0f;
            lTransition_297782.mute = false;
            lTransition_297782.solo = false;
            lTransition_297782.canTransitionToSelf = true;
            lTransition_297782.interruptionSource = (TransitionInterruptionSource)0;
            for (int i = lTransition_297782.conditions.Length - 1; i >= 0; i--) { lTransition_297782.RemoveCondition(lTransition_297782.conditions[i]); }

            return lJumpSSM;
        }

        /// <summary>
        /// Verifies that the BasicDamaged SSM has been created and has a default animation state
        /// </summary>
        /// <param name="rController"></param>
        /// <param name="rLayerIndex"></param>
        /// <param name="rCreateStates"></param>
        public static AnimatorStateMachine EnsureBasicDamagedSSM(AnimatorController rController, int rLayerIndex,
            bool rCreateStates = true)
        {
            if (rController == null) return null;

            AnimatorStateMachine lLayerStateMachine = rController.layers[rLayerIndex].stateMachine;

            AnimatorStateMachine lDamagedSSM = MotionControllerMotion.EditorFindSSM(lLayerStateMachine, "BasicDamaged-SM");
            if (lDamagedSSM == null) { lDamagedSSM = lLayerStateMachine.AddStateMachine("BasicDamaged-SM", new Vector3(192, -960, 0)); }
            if (!rCreateStates) return lDamagedSSM;

            AnimatorState lDamagedState = MotionControllerMotion.EditorFindState(lDamagedSSM, "Unarmed Damaged 0");
            if (lDamagedState == null) { lDamagedState = lDamagedSSM.AddState("Unarmed Damaged 0", new Vector3(312, -24, 0)); }

            lDamagedState.speed = 3f;
            lDamagedState.mirror = false;
            lDamagedState.tag = "";
            lDamagedState.motion = MotionControllerMotion.EditorFindAnimationClip(
                "Assets/ootii/Assets/MotionController/Content/Animations/Humanoid/Utilities/Utilities_01.fbx", "Damaged");

            AnimatorState lExitState = MotionControllerMotion.EditorFindState(lDamagedSSM, "IdlePose");
            if (lExitState == null) { lExitState = lDamagedSSM.AddState("IdlePose", new Vector3(600, -24, 0)); }

            lExitState.speed = 1f;
            lExitState.mirror = false;
            lExitState.tag = "Exit";
            lExitState.motion = MotionControllerMotion.EditorFindAnimationClip(
                "Assets/ootii/Assets/MotionController/Content/Animations/Humanoid/Idling/ootii_Idle.fbx", "IdlePose");

            AnimatorStateTransition lAnyTransition_11562 = MotionControllerMotion.EditorFindAnyStateTransition(lLayerStateMachine, lDamagedState, 0);
            if (lAnyTransition_11562 == null) { lAnyTransition_11562 = lLayerStateMachine.AddAnyStateTransition(lDamagedState); }

            lAnyTransition_11562.isExit = false;
            lAnyTransition_11562.hasExitTime = false;
            lAnyTransition_11562.hasFixedDuration = true;
            lAnyTransition_11562.exitTime = 0.75f;
            lAnyTransition_11562.duration = 0.1f;
            lAnyTransition_11562.offset = 0.106185f;
            lAnyTransition_11562.mute = false;
            lAnyTransition_11562.solo = false;
            lAnyTransition_11562.canTransitionToSelf = false;
            lAnyTransition_11562.orderedInterruption = true;
            lAnyTransition_11562.interruptionSource = (TransitionInterruptionSource) 0;
            for (int i = lAnyTransition_11562.conditions.Length - 1; i >= 0; i--) { lAnyTransition_11562.RemoveCondition(lAnyTransition_11562.conditions[i]); }

            lAnyTransition_11562.AddCondition(AnimatorConditionMode.Equals, 3350f, "L" + rLayerIndex + "MotionPhase");
            lAnyTransition_11562.AddCondition(AnimatorConditionMode.Equals, 0f, "L" + rLayerIndex + "MotionForm");

            AnimatorStateTransition lTransition_11736 = MotionControllerMotion.EditorFindTransition(lDamagedState, lExitState, 0);
            if (lTransition_11736 == null) { lTransition_11736 = lDamagedState.AddTransition(lExitState); }

            lTransition_11736.isExit = false;
            lTransition_11736.hasExitTime = true;
            lTransition_11736.hasFixedDuration = true;
            lTransition_11736.exitTime = 0.9318182f;
            lTransition_11736.duration = 0.15f;
            lTransition_11736.offset = 0f;
            lTransition_11736.mute = false;
            lTransition_11736.solo = false;
            lTransition_11736.canTransitionToSelf = true;
            lTransition_11736.orderedInterruption = true;
            lTransition_11736.interruptionSource = (TransitionInterruptionSource) 0;
            for (int i = lTransition_11736.conditions.Length - 1; i >= 0; i--) { lTransition_11736.RemoveCondition(lTransition_11736.conditions[i]); }

            return lDamagedSSM;
        }

        /// <summary>
        /// Verifies that the BasicDeath SSM has been created and has a default animation state
        /// </summary>        
        /// <param name="rController"></param>
        /// <param name="rLayerIndex"></param>
        /// <param name="rCreateStates"></param>
        public static AnimatorStateMachine EnsureBasicDeathSSM(AnimatorController rController, int rLayerIndex, bool rCreateStates = true)
        {
            if (rController == null) return null;

            AnimatorStateMachine lLayerStateMachine = rController.layers[rLayerIndex].stateMachine;

            AnimatorStateMachine lDeathSSM = MotionControllerMotion.EditorFindSSM(lLayerStateMachine, "BasicDeath-SM");
            if (lDeathSSM == null) { lDeathSSM = lLayerStateMachine.AddStateMachine("BasicDeath-SM", new Vector3(192, -912, 0)); }

            if (!rCreateStates) return lDeathSSM;

            AnimatorState lFromFrontState = MotionControllerMotion.EditorFindState(lDeathSSM, "Unarmed Death 0");
            if (lFromFrontState == null) { lFromFrontState = lDeathSSM.AddState("Unarmed Death 0", new Vector3(324, -72, 0)); }
            lFromFrontState.speed = 1.5f;
            lFromFrontState.mirror = false;
            lFromFrontState.tag = "";
            lFromFrontState.motion = MotionControllerMotion.EditorFindAnimationClip(DefaultPaths.StandardAnimations + "Utilities/Utilities_01.fbx", "DeathBackward");

            AnimatorState lFromBehindState = MotionControllerMotion.EditorFindState(lDeathSSM, "Unarmed Death 180");
            if (lFromBehindState == null) { lFromBehindState = lDeathSSM.AddState("Unarmed Death 180", new Vector3(324, -24, 0)); }
            lFromBehindState.speed = 1.8f;
            lFromBehindState.mirror = false;
            lFromBehindState.tag = "";
            lFromBehindState.motion = MotionControllerMotion.EditorFindAnimationClip(DefaultPaths.StandardAnimations + "Utilities/Utilities_01.fbx", "DeathForward");

            AnimatorStateTransition lAnyTransition_312814 = MotionControllerMotion.EditorFindAnyStateTransition(lLayerStateMachine, lFromFrontState, 0);
            if (lAnyTransition_312814 == null) { lAnyTransition_312814 = lLayerStateMachine.AddAnyStateTransition(lFromFrontState); }
            lAnyTransition_312814.isExit = false;
            lAnyTransition_312814.hasExitTime = false;
            lAnyTransition_312814.hasFixedDuration = true;
            lAnyTransition_312814.exitTime = 0.75f;
            lAnyTransition_312814.duration = 0.1f;
            lAnyTransition_312814.offset = 0.115787f;
            lAnyTransition_312814.mute = false;
            lAnyTransition_312814.solo = false;
            lAnyTransition_312814.canTransitionToSelf = true;
            lAnyTransition_312814.interruptionSource = (TransitionInterruptionSource)0;
            for (int i = lAnyTransition_312814.conditions.Length - 1; i >= 0; i--) { lAnyTransition_312814.RemoveCondition(lAnyTransition_312814.conditions[i]); }
            lAnyTransition_312814.AddCondition(AnimatorConditionMode.Equals, 3375f, "L" + rLayerIndex + "MotionPhase");
            lAnyTransition_312814.AddCondition(AnimatorConditionMode.Equals, 0f, "L" + rLayerIndex + "MotionForm");
            lAnyTransition_312814.AddCondition(AnimatorConditionMode.Greater, -100f, "L" + rLayerIndex + "MotionParameter");
            lAnyTransition_312814.AddCondition(AnimatorConditionMode.Less, 100f, "L" + rLayerIndex + "MotionParameter");

            AnimatorStateTransition lAnyTransition_304334 = MotionControllerMotion.EditorFindAnyStateTransition(lLayerStateMachine, lFromBehindState, 0);
            if (lAnyTransition_304334 == null) { lAnyTransition_304334 = lLayerStateMachine.AddAnyStateTransition(lFromBehindState); }
            lAnyTransition_304334.isExit = false;
            lAnyTransition_304334.hasExitTime = false;
            lAnyTransition_304334.hasFixedDuration = true;
            lAnyTransition_304334.exitTime = 0.75f;
            lAnyTransition_304334.duration = 0.1f;
            lAnyTransition_304334.offset = 0.115787f;
            lAnyTransition_304334.mute = false;
            lAnyTransition_304334.solo = false;
            lAnyTransition_304334.canTransitionToSelf = true;
            lAnyTransition_304334.interruptionSource = (TransitionInterruptionSource)0;
            for (int i = lAnyTransition_304334.conditions.Length - 1; i >= 0; i--) { lAnyTransition_304334.RemoveCondition(lAnyTransition_304334.conditions[i]); }
            lAnyTransition_304334.AddCondition(AnimatorConditionMode.Equals, 3375f, "L" + rLayerIndex + "MotionPhase");
            lAnyTransition_304334.AddCondition(AnimatorConditionMode.Equals, 0f, "L" + rLayerIndex + "MotionForm");
            lAnyTransition_304334.AddCondition(AnimatorConditionMode.Greater, 100f, "L" + rLayerIndex + "MotionParameter");

            AnimatorStateTransition lAnyTransition_310594 = MotionControllerMotion.EditorFindAnyStateTransition(lLayerStateMachine, lFromBehindState, 1);
            if (lAnyTransition_310594 == null) { lAnyTransition_310594 = lLayerStateMachine.AddAnyStateTransition(lFromBehindState); }
            lAnyTransition_310594.isExit = false;
            lAnyTransition_310594.hasExitTime = false;
            lAnyTransition_310594.hasFixedDuration = true;
            lAnyTransition_310594.exitTime = 0.75f;
            lAnyTransition_310594.duration = 0.1f;
            lAnyTransition_310594.offset = 0.115787f;
            lAnyTransition_310594.mute = false;
            lAnyTransition_310594.solo = false;
            lAnyTransition_310594.canTransitionToSelf = true;
            lAnyTransition_310594.interruptionSource = (TransitionInterruptionSource)0;
            for (int i = lAnyTransition_310594.conditions.Length - 1; i >= 0; i--) { lAnyTransition_310594.RemoveCondition(lAnyTransition_310594.conditions[i]); }
            lAnyTransition_310594.AddCondition(AnimatorConditionMode.Equals, 3375f, "L" + rLayerIndex + "MotionPhase");
            lAnyTransition_310594.AddCondition(AnimatorConditionMode.Equals, 0f, "L" + rLayerIndex + "MotionForm");
            lAnyTransition_310594.AddCondition(AnimatorConditionMode.Less, -100f, "L" + rLayerIndex + "MotionParameter");


            return lDeathSSM;
        }

        /// <summary>
        /// Verifies that the BasicDeath SSM has been created and has a default animation state
        /// </summary>
        /// <param name="rController"></param>
        /// <param name="rLayerIndex"></param>
        /// <param name="rCreateStates"></param>
        public static AnimatorStateMachine EnsureBasicInteractionSSM(AnimatorController rController, int rLayerIndex, bool rCreateStates = true)
        {            
            if (rController == null) return null;

            AnimatorStateMachine lLayerStateMachine = rController.layers[rLayerIndex].stateMachine;

            AnimatorStateMachine lInteractionSSM = MotionControllerMotion.EditorFindSSM(lLayerStateMachine, "BasicInteraction-SM");
            if (lInteractionSSM == null) { lInteractionSSM = lLayerStateMachine.AddStateMachine("BasicInteraction-SM", new Vector3(408, -960, 0)); }

            if (!rCreateStates) return lInteractionSSM;

            AnimatorState lGrabHighState = MotionControllerMotion.EditorFindState(lInteractionSSM, "Idle_GrabHighFront");
            if (lGrabHighState == null) { lGrabHighState = lInteractionSSM.AddState("Idle_GrabHighFront", new Vector3(337, 54, 0)); }
            lGrabHighState.speed = 1.5f;
            lGrabHighState.mirror = false;
            lGrabHighState.tag = "";
            lGrabHighState.motion = MotionControllerMotion.EditorFindAnimationClip(DefaultPaths.StandardAnimations + "Interacting/Unity_IdleGrab_FrontHigh.fbx", "Idle_GrabHighFront");

            AnimatorState lPickupState = MotionControllerMotion.EditorFindState(lInteractionSSM, "Idle_PickUp");
            if (lPickupState == null) { lPickupState = lInteractionSSM.AddState("Idle_PickUp", new Vector3(336, 168, 0)); }
            lPickupState.speed = 1.5f;
            lPickupState.mirror = false;
            lPickupState.tag = "";
            lPickupState.motion = MotionControllerMotion.EditorFindAnimationClip(DefaultPaths.StandardAnimations + "Interacting/unity_IdleGrab_LowFront.fbx", "Idle_PickUp");

            AnimatorState lPushButtonState = MotionControllerMotion.EditorFindState(lInteractionSSM, "Idle_PushButton");
            if (lPushButtonState == null) { lPushButtonState = lInteractionSSM.AddState("Idle_PushButton", new Vector3(336, -48, 0)); }
            lPushButtonState.speed = 1.5f;
            lPushButtonState.mirror = false;
            lPushButtonState.tag = "";
            lPushButtonState.motion = MotionControllerMotion.EditorFindAnimationClip(DefaultPaths.StandardAnimations + "Interacting/unity_IdleGrab_Neutral.fbx", "Idle_PushButton");

            AnimatorState lExitState = MotionControllerMotion.EditorFindState(lInteractionSSM, "IdlePose");
            if (lExitState == null) { lExitState = lInteractionSSM.AddState("IdlePose", new Vector3(600, 48, 0)); }
            lExitState.speed = 1f;
            lExitState.mirror = false;
            lExitState.tag = "Exit";
            lExitState.motion = MotionControllerMotion.EditorFindAnimationClip(DefaultPaths.StandardAnimations + "Idling/unity_Idle_IdleToIdlesR.fbx", "IdlePose");

            AnimatorStateTransition lAnyTransition_307944 = MotionControllerMotion.EditorFindAnyStateTransition(lLayerStateMachine, lPushButtonState, 0);
            if (lAnyTransition_307944 == null) { lAnyTransition_307944 = lLayerStateMachine.AddAnyStateTransition(lPushButtonState); }
            lAnyTransition_307944.isExit = false;
            lAnyTransition_307944.hasExitTime = false;
            lAnyTransition_307944.hasFixedDuration = true;
            lAnyTransition_307944.exitTime = 0.75f;
            lAnyTransition_307944.duration = 0.25f;
            lAnyTransition_307944.offset = 0.1517324f;
            lAnyTransition_307944.mute = false;
            lAnyTransition_307944.solo = false;
            lAnyTransition_307944.canTransitionToSelf = true;
            lAnyTransition_307944.interruptionSource = (TransitionInterruptionSource)0;
            for (int i = lAnyTransition_307944.conditions.Length - 1; i >= 0; i--) { lAnyTransition_307944.RemoveCondition(lAnyTransition_307944.conditions[i]); }
            lAnyTransition_307944.AddCondition(AnimatorConditionMode.Equals, 3450f, "L" + rLayerIndex + "MotionPhase");
            lAnyTransition_307944.AddCondition(AnimatorConditionMode.Equals, 0f, "L" + rLayerIndex + "MotionForm");

            AnimatorStateTransition lAnyTransition_301430 = MotionControllerMotion.EditorFindAnyStateTransition(lLayerStateMachine, lGrabHighState, 0);
            if (lAnyTransition_301430 == null) { lAnyTransition_301430 = lLayerStateMachine.AddAnyStateTransition(lGrabHighState); }
            lAnyTransition_301430.isExit = false;
            lAnyTransition_301430.hasExitTime = false;
            lAnyTransition_301430.hasFixedDuration = true;
            lAnyTransition_301430.exitTime = 0.75f;
            lAnyTransition_301430.duration = 0.25f;
            lAnyTransition_301430.offset = 0.07021895f;
            lAnyTransition_301430.mute = false;
            lAnyTransition_301430.solo = false;
            lAnyTransition_301430.canTransitionToSelf = true;
            lAnyTransition_301430.interruptionSource = (TransitionInterruptionSource)0;
            for (int i = lAnyTransition_301430.conditions.Length - 1; i >= 0; i--) { lAnyTransition_301430.RemoveCondition(lAnyTransition_301430.conditions[i]); }
            lAnyTransition_301430.AddCondition(AnimatorConditionMode.Equals, 3450f, "L" + rLayerIndex + "MotionPhase");
            lAnyTransition_301430.AddCondition(AnimatorConditionMode.Equals, 1f, "L" + rLayerIndex + "MotionForm");

            AnimatorStateTransition lAnyTransition_302096 = MotionControllerMotion.EditorFindAnyStateTransition(lLayerStateMachine, lPickupState, 0);
            if (lAnyTransition_302096 == null) { lAnyTransition_302096 = lLayerStateMachine.AddAnyStateTransition(lPickupState); }
            lAnyTransition_302096.isExit = false;
            lAnyTransition_302096.hasExitTime = false;
            lAnyTransition_302096.hasFixedDuration = true;
            lAnyTransition_302096.exitTime = 0.75f;
            lAnyTransition_302096.duration = 0.25f;
            lAnyTransition_302096.offset = 0f;
            lAnyTransition_302096.mute = false;
            lAnyTransition_302096.solo = false;
            lAnyTransition_302096.canTransitionToSelf = true;
            lAnyTransition_302096.interruptionSource = (TransitionInterruptionSource)0;
            for (int i = lAnyTransition_302096.conditions.Length - 1; i >= 0; i--) { lAnyTransition_302096.RemoveCondition(lAnyTransition_302096.conditions[i]); }
            lAnyTransition_302096.AddCondition(AnimatorConditionMode.Equals, 3450f, "L" + rLayerIndex + "MotionPhase");
            lAnyTransition_302096.AddCondition(AnimatorConditionMode.Equals, 2f, "L" + rLayerIndex + "MotionForm");

            AnimatorStateTransition lTransition_305594 = MotionControllerMotion.EditorFindTransition(lGrabHighState, lExitState, 0);
            if (lTransition_305594 == null) { lTransition_305594 = lGrabHighState.AddTransition(lExitState); }
            lTransition_305594.isExit = false;
            lTransition_305594.hasExitTime = true;
            lTransition_305594.hasFixedDuration = true;
            lTransition_305594.exitTime = 0.9285715f;
            lTransition_305594.duration = 0.25f;
            lTransition_305594.offset = 0f;
            lTransition_305594.mute = false;
            lTransition_305594.solo = false;
            lTransition_305594.canTransitionToSelf = true;
            lTransition_305594.interruptionSource = (TransitionInterruptionSource)0;
            for (int i = lTransition_305594.conditions.Length - 1; i >= 0; i--) { lTransition_305594.RemoveCondition(lTransition_305594.conditions[i]); }

            AnimatorStateTransition lTransition_299972 = MotionControllerMotion.EditorFindTransition(lPickupState, lExitState, 0);
            if (lTransition_299972 == null) { lTransition_299972 = lPickupState.AddTransition(lExitState); }
            lTransition_299972.isExit = false;
            lTransition_299972.hasExitTime = true;
            lTransition_299972.hasFixedDuration = true;
            lTransition_299972.exitTime = 0.90625f;
            lTransition_299972.duration = 0.25f;
            lTransition_299972.offset = 0f;
            lTransition_299972.mute = false;
            lTransition_299972.solo = false;
            lTransition_299972.canTransitionToSelf = true;
            lTransition_299972.interruptionSource = (TransitionInterruptionSource)0;
            for (int i = lTransition_299972.conditions.Length - 1; i >= 0; i--) { lTransition_299972.RemoveCondition(lTransition_299972.conditions[i]); }

            AnimatorStateTransition lTransition_311296 = MotionControllerMotion.EditorFindTransition(lPushButtonState, lExitState, 0);
            if (lTransition_311296 == null) { lTransition_311296 = lPushButtonState.AddTransition(lExitState); }
            lTransition_311296.isExit = false;
            lTransition_311296.hasExitTime = true;
            lTransition_311296.hasFixedDuration = true;
            lTransition_311296.exitTime = 0.7673402f;
            lTransition_311296.duration = 0.2499998f;
            lTransition_311296.offset = 0f;
            lTransition_311296.mute = false;
            lTransition_311296.solo = false;
            lTransition_311296.canTransitionToSelf = true;
            lTransition_311296.interruptionSource = (TransitionInterruptionSource)0;
            for (int i = lTransition_311296.conditions.Length - 1; i >= 0; i--) { lTransition_311296.RemoveCondition(lTransition_311296.conditions[i]); }


            return lInteractionSSM;

        }

        /// <summary>
        /// Verifies that the BasicEquipStore SSM has been created; it won't have any animation states 
        /// without a weapon motion pack.
        /// </summary>
        /// <param name="rController"></param>
        /// <param name="rLayerIndex"></param>
        /// <param name="rCreateStates"></param>
        /// <returns></returns>
        public static AnimatorStateMachine EnsureBasicEquipStoreSSM(AnimatorController rController, int rLayerIndex, bool rCreateStates = true)
        {            
            if (rController == null) return null;

            AnimatorStateMachine lLayerStateMachine = rController.layers[rLayerIndex].stateMachine;

            AnimatorStateMachine lEquipStoreSSM = MotionControllerMotion.EditorFindSSM(lLayerStateMachine, "BasicEquipStore-SM");
            if (lEquipStoreSSM == null) { lEquipStoreSSM = lLayerStateMachine.AddStateMachine("BasicEquipStore-SM", new Vector3(192, -1008, 0)); }

            if (!rCreateStates) { return lEquipStoreSSM; }

            AnimatorState lExitState = MotionControllerMotion.EditorFindState(lEquipStoreSSM, "Empty");
            if (lExitState == null) { lExitState = lEquipStoreSSM.AddState("Empty", new Vector3(24, 192, 0)); }
            lExitState.speed = 1f;
            lExitState.mirror = false;
            lExitState.tag = "Exit";

            return lEquipStoreSSM;
        }

        /// <summary>
        /// Verifies that the BasicMeleeAttack SSM has been created; it won't have any animation states 
        /// without a weapon motion pack.
        /// </summary>
        /// <param name="rController"></param>
        /// <param name="rLayerIndex"></param>
        /// <param name="rCreateStates"></param>
        public static AnimatorStateMachine EnsureBasicMeleeAttackSSM(AnimatorController rController, int rLayerIndex, bool rCreateStates = true)
        {            
            if (rController == null) return null;

            AnimatorStateMachine lLayerStateMachine = rController.layers[rLayerIndex].stateMachine;

            AnimatorStateMachine lAttackSSM = MotionControllerMotion.EditorFindSSM(lLayerStateMachine, "BasicMeleeAttack-SM");
            if (lAttackSSM == null) { lAttackSSM = lLayerStateMachine.AddStateMachine("BasicMeleeAttack-SM", new Vector3(624, -1056, 0)); }

            return lAttackSSM;
        }       

        /// <summary>
        /// Verifies that the HandPose SSM on the either the Left Hand Right Hand layer has been created and has a default animation state
        /// </summary>        
        /// <param name="rController"></param>
        /// <param name="rLayerIndex"></param>
        /// <param name="rCreateStates"></param>
        public static AnimatorStateMachine EnsureBasicHandPoseSSM(AnimatorController rController, int rLayerIndex, bool rCreateStates = true)
        {
            if (rController == null) { return null ;}
            AnimatorStateMachine lLayerStateMachine = rController.layers[rLayerIndex].stateMachine;

            AnimatorStateMachine lHandPoseSSM = MotionControllerMotion.EditorFindSSM(lLayerStateMachine, "BasicHandPose-SM");
            if (lHandPoseSSM == null) { lHandPoseSSM = lLayerStateMachine.AddStateMachine("BasicHandPose-SM", new Vector3(324, 0, 0)); }

            if (!rCreateStates) { return lHandPoseSSM; }

            AnimatorState lExitState = MotionControllerMotion.EditorFindState(lHandPoseSSM, "Empty");
            if (lExitState == null) { lExitState = lHandPoseSSM.AddState("Empty", new Vector3(648, -144, 0)); }
            lExitState.speed = 1f;
            lExitState.mirror = false;
            lExitState.tag = "Exit";

            AnimatorState lDefaultState = MotionControllerMotion.EditorFindState(lHandPoseSSM, "Default HandPose");
            if (lDefaultState == null) { lDefaultState = lHandPoseSSM.AddState("Default HandPose", new Vector3(348, -144, 0)); }
            lDefaultState.speed = 1f;
            lDefaultState.mirror = false;
            lDefaultState.tag = "";

            AnimatorStateTransition lDefaultTransition = MotionControllerMotion.EditorFindAnyStateTransition(lLayerStateMachine, lDefaultState, 0);
            if (lDefaultTransition == null) { lDefaultTransition = lLayerStateMachine.AddAnyStateTransition(lDefaultState); }
            lDefaultTransition.isExit = false;
            lDefaultTransition.hasExitTime = false;
            lDefaultTransition.hasFixedDuration = true;
            lDefaultTransition.exitTime = 0.75f;
            lDefaultTransition.duration = 0.25f;
            lDefaultTransition.offset = 0f;
            lDefaultTransition.mute = false;
            lDefaultTransition.solo = false;
            lDefaultTransition.canTransitionToSelf = true;
            lDefaultTransition.orderedInterruption = true;
            lDefaultTransition.interruptionSource = (TransitionInterruptionSource)0;
            for (int i = lDefaultTransition.conditions.Length - 1; i >= 0; i--) { lDefaultTransition.RemoveCondition(lDefaultTransition.conditions[i]); }
            lDefaultTransition.AddCondition(AnimatorConditionMode.Equals, 3660f, "L" + rLayerIndex + "MotionPhase");
            lDefaultTransition.AddCondition(AnimatorConditionMode.Equals, 0f, "L" + rLayerIndex + "MotionForm");

            AnimatorStateTransition lExitTransition = MotionControllerMotion.EditorFindTransition(lDefaultState, lExitState, 0);
            if (lExitTransition == null) { lExitTransition = lDefaultState.AddTransition(lExitState); }
            lExitTransition.isExit = false;
            lExitTransition.hasExitTime = false;
            lExitTransition.hasFixedDuration = true;
            lExitTransition.exitTime = 0.75f;
            lExitTransition.duration = 0.25f;
            lExitTransition.offset = 0f;
            lExitTransition.mute = false;
            lExitTransition.solo = false;
            lExitTransition.canTransitionToSelf = true;
            lExitTransition.orderedInterruption = true;
            lExitTransition.interruptionSource = (TransitionInterruptionSource)0;
            for (int i = lExitTransition.conditions.Length - 1; i >= 0; i--) { lExitTransition.RemoveCondition(lExitTransition.conditions[i]); }
            lExitTransition.AddCondition(AnimatorConditionMode.Equals, 3661f, "L" + rLayerIndex + "MotionPhase");

            return lHandPoseSSM;
        }
               
        /// <summary>
        /// Verifies that the BasicDodge SSM has been created; it won't have any animation states 
        /// without a weapon motion pack.
        /// </summary>
        /// <param name="rController"></param>
        /// <param name="rLayerIndex"></param>
        /// <param name="rCreateStates"></param>
        /// <returns></returns>
        public static AnimatorStateMachine EnsureBasicDodgeSSM(AnimatorController rController, int rLayerIndex, bool rCreateStates = true)
        {
            if (rController == null) { return null; }
            AnimatorStateMachine lLayerStateMachine = rController.layers[rLayerIndex].stateMachine;

            AnimatorStateMachine lDodgeSSM = MotionControllerMotion.EditorFindSSM(lLayerStateMachine, "BasicDodge-SM");
            if (lDodgeSSM == null) { lDodgeSSM = lLayerStateMachine.AddStateMachine("BasicDodge-SM", new Vector3(624, -912, 0)); }

            AnimatorState lExitState = MotionControllerMotion.EditorFindState(lDodgeSSM, "Empty");
            if (lExitState == null) { lExitState = lDodgeSSM.AddState("Empty", new Vector3(360, -96, 0)); }
            lExitState.speed = 1f;
            lExitState.mirror = false;
            lExitState.tag = "Exit";

            return lDodgeSSM;
        }

        #endregion Verify Basic Animator States


        #region Verify Standard Motion Animator States

        /// <summary>
        /// Verifies that the standard Climb 0.5m SSM has been created
        /// </summary>
        /// <param name="rMotionController"></param>
        /// <param name="rLayerIndex"></param>
        /// <param name="rCreateStates"></param>
        /// <returns></returns>
        public static AnimatorStateMachine EnsureStandardClimb_0_5m(MotionController rMotionController, int rLayerIndex, bool rCreateStates = true)
        {
            AnimatorController lController = GetAnimatorController(rMotionController);
            if (lController == null) return null;

            AnimatorControllerLayer lLayer = lController.layers[rLayerIndex];
            AnimatorStateMachine lLayerStateMachine = lLayer.stateMachine;

            AnimatorStateMachine lClimbSSM = MotionControllerMotion.EditorFindSSM(lLayerStateMachine, "Climb_0_5m-SM");
            if (lClimbSSM == null) { lClimbSSM = lLayerStateMachine.AddStateMachine("Climb_0_5m-SM", new Vector3(336, -48, 0)); }

            if (!rCreateStates) return lClimbSSM;

            AnimatorState lState_N583146 = MotionControllerMotion.EditorFindState(lClimbSSM, "IdleClimbMid");
            if (lState_N583146 == null) { lState_N583146 = lClimbSSM.AddState("IdleClimbMid", new Vector3(264, 12, 0)); }
            lState_N583146.speed = 1f;
            lState_N583146.mirror = false;
            lState_N583146.tag = "";
            lState_N583146.motion = MotionControllerMotion.EditorFindAnimationClip(DefaultPaths.StandardAnimations + "Climbing/ootii_ClimbMid.fbx", "IdleClimbMid");

            AnimatorState lState_N583148 = MotionControllerMotion.EditorFindState(lClimbSSM, "ClimbRecoverIdle");
            if (lState_N583148 == null) { lState_N583148 = lClimbSSM.AddState("ClimbRecoverIdle", new Vector3(504, -36, 0)); }
            lState_N583148.speed = 1f;
            lState_N583148.mirror = false;
            lState_N583148.tag = "";
            lState_N583148.motion = MotionControllerMotion.EditorFindAnimationClip(DefaultPaths.StandardAnimations + "Climbing/ootii_ClimbMid.fbx", "ClimbRecoverIdle");

            AnimatorState lState_N583150 = MotionControllerMotion.EditorFindState(lClimbSSM, "IdlePose");
            if (lState_N583150 == null) { lState_N583150 = lClimbSSM.AddState("IdlePose", new Vector3(504, 36, 0)); }
            lState_N583150.speed = 1f;
            lState_N583150.mirror = false;
            lState_N583150.tag = "";
            lState_N583150.motion = MotionControllerMotion.EditorFindAnimationClip(DefaultPaths.StandardAnimations + "Idling/unity_Idle_IdleToIdlesR.fbx", "IdlePose");

            AnimatorStateTransition lAnyTransition_N583152 = MotionControllerMotion.EditorFindAnyStateTransition(lLayerStateMachine, lState_N583146, 0);
            if (lAnyTransition_N583152 == null) { lAnyTransition_N583152 = lLayerStateMachine.AddAnyStateTransition(lState_N583146); }
            lAnyTransition_N583152.isExit = false;
            lAnyTransition_N583152.hasExitTime = false;
            lAnyTransition_N583152.hasFixedDuration = false;
            lAnyTransition_N583152.exitTime = 0.9f;
            lAnyTransition_N583152.duration = 0.1f;
            lAnyTransition_N583152.offset = 0f;
            lAnyTransition_N583152.mute = false;
            lAnyTransition_N583152.solo = false;
            lAnyTransition_N583152.canTransitionToSelf = true;
            lAnyTransition_N583152.orderedInterruption = true;
            lAnyTransition_N583152.interruptionSource = (TransitionInterruptionSource)0;
            for (int i = lAnyTransition_N583152.conditions.Length - 1; i >= 0; i--) { lAnyTransition_N583152.RemoveCondition(lAnyTransition_N583152.conditions[i]); }
            lAnyTransition_N583152.AddCondition(AnimatorConditionMode.Equals, 900f, "L" + rLayerIndex + "MotionPhase");

            AnimatorStateTransition lTransition_N583154 = MotionControllerMotion.EditorFindTransition(lState_N583146, lState_N583148, 0);
            if (lTransition_N583154 == null) { lTransition_N583154 = lState_N583146.AddTransition(lState_N583148); }
            lTransition_N583154.isExit = false;
            lTransition_N583154.hasExitTime = true;
            lTransition_N583154.hasFixedDuration = false;
            lTransition_N583154.exitTime = 1.00067f;
            lTransition_N583154.duration = 0f;
            lTransition_N583154.offset = 0f;
            lTransition_N583154.mute = true;
            lTransition_N583154.solo = false;
            lTransition_N583154.canTransitionToSelf = true;
            lTransition_N583154.orderedInterruption = true;
            lTransition_N583154.interruptionSource = (TransitionInterruptionSource)0;
            for (int i = lTransition_N583154.conditions.Length - 1; i >= 0; i--) { lTransition_N583154.RemoveCondition(lTransition_N583154.conditions[i]); }

            AnimatorStateTransition lTransition_N583156 = MotionControllerMotion.EditorFindTransition(lState_N583146, lState_N583150, 0);
            if (lTransition_N583156 == null) { lTransition_N583156 = lState_N583146.AddTransition(lState_N583150); }
            lTransition_N583156.isExit = false;
            lTransition_N583156.hasExitTime = true;
            lTransition_N583156.hasFixedDuration = true;
            lTransition_N583156.exitTime = 0.8125f;
            lTransition_N583156.duration = 0.25f;
            lTransition_N583156.offset = 0f;
            lTransition_N583156.mute = false;
            lTransition_N583156.solo = false;
            lTransition_N583156.canTransitionToSelf = true;
            lTransition_N583156.orderedInterruption = true;
            lTransition_N583156.interruptionSource = (TransitionInterruptionSource)0;
            for (int i = lTransition_N583156.conditions.Length - 1; i >= 0; i--) { lTransition_N583156.RemoveCondition(lTransition_N583156.conditions[i]); }

            return lClimbSSM;
        }

        /// <summary>
        /// Verifies that the standard Climb 1m SSM has been created
        /// </summary>
        /// <param name="rMotionController"></param>
        /// <param name="rLayerIndex"></param>
        /// <param name="rCreateStates"></param>
        /// <returns></returns>
        public static AnimatorStateMachine EnsureStandardClimb_1m(MotionController rMotionController, int rLayerIndex,
            bool rCreateStates = true)
        {
            AnimatorController lController = GetAnimatorController(rMotionController);
            if (lController == null) return null;
            
            AnimatorControllerLayer lLayer = lController.layers[rLayerIndex];
            AnimatorStateMachine lLayerStateMachine = lLayer.stateMachine;

            AnimatorStateMachine lClimbSSM = MotionControllerMotion.EditorFindSSM(lLayerStateMachine, "Climb_1m-SM");
            if (lClimbSSM == null) { lClimbSSM = lLayerStateMachine.AddStateMachine("Climb_1m-SM", new Vector3(552, -48, 0)); }

            if (!rCreateStates)  return lClimbSSM; 

            AnimatorState lState_N590976 = MotionControllerMotion.EditorFindState(lClimbSSM, "IdlePose");
            if (lState_N590976 == null) { lState_N590976 = lClimbSSM.AddState("IdlePose", new Vector3(564, 36, 0)); }
            lState_N590976.speed = 1f;
            lState_N590976.mirror = false;
            lState_N590976.tag = "";
            lState_N590976.motion = MotionControllerMotion.EditorFindAnimationClip(DefaultPaths.StandardAnimations + "Idling/unity_Idle_IdleToIdlesR.fbx", "IdlePose");

            AnimatorState lState_N590978 = MotionControllerMotion.EditorFindState(lClimbSSM, "Climb_1m");
            if (lState_N590978 == null) { lState_N590978 = lClimbSSM.AddState("Climb_1m", new Vector3(312, 36, 0)); }
            lState_N590978.speed = 1f;
            lState_N590978.mirror = false;
            lState_N590978.tag = "";
            lState_N590978.motion = MotionControllerMotion.EditorFindAnimationClip(DefaultPaths.StandardAnimations + "Climbing/unity_Idle_JumpUpMedium_2Hands_Idle.fbx", "Climb_1m");

            AnimatorState lState_N590980 = MotionControllerMotion.EditorFindState(lClimbSSM, "IdleStart");
            if (lState_N590980 == null) { lState_N590980 = lClimbSSM.AddState("IdleStart", new Vector3(48, 36, 0)); }
            lState_N590980.speed = 1f;
            lState_N590980.mirror = false;
            lState_N590980.tag = "";
            lState_N590980.motion = MotionControllerMotion.EditorFindAnimationClip(DefaultPaths.StandardAnimations + "Idling/unity_Idle_IdleToIdlesR.fbx", "IdleStart");

            AnimatorStateTransition lAnyTransition_N590982 = MotionControllerMotion.EditorFindAnyStateTransition(lLayerStateMachine, lState_N590980, 0);
            if (lAnyTransition_N590982 == null) { lAnyTransition_N590982 = lLayerStateMachine.AddAnyStateTransition(lState_N590980); }
            lAnyTransition_N590982.isExit = false;
            lAnyTransition_N590982.hasExitTime = false;
            lAnyTransition_N590982.hasFixedDuration = true;
            lAnyTransition_N590982.exitTime = 0.9f;
            lAnyTransition_N590982.duration = 0.1f;
            lAnyTransition_N590982.offset = 0f;
            lAnyTransition_N590982.mute = false;
            lAnyTransition_N590982.solo = false;
            lAnyTransition_N590982.canTransitionToSelf = true;
            lAnyTransition_N590982.orderedInterruption = true;
            lAnyTransition_N590982.interruptionSource = (TransitionInterruptionSource)0;
            for (int i = lAnyTransition_N590982.conditions.Length - 1; i >= 0; i--) { lAnyTransition_N590982.RemoveCondition(lAnyTransition_N590982.conditions[i]); }
            lAnyTransition_N590982.AddCondition(AnimatorConditionMode.Equals, 950f, "L" + rLayerIndex + "MotionPhase");

            AnimatorStateTransition lTransition_N590984 = MotionControllerMotion.EditorFindTransition(lState_N590978, lState_N590976, 0);
            if (lTransition_N590984 == null) { lTransition_N590984 = lState_N590978.AddTransition(lState_N590976); }
            lTransition_N590984.isExit = false;
            lTransition_N590984.hasExitTime = true;
            lTransition_N590984.hasFixedDuration = true;
            lTransition_N590984.exitTime = 0.4088161f;
            lTransition_N590984.duration = 0.2500001f;
            lTransition_N590984.offset = 0f;
            lTransition_N590984.mute = false;
            lTransition_N590984.solo = false;
            lTransition_N590984.canTransitionToSelf = true;
            lTransition_N590984.orderedInterruption = true;
            lTransition_N590984.interruptionSource = (TransitionInterruptionSource)0;
            for (int i = lTransition_N590984.conditions.Length - 1; i >= 0; i--) { lTransition_N590984.RemoveCondition(lTransition_N590984.conditions[i]); }

            AnimatorStateTransition lTransition_N590986 = MotionControllerMotion.EditorFindTransition(lState_N590980, lState_N590978, 0);
            if (lTransition_N590986 == null) { lTransition_N590986 = lState_N590980.AddTransition(lState_N590978); }
            lTransition_N590986.isExit = false;
            lTransition_N590986.hasExitTime = true;
            lTransition_N590986.hasFixedDuration = true;
            lTransition_N590986.exitTime = 2.39536f;
            lTransition_N590986.duration = 0.1034331f;
            lTransition_N590986.offset = 0f;
            lTransition_N590986.mute = false;
            lTransition_N590986.solo = false;
            lTransition_N590986.canTransitionToSelf = true;
            lTransition_N590986.orderedInterruption = true;
            lTransition_N590986.interruptionSource = (TransitionInterruptionSource)0;
            for (int i = lTransition_N590986.conditions.Length - 1; i >= 0; i--) { lTransition_N590986.RemoveCondition(lTransition_N590986.conditions[i]); }

            return lClimbSSM;
        }

        /// <summary>
        /// Verifies that the standard Climb 1.8m SSM has been created
        /// </summary>
        /// <param name="rMotionController"></param>
        /// <param name="rLayerIndex"></param>
        /// <param name="rCreateStates"></param>
        /// <returns></returns>
        public static AnimatorStateMachine EnsureStandardClimb_1_8m(MotionController rMotionController, int rLayerIndex,
            bool rCreateStates = true)
        {
            AnimatorController lController = GetAnimatorController(rMotionController);
            if (lController == null) return null;

            AnimatorControllerLayer lLayer = lController.layers[rLayerIndex];
            AnimatorStateMachine lLayerStateMachine = lLayer.stateMachine;

            AnimatorStateMachine lClimbSSM = MotionControllerMotion.EditorFindSSM(lLayerStateMachine, "Climb_1_8m-SM");
            if (lClimbSSM == null) { lClimbSSM = lLayerStateMachine.AddStateMachine("Climb_1_8m-SM", new Vector3(768, -48, 0)); }

            if (!rCreateStates)  return lClimbSSM; 

            AnimatorState lState_N650716 = MotionControllerMotion.EditorFindState(lClimbSSM, "IdlePose");
            if (lState_N650716 == null) { lState_N650716 = lClimbSSM.AddState("IdlePose", new Vector3(576, 12, 0)); }
            lState_N650716.speed = 1f;
            lState_N650716.mirror = false;
            lState_N650716.tag = "";
            lState_N650716.motion = MotionControllerMotion.EditorFindAnimationClip(DefaultPaths.StandardAnimations + "Idling/unity_Idle_IdleToIdlesR.fbx", "IdlePose");

            AnimatorState lState_N650718 = MotionControllerMotion.EditorFindState(lClimbSSM, "LegUpToIdle");
            if (lState_N650718 == null) { lState_N650718 = lClimbSSM.AddState("LegUpToIdle", new Vector3(288, 12, 0)); }
            lState_N650718.speed = 1f;
            lState_N650718.mirror = false;
            lState_N650718.tag = "";
            lState_N650718.motion = MotionControllerMotion.EditorFindAnimationClip(DefaultPaths.StandardAnimations + "Climbing/unity_Idle_JumpUpMedium_2Hands_Idle.fbx", "LegUpToIdle");

            AnimatorStateTransition lAnyTransition_N650720 = MotionControllerMotion.EditorFindAnyStateTransition(lLayerStateMachine, lState_N650718, 0);
            if (lAnyTransition_N650720 == null) { lAnyTransition_N650720 = lLayerStateMachine.AddAnyStateTransition(lState_N650718); }
            lAnyTransition_N650720.isExit = false;
            lAnyTransition_N650720.hasExitTime = false;
            lAnyTransition_N650720.hasFixedDuration = true;
            lAnyTransition_N650720.exitTime = 0.9f;
            lAnyTransition_N650720.duration = 0.1f;
            lAnyTransition_N650720.offset = 0f;
            lAnyTransition_N650720.mute = false;
            lAnyTransition_N650720.solo = false;
            lAnyTransition_N650720.canTransitionToSelf = true;
            lAnyTransition_N650720.orderedInterruption = true;
            lAnyTransition_N650720.interruptionSource = (TransitionInterruptionSource)0;
            for (int i = lAnyTransition_N650720.conditions.Length - 1; i >= 0; i--) { lAnyTransition_N650720.RemoveCondition(lAnyTransition_N650720.conditions[i]); }
            lAnyTransition_N650720.AddCondition(AnimatorConditionMode.Equals, 1250f, "L" + rLayerIndex + "MotionPhase");

            AnimatorStateTransition lTransition_N650722 = MotionControllerMotion.EditorFindTransition(lState_N650718, lState_N650716, 0);
            if (lTransition_N650722 == null) { lTransition_N650722 = lState_N650718.AddTransition(lState_N650716); }
            lTransition_N650722.isExit = false;
            lTransition_N650722.hasExitTime = true;
            lTransition_N650722.hasFixedDuration = true;
            lTransition_N650722.exitTime = 0.8626919f;
            lTransition_N650722.duration = 0.25f;
            lTransition_N650722.offset = 0f;
            lTransition_N650722.mute = false;
            lTransition_N650722.solo = false;
            lTransition_N650722.canTransitionToSelf = true;
            lTransition_N650722.orderedInterruption = true;
            lTransition_N650722.interruptionSource = (TransitionInterruptionSource)0;
            for (int i = lTransition_N650722.conditions.Length - 1; i >= 0; i--) { lTransition_N650722.RemoveCondition(lTransition_N650722.conditions[i]); }

            return lClimbSSM;
        }

        /// <summary>
        /// Verifies that the standard Climb 2.5m SSM has been created
        /// </summary>
        /// <param name="rMotionController"></param>
        /// <param name="rLayerIndex"></param>
        /// <param name="rCreateStates"></param>
        /// <returns></returns>
        public static AnimatorStateMachine EnsureStandardClimb_2_5m(MotionController rMotionController, int rLayerIndex,
            bool rCreateStates = true)
        {
            AnimatorController lController = GetAnimatorController(rMotionController);
            if (lController == null) return null;
            
            AnimatorControllerLayer lLayer = lController.layers[rLayerIndex];
            AnimatorStateMachine lLayerStateMachine = lLayer.stateMachine;

            AnimatorStateMachine lClimbSSM = MotionControllerMotion.EditorFindSSM(lLayerStateMachine, "Climb_2_5m-SM");
            if (lClimbSSM == null) { lClimbSSM = lLayerStateMachine.AddStateMachine("Climb_2_5m-SM", new Vector3(984, -48, 0)); }

            if (!rCreateStates) return lClimbSSM;

            AnimatorState lState_N655886 = MotionControllerMotion.EditorFindState(lClimbSSM, "IdlePose");
            if (lState_N655886 == null) { lState_N655886 = lClimbSSM.AddState("IdlePose", new Vector3(696, 12, 0)); }
            lState_N655886.speed = 1f;
            lState_N655886.mirror = false;
            lState_N655886.tag = "";
            lState_N655886.motion = MotionControllerMotion.EditorFindAnimationClip(DefaultPaths.StandardAnimations + "Idling/unity_Idle_IdleToIdlesR.fbx", "IdlePose");

            AnimatorState lState_N655888 = MotionControllerMotion.EditorFindState(lClimbSSM, "Climb_2_5m");
            if (lState_N655888 == null) { lState_N655888 = lClimbSSM.AddState("Climb_2_5m", new Vector3(288, 12, 0)); }
            lState_N655888.speed = 0.8f;
            lState_N655888.mirror = false;
            lState_N655888.tag = "";
            lState_N655888.motion = MotionControllerMotion.EditorFindAnimationClip(DefaultPaths.StandardAnimations + "Climbing/unity_Run_JumpUpHigh_Run.fbx", "Climb_2_5m");

            AnimatorState lState_N655890 = MotionControllerMotion.EditorFindState(lClimbSSM, "ClimbToIdle");
            if (lState_N655890 == null) { lState_N655890 = lClimbSSM.AddState("ClimbToIdle", new Vector3(546, 114, 0)); }
            lState_N655890.speed = 1f;
            lState_N655890.mirror = false;
            lState_N655890.tag = "";
            lState_N655890.motion = MotionControllerMotion.EditorFindAnimationClip(DefaultPaths.StandardAnimations + "Climbing/unity_Idle_JumpUpHigh_StepBack_Idle.fbx", "ClimbToIdle");

            AnimatorState lState_N655892 = MotionControllerMotion.EditorFindState(lClimbSSM, "StandClimb_2_5m");
            if (lState_N655892 == null) { lState_N655892 = lClimbSSM.AddState("StandClimb_2_5m", new Vector3(288, 96, 0)); }
            lState_N655892.speed = 1f;
            lState_N655892.mirror = false;
            lState_N655892.tag = "";
            lState_N655892.motion = MotionControllerMotion.EditorFindAnimationClip(DefaultPaths.StandardAnimations + "Climbing/unity_Idle_JumpUpHigh_StepBack_Idle.fbx", "StandClimb_2_5m");

            AnimatorStateTransition lAnyTransition_N655894 = MotionControllerMotion.EditorFindAnyStateTransition(lLayerStateMachine, lState_N655888, 0);
            if (lAnyTransition_N655894 == null) { lAnyTransition_N655894 = lLayerStateMachine.AddAnyStateTransition(lState_N655888); }
            lAnyTransition_N655894.isExit = false;
            lAnyTransition_N655894.hasExitTime = false;
            lAnyTransition_N655894.hasFixedDuration = true;
            lAnyTransition_N655894.exitTime = 0.9f;
            lAnyTransition_N655894.duration = 0.1f;
            lAnyTransition_N655894.offset = 0f;
            lAnyTransition_N655894.mute = false;
            lAnyTransition_N655894.solo = false;
            lAnyTransition_N655894.canTransitionToSelf = true;
            lAnyTransition_N655894.orderedInterruption = true;
            lAnyTransition_N655894.interruptionSource = (TransitionInterruptionSource)0;
            for (int i = lAnyTransition_N655894.conditions.Length - 1; i >= 0; i--) { lAnyTransition_N655894.RemoveCondition(lAnyTransition_N655894.conditions[i]); }
            lAnyTransition_N655894.AddCondition(AnimatorConditionMode.Equals, 1200f, "L" + rLayerIndex + "MotionPhase");

            AnimatorStateTransition lAnyTransition_N655896 = MotionControllerMotion.EditorFindAnyStateTransition(lLayerStateMachine, lState_N655892, 0);
            if (lAnyTransition_N655896 == null) { lAnyTransition_N655896 = lLayerStateMachine.AddAnyStateTransition(lState_N655892); }
            lAnyTransition_N655896.isExit = false;
            lAnyTransition_N655896.hasExitTime = false;
            lAnyTransition_N655896.hasFixedDuration = true;
            lAnyTransition_N655896.exitTime = 0.9f;
            lAnyTransition_N655896.duration = 0.1f;
            lAnyTransition_N655896.offset = 0f;
            lAnyTransition_N655896.mute = false;
            lAnyTransition_N655896.solo = false;
            lAnyTransition_N655896.canTransitionToSelf = true;
            lAnyTransition_N655896.orderedInterruption = true;
            lAnyTransition_N655896.interruptionSource = (TransitionInterruptionSource)0;
            for (int i = lAnyTransition_N655896.conditions.Length - 1; i >= 0; i--) { lAnyTransition_N655896.RemoveCondition(lAnyTransition_N655896.conditions[i]); }
            lAnyTransition_N655896.AddCondition(AnimatorConditionMode.Equals, 1205f, "L" + rLayerIndex + "MotionPhase");

            AnimatorStateTransition lTransition_N655898 = MotionControllerMotion.EditorFindTransition(lState_N655888, lState_N655890, 0);
            if (lTransition_N655898 == null) { lTransition_N655898 = lState_N655888.AddTransition(lState_N655890); }
            lTransition_N655898.isExit = false;
            lTransition_N655898.hasExitTime = true;
            lTransition_N655898.hasFixedDuration = true;
            lTransition_N655898.exitTime = 0.6883073f;
            lTransition_N655898.duration = 0.2500001f;
            lTransition_N655898.offset = 0f;
            lTransition_N655898.mute = false;
            lTransition_N655898.solo = false;
            lTransition_N655898.canTransitionToSelf = true;
            lTransition_N655898.orderedInterruption = true;
            lTransition_N655898.interruptionSource = (TransitionInterruptionSource)0;
            for (int i = lTransition_N655898.conditions.Length - 1; i >= 0; i--) { lTransition_N655898.RemoveCondition(lTransition_N655898.conditions[i]); }

            AnimatorStateTransition lTransition_N655900 = MotionControllerMotion.EditorFindTransition(lState_N655890, lState_N655886, 0);
            if (lTransition_N655900 == null) { lTransition_N655900 = lState_N655890.AddTransition(lState_N655886); }
            lTransition_N655900.isExit = false;
            lTransition_N655900.hasExitTime = true;
            lTransition_N655900.hasFixedDuration = true;
            lTransition_N655900.exitTime = 0.4848107f;
            lTransition_N655900.duration = 0.08987129f;
            lTransition_N655900.offset = 0f;
            lTransition_N655900.mute = false;
            lTransition_N655900.solo = false;
            lTransition_N655900.canTransitionToSelf = true;
            lTransition_N655900.orderedInterruption = true;
            lTransition_N655900.interruptionSource = (TransitionInterruptionSource)0;
            for (int i = lTransition_N655900.conditions.Length - 1; i >= 0; i--) { lTransition_N655900.RemoveCondition(lTransition_N655900.conditions[i]); }

            AnimatorStateTransition lTransition_N655902 = MotionControllerMotion.EditorFindTransition(lState_N655892, lState_N655890, 0);
            if (lTransition_N655902 == null) { lTransition_N655902 = lState_N655892.AddTransition(lState_N655890); }
            lTransition_N655902.isExit = false;
            lTransition_N655902.hasExitTime = true;
            lTransition_N655902.hasFixedDuration = true;
            lTransition_N655902.exitTime = 0.9879599f;
            lTransition_N655902.duration = 0.04615402f;
            lTransition_N655902.offset = 0f;
            lTransition_N655902.mute = false;
            lTransition_N655902.solo = false;
            lTransition_N655902.canTransitionToSelf = true;
            lTransition_N655902.orderedInterruption = true;
            lTransition_N655902.interruptionSource = (TransitionInterruptionSource)0;
            for (int i = lTransition_N655902.conditions.Length - 1; i >= 0; i--) { lTransition_N655902.RemoveCondition(lTransition_N655902.conditions[i]); }

            return lClimbSSM;
        }

        /// <summary>
        /// Verifies that the standard Climb Ladder SSM has been created
        /// </summary>
        /// <param name="rMotionController"></param>
        /// <param name="rLayerIndex"></param>
        /// <param name="rCreateStates"></param>
        /// <returns></returns>
        public static AnimatorStateMachine EnsureStandardClimbLadder(MotionController rMotionController, int rLayerIndex,
            bool rCreateStates = true)
        {
            AnimatorController lController = GetAnimatorController(rMotionController);
            if (lController == null) return null;

            AnimatorControllerLayer lLayer = lController.layers[rLayerIndex];
            AnimatorStateMachine lLayerStateMachine = lLayer.stateMachine;

            AnimatorStateMachine lClimbLadderSSM = MotionControllerMotion.EditorFindSSM(lLayerStateMachine, "ClimbLadder-SM");
            if (lClimbLadderSSM == null) { lClimbLadderSSM = lLayerStateMachine.AddStateMachine("ClimbLadder-SM", new Vector3(336, -108, 0)); }

            if (!rCreateStates) return lClimbLadderSSM;

            AnimatorState lState_N658166 = MotionControllerMotion.EditorFindState(lClimbLadderSSM, "LadderBottomOn");
            if (lState_N658166 == null) { lState_N658166 = lClimbLadderSSM.AddState("LadderBottomOn", new Vector3(384, -24, 0)); }
            lState_N658166.speed = 1f;
            lState_N658166.mirror = false;
            lState_N658166.tag = "";
            lState_N658166.motion = MotionControllerMotion.EditorFindAnimationClip(DefaultPaths.StandardAnimations + "Climbing/ootii_Ladder.fbx", "LadderBottomOn");

            AnimatorState lState_N658168 = MotionControllerMotion.EditorFindState(lClimbLadderSSM, "LadderUp");
            if (lState_N658168 == null) { lState_N658168 = lClimbLadderSSM.AddState("LadderUp", new Vector3(756, -24, 0)); }
            lState_N658168.speed = 1f;
            lState_N658168.mirror = false;
            lState_N658168.tag = "";
            lState_N658168.motion = MotionControllerMotion.EditorFindAnimationClip(DefaultPaths.StandardAnimations + "Climbing/ootii_Ladder.fbx", "LadderUpLoop");

            AnimatorState lState_N658170 = MotionControllerMotion.EditorFindState(lClimbLadderSSM, "LadderDown");
            if (lState_N658170 == null) { lState_N658170 = lClimbLadderSSM.AddState("LadderDown", new Vector3(756, 168, 0)); }
            lState_N658170.speed = 1f;
            lState_N658170.mirror = false;
            lState_N658170.tag = "";
            lState_N658170.motion = MotionControllerMotion.EditorFindAnimationClip(DefaultPaths.StandardAnimations + "Climbing/ootii_Ladder.fbx", "LadderDownLoop");

            AnimatorState lState_N658172 = MotionControllerMotion.EditorFindState(lClimbLadderSSM, "LadderBottomOff");
            if (lState_N658172 == null) { lState_N658172 = lClimbLadderSSM.AddState("LadderBottomOff", new Vector3(948, 276, 0)); }
            lState_N658172.speed = 1f;
            lState_N658172.mirror = false;
            lState_N658172.tag = "";
            lState_N658172.motion = MotionControllerMotion.EditorFindAnimationClip(DefaultPaths.StandardAnimations + "Climbing/ootii_Ladder.fbx", "LadderBottomOff");

            AnimatorState lState_N658174 = MotionControllerMotion.EditorFindState(lClimbLadderSSM, "LadderPose");
            if (lState_N658174 == null) { lState_N658174 = lClimbLadderSSM.AddState("LadderPose", new Vector3(612, 84, 0)); }
            lState_N658174.speed = 1f;
            lState_N658174.mirror = false;
            lState_N658174.tag = "";
            lState_N658174.motion = MotionControllerMotion.EditorFindAnimationClip(DefaultPaths.StandardAnimations + "Climbing/ootii_Ladder.fbx", "LadderPose");

            AnimatorState lState_N658176 = MotionControllerMotion.EditorFindState(lClimbLadderSSM, "IdlePose");
            if (lState_N658176 == null) { lState_N658176 = lClimbLadderSSM.AddState("IdlePose", new Vector3(984, 84, 0)); }
            lState_N658176.speed = 1f;
            lState_N658176.mirror = false;
            lState_N658176.tag = "";
            lState_N658176.motion = MotionControllerMotion.EditorFindAnimationClip(DefaultPaths.StandardAnimations + "Idling/unity_Idle_IdleToIdlesR.fbx", "IdlePose");

            AnimatorState lState_N658178 = MotionControllerMotion.EditorFindState(lClimbLadderSSM, "ClimbToIdle");
            if (lState_N658178 == null) { lState_N658178 = lClimbLadderSSM.AddState("ClimbToIdle", new Vector3(948, -120, 0)); }
            lState_N658178.speed = 1f;
            lState_N658178.mirror = false;
            lState_N658178.tag = "";
            lState_N658178.motion = MotionControllerMotion.EditorFindAnimationClip(DefaultPaths.StandardAnimations + "Climbing/unity_Idle_JumpUpMedium_2Hands_Idle.fbx", "PullUpToIdle");

            AnimatorState lState_N658180 = MotionControllerMotion.EditorFindState(lClimbLadderSSM, "LadderTopOn");
            if (lState_N658180 == null) { lState_N658180 = lClimbLadderSSM.AddState("LadderTopOn", new Vector3(384, 168, 0)); }
            lState_N658180.speed = -1f;
            lState_N658180.mirror = false;
            lState_N658180.tag = "";
            lState_N658180.motion = MotionControllerMotion.EditorFindAnimationClip(DefaultPaths.StandardAnimations + "Climbing/unity_Idle_JumpUpMedium_2Hands_Idle.fbx", "PullUpToIdle");

            AnimatorState lState_N658182 = MotionControllerMotion.EditorFindState(lClimbLadderSSM, "IdleTurn180L");
            if (lState_N658182 == null) { lState_N658182 = lClimbLadderSSM.AddState("IdleTurn180L", new Vector3(384, 240, 0)); }
            lState_N658182.speed = 1f;
            lState_N658182.mirror = false;
            lState_N658182.tag = "";
            lState_N658182.motion = MotionControllerMotion.EditorFindAnimationClip(DefaultPaths.StandardAnimations + "Idling/unity_Idle_IdleToIdlesR.fbx", "IdleTurn180L");

            AnimatorState lState_N658184 = MotionControllerMotion.EditorFindState(lClimbLadderSSM, "JumpToClimb");
            if (lState_N658184 == null) { lState_N658184 = lClimbLadderSSM.AddState("JumpToClimb", new Vector3(384, 84, 0)); }
            lState_N658184.speed = 1f;
            lState_N658184.mirror = false;
            lState_N658184.tag = "";
            lState_N658184.motion = MotionControllerMotion.EditorFindAnimationClip(DefaultPaths.StandardAnimations + "Climbing/ootii_ClimbCrouch.fbx", "JumpToClimb");

            AnimatorStateTransition lAnyTransition_N658186 = MotionControllerMotion.EditorFindAnyStateTransition(lLayerStateMachine, lState_N658166, 0);
            if (lAnyTransition_N658186 == null) { lAnyTransition_N658186 = lLayerStateMachine.AddAnyStateTransition(lState_N658166); }
            lAnyTransition_N658186.isExit = false;
            lAnyTransition_N658186.hasExitTime = false;
            lAnyTransition_N658186.hasFixedDuration = true;
            lAnyTransition_N658186.exitTime = 0.9f;
            lAnyTransition_N658186.duration = 0.2f;
            lAnyTransition_N658186.offset = 0f;
            lAnyTransition_N658186.mute = false;
            lAnyTransition_N658186.solo = false;
            lAnyTransition_N658186.canTransitionToSelf = true;
            lAnyTransition_N658186.orderedInterruption = true;
            lAnyTransition_N658186.interruptionSource = (TransitionInterruptionSource)0;
            for (int i = lAnyTransition_N658186.conditions.Length - 1; i >= 0; i--) { lAnyTransition_N658186.RemoveCondition(lAnyTransition_N658186.conditions[i]); }
            lAnyTransition_N658186.AddCondition(AnimatorConditionMode.Equals, 1500f, "L" + rLayerIndex + "MotionPhase");

            AnimatorStateTransition lAnyTransition_N658188 = MotionControllerMotion.EditorFindAnyStateTransition(lLayerStateMachine, lState_N658182, 0);
            if (lAnyTransition_N658188 == null) { lAnyTransition_N658188 = lLayerStateMachine.AddAnyStateTransition(lState_N658182); }
            lAnyTransition_N658188.isExit = false;
            lAnyTransition_N658188.hasExitTime = false;
            lAnyTransition_N658188.hasFixedDuration = true;
            lAnyTransition_N658188.exitTime = 0.9f;
            lAnyTransition_N658188.duration = 0.09999999f;
            lAnyTransition_N658188.offset = 0.2584679f;
            lAnyTransition_N658188.mute = false;
            lAnyTransition_N658188.solo = false;
            lAnyTransition_N658188.canTransitionToSelf = true;
            lAnyTransition_N658188.orderedInterruption = true;
            lAnyTransition_N658188.interruptionSource = (TransitionInterruptionSource)0;
            for (int i = lAnyTransition_N658188.conditions.Length - 1; i >= 0; i--) { lAnyTransition_N658188.RemoveCondition(lAnyTransition_N658188.conditions[i]); }
            lAnyTransition_N658188.AddCondition(AnimatorConditionMode.Equals, 1506f, "L" + rLayerIndex + "MotionPhase");

            AnimatorStateTransition lAnyTransition_N658190 = MotionControllerMotion.EditorFindAnyStateTransition(lLayerStateMachine, lState_N658180, 0);
            if (lAnyTransition_N658190 == null) { lAnyTransition_N658190 = lLayerStateMachine.AddAnyStateTransition(lState_N658180); }
            lAnyTransition_N658190.isExit = false;
            lAnyTransition_N658190.hasExitTime = false;
            lAnyTransition_N658190.hasFixedDuration = true;
            lAnyTransition_N658190.exitTime = 0.9f;
            lAnyTransition_N658190.duration = 0.1f;
            lAnyTransition_N658190.offset = 0f;
            lAnyTransition_N658190.mute = false;
            lAnyTransition_N658190.solo = false;
            lAnyTransition_N658190.canTransitionToSelf = true;
            lAnyTransition_N658190.orderedInterruption = true;
            lAnyTransition_N658190.interruptionSource = (TransitionInterruptionSource)0;
            for (int i = lAnyTransition_N658190.conditions.Length - 1; i >= 0; i--) { lAnyTransition_N658190.RemoveCondition(lAnyTransition_N658190.conditions[i]); }
            lAnyTransition_N658190.AddCondition(AnimatorConditionMode.Equals, 1505f, "L" + rLayerIndex + "MotionPhase");

            AnimatorStateTransition lAnyTransition_N658192 = MotionControllerMotion.EditorFindAnyStateTransition(lLayerStateMachine, lState_N658184, 0);
            if (lAnyTransition_N658192 == null) { lAnyTransition_N658192 = lLayerStateMachine.AddAnyStateTransition(lState_N658184); }
            lAnyTransition_N658192.isExit = false;
            lAnyTransition_N658192.hasExitTime = false;
            lAnyTransition_N658192.hasFixedDuration = true;
            lAnyTransition_N658192.exitTime = 0.9f;
            lAnyTransition_N658192.duration = 0.1f;
            lAnyTransition_N658192.offset = 0f;
            lAnyTransition_N658192.mute = false;
            lAnyTransition_N658192.solo = false;
            lAnyTransition_N658192.canTransitionToSelf = true;
            lAnyTransition_N658192.orderedInterruption = true;
            lAnyTransition_N658192.interruptionSource = (TransitionInterruptionSource)0;
            for (int i = lAnyTransition_N658192.conditions.Length - 1; i >= 0; i--) { lAnyTransition_N658192.RemoveCondition(lAnyTransition_N658192.conditions[i]); }
            lAnyTransition_N658192.AddCondition(AnimatorConditionMode.Equals, 1504f, "L" + rLayerIndex + "MotionPhase");

            AnimatorStateTransition lTransition_N658194 = MotionControllerMotion.EditorFindTransition(lState_N658166, lState_N658168, 0);
            if (lTransition_N658194 == null) { lTransition_N658194 = lState_N658166.AddTransition(lState_N658168); }
            lTransition_N658194.isExit = false;
            lTransition_N658194.hasExitTime = true;
            lTransition_N658194.hasFixedDuration = true;
            lTransition_N658194.exitTime = 0.9289518f;
            lTransition_N658194.duration = 0.03373986f;
            lTransition_N658194.offset = 0f;
            lTransition_N658194.mute = false;
            lTransition_N658194.solo = false;
            lTransition_N658194.canTransitionToSelf = true;
            lTransition_N658194.orderedInterruption = true;
            lTransition_N658194.interruptionSource = (TransitionInterruptionSource)0;
            for (int i = lTransition_N658194.conditions.Length - 1; i >= 0; i--) { lTransition_N658194.RemoveCondition(lTransition_N658194.conditions[i]); }

            AnimatorStateTransition lTransition_N658196 = MotionControllerMotion.EditorFindTransition(lState_N658168, lState_N658170, 0);
            if (lTransition_N658196 == null) { lTransition_N658196 = lState_N658168.AddTransition(lState_N658170); }
            lTransition_N658196.isExit = false;
            lTransition_N658196.hasExitTime = false;
            lTransition_N658196.hasFixedDuration = true;
            lTransition_N658196.exitTime = 0.6739131f;
            lTransition_N658196.duration = 0.25f;
            lTransition_N658196.offset = 0f;
            lTransition_N658196.mute = false;
            lTransition_N658196.solo = false;
            lTransition_N658196.canTransitionToSelf = true;
            lTransition_N658196.orderedInterruption = true;
            lTransition_N658196.interruptionSource = (TransitionInterruptionSource)0;
            for (int i = lTransition_N658196.conditions.Length - 1; i >= 0; i--) { lTransition_N658196.RemoveCondition(lTransition_N658196.conditions[i]); }
            lTransition_N658196.AddCondition(AnimatorConditionMode.Less, -0.1f, "InputY");

            AnimatorStateTransition lTransition_N658198 = MotionControllerMotion.EditorFindTransition(lState_N658168, lState_N658174, 0);
            if (lTransition_N658198 == null) { lTransition_N658198 = lState_N658168.AddTransition(lState_N658174); }
            lTransition_N658198.isExit = false;
            lTransition_N658198.hasExitTime = true;
            lTransition_N658198.hasFixedDuration = true;
            lTransition_N658198.exitTime = 1f;
            lTransition_N658198.duration = 0f;
            lTransition_N658198.offset = 0f;
            lTransition_N658198.mute = false;
            lTransition_N658198.solo = false;
            lTransition_N658198.canTransitionToSelf = true;
            lTransition_N658198.orderedInterruption = true;
            lTransition_N658198.interruptionSource = (TransitionInterruptionSource)0;
            for (int i = lTransition_N658198.conditions.Length - 1; i >= 0; i--) { lTransition_N658198.RemoveCondition(lTransition_N658198.conditions[i]); }
            lTransition_N658198.AddCondition(AnimatorConditionMode.Greater, -0.1f, "InputY");
            lTransition_N658198.AddCondition(AnimatorConditionMode.Less, 0.1f, "InputY");

            AnimatorStateTransition lTransition_N658200 = MotionControllerMotion.EditorFindTransition(lState_N658168, lState_N658178, 0);
            if (lTransition_N658200 == null) { lTransition_N658200 = lState_N658168.AddTransition(lState_N658178); }
            lTransition_N658200.isExit = false;
            lTransition_N658200.hasExitTime = false;
            lTransition_N658200.hasFixedDuration = true;
            lTransition_N658200.exitTime = 0.6875f;
            lTransition_N658200.duration = 0.25f;
            lTransition_N658200.offset = 0f;
            lTransition_N658200.mute = false;
            lTransition_N658200.solo = false;
            lTransition_N658200.canTransitionToSelf = true;
            lTransition_N658200.orderedInterruption = true;
            lTransition_N658200.interruptionSource = (TransitionInterruptionSource)0;
            for (int i = lTransition_N658200.conditions.Length - 1; i >= 0; i--) { lTransition_N658200.RemoveCondition(lTransition_N658200.conditions[i]); }
            lTransition_N658200.AddCondition(AnimatorConditionMode.Equals, 1520f, "L" + rLayerIndex + "MotionPhase");

            AnimatorStateTransition lTransition_N658202 = MotionControllerMotion.EditorFindTransition(lState_N658170, lState_N658172, 0);
            if (lTransition_N658202 == null) { lTransition_N658202 = lState_N658170.AddTransition(lState_N658172); }
            lTransition_N658202.isExit = false;
            lTransition_N658202.hasExitTime = false;
            lTransition_N658202.hasFixedDuration = true;
            lTransition_N658202.exitTime = 0.8700946f;
            lTransition_N658202.duration = 0.1035532f;
            lTransition_N658202.offset = 0f;
            lTransition_N658202.mute = false;
            lTransition_N658202.solo = false;
            lTransition_N658202.canTransitionToSelf = true;
            lTransition_N658202.orderedInterruption = true;
            lTransition_N658202.interruptionSource = (TransitionInterruptionSource)0;
            for (int i = lTransition_N658202.conditions.Length - 1; i >= 0; i--) { lTransition_N658202.RemoveCondition(lTransition_N658202.conditions[i]); }
            lTransition_N658202.AddCondition(AnimatorConditionMode.Equals, 1510f, "L" + rLayerIndex + "MotionPhase");

            AnimatorStateTransition lTransition_N658204 = MotionControllerMotion.EditorFindTransition(lState_N658170, lState_N658168, 0);
            if (lTransition_N658204 == null) { lTransition_N658204 = lState_N658170.AddTransition(lState_N658168); }
            lTransition_N658204.isExit = false;
            lTransition_N658204.hasExitTime = false;
            lTransition_N658204.hasFixedDuration = true;
            lTransition_N658204.exitTime = 0.6739131f;
            lTransition_N658204.duration = 0.25f;
            lTransition_N658204.offset = 0f;
            lTransition_N658204.mute = false;
            lTransition_N658204.solo = false;
            lTransition_N658204.canTransitionToSelf = true;
            lTransition_N658204.orderedInterruption = true;
            lTransition_N658204.interruptionSource = (TransitionInterruptionSource)0;
            for (int i = lTransition_N658204.conditions.Length - 1; i >= 0; i--) { lTransition_N658204.RemoveCondition(lTransition_N658204.conditions[i]); }
            lTransition_N658204.AddCondition(AnimatorConditionMode.Greater, 0.1f, "InputY");

            AnimatorStateTransition lTransition_N658206 = MotionControllerMotion.EditorFindTransition(lState_N658170, lState_N658174, 0);
            if (lTransition_N658206 == null) { lTransition_N658206 = lState_N658170.AddTransition(lState_N658174); }
            lTransition_N658206.isExit = false;
            lTransition_N658206.hasExitTime = true;
            lTransition_N658206.hasFixedDuration = true;
            lTransition_N658206.exitTime = 1f;
            lTransition_N658206.duration = 0f;
            lTransition_N658206.offset = 0f;
            lTransition_N658206.mute = false;
            lTransition_N658206.solo = false;
            lTransition_N658206.canTransitionToSelf = true;
            lTransition_N658206.orderedInterruption = true;
            lTransition_N658206.interruptionSource = (TransitionInterruptionSource)0;
            for (int i = lTransition_N658206.conditions.Length - 1; i >= 0; i--) { lTransition_N658206.RemoveCondition(lTransition_N658206.conditions[i]); }
            lTransition_N658206.AddCondition(AnimatorConditionMode.Greater, -0.1f, "InputY");
            lTransition_N658206.AddCondition(AnimatorConditionMode.Less, 0.1f, "InputY");

            AnimatorStateTransition lTransition_N658208 = MotionControllerMotion.EditorFindTransition(lState_N658172, lState_N658176, 0);
            if (lTransition_N658208 == null) { lTransition_N658208 = lState_N658172.AddTransition(lState_N658176); }
            lTransition_N658208.isExit = false;
            lTransition_N658208.hasExitTime = true;
            lTransition_N658208.hasFixedDuration = true;
            lTransition_N658208.exitTime = 0.5000001f;
            lTransition_N658208.duration = 0.1107834f;
            lTransition_N658208.offset = 0f;
            lTransition_N658208.mute = false;
            lTransition_N658208.solo = false;
            lTransition_N658208.canTransitionToSelf = true;
            lTransition_N658208.orderedInterruption = true;
            lTransition_N658208.interruptionSource = (TransitionInterruptionSource)0;
            for (int i = lTransition_N658208.conditions.Length - 1; i >= 0; i--) { lTransition_N658208.RemoveCondition(lTransition_N658208.conditions[i]); }

            AnimatorStateTransition lTransition_N658210 = MotionControllerMotion.EditorFindTransition(lState_N658174, lState_N658170, 0);
            if (lTransition_N658210 == null) { lTransition_N658210 = lState_N658174.AddTransition(lState_N658170); }
            lTransition_N658210.isExit = false;
            lTransition_N658210.hasExitTime = true;
            lTransition_N658210.hasFixedDuration = true;
            lTransition_N658210.exitTime = 0f;
            lTransition_N658210.duration = 0f;
            lTransition_N658210.offset = 0f;
            lTransition_N658210.mute = false;
            lTransition_N658210.solo = false;
            lTransition_N658210.canTransitionToSelf = true;
            lTransition_N658210.orderedInterruption = true;
            lTransition_N658210.interruptionSource = (TransitionInterruptionSource)0;
            for (int i = lTransition_N658210.conditions.Length - 1; i >= 0; i--) { lTransition_N658210.RemoveCondition(lTransition_N658210.conditions[i]); }
            lTransition_N658210.AddCondition(AnimatorConditionMode.Less, -0.1f, "InputY");

            AnimatorStateTransition lTransition_N658212 = MotionControllerMotion.EditorFindTransition(lState_N658174, lState_N658168, 0);
            if (lTransition_N658212 == null) { lTransition_N658212 = lState_N658174.AddTransition(lState_N658168); }
            lTransition_N658212.isExit = false;
            lTransition_N658212.hasExitTime = true;
            lTransition_N658212.hasFixedDuration = true;
            lTransition_N658212.exitTime = 0f;
            lTransition_N658212.duration = 0f;
            lTransition_N658212.offset = 0f;
            lTransition_N658212.mute = false;
            lTransition_N658212.solo = false;
            lTransition_N658212.canTransitionToSelf = true;
            lTransition_N658212.orderedInterruption = true;
            lTransition_N658212.interruptionSource = (TransitionInterruptionSource)0;
            for (int i = lTransition_N658212.conditions.Length - 1; i >= 0; i--) { lTransition_N658212.RemoveCondition(lTransition_N658212.conditions[i]); }
            lTransition_N658212.AddCondition(AnimatorConditionMode.Greater, 0.1f, "InputY");

            AnimatorStateTransition lTransition_N658214 = MotionControllerMotion.EditorFindTransition(lState_N658178, lState_N658176, 0);
            if (lTransition_N658214 == null) { lTransition_N658214 = lState_N658178.AddTransition(lState_N658176); }
            lTransition_N658214.isExit = false;
            lTransition_N658214.hasExitTime = true;
            lTransition_N658214.hasFixedDuration = true;
            lTransition_N658214.exitTime = 0.8255814f;
            lTransition_N658214.duration = 0.25f;
            lTransition_N658214.offset = 0f;
            lTransition_N658214.mute = false;
            lTransition_N658214.solo = false;
            lTransition_N658214.canTransitionToSelf = true;
            lTransition_N658214.orderedInterruption = true;
            lTransition_N658214.interruptionSource = (TransitionInterruptionSource)0;
            for (int i = lTransition_N658214.conditions.Length - 1; i >= 0; i--) { lTransition_N658214.RemoveCondition(lTransition_N658214.conditions[i]); }

            AnimatorStateTransition lTransition_N658216 = MotionControllerMotion.EditorFindTransition(lState_N658180, lState_N658170, 0);
            if (lTransition_N658216 == null) { lTransition_N658216 = lState_N658180.AddTransition(lState_N658170); }
            lTransition_N658216.isExit = false;
            lTransition_N658216.hasExitTime = true;
            lTransition_N658216.hasFixedDuration = true;
            lTransition_N658216.exitTime = 0.8706896f;
            lTransition_N658216.duration = 0.25f;
            lTransition_N658216.offset = 0f;
            lTransition_N658216.mute = false;
            lTransition_N658216.solo = false;
            lTransition_N658216.canTransitionToSelf = true;
            lTransition_N658216.orderedInterruption = true;
            lTransition_N658216.interruptionSource = (TransitionInterruptionSource)0;
            for (int i = lTransition_N658216.conditions.Length - 1; i >= 0; i--) { lTransition_N658216.RemoveCondition(lTransition_N658216.conditions[i]); }

            AnimatorStateTransition lTransition_N658218 = MotionControllerMotion.EditorFindTransition(lState_N658182, lState_N658180, 0);
            if (lTransition_N658218 == null) { lTransition_N658218 = lState_N658182.AddTransition(lState_N658180); }
            lTransition_N658218.isExit = false;
            lTransition_N658218.hasExitTime = true;
            lTransition_N658218.hasFixedDuration = true;
            lTransition_N658218.exitTime = 0.7307953f;
            lTransition_N658218.duration = 0.2499998f;
            lTransition_N658218.offset = 0f;
            lTransition_N658218.mute = false;
            lTransition_N658218.solo = false;
            lTransition_N658218.canTransitionToSelf = true;
            lTransition_N658218.orderedInterruption = true;
            lTransition_N658218.interruptionSource = (TransitionInterruptionSource)0;
            for (int i = lTransition_N658218.conditions.Length - 1; i >= 0; i--) { lTransition_N658218.RemoveCondition(lTransition_N658218.conditions[i]); }

            AnimatorStateTransition lTransition_N658220 = MotionControllerMotion.EditorFindTransition(lState_N658184, lState_N658174, 0);
            if (lTransition_N658220 == null) { lTransition_N658220 = lState_N658184.AddTransition(lState_N658174); }
            lTransition_N658220.isExit = false;
            lTransition_N658220.hasExitTime = true;
            lTransition_N658220.hasFixedDuration = true;
            lTransition_N658220.exitTime = 0.7f;
            lTransition_N658220.duration = 0.3f;
            lTransition_N658220.offset = 0f;
            lTransition_N658220.mute = false;
            lTransition_N658220.solo = false;
            lTransition_N658220.canTransitionToSelf = true;
            lTransition_N658220.orderedInterruption = true;
            lTransition_N658220.interruptionSource = (TransitionInterruptionSource)0;
            for (int i = lTransition_N658220.conditions.Length - 1; i >= 0; i--) { lTransition_N658220.RemoveCondition(lTransition_N658220.conditions[i]); }

            return lClimbLadderSSM;
        }

        /// <summary>
        /// Verifies that the standard Climb Wall SSM has been created
        /// </summary>
        /// <param name="rMotionController"></param>
        /// <param name="rLayerIndex"></param>
        /// <param name="rCreateStates"></param>
        /// <returns></returns>
        public static AnimatorStateMachine EnsureStandardClimbWall(MotionController rMotionController, int rLayerIndex,
            bool rCreateStates = true)
        {
            AnimatorController lController = GetAnimatorController(rMotionController);
            if (lController == null) return null;

            AnimatorControllerLayer lLayer = lController.layers[rLayerIndex];
            AnimatorStateMachine lLayerStateMachine = lLayer.stateMachine;

            AnimatorStateMachine lClimbSSM = MotionControllerMotion.EditorFindSSM(lLayerStateMachine, "ClimbWall-SM");
            if (lClimbSSM == null) { lClimbSSM = lLayerStateMachine.AddStateMachine("ClimbWall-SM", new Vector3(552, -108, 0)); }

            if (!rCreateStates) return lClimbSSM;

            AnimatorState lState_N660460 = MotionControllerMotion.EditorFindState(lClimbSSM, "ScaleWallBottomOn");
            if (lState_N660460 == null) { lState_N660460 = lClimbSSM.AddState("ScaleWallBottomOn", new Vector3(384, -24, 0)); }
            lState_N660460.speed = 1.5f;
            lState_N660460.mirror = false;
            lState_N660460.tag = "";
            lState_N660460.motion = MotionControllerMotion.EditorFindAnimationClip(DefaultPaths.StandardAnimations + "Climbing/ootii_ScaleWall.fbx", "ScaleWallBottomOn");

            AnimatorState lState_N660462 = MotionControllerMotion.EditorFindState(lClimbSSM, "ScaleWallUp");
            if (lState_N660462 == null) { lState_N660462 = lClimbSSM.AddState("ScaleWallUp", new Vector3(756, -24, 0)); }
            lState_N660462.speed = 1f;
            lState_N660462.mirror = false;
            lState_N660462.tag = "";
            lState_N660462.motion = MotionControllerMotion.EditorFindAnimationClip(DefaultPaths.StandardAnimations + "Climbing/ootii_ScaleWall.fbx", "ScaleWallUpLoop");

            AnimatorState lState_N660464 = MotionControllerMotion.EditorFindState(lClimbSSM, "ScaleWallDown");
            if (lState_N660464 == null) { lState_N660464 = lClimbSSM.AddState("ScaleWallDown", new Vector3(756, 168, 0)); }
            lState_N660464.speed = 1f;
            lState_N660464.mirror = false;
            lState_N660464.tag = "";
            lState_N660464.motion = MotionControllerMotion.EditorFindAnimationClip(DefaultPaths.StandardAnimations + "Climbing/ootii_ScaleWall.fbx", "ScaleWallDownLoop");

            AnimatorState lState_N660466 = MotionControllerMotion.EditorFindState(lClimbSSM, "ScaleWallBottomOff");
            if (lState_N660466 == null) { lState_N660466 = lClimbSSM.AddState("ScaleWallBottomOff", new Vector3(948, 276, 0)); }
            lState_N660466.speed = 1f;
            lState_N660466.mirror = false;
            lState_N660466.tag = "";
            lState_N660466.motion = MotionControllerMotion.EditorFindAnimationClip(DefaultPaths.StandardAnimations + "Climbing/ootii_ScaleWall.fbx", "ScaleWallBottomOff");

            AnimatorState lState_N660468 = MotionControllerMotion.EditorFindState(lClimbSSM, "ScaleWallPose");
            if (lState_N660468 == null) { lState_N660468 = lClimbSSM.AddState("ScaleWallPose", new Vector3(612, 72, 0)); }
            lState_N660468.speed = 1f;
            lState_N660468.mirror = false;
            lState_N660468.tag = "";
            lState_N660468.motion = MotionControllerMotion.EditorFindAnimationClip(DefaultPaths.StandardAnimations + "Climbing/ootii_ScaleWall.fbx", "ScaleWallPose");

            AnimatorState lState_N660470 = MotionControllerMotion.EditorFindState(lClimbSSM, "IdlePose");
            if (lState_N660470 == null) { lState_N660470 = lClimbSSM.AddState("IdlePose", new Vector3(984, 84, 0)); }
            lState_N660470.speed = 1f;
            lState_N660470.mirror = false;
            lState_N660470.tag = "";
            lState_N660470.motion = MotionControllerMotion.EditorFindAnimationClip(DefaultPaths.StandardAnimations + "Idling/unity_Idle_IdleToIdlesR.fbx", "IdlePose");

            AnimatorState lState_N660472 = MotionControllerMotion.EditorFindState(lClimbSSM, "ClimbToIdle");
            if (lState_N660472 == null) { lState_N660472 = lClimbSSM.AddState("ClimbToIdle", new Vector3(948, -120, 0)); }
            lState_N660472.speed = 1f;
            lState_N660472.mirror = false;
            lState_N660472.tag = "";
            lState_N660472.motion = MotionControllerMotion.EditorFindAnimationClip(DefaultPaths.StandardAnimations + "Climbing/unity_Idle_JumpUpMedium_2Hands_Idle.fbx", "PullUpToIdle");

            AnimatorState lState_N660474 = MotionControllerMotion.EditorFindState(lClimbSSM, "ScaleWallTopOn");
            if (lState_N660474 == null) { lState_N660474 = lClimbSSM.AddState("ScaleWallTopOn", new Vector3(384, 168, 0)); }
            lState_N660474.speed = -1f;
            lState_N660474.mirror = false;
            lState_N660474.tag = "";
            lState_N660474.motion = MotionControllerMotion.EditorFindAnimationClip(DefaultPaths.StandardAnimations + "Climbing/unity_Idle_JumpUpMedium_2Hands_Idle.fbx", "PullUpToIdle");

            AnimatorState lState_N660476 = MotionControllerMotion.EditorFindState(lClimbSSM, "IdleTurn180L");
            if (lState_N660476 == null) { lState_N660476 = lClimbSSM.AddState("IdleTurn180L", new Vector3(384, 240, 0)); }
            lState_N660476.speed = 1f;
            lState_N660476.mirror = false;
            lState_N660476.tag = "";
            lState_N660476.motion = MotionControllerMotion.EditorFindAnimationClip(DefaultPaths.StandardAnimations + "Idling/unity_Idle_IdleToIdlesR.fbx", "IdleTurn180L");

            AnimatorState lState_N660478 = MotionControllerMotion.EditorFindState(lClimbSSM, "JumpToClimb");
            if (lState_N660478 == null) { lState_N660478 = lClimbSSM.AddState("JumpToClimb", new Vector3(384, 72, 0)); }
            lState_N660478.speed = 1f;
            lState_N660478.mirror = false;
            lState_N660478.tag = "";

            AnimatorStateTransition lAnyTransition_N660480 = MotionControllerMotion.EditorFindAnyStateTransition(lLayerStateMachine, lState_N660460, 0);
            if (lAnyTransition_N660480 == null) { lAnyTransition_N660480 = lLayerStateMachine.AddAnyStateTransition(lState_N660460); }
            lAnyTransition_N660480.isExit = false;
            lAnyTransition_N660480.hasExitTime = false;
            lAnyTransition_N660480.hasFixedDuration = true;
            lAnyTransition_N660480.exitTime = 0.9f;
            lAnyTransition_N660480.duration = 0.2f;
            lAnyTransition_N660480.offset = 0f;
            lAnyTransition_N660480.mute = false;
            lAnyTransition_N660480.solo = false;
            lAnyTransition_N660480.canTransitionToSelf = true;
            lAnyTransition_N660480.orderedInterruption = true;
            lAnyTransition_N660480.interruptionSource = (TransitionInterruptionSource)0;
            for (int i = lAnyTransition_N660480.conditions.Length - 1; i >= 0; i--) { lAnyTransition_N660480.RemoveCondition(lAnyTransition_N660480.conditions[i]); }
            lAnyTransition_N660480.AddCondition(AnimatorConditionMode.Equals, 1600f, "L" + rLayerIndex + "MotionPhase");

            AnimatorStateTransition lAnyTransition_N660482 = MotionControllerMotion.EditorFindAnyStateTransition(lLayerStateMachine, lState_N660474, 0);
            if (lAnyTransition_N660482 == null) { lAnyTransition_N660482 = lLayerStateMachine.AddAnyStateTransition(lState_N660474); }
            lAnyTransition_N660482.isExit = false;
            lAnyTransition_N660482.hasExitTime = false;
            lAnyTransition_N660482.hasFixedDuration = true;
            lAnyTransition_N660482.exitTime = 0.9f;
            lAnyTransition_N660482.duration = 0.1f;
            lAnyTransition_N660482.offset = 0f;
            lAnyTransition_N660482.mute = false;
            lAnyTransition_N660482.solo = false;
            lAnyTransition_N660482.canTransitionToSelf = true;
            lAnyTransition_N660482.orderedInterruption = true;
            lAnyTransition_N660482.interruptionSource = (TransitionInterruptionSource)0;
            for (int i = lAnyTransition_N660482.conditions.Length - 1; i >= 0; i--) { lAnyTransition_N660482.RemoveCondition(lAnyTransition_N660482.conditions[i]); }
            lAnyTransition_N660482.AddCondition(AnimatorConditionMode.Equals, 1605f, "L" + rLayerIndex + "MotionPhase");

            AnimatorStateTransition lAnyTransition_N660484 = MotionControllerMotion.EditorFindAnyStateTransition(lLayerStateMachine, lState_N660476, 0);
            if (lAnyTransition_N660484 == null) { lAnyTransition_N660484 = lLayerStateMachine.AddAnyStateTransition(lState_N660476); }
            lAnyTransition_N660484.isExit = false;
            lAnyTransition_N660484.hasExitTime = false;
            lAnyTransition_N660484.hasFixedDuration = true;
            lAnyTransition_N660484.exitTime = 0.9f;
            lAnyTransition_N660484.duration = 0.1f;
            lAnyTransition_N660484.offset = 0f;
            lAnyTransition_N660484.mute = false;
            lAnyTransition_N660484.solo = false;
            lAnyTransition_N660484.canTransitionToSelf = true;
            lAnyTransition_N660484.orderedInterruption = true;
            lAnyTransition_N660484.interruptionSource = (TransitionInterruptionSource)0;
            for (int i = lAnyTransition_N660484.conditions.Length - 1; i >= 0; i--) { lAnyTransition_N660484.RemoveCondition(lAnyTransition_N660484.conditions[i]); }
            lAnyTransition_N660484.AddCondition(AnimatorConditionMode.Equals, 1606f, "L" + rLayerIndex + "MotionPhase");

            AnimatorStateTransition lAnyTransition_N660486 = MotionControllerMotion.EditorFindAnyStateTransition(lLayerStateMachine, lState_N660478, 0);
            if (lAnyTransition_N660486 == null) { lAnyTransition_N660486 = lLayerStateMachine.AddAnyStateTransition(lState_N660478); }
            lAnyTransition_N660486.isExit = false;
            lAnyTransition_N660486.hasExitTime = false;
            lAnyTransition_N660486.hasFixedDuration = true;
            lAnyTransition_N660486.exitTime = 0.9f;
            lAnyTransition_N660486.duration = 0.1f;
            lAnyTransition_N660486.offset = 0f;
            lAnyTransition_N660486.mute = false;
            lAnyTransition_N660486.solo = false;
            lAnyTransition_N660486.canTransitionToSelf = true;
            lAnyTransition_N660486.orderedInterruption = true;
            lAnyTransition_N660486.interruptionSource = (TransitionInterruptionSource)0;
            for (int i = lAnyTransition_N660486.conditions.Length - 1; i >= 0; i--) { lAnyTransition_N660486.RemoveCondition(lAnyTransition_N660486.conditions[i]); }
            lAnyTransition_N660486.AddCondition(AnimatorConditionMode.Equals, 1604f, "L" + rLayerIndex + "MotionPhase");

            AnimatorStateTransition lTransition_N660488 = MotionControllerMotion.EditorFindTransition(lState_N660460, lState_N660462, 0);
            if (lTransition_N660488 == null) { lTransition_N660488 = lState_N660460.AddTransition(lState_N660462); }
            lTransition_N660488.isExit = false;
            lTransition_N660488.hasExitTime = true;
            lTransition_N660488.hasFixedDuration = true;
            lTransition_N660488.exitTime = 0.8279549f;
            lTransition_N660488.duration = 0.03373984f;
            lTransition_N660488.offset = 0f;
            lTransition_N660488.mute = false;
            lTransition_N660488.solo = false;
            lTransition_N660488.canTransitionToSelf = true;
            lTransition_N660488.orderedInterruption = true;
            lTransition_N660488.interruptionSource = (TransitionInterruptionSource)0;
            for (int i = lTransition_N660488.conditions.Length - 1; i >= 0; i--) { lTransition_N660488.RemoveCondition(lTransition_N660488.conditions[i]); }

            AnimatorStateTransition lTransition_N660490 = MotionControllerMotion.EditorFindTransition(lState_N660462, lState_N660464, 0);
            if (lTransition_N660490 == null) { lTransition_N660490 = lState_N660462.AddTransition(lState_N660464); }
            lTransition_N660490.isExit = false;
            lTransition_N660490.hasExitTime = false;
            lTransition_N660490.hasFixedDuration = true;
            lTransition_N660490.exitTime = 0.6739131f;
            lTransition_N660490.duration = 0.25f;
            lTransition_N660490.offset = 0f;
            lTransition_N660490.mute = false;
            lTransition_N660490.solo = false;
            lTransition_N660490.canTransitionToSelf = true;
            lTransition_N660490.orderedInterruption = true;
            lTransition_N660490.interruptionSource = (TransitionInterruptionSource)0;
            for (int i = lTransition_N660490.conditions.Length - 1; i >= 0; i--) { lTransition_N660490.RemoveCondition(lTransition_N660490.conditions[i]); }
            lTransition_N660490.AddCondition(AnimatorConditionMode.Less, -0.1f, "InputY");

            AnimatorStateTransition lTransition_N660492 = MotionControllerMotion.EditorFindTransition(lState_N660462, lState_N660468, 0);
            if (lTransition_N660492 == null) { lTransition_N660492 = lState_N660462.AddTransition(lState_N660468); }
            lTransition_N660492.isExit = false;
            lTransition_N660492.hasExitTime = true;
            lTransition_N660492.hasFixedDuration = true;
            lTransition_N660492.exitTime = 1f;
            lTransition_N660492.duration = 0f;
            lTransition_N660492.offset = 0f;
            lTransition_N660492.mute = false;
            lTransition_N660492.solo = false;
            lTransition_N660492.canTransitionToSelf = true;
            lTransition_N660492.orderedInterruption = true;
            lTransition_N660492.interruptionSource = (TransitionInterruptionSource)0;
            for (int i = lTransition_N660492.conditions.Length - 1; i >= 0; i--) { lTransition_N660492.RemoveCondition(lTransition_N660492.conditions[i]); }
            lTransition_N660492.AddCondition(AnimatorConditionMode.Greater, -0.1f, "InputY");
            lTransition_N660492.AddCondition(AnimatorConditionMode.Less, 0.1f, "InputY");

            AnimatorStateTransition lTransition_N660494 = MotionControllerMotion.EditorFindTransition(lState_N660462, lState_N660472, 0);
            if (lTransition_N660494 == null) { lTransition_N660494 = lState_N660462.AddTransition(lState_N660472); }
            lTransition_N660494.isExit = false;
            lTransition_N660494.hasExitTime = false;
            lTransition_N660494.hasFixedDuration = true;
            lTransition_N660494.exitTime = 0.6875f;
            lTransition_N660494.duration = 0.25f;
            lTransition_N660494.offset = 0f;
            lTransition_N660494.mute = false;
            lTransition_N660494.solo = false;
            lTransition_N660494.canTransitionToSelf = true;
            lTransition_N660494.orderedInterruption = true;
            lTransition_N660494.interruptionSource = (TransitionInterruptionSource)0;
            for (int i = lTransition_N660494.conditions.Length - 1; i >= 0; i--) { lTransition_N660494.RemoveCondition(lTransition_N660494.conditions[i]); }
            lTransition_N660494.AddCondition(AnimatorConditionMode.Equals, 1620f, "L" + rLayerIndex + "MotionPhase");

            AnimatorStateTransition lTransition_N660496 = MotionControllerMotion.EditorFindTransition(lState_N660464, lState_N660466, 0);
            if (lTransition_N660496 == null) { lTransition_N660496 = lState_N660464.AddTransition(lState_N660466); }
            lTransition_N660496.isExit = false;
            lTransition_N660496.hasExitTime = false;
            lTransition_N660496.hasFixedDuration = true;
            lTransition_N660496.exitTime = 0.9013604f;
            lTransition_N660496.duration = 0.1806295f;
            lTransition_N660496.offset = 0f;
            lTransition_N660496.mute = false;
            lTransition_N660496.solo = false;
            lTransition_N660496.canTransitionToSelf = true;
            lTransition_N660496.orderedInterruption = true;
            lTransition_N660496.interruptionSource = (TransitionInterruptionSource)0;
            for (int i = lTransition_N660496.conditions.Length - 1; i >= 0; i--) { lTransition_N660496.RemoveCondition(lTransition_N660496.conditions[i]); }
            lTransition_N660496.AddCondition(AnimatorConditionMode.Equals, 1610f, "L" + rLayerIndex + "MotionPhase");

            AnimatorStateTransition lTransition_N660498 = MotionControllerMotion.EditorFindTransition(lState_N660464, lState_N660462, 0);
            if (lTransition_N660498 == null) { lTransition_N660498 = lState_N660464.AddTransition(lState_N660462); }
            lTransition_N660498.isExit = false;
            lTransition_N660498.hasExitTime = false;
            lTransition_N660498.hasFixedDuration = true;
            lTransition_N660498.exitTime = 0.6739131f;
            lTransition_N660498.duration = 0.25f;
            lTransition_N660498.offset = 0f;
            lTransition_N660498.mute = false;
            lTransition_N660498.solo = false;
            lTransition_N660498.canTransitionToSelf = true;
            lTransition_N660498.orderedInterruption = true;
            lTransition_N660498.interruptionSource = (TransitionInterruptionSource)0;
            for (int i = lTransition_N660498.conditions.Length - 1; i >= 0; i--) { lTransition_N660498.RemoveCondition(lTransition_N660498.conditions[i]); }
            lTransition_N660498.AddCondition(AnimatorConditionMode.Greater, 0.1f, "InputY");

            AnimatorStateTransition lTransition_N660500 = MotionControllerMotion.EditorFindTransition(lState_N660464, lState_N660468, 0);
            if (lTransition_N660500 == null) { lTransition_N660500 = lState_N660464.AddTransition(lState_N660468); }
            lTransition_N660500.isExit = false;
            lTransition_N660500.hasExitTime = true;
            lTransition_N660500.hasFixedDuration = true;
            lTransition_N660500.exitTime = 1f;
            lTransition_N660500.duration = 0f;
            lTransition_N660500.offset = 0f;
            lTransition_N660500.mute = false;
            lTransition_N660500.solo = false;
            lTransition_N660500.canTransitionToSelf = true;
            lTransition_N660500.orderedInterruption = true;
            lTransition_N660500.interruptionSource = (TransitionInterruptionSource)0;
            for (int i = lTransition_N660500.conditions.Length - 1; i >= 0; i--) { lTransition_N660500.RemoveCondition(lTransition_N660500.conditions[i]); }
            lTransition_N660500.AddCondition(AnimatorConditionMode.Greater, -0.1f, "InputY");
            lTransition_N660500.AddCondition(AnimatorConditionMode.Less, 0.1f, "InputY");

            AnimatorStateTransition lTransition_N660502 = MotionControllerMotion.EditorFindTransition(lState_N660466, lState_N660470, 0);
            if (lTransition_N660502 == null) { lTransition_N660502 = lState_N660466.AddTransition(lState_N660470); }
            lTransition_N660502.isExit = false;
            lTransition_N660502.hasExitTime = true;
            lTransition_N660502.hasFixedDuration = true;
            lTransition_N660502.exitTime = 0.5000001f;
            lTransition_N660502.duration = 0.1107834f;
            lTransition_N660502.offset = 0f;
            lTransition_N660502.mute = false;
            lTransition_N660502.solo = false;
            lTransition_N660502.canTransitionToSelf = true;
            lTransition_N660502.orderedInterruption = true;
            lTransition_N660502.interruptionSource = (TransitionInterruptionSource)0;
            for (int i = lTransition_N660502.conditions.Length - 1; i >= 0; i--) { lTransition_N660502.RemoveCondition(lTransition_N660502.conditions[i]); }

            AnimatorStateTransition lTransition_N660504 = MotionControllerMotion.EditorFindTransition(lState_N660468, lState_N660464, 0);
            if (lTransition_N660504 == null) { lTransition_N660504 = lState_N660468.AddTransition(lState_N660464); }
            lTransition_N660504.isExit = false;
            lTransition_N660504.hasExitTime = true;
            lTransition_N660504.hasFixedDuration = true;
            lTransition_N660504.exitTime = 0f;
            lTransition_N660504.duration = 0f;
            lTransition_N660504.offset = 0f;
            lTransition_N660504.mute = false;
            lTransition_N660504.solo = false;
            lTransition_N660504.canTransitionToSelf = true;
            lTransition_N660504.orderedInterruption = true;
            lTransition_N660504.interruptionSource = (TransitionInterruptionSource)0;
            for (int i = lTransition_N660504.conditions.Length - 1; i >= 0; i--) { lTransition_N660504.RemoveCondition(lTransition_N660504.conditions[i]); }
            lTransition_N660504.AddCondition(AnimatorConditionMode.Less, -0.1f, "InputY");

            AnimatorStateTransition lTransition_N660506 = MotionControllerMotion.EditorFindTransition(lState_N660468, lState_N660462, 0);
            if (lTransition_N660506 == null) { lTransition_N660506 = lState_N660468.AddTransition(lState_N660462); }
            lTransition_N660506.isExit = false;
            lTransition_N660506.hasExitTime = true;
            lTransition_N660506.hasFixedDuration = true;
            lTransition_N660506.exitTime = 0f;
            lTransition_N660506.duration = 0f;
            lTransition_N660506.offset = 0f;
            lTransition_N660506.mute = false;
            lTransition_N660506.solo = false;
            lTransition_N660506.canTransitionToSelf = true;
            lTransition_N660506.orderedInterruption = true;
            lTransition_N660506.interruptionSource = (TransitionInterruptionSource)0;
            for (int i = lTransition_N660506.conditions.Length - 1; i >= 0; i--) { lTransition_N660506.RemoveCondition(lTransition_N660506.conditions[i]); }
            lTransition_N660506.AddCondition(AnimatorConditionMode.Greater, 0.1f, "InputY");

            AnimatorStateTransition lTransition_N660508 = MotionControllerMotion.EditorFindTransition(lState_N660472, lState_N660470, 0);
            if (lTransition_N660508 == null) { lTransition_N660508 = lState_N660472.AddTransition(lState_N660470); }
            lTransition_N660508.isExit = false;
            lTransition_N660508.hasExitTime = true;
            lTransition_N660508.hasFixedDuration = true;
            lTransition_N660508.exitTime = 0.8255814f;
            lTransition_N660508.duration = 0.25f;
            lTransition_N660508.offset = 0f;
            lTransition_N660508.mute = false;
            lTransition_N660508.solo = false;
            lTransition_N660508.canTransitionToSelf = true;
            lTransition_N660508.orderedInterruption = true;
            lTransition_N660508.interruptionSource = (TransitionInterruptionSource)0;
            for (int i = lTransition_N660508.conditions.Length - 1; i >= 0; i--) { lTransition_N660508.RemoveCondition(lTransition_N660508.conditions[i]); }

            AnimatorStateTransition lTransition_N660510 = MotionControllerMotion.EditorFindTransition(lState_N660474, lState_N660464, 0);
            if (lTransition_N660510 == null) { lTransition_N660510 = lState_N660474.AddTransition(lState_N660464); }
            lTransition_N660510.isExit = false;
            lTransition_N660510.hasExitTime = true;
            lTransition_N660510.hasFixedDuration = true;
            lTransition_N660510.exitTime = 0.8706896f;
            lTransition_N660510.duration = 0.25f;
            lTransition_N660510.offset = 0f;
            lTransition_N660510.mute = false;
            lTransition_N660510.solo = false;
            lTransition_N660510.canTransitionToSelf = true;
            lTransition_N660510.orderedInterruption = true;
            lTransition_N660510.interruptionSource = (TransitionInterruptionSource)0;
            for (int i = lTransition_N660510.conditions.Length - 1; i >= 0; i--) { lTransition_N660510.RemoveCondition(lTransition_N660510.conditions[i]); }

            AnimatorStateTransition lTransition_N660512 = MotionControllerMotion.EditorFindTransition(lState_N660476, lState_N660474, 0);
            if (lTransition_N660512 == null) { lTransition_N660512 = lState_N660476.AddTransition(lState_N660474); }
            lTransition_N660512.isExit = false;
            lTransition_N660512.hasExitTime = true;
            lTransition_N660512.hasFixedDuration = true;
            lTransition_N660512.exitTime = 0.7307953f;
            lTransition_N660512.duration = 0.2499998f;
            lTransition_N660512.offset = 0f;
            lTransition_N660512.mute = false;
            lTransition_N660512.solo = false;
            lTransition_N660512.canTransitionToSelf = true;
            lTransition_N660512.orderedInterruption = true;
            lTransition_N660512.interruptionSource = (TransitionInterruptionSource)0;
            for (int i = lTransition_N660512.conditions.Length - 1; i >= 0; i--) { lTransition_N660512.RemoveCondition(lTransition_N660512.conditions[i]); }

            AnimatorStateTransition lTransition_N660514 = MotionControllerMotion.EditorFindTransition(lState_N660478, lState_N660468, 0);
            if (lTransition_N660514 == null) { lTransition_N660514 = lState_N660478.AddTransition(lState_N660468); }
            lTransition_N660514.isExit = false;
            lTransition_N660514.hasExitTime = true;
            lTransition_N660514.hasFixedDuration = true;
            lTransition_N660514.exitTime = 0.7115384f;
            lTransition_N660514.duration = 0.25f;
            lTransition_N660514.offset = 0f;
            lTransition_N660514.mute = false;
            lTransition_N660514.solo = false;
            lTransition_N660514.canTransitionToSelf = true;
            lTransition_N660514.orderedInterruption = true;
            lTransition_N660514.interruptionSource = (TransitionInterruptionSource)0;
            for (int i = lTransition_N660514.conditions.Length - 1; i >= 0; i--) { lTransition_N660514.RemoveCondition(lTransition_N660514.conditions[i]); }

            return lClimbSSM;
        }

        /// <summary>
        /// Verifies that the standard Climb Ledge SSM has been created
        /// </summary>
        /// <param name="rMotionController"></param>
        /// <param name="rLayerIndex"></param>
        /// <param name="rCreateStates"></param>
        /// <returns></returns>
        public static AnimatorStateMachine EnsureStandardClimbLedge(MotionController rMotionController, int rLayerIndex,
            bool rCreateStates = true)
        {
            AnimatorController lController = GetAnimatorController(rMotionController);
            if (lController == null) return null;

            AnimatorControllerLayer lLayer = lController.layers[rLayerIndex];
            AnimatorStateMachine lLayerStateMachine = lLayer.stateMachine;
            
            AnimatorStateMachine lClimbSSM = MotionControllerMotion.EditorFindSSM(lLayerStateMachine, "ClimbCrouch-SM");
            if (lClimbSSM == null) { lClimbSSM = lLayerStateMachine.AddStateMachine("ClimbCrouch-SM", new Vector3(768, -108, 0)); }

            if (!rCreateStates) return lClimbSSM;

            AnimatorState lState_N662356 = MotionControllerMotion.EditorFindState(lClimbSSM, "ClimbCrouchPose");
            if (lState_N662356 == null) { lState_N662356 = lClimbSSM.AddState("ClimbCrouchPose", new Vector3(468, 96, 0)); }
            lState_N662356.speed = 1f;
            lState_N662356.mirror = false;
            lState_N662356.tag = "";
            lState_N662356.motion = MotionControllerMotion.EditorFindAnimationClip(DefaultPaths.StandardAnimations + "Climbing/ootii_ClimbCrouch.fbx", "ClimbCrouchPose");

            AnimatorState lState_N662358 = MotionControllerMotion.EditorFindState(lClimbSSM, "IdleToClimbCrouch");
            if (lState_N662358 == null) { lState_N662358 = lClimbSSM.AddState("IdleToClimbCrouch", new Vector3(108, -108, 0)); }
            lState_N662358.speed = 1f;
            lState_N662358.mirror = false;
            lState_N662358.tag = "";
            lState_N662358.motion = MotionControllerMotion.EditorFindAnimationClip(DefaultPaths.StandardAnimations + "Climbing/ootii_ClimbCrouch.fbx", "IdleToClimbCrouch");

            AnimatorState lState_N662360 = MotionControllerMotion.EditorFindState(lClimbSSM, "ClimbCrouchToTop");
            if (lState_N662360 == null) { lState_N662360 = lClimbSSM.AddState("ClimbCrouchToTop", new Vector3(468, -72, 0)); }
            lState_N662360.speed = 1f;
            lState_N662360.mirror = false;
            lState_N662360.tag = "";
            lState_N662360.motion = MotionControllerMotion.EditorFindAnimationClip(DefaultPaths.StandardAnimations + "Climbing/ootii_ClimbCrouch.fbx", "ClimbCrouchToIdle");

            AnimatorState lState_N662362 = MotionControllerMotion.EditorFindState(lClimbSSM, "JumpRiseToClimbCrouch");
            if (lState_N662362 == null) { lState_N662362 = lClimbSSM.AddState("JumpRiseToClimbCrouch", new Vector3(108, -36, 0)); }
            lState_N662362.speed = 1f;
            lState_N662362.mirror = false;
            lState_N662362.tag = "";
            lState_N662362.motion = MotionControllerMotion.EditorFindAnimationClip(DefaultPaths.StandardAnimations + "Climbing/ootii_ClimbCrouch.fbx", "JumpRiseToClimbCrouch");

            AnimatorState lState_N662364 = MotionControllerMotion.EditorFindState(lClimbSSM, "JumpFallToClimbCrouch");
            if (lState_N662364 == null) { lState_N662364 = lClimbSSM.AddState("JumpFallToClimbCrouch", new Vector3(108, 108, 0)); }
            lState_N662364.speed = 1f;
            lState_N662364.mirror = false;
            lState_N662364.tag = "";
            lState_N662364.motion = MotionControllerMotion.EditorFindAnimationClip(DefaultPaths.StandardAnimations + "Climbing/ootii_ClimbCrouch.fbx", "JumpFallToClimbCrouch");

            AnimatorState lState_N662366 = MotionControllerMotion.EditorFindState(lClimbSSM, "ClimbCrouchToJumpFall");
            if (lState_N662366 == null) { lState_N662366 = lClimbSSM.AddState("ClimbCrouchToJumpFall", new Vector3(468, 240, 0)); }
            lState_N662366.speed = 1f;
            lState_N662366.mirror = false;
            lState_N662366.tag = "";
            lState_N662366.motion = MotionControllerMotion.EditorFindAnimationClip(DefaultPaths.StandardAnimations + "Climbing/ootii_ClimbCrouch.fbx", "ClimbCrouchToJumpFall");

            AnimatorState lState_N662368 = MotionControllerMotion.EditorFindState(lClimbSSM, "JumpTopToClimbCrouch");
            if (lState_N662368 == null) { lState_N662368 = lClimbSSM.AddState("JumpTopToClimbCrouch", new Vector3(108, 36, 0)); }
            lState_N662368.speed = 1f;
            lState_N662368.mirror = false;
            lState_N662368.tag = "";
            lState_N662368.motion = MotionControllerMotion.EditorFindAnimationClip(DefaultPaths.StandardAnimations + "Climbing/ootii_ClimbCrouch.fbx", "JumpTopToClimbCrouch");

            AnimatorState lState_N662370 = MotionControllerMotion.EditorFindState(lClimbSSM, "ClimbCrouchRecoverIdle");
            if (lState_N662370 == null) { lState_N662370 = lClimbSSM.AddState("ClimbCrouchRecoverIdle", new Vector3(468, -156, 0)); }
            lState_N662370.speed = 1f;
            lState_N662370.mirror = false;
            lState_N662370.tag = "";
            lState_N662370.motion = MotionControllerMotion.EditorFindAnimationClip(DefaultPaths.StandardAnimations + "Climbing/ootii_ClimbCrouch.fbx", "ClimbCrouchRecoverIdle");

            AnimatorState lState_N662372 = MotionControllerMotion.EditorFindState(lClimbSSM, "ClimbCrouchShimmyRight");
            if (lState_N662372 == null) { lState_N662372 = lClimbSSM.AddState("ClimbCrouchShimmyRight", new Vector3(684, 312, 0)); }
            lState_N662372.speed = 1f;
            lState_N662372.mirror = false;
            lState_N662372.tag = "";
            lState_N662372.motion = MotionControllerMotion.EditorFindAnimationClip(DefaultPaths.StandardAnimations + "Climbing/ootii_ClimbCrouchShimmy.fbx", "ShimmyRight");

            AnimatorState lState_N662374 = MotionControllerMotion.EditorFindState(lClimbSSM, "ClimbCrouchShimmyLeft");
            if (lState_N662374 == null) { lState_N662374 = lClimbSSM.AddState("ClimbCrouchShimmyLeft", new Vector3(252, 312, 0)); }
            lState_N662374.speed = 1f;
            lState_N662374.mirror = false;
            lState_N662374.tag = "";
            lState_N662374.motion = MotionControllerMotion.EditorFindAnimationClip(DefaultPaths.StandardAnimations + "Climbing/ootii_ClimbCrouchShimmy.fbx", "ShimmyLeft");

            AnimatorState lState_N662376 = MotionControllerMotion.EditorFindState(lClimbSSM, "IdlePose");
            if (lState_N662376 == null) { lState_N662376 = lClimbSSM.AddState("IdlePose", new Vector3(720, -156, 0)); }
            lState_N662376.speed = 1f;
            lState_N662376.mirror = false;
            lState_N662376.tag = "";
            lState_N662376.motion = MotionControllerMotion.EditorFindAnimationClip(DefaultPaths.StandardAnimations + "Idling/unity_Idle_IdleToIdlesR.fbx", "IdlePose");

            AnimatorStateTransition lAnyTransition_N662378 = MotionControllerMotion.EditorFindAnyStateTransition(lLayerStateMachine, lState_N662358, 0);
            if (lAnyTransition_N662378 == null) { lAnyTransition_N662378 = lLayerStateMachine.AddAnyStateTransition(lState_N662358); }
            lAnyTransition_N662378.isExit = false;
            lAnyTransition_N662378.hasExitTime = false;
            lAnyTransition_N662378.hasFixedDuration = false;
            lAnyTransition_N662378.exitTime = 0.9f;
            lAnyTransition_N662378.duration = 0.1f;
            lAnyTransition_N662378.offset = 0f;
            lAnyTransition_N662378.mute = false;
            lAnyTransition_N662378.solo = false;
            lAnyTransition_N662378.canTransitionToSelf = true;
            lAnyTransition_N662378.orderedInterruption = true;
            lAnyTransition_N662378.interruptionSource = (TransitionInterruptionSource)0;
            for (int i = lAnyTransition_N662378.conditions.Length - 1; i >= 0; i--) { lAnyTransition_N662378.RemoveCondition(lAnyTransition_N662378.conditions[i]); }
            lAnyTransition_N662378.AddCondition(AnimatorConditionMode.Equals, 300f, "L" + rLayerIndex + "MotionPhase");

            AnimatorStateTransition lAnyTransition_N662380 = MotionControllerMotion.EditorFindAnyStateTransition(lLayerStateMachine, lState_N662362, 0);
            if (lAnyTransition_N662380 == null) { lAnyTransition_N662380 = lLayerStateMachine.AddAnyStateTransition(lState_N662362); }
            lAnyTransition_N662380.isExit = false;
            lAnyTransition_N662380.hasExitTime = false;
            lAnyTransition_N662380.hasFixedDuration = false;
            lAnyTransition_N662380.exitTime = 0.9f;
            lAnyTransition_N662380.duration = 0.1f;
            lAnyTransition_N662380.offset = 0f;
            lAnyTransition_N662380.mute = false;
            lAnyTransition_N662380.solo = false;
            lAnyTransition_N662380.canTransitionToSelf = true;
            lAnyTransition_N662380.orderedInterruption = true;
            lAnyTransition_N662380.interruptionSource = (TransitionInterruptionSource)0;
            for (int i = lAnyTransition_N662380.conditions.Length - 1; i >= 0; i--) { lAnyTransition_N662380.RemoveCondition(lAnyTransition_N662380.conditions[i]); }
            lAnyTransition_N662380.AddCondition(AnimatorConditionMode.Equals, 301f, "L" + rLayerIndex + "MotionPhase");

            AnimatorStateTransition lAnyTransition_N662382 = MotionControllerMotion.EditorFindAnyStateTransition(lLayerStateMachine, lState_N662368, 0);
            if (lAnyTransition_N662382 == null) { lAnyTransition_N662382 = lLayerStateMachine.AddAnyStateTransition(lState_N662368); }
            lAnyTransition_N662382.isExit = false;
            lAnyTransition_N662382.hasExitTime = false;
            lAnyTransition_N662382.hasFixedDuration = false;
            lAnyTransition_N662382.exitTime = 0.9f;
            lAnyTransition_N662382.duration = 0.1f;
            lAnyTransition_N662382.offset = 0f;
            lAnyTransition_N662382.mute = false;
            lAnyTransition_N662382.solo = false;
            lAnyTransition_N662382.canTransitionToSelf = true;
            lAnyTransition_N662382.orderedInterruption = true;
            lAnyTransition_N662382.interruptionSource = (TransitionInterruptionSource)0;
            for (int i = lAnyTransition_N662382.conditions.Length - 1; i >= 0; i--) { lAnyTransition_N662382.RemoveCondition(lAnyTransition_N662382.conditions[i]); }
            lAnyTransition_N662382.AddCondition(AnimatorConditionMode.Equals, 302f, "L" + rLayerIndex + "MotionPhase");

            AnimatorStateTransition lAnyTransition_N662384 = MotionControllerMotion.EditorFindAnyStateTransition(lLayerStateMachine, lState_N662364, 0);
            if (lAnyTransition_N662384 == null) { lAnyTransition_N662384 = lLayerStateMachine.AddAnyStateTransition(lState_N662364); }
            lAnyTransition_N662384.isExit = false;
            lAnyTransition_N662384.hasExitTime = false;
            lAnyTransition_N662384.hasFixedDuration = false;
            lAnyTransition_N662384.exitTime = 0.9f;
            lAnyTransition_N662384.duration = 0.1f;
            lAnyTransition_N662384.offset = 0f;
            lAnyTransition_N662384.mute = false;
            lAnyTransition_N662384.solo = false;
            lAnyTransition_N662384.canTransitionToSelf = true;
            lAnyTransition_N662384.orderedInterruption = true;
            lAnyTransition_N662384.interruptionSource = (TransitionInterruptionSource)0;
            for (int i = lAnyTransition_N662384.conditions.Length - 1; i >= 0; i--) { lAnyTransition_N662384.RemoveCondition(lAnyTransition_N662384.conditions[i]); }
            lAnyTransition_N662384.AddCondition(AnimatorConditionMode.Equals, 303f, "L" + rLayerIndex + "MotionPhase");

            AnimatorStateTransition lTransition_N662386 = MotionControllerMotion.EditorFindTransition(lState_N662356, lState_N662360, 0);
            if (lTransition_N662386 == null) { lTransition_N662386 = lState_N662356.AddTransition(lState_N662360); }
            lTransition_N662386.isExit = false;
            lTransition_N662386.hasExitTime = false;
            lTransition_N662386.hasFixedDuration = false;
            lTransition_N662386.exitTime = 0.9f;
            lTransition_N662386.duration = 0f;
            lTransition_N662386.offset = 0f;
            lTransition_N662386.mute = false;
            lTransition_N662386.solo = false;
            lTransition_N662386.canTransitionToSelf = true;
            lTransition_N662386.orderedInterruption = true;
            lTransition_N662386.interruptionSource = (TransitionInterruptionSource)0;
            for (int i = lTransition_N662386.conditions.Length - 1; i >= 0; i--) { lTransition_N662386.RemoveCondition(lTransition_N662386.conditions[i]); }
            lTransition_N662386.AddCondition(AnimatorConditionMode.Equals, 350f, "L" + rLayerIndex + "MotionPhase");

            AnimatorStateTransition lTransition_N662388 = MotionControllerMotion.EditorFindTransition(lState_N662356, lState_N662366, 0);
            if (lTransition_N662388 == null) { lTransition_N662388 = lState_N662356.AddTransition(lState_N662366); }
            lTransition_N662388.isExit = false;
            lTransition_N662388.hasExitTime = false;
            lTransition_N662388.hasFixedDuration = false;
            lTransition_N662388.exitTime = 0.9f;
            lTransition_N662388.duration = 0f;
            lTransition_N662388.offset = 0f;
            lTransition_N662388.mute = false;
            lTransition_N662388.solo = false;
            lTransition_N662388.canTransitionToSelf = true;
            lTransition_N662388.orderedInterruption = true;
            lTransition_N662388.interruptionSource = (TransitionInterruptionSource)0;
            for (int i = lTransition_N662388.conditions.Length - 1; i >= 0; i--) { lTransition_N662388.RemoveCondition(lTransition_N662388.conditions[i]); }
            lTransition_N662388.AddCondition(AnimatorConditionMode.Equals, 370f, "L" + rLayerIndex + "MotionPhase");

            AnimatorStateTransition lTransition_N662390 = MotionControllerMotion.EditorFindTransition(lState_N662356, lState_N662372, 0);
            if (lTransition_N662390 == null) { lTransition_N662390 = lState_N662356.AddTransition(lState_N662372); }
            lTransition_N662390.isExit = false;
            lTransition_N662390.hasExitTime = false;
            lTransition_N662390.hasFixedDuration = false;
            lTransition_N662390.exitTime = 0.9f;
            lTransition_N662390.duration = 1.5f;
            lTransition_N662390.offset = 0f;
            lTransition_N662390.mute = false;
            lTransition_N662390.solo = false;
            lTransition_N662390.canTransitionToSelf = true;
            lTransition_N662390.orderedInterruption = true;
            lTransition_N662390.interruptionSource = (TransitionInterruptionSource)0;
            for (int i = lTransition_N662390.conditions.Length - 1; i >= 0; i--) { lTransition_N662390.RemoveCondition(lTransition_N662390.conditions[i]); }
            lTransition_N662390.AddCondition(AnimatorConditionMode.Equals, 385f, "L" + rLayerIndex + "MotionPhase");

            AnimatorStateTransition lTransition_N662392 = MotionControllerMotion.EditorFindTransition(lState_N662356, lState_N662374, 0);
            if (lTransition_N662392 == null) { lTransition_N662392 = lState_N662356.AddTransition(lState_N662374); }
            lTransition_N662392.isExit = false;
            lTransition_N662392.hasExitTime = false;
            lTransition_N662392.hasFixedDuration = false;
            lTransition_N662392.exitTime = 0.9f;
            lTransition_N662392.duration = 1.5f;
            lTransition_N662392.offset = 0f;
            lTransition_N662392.mute = false;
            lTransition_N662392.solo = false;
            lTransition_N662392.canTransitionToSelf = true;
            lTransition_N662392.orderedInterruption = true;
            lTransition_N662392.interruptionSource = (TransitionInterruptionSource)0;
            for (int i = lTransition_N662392.conditions.Length - 1; i >= 0; i--) { lTransition_N662392.RemoveCondition(lTransition_N662392.conditions[i]); }
            lTransition_N662392.AddCondition(AnimatorConditionMode.Equals, 380f, "L" + rLayerIndex + "MotionPhase");

            AnimatorStateTransition lTransition_N662394 = MotionControllerMotion.EditorFindTransition(lState_N662358, lState_N662360, 0);
            if (lTransition_N662394 == null) { lTransition_N662394 = lState_N662358.AddTransition(lState_N662360); }
            lTransition_N662394.isExit = false;
            lTransition_N662394.hasExitTime = true;
            lTransition_N662394.hasFixedDuration = false;
            lTransition_N662394.exitTime = 1f;
            lTransition_N662394.duration = 0f;
            lTransition_N662394.offset = 0f;
            lTransition_N662394.mute = false;
            lTransition_N662394.solo = false;
            lTransition_N662394.canTransitionToSelf = true;
            lTransition_N662394.orderedInterruption = true;
            lTransition_N662394.interruptionSource = (TransitionInterruptionSource)0;
            for (int i = lTransition_N662394.conditions.Length - 1; i >= 0; i--) { lTransition_N662394.RemoveCondition(lTransition_N662394.conditions[i]); }
            lTransition_N662394.AddCondition(AnimatorConditionMode.Equals, 350f, "L" + rLayerIndex + "MotionPhase");

            AnimatorStateTransition lTransition_N662396 = MotionControllerMotion.EditorFindTransition(lState_N662358, lState_N662356, 0);
            if (lTransition_N662396 == null) { lTransition_N662396 = lState_N662358.AddTransition(lState_N662356); }
            lTransition_N662396.isExit = false;
            lTransition_N662396.hasExitTime = true;
            lTransition_N662396.hasFixedDuration = false;
            lTransition_N662396.exitTime = 1f;
            lTransition_N662396.duration = 0f;
            lTransition_N662396.offset = 0f;
            lTransition_N662396.mute = false;
            lTransition_N662396.solo = false;
            lTransition_N662396.canTransitionToSelf = true;
            lTransition_N662396.orderedInterruption = true;
            lTransition_N662396.interruptionSource = (TransitionInterruptionSource)0;
            for (int i = lTransition_N662396.conditions.Length - 1; i >= 0; i--) { lTransition_N662396.RemoveCondition(lTransition_N662396.conditions[i]); }
            lTransition_N662396.AddCondition(AnimatorConditionMode.Equals, 320f, "L" + rLayerIndex + "MotionPhase");

            AnimatorStateTransition lTransition_N662398 = MotionControllerMotion.EditorFindTransition(lState_N662360, lState_N662370, 0);
            if (lTransition_N662398 == null) { lTransition_N662398 = lState_N662360.AddTransition(lState_N662370); }
            lTransition_N662398.isExit = false;
            lTransition_N662398.hasExitTime = true;
            lTransition_N662398.hasFixedDuration = false;
            lTransition_N662398.exitTime = 0.9253032f;
            lTransition_N662398.duration = 0.07885714f;
            lTransition_N662398.offset = 0f;
            lTransition_N662398.mute = false;
            lTransition_N662398.solo = false;
            lTransition_N662398.canTransitionToSelf = true;
            lTransition_N662398.orderedInterruption = true;
            lTransition_N662398.interruptionSource = (TransitionInterruptionSource)0;
            for (int i = lTransition_N662398.conditions.Length - 1; i >= 0; i--) { lTransition_N662398.RemoveCondition(lTransition_N662398.conditions[i]); }

            AnimatorStateTransition lTransition_N662400 = MotionControllerMotion.EditorFindTransition(lState_N662362, lState_N662356, 0);
            if (lTransition_N662400 == null) { lTransition_N662400 = lState_N662362.AddTransition(lState_N662356); }
            lTransition_N662400.isExit = false;
            lTransition_N662400.hasExitTime = true;
            lTransition_N662400.hasFixedDuration = false;
            lTransition_N662400.exitTime = 0.7965769f;
            lTransition_N662400.duration = 0f;
            lTransition_N662400.offset = 0f;
            lTransition_N662400.mute = false;
            lTransition_N662400.solo = false;
            lTransition_N662400.canTransitionToSelf = true;
            lTransition_N662400.orderedInterruption = true;
            lTransition_N662400.interruptionSource = (TransitionInterruptionSource)0;
            for (int i = lTransition_N662400.conditions.Length - 1; i >= 0; i--) { lTransition_N662400.RemoveCondition(lTransition_N662400.conditions[i]); }
            lTransition_N662400.AddCondition(AnimatorConditionMode.Equals, 320f, "L" + rLayerIndex + "MotionPhase");

            AnimatorStateTransition lTransition_N662402 = MotionControllerMotion.EditorFindTransition(lState_N662362, lState_N662360, 0);
            if (lTransition_N662402 == null) { lTransition_N662402 = lState_N662362.AddTransition(lState_N662360); }
            lTransition_N662402.isExit = false;
            lTransition_N662402.hasExitTime = true;
            lTransition_N662402.hasFixedDuration = false;
            lTransition_N662402.exitTime = 1f;
            lTransition_N662402.duration = 0f;
            lTransition_N662402.offset = 0f;
            lTransition_N662402.mute = false;
            lTransition_N662402.solo = false;
            lTransition_N662402.canTransitionToSelf = true;
            lTransition_N662402.orderedInterruption = true;
            lTransition_N662402.interruptionSource = (TransitionInterruptionSource)0;
            for (int i = lTransition_N662402.conditions.Length - 1; i >= 0; i--) { lTransition_N662402.RemoveCondition(lTransition_N662402.conditions[i]); }
            lTransition_N662402.AddCondition(AnimatorConditionMode.Equals, 350f, "L" + rLayerIndex + "MotionPhase");

            AnimatorStateTransition lTransition_N662404 = MotionControllerMotion.EditorFindTransition(lState_N662364, lState_N662356, 0);
            if (lTransition_N662404 == null) { lTransition_N662404 = lState_N662364.AddTransition(lState_N662356); }
            lTransition_N662404.isExit = false;
            lTransition_N662404.hasExitTime = true;
            lTransition_N662404.hasFixedDuration = false;
            lTransition_N662404.exitTime = 0.8410774f;
            lTransition_N662404.duration = 0f;
            lTransition_N662404.offset = 0f;
            lTransition_N662404.mute = false;
            lTransition_N662404.solo = false;
            lTransition_N662404.canTransitionToSelf = true;
            lTransition_N662404.orderedInterruption = true;
            lTransition_N662404.interruptionSource = (TransitionInterruptionSource)0;
            for (int i = lTransition_N662404.conditions.Length - 1; i >= 0; i--) { lTransition_N662404.RemoveCondition(lTransition_N662404.conditions[i]); }
            lTransition_N662404.AddCondition(AnimatorConditionMode.Equals, 320f, "L" + rLayerIndex + "MotionPhase");

            AnimatorStateTransition lTransition_N662406 = MotionControllerMotion.EditorFindTransition(lState_N662364, lState_N662360, 0);
            if (lTransition_N662406 == null) { lTransition_N662406 = lState_N662364.AddTransition(lState_N662360); }
            lTransition_N662406.isExit = false;
            lTransition_N662406.hasExitTime = true;
            lTransition_N662406.hasFixedDuration = false;
            lTransition_N662406.exitTime = 1f;
            lTransition_N662406.duration = 0f;
            lTransition_N662406.offset = 0f;
            lTransition_N662406.mute = false;
            lTransition_N662406.solo = false;
            lTransition_N662406.canTransitionToSelf = true;
            lTransition_N662406.orderedInterruption = true;
            lTransition_N662406.interruptionSource = (TransitionInterruptionSource)0;
            for (int i = lTransition_N662406.conditions.Length - 1; i >= 0; i--) { lTransition_N662406.RemoveCondition(lTransition_N662406.conditions[i]); }
            lTransition_N662406.AddCondition(AnimatorConditionMode.Equals, 350f, "L" + rLayerIndex + "MotionPhase");

            AnimatorStateTransition lTransition_N662408 = MotionControllerMotion.EditorFindTransition(lState_N662368, lState_N662356, 0);
            if (lTransition_N662408 == null) { lTransition_N662408 = lState_N662368.AddTransition(lState_N662356); }
            lTransition_N662408.isExit = false;
            lTransition_N662408.hasExitTime = true;
            lTransition_N662408.hasFixedDuration = false;
            lTransition_N662408.exitTime = 0.8276094f;
            lTransition_N662408.duration = 0f;
            lTransition_N662408.offset = 0f;
            lTransition_N662408.mute = false;
            lTransition_N662408.solo = false;
            lTransition_N662408.canTransitionToSelf = true;
            lTransition_N662408.orderedInterruption = true;
            lTransition_N662408.interruptionSource = (TransitionInterruptionSource)0;
            for (int i = lTransition_N662408.conditions.Length - 1; i >= 0; i--) { lTransition_N662408.RemoveCondition(lTransition_N662408.conditions[i]); }
            lTransition_N662408.AddCondition(AnimatorConditionMode.Equals, 320f, "L" + rLayerIndex + "MotionPhase");

            AnimatorStateTransition lTransition_N662410 = MotionControllerMotion.EditorFindTransition(lState_N662368, lState_N662360, 0);
            if (lTransition_N662410 == null) { lTransition_N662410 = lState_N662368.AddTransition(lState_N662360); }
            lTransition_N662410.isExit = false;
            lTransition_N662410.hasExitTime = true;
            lTransition_N662410.hasFixedDuration = false;
            lTransition_N662410.exitTime = 1f;
            lTransition_N662410.duration = 0f;
            lTransition_N662410.offset = 0f;
            lTransition_N662410.mute = false;
            lTransition_N662410.solo = false;
            lTransition_N662410.canTransitionToSelf = true;
            lTransition_N662410.orderedInterruption = true;
            lTransition_N662410.interruptionSource = (TransitionInterruptionSource)0;
            for (int i = lTransition_N662410.conditions.Length - 1; i >= 0; i--) { lTransition_N662410.RemoveCondition(lTransition_N662410.conditions[i]); }
            lTransition_N662410.AddCondition(AnimatorConditionMode.Equals, 350f, "L" + rLayerIndex + "MotionPhase");

            AnimatorStateTransition lTransition_N662412 = MotionControllerMotion.EditorFindTransition(lState_N662370, lState_N662376, 0);
            if (lTransition_N662412 == null) { lTransition_N662412 = lState_N662370.AddTransition(lState_N662376); }
            lTransition_N662412.isExit = false;
            lTransition_N662412.hasExitTime = true;
            lTransition_N662412.hasFixedDuration = true;
            lTransition_N662412.exitTime = 0.3750001f;
            lTransition_N662412.duration = 0.25f;
            lTransition_N662412.offset = 0f;
            lTransition_N662412.mute = false;
            lTransition_N662412.solo = false;
            lTransition_N662412.canTransitionToSelf = true;
            lTransition_N662412.orderedInterruption = true;
            lTransition_N662412.interruptionSource = (TransitionInterruptionSource)0;
            for (int i = lTransition_N662412.conditions.Length - 1; i >= 0; i--) { lTransition_N662412.RemoveCondition(lTransition_N662412.conditions[i]); }

            AnimatorStateTransition lTransition_N662414 = MotionControllerMotion.EditorFindTransition(lState_N662372, lState_N662356, 0);
            if (lTransition_N662414 == null) { lTransition_N662414 = lState_N662372.AddTransition(lState_N662356); }
            lTransition_N662414.isExit = false;
            lTransition_N662414.hasExitTime = true;
            lTransition_N662414.hasFixedDuration = false;
            lTransition_N662414.exitTime = 0.9501459f;
            lTransition_N662414.duration = 0.0498541f;
            lTransition_N662414.offset = 0f;
            lTransition_N662414.mute = false;
            lTransition_N662414.solo = false;
            lTransition_N662414.canTransitionToSelf = true;
            lTransition_N662414.orderedInterruption = true;
            lTransition_N662414.interruptionSource = (TransitionInterruptionSource)0;
            for (int i = lTransition_N662414.conditions.Length - 1; i >= 0; i--) { lTransition_N662414.RemoveCondition(lTransition_N662414.conditions[i]); }

            AnimatorStateTransition lTransition_N662416 = MotionControllerMotion.EditorFindTransition(lState_N662372, lState_N662366, 0);
            if (lTransition_N662416 == null) { lTransition_N662416 = lState_N662372.AddTransition(lState_N662366); }
            lTransition_N662416.isExit = false;
            lTransition_N662416.hasExitTime = false;
            lTransition_N662416.hasFixedDuration = false;
            lTransition_N662416.exitTime = 0.9f;
            lTransition_N662416.duration = 0.25f;
            lTransition_N662416.offset = 0f;
            lTransition_N662416.mute = false;
            lTransition_N662416.solo = false;
            lTransition_N662416.canTransitionToSelf = true;
            lTransition_N662416.orderedInterruption = true;
            lTransition_N662416.interruptionSource = (TransitionInterruptionSource)0;
            for (int i = lTransition_N662416.conditions.Length - 1; i >= 0; i--) { lTransition_N662416.RemoveCondition(lTransition_N662416.conditions[i]); }
            lTransition_N662416.AddCondition(AnimatorConditionMode.Equals, 370f, "L" + rLayerIndex + "MotionPhase");

            AnimatorStateTransition lTransition_N662418 = MotionControllerMotion.EditorFindTransition(lState_N662374, lState_N662356, 0);
            if (lTransition_N662418 == null) { lTransition_N662418 = lState_N662374.AddTransition(lState_N662356); }
            lTransition_N662418.isExit = false;
            lTransition_N662418.hasExitTime = true;
            lTransition_N662418.hasFixedDuration = false;
            lTransition_N662418.exitTime = 0.949408f;
            lTransition_N662418.duration = 0.1478843f;
            lTransition_N662418.offset = 0f;
            lTransition_N662418.mute = false;
            lTransition_N662418.solo = false;
            lTransition_N662418.canTransitionToSelf = true;
            lTransition_N662418.orderedInterruption = true;
            lTransition_N662418.interruptionSource = (TransitionInterruptionSource)0;
            for (int i = lTransition_N662418.conditions.Length - 1; i >= 0; i--) { lTransition_N662418.RemoveCondition(lTransition_N662418.conditions[i]); }

            AnimatorStateTransition lTransition_N662420 = MotionControllerMotion.EditorFindTransition(lState_N662374, lState_N662366, 0);
            if (lTransition_N662420 == null) { lTransition_N662420 = lState_N662374.AddTransition(lState_N662366); }
            lTransition_N662420.isExit = false;
            lTransition_N662420.hasExitTime = false;
            lTransition_N662420.hasFixedDuration = false;
            lTransition_N662420.exitTime = 0.9f;
            lTransition_N662420.duration = 0.25f;
            lTransition_N662420.offset = 0f;
            lTransition_N662420.mute = false;
            lTransition_N662420.solo = false;
            lTransition_N662420.canTransitionToSelf = true;
            lTransition_N662420.orderedInterruption = true;
            lTransition_N662420.interruptionSource = (TransitionInterruptionSource)0;
            for (int i = lTransition_N662420.conditions.Length - 1; i >= 0; i--) { lTransition_N662420.RemoveCondition(lTransition_N662420.conditions[i]); }
            lTransition_N662420.AddCondition(AnimatorConditionMode.Equals, 370f, "L" + rLayerIndex + "MotionPhase");

            return lClimbSSM;
        }

        /// <summary>
        /// Verifies that the standard Jump/Fall SSM has been created
        /// </summary>
        /// <param name="rMotionController"></param>
        /// <param name="rLayerIndex"></param>
        /// <param name="rCreateStates"></param>
        /// <returns></returns>
        public static AnimatorStateMachine EnsureStandardJump(MotionController rMotionController, int rLayerIndex,
            bool rCreateStates = true)
        {
            AnimatorController lController = GetAnimatorController(rMotionController);
            if (lController == null) return null;

            AnimatorControllerLayer lLayer = lController.layers[rLayerIndex];
            AnimatorStateMachine lLayerStateMachine = lLayer.stateMachine;

            AnimatorStateMachine lJumpSSM = MotionControllerMotion.EditorFindSSM(lLayerStateMachine, "Jump-SM");
            if (lJumpSSM == null) { lJumpSSM = lLayerStateMachine.AddStateMachine("Jump-SM", new Vector3(336, -288, 0)); }

            if (!rCreateStates) return lJumpSSM;

            AnimatorState lState_N594310 = MotionControllerMotion.EditorFindState(lJumpSSM, "JumpRise");
            if (lState_N594310 == null) { lState_N594310 = lJumpSSM.AddState("JumpRise", new Vector3(-12, 132, 0)); }
            lState_N594310.speed = 1f;
            lState_N594310.mirror = false;
            lState_N594310.tag = "";
            lState_N594310.motion = MotionControllerMotion.EditorFindAnimationClip(DefaultPaths.StandardAnimations + "Jumping/ootii_Jump.fbx", "IdleToRise");

            AnimatorState lState_N594312 = MotionControllerMotion.EditorFindState(lJumpSSM, "JumpLand");
            if (lState_N594312 == null) { lState_N594312 = lJumpSSM.AddState("JumpLand", new Vector3(852, 132, 0)); }
            lState_N594312.speed = 1f;
            lState_N594312.mirror = false;
            lState_N594312.tag = "";
            lState_N594312.motion = MotionControllerMotion.EditorFindAnimationClip(DefaultPaths.StandardAnimations + "Jumping/ootii_Jump.fbx", "FallToLand");

            AnimatorState lState_N594314 = MotionControllerMotion.EditorFindState(lJumpSSM, "JumpRisePose");
            if (lState_N594314 == null) { lState_N594314 = lJumpSSM.AddState("JumpRisePose", new Vector3(132, 12, 0)); }
            lState_N594314.speed = 1f;
            lState_N594314.mirror = false;
            lState_N594314.tag = "";
            lState_N594314.motion = MotionControllerMotion.EditorFindAnimationClip(DefaultPaths.StandardAnimations + "Jumping/ootii_Jump.fbx", "RisePose");

            AnimatorState lState_N594316 = MotionControllerMotion.EditorFindState(lJumpSSM, "JumpFallPose");
            if (lState_N594316 == null) { lState_N594316 = lJumpSSM.AddState("JumpFallPose", new Vector3(660, 0, 0)); }
            lState_N594316.speed = 0.8f;
            lState_N594316.mirror = false;
            lState_N594316.tag = "";
            lState_N594316.motion = MotionControllerMotion.EditorFindAnimationClip(DefaultPaths.StandardAnimations + "Jumping/ootii_Jump.fbx", "Fall");

            AnimatorState lState_N594318 = MotionControllerMotion.EditorFindState(lJumpSSM, "JumpTopToFall");
            if (lState_N594318 == null) { lState_N594318 = lJumpSSM.AddState("JumpTopToFall", new Vector3(552, 132, 0)); }
            lState_N594318.speed = 1f;
            lState_N594318.mirror = false;
            lState_N594318.tag = "";
            lState_N594318.motion = MotionControllerMotion.EditorFindAnimationClip(DefaultPaths.StandardAnimations + "Jumping/ootii_Jump.fbx", "TopToFall");

            AnimatorState lState_N594320 = MotionControllerMotion.EditorFindState(lJumpSSM, "JumpRiseToTop");
            if (lState_N594320 == null) { lState_N594320 = lJumpSSM.AddState("JumpRiseToTop", new Vector3(252, 132, 0)); }
            lState_N594320.speed = 1f;
            lState_N594320.mirror = false;
            lState_N594320.tag = "";
            lState_N594320.motion = MotionControllerMotion.EditorFindAnimationClip(DefaultPaths.StandardAnimations + "Jumping/ootii_Jump.fbx", "RiseToTop");

            AnimatorState lState_N594322 = MotionControllerMotion.EditorFindState(lJumpSSM, "JumpTopPose");
            if (lState_N594322 == null) { lState_N594322 = lJumpSSM.AddState("JumpTopPose", new Vector3(396, 12, 0)); }
            lState_N594322.speed = 1f;
            lState_N594322.mirror = false;
            lState_N594322.tag = "";
            lState_N594322.motion = MotionControllerMotion.EditorFindAnimationClip(DefaultPaths.StandardAnimations + "Jumping/ootii_Jump.fbx", "TopPose");

            AnimatorState lState_N594324 = MotionControllerMotion.EditorFindState(lJumpSSM, "JumpRecoverIdle");
            if (lState_N594324 == null) { lState_N594324 = lJumpSSM.AddState("JumpRecoverIdle", new Vector3(948, -84, 0)); }
            lState_N594324.speed = 1f;
            lState_N594324.mirror = false;
            lState_N594324.tag = "";
            lState_N594324.motion = MotionControllerMotion.EditorFindAnimationClip(DefaultPaths.StandardAnimations + "Jumping/ootii_Jump.fbx", "LandToIdle");

            AnimatorState lState_N594326 = MotionControllerMotion.EditorFindState(lJumpSSM, "JumpRecoverRun");
            if (lState_N594326 == null) { lState_N594326 = lJumpSSM.AddState("JumpRecoverRun", new Vector3(936, 252, 0)); }
            lState_N594326.speed = 1f;
            lState_N594326.mirror = false;
            lState_N594326.tag = "";
            lState_N594326.motion = MotionControllerMotion.EditorFindAnimationClip(DefaultPaths.StandardAnimations + "Jumping/ootii_Jump.fbx", "LandToRun2");

            AnimatorState lState_N594328 = MotionControllerMotion.EditorFindState(lJumpSSM, "IdlePose");
            if (lState_N594328 == null) { lState_N594328 = lJumpSSM.AddState("IdlePose", new Vector3(1176, -84, 0)); }
            lState_N594328.speed = 1f;
            lState_N594328.mirror = false;
            lState_N594328.tag = "";
            lState_N594328.motion = MotionControllerMotion.EditorFindAnimationClip(DefaultPaths.StandardAnimations + "Idling/unity_Idle_IdleToIdlesR.fbx", "IdlePose");

            AnimatorStateTransition lAnyTransition_N594330 = MotionControllerMotion.EditorFindAnyStateTransition(lLayerStateMachine, lState_N594310, 0);
            if (lAnyTransition_N594330 == null) { lAnyTransition_N594330 = lLayerStateMachine.AddAnyStateTransition(lState_N594310); }
            lAnyTransition_N594330.isExit = false;
            lAnyTransition_N594330.hasExitTime = false;
            lAnyTransition_N594330.hasFixedDuration = true;
            lAnyTransition_N594330.exitTime = 0.5f;
            lAnyTransition_N594330.duration = 0.1f;
            lAnyTransition_N594330.offset = 0f;
            lAnyTransition_N594330.mute = false;
            lAnyTransition_N594330.solo = false;
            lAnyTransition_N594330.canTransitionToSelf = true;
            lAnyTransition_N594330.orderedInterruption = true;
            lAnyTransition_N594330.interruptionSource = (TransitionInterruptionSource)0;
            for (int i = lAnyTransition_N594330.conditions.Length - 1; i >= 0; i--) { lAnyTransition_N594330.RemoveCondition(lAnyTransition_N594330.conditions[i]); }
            lAnyTransition_N594330.AddCondition(AnimatorConditionMode.Equals, 251f, "L" + rLayerIndex + "MotionPhase");

            AnimatorStateTransition lAnyTransition_N594332 = MotionControllerMotion.EditorFindAnyStateTransition(lLayerStateMachine, lState_N594316, 0);
            if (lAnyTransition_N594332 == null) { lAnyTransition_N594332 = lLayerStateMachine.AddAnyStateTransition(lState_N594316); }
            lAnyTransition_N594332.isExit = false;
            lAnyTransition_N594332.hasExitTime = false;
            lAnyTransition_N594332.hasFixedDuration = true;
            lAnyTransition_N594332.exitTime = 0.9f;
            lAnyTransition_N594332.duration = 0.2f;
            lAnyTransition_N594332.offset = 0f;
            lAnyTransition_N594332.mute = false;
            lAnyTransition_N594332.solo = false;
            lAnyTransition_N594332.canTransitionToSelf = true;
            lAnyTransition_N594332.orderedInterruption = true;
            lAnyTransition_N594332.interruptionSource = (TransitionInterruptionSource)0;
            for (int i = lAnyTransition_N594332.conditions.Length - 1; i >= 0; i--) { lAnyTransition_N594332.RemoveCondition(lAnyTransition_N594332.conditions[i]); }
            lAnyTransition_N594332.AddCondition(AnimatorConditionMode.Equals, 250f, "L" + rLayerIndex + "MotionPhase");

            AnimatorStateTransition lTransition_N594334 = MotionControllerMotion.EditorFindTransition(lState_N594310, lState_N594320, 0);
            if (lTransition_N594334 == null) { lTransition_N594334 = lState_N594310.AddTransition(lState_N594320); }
            lTransition_N594334.isExit = false;
            lTransition_N594334.hasExitTime = false;
            lTransition_N594334.hasFixedDuration = false;
            lTransition_N594334.exitTime = 0.9427966f;
            lTransition_N594334.duration = 0.07627118f;
            lTransition_N594334.offset = 0f;
            lTransition_N594334.mute = false;
            lTransition_N594334.solo = false;
            lTransition_N594334.canTransitionToSelf = true;
            lTransition_N594334.orderedInterruption = true;
            lTransition_N594334.interruptionSource = (TransitionInterruptionSource)0;
            for (int i = lTransition_N594334.conditions.Length - 1; i >= 0; i--) { lTransition_N594334.RemoveCondition(lTransition_N594334.conditions[i]); }
            lTransition_N594334.AddCondition(AnimatorConditionMode.Equals, 203f, "L" + rLayerIndex + "MotionPhase");

            AnimatorStateTransition lTransition_N594336 = MotionControllerMotion.EditorFindTransition(lState_N594310, lState_N594314, 0);
            if (lTransition_N594336 == null) { lTransition_N594336 = lState_N594310.AddTransition(lState_N594314); }
            lTransition_N594336.isExit = false;
            lTransition_N594336.hasExitTime = true;
            lTransition_N594336.hasFixedDuration = false;
            lTransition_N594336.exitTime = 0.9455966f;
            lTransition_N594336.duration = 0.05858077f;
            lTransition_N594336.offset = 0f;
            lTransition_N594336.mute = false;
            lTransition_N594336.solo = false;
            lTransition_N594336.canTransitionToSelf = true;
            lTransition_N594336.orderedInterruption = true;
            lTransition_N594336.interruptionSource = (TransitionInterruptionSource)0;
            for (int i = lTransition_N594336.conditions.Length - 1; i >= 0; i--) { lTransition_N594336.RemoveCondition(lTransition_N594336.conditions[i]); }

            AnimatorStateTransition lTransition_N594338 = MotionControllerMotion.EditorFindTransition(lState_N594312, lState_N594326, 0);
            if (lTransition_N594338 == null) { lTransition_N594338 = lState_N594312.AddTransition(lState_N594326); }
            lTransition_N594338.isExit = false;
            lTransition_N594338.hasExitTime = true;
            lTransition_N594338.hasFixedDuration = false;
            lTransition_N594338.exitTime = 0.882005f;
            lTransition_N594338.duration = 0.117995f;
            lTransition_N594338.offset = 0f;
            lTransition_N594338.mute = false;
            lTransition_N594338.solo = false;
            lTransition_N594338.canTransitionToSelf = true;
            lTransition_N594338.orderedInterruption = true;
            lTransition_N594338.interruptionSource = (TransitionInterruptionSource)0;
            for (int i = lTransition_N594338.conditions.Length - 1; i >= 0; i--) { lTransition_N594338.RemoveCondition(lTransition_N594338.conditions[i]); }
            lTransition_N594338.AddCondition(AnimatorConditionMode.Equals, 209f, "L" + rLayerIndex + "MotionPhase");

            AnimatorStateTransition lTransition_N594340 = MotionControllerMotion.EditorFindTransition(lState_N594312, lState_N594324, 0);
            if (lTransition_N594340 == null) { lTransition_N594340 = lState_N594312.AddTransition(lState_N594324); }
            lTransition_N594340.isExit = false;
            lTransition_N594340.hasExitTime = true;
            lTransition_N594340.hasFixedDuration = false;
            lTransition_N594340.exitTime = 0.8636364f;
            lTransition_N594340.duration = 0.169278f;
            lTransition_N594340.offset = 0f;
            lTransition_N594340.mute = false;
            lTransition_N594340.solo = false;
            lTransition_N594340.canTransitionToSelf = true;
            lTransition_N594340.orderedInterruption = true;
            lTransition_N594340.interruptionSource = (TransitionInterruptionSource)0;
            for (int i = lTransition_N594340.conditions.Length - 1; i >= 0; i--) { lTransition_N594340.RemoveCondition(lTransition_N594340.conditions[i]); }
            lTransition_N594340.AddCondition(AnimatorConditionMode.Equals, 208f, "L" + rLayerIndex + "MotionPhase");

            AnimatorStateTransition lTransition_N594342 = MotionControllerMotion.EditorFindTransition(lState_N594314, lState_N594320, 0);
            if (lTransition_N594342 == null) { lTransition_N594342 = lState_N594314.AddTransition(lState_N594320); }
            lTransition_N594342.isExit = false;
            lTransition_N594342.hasExitTime = false;
            lTransition_N594342.hasFixedDuration = false;
            lTransition_N594342.exitTime = 0.9f;
            lTransition_N594342.duration = 0f;
            lTransition_N594342.offset = 0f;
            lTransition_N594342.mute = false;
            lTransition_N594342.solo = false;
            lTransition_N594342.canTransitionToSelf = true;
            lTransition_N594342.orderedInterruption = true;
            lTransition_N594342.interruptionSource = (TransitionInterruptionSource)0;
            for (int i = lTransition_N594342.conditions.Length - 1; i >= 0; i--) { lTransition_N594342.RemoveCondition(lTransition_N594342.conditions[i]); }
            lTransition_N594342.AddCondition(AnimatorConditionMode.Equals, 203f, "L" + rLayerIndex + "MotionPhase");

            AnimatorStateTransition lTransition_N594344 = MotionControllerMotion.EditorFindTransition(lState_N594316, lState_N594312, 0);
            if (lTransition_N594344 == null) { lTransition_N594344 = lState_N594316.AddTransition(lState_N594312); }
            lTransition_N594344.isExit = false;
            lTransition_N594344.hasExitTime = false;
            lTransition_N594344.hasFixedDuration = false;
            lTransition_N594344.exitTime = 0.02201979f;
            lTransition_N594344.duration = 0.05033548f;
            lTransition_N594344.offset = 0f;
            lTransition_N594344.mute = false;
            lTransition_N594344.solo = false;
            lTransition_N594344.canTransitionToSelf = true;
            lTransition_N594344.orderedInterruption = true;
            lTransition_N594344.interruptionSource = (TransitionInterruptionSource)0;
            for (int i = lTransition_N594344.conditions.Length - 1; i >= 0; i--) { lTransition_N594344.RemoveCondition(lTransition_N594344.conditions[i]); }
            lTransition_N594344.AddCondition(AnimatorConditionMode.Equals, 207f, "L" + rLayerIndex + "MotionPhase");

            AnimatorStateTransition lTransition_N594346 = MotionControllerMotion.EditorFindTransition(lState_N594318, lState_N594312, 0);
            if (lTransition_N594346 == null) { lTransition_N594346 = lState_N594318.AddTransition(lState_N594312); }
            lTransition_N594346.isExit = false;
            lTransition_N594346.hasExitTime = false;
            lTransition_N594346.hasFixedDuration = false;
            lTransition_N594346.exitTime = 0.9f;
            lTransition_N594346.duration = 0f;
            lTransition_N594346.offset = 0f;
            lTransition_N594346.mute = false;
            lTransition_N594346.solo = false;
            lTransition_N594346.canTransitionToSelf = true;
            lTransition_N594346.orderedInterruption = true;
            lTransition_N594346.interruptionSource = (TransitionInterruptionSource)0;
            for (int i = lTransition_N594346.conditions.Length - 1; i >= 0; i--) { lTransition_N594346.RemoveCondition(lTransition_N594346.conditions[i]); }
            lTransition_N594346.AddCondition(AnimatorConditionMode.Equals, 207f, "L" + rLayerIndex + "MotionPhase");

            AnimatorStateTransition lTransition_N594348 = MotionControllerMotion.EditorFindTransition(lState_N594318, lState_N594316, 0);
            if (lTransition_N594348 == null) { lTransition_N594348 = lState_N594318.AddTransition(lState_N594316); }
            lTransition_N594348.isExit = false;
            lTransition_N594348.hasExitTime = true;
            lTransition_N594348.hasFixedDuration = false;
            lTransition_N594348.exitTime = 0.840604f;
            lTransition_N594348.duration = 0.159396f;
            lTransition_N594348.offset = 0f;
            lTransition_N594348.mute = false;
            lTransition_N594348.solo = false;
            lTransition_N594348.canTransitionToSelf = true;
            lTransition_N594348.orderedInterruption = true;
            lTransition_N594348.interruptionSource = (TransitionInterruptionSource)0;
            for (int i = lTransition_N594348.conditions.Length - 1; i >= 0; i--) { lTransition_N594348.RemoveCondition(lTransition_N594348.conditions[i]); }

            AnimatorStateTransition lTransition_N594350 = MotionControllerMotion.EditorFindTransition(lState_N594318, lState_N594324, 0);
            if (lTransition_N594350 == null) { lTransition_N594350 = lState_N594318.AddTransition(lState_N594324); }
            lTransition_N594350.isExit = false;
            lTransition_N594350.hasExitTime = false;
            lTransition_N594350.hasFixedDuration = false;
            lTransition_N594350.exitTime = 0.9f;
            lTransition_N594350.duration = 0.4982873f;
            lTransition_N594350.offset = 0f;
            lTransition_N594350.mute = false;
            lTransition_N594350.solo = false;
            lTransition_N594350.canTransitionToSelf = true;
            lTransition_N594350.orderedInterruption = true;
            lTransition_N594350.interruptionSource = (TransitionInterruptionSource)0;
            for (int i = lTransition_N594350.conditions.Length - 1; i >= 0; i--) { lTransition_N594350.RemoveCondition(lTransition_N594350.conditions[i]); }
            lTransition_N594350.AddCondition(AnimatorConditionMode.Equals, 208f, "L" + rLayerIndex + "MotionPhase");

            AnimatorStateTransition lTransition_N594352 = MotionControllerMotion.EditorFindTransition(lState_N594318, lState_N594326, 0);
            if (lTransition_N594352 == null) { lTransition_N594352 = lState_N594318.AddTransition(lState_N594326); }
            lTransition_N594352.isExit = false;
            lTransition_N594352.hasExitTime = false;
            lTransition_N594352.hasFixedDuration = false;
            lTransition_N594352.exitTime = 0.9f;
            lTransition_N594352.duration = 0.5029359f;
            lTransition_N594352.offset = 0f;
            lTransition_N594352.mute = false;
            lTransition_N594352.solo = false;
            lTransition_N594352.canTransitionToSelf = true;
            lTransition_N594352.orderedInterruption = true;
            lTransition_N594352.interruptionSource = (TransitionInterruptionSource)0;
            for (int i = lTransition_N594352.conditions.Length - 1; i >= 0; i--) { lTransition_N594352.RemoveCondition(lTransition_N594352.conditions[i]); }
            lTransition_N594352.AddCondition(AnimatorConditionMode.Equals, 209f, "L" + rLayerIndex + "MotionPhase");

            AnimatorStateTransition lTransition_N594354 = MotionControllerMotion.EditorFindTransition(lState_N594320, lState_N594318, 0);
            if (lTransition_N594354 == null) { lTransition_N594354 = lState_N594320.AddTransition(lState_N594318); }
            lTransition_N594354.isExit = false;
            lTransition_N594354.hasExitTime = false;
            lTransition_N594354.hasFixedDuration = false;
            lTransition_N594354.exitTime = 0.903662f;
            lTransition_N594354.duration = 0.1926761f;
            lTransition_N594354.offset = 0f;
            lTransition_N594354.mute = false;
            lTransition_N594354.solo = false;
            lTransition_N594354.canTransitionToSelf = true;
            lTransition_N594354.orderedInterruption = true;
            lTransition_N594354.interruptionSource = (TransitionInterruptionSource)0;
            for (int i = lTransition_N594354.conditions.Length - 1; i >= 0; i--) { lTransition_N594354.RemoveCondition(lTransition_N594354.conditions[i]); }
            lTransition_N594354.AddCondition(AnimatorConditionMode.Equals, 205f, "L" + rLayerIndex + "MotionPhase");

            AnimatorStateTransition lTransition_N594356 = MotionControllerMotion.EditorFindTransition(lState_N594320, lState_N594322, 0);
            if (lTransition_N594356 == null) { lTransition_N594356 = lState_N594320.AddTransition(lState_N594322); }
            lTransition_N594356.isExit = false;
            lTransition_N594356.hasExitTime = true;
            lTransition_N594356.hasFixedDuration = false;
            lTransition_N594356.exitTime = 1f;
            lTransition_N594356.duration = 0f;
            lTransition_N594356.offset = 0f;
            lTransition_N594356.mute = false;
            lTransition_N594356.solo = false;
            lTransition_N594356.canTransitionToSelf = true;
            lTransition_N594356.orderedInterruption = true;
            lTransition_N594356.interruptionSource = (TransitionInterruptionSource)0;
            for (int i = lTransition_N594356.conditions.Length - 1; i >= 0; i--) { lTransition_N594356.RemoveCondition(lTransition_N594356.conditions[i]); }
            lTransition_N594356.AddCondition(AnimatorConditionMode.Equals, 203f, "L" + rLayerIndex + "MotionPhase");

            AnimatorStateTransition lTransition_N594358 = MotionControllerMotion.EditorFindTransition(lState_N594320, lState_N594324, 0);
            if (lTransition_N594358 == null) { lTransition_N594358 = lState_N594320.AddTransition(lState_N594324); }
            lTransition_N594358.isExit = false;
            lTransition_N594358.hasExitTime = true;
            lTransition_N594358.hasFixedDuration = false;
            lTransition_N594358.exitTime = 0.07359297f;
            lTransition_N594358.duration = 1.948052f;
            lTransition_N594358.offset = 0.005134339f;
            lTransition_N594358.mute = false;
            lTransition_N594358.solo = false;
            lTransition_N594358.canTransitionToSelf = true;
            lTransition_N594358.orderedInterruption = true;
            lTransition_N594358.interruptionSource = (TransitionInterruptionSource)0;
            for (int i = lTransition_N594358.conditions.Length - 1; i >= 0; i--) { lTransition_N594358.RemoveCondition(lTransition_N594358.conditions[i]); }
            lTransition_N594358.AddCondition(AnimatorConditionMode.Equals, 208f, "L" + rLayerIndex + "MotionPhase");

            AnimatorStateTransition lTransition_N594360 = MotionControllerMotion.EditorFindTransition(lState_N594320, lState_N594326, 0);
            if (lTransition_N594360 == null) { lTransition_N594360 = lState_N594320.AddTransition(lState_N594326); }
            lTransition_N594360.isExit = false;
            lTransition_N594360.hasExitTime = true;
            lTransition_N594360.hasFixedDuration = false;
            lTransition_N594360.exitTime = 0f;
            lTransition_N594360.duration = 2.5f;
            lTransition_N594360.offset = 0f;
            lTransition_N594360.mute = false;
            lTransition_N594360.solo = false;
            lTransition_N594360.canTransitionToSelf = true;
            lTransition_N594360.orderedInterruption = true;
            lTransition_N594360.interruptionSource = (TransitionInterruptionSource)0;
            for (int i = lTransition_N594360.conditions.Length - 1; i >= 0; i--) { lTransition_N594360.RemoveCondition(lTransition_N594360.conditions[i]); }
            lTransition_N594360.AddCondition(AnimatorConditionMode.Equals, 209f, "L" + rLayerIndex + "MotionPhase");

            AnimatorStateTransition lTransition_N594362 = MotionControllerMotion.EditorFindTransition(lState_N594322, lState_N594318, 0);
            if (lTransition_N594362 == null) { lTransition_N594362 = lState_N594322.AddTransition(lState_N594318); }
            lTransition_N594362.isExit = false;
            lTransition_N594362.hasExitTime = false;
            lTransition_N594362.hasFixedDuration = false;
            lTransition_N594362.exitTime = 0.9f;
            lTransition_N594362.duration = 0.2013423f;
            lTransition_N594362.offset = 0f;
            lTransition_N594362.mute = false;
            lTransition_N594362.solo = false;
            lTransition_N594362.canTransitionToSelf = true;
            lTransition_N594362.orderedInterruption = true;
            lTransition_N594362.interruptionSource = (TransitionInterruptionSource)0;
            for (int i = lTransition_N594362.conditions.Length - 1; i >= 0; i--) { lTransition_N594362.RemoveCondition(lTransition_N594362.conditions[i]); }
            lTransition_N594362.AddCondition(AnimatorConditionMode.Equals, 205f, "L" + rLayerIndex + "MotionPhase");

            AnimatorStateTransition lTransition_N594364 = MotionControllerMotion.EditorFindTransition(lState_N594322, lState_N594324, 0);
            if (lTransition_N594364 == null) { lTransition_N594364 = lState_N594322.AddTransition(lState_N594324); }
            lTransition_N594364.isExit = false;
            lTransition_N594364.hasExitTime = false;
            lTransition_N594364.hasFixedDuration = false;
            lTransition_N594364.exitTime = 0.9f;
            lTransition_N594364.duration = 1.25f;
            lTransition_N594364.offset = 0f;
            lTransition_N594364.mute = false;
            lTransition_N594364.solo = false;
            lTransition_N594364.canTransitionToSelf = true;
            lTransition_N594364.orderedInterruption = true;
            lTransition_N594364.interruptionSource = (TransitionInterruptionSource)0;
            for (int i = lTransition_N594364.conditions.Length - 1; i >= 0; i--) { lTransition_N594364.RemoveCondition(lTransition_N594364.conditions[i]); }
            lTransition_N594364.AddCondition(AnimatorConditionMode.Equals, 208f, "L" + rLayerIndex + "MotionPhase");

            AnimatorStateTransition lTransition_N594366 = MotionControllerMotion.EditorFindTransition(lState_N594324, lState_N594328, 0);
            if (lTransition_N594366 == null) { lTransition_N594366 = lState_N594324.AddTransition(lState_N594328); }
            lTransition_N594366.isExit = false;
            lTransition_N594366.hasExitTime = true;
            lTransition_N594366.hasFixedDuration = true;
            lTransition_N594366.exitTime = 0.8000917f;
            lTransition_N594366.duration = 0.1463132f;
            lTransition_N594366.offset = 0f;
            lTransition_N594366.mute = false;
            lTransition_N594366.solo = false;
            lTransition_N594366.canTransitionToSelf = true;
            lTransition_N594366.orderedInterruption = true;
            lTransition_N594366.interruptionSource = (TransitionInterruptionSource)0;
            for (int i = lTransition_N594366.conditions.Length - 1; i >= 0; i--) { lTransition_N594366.RemoveCondition(lTransition_N594366.conditions[i]); }

            return lJumpSSM;
        }

        /// <summary>
        /// Verifies that the standard Running Jump SSM has been created
        /// </summary>
        /// <param name="rMotionController"></param>
        /// <param name="rLayerIndex"></param>
        /// <param name="rCreateStates"></param>
        /// <returns></returns>
        public static AnimatorStateMachine EnsureStandardRunningJump(MotionController rMotionController, int rLayerIndex)
        {
            AnimatorController lController = GetAnimatorController(rMotionController);
            if (lController == null) return null;

            AnimatorControllerLayer lLayer = lController.layers[rLayerIndex];
            AnimatorStateMachine lLayerStateMachine = lLayer.stateMachine;

            AnimatorStateMachine lJumpSSM =
                MotionControllerMotion.EditorFindSSM(lLayerStateMachine, "RunningJump-SM");
            if (lJumpSSM == null)
            {
                lJumpSSM = lLayerStateMachine.AddStateMachine("RunningJump-SM", new Vector3(552, -288, 0));
            }

            AnimatorState lState_N2496 =
                MotionControllerMotion.EditorFindState(lJumpSSM, "IdlePose");
            if (lState_N2496 == null)
            {
                lState_N2496 = lJumpSSM.AddState("IdlePose", new Vector3(840, 204, 0));
            }

            lState_N2496.speed = 1f;
            lState_N2496.mirror = false;
            lState_N2496.tag = "";
            lState_N2496.motion = MotionControllerMotion.EditorFindAnimationClip(
                "Assets/ootii/Assets/MotionController/Content/Animations/Humanoid/Idling/unity_Idle_IdleToIdlesR.fbx",
                "IdlePose");

            AnimatorState lState_N2498 =
                MotionControllerMotion.EditorFindState(lJumpSSM, "RunJump_RunForward");
            if (lState_N2498 == null)
            {
                lState_N2498 = lJumpSSM.AddState("RunJump_RunForward", new Vector3(588, 288, 0));
            }

            lState_N2498.speed = 1f;
            lState_N2498.mirror = false;
            lState_N2498.tag = "";
            lState_N2498.motion = MotionControllerMotion.EditorFindAnimationClip(
                "Assets/ootii/Assets/MotionController/Content/Animations/Humanoid/Running/RunForward_v2.fbx",
                "RunForward");

            AnimatorState lState_N2500 =
                MotionControllerMotion.EditorFindState(lJumpSSM, "RunningJump");
            if (lState_N2500 == null)
            {
                lState_N2500 = lJumpSSM.AddState("RunningJump", new Vector3(324, 204, 0));
            }

            lState_N2500.speed = 1f;
            lState_N2500.mirror = false;
            lState_N2500.tag = "";
            lState_N2500.motion = MotionControllerMotion.EditorFindAnimationClip(
                "Assets/ootii/Assets/MotionController/Content/Animations/Humanoid/Jumping/ootii_RunningJump.fbx",
                "RunningJump");

            AnimatorState lState_N2502 =
                MotionControllerMotion.EditorFindState(lJumpSSM, "LandToIdle");
            if (lState_N2502 == null)
            {
                lState_N2502 = lJumpSSM.AddState("LandToIdle", new Vector3(588, 204, 0));
            }

            lState_N2502.speed = 1f;
            lState_N2502.mirror = false;
            lState_N2502.tag = "";
            lState_N2502.motion = MotionControllerMotion.EditorFindAnimationClip(
                "Assets/ootii/Assets/MotionController/Content/Animations/Humanoid/Jumping/ootii_Jump.fbx",
                "LandToIdle");

            AnimatorStateTransition lAnyTransition_N2504 =
                MotionControllerMotion.EditorFindAnyStateTransition(lLayerStateMachine, lState_N2500, 0);
            if (lAnyTransition_N2504 == null)
            {
                lAnyTransition_N2504 = lLayerStateMachine.AddAnyStateTransition(lState_N2500);
            }

            lAnyTransition_N2504.isExit = false;
            lAnyTransition_N2504.hasExitTime = false;
            lAnyTransition_N2504.hasFixedDuration = true;
            lAnyTransition_N2504.exitTime = 0.9f;
            lAnyTransition_N2504.duration = 0.05f;
            lAnyTransition_N2504.offset = 0f;
            lAnyTransition_N2504.mute = false;
            lAnyTransition_N2504.solo = false;
            lAnyTransition_N2504.canTransitionToSelf = true;
            lAnyTransition_N2504.orderedInterruption = true;
            lAnyTransition_N2504.interruptionSource = (TransitionInterruptionSource) 0;
            for (int i = lAnyTransition_N2504.conditions.Length - 1; i >= 0; i--)
            {
                lAnyTransition_N2504.RemoveCondition(lAnyTransition_N2504.conditions[i]);
            }

            lAnyTransition_N2504.AddCondition(AnimatorConditionMode.Equals, 27500f,
                "L" + rLayerIndex + "MotionPhase");

            AnimatorStateTransition lTransition_N2506 =
                MotionControllerMotion.EditorFindTransition(lState_N2500, lState_N2498, 0);
            if (lTransition_N2506 == null)
            {
                lTransition_N2506 = lState_N2500.AddTransition(lState_N2498);
            }

            lTransition_N2506.isExit = false;
            lTransition_N2506.hasExitTime = false;
            lTransition_N2506.hasFixedDuration = true;
            lTransition_N2506.exitTime = 0.8318414f;
            lTransition_N2506.duration = 0.1f;
            lTransition_N2506.offset = 0.8475341f;
            lTransition_N2506.mute = false;
            lTransition_N2506.solo = false;
            lTransition_N2506.canTransitionToSelf = true;
            lTransition_N2506.orderedInterruption = true;
            lTransition_N2506.interruptionSource = (TransitionInterruptionSource) 0;
            for (int i = lTransition_N2506.conditions.Length - 1; i >= 0; i--)
            {
                lTransition_N2506.RemoveCondition(lTransition_N2506.conditions[i]);
            }

            lTransition_N2506.AddCondition(AnimatorConditionMode.Equals, 27545f,
                "L" + rLayerIndex + "MotionPhase");

            AnimatorStateTransition lTransition_N2508 =
                MotionControllerMotion.EditorFindTransition(lState_N2500, lState_N2502, 0);
            if (lTransition_N2508 == null)
            {
                lTransition_N2508 = lState_N2500.AddTransition(lState_N2502);
            }

            lTransition_N2508.isExit = false;
            lTransition_N2508.hasExitTime = false;
            lTransition_N2508.hasFixedDuration = true;
            lTransition_N2508.exitTime = 0.8032071f;
            lTransition_N2508.duration = 0.1951104f;
            lTransition_N2508.offset = 0f;
            lTransition_N2508.mute = false;
            lTransition_N2508.solo = false;
            lTransition_N2508.canTransitionToSelf = true;
            lTransition_N2508.orderedInterruption = true;
            lTransition_N2508.interruptionSource = (TransitionInterruptionSource) 0;
            for (int i = lTransition_N2508.conditions.Length - 1; i >= 0; i--)
            {
                lTransition_N2508.RemoveCondition(lTransition_N2508.conditions[i]);
            }

            lTransition_N2508.AddCondition(AnimatorConditionMode.Equals, 27540f,
                "L" + rLayerIndex + "MotionPhase");

            AnimatorStateTransition lTransition_N2510 =
                MotionControllerMotion.EditorFindTransition(lState_N2502, lState_N2496, 0);
            if (lTransition_N2510 == null)
            {
                lTransition_N2510 = lState_N2502.AddTransition(lState_N2496);
            }

            lTransition_N2510.isExit = false;
            lTransition_N2510.hasExitTime = true;
            lTransition_N2510.hasFixedDuration = true;
            lTransition_N2510.exitTime = 0.6590909f;
            lTransition_N2510.duration = 0.25f;
            lTransition_N2510.offset = 0f;
            lTransition_N2510.mute = false;
            lTransition_N2510.solo = false;
            lTransition_N2510.canTransitionToSelf = true;
            lTransition_N2510.orderedInterruption = true;
            lTransition_N2510.interruptionSource = (TransitionInterruptionSource) 0;
            for (int i = lTransition_N2510.conditions.Length - 1; i >= 0; i--)
            {
                lTransition_N2510.RemoveCondition(lTransition_N2510.conditions[i]);
            }

            return lJumpSSM;
        }


        /// <summary>
        /// Verifies that the standard Vault 1m SSM has been created
        /// </summary>
        /// <param name="rMotionController"></param>
        /// <param name="rLayerIndex"></param>
        /// <param name="rCreateStates"></param>
        /// <returns></returns>
        public static AnimatorStateMachine EnsureStandardVault_1m(MotionController rMotionController, int rLayerIndex,
            bool rCreateStates = true)
        {
            AnimatorController lController = GetAnimatorController(rMotionController);
            if (lController == null) return null;

            AnimatorControllerLayer lLayer = lController.layers[rLayerIndex];
            AnimatorStateMachine lLayerStateMachine = lLayer.stateMachine;

            AnimatorStateMachine lVaultSSM = MotionControllerMotion.EditorFindSSM(lLayerStateMachine, "Vault_1m-SM");
            if (lVaultSSM == null) { lVaultSSM = lLayerStateMachine.AddStateMachine("Vault_1m-SM", new Vector3(336, 12, 0)); }

            if (!rCreateStates) return lVaultSSM;

            AnimatorState lState_N663776 = MotionControllerMotion.EditorFindState(lVaultSSM, "WalkVault_1m");
            if (lState_N663776 == null) { lState_N663776 = lVaultSSM.AddState("WalkVault_1m", new Vector3(348, 12, 0)); }
            lState_N663776.speed = 1f;
            lState_N663776.mirror = false;
            lState_N663776.tag = "";
            lState_N663776.motion = MotionControllerMotion.EditorFindAnimationClip(DefaultPaths.StandardAnimations + "Jumping/unity_WalkJump_ToLeft_R_2.fbx", "WalkVault_1m");

            AnimatorState lState_N663778 = MotionControllerMotion.EditorFindState(lVaultSSM, "WalkForward");
            if (lState_N663778 == null) { lState_N663778 = lVaultSSM.AddState("WalkForward", new Vector3(600, 12, 0)); }
            lState_N663778.speed = 1f;
            lState_N663778.mirror = false;
            lState_N663778.tag = "";
            lState_N663778.motion = MotionControllerMotion.EditorFindAnimationClip(DefaultPaths.StandardAnimations + "Walking/unity_WalkFWD.fbx", "WalkForward");

            AnimatorState lState_N663780 = MotionControllerMotion.EditorFindState(lVaultSSM, "RunVault_1m");
            if (lState_N663780 == null) { lState_N663780 = lVaultSSM.AddState("RunVault_1m", new Vector3(348, 96, 0)); }
            lState_N663780.speed = 1f;
            lState_N663780.mirror = false;
            lState_N663780.tag = "";
            lState_N663780.motion = MotionControllerMotion.EditorFindAnimationClip(DefaultPaths.StandardAnimations + "Jumping/unity_RunJump_ToLeft_4.fbx", "RunVault_1m");

            AnimatorState lState_N663782 = MotionControllerMotion.EditorFindState(lVaultSSM, "RunForward");
            if (lState_N663782 == null) { lState_N663782 = lVaultSSM.AddState("RunForward", new Vector3(600, 96, 0)); }
            lState_N663782.speed = 1f;
            lState_N663782.mirror = false;
            lState_N663782.tag = "";
            lState_N663782.motion = MotionControllerMotion.EditorFindAnimationClip(DefaultPaths.StandardAnimations + "Running/unity_JogForward_NtrlFaceFwd.fbx", "RunForward");

            AnimatorStateTransition lAnyTransition_N663784 = MotionControllerMotion.EditorFindAnyStateTransition(lLayerStateMachine, lState_N663776, 0);
            if (lAnyTransition_N663784 == null) { lAnyTransition_N663784 = lLayerStateMachine.AddAnyStateTransition(lState_N663776); }
            lAnyTransition_N663784.isExit = false;
            lAnyTransition_N663784.hasExitTime = false;
            lAnyTransition_N663784.hasFixedDuration = true;
            lAnyTransition_N663784.exitTime = 0.9f;
            lAnyTransition_N663784.duration = 0.1f;
            lAnyTransition_N663784.offset = 0f;
            lAnyTransition_N663784.mute = false;
            lAnyTransition_N663784.solo = false;
            lAnyTransition_N663784.canTransitionToSelf = true;
            lAnyTransition_N663784.orderedInterruption = true;
            lAnyTransition_N663784.interruptionSource = (TransitionInterruptionSource)0;
            for (int i = lAnyTransition_N663784.conditions.Length - 1; i >= 0; i--) { lAnyTransition_N663784.RemoveCondition(lAnyTransition_N663784.conditions[i]); }
            lAnyTransition_N663784.AddCondition(AnimatorConditionMode.Equals, 1300f, "L" + rLayerIndex + "MotionPhase");

            AnimatorStateTransition lAnyTransition_N663786 = MotionControllerMotion.EditorFindAnyStateTransition(lLayerStateMachine, lState_N663780, 0);
            if (lAnyTransition_N663786 == null) { lAnyTransition_N663786 = lLayerStateMachine.AddAnyStateTransition(lState_N663780); }
            lAnyTransition_N663786.isExit = false;
            lAnyTransition_N663786.hasExitTime = false;
            lAnyTransition_N663786.hasFixedDuration = true;
            lAnyTransition_N663786.exitTime = 0.9f;
            lAnyTransition_N663786.duration = 0.1f;
            lAnyTransition_N663786.offset = 0f;
            lAnyTransition_N663786.mute = false;
            lAnyTransition_N663786.solo = false;
            lAnyTransition_N663786.canTransitionToSelf = true;
            lAnyTransition_N663786.orderedInterruption = true;
            lAnyTransition_N663786.interruptionSource = (TransitionInterruptionSource)0;
            for (int i = lAnyTransition_N663786.conditions.Length - 1; i >= 0; i--) { lAnyTransition_N663786.RemoveCondition(lAnyTransition_N663786.conditions[i]); }
            lAnyTransition_N663786.AddCondition(AnimatorConditionMode.Equals, 1305f, "L" + rLayerIndex + "MotionPhase");

            AnimatorStateTransition lTransition_N663788 = MotionControllerMotion.EditorFindTransition(lState_N663776, lState_N663778, 0);
            if (lTransition_N663788 == null) { lTransition_N663788 = lState_N663776.AddTransition(lState_N663778); }
            lTransition_N663788.isExit = false;
            lTransition_N663788.hasExitTime = true;
            lTransition_N663788.hasFixedDuration = true;
            lTransition_N663788.exitTime = 0.7967739f;
            lTransition_N663788.duration = 0.103878f;
            lTransition_N663788.offset = 0.0009236346f;
            lTransition_N663788.mute = false;
            lTransition_N663788.solo = false;
            lTransition_N663788.canTransitionToSelf = true;
            lTransition_N663788.orderedInterruption = true;
            lTransition_N663788.interruptionSource = (TransitionInterruptionSource)0;
            for (int i = lTransition_N663788.conditions.Length - 1; i >= 0; i--) { lTransition_N663788.RemoveCondition(lTransition_N663788.conditions[i]); }

            AnimatorStateTransition lTransition_N663790 = MotionControllerMotion.EditorFindTransition(lState_N663780, lState_N663782, 0);
            if (lTransition_N663790 == null) { lTransition_N663790 = lState_N663780.AddTransition(lState_N663782); }
            lTransition_N663790.isExit = false;
            lTransition_N663790.hasExitTime = true;
            lTransition_N663790.hasFixedDuration = true;
            lTransition_N663790.exitTime = 0.8584905f;
            lTransition_N663790.duration = 0.2499999f;
            lTransition_N663790.offset = 0.4060542f;
            lTransition_N663790.mute = false;
            lTransition_N663790.solo = false;
            lTransition_N663790.canTransitionToSelf = true;
            lTransition_N663790.orderedInterruption = true;
            lTransition_N663790.interruptionSource = (TransitionInterruptionSource)0;
            for (int i = lTransition_N663790.conditions.Length - 1; i >= 0; i--) { lTransition_N663790.RemoveCondition(lTransition_N663790.conditions[i]); }

            return lVaultSSM;
        }


        /// <summary>
        /// Verifies that the standard Balance Walk SSM has been created
        /// </summary>
        /// <param name="rMotionController"></param>
        /// <param name="rLayerIndex"></param>
        /// <param name="rCreateStates"></param>
        /// <returns></returns>
        public static AnimatorStateMachine EnsureStandardBalanceWalk(MotionController rMotionController, int rLayerIndex,
            bool rCreateStates = true)
        {
            AnimatorController lController = GetAnimatorController(rMotionController);
            if (lController == null) return null;
            
            AnimatorControllerLayer lLayer = lController.layers[rLayerIndex];
            AnimatorStateMachine lLayerStateMachine = lLayer.stateMachine;

            AnimatorStateMachine lBalanceSSM = MotionControllerMotion.EditorFindSSM(lLayerStateMachine, "BalanceWalk-SM");
            if (lBalanceSSM == null) { lBalanceSSM = lLayerStateMachine.AddStateMachine("BalanceWalk-SM", new Vector3(336, -228, 0)); }

            if (!rCreateStates) return lBalanceSSM;

            AnimatorState lState_N664830 = MotionControllerMotion.EditorFindState(lBalanceSSM, "BalanceForward");
            if (lState_N664830 == null) { lState_N664830 = lBalanceSSM.AddState("BalanceForward", new Vector3(204, 180, 0)); }
            lState_N664830.speed = 0.6f;
            lState_N664830.mirror = false;
            lState_N664830.tag = "";
            lState_N664830.motion = MotionControllerMotion.EditorFindAnimationClip(DefaultPaths.StandardAnimations + "Navigating/cmu_Balance_89_06.fbx", "BalanceForward");

            AnimatorState lState_N664832 = MotionControllerMotion.EditorFindState(lBalanceSSM, "BalanceBackward");
            if (lState_N664832 == null) { lState_N664832 = lBalanceSSM.AddState("BalanceBackward", new Vector3(480, 180, 0)); }
            lState_N664832.speed = 0.3f;
            lState_N664832.mirror = false;
            lState_N664832.tag = "";
            lState_N664832.motion = MotionControllerMotion.EditorFindAnimationClip(DefaultPaths.StandardAnimations + "Navigating/cmu_Balance_89_06.fbx", "BalanceBackward");

            AnimatorState lState_N664834 = MotionControllerMotion.EditorFindState(lBalanceSSM, "BalanceIdlePose");
            if (lState_N664834 == null) { lState_N664834 = lBalanceSSM.AddState("BalanceIdlePose", new Vector3(336, 72, 0)); }
            lState_N664834.speed = 0.1f;
            lState_N664834.mirror = false;
            lState_N664834.tag = "";
            lState_N664834.motion = MotionControllerMotion.EditorFindAnimationClip(DefaultPaths.StandardAnimations + "Navigating/cmu_Balance_89_06.fbx", "BalanceIdlePose");

            AnimatorState lState_N664836 = MotionControllerMotion.EditorFindState(lBalanceSSM, "IdlePose");
            if (lState_N664836 == null) { lState_N664836 = lBalanceSSM.AddState("IdlePose", new Vector3(192, 372, 0)); }
            lState_N664836.speed = 1f;
            lState_N664836.mirror = false;
            lState_N664836.tag = "";
            lState_N664836.motion = MotionControllerMotion.EditorFindAnimationClip(DefaultPaths.StandardAnimations + "Idling/unity_Idle_IdleToIdlesR.fbx", "IdlePose");

            AnimatorState lState_N664838 = MotionControllerMotion.EditorFindState(lBalanceSSM, "BalanceFallLeft");
            if (lState_N664838 == null) { lState_N664838 = lBalanceSSM.AddState("BalanceFallLeft", new Vector3(492, 372, 0)); }
            lState_N664838.speed = 1f;
            lState_N664838.mirror = false;
            lState_N664838.tag = "";
            lState_N664838.motion = MotionControllerMotion.EditorFindAnimationClip(DefaultPaths.StandardAnimations + "Navigating/cmu_Balance_89_06.fbx", "BalanceFallLeft");

            AnimatorStateTransition lAnyTransition_N664840 = MotionControllerMotion.EditorFindAnyStateTransition(lLayerStateMachine, lState_N664834, 0);
            if (lAnyTransition_N664840 == null) { lAnyTransition_N664840 = lLayerStateMachine.AddAnyStateTransition(lState_N664834); }
            lAnyTransition_N664840.isExit = false;
            lAnyTransition_N664840.hasExitTime = false;
            lAnyTransition_N664840.hasFixedDuration = true;
            lAnyTransition_N664840.exitTime = 0.9f;
            lAnyTransition_N664840.duration = 0.2f;
            lAnyTransition_N664840.offset = 0f;
            lAnyTransition_N664840.mute = false;
            lAnyTransition_N664840.solo = false;
            lAnyTransition_N664840.canTransitionToSelf = true;
            lAnyTransition_N664840.orderedInterruption = true;
            lAnyTransition_N664840.interruptionSource = (TransitionInterruptionSource)0;
            for (int i = lAnyTransition_N664840.conditions.Length - 1; i >= 0; i--) { lAnyTransition_N664840.RemoveCondition(lAnyTransition_N664840.conditions[i]); }
            lAnyTransition_N664840.AddCondition(AnimatorConditionMode.Equals, 1400f, "L" + rLayerIndex + "MotionPhase");

            AnimatorStateTransition lTransition_N664842 = MotionControllerMotion.EditorFindTransition(lState_N664830, lState_N664832, 0);
            if (lTransition_N664842 == null) { lTransition_N664842 = lState_N664830.AddTransition(lState_N664832); }
            lTransition_N664842.isExit = false;
            lTransition_N664842.hasExitTime = false;
            lTransition_N664842.hasFixedDuration = true;
            lTransition_N664842.exitTime = 0.7500005f;
            lTransition_N664842.duration = 0.25f;
            lTransition_N664842.offset = 0f;
            lTransition_N664842.mute = false;
            lTransition_N664842.solo = false;
            lTransition_N664842.canTransitionToSelf = true;
            lTransition_N664842.orderedInterruption = true;
            lTransition_N664842.interruptionSource = (TransitionInterruptionSource)0;
            for (int i = lTransition_N664842.conditions.Length - 1; i >= 0; i--) { lTransition_N664842.RemoveCondition(lTransition_N664842.conditions[i]); }
            lTransition_N664842.AddCondition(AnimatorConditionMode.Greater, 0.1f, "InputMagnitude");
            lTransition_N664842.AddCondition(AnimatorConditionMode.Less, -110f, "InputAngleFromAvatar");

            AnimatorStateTransition lTransition_N664844 = MotionControllerMotion.EditorFindTransition(lState_N664830, lState_N664832, 1);
            if (lTransition_N664844 == null) { lTransition_N664844 = lState_N664830.AddTransition(lState_N664832); }
            lTransition_N664844.isExit = false;
            lTransition_N664844.hasExitTime = false;
            lTransition_N664844.hasFixedDuration = true;
            lTransition_N664844.exitTime = 0.7500005f;
            lTransition_N664844.duration = 0.25f;
            lTransition_N664844.offset = 0f;
            lTransition_N664844.mute = false;
            lTransition_N664844.solo = false;
            lTransition_N664844.canTransitionToSelf = true;
            lTransition_N664844.orderedInterruption = true;
            lTransition_N664844.interruptionSource = (TransitionInterruptionSource)0;
            for (int i = lTransition_N664844.conditions.Length - 1; i >= 0; i--) { lTransition_N664844.RemoveCondition(lTransition_N664844.conditions[i]); }
            lTransition_N664844.AddCondition(AnimatorConditionMode.Greater, 0.1f, "InputMagnitude");
            lTransition_N664844.AddCondition(AnimatorConditionMode.Greater, 110f, "InputAngleFromAvatar");

            AnimatorStateTransition lTransition_N664846 = MotionControllerMotion.EditorFindTransition(lState_N664830, lState_N664834, 0);
            if (lTransition_N664846 == null) { lTransition_N664846 = lState_N664830.AddTransition(lState_N664834); }
            lTransition_N664846.isExit = false;
            lTransition_N664846.hasExitTime = false;
            lTransition_N664846.hasFixedDuration = true;
            lTransition_N664846.exitTime = 0.7500005f;
            lTransition_N664846.duration = 0.25f;
            lTransition_N664846.offset = 0f;
            lTransition_N664846.mute = false;
            lTransition_N664846.solo = false;
            lTransition_N664846.canTransitionToSelf = true;
            lTransition_N664846.orderedInterruption = true;
            lTransition_N664846.interruptionSource = (TransitionInterruptionSource)0;
            for (int i = lTransition_N664846.conditions.Length - 1; i >= 0; i--) { lTransition_N664846.RemoveCondition(lTransition_N664846.conditions[i]); }
            lTransition_N664846.AddCondition(AnimatorConditionMode.Less, 0.1f, "InputMagnitude");

            AnimatorStateTransition lTransition_N664848 = MotionControllerMotion.EditorFindTransition(lState_N664830, lState_N664836, 0);
            if (lTransition_N664848 == null) { lTransition_N664848 = lState_N664830.AddTransition(lState_N664836); }
            lTransition_N664848.isExit = false;
            lTransition_N664848.hasExitTime = true;
            lTransition_N664848.hasFixedDuration = true;
            lTransition_N664848.exitTime = 0.7500005f;
            lTransition_N664848.duration = 0.25f;
            lTransition_N664848.offset = 0f;
            lTransition_N664848.mute = false;
            lTransition_N664848.solo = false;
            lTransition_N664848.canTransitionToSelf = true;
            lTransition_N664848.orderedInterruption = true;
            lTransition_N664848.interruptionSource = (TransitionInterruptionSource)0;
            for (int i = lTransition_N664848.conditions.Length - 1; i >= 0; i--) { lTransition_N664848.RemoveCondition(lTransition_N664848.conditions[i]); }
            lTransition_N664848.AddCondition(AnimatorConditionMode.Equals, 1410f, "L" + rLayerIndex + "MotionPhase");

            AnimatorStateTransition lTransition_N664850 = MotionControllerMotion.EditorFindTransition(lState_N664830, lState_N664838, 0);
            if (lTransition_N664850 == null) { lTransition_N664850 = lState_N664830.AddTransition(lState_N664838); }
            lTransition_N664850.isExit = false;
            lTransition_N664850.hasExitTime = false;
            lTransition_N664850.hasFixedDuration = true;
            lTransition_N664850.exitTime = 0.7272731f;
            lTransition_N664850.duration = 0.25f;
            lTransition_N664850.offset = 0f;
            lTransition_N664850.mute = false;
            lTransition_N664850.solo = false;
            lTransition_N664850.canTransitionToSelf = true;
            lTransition_N664850.orderedInterruption = true;
            lTransition_N664850.interruptionSource = (TransitionInterruptionSource)0;
            for (int i = lTransition_N664850.conditions.Length - 1; i >= 0; i--) { lTransition_N664850.RemoveCondition(lTransition_N664850.conditions[i]); }
            lTransition_N664850.AddCondition(AnimatorConditionMode.Equals, 1405f, "L" + rLayerIndex + "MotionPhase");

            AnimatorStateTransition lTransition_N664852 = MotionControllerMotion.EditorFindTransition(lState_N664832, lState_N664830, 0);
            if (lTransition_N664852 == null) { lTransition_N664852 = lState_N664832.AddTransition(lState_N664830); }
            lTransition_N664852.isExit = false;
            lTransition_N664852.hasExitTime = false;
            lTransition_N664852.hasFixedDuration = true;
            lTransition_N664852.exitTime = 0.7000002f;
            lTransition_N664852.duration = 0.25f;
            lTransition_N664852.offset = 0f;
            lTransition_N664852.mute = false;
            lTransition_N664852.solo = false;
            lTransition_N664852.canTransitionToSelf = true;
            lTransition_N664852.orderedInterruption = true;
            lTransition_N664852.interruptionSource = (TransitionInterruptionSource)0;
            for (int i = lTransition_N664852.conditions.Length - 1; i >= 0; i--) { lTransition_N664852.RemoveCondition(lTransition_N664852.conditions[i]); }
            lTransition_N664852.AddCondition(AnimatorConditionMode.Greater, 0.1f, "InputMagnitude");
            lTransition_N664852.AddCondition(AnimatorConditionMode.Greater, -80f, "InputAngleFromAvatar");
            lTransition_N664852.AddCondition(AnimatorConditionMode.Less, 80f, "InputAngleFromAvatar");

            AnimatorStateTransition lTransition_N664854 = MotionControllerMotion.EditorFindTransition(lState_N664832, lState_N664834, 0);
            if (lTransition_N664854 == null) { lTransition_N664854 = lState_N664832.AddTransition(lState_N664834); }
            lTransition_N664854.isExit = false;
            lTransition_N664854.hasExitTime = false;
            lTransition_N664854.hasFixedDuration = true;
            lTransition_N664854.exitTime = 0.7000002f;
            lTransition_N664854.duration = 0.25f;
            lTransition_N664854.offset = 0f;
            lTransition_N664854.mute = false;
            lTransition_N664854.solo = false;
            lTransition_N664854.canTransitionToSelf = true;
            lTransition_N664854.orderedInterruption = true;
            lTransition_N664854.interruptionSource = (TransitionInterruptionSource)0;
            for (int i = lTransition_N664854.conditions.Length - 1; i >= 0; i--) { lTransition_N664854.RemoveCondition(lTransition_N664854.conditions[i]); }
            lTransition_N664854.AddCondition(AnimatorConditionMode.Less, 0.1f, "InputMagnitude");

            AnimatorStateTransition lTransition_N664856 = MotionControllerMotion.EditorFindTransition(lState_N664832, lState_N664836, 0);
            if (lTransition_N664856 == null) { lTransition_N664856 = lState_N664832.AddTransition(lState_N664836); }
            lTransition_N664856.isExit = false;
            lTransition_N664856.hasExitTime = true;
            lTransition_N664856.hasFixedDuration = true;
            lTransition_N664856.exitTime = 0.7000002f;
            lTransition_N664856.duration = 0.25f;
            lTransition_N664856.offset = 0f;
            lTransition_N664856.mute = false;
            lTransition_N664856.solo = false;
            lTransition_N664856.canTransitionToSelf = true;
            lTransition_N664856.orderedInterruption = true;
            lTransition_N664856.interruptionSource = (TransitionInterruptionSource)0;
            for (int i = lTransition_N664856.conditions.Length - 1; i >= 0; i--) { lTransition_N664856.RemoveCondition(lTransition_N664856.conditions[i]); }
            lTransition_N664856.AddCondition(AnimatorConditionMode.Equals, 1410f, "L" + rLayerIndex + "MotionPhase");

            AnimatorStateTransition lTransition_N664858 = MotionControllerMotion.EditorFindTransition(lState_N664832, lState_N664838, 0);
            if (lTransition_N664858 == null) { lTransition_N664858 = lState_N664832.AddTransition(lState_N664838); }
            lTransition_N664858.isExit = false;
            lTransition_N664858.hasExitTime = false;
            lTransition_N664858.hasFixedDuration = true;
            lTransition_N664858.exitTime = 0.3333367f;
            lTransition_N664858.duration = 0.25f;
            lTransition_N664858.offset = 0f;
            lTransition_N664858.mute = false;
            lTransition_N664858.solo = false;
            lTransition_N664858.canTransitionToSelf = true;
            lTransition_N664858.orderedInterruption = true;
            lTransition_N664858.interruptionSource = (TransitionInterruptionSource)0;
            for (int i = lTransition_N664858.conditions.Length - 1; i >= 0; i--) { lTransition_N664858.RemoveCondition(lTransition_N664858.conditions[i]); }
            lTransition_N664858.AddCondition(AnimatorConditionMode.Equals, 1405f, "L" + rLayerIndex + "MotionPhase");

            AnimatorStateTransition lTransition_N664860 = MotionControllerMotion.EditorFindTransition(lState_N664834, lState_N664830, 0);
            if (lTransition_N664860 == null) { lTransition_N664860 = lState_N664834.AddTransition(lState_N664830); }
            lTransition_N664860.isExit = false;
            lTransition_N664860.hasExitTime = false;
            lTransition_N664860.hasFixedDuration = true;
            lTransition_N664860.exitTime = 7.428772E-10f;
            lTransition_N664860.duration = 0.09999999f;
            lTransition_N664860.offset = 0.5934225f;
            lTransition_N664860.mute = false;
            lTransition_N664860.solo = false;
            lTransition_N664860.canTransitionToSelf = true;
            lTransition_N664860.orderedInterruption = true;
            lTransition_N664860.interruptionSource = (TransitionInterruptionSource)0;
            for (int i = lTransition_N664860.conditions.Length - 1; i >= 0; i--) { lTransition_N664860.RemoveCondition(lTransition_N664860.conditions[i]); }
            lTransition_N664860.AddCondition(AnimatorConditionMode.Greater, 0.1f, "InputMagnitude");
            lTransition_N664860.AddCondition(AnimatorConditionMode.Greater, -80f, "InputAngleFromAvatar");
            lTransition_N664860.AddCondition(AnimatorConditionMode.Less, 80f, "InputAngleFromAvatar");

            AnimatorStateTransition lTransition_N664862 = MotionControllerMotion.EditorFindTransition(lState_N664834, lState_N664832, 0);
            if (lTransition_N664862 == null) { lTransition_N664862 = lState_N664834.AddTransition(lState_N664832); }
            lTransition_N664862.isExit = false;
            lTransition_N664862.hasExitTime = false;
            lTransition_N664862.hasFixedDuration = true;
            lTransition_N664862.exitTime = 8.051802E-10f;
            lTransition_N664862.duration = 0.25f;
            lTransition_N664862.offset = 0.192521f;
            lTransition_N664862.mute = false;
            lTransition_N664862.solo = false;
            lTransition_N664862.canTransitionToSelf = true;
            lTransition_N664862.orderedInterruption = true;
            lTransition_N664862.interruptionSource = (TransitionInterruptionSource)0;
            for (int i = lTransition_N664862.conditions.Length - 1; i >= 0; i--) { lTransition_N664862.RemoveCondition(lTransition_N664862.conditions[i]); }
            lTransition_N664862.AddCondition(AnimatorConditionMode.Greater, 0.1f, "InputMagnitude");
            lTransition_N664862.AddCondition(AnimatorConditionMode.Less, -110f, "InputAngleFromAvatar");

            AnimatorStateTransition lTransition_N664864 = MotionControllerMotion.EditorFindTransition(lState_N664834, lState_N664832, 1);
            if (lTransition_N664864 == null) { lTransition_N664864 = lState_N664834.AddTransition(lState_N664832); }
            lTransition_N664864.isExit = false;
            lTransition_N664864.hasExitTime = false;
            lTransition_N664864.hasFixedDuration = true;
            lTransition_N664864.exitTime = 8.051802E-10f;
            lTransition_N664864.duration = 0.25f;
            lTransition_N664864.offset = 0.1283474f;
            lTransition_N664864.mute = false;
            lTransition_N664864.solo = false;
            lTransition_N664864.canTransitionToSelf = true;
            lTransition_N664864.orderedInterruption = true;
            lTransition_N664864.interruptionSource = (TransitionInterruptionSource)0;
            for (int i = lTransition_N664864.conditions.Length - 1; i >= 0; i--) { lTransition_N664864.RemoveCondition(lTransition_N664864.conditions[i]); }
            lTransition_N664864.AddCondition(AnimatorConditionMode.Greater, 0.1f, "InputMagnitude");
            lTransition_N664864.AddCondition(AnimatorConditionMode.Greater, 110f, "InputAngleFromAvatar");

            AnimatorStateTransition lTransition_N664866 = MotionControllerMotion.EditorFindTransition(lState_N664834, lState_N664836, 0);
            if (lTransition_N664866 == null) { lTransition_N664866 = lState_N664834.AddTransition(lState_N664836); }
            lTransition_N664866.isExit = false;
            lTransition_N664866.hasExitTime = false;
            lTransition_N664866.hasFixedDuration = true;
            lTransition_N664866.exitTime = 0f;
            lTransition_N664866.duration = 0.25f;
            lTransition_N664866.offset = 0f;
            lTransition_N664866.mute = false;
            lTransition_N664866.solo = false;
            lTransition_N664866.canTransitionToSelf = true;
            lTransition_N664866.orderedInterruption = true;
            lTransition_N664866.interruptionSource = (TransitionInterruptionSource)0;
            for (int i = lTransition_N664866.conditions.Length - 1; i >= 0; i--) { lTransition_N664866.RemoveCondition(lTransition_N664866.conditions[i]); }
            lTransition_N664866.AddCondition(AnimatorConditionMode.Equals, 1410f, "L" + rLayerIndex + "MotionPhase");

            AnimatorStateTransition lTransition_N664868 = MotionControllerMotion.EditorFindTransition(lState_N664834, lState_N664838, 0);
            if (lTransition_N664868 == null) { lTransition_N664868 = lState_N664834.AddTransition(lState_N664838); }
            lTransition_N664868.isExit = false;
            lTransition_N664868.hasExitTime = false;
            lTransition_N664868.hasFixedDuration = true;
            lTransition_N664868.exitTime = 0.3333333f;
            lTransition_N664868.duration = 0.25f;
            lTransition_N664868.offset = 0f;
            lTransition_N664868.mute = false;
            lTransition_N664868.solo = false;
            lTransition_N664868.canTransitionToSelf = true;
            lTransition_N664868.orderedInterruption = true;
            lTransition_N664868.interruptionSource = (TransitionInterruptionSource)0;
            for (int i = lTransition_N664868.conditions.Length - 1; i >= 0; i--) { lTransition_N664868.RemoveCondition(lTransition_N664868.conditions[i]); }
            lTransition_N664868.AddCondition(AnimatorConditionMode.Equals, 1405f, "L" + rLayerIndex + "MotionPhase");

            AnimatorStateTransition lTransition_N664870 = MotionControllerMotion.EditorFindTransition(lState_N664836, lState_N664834, 0);
            if (lTransition_N664870 == null) { lTransition_N664870 = lState_N664836.AddTransition(lState_N664834); }
            lTransition_N664870.isExit = false;
            lTransition_N664870.hasExitTime = false;
            lTransition_N664870.hasFixedDuration = true;
            lTransition_N664870.exitTime = 0f;
            lTransition_N664870.duration = 0.25f;
            lTransition_N664870.offset = 0f;
            lTransition_N664870.mute = false;
            lTransition_N664870.solo = false;
            lTransition_N664870.canTransitionToSelf = true;
            lTransition_N664870.orderedInterruption = true;
            lTransition_N664870.interruptionSource = (TransitionInterruptionSource)0;
            for (int i = lTransition_N664870.conditions.Length - 1; i >= 0; i--) { lTransition_N664870.RemoveCondition(lTransition_N664870.conditions[i]); }
            lTransition_N664870.AddCondition(AnimatorConditionMode.Equals, 1400f, "L" + rLayerIndex + "MotionPhase");

            return lBalanceSSM;
        }


        /// <summary>
        /// New way to create sub-state machines without destroying what exists first.
        /// </summary>
        public static AnimatorStateMachine EnsureStandardUtilities(MotionController rMotionController, int rLayerIndex)
        {
            AnimatorController lController = GetAnimatorController(rMotionController);
            if (lController == null) return null;

            AnimatorControllerLayer lLayer = lController.layers[rLayerIndex];

            AnimatorStateMachine lLayerStateMachine = lLayer.stateMachine;

            AnimatorStateMachine lUtilitiesSSM = MotionControllerMotion.EditorFindSSM(lLayerStateMachine, "Utilities-SM");
            if (lUtilitiesSSM == null) { lUtilitiesSSM = lLayerStateMachine.AddStateMachine("Utilities-SM", new Vector3(192, -180, 0)); }

            AnimatorState lState_74042 = MotionControllerMotion.EditorFindState(lUtilitiesSSM, "Idle_PushButton");
            if (lState_74042 == null) { lState_74042 = lUtilitiesSSM.AddState("Idle_PushButton", new Vector3(300, 48, 0)); }
            lState_74042.speed = 1f;
            lState_74042.mirror = false;
            lState_74042.tag = "";
            lState_74042.motion = MotionControllerMotion.EditorFindAnimationClip("Assets/ootii/Assets/MotionController/Content/Animations/Humanoid/Interacting/unity_IdleGrab_Neutral.fbx", "Idle_PushButton");

            AnimatorState lState_75072 = MotionControllerMotion.EditorFindState(lUtilitiesSSM, "IdlePose");
            if (lState_75072 == null) { lState_75072 = lUtilitiesSSM.AddState("IdlePose", new Vector3(792, 156, 0)); }
            lState_75072.speed = 1f;
            lState_75072.mirror = false;
            lState_75072.tag = "";
            lState_75072.motion = MotionControllerMotion.EditorFindAnimationClip("Assets/ootii/Assets/MotionController/Content/Animations/Humanoid/Idling/unity_Idle_IdleToIdlesR.fbx", "IdlePose");

            AnimatorState lState_74044 = MotionControllerMotion.EditorFindState(lUtilitiesSSM, "Idle_PickUp");
            if (lState_74044 == null) { lState_74044 = lUtilitiesSSM.AddState("Idle_PickUp", new Vector3(300, 120, 0)); }
            lState_74044.speed = 1f;
            lState_74044.mirror = false;
            lState_74044.tag = "";
            lState_74044.motion = MotionControllerMotion.EditorFindAnimationClip("Assets/ootii/Assets/MotionController/Content/Animations/Humanoid/Interacting/unity_IdleGrab_LowFront.fbx", "Idle_PickUp");

            AnimatorState lState_75074 = MotionControllerMotion.EditorFindState(lUtilitiesSSM, "Sleeping");
            if (lState_75074 == null) { lState_75074 = lUtilitiesSSM.AddState("Sleeping", new Vector3(540, 552, 0)); }
            lState_75074.speed = 1f;
            lState_75074.mirror = false;
            lState_75074.tag = "";
            lState_75074.motion = MotionControllerMotion.EditorFindAnimationClip("Assets/ootii/Assets/MotionController/Content/Animations/Humanoid/Utilities/Sleeping.fbx", "Sleeping");

            AnimatorState lState_75076 = MotionControllerMotion.EditorFindState(lUtilitiesSSM, "GettingUp");
            if (lState_75076 == null) { lState_75076 = lUtilitiesSSM.AddState("GettingUp", new Vector3(780, 552, 0)); }
            lState_75076.speed = 1.7f;
            lState_75076.mirror = false;
            lState_75076.tag = "";
            lState_75076.motion = MotionControllerMotion.EditorFindAnimationClip("Assets/ootii/Assets/MotionController/Content/Animations/Humanoid/Utilities/Sleeping.fbx", "GettingUp");

            AnimatorState lState_74046 = MotionControllerMotion.EditorFindState(lUtilitiesSSM, "LayingDown");
            if (lState_74046 == null) { lState_74046 = lUtilitiesSSM.AddState("LayingDown", new Vector3(300, 552, 0)); }
            lState_74046.speed = -1.7f;
            lState_74046.mirror = false;
            lState_74046.tag = "";
            lState_74046.motion = MotionControllerMotion.EditorFindAnimationClip("Assets/ootii/Assets/MotionController/Content/Animations/Humanoid/Utilities/Sleeping.fbx", "LayingDown");

            AnimatorState lState_74050 = MotionControllerMotion.EditorFindState(lUtilitiesSSM, "Death_180");
            if (lState_74050 == null) { lState_74050 = lUtilitiesSSM.AddState("Death_180", new Vector3(300, 768, 0)); }
            lState_74050.speed = 1.8f;
            lState_74050.mirror = false;
            lState_74050.tag = "";
            lState_74050.motion = MotionControllerMotion.EditorFindAnimationClip("Assets/ootii/Assets/MotionController/Content/Animations/Humanoid/Utilities/Utilities_01.fbx", "DeathForward");

            AnimatorState lState_74052 = MotionControllerMotion.EditorFindState(lUtilitiesSSM, "Death_0");
            if (lState_74052 == null) { lState_74052 = lUtilitiesSSM.AddState("Death_0", new Vector3(300, 696, 0)); }
            lState_74052.speed = 1.5f;
            lState_74052.mirror = false;
            lState_74052.tag = "";
            lState_74052.motion = MotionControllerMotion.EditorFindAnimationClip("Assets/ootii/Assets/MotionController/Content/Animations/Humanoid/Utilities/Utilities_01.fbx", "DeathBackward");

            AnimatorState lState_74048 = MotionControllerMotion.EditorFindState(lUtilitiesSSM, "Damaged_0");
            if (lState_74048 == null) { lState_74048 = lUtilitiesSSM.AddState("Damaged_0", new Vector3(300, 264, 0)); }
            lState_74048.speed = 3f;
            lState_74048.mirror = false;
            lState_74048.tag = "";
            lState_74048.motion = MotionControllerMotion.EditorFindAnimationClip("Assets/ootii/Assets/MotionController/Content/Animations/Humanoid/Utilities/Utilities_01.fbx", "Damaged");

            AnimatorState lState_74054 = MotionControllerMotion.EditorFindState(lUtilitiesSSM, "Stunned");
            if (lState_74054 == null) { lState_74054 = lUtilitiesSSM.AddState("Stunned", new Vector3(300, 336, 0)); }
            lState_74054.speed = 1f;
            lState_74054.mirror = false;
            lState_74054.tag = "";
            lState_74054.motion = MotionControllerMotion.EditorFindAnimationClip("Assets/ootii/Assets/MotionController/Content/Animations/Humanoid/Utilities/Utilities_01.fbx", "Stunned");

            AnimatorState lState_74056 = MotionControllerMotion.EditorFindState(lUtilitiesSSM, "Cower");
            if (lState_74056 == null) { lState_74056 = lUtilitiesSSM.AddState("Cower", new Vector3(300, 408, 0)); }
            lState_74056.speed = 1f;
            lState_74056.mirror = false;
            lState_74056.tag = "";
            lState_74056.motion = MotionControllerMotion.EditorFindAnimationClip("Assets/ootii/Assets/MotionController/Content/Animations/Humanoid/Utilities/Utilities_01.fbx", "Cower");

            AnimatorState lState_75078 = MotionControllerMotion.EditorFindState(lUtilitiesSSM, "Cower Out");
            if (lState_75078 == null) { lState_75078 = lUtilitiesSSM.AddState("Cower Out", new Vector3(540, 408, 0)); }
            lState_75078.speed = -1f;
            lState_75078.mirror = false;
            lState_75078.tag = "";
            lState_75078.motion = MotionControllerMotion.EditorFindAnimationClip("Assets/ootii/Assets/MotionController/Content/Animations/Humanoid/Utilities/Utilities_01.fbx", "Cower");

            AnimatorState lState_74058 = MotionControllerMotion.EditorFindState(lUtilitiesSSM, "KnockedDown");
            if (lState_74058 == null) { lState_74058 = lUtilitiesSSM.AddState("KnockedDown", new Vector3(300, 480, 0)); }
            lState_74058.speed = 1f;
            lState_74058.mirror = false;
            lState_74058.tag = "";
            lState_74058.motion = MotionControllerMotion.EditorFindAnimationClip("Assets/ootii/Assets/MotionController/Content/Animations/Humanoid/Utilities/Utilities_01.fbx", "KnockedDown");

            AnimatorState lState_75080 = MotionControllerMotion.EditorFindState(lUtilitiesSSM, "GettingUpBackward");
            if (lState_75080 == null) { lState_75080 = lUtilitiesSSM.AddState("GettingUpBackward", new Vector3(540, 480, 0)); }
            lState_75080.speed = 1f;
            lState_75080.mirror = false;
            lState_75080.tag = "";
            lState_75080.motion = MotionControllerMotion.EditorFindAnimationClip("Assets/ootii/Assets/MotionController/Content/Animations/Humanoid/Utilities/Utilities_01.fbx", "GettingUpBackward");

            AnimatorState lState_74060 = MotionControllerMotion.EditorFindState(lUtilitiesSSM, "DeathPose");
            if (lState_74060 == null) { lState_74060 = lUtilitiesSSM.AddState("DeathPose", new Vector3(300, 840, 0)); }
            lState_74060.speed = 1f;
            lState_74060.mirror = false;
            lState_74060.tag = "";
            lState_74060.motion = MotionControllerMotion.EditorFindAnimationClip("Assets/ootii/Assets/MotionController/Content/Animations/Humanoid/Utilities/Utilities_01.fbx", "DeathPose");

            AnimatorState lState_74062 = MotionControllerMotion.EditorFindState(lUtilitiesSSM, "Frozen");
            if (lState_74062 == null) { lState_74062 = lUtilitiesSSM.AddState("Frozen", new Vector3(300, 192, 0)); }
            lState_74062.speed = 1f;
            lState_74062.mirror = false;
            lState_74062.tag = "";
            lState_74062.motion = MotionControllerMotion.EditorFindAnimationClip("Assets/ootii/Assets/MotionController/Content/Animations/Humanoid/Utilities/Utilities_02.fbx", "Frozen");

            AnimatorState lState_74064 = MotionControllerMotion.EditorFindState(lUtilitiesSSM, "PushedBack_Pose");
            if (lState_74064 == null) { lState_74064 = lUtilitiesSSM.AddState("PushedBack_Pose", new Vector3(300, 624, 0)); }
            lState_74064.speed = 1f;
            lState_74064.mirror = false;
            lState_74064.tag = "";
            lState_74064.motion = MotionControllerMotion.EditorFindAnimationClip("Assets/ootii/Assets/MotionController/Content/Animations/Humanoid/Idling/unity_PushedBack.fbx", "PushedBack_Pose");

            AnimatorState lState_75082 = MotionControllerMotion.EditorFindState(lUtilitiesSSM, "PushedBack_Recover");
            if (lState_75082 == null) { lState_75082 = lUtilitiesSSM.AddState("PushedBack_Recover", new Vector3(840, 624, 0)); }
            lState_75082.speed = 2f;
            lState_75082.mirror = false;
            lState_75082.tag = "";
            lState_75082.motion = MotionControllerMotion.EditorFindAnimationClip("Assets/ootii/Assets/MotionController/Content/Animations/Humanoid/Idling/unity_PushedBack.fbx", "PushedBack_Recover");

            AnimatorState lState_75084 = MotionControllerMotion.EditorFindState(lUtilitiesSSM, "PushedBack_Loop");
            if (lState_75084 == null) { lState_75084 = lUtilitiesSSM.AddState("PushedBack_Loop", new Vector3(540, 624, 0)); }
            lState_75084.speed = 0.4f;
            lState_75084.mirror = false;
            lState_75084.tag = "";
            lState_75084.motion = MotionControllerMotion.EditorFindAnimationClip("Assets/ootii/Assets/MotionController/Content/Animations/Humanoid/Idling/unity_PushedBack.fbx", "PushedBack_Loop");

            AnimatorStateTransition lAnyTransition_73796 = MotionControllerMotion.EditorFindAnyStateTransition(lLayerStateMachine, lState_74042, 0);
            if (lAnyTransition_73796 == null) { lAnyTransition_73796 = lLayerStateMachine.AddAnyStateTransition(lState_74042); }
            lAnyTransition_73796.isExit = false;
            lAnyTransition_73796.hasExitTime = false;
            lAnyTransition_73796.hasFixedDuration = true;
            lAnyTransition_73796.exitTime = 0.8999999f;
            lAnyTransition_73796.duration = 0.09999999f;
            lAnyTransition_73796.offset = 0.1753971f;
            lAnyTransition_73796.mute = false;
            lAnyTransition_73796.solo = false;
            lAnyTransition_73796.canTransitionToSelf = true;
            lAnyTransition_73796.orderedInterruption = true;
            lAnyTransition_73796.interruptionSource = (TransitionInterruptionSource)0;
            for (int i = lAnyTransition_73796.conditions.Length - 1; i >= 0; i--) { lAnyTransition_73796.RemoveCondition(lAnyTransition_73796.conditions[i]); }
            lAnyTransition_73796.AddCondition(AnimatorConditionMode.Equals, 2000f, "L" + rLayerIndex + "MotionPhase");

            AnimatorStateTransition lAnyTransition_73798 = MotionControllerMotion.EditorFindAnyStateTransition(lLayerStateMachine, lState_74044, 0);
            if (lAnyTransition_73798 == null) { lAnyTransition_73798 = lLayerStateMachine.AddAnyStateTransition(lState_74044); }
            lAnyTransition_73798.isExit = false;
            lAnyTransition_73798.hasExitTime = false;
            lAnyTransition_73798.hasFixedDuration = true;
            lAnyTransition_73798.exitTime = 0.9f;
            lAnyTransition_73798.duration = 0.1f;
            lAnyTransition_73798.offset = 0f;
            lAnyTransition_73798.mute = false;
            lAnyTransition_73798.solo = false;
            lAnyTransition_73798.canTransitionToSelf = true;
            lAnyTransition_73798.orderedInterruption = true;
            lAnyTransition_73798.interruptionSource = (TransitionInterruptionSource)0;
            for (int i = lAnyTransition_73798.conditions.Length - 1; i >= 0; i--) { lAnyTransition_73798.RemoveCondition(lAnyTransition_73798.conditions[i]); }
            lAnyTransition_73798.AddCondition(AnimatorConditionMode.Equals, 2001f, "L" + rLayerIndex + "MotionPhase");

            AnimatorStateTransition lAnyTransition_73800 = MotionControllerMotion.EditorFindAnyStateTransition(lLayerStateMachine, lState_74046, 0);
            if (lAnyTransition_73800 == null) { lAnyTransition_73800 = lLayerStateMachine.AddAnyStateTransition(lState_74046); }
            lAnyTransition_73800.isExit = false;
            lAnyTransition_73800.hasExitTime = false;
            lAnyTransition_73800.hasFixedDuration = true;
            lAnyTransition_73800.exitTime = 0.8999993f;
            lAnyTransition_73800.duration = 0.3f;
            lAnyTransition_73800.offset = 0.3938867f;
            lAnyTransition_73800.mute = false;
            lAnyTransition_73800.solo = false;
            lAnyTransition_73800.canTransitionToSelf = true;
            lAnyTransition_73800.orderedInterruption = true;
            lAnyTransition_73800.interruptionSource = (TransitionInterruptionSource)0;
            for (int i = lAnyTransition_73800.conditions.Length - 1; i >= 0; i--) { lAnyTransition_73800.RemoveCondition(lAnyTransition_73800.conditions[i]); }
            lAnyTransition_73800.AddCondition(AnimatorConditionMode.Equals, 1820f, "L" + rLayerIndex + "MotionPhase");

            AnimatorStateTransition lAnyTransition_73802 = MotionControllerMotion.EditorFindAnyStateTransition(lLayerStateMachine, lState_74048, 0);
            if (lAnyTransition_73802 == null) { lAnyTransition_73802 = lLayerStateMachine.AddAnyStateTransition(lState_74048); }
            lAnyTransition_73802.isExit = false;
            lAnyTransition_73802.hasExitTime = false;
            lAnyTransition_73802.hasFixedDuration = true;
            lAnyTransition_73802.exitTime = 0.9000001f;
            lAnyTransition_73802.duration = 0.1f;
            lAnyTransition_73802.offset = 0.1943718f;
            lAnyTransition_73802.mute = false;
            lAnyTransition_73802.solo = false;
            lAnyTransition_73802.canTransitionToSelf = true;
            lAnyTransition_73802.orderedInterruption = true;
            lAnyTransition_73802.interruptionSource = (TransitionInterruptionSource)0;
            for (int i = lAnyTransition_73802.conditions.Length - 1; i >= 0; i--) { lAnyTransition_73802.RemoveCondition(lAnyTransition_73802.conditions[i]); }
            lAnyTransition_73802.AddCondition(AnimatorConditionMode.Equals, 1850f, "L" + rLayerIndex + "MotionPhase");

            AnimatorStateTransition lAnyTransition_73804 = MotionControllerMotion.EditorFindAnyStateTransition(lLayerStateMachine, lState_74050, 0);
            if (lAnyTransition_73804 == null) { lAnyTransition_73804 = lLayerStateMachine.AddAnyStateTransition(lState_74050); }
            lAnyTransition_73804.isExit = false;
            lAnyTransition_73804.hasExitTime = false;
            lAnyTransition_73804.hasFixedDuration = true;
            lAnyTransition_73804.exitTime = 0.8999999f;
            lAnyTransition_73804.duration = 0.1f;
            lAnyTransition_73804.offset = 0.06562664f;
            lAnyTransition_73804.mute = false;
            lAnyTransition_73804.solo = false;
            lAnyTransition_73804.canTransitionToSelf = true;
            lAnyTransition_73804.orderedInterruption = true;
            lAnyTransition_73804.interruptionSource = (TransitionInterruptionSource)0;
            for (int i = lAnyTransition_73804.conditions.Length - 1; i >= 0; i--) { lAnyTransition_73804.RemoveCondition(lAnyTransition_73804.conditions[i]); }
            lAnyTransition_73804.AddCondition(AnimatorConditionMode.Equals, 1840f, "L" + rLayerIndex + "MotionPhase");
            lAnyTransition_73804.AddCondition(AnimatorConditionMode.Greater, 100f, "L" + rLayerIndex + "MotionParameter");

            AnimatorStateTransition lAnyTransition_73806 = MotionControllerMotion.EditorFindAnyStateTransition(lLayerStateMachine, lState_74052, 0);
            if (lAnyTransition_73806 == null) { lAnyTransition_73806 = lLayerStateMachine.AddAnyStateTransition(lState_74052); }
            lAnyTransition_73806.isExit = false;
            lAnyTransition_73806.hasExitTime = false;
            lAnyTransition_73806.hasFixedDuration = true;
            lAnyTransition_73806.exitTime = 0.8999998f;
            lAnyTransition_73806.duration = 0.1f;
            lAnyTransition_73806.offset = 0.1486627f;
            lAnyTransition_73806.mute = false;
            lAnyTransition_73806.solo = false;
            lAnyTransition_73806.canTransitionToSelf = true;
            lAnyTransition_73806.orderedInterruption = true;
            lAnyTransition_73806.interruptionSource = (TransitionInterruptionSource)0;
            for (int i = lAnyTransition_73806.conditions.Length - 1; i >= 0; i--) { lAnyTransition_73806.RemoveCondition(lAnyTransition_73806.conditions[i]); }
            lAnyTransition_73806.AddCondition(AnimatorConditionMode.Equals, 1840f, "L" + rLayerIndex + "MotionPhase");
            lAnyTransition_73806.AddCondition(AnimatorConditionMode.Greater, -100f, "L" + rLayerIndex + "MotionParameter");
            lAnyTransition_73806.AddCondition(AnimatorConditionMode.Less, 100f, "L" + rLayerIndex + "MotionParameter");

            AnimatorStateTransition lAnyTransition_73808 = MotionControllerMotion.EditorFindAnyStateTransition(lLayerStateMachine, lState_74054, 0);
            if (lAnyTransition_73808 == null) { lAnyTransition_73808 = lLayerStateMachine.AddAnyStateTransition(lState_74054); }
            lAnyTransition_73808.isExit = false;
            lAnyTransition_73808.hasExitTime = false;
            lAnyTransition_73808.hasFixedDuration = true;
            lAnyTransition_73808.exitTime = 0.9f;
            lAnyTransition_73808.duration = 0.2f;
            lAnyTransition_73808.offset = 0f;
            lAnyTransition_73808.mute = false;
            lAnyTransition_73808.solo = false;
            lAnyTransition_73808.canTransitionToSelf = true;
            lAnyTransition_73808.orderedInterruption = true;
            lAnyTransition_73808.interruptionSource = (TransitionInterruptionSource)0;
            for (int i = lAnyTransition_73808.conditions.Length - 1; i >= 0; i--) { lAnyTransition_73808.RemoveCondition(lAnyTransition_73808.conditions[i]); }
            lAnyTransition_73808.AddCondition(AnimatorConditionMode.Equals, 1870f, "L" + rLayerIndex + "MotionPhase");

            AnimatorStateTransition lAnyTransition_73810 = MotionControllerMotion.EditorFindAnyStateTransition(lLayerStateMachine, lState_74056, 0);
            if (lAnyTransition_73810 == null) { lAnyTransition_73810 = lLayerStateMachine.AddAnyStateTransition(lState_74056); }
            lAnyTransition_73810.isExit = false;
            lAnyTransition_73810.hasExitTime = false;
            lAnyTransition_73810.hasFixedDuration = true;
            lAnyTransition_73810.exitTime = 0.9f;
            lAnyTransition_73810.duration = 0.2f;
            lAnyTransition_73810.offset = 0f;
            lAnyTransition_73810.mute = false;
            lAnyTransition_73810.solo = false;
            lAnyTransition_73810.canTransitionToSelf = true;
            lAnyTransition_73810.orderedInterruption = true;
            lAnyTransition_73810.interruptionSource = (TransitionInterruptionSource)0;
            for (int i = lAnyTransition_73810.conditions.Length - 1; i >= 0; i--) { lAnyTransition_73810.RemoveCondition(lAnyTransition_73810.conditions[i]); }
            lAnyTransition_73810.AddCondition(AnimatorConditionMode.Equals, 1860f, "L" + rLayerIndex + "MotionPhase");

            AnimatorStateTransition lAnyTransition_73812 = MotionControllerMotion.EditorFindAnyStateTransition(lLayerStateMachine, lState_74058, 0);
            if (lAnyTransition_73812 == null) { lAnyTransition_73812 = lLayerStateMachine.AddAnyStateTransition(lState_74058); }
            lAnyTransition_73812.isExit = false;
            lAnyTransition_73812.hasExitTime = false;
            lAnyTransition_73812.hasFixedDuration = true;
            lAnyTransition_73812.exitTime = 0.9f;
            lAnyTransition_73812.duration = 0.2f;
            lAnyTransition_73812.offset = 0.08291358f;
            lAnyTransition_73812.mute = false;
            lAnyTransition_73812.solo = false;
            lAnyTransition_73812.canTransitionToSelf = true;
            lAnyTransition_73812.orderedInterruption = true;
            lAnyTransition_73812.interruptionSource = (TransitionInterruptionSource)0;
            for (int i = lAnyTransition_73812.conditions.Length - 1; i >= 0; i--) { lAnyTransition_73812.RemoveCondition(lAnyTransition_73812.conditions[i]); }
            lAnyTransition_73812.AddCondition(AnimatorConditionMode.Equals, 1880f, "L" + rLayerIndex + "MotionPhase");

            AnimatorStateTransition lAnyTransition_73814 = MotionControllerMotion.EditorFindAnyStateTransition(lLayerStateMachine, lState_74060, 0);
            if (lAnyTransition_73814 == null) { lAnyTransition_73814 = lLayerStateMachine.AddAnyStateTransition(lState_74060); }
            lAnyTransition_73814.isExit = false;
            lAnyTransition_73814.hasExitTime = false;
            lAnyTransition_73814.hasFixedDuration = true;
            lAnyTransition_73814.exitTime = 0.9f;
            lAnyTransition_73814.duration = 0.1f;
            lAnyTransition_73814.offset = 0f;
            lAnyTransition_73814.mute = false;
            lAnyTransition_73814.solo = false;
            lAnyTransition_73814.canTransitionToSelf = true;
            lAnyTransition_73814.orderedInterruption = true;
            lAnyTransition_73814.interruptionSource = (TransitionInterruptionSource)0;
            for (int i = lAnyTransition_73814.conditions.Length - 1; i >= 0; i--) { lAnyTransition_73814.RemoveCondition(lAnyTransition_73814.conditions[i]); }
            lAnyTransition_73814.AddCondition(AnimatorConditionMode.Equals, -99f, "L" + rLayerIndex + "MotionPhase");

            AnimatorStateTransition lAnyTransition_73816 = MotionControllerMotion.EditorFindAnyStateTransition(lLayerStateMachine, lState_74050, 1);
            if (lAnyTransition_73816 == null) { lAnyTransition_73816 = lLayerStateMachine.AddAnyStateTransition(lState_74050); }
            lAnyTransition_73816.isExit = false;
            lAnyTransition_73816.hasExitTime = false;
            lAnyTransition_73816.hasFixedDuration = true;
            lAnyTransition_73816.exitTime = 0.9f;
            lAnyTransition_73816.duration = 0.1f;
            lAnyTransition_73816.offset = 0.06562664f;
            lAnyTransition_73816.mute = false;
            lAnyTransition_73816.solo = false;
            lAnyTransition_73816.canTransitionToSelf = true;
            lAnyTransition_73816.orderedInterruption = true;
            lAnyTransition_73816.interruptionSource = (TransitionInterruptionSource)0;
            for (int i = lAnyTransition_73816.conditions.Length - 1; i >= 0; i--) { lAnyTransition_73816.RemoveCondition(lAnyTransition_73816.conditions[i]); }
            lAnyTransition_73816.AddCondition(AnimatorConditionMode.Equals, 1840f, "L" + rLayerIndex + "MotionPhase");
            lAnyTransition_73816.AddCondition(AnimatorConditionMode.Less, -100f, "L" + rLayerIndex + "MotionParameter");

            AnimatorStateTransition lAnyTransition_73818 = MotionControllerMotion.EditorFindAnyStateTransition(lLayerStateMachine, lState_74062, 0);
            if (lAnyTransition_73818 == null) { lAnyTransition_73818 = lLayerStateMachine.AddAnyStateTransition(lState_74062); }
            lAnyTransition_73818.isExit = false;
            lAnyTransition_73818.hasExitTime = false;
            lAnyTransition_73818.hasFixedDuration = true;
            lAnyTransition_73818.exitTime = 0.9f;
            lAnyTransition_73818.duration = 0.2f;
            lAnyTransition_73818.offset = 0f;
            lAnyTransition_73818.mute = false;
            lAnyTransition_73818.solo = false;
            lAnyTransition_73818.canTransitionToSelf = true;
            lAnyTransition_73818.orderedInterruption = true;
            lAnyTransition_73818.interruptionSource = (TransitionInterruptionSource)0;
            for (int i = lAnyTransition_73818.conditions.Length - 1; i >= 0; i--) { lAnyTransition_73818.RemoveCondition(lAnyTransition_73818.conditions[i]); }
            lAnyTransition_73818.AddCondition(AnimatorConditionMode.Equals, 1890f, "L" + rLayerIndex + "MotionPhase");

            AnimatorStateTransition lAnyTransition_73820 = MotionControllerMotion.EditorFindAnyStateTransition(lLayerStateMachine, lState_74064, 0);
            if (lAnyTransition_73820 == null) { lAnyTransition_73820 = lLayerStateMachine.AddAnyStateTransition(lState_74064); }
            lAnyTransition_73820.isExit = false;
            lAnyTransition_73820.hasExitTime = false;
            lAnyTransition_73820.hasFixedDuration = true;
            lAnyTransition_73820.exitTime = 0.9f;
            lAnyTransition_73820.duration = 0.1f;
            lAnyTransition_73820.offset = 0f;
            lAnyTransition_73820.mute = false;
            lAnyTransition_73820.solo = false;
            lAnyTransition_73820.canTransitionToSelf = true;
            lAnyTransition_73820.orderedInterruption = true;
            lAnyTransition_73820.interruptionSource = (TransitionInterruptionSource)0;
            for (int i = lAnyTransition_73820.conditions.Length - 1; i >= 0; i--) { lAnyTransition_73820.RemoveCondition(lAnyTransition_73820.conditions[i]); }
            lAnyTransition_73820.AddCondition(AnimatorConditionMode.Equals, 1830f, "L" + rLayerIndex + "MotionPhase");

            AnimatorStateTransition lTransition_75086 = MotionControllerMotion.EditorFindTransition(lState_74042, lState_75072, 0);
            if (lTransition_75086 == null) { lTransition_75086 = lState_74042.AddTransition(lState_75072); }
            lTransition_75086.isExit = false;
            lTransition_75086.hasExitTime = true;
            lTransition_75086.hasFixedDuration = true;
            lTransition_75086.exitTime = 0.758442f;
            lTransition_75086.duration = 0.2499998f;
            lTransition_75086.offset = 0f;
            lTransition_75086.mute = false;
            lTransition_75086.solo = false;
            lTransition_75086.canTransitionToSelf = true;
            lTransition_75086.orderedInterruption = true;
            lTransition_75086.interruptionSource = (TransitionInterruptionSource)0;
            for (int i = lTransition_75086.conditions.Length - 1; i >= 0; i--) { lTransition_75086.RemoveCondition(lTransition_75086.conditions[i]); }

            AnimatorStateTransition lTransition_75088 = MotionControllerMotion.EditorFindTransition(lState_74044, lState_75072, 0);
            if (lTransition_75088 == null) { lTransition_75088 = lState_74044.AddTransition(lState_75072); }
            lTransition_75088.isExit = false;
            lTransition_75088.hasExitTime = true;
            lTransition_75088.hasFixedDuration = true;
            lTransition_75088.exitTime = 0.90625f;
            lTransition_75088.duration = 0.25f;
            lTransition_75088.offset = 0f;
            lTransition_75088.mute = false;
            lTransition_75088.solo = false;
            lTransition_75088.canTransitionToSelf = true;
            lTransition_75088.orderedInterruption = true;
            lTransition_75088.interruptionSource = (TransitionInterruptionSource)0;
            for (int i = lTransition_75088.conditions.Length - 1; i >= 0; i--) { lTransition_75088.RemoveCondition(lTransition_75088.conditions[i]); }

            AnimatorStateTransition lTransition_75090 = MotionControllerMotion.EditorFindTransition(lState_75074, lState_75076, 0);
            if (lTransition_75090 == null) { lTransition_75090 = lState_75074.AddTransition(lState_75076); }
            lTransition_75090.isExit = false;
            lTransition_75090.hasExitTime = false;
            lTransition_75090.hasFixedDuration = true;
            lTransition_75090.exitTime = 0.9635922f;
            lTransition_75090.duration = 0.5f;
            lTransition_75090.offset = 0f;
            lTransition_75090.mute = false;
            lTransition_75090.solo = false;
            lTransition_75090.canTransitionToSelf = true;
            lTransition_75090.orderedInterruption = true;
            lTransition_75090.interruptionSource = (TransitionInterruptionSource)0;
            for (int i = lTransition_75090.conditions.Length - 1; i >= 0; i--) { lTransition_75090.RemoveCondition(lTransition_75090.conditions[i]); }
            lTransition_75090.AddCondition(AnimatorConditionMode.Equals, 1825f, "L" + rLayerIndex + "MotionPhase");

            AnimatorStateTransition lTransition_75092 = MotionControllerMotion.EditorFindTransition(lState_75076, lState_75072, 0);
            if (lTransition_75092 == null) { lTransition_75092 = lState_75076.AddTransition(lState_75072); }
            lTransition_75092.isExit = false;
            lTransition_75092.hasExitTime = true;
            lTransition_75092.hasFixedDuration = true;
            lTransition_75092.exitTime = 0.5882814f;
            lTransition_75092.duration = 0.25f;
            lTransition_75092.offset = 0f;
            lTransition_75092.mute = false;
            lTransition_75092.solo = false;
            lTransition_75092.canTransitionToSelf = true;
            lTransition_75092.orderedInterruption = true;
            lTransition_75092.interruptionSource = (TransitionInterruptionSource)0;
            for (int i = lTransition_75092.conditions.Length - 1; i >= 0; i--) { lTransition_75092.RemoveCondition(lTransition_75092.conditions[i]); }

            AnimatorStateTransition lTransition_75094 = MotionControllerMotion.EditorFindTransition(lState_74046, lState_75074, 0);
            if (lTransition_75094 == null) { lTransition_75094 = lState_74046.AddTransition(lState_75074); }
            lTransition_75094.isExit = false;
            lTransition_75094.hasExitTime = true;
            lTransition_75094.hasFixedDuration = true;
            lTransition_75094.exitTime = 0.9f;
            lTransition_75094.duration = 0.5f;
            lTransition_75094.offset = 0f;
            lTransition_75094.mute = false;
            lTransition_75094.solo = false;
            lTransition_75094.canTransitionToSelf = true;
            lTransition_75094.orderedInterruption = true;
            lTransition_75094.interruptionSource = (TransitionInterruptionSource)0;
            for (int i = lTransition_75094.conditions.Length - 1; i >= 0; i--) { lTransition_75094.RemoveCondition(lTransition_75094.conditions[i]); }

            AnimatorStateTransition lTransition_75096 = MotionControllerMotion.EditorFindTransition(lState_74048, lState_75072, 0);
            if (lTransition_75096 == null) { lTransition_75096 = lState_74048.AddTransition(lState_75072); }
            lTransition_75096.isExit = false;
            lTransition_75096.hasExitTime = true;
            lTransition_75096.hasFixedDuration = true;
            lTransition_75096.exitTime = 0.8578009f;
            lTransition_75096.duration = 0.2500002f;
            lTransition_75096.offset = 27.37617f;
            lTransition_75096.mute = false;
            lTransition_75096.solo = false;
            lTransition_75096.canTransitionToSelf = true;
            lTransition_75096.orderedInterruption = true;
            lTransition_75096.interruptionSource = (TransitionInterruptionSource)0;
            for (int i = lTransition_75096.conditions.Length - 1; i >= 0; i--) { lTransition_75096.RemoveCondition(lTransition_75096.conditions[i]); }

            AnimatorStateTransition lTransition_75098 = MotionControllerMotion.EditorFindTransition(lState_74054, lState_75072, 0);
            if (lTransition_75098 == null) { lTransition_75098 = lState_74054.AddTransition(lState_75072); }
            lTransition_75098.isExit = false;
            lTransition_75098.hasExitTime = false;
            lTransition_75098.hasFixedDuration = true;
            lTransition_75098.exitTime = 0.97f;
            lTransition_75098.duration = 0.25f;
            lTransition_75098.offset = 0f;
            lTransition_75098.mute = false;
            lTransition_75098.solo = false;
            lTransition_75098.canTransitionToSelf = true;
            lTransition_75098.orderedInterruption = true;
            lTransition_75098.interruptionSource = (TransitionInterruptionSource)0;
            for (int i = lTransition_75098.conditions.Length - 1; i >= 0; i--) { lTransition_75098.RemoveCondition(lTransition_75098.conditions[i]); }
            lTransition_75098.AddCondition(AnimatorConditionMode.Equals, 1875f, "L" + rLayerIndex + "MotionPhase");

            AnimatorStateTransition lTransition_75100 = MotionControllerMotion.EditorFindTransition(lState_74056, lState_75078, 0);
            if (lTransition_75100 == null) { lTransition_75100 = lState_74056.AddTransition(lState_75078); }
            lTransition_75100.isExit = false;
            lTransition_75100.hasExitTime = false;
            lTransition_75100.hasFixedDuration = true;
            lTransition_75100.exitTime = 1f;
            lTransition_75100.duration = 0f;
            lTransition_75100.offset = 0f;
            lTransition_75100.mute = false;
            lTransition_75100.solo = false;
            lTransition_75100.canTransitionToSelf = true;
            lTransition_75100.orderedInterruption = true;
            lTransition_75100.interruptionSource = (TransitionInterruptionSource)0;
            for (int i = lTransition_75100.conditions.Length - 1; i >= 0; i--) { lTransition_75100.RemoveCondition(lTransition_75100.conditions[i]); }
            lTransition_75100.AddCondition(AnimatorConditionMode.Equals, 1865f, "L" + rLayerIndex + "MotionPhase");

            AnimatorStateTransition lTransition_75102 = MotionControllerMotion.EditorFindTransition(lState_75078, lState_75072, 0);
            if (lTransition_75102 == null) { lTransition_75102 = lState_75078.AddTransition(lState_75072); }
            lTransition_75102.isExit = false;
            lTransition_75102.hasExitTime = true;
            lTransition_75102.hasFixedDuration = true;
            lTransition_75102.exitTime = 0.8333334f;
            lTransition_75102.duration = 0.25f;
            lTransition_75102.offset = 0f;
            lTransition_75102.mute = false;
            lTransition_75102.solo = false;
            lTransition_75102.canTransitionToSelf = true;
            lTransition_75102.orderedInterruption = true;
            lTransition_75102.interruptionSource = (TransitionInterruptionSource)0;
            for (int i = lTransition_75102.conditions.Length - 1; i >= 0; i--) { lTransition_75102.RemoveCondition(lTransition_75102.conditions[i]); }

            AnimatorStateTransition lTransition_75104 = MotionControllerMotion.EditorFindTransition(lState_74058, lState_75080, 0);
            if (lTransition_75104 == null) { lTransition_75104 = lState_74058.AddTransition(lState_75080); }
            lTransition_75104.isExit = false;
            lTransition_75104.hasExitTime = false;
            lTransition_75104.hasFixedDuration = true;
            lTransition_75104.exitTime = 0.8863636f;
            lTransition_75104.duration = 0.25f;
            lTransition_75104.offset = 0f;
            lTransition_75104.mute = false;
            lTransition_75104.solo = false;
            lTransition_75104.canTransitionToSelf = true;
            lTransition_75104.orderedInterruption = true;
            lTransition_75104.interruptionSource = (TransitionInterruptionSource)0;
            for (int i = lTransition_75104.conditions.Length - 1; i >= 0; i--) { lTransition_75104.RemoveCondition(lTransition_75104.conditions[i]); }
            lTransition_75104.AddCondition(AnimatorConditionMode.Equals, 1885f, "L" + rLayerIndex + "MotionPhase");

            AnimatorStateTransition lTransition_75106 = MotionControllerMotion.EditorFindTransition(lState_75080, lState_75072, 0);
            if (lTransition_75106 == null) { lTransition_75106 = lState_75080.AddTransition(lState_75072); }
            lTransition_75106.isExit = false;
            lTransition_75106.hasExitTime = true;
            lTransition_75106.hasFixedDuration = true;
            lTransition_75106.exitTime = 0.6343459f;
            lTransition_75106.duration = 0.2500002f;
            lTransition_75106.offset = 0f;
            lTransition_75106.mute = false;
            lTransition_75106.solo = false;
            lTransition_75106.canTransitionToSelf = true;
            lTransition_75106.orderedInterruption = true;
            lTransition_75106.interruptionSource = (TransitionInterruptionSource)0;
            for (int i = lTransition_75106.conditions.Length - 1; i >= 0; i--) { lTransition_75106.RemoveCondition(lTransition_75106.conditions[i]); }

            AnimatorStateTransition lTransition_75108 = MotionControllerMotion.EditorFindTransition(lState_74062, lState_75072, 0);
            if (lTransition_75108 == null) { lTransition_75108 = lState_74062.AddTransition(lState_75072); }
            lTransition_75108.isExit = false;
            lTransition_75108.hasExitTime = false;
            lTransition_75108.hasFixedDuration = true;
            lTransition_75108.exitTime = 0f;
            lTransition_75108.duration = 0.25f;
            lTransition_75108.offset = 0f;
            lTransition_75108.mute = false;
            lTransition_75108.solo = false;
            lTransition_75108.canTransitionToSelf = true;
            lTransition_75108.orderedInterruption = true;
            lTransition_75108.interruptionSource = (TransitionInterruptionSource)0;
            for (int i = lTransition_75108.conditions.Length - 1; i >= 0; i--) { lTransition_75108.RemoveCondition(lTransition_75108.conditions[i]); }
            lTransition_75108.AddCondition(AnimatorConditionMode.Equals, 1895f, "L" + rLayerIndex + "MotionPhase");

            AnimatorStateTransition lTransition_75110 = MotionControllerMotion.EditorFindTransition(lState_74064, lState_75084, 0);
            if (lTransition_75110 == null) { lTransition_75110 = lState_74064.AddTransition(lState_75084); }
            lTransition_75110.isExit = false;
            lTransition_75110.hasExitTime = true;
            lTransition_75110.hasFixedDuration = true;
            lTransition_75110.exitTime = 0.95f;
            lTransition_75110.duration = 0.1f;
            lTransition_75110.offset = 0f;
            lTransition_75110.mute = false;
            lTransition_75110.solo = false;
            lTransition_75110.canTransitionToSelf = true;
            lTransition_75110.orderedInterruption = true;
            lTransition_75110.interruptionSource = (TransitionInterruptionSource)0;
            for (int i = lTransition_75110.conditions.Length - 1; i >= 0; i--) { lTransition_75110.RemoveCondition(lTransition_75110.conditions[i]); }

            AnimatorStateTransition lTransition_75112 = MotionControllerMotion.EditorFindTransition(lState_75082, lState_75072, 0);
            if (lTransition_75112 == null) { lTransition_75112 = lState_75082.AddTransition(lState_75072); }
            lTransition_75112.isExit = false;
            lTransition_75112.hasExitTime = true;
            lTransition_75112.hasFixedDuration = true;
            lTransition_75112.exitTime = 0.6834157f;
            lTransition_75112.duration = 0.25f;
            lTransition_75112.offset = 0f;
            lTransition_75112.mute = false;
            lTransition_75112.solo = false;
            lTransition_75112.canTransitionToSelf = true;
            lTransition_75112.orderedInterruption = true;
            lTransition_75112.interruptionSource = (TransitionInterruptionSource)0;
            for (int i = lTransition_75112.conditions.Length - 1; i >= 0; i--) { lTransition_75112.RemoveCondition(lTransition_75112.conditions[i]); }

            AnimatorStateTransition lTransition_75114 = MotionControllerMotion.EditorFindTransition(lState_75084, lState_75082, 0);
            if (lTransition_75114 == null) { lTransition_75114 = lState_75084.AddTransition(lState_75082); }
            lTransition_75114.isExit = false;
            lTransition_75114.hasExitTime = true;
            lTransition_75114.hasFixedDuration = true;
            lTransition_75114.exitTime = 0.06250077f;
            lTransition_75114.duration = 0.25f;
            lTransition_75114.offset = 0f;
            lTransition_75114.mute = false;
            lTransition_75114.solo = false;
            lTransition_75114.canTransitionToSelf = true;
            lTransition_75114.orderedInterruption = true;
            lTransition_75114.interruptionSource = (TransitionInterruptionSource)0;
            for (int i = lTransition_75114.conditions.Length - 1; i >= 0; i--) { lTransition_75114.RemoveCondition(lTransition_75114.conditions[i]); }
            lTransition_75114.AddCondition(AnimatorConditionMode.Equals, 1835f, "L" + rLayerIndex + "MotionPhase");

            return lUtilitiesSSM;
        }


        #endregion Verify Standard Motion Animator States

#endif
    }
}
