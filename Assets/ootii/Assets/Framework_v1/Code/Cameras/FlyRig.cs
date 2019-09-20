using UnityEngine;
using com.ootii.Actors;
using com.ootii.Geometry;
using com.ootii.Helpers;
using com.ootii.Input;
using com.ootii.Timing;

namespace com.ootii.Cameras
{
    /// <summary>
    /// Mostly a debug camera, but allow the player to fly around the scene.
    /// </summary>
    [AddComponentMenu("ootii/Camera Rigs/Fly Rig")]
    public class FlyRig : BaseCameraRig
    {
        /// <summary>
        /// GameObject that owns the IInputSource we really want
        /// </summary>
        public GameObject _InputSourceOwner = null;
        public GameObject InputSourceOwner
        {
            get { return _InputSourceOwner; }

            set
            {
                _InputSourceOwner = value;

                // Object that will provide access to the keyboard, mouse, etc
                if (_InputSourceOwner != null) { mInputSource = InterfaceHelper.GetComponent<IInputSource>(_InputSourceOwner); }
            }
        }

        /// <summary>
        /// Don't set an anchor
        /// </summary>
        public override Transform Anchor
        {
            get { return base.Anchor; }
            set { }
        }

        /// <summary>
        /// Speed for the flight camera
        /// </summary>
        public float _MoveSpeed = 5f;
        public float MoveSpeed
        {
            get { return _MoveSpeed; }
            set { _MoveSpeed = value; }
        }

        /// <summary>
        /// Speed factor for the flight camera
        /// </summary>
        public float _FastFactor = 3f;
        public float FastFactor
        {
            get { return _FastFactor; }
            set { _FastFactor= value; }
        }

        /// <summary>
        /// Speed factor for the flight camera
        /// </summary>
        public float _SlowFactor = 0.2f;
        public float SlowFactor
        {
            get { return _SlowFactor; }
            set { _SlowFactor = value; }
        }

        /// <summary>
        /// Speed factor when the mouse wheel is scrolled
        /// </summary>
        public float _ScrollFactor = 1f;
        public float ScrollFactor
        {
            get { return _ScrollFactor; }
            set { _ScrollFactor = value; }
        }

        /// <summary>
        /// Determines if we invert the pitch 
        /// </summary>
        public bool _InvertPitch = true;
        public bool InvertPitch
        {
            get { return _InvertPitch;  }
            set { _InvertPitch = value; }
        }

        /// <summary>
        /// Degrees per second the actor rotates
        /// </summary>
        public float _RotationSpeed = 120f;
        public virtual float RotationSpeed
        {
            get { return _RotationSpeed; }

            set
            {
                _RotationSpeed = value;
                mDegreesPer60FPSTick = _RotationSpeed / 60f;
            }
        }

        /// <summary>
        /// Speed we'll actually apply to the rotation. This is essencially the
        /// number of degrees per tick assuming we're running at 60 FPS
        /// </summary>
        protected float mDegreesPer60FPSTick = 1f;

        /// <summary>
        /// Represents the "pole" that the camera is attched to the anchor with. This pole
        /// is the direction from the anchor to the camera (in natural "up" space)
        /// </summary>
        protected Vector3 mToCameraDirection = Vector3.back;

        /// <summary>
        /// We keep track of the tilt so we can make small changes to it as the actor rotates.
        /// This is safter than trying to do a full rotation all at once which can cause odd
        /// rotations as we hit 180 degrees.
        /// </summary>
        protected Quaternion mTilt = Quaternion.identity;

        /// <summary>
        /// Provides access to the keyboard, mouse, etc.
        /// </summary>
        protected IInputSource mInputSource = null;

        /// <summary>
        /// Internal values to track the yaw and pitch over time
        /// </summary>
        private float mYaw = 0f;

        /// <summary>
        /// Internal values to track the yaw and pitch over time
        /// </summary>
        private float mPitch = 0f;

        /// <summary>
        /// Use this for initialization
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            if (_Anchor != null && this.enabled)
            {
                ICharacterController lController = InterfaceHelper.GetComponent<ICharacterController>(_Anchor.gameObject);
                if (lController != null)
                {
                    IsInternalUpdateEnabled = false;
                    IsFixedUpdateEnabled = false;
                    lController.OnControllerPostLateUpdate += OnControllerLateUpdate;
                }

                mTilt = QuaternionExt.FromToRotation(_Transform.up, _Anchor.up);

                mToCameraDirection = _Transform.position - _Anchor.position;
                mToCameraDirection.y = 0f;
                mToCameraDirection.Normalize();
            }

            // Object that will provide access to the keyboard, mouse, etc
            if (_InputSourceOwner != null) { mInputSource = InterfaceHelper.GetComponent<IInputSource>(_InputSourceOwner); }

            // Default the speed we'll use to rotate
            mDegreesPer60FPSTick = _RotationSpeed / 60f;
        }

        /// <summary>
        /// Use this for initialization
        /// </summary>
        protected override void Start()
        {
            mYaw = transform.rotation.eulerAngles.y;
            mPitch = transform.rotation.eulerAngles.x;

            base.Start();
        }

        /// <summary>
        /// Called when the component is enabled. This is also called after awake. So,
        /// we need to ensure we're not doubling up on the assignment.
        /// </summary>
        protected void OnEnable()
        {
            if (_Anchor != null)
            {
                ICharacterController lController = InterfaceHelper.GetComponent<ICharacterController>(_Anchor.gameObject);
                if (lController != null)
                {
                    if (lController.OnControllerPostLateUpdate != null) { lController.OnControllerPostLateUpdate -= OnControllerLateUpdate; }
                    lController.OnControllerPostLateUpdate += OnControllerLateUpdate;
                }
            }
        }

        /// <summary>
        /// Called when the component is disabled.
        /// </summary>
        protected void OnDisable()
        {
            if (_Anchor != null)
            {
                ICharacterController lController = InterfaceHelper.GetComponent<ICharacterController>(_Anchor.gameObject);
                if (lController != null && lController.OnControllerPostLateUpdate != null)
                {
                    lController.OnControllerPostLateUpdate -= OnControllerLateUpdate;
                }
            }
        }

        /// <summary>
        /// LateUpdate logic for the controller should be done here. This allows us
        /// to support dynamic and fixed update times
        /// </summary>
        /// <param name="rDeltaTime">Time since the last frame (or fixed update call)</param>
        /// <param name="rUpdateIndex">Index of the update to help manage dynamic/fixed updates. [0: Invalid update, >=1: Valid update]</param>
        public override void RigLateUpdate(float rDeltaTime, int rUpdateIndex)
        {
            // Get out if we're not in a valid update
            if (rUpdateIndex < 0) { return; }
            if (mInputSource == null) { return; }

            // Determine the linear movement
            Vector3 lMovement = Vector3.zero;
            Quaternion lRotation = _Transform.rotation;

            float lSpeed = _MoveSpeed;
            if (mInputSource.IsPressed(KeyCode.LeftShift)) { lSpeed *= _FastFactor; }
            if (mInputSource.IsPressed(KeyCode.Space)) { lSpeed *= _SlowFactor; }

            if (mInputSource.IsViewingActivated)
            {
                // Handle the rotations
                float lYaw = mInputSource.ViewX;
                float lYawMovement = lYaw * mDegreesPer60FPSTick;
                mYaw = (mYaw + lYawMovement) % 360f;

                float lPitch = mInputSource.ViewY * (_InvertPitch ? -1 : 1);
                float lPitchMovement = lPitch * mDegreesPer60FPSTick;
                mPitch = (mPitch + lPitchMovement) % 360f;

                lRotation = Quaternion.AngleAxis(mYaw, Vector3.up) * Quaternion.AngleAxis(mPitch, Vector3.right);

                // Handle the movement
                float lInputY = 0f;
                if (mInputSource.IsPressed(KeyCode.E)) { lInputY = 1f; }
                if (mInputSource.IsPressed(KeyCode.Q)) { lInputY = -1f; }

                float lInputX = 0f;
                if (mInputSource.IsPressed(KeyCode.D)) { lInputX = 1f; }
                if (mInputSource.IsPressed(KeyCode.A)) { lInputX = -1f; }

                float lInputZ = 0f;
                if (mInputSource.IsPressed(KeyCode.W)) { lInputZ = 1f; }
                if (mInputSource.IsPressed(KeyCode.S)) { lInputZ = -1f; }

                Vector3 lVelocity = new Vector3(lInputX, lInputY, lInputZ);
                lMovement = lRotation * (lVelocity * lSpeed * rDeltaTime);
            }
            // Deal with the scroll wheel
            else if (mInputSource.GetValue("Mouse ScrollWheel") != 0f)
            {
                float lInputZ = mInputSource.GetValue("Mouse ScrollWheel") * _ScrollFactor;

                Vector3 lVelocity = new Vector3(0f, 0f, lInputZ);
                lMovement = lRotation * (lVelocity * lSpeed * rDeltaTime);
            }

            if (_Anchor == null)
            {
                _Transform.rotation = lRotation;

                // Apply movement if it exists
                if (lMovement.sqrMagnitude > 0f)
                {
                    _Transform.position = _Transform.position + lMovement;
                }
            }
            else
            {
                
            }
        }

        /// <summary>
        /// Delegate callback for handling the camera movement AFTER the character controller
        /// </summary>
        /// <param name="rController"></param>
        /// <param name="rDeltaTime"></param>
        /// <param name="rUpdateIndex"></param>
        private void OnControllerLateUpdate(ICharacterController rController, float rDeltaTime, int rUpdateIndex)
        {
            RigLateUpdate(rDeltaTime, rUpdateIndex);

            // Call out to our events if needed
            if (mOnPostLateUpdate != null) { mOnPostLateUpdate(rDeltaTime, mUpdateIndex, this); }
        }
    }
}