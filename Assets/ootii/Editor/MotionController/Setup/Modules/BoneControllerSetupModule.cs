#if OOTII_BC && OOTII_MC
using UnityEngine;
using com.ootii.Actors.BoneControllers;
using com.ootii.Geometry;
using com.ootii.Helpers;

using Object = UnityEngine.Object;
using UnityEditor;

namespace com.ootii.Setup.Modules
{
    [ModuleName("Bone Controller"), ModuleCategory(SetupModuleCategories.MotionPacks)]
    [ModuleDescription("Sets up ootii's Bone Controller with Foot to Ground IK and Hand IK.")]
    public sealed class BoneControllerSetupModule : SetupModule, IConfigureComponents
    {
        public bool _IsHumanoidRig = true;
        public bool IsHumanoidRig
        {
            get { return _IsHumanoidRig; }
            set { _IsHumanoidRig = value; }
        }

        public Transform _RootTransform = null;
        public Transform RootTransform
        {
            get { return _RootTransform; }
            set { _RootTransform = value; }
        }

        public bool _SetBoneJoints = true;
        public bool SetBoneJoints
        {
            get { return _SetBoneJoints; }
            set { _SetBoneJoints = value; }
        }

        public bool _CreateBoneColliders = false;
        public bool CreateBoneColliders
        {
            get { return _CreateBoneColliders; }
            set { _CreateBoneColliders = value; }
        }
        
        public bool _UseFootIK = true;
        public bool UseFootIK
        {
            get { return _UseFootIK; }
            set { _UseFootIK = value; }
        }

        public bool _UseHandIK = true;
        public bool UseHandIK
        {
            get { return _UseHandIK; }
            set { _UseHandIK = value; }
        }

        public string _LeftFootMotorName = "Left Leg";
        public string LeftFootMotorName
        {
            get { return _LeftFootMotorName; }
            set { _LeftFootMotorName = value; }
        }

        public string _RightFootMotorName = "Right Leg";
        public string RightFootMotorName
        {
            get { return _RightFootMotorName; }
            set { _RightFootMotorName = value; }
        }

        public string _LeftHandMotorName = "Left Hand";
        public string LeftHandMotorName
        {
            get { return _LeftHandMotorName; }
            set { _LeftHandMotorName = value; }
        }

        public string _RightHandMotorName = "Right Hand";
        public string RightHandMotorName
        {
            get { return _RightHandMotorName; }
            set { _RightHandMotorName = value; }
        }

        public int _GroundLayers = LayerMask.GetMask("Default");
        public LayerMask GroundLayers
        {
            get { return _GroundLayers; }
            set { _GroundLayers = value; }
        }

        private string mBoneFilters = "IK_|FK_|Roll";
        

        public override void Initialize(bool rUseDefaults = false)
        {
            if (rUseDefaults)
            {
                IsHumanoidRig = true;
                UseHandIK = true;
                UseFootIK = true;
                CreateBoneColliders = false;
                SetBoneJoints = true;
                GroundLayers = LayerMask.GetMask("Default");
            }
        }

        public void ConfigureComponents()
        {
            BoneController lBoneController = mMotionController.GetOrAddComponent<BoneController>();
            lBoneController.EditorBoneFilters = mBoneFilters;
            Animator lAnimator = mMotionController.GetComponent<Animator>();            

            lBoneController.RootTransform = lAnimator.GetBoneTransform(HumanBodyBones.Hips); 

            if (SetBoneJoints)
            {
                if (IsHumanoidRig) { lBoneController.SetHumanoidBoneJoints(0); }
                else { lBoneController.SetBoneJoints(0); }
            }

            if (CreateBoneColliders)
            {
                if (IsHumanoidRig) { lBoneController.SetHumanoidBoneColliders(0); }
                else { lBoneController.SetBoneColliders(0); }
            }

            if (UseFootIK)
            {
                CreateFootToGroundMotor(lBoneController, true);
                CreateFootToGroundMotor(lBoneController, false);
            }

            if (UseHandIK)
            {
                // Not using right hand IK for anything yet
                //CreateLimbReachMotor(lBoneController, true);
                CreateLimbReachMotor(lBoneController, false);
            }
        }


        public override bool OnInspectorGUI(Object rTarget)
        {
            bool lIsDirty = false;

            //GUILayout.BeginHorizontal();

            //if (EditorHelper.ObjectField("Root Transform", "The root transform of the character's skeleton", RootTransform, typeof(Transform), rTarget))
            //{
            //    lIsDirty = true;
            //    RootTransform = (Transform)EditorHelper.FieldObjectValue;
            //}

            //GUILayout.Space(10);

            if (EditorHelper.BoolField("Is Humanoid", "Does the model use a Humanoid rig?", IsHumanoidRig, rTarget))
            {
                lIsDirty = true;
                IsHumanoidRig = EditorHelper.FieldBoolValue;
            }

            //GUILayout.EndHorizontal();

            if (EditorHelper.BoolField("Set Joints", "Set up the bone joints using default settings and limits.", SetBoneJoints, rTarget))
            {
                lIsDirty = true;
                SetBoneJoints = EditorHelper.FieldBoolValue;
            }

            if (EditorHelper.BoolField("Create Colliders", "Create box 'pseudo-colliders' on the bones using the default settings.", CreateBoneColliders, rTarget))
            {
                lIsDirty = true;
                CreateBoneColliders = EditorHelper.FieldBoolValue;
            }

            if (EditorHelper.BoolField("Use Foot IK", "Create Foot To Ground motors to enable grounding IK.", UseFootIK, rTarget))
            {
                lIsDirty = true;
                UseFootIK = EditorHelper.FieldBoolValue;
            }

            if (UseFootIK)
            {                
                EditorGUI.indentLevel++;
                int lNewLayers = EditorHelper.LayerMaskField(new GUIContent("Ground Layers", "Layers that we'll use for Foot IK grounding"), GroundLayers);
                if (lNewLayers != GroundLayers)
                {
                    lIsDirty = true;
                    GroundLayers = lNewLayers;
                }
                EditorGUI.indentLevel--;
            }

            if (EditorHelper.BoolField("Use Hand IK", "Create Limb Reach motors to enable hand IK.", UseHandIK, rTarget))
            {
                lIsDirty = true;
                UseHandIK = EditorHelper.FieldBoolValue;
            }

            return lIsDirty;
        }        

        private void CreateFootToGroundMotor(BoneController rBoneController, bool rIsRightSide)
        {
            string lName = rIsRightSide ? RightFootMotorName : LeftFootMotorName;
            FootGround2BoneMotor lMotor = rBoneController.GetMotor<FootGround2BoneMotor>(lName);
            if (lMotor == null)
            {
                lMotor = new FootGround2BoneMotor(rBoneController) { Name = lName };                
            }
            lMotor.RotateFootOnMovement = true;
            lMotor.AllowLegExtension = true;
            lMotor.AutoLoadBones(rIsRightSide ? "Humanoid Right" : "Humanoid Left");
        }

        private void CreateLimbReachMotor(BoneController rBoneController, bool rIsRightSide)
        {
            string lName = rIsRightSide ? RightHandMotorName : LeftHandMotorName;
            LimbReachMotor lMotor = rBoneController.GetMotor<LimbReachMotor>(lName);
            if (lMotor == null)
            {
                lMotor = new LimbReachMotor(rBoneController) { Name = lName };
            }

            // Left Hand IK on weapon
            if (!rIsRightSide)
            {
                lMotor.Bone2Extension = 0.075f;
            }
            lMotor.AutoLoadBones(rIsRightSide ? "Humanoid Right Arm" : "Humanoid Left Arm");
        }
       
    }
}
#endif
