using System;
using System.Collections.Generic;
using com.ootii.Actors;
using com.ootii.Actors.AnimationControllers;
using com.ootii.Actors.Attributes;
using com.ootii.Actors.Combat;
using com.ootii.Actors.Inventory;
using com.ootii.Actors.LifeCores;
using com.ootii.Cameras;
using com.ootii.Geometry;
using com.ootii.Helpers;
using com.ootii.Reactors;
using UnityEngine;
using UnityEngine.AI;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace com.ootii.Setup
{
    /// <summary>
    /// Helper functions for creating and configuring components on a character, using reasonable default settings and assumptions.
    /// </summary>
    public static class CharacterSetupHelper 
    {
#if UNITY_EDITOR
        /// <summary>
        /// Create the Motion Controller component and configure it for use by a player or NPC
        /// </summary>
        /// <param name="rCharacter"></param>
        /// <param name="rIsPlayer"></param>
        /// <returns></returns>
        public static MotionController CreateMotionController(GameObject rCharacter, bool rIsPlayer)
        {
            MotionController lMotionController = rCharacter.GetOrAddComponent<MotionController>();

            // Enable auto-find for a Player character and disable for NPCs
            lMotionController.AutoFindInputSource = rIsPlayer;
            lMotionController.AutoFindCameraTransform = rIsPlayer;

            if (!rIsPlayer)
            {
                // Remove any references to the camera rig or input source for NPCs
                lMotionController.InputSource = null;                
                lMotionController.CameraRig = null;
                lMotionController.CameraTransform = null;
            }

            return lMotionController;
        }

        /// <summary>
        /// Set up the ActorController with default grounding and collision layers
        /// </summary>
        /// <param name="rMotionController"></param>
        /// <param name="rIsPlayer"></param>        
        public static ActorController ConfigureActorController(MotionController rMotionController, bool rIsPlayer)
        {
            ActorController lActorController = rMotionController.GetOrAddComponent<ActorController>();          

            var lGroundingLayers = new string[]
            {
                "Default",
                DefaultLayers.Names[DefaultLayers.Climb],
                DefaultLayers.Names[DefaultLayers.ScalableWall],
                DefaultLayers.Names[DefaultLayers.ScalableLedge],
                DefaultLayers.Names[DefaultLayers.Interaction],
                DefaultLayers.Names[DefaultLayers.BalanceWalk],
                DefaultLayers.Names[DefaultLayers.PhysicsObjects]
            };

            var lCollisionLayers = new string[]
            {
                "Default",
                DefaultLayers.Names[DefaultLayers.Climb],
                DefaultLayers.Names[DefaultLayers.ScalableWall],
                DefaultLayers.Names[DefaultLayers.ScalableLedge],
                DefaultLayers.Names[DefaultLayers.Interaction],
                DefaultLayers.Names[DefaultLayers.BalanceWalk],
                DefaultLayers.Names[DefaultLayers.Ladder]
            };
            
            lActorController.IsGroundingLayersEnabled = true;
            lActorController.GroundingLayers = LayerMask.GetMask(lGroundingLayers);
            lActorController.CollisionLayers = LayerMask.GetMask(lCollisionLayers);
            lActorController.BaseRadius = 0.3f;

            // Set the inspector to Advanced mode, as Basic mode functions will change what it set up here
            lActorController.EditorShowAdvanced = true;

            CreateDefaultBodyShapes(rMotionController);     
            
            return lActorController;     
        }        

        /// <summary>
        /// Recreate the default body shapes on the character's ActorController
        /// </summary>
        /// <param name="rMotionController"></param>
        public static void CreateDefaultBodyShapes(MotionController rMotionController)
        {
            ActorController lActorController = rMotionController.GetOrAddComponent<ActorController>();           

            lActorController.BodyShapes.Clear();
            BodyCapsule lCapsule = new BodyCapsule
            {
                _Parent = lActorController.transform,
                Name = "Body Capsule",
                Radius = 0.3f,
                Offset = new Vector3(0f, 0.6f, 0f),
                IsEnabledOnGround = true,
                IsEnabledOnSlope = true,
                IsEnabledAboveGround = true,
                EndTransform = lActorController.transform.FindTransform(HumanBodyBones.Head)
            };

            if (lCapsule.EndTransform == null) { lCapsule.EndTransform = lActorController.transform.FindTransform("Head"); }
            if (lCapsule.EndTransform == null) { lCapsule.EndOffset = new Vector3(0f, 1.6f, 0f); }

            lActorController.BodyShapes.Add(lCapsule);

            BodySphere lSphere = new BodySphere
            {
                _Parent = lActorController.transform,
                Name = "Foot Sphere",
                Radius = 0.25f,
                Offset = new Vector3(0f, 0.25f, 0f),
                IsEnabledOnGround = false,
                IsEnabledOnSlope = false,
                IsEnabledAboveGround = true
            };

            lActorController.BodyShapes.Add(lSphere);
            lActorController.EditorComponentInitialized = true;

            // Save the new body shapes
            lActorController.SerializeBodyShapes();
        }

        /// <summary>
        /// Enable the UseTransform option on the Actor Controller
        /// </summary>
        /// <param name="rMotionController"></param>
        public static void EnableUseTransform(MotionController rMotionController)
        {
            // Set Use Transform
            var lActorController = rMotionController.GetComponent<ActorController>();
            lActorController.UseTransformPosition = true;

            // Add the reactor that toggles Use Transform on and off as motions activate or deactivate
            var lActorCore = rMotionController.GetOrAddComponent<ActorCore>();
            lActorCore.GetOrAddReactor<AgentSetUseTransform>();
        }

        /// <summary>
        /// Create the BasicAttributes and add the Health attribute
        /// </summary>
        /// <param name="rMotionController"></param>
        /// <param name="rHealth">Health value to set; if the Health attribute already exist, then its value 
        /// is not overwritten</param>
        /// <param name="rMana"></param>
        /// <param name="rStamina"></param>
        /// <returns></returns>
        public static BasicAttributes CreateBasicAttributes(MotionController rMotionController, float rHealth, float rMana = 0, float rStamina = 0)
        {
            BasicAttributes lAttributes = rMotionController.GetOrAddComponent<BasicAttributes>();

            if (!lAttributes.AttributeExists("Health"))
            {
                lAttributes.SetAttributeValue<float>("Health", rHealth);
                BasicAttributeFloat lHealth = lAttributes.GetAttribute("Health") as BasicAttributeFloat;
                if (lHealth != null)
                {
                    lHealth.MinValue = 0;
                    lHealth.MaxValue = rHealth;                    
                }
            }

            if (rMana > 0 && !lAttributes.AttributeExists("Mana"))
            {
                lAttributes.SetAttributeValue<float>("Mana", rMana);
                BasicAttributeFloat lMana = lAttributes.GetAttribute("Mana") as BasicAttributeFloat;
                if (lMana != null)
                {
                    lMana.MinValue = 0;
                    lMana.MaxValue = rMana;                 
                }
            }

            if (rStamina > 0 && !lAttributes.AttributeExists("Stamina"))
            {
                lAttributes.SetAttributeValue<float>("Stamina", rStamina);
                BasicAttributeFloat lStamina = lAttributes.GetAttribute("Stamina") as BasicAttributeFloat;
                if (lStamina != null)
                {
                    lStamina.MinValue = 0;
                    lStamina.MaxValue = rMana;                    
                }
            }

            lAttributes.OnBeforeSerialize();

            return lAttributes;
        }
        
        /// <summary>
        /// Create and configure the ActorCore without changing the Default Form
        /// </summary>
        /// <param name="rMotionController"></param>
        public static ActorCore CreateActorCore(MotionController rMotionController)
        {
            ActorCore lCore = rMotionController.GetOrAddComponent<ActorCore>();

            lCore.SetInitialStateValue(ActorCore.DefaultStates.Stance, 0);
            lCore.SetInitialStateValue(ActorCore.DefaultStates.DefaultForm, 0);
            lCore.SetInitialStateValue(ActorCore.DefaultStates.CurrentForm, 0);            

            lCore.GetOrAddReactor<BasicAttackedReactor>();
            lCore.GetOrAddReactor<BasicDamagedReactor>();
            lCore.GetOrAddReactor<BasicKilledReactor>();

            lCore.IsAlive = true;
            return lCore;
        }

        /// <summary>
        /// Create and configure the ActorCore using the specified Default Form. The Current Form is set to the
        /// value of the Default Form
        /// </summary>
        /// <param name="rMotionController"></param>
        /// <param name="rDefaultForm"></param>
        public static ActorCore CreateActorCore(MotionController rMotionController, int rDefaultForm)
        {
            ActorCore lCore = CreateActorCore(rMotionController);

            // Overwrite any initial Form values
            lCore.SetStateValue(ActorCore.DefaultStates.DefaultForm, rDefaultForm);
            lCore.SetStateValue(ActorCore.DefaultStates.CurrentForm, rDefaultForm);

            return lCore;
        }

        /// <summary>
        /// Add and configure a Kinematic Rigbody on the character
        /// </summary>
        /// <param name="rMotionController"></param>
        /// <param name="rMass"></param>
        /// <returns></returns>
        public static Rigidbody AddRigidbody(MotionController rMotionController, float rMass = 50.0f)
        {
            Rigidbody lRigidbody = rMotionController.gameObject.GetComponent<Rigidbody>();
            if (lRigidbody == null) { lRigidbody = rMotionController.gameObject.AddComponent<Rigidbody>(); }

            lRigidbody.mass = rMass;
            lRigidbody.useGravity = false;
            lRigidbody.isKinematic = true;

            return lRigidbody;
        }

        /// <summary>
        /// Add and configure a NavMesh Obstacle. This just needs to be on the player; NPCs with a NavMeshAgent
        /// use that for avoidance
        /// </summary>
        /// <param name="rMotionController"></param>
        /// <returns></returns>
        public static NavMeshObstacle AddNavMeshObstacle(MotionController rMotionController)
        {
            NavMeshObstacle lNavMeshObstacle = rMotionController.GetOrAddComponent<NavMeshObstacle>();
            if (lNavMeshObstacle == null) return null;
            lNavMeshObstacle.shape = NavMeshObstacleShape.Capsule;
            lNavMeshObstacle.center = Vector3.up;

            return lNavMeshObstacle;
        }

        /// <summary>
        /// Create and configure a NavMeshAgent component on the character
        /// </summary>
        /// <param name="rMotionController"></param>
        /// <param name="rAngularSpeed"></param>
        /// <returns></returns>
        public static NavMeshAgent AddNavMeshAgent(MotionController rMotionController, float rAngularSpeed = 240f)
        {
            NavMeshAgent lNavMeshAgent = rMotionController.GetOrAddComponent<NavMeshAgent>();
            if (lNavMeshAgent != null)
            {
                lNavMeshAgent.angularSpeed = 240f;
            }

            return lNavMeshAgent;
        }
      
        /// <summary>
        /// Create and configure the Combatant component on the character
        /// </summary>
        /// <param name="rMotionController"></param>
        /// <param name="rMaxLockDistance"></param>
        /// <param name="rIsPlayer"></param>
        /// <returns></returns>
        public static ICombatant CreateCombatant(MotionController rMotionController, float rMaxLockDistance, bool rIsPlayer)
        {
            ICombatant lCombatant = rMotionController.GetComponent<ICombatant>();
            if (lCombatant == null)
            {                
                Type lType = AssemblyHelper.ResolveType("com.ootii.Actors.Combat.Combatant, " + AssemblyHelper.AssemblyInfo);
                if (lType == null) { return null; }

                lCombatant = (ICombatant)rMotionController.gameObject.AddComponent(lType);
                if (lCombatant == null) { return null; }
            }

            lCombatant.ForceActorRotation = rIsPlayer;
            ReflectionHelper.SetProperty(lCombatant, "IsLockingEnabled", true);
            ReflectionHelper.SetProperty(lCombatant, "ForceCameraRotation", rIsPlayer);            
            ReflectionHelper.SetProperty(lCombatant, "MaxLockDistance", rMaxLockDistance);

            if (rIsPlayer)
            {
                ReflectionHelper.SetProperty(lCombatant, "TargetLockedIcon",
                    AssetDatabase.LoadAssetAtPath<Texture>(DefaultPaths.FrameworkContent +
                                                           "Textures/UI/TargetIcon_2.png"));
            }

            return lCombatant;
        }

        /// <summary>
        /// Create and configure the Combatant component on the character
        /// </summary>
        /// <param name="rMotionController"></param>
        /// <param name="rIsPlayer"></param>
        /// <returns></returns>
        public static ICombatant CreateCombatant(MotionController rMotionController, bool rIsPlayer = true)
        {
            return CreateCombatant(rMotionController, 10f, rIsPlayer);
        }


        /// <summary>
        /// Tests if we have a valid camera rig associated with the character
        /// </summary>
        /// <param name="rMotionController"></param>
        /// <param name="rCheckForCameraController"></param>
        /// <returns></returns>
        public static bool HasValidCameraRig(MotionController rMotionController, bool rCheckForCameraController = false)
        {
            IBaseCameraRig lCameraRig = rMotionController.CameraRig;
            if (lCameraRig == null) { lCameraRig = rMotionController.ExtractCameraRig(rMotionController.CameraTransform); }
            if (lCameraRig is FollowRig) { lCameraRig = null; }

            return rCheckForCameraController
                ? lCameraRig != null && lCameraRig.GetType().FullName == "com.ootii.Cameras.CameraController"
                : lCameraRig != null;
        }

        #region Inventory

        /// <summary>
        /// Create and configure the BasicInventory component
        /// </summary>
        /// <param name="rMotionController"></param>
        /// <param name="rCreateDefaultSlots"></param>
        /// <param name="rIsPlayer"></param>
        /// <returns></returns>
        public static BasicInventory CreateBasicInventory(MotionController rMotionController, bool rCreateDefaultSlots, bool rIsPlayer)
        {
            BasicInventory lInventory = rMotionController.GetOrAddComponent<BasicInventory>();

            if (rCreateDefaultSlots) { CreateStandardInventorySlots(lInventory); }

            if (!rIsPlayer)
            {
                lInventory.AutoFindInputSource = false;
                lInventory.InputSource = null;
            }

            return lInventory;
        }
        

        /// <summary>
        /// Create the standard inventory slots used in the Motion Pack demos
        /// </summary>
        /// <param name="rInventory"></param>
        public static void CreateStandardInventorySlots(BasicInventory rInventory)
        {
            BasicInventorySlot lSlot = rInventory.GetInventorySlot(BasicInventory.DefaultSlotNames.RightHand);
            if (lSlot == null)
            {
                rInventory.Slots.Add(new BasicInventorySlot());
                rInventory.Slots[rInventory.Slots.Count - 1].ID = BasicInventory.DefaultSlotNames.RightHand;
                rInventory.Slots[rInventory.Slots.Count - 1].ItemID = "";
            }

            lSlot = rInventory.GetInventorySlot(BasicInventory.DefaultSlotNames.LeftLowerArm);
            if (lSlot == null)
            {
                rInventory.Slots.Add(new BasicInventorySlot());
                rInventory.Slots[rInventory.Slots.Count - 1].ID = BasicInventory.DefaultSlotNames.LeftLowerArm;
                rInventory.Slots[rInventory.Slots.Count - 1].ItemID = "";
            }

            lSlot = rInventory.GetInventorySlot(BasicInventory.DefaultSlotNames.LeftHand);
            if (lSlot == null)
            {
                rInventory.Slots.Add(new BasicInventorySlot());
                rInventory.Slots[rInventory.Slots.Count - 1].ID = BasicInventory.DefaultSlotNames.LeftHand;
                rInventory.Slots[rInventory.Slots.Count - 1].ItemID = "";
            }

            lSlot = rInventory.GetInventorySlot(BasicInventory.DefaultSlotNames.ReadyProjectile);
            if (lSlot == null)
            {
                rInventory.Slots.Add(new BasicInventorySlot());
                rInventory.Slots[rInventory.Slots.Count - 1].ID = BasicInventory.DefaultSlotNames.ReadyProjectile;
                rInventory.Slots[rInventory.Slots.Count - 1].ItemID = "";
            }
        }

        #endregion Inventory
#endif
    }   
}

