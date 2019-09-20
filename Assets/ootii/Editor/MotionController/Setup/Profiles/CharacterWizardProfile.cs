using System;
using System.Collections;
using com.ootii.Actors.AnimationControllers;
using com.ootii.Cameras;
using com.ootii.Graphics.UI;
using com.ootii.Helpers;
using com.ootii.UI;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using com.ootii.Actors.Attributes;
using com.ootii.Geometry;
using com.ootii.Setup.Modules;
using UnityEngine;


using UnityEditor;
using UnityEditor.Animations;


namespace com.ootii.Setup
{
    [CreateAssetMenu(menuName = "ootii/Temp/Character Wizard Profile")]
    public class CharacterWizardProfile : BaseSetupProfile
    {       
#if OOTII_SSMP || OOTII_AYMP || OOTII_SCMP
        private static bool mHasCombatMotionPack = true;
#else
        private static bool mHasCombatMotionPack = false;
#endif

        public enum PriorityStatus
        {
            Default,    // the default built-in profiles
            Official,   // other official ootii profiles (Motion Packs, etc)
            User        // User-created profiles
        }

        #region Serialized Fields

        // ===================================================
        // Profile Settings  
        [Tooltip("The image representing this profile when displayed in the Wizard")]
        public Texture2D Image = null;

        [Tooltip("Don't allow this profile to be edited in the inspector.")]
        public bool ReadOnly = false;

        [Tooltip("Display priority of this profile.")]
        public PriorityStatus Priority = PriorityStatus.User;

        [Tooltip("Text shown under this profile's button when shown in Basic mode.")]
        public string DisplayText = "";

        [TextArea(3, 5)]
        [Tooltip("Description of the profile; this is shown in Basic mode.")]
        public string Description = "";
      
        // ===================================================
        // NPC Settings
        [Tooltip("Enable UseTransformPosition on the Actor Controller")]
        public bool UseTransformPosition = true;

        [Tooltip("Add a Unity NavMesh Agent component to an NPC?")]
        public bool AddNavMeshAgent = true;

        [Tooltip("The Angular Speed to set on the NavMesh Agent.")]
        public float NavMeshAngularSpeed = 240f;

        [Tooltip("Use a health bar to visualize the NPC's health?")]
        public bool UseCombatantHUD = true;

        [Tooltip("Prefab for the NPC's health bar HUD. This should be configured to display in World Space.")]
        public BasicCombatantHUD CombatantHealthBarPrefab;

        [Tooltip("Y-axis position of the world space HUD")]
        public float WorldSpaceHUDPosition = 2.2f;

        [Tooltip("Layer to assign a created NPC to.")]
        public int NPCLayer = 0;

        // ===================================================
        // Animator Settings  
        [Tooltip("The Animator Controller to use for this character.")]
        public RuntimeAnimatorController AnimatorController;

        [Tooltip("Create a new animator controller?")]
        public bool CreateNewAnimator = true;

        [Tooltip("Create a new animator controller every time.")]
        public bool AlwaysCreateNewAnimator = false;

        [Tooltip("Configure existing animator?")]
        public bool ConfigureAnimator = true;

        [Tooltip("The animator controller will be created with the same name as the profile.")]
        public bool AnimatorUsesProfileName = false;

        [Tooltip("The animator controller will be created with the same name as the character.")]
        public bool AnimatorUsesCharacterName = true;

        [Tooltip("The name of the created animator controller.")]
        public string AnimatorName = "BasicHumanoid";

        [Tooltip("The path to the folder containing the animator controller")]
        public string AnimatorPath = String.Empty;

        // ===================================================
        // Player Settings
        [Tooltip("Use an already configured camera rig prefab? If false, create a new camera rig.")]
        public bool UseCameraPrefab = false;

        [Tooltip("The camera prefab object to use.")]
        public BaseCameraRig CameraPrefab;

        [Tooltip("The Camera Controller will follow a Base Camera Anchor instead of the player.")]
        public bool UseCameraAnchor = false;

        [Tooltip("Offset from the target's root that the anchor will follow")]
        public Vector3 AnchorTargetOffset = Vector3.zero;

        [Tooltip("Does the anchor rotate with the target?")]
        public bool AnchorRotatesWithTarget = true;

        [Range(0,1), Tooltip("Lerp applied to the position to smooth movement.")]
        public float AnchorMovementLerp = 1;

        [Tooltip("Update the Unity Input Manager with the default input settings for all ootii assets?")]
        public bool UpdateInput = true;

        [Tooltip("Which mouse button determines view rotation?")]
        public int ViewActivator = 0;

        [Tooltip("Add the Game Core object, if it doesn't exist?")]
        public bool AddGameCore = true;

        [Tooltip("Use the targeting reticle for the player?")]
        public bool UseReticle = true;

        [Tooltip("The reticle prefab")]
        public Reticle ReticlePrefab;

        [Tooltip("Is the reticle visible on startup?")]
        public bool ReticleStartVisible = false;

        [Tooltip("Set up a health bar HUD for the player?")]
        public bool UsePlayerHUD = mHasCombatMotionPack;

        [Tooltip("Prefab for the player HUD")]
        public BasicCombatantHUD PlayerHealthBarPrefab;

        [Tooltip("Hide the playe's health bar when full?")]
        public bool PlayerHealthBarHideWhenFull = false;

        [Tooltip("Layer to assign a created Player to.")]
        public int PlayerLayer = 0;


        // ===================================================
        // Component Settings        
        [Tooltip("Add and configure the BasicAttributes component.")]
        public bool AddBasicAttributes = true;

        [Tooltip("Default value of the 'Health' attribute.")]
        public float HealthValue = 100;

        [Tooltip("Add and configure the Basic Inventory component.")]
        public bool AddBasicInventory = true;

        [Tooltip("Create the slots used with the default Basic Inventory setup.")]
        public bool CreateDefaultSlots = true;

        [Tooltip("Add a kinematic rigidbody component.")]
        public bool AddRigidbody = true;

        [Tooltip("The mass of the Rigidbody.")]
        public float RigidbodyMass = 50.0f;

        [Tooltip("Add and configure a Combatant component.")]
        public bool AddCombatant = mHasCombatMotionPack;

        [Range(0, 100)]
        [Tooltip("The maximum target lock distance for the Combatant.")]
        public float MaxTargetLockDistance = 10.0f;

        // ===================================================
        // Motion Settings
        public int MovementStyle = 0; // Default to "Adventure"        

        [Tooltip("Add the set of Basic motions: Basic Idle, Basic Walk/Run/Pivot, Basic Walk/Run/Strafe.")]
        public bool AddBasicMotions = true;

        [Tooltip("Adds the Basic full-body reaction motions: Basic Damaged and Basic Death.")]
        public bool AddFullBodyReactions = true;

        [Tooltip("Adds the utility/reaction motions: Cower, Frozen, Pushed Back, Knocked Down, Sleep")]
        public bool AddUtilityMotions = true;

        [Tooltip("Adds the Basic Interaction motion.")]
        public bool AddInteraction = true;

        [Tooltip("The layer mask for the Basic Interaction motion.")]
        public LayerMask InteractionLayers = 1 << DefaultLayers.Interaction;

        [Tooltip("Adds the standard Jump and Fall motions.")]
        public bool AddJumpMotions = true;

        [Tooltip("Adds the standard climbing motions: Climb 0.5m, Climb 1.0m, Climb 1.8m, Climb 2.5m, and Climb Ladder.")]
        public bool AddClimbMotions = true;

        [Tooltip("The layer mask for the standard climbing motions: Climb 0.5m, Climb 1.0m, Climb 1.8m, Climb 2.5m.")]
        public LayerMask ClimbLayers = 1 << DefaultLayers.Climb;

        [Tooltip("The layer mask for the standard Climb Ladder motion.")]
        public LayerMask ClimbLadderLayers = 1 << DefaultLayers.Ladder;

        [Tooltip("The layer mask for the standard Vault 1m motion.")]
        public LayerMask VaultLayers = 1 << DefaultLayers.Climb;

        // ===================================================
        // Serialized but not displayed in Editor

        // List of Motion Packs required by this profile (optional); if any of these are missing,
        // then the Profile will not be displayed in Basic Mode (a warning message will be shown
        // in Advanced Mode)
        public List<string> _RequiredMotionPacks = new List<string>();

        #endregion Serialized Fields

        public bool IsWorking { get { return mIsWorking; } }
        private bool mIsWorking = false;

        public bool HasRequiredPacks { get { return mHasRequiredPacks; } }
        protected bool mHasRequiredPacks = true;

        protected WaitForSeconds mShortWait = new WaitForSeconds(0.25f);            

        // The highest-indexed animator layer that we will need to configure
        protected int mLastLayerIndex = EnumMotionLayer.UPPER_BODY;
        

        #region Initialization

        protected virtual void OnEnable()
        {
            if (string.IsNullOrEmpty(DisplayText))
            {
                SetDefaultDisplayText();
            }

            mHasRequiredPacks = CheckRequiredMotionPacks();
            
            // If there are no saved preferences for certain prefabs, load the default ones            
            if (CombatantHealthBarPrefab == null)
            {
                CombatantHealthBarPrefab = Resources.Load<BasicCombatantHUD>("UI/Combatant HUD");
            }

            if (PlayerHealthBarPrefab == null)
            {
                PlayerHealthBarPrefab = Resources.Load<BasicCombatantHUD>("UI/Player HUD");
            }

            if (ReticlePrefab == null)
            {
                ReticlePrefab = Resources.Load<Reticle>("UI/Reticle");
            }

#if OOTII_CC
            CameraPrefab = Resources.Load<CameraController>("Camera Rig [Advanced]");
#else
            CameraPrefab = Resources.Load<BaseCameraRig>("Camera Rig [Orbit]");
#endif

            InitializeModules();                    
        }

        /// <summary>
        /// Verify that all required Motion Packs are installed for the profile
        /// </summary>        
        /// <returns></returns>
        public virtual bool CheckRequiredMotionPacks()
        {
            // Only worry about this for the "official" profiles that we show in Basic Mode
            if (Priority == PriorityStatus.User) { return true; }
            if (_RequiredMotionPacks.Any())
            {
                foreach (var lRequiredPack in _RequiredMotionPacks)
                {
                    if (!MotionPackSetupHelper.PackNames.Contains(lRequiredPack))
                    {                        
                        return false;
                    }
                }
            }
            
            return true;
        }

        /// <summary>
        /// Called when the profile is loaded into the Character Wizard
        /// </summary>
        public virtual void UpdateRequirements()
        {
            mHasRequiredPacks = CheckRequiredMotionPacks();
        }


        #endregion Initialization


        #region Group Toggles

        /// <summary>
        /// Enable or disable all component options
        /// </summary>
        /// <param name="rEnabled"></param>
        public virtual void SetComponentOptions(bool rEnabled)
        {
            AddBasicAttributes = rEnabled;
            AddBasicInventory = rEnabled;
            AddRigidbody = rEnabled;
            AddCombatant = rEnabled;
        }
       
        /// <summary>
        /// Enable or disable all motion options
        /// </summary>
        /// <param name="rEnabled"></param>
        public virtual void SetMotionOptions(bool rEnabled)
        {
            AddBasicMotions = rEnabled;
            AddUtilityMotions = rEnabled;
            AddJumpMotions = rEnabled;
            AddClimbMotions = rEnabled;
        }
        
        #endregion Group Toggles


        #region Create Character      


        public virtual IEnumerator CreateCharacter(MotionController rMotionController, string lAnimatorPath, string lCharacterName, bool rIsPlayer)
        {
            mIsWorking = true;

            // Initialize the modules with the Motion Controller references and Is Player
            if (Modules != null)
            {
                foreach (var lModule in Modules)
                {
                    // Check if the module requires a higher-indexed animator layer than we currently have stored.
                    // This way we only need to verify the Animator Controller and Motion Layers setup once.
                    int lLayerIndex = ModuleLastAnimatorLayer.GetLayer(lModule.GetType());
                    if (lLayerIndex > mLastLayerIndex) { mLastLayerIndex = lLayerIndex; }                    
                    lModule.BeginSetup(rMotionController);
                }
            }
            if (rIsPlayer)
            {
               SetupPlayer(rMotionController);
                yield return mShortWait;
            }
            else
            {
                SetupNPC(rMotionController);
            }
            
            SetupComponents(rMotionController, rIsPlayer);
            yield return mShortWait;
            
            CreateMotions(rMotionController, rIsPlayer);
            yield return mShortWait;

            if (rIsPlayer)
            {
                SetupSceneUI(rMotionController);
            }
            else if (UseCombatantHUD)
            {
                // If just setting up an AI character, we'll use world space HUDs for now and not mess around with any scene UI 
                // already set up
                SetupCombatantHUD(rMotionController);
            }            
            yield return mShortWait;

            if (rIsPlayer && UpdateInput)
            {
                ConfigureInput(rMotionController);
            }            

            
            VerifyAnimatorConfiguration(rMotionController, lAnimatorPath, lCharacterName);

            while (EditorApplication.isUpdating)
            {
                yield return mShortWait;
            }

            EditorUtility.DisplayProgressBar("Finalizing (Step 6 or 6)", "Completing character setup", 1f);            

            yield return mShortWait;
            mIsWorking = false;
        }

        /// <summary>
        /// Creates the Input Source, Camera and Game Core, assigns references, and sets the player's Layer and Tag
        /// </summary>
        /// <param name="rMotionController"></param>
        protected virtual void SetupPlayer(MotionController rMotionController)
        {
            // Find or create the input source
            GameObject lInputSourceGO = InputSetupHelper.GetOrCreateInputSource(ViewActivator);
            rMotionController.InputSourceOwner = lInputSourceGO;

            // Find or create the camera
            BaseCameraRig lCameraRig = CameraSetupHelper.FindSceneCameraRig();
            if (lCameraRig == null) { lCameraRig = CreateCameraRig(); }

            rMotionController.CameraTransform = lCameraRig.transform;
            ReflectionHelper.SetProperty(lCameraRig, "InputSourceOwner", lInputSourceGO);

            if (UseCameraAnchor)
            {
                GameObject lCameraAnchorGO = null;
                BaseCameraAnchor lCameraAnchor = CameraSetupHelper.GetOrCreateCameraAnchor(out lCameraAnchorGO);
                lCameraAnchor.Root = rMotionController.gameObject.transform;
                lCameraAnchor.RootOffset = AnchorTargetOffset;
                lCameraAnchor.RotateWithTarget = AnchorRotatesWithTarget;
                if (AnchorRotatesWithTarget)
                {
                    lCameraAnchor.RotationRoot = rMotionController.gameObject.transform;
                }
                lCameraAnchor.MovementLerp = AnchorMovementLerp;
                lCameraRig.Anchor = lCameraAnchorGO.transform;
            }
            else
            {
                CameraSetupHelper.DisableCameraAnchors();
                lCameraRig.Anchor = rMotionController.transform;
            }

            if (AddGameCore) { SceneSetupHelper.ConfigureGameCore(lInputSourceGO); }

            // Set player's layer and tag
            rMotionController.gameObject.layer = PlayerLayer;
            rMotionController.gameObject.tag = "Player";
        }
     
        /// <summary>
        /// Performs additional setup for NPCs
        /// </summary>
        /// <param name="rMotionController"></param>
        protected virtual void SetupNPC(MotionController rMotionController)
        {
            rMotionController.gameObject.layer = NPCLayer;
        }

        /// <summary>
        /// Get the active camera rig based on the current configuration
        /// </summary>
        /// <returns></returns>
        protected virtual BaseCameraRig CreateCameraRig()
        {           

            BaseCameraRig lCameraRig = null;
            if (UseCameraPrefab && CameraPrefab != null)
            {
                lCameraRig = CameraSetupHelper.InstantiateCamera(CameraPrefab);
            }
            else
            {
#if OOTII_CC
                lCameraRig = CameraSetupHelper.CreateCameraRig<CameraController>();
                CameraSetupHelper.SetupThirdPersonCamera(lCameraRig);
#else
                lCameraRig = CameraSetupHelper.CreateCameraRig<OrbitRig>();            
#endif
            }


            return lCameraRig;
        }

        /// <summary>
        /// Step 1: Add and configure the actor components on the character
        /// </summary>
        /// <param name="rMotionController"></param>
        /// <param name="rIsPlayer"></param>        
        protected virtual void SetupComponents(MotionController rMotionController, bool rIsPlayer)
        {
            string lComponentsTitle = "Configuring Components (Step 1 of 6)";
            float lProgressAmount = 1f / 6f;

            EditorUtility.DisplayProgressBar(lComponentsTitle, "Base components", lProgressAmount);

            // Set the Motion Controller's inspector to Advanced mode, as Basic mode's functionality is superseded by
            // the Character Wizard
            rMotionController.EditorTabIndex = 1;

            // Configure core components (Actor Controller and Actor Core):
            CharacterSetupHelper.ConfigureActorController(rMotionController, rIsPlayer);
            CharacterSetupHelper.CreateActorCore(rMotionController, 0);
            if (!rIsPlayer && UseTransformPosition) { CharacterSetupHelper.EnableUseTransform(rMotionController); }
            
            // Configure Attributes:
            if (AddBasicAttributes)
            {
                var lAttributes = CharacterSetupHelper.CreateBasicAttributes(rMotionController, HealthValue);
                if (!rIsPlayer)
                {
                    // Add the "NPC" tag attribute to NPCs
                    if (!lAttributes.AttributeExists("NPC"))
                    {
                        lAttributes.AddAttribute("NPC", EnumAttributeTypes.Types[EnumAttributeTypes.TAG]);
                        lAttributes.OnBeforeSerialize();
                    }
                }
            }

            // Configure Inventory:
            if (AddBasicInventory) { CharacterSetupHelper.CreateBasicInventory(rMotionController, CreateDefaultSlots, rIsPlayer); }

            // Configure the Combatant:
            if (AddCombatant) { CharacterSetupHelper.CreateCombatant(rMotionController, MaxTargetLockDistance, rIsPlayer); }

            // Configure additional Unity components:
            if (AddRigidbody) { CharacterSetupHelper.AddRigidbody(rMotionController, RigidbodyMass); }
            if (!rIsPlayer && AddNavMeshAgent) { CharacterSetupHelper.AddNavMeshAgent(rMotionController, NavMeshAngularSpeed); }            

            // Configure components in enabled modules:
            if (Modules == null) { return; }

            EditorUtility.DisplayProgressBar(lComponentsTitle, "Setting up module components", lProgressAmount);
            foreach (var lModule in Modules)
            {
                if (lModule.IsValid && lModule is IConfigureComponents)
                {
                    ((IConfigureComponents)lModule).ConfigureComponents();
                }
            }
        }

        /// <summary>
        /// Step 2: Add and configure scene UI objects. Currently, this should only be run when 
        /// configuring the player
        /// </summary>
        /// <param name="rMotionController"></param>
        protected virtual void SetupSceneUI(MotionController rMotionController)
        {
            string lProgressTitle = "Configuring Scene Objects (Step 2 of 6)";
            float lProgressAmount = 2f / 6f;

            EditorUtility.DisplayProgressBar(lProgressTitle, "Base scene objects", lProgressAmount);

            GameObject lUIContainer = UISetupHelper.GetUIContainer();
            if (lUIContainer == null) return;
         
            if (UseReticle && ReticlePrefab != null)
            {
                Reticle lReticle = FindObjectOfType<Reticle>();
                if (lReticle == null)
                {

                    lReticle = Instantiate(ReticlePrefab);
                    lReticle.name = ReticlePrefab.name;
                    lReticle.transform.SetParent(lUIContainer.transform);
                    lReticle.transform.Reset();
                }
               
                lReticle.IsVisible = ReticleStartVisible;               
            }

            if (UsePlayerHUD)
            {
                Canvas lSceneCanvas = lUIContainer.GetComponentInChildren<Canvas>();
                SetupPlayerHUD(rMotionController, lSceneCanvas);
            }            
        }        

        /// <summary>
        /// Create and configure the Player health bar HUD
        /// </summary>
        /// <param name="rMotionController"></param>
        /// <param name="rSceneCanvas"></param>
        protected virtual void SetupPlayerHUD(MotionController rMotionController, Canvas rSceneCanvas)
        {
            // We want a single Player HUD in the scene, so attempt to find a HUD that is not set to World Space            
            List<BasicCombatantHUD> lHUDs = FindObjectsOfType<BasicCombatantHUD>().ToList();
            BasicCombatantHUD lPlayerHUD = null;
            if (lHUDs != null)
                lPlayerHUD = lHUDs.FirstOrDefault(x => !x.DisplayInWorldSpace);

            if (lPlayerHUD != null)
            {
                lPlayerHUD.UsePlayer = true;
                lPlayerHUD.BasicAttributes = rMotionController.GetComponent<BasicAttributes>();
                lPlayerHUD.HealthKey = "Health";
                lPlayerHUD.HideWhenFull = PlayerHealthBarHideWhenFull;
            }
            else
            {
                lPlayerHUD = Instantiate(PlayerHealthBarPrefab);
                lPlayerHUD.name = "Player HUD";
                lPlayerHUD.transform.SetParent(rSceneCanvas.transform);
                lPlayerHUD.transform.ResetRect();
            }            
        }

        /// <summary>
        /// Create and configure the NPC combatant HUD. Right now, only uses a HUD drawn in World Space (attached to the character)
        /// </summary>
        /// <param name="rMotionController"></param>
        protected virtual void SetupCombatantHUD(MotionController rMotionController)
        {
            //bool lIsWorldSpacePrefab = false;

            //if (CombatantHealthBarPrefab != null)
            //{
            //    lIsWorldSpacePrefab = CombatantHealthBarPrefab.DisplayInWorldSpace;
            //}

            //// If the HUD is set up to display in world space, then we just need to ensure an instance is attached to the character
            //if (!lIsWorldSpacePrefab) return;

            // Check to see if the current character GameObject already has a HUD attached
            BasicCombatantHUD lCombatantHUD = rMotionController.GetComponentInChildren<BasicCombatantHUD>();

            // Instantiate the HUD prefab if it wasn't found
            if (lCombatantHUD == null)
            {
                lCombatantHUD = Instantiate(CombatantHealthBarPrefab);
                lCombatantHUD.name = "Combatant HUD";
                lCombatantHUD.transform.SetParent(rMotionController.gameObject.transform);
                lCombatantHUD.transform.Reset();
                lCombatantHUD.transform.position = new Vector3(0, WorldSpaceHUDPosition, 0);
            }

            // Set up the HUD for this character
            
            lCombatantHUD.UsePlayer = false;
            lCombatantHUD.BasicAttributes = rMotionController.GetComponent<BasicAttributes>();
            lCombatantHUD.HealthKey = "Health";
                        
            // TODO: allow AI characters to share a single instance of a HUD; we would reassign the HUD to an AI character
            // when the player's targeted Combatant changes.
        }

        /// <summary>
        /// Step 3: Create and configure the selected motions on the character
        /// </summary>
        /// <param name="rMotionController"></param>
        /// <param name="rIsPlayer"></param>
        protected virtual void CreateMotions(MotionController rMotionController, bool rIsPlayer)
        {
            string lProgressTitle = "Configuring Motions (Step 3 of 6)";
            float lProgressAmount = 3f / 6f;

            EditorUtility.DisplayProgressBar(lProgressTitle, "Basic motions", lProgressAmount);

            if (AddBasicMotions)
            {                
                MotionSetupHelper.EnsureMotionLayers(rMotionController, mLastLayerIndex);

                if (rIsPlayer)
                {
                    BasicMotionPackDefinition.CreateBasicLocomotion(rMotionController, false, MovementStyle);
                }
                else
                {
                    BasicMotionPackDefinition.CreateBasicNPCLocomotion(rMotionController, true);
                }
                

                if (AddInteraction) { BasicMotionPackDefinition.CreateInteractions(rMotionController); }
                if (AddFullBodyReactions)
                {
                    BasicMotionPackDefinition.CreateBasicDamagedMotion(rMotionController, true);
                    BasicMotionPackDefinition.CreateBasicDeathMotion(rMotionController, true);
                }
                if (AddUtilityMotions) { BasicMotionPackDefinition.CreateUtilityMotions(rMotionController, true); }
            }

            if (AddJumpMotions)
            {
                BasicMotionPackDefinition.CreateJumpMotions(rMotionController);
            }
            if (AddClimbMotions)
            {
                BasicMotionPackDefinition.CreateClimbMotions(rMotionController, ClimbLayers);
                if (rIsPlayer)
                {
                    BasicMotionPackDefinition.CreateClimbLadderMotion(rMotionController, ClimbLadderLayers);
                    BasicMotionPackDefinition.CreateVaultMotion(rMotionController, VaultLayers, true);
                }
            }

            //if (UseHandPoses)
            //{
            //    BasicMotionPackDefinition.CreateBasicHandPoseMotions(rMotionController);
            //}
            
            if (Modules == null) { return; }
            foreach (var lModule in Modules)
            {
                if (lModule.IsValid && lModule is IConfigureMotions)
                {
                    EditorUtility.DisplayProgressBar(lProgressTitle, lModule.Name, lProgressAmount);
                    ((IConfigureMotions)lModule).ConfigureMotions();
                }
            }
        }        

        /// <summary>
        /// Step 4: Apply Input Manager settings
        /// </summary>
        /// <param name="rMotionController"></param>
        protected virtual void ConfigureInput(MotionController rMotionController)
        {
            string lProgressTitle = "Configuring Input (Step 4 of 6)";
            float lProgressAmount = 4f / 6f;

            EditorUtility.DisplayProgressBar(lProgressTitle, "Input Manager settings", lProgressAmount);
            InputSetupHelper.CreateDefaultInputSettings(rMotionController);
            if (Modules == null) { return; }

            foreach (var lModule in Modules)
            {
                if (lModule.IsValid && lModule is IConfigureInput)
                {
                    ((IConfigureInput)lModule).ConfigureInput();
                }                
            }
        }

        /// <summary>
        /// Step 5: Ensure that all of the required SSMs have been created on the animator controller
        /// </summary>
        /// <param name="rMotionController"></param>
        /// <param name="rFolderPath"></param>
        /// <param name="rCharacterName"></param>
        protected virtual void VerifyAnimatorConfiguration(MotionController rMotionController, string rFolderPath, string rCharacterName)
        { 
            string lProgressTitle = "Configuring Animator (Step 5 of 6)";
            float lProgressAmount = 4f / 6f;

            EditorUtility.DisplayProgressBar(lProgressTitle, "Basic animator states", lProgressAmount);
            
            AnimatorController lController = null;                
            if (CreateNewAnimator)
            {
                if (!rFolderPath.EndsWith("/")) { rFolderPath += "/"; }
                string lAnimatorPath;

                if (AnimatorUsesCharacterName)
                {
                    lAnimatorPath = rFolderPath + rCharacterName;
                }
                else if (AnimatorUsesProfileName)
                {
                    lAnimatorPath = rFolderPath + this.name;
                }
                else
                {
                    lAnimatorPath = rFolderPath + AnimatorName;
                }                
                
                if (!lAnimatorPath.EndsWith(".controller")) { lAnimatorPath += ".controller"; }
                

                lController = AnimatorSetupHelper.EnsureAnimatorController(rMotionController, lAnimatorPath, mLastLayerIndex);
                AnimatorController = lController;
            }            

            if (!ConfigureAnimator) { return;}

            // Verify the base Animator Controller configuration
            lController = AnimatorSetupHelper.EnsureAnimatorController(rMotionController, mLastLayerIndex);            
            
            // Verify the SSMs for all Basic motions
            BasicMotionPackDefinition.EnsureBaseLayerStartState(lController);
            BasicMotionPackDefinition.EnsureBasicIdleSSM(lController, 0);
            BasicMotionPackDefinition.EnsureBasicWalkRunPivotSSM(lController, 0);
            BasicMotionPackDefinition.EnsureBasicWalkRunStrafeSSM(lController, 0);
            BasicMotionPackDefinition.EnsureBasicDamagedSSM(lController, 0);
            BasicMotionPackDefinition.EnsureBasicDeathSSM(lController, 0);
            BasicMotionPackDefinition.EnsureBasicInteractionSSM(lController, 0);
            BasicMotionPackDefinition.EnsureBasicJumpSSM(lController, 0);
            BasicMotionPackDefinition.EnsureBasicDodgeSSM(lController, 0);        

            // Verify the SSMs for the "standard" motion set
            if (AddJumpMotions)
            {
                BasicMotionPackDefinition.EnsureStandardJump(rMotionController, 0);
            }

            if (AddClimbMotions)
            {
                BasicMotionPackDefinition.EnsureStandardClimb_0_5m(rMotionController, 0);
                BasicMotionPackDefinition.EnsureStandardClimb_1m(rMotionController, 0);
                BasicMotionPackDefinition.EnsureStandardClimb_1_8m(rMotionController, 0);
                BasicMotionPackDefinition.EnsureStandardClimb_2_5m(rMotionController, 0);
                BasicMotionPackDefinition.EnsureStandardClimbLadder(rMotionController, 0);
                BasicMotionPackDefinition.EnsureStandardClimbWall(rMotionController, 0);
                BasicMotionPackDefinition.EnsureStandardClimbLedge(rMotionController, 0);
                BasicMotionPackDefinition.EnsureStandardVault_1m(rMotionController, 0);
            }
            if (AddUtilityMotions)
            {
                BasicMotionPackDefinition.EnsureStandardBalanceWalk(rMotionController, 0);                
                BasicMotionPackDefinition.EnsureStandardUtilities(rMotionController, 0);
            }

            if (Modules == null) { return; }

            int lModuleCount = Modules.Count(x => x is IConfigureAnimator);            
            // fill additional 20% of progress bar across all modules
            float lModuleAmount = lModuleCount > 0 ? 0.2f/lModuleCount : 0;

            foreach (var lModule in Modules)
            {
                if (lModule.IsValid && lModule is IConfigureAnimator)
                {
                    lProgressAmount += lModuleAmount;
                    EditorUtility.DisplayProgressBar(lProgressTitle, lModule.Name, lProgressAmount);
                    ((IConfigureAnimator)lModule).ConfigureAnimator();
                }
            }
        }
       
        #endregion  Create Character


        #region Utility Functions

        /// <summary>
        /// Set the default Display Text based on the profile name (upon creation of the profile or the 
        /// first time it is renamed)
        /// </summary>
        public void SetDefaultDisplayText()
        {
            string lDisplayText = this.name.EndsWith(" Profile")
                ? this.name.Replace(" Profile", "")
                : this.name;

            if (!lDisplayText.StartsWith("Set Up ", StringComparison.Ordinal))
            {
                lDisplayText = lDisplayText.Insert(0, "Set Up ");
            }

            DisplayText = lDisplayText;
        }
       

        #endregion Utility Functions


        #region Asset Management
        
        /// <summary>
        /// Create a copy of this profile at the specified path
        /// </summary>
        /// <param name="rNewPath"></param>
        /// <returns></returns>
        public CharacterWizardProfile Copy(string rNewPath = "")
        {
            CharacterWizardProfile lCopiedProfile = null;
            try
            {                
                string lNewPath = string.IsNullOrEmpty(rNewPath) 
                    ? AssetHelper.GetNewAssetPath(AssetPath)
                    : AssetHelper.GetNewAssetPath(rNewPath);

                if (!lNewPath.EndsWith(".asset")) { lNewPath += ".asset"; }

                if (!File.Exists(lNewPath))
                {
                    AssetDatabase.CopyAsset(AssetPath, lNewPath);
                }

                lCopiedProfile = AssetDatabase.LoadAssetAtPath<CharacterWizardProfile>(lNewPath);
                if (lCopiedProfile != null)
                {
                    // Ensure that the copied profile is set up for user editing
                    lCopiedProfile.ReadOnly = false;
                    lCopiedProfile.Priority = PriorityStatus.User;
                    lCopiedProfile._Renamed = false;
                }
                return lCopiedProfile;

            }
            catch (IOException ex)
            {
                Debug.LogException(ex);
            }

            return lCopiedProfile;
        }
        

        /// <summary>
        /// Store the profile's path in EditorPrefs
        /// </summary>
        /// <param name="rPrefsKey"></param>
        public void StorePath(string rPrefsKey)
        {
            EditorPrefs.SetString(rPrefsKey, AssetPath);
        }

        /// <summary>
        /// Create a new profile at the specified path
        /// </summary>
        /// <param name="rPath"></param>
        /// <returns></returns>
        public static CharacterWizardProfile Create(string rPath)
        {
            string lPath = AssetHelper.GetNewAssetPath(rPath);
            return AssetHelper.GetOrCreateAsset<CharacterWizardProfile>(lPath);
        }


        /// <summary>
        /// Load a profile at the specified path
        /// </summary>
        /// <param name="rPath"></param>
        /// <returns></returns>
        public static CharacterWizardProfile Load(string rPath)
        {
            CharacterWizardProfile lProfile = AssetDatabase.LoadAssetAtPath<CharacterWizardProfile>(rPath);

            return lProfile;
        }

        // Load a profile using the path stored in EditorPrefs (if any)
        public static CharacterWizardProfile LoadFromStoredPath(string rPrefsKey)
        {
            string lSavedPath = EditorPrefs.GetString(rPrefsKey, "");
            if (string.IsNullOrEmpty(lSavedPath)) { return null; }

            return Load(lSavedPath);
        }

        /// <summary>
        /// Set the default display text the first time the asset is renamed
        /// </summary>
        /// <param name="rSetRenamed"></param>
        protected override void OnFirstRenamed(bool rSetRenamed)
        {
            SetDefaultDisplayText();
            base.OnFirstRenamed(rSetRenamed);
        }

        #endregion Asset Management
       

    }
}
