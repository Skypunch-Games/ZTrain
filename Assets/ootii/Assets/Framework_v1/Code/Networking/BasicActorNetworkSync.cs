using UnityEngine;
using UnityEngine.Networking;
using com.ootii.Actors;
using com.ootii.Actors.AnimationControllers;
using com.ootii.Geometry;

namespace com.ootii.Networking
{
#if UNITY5 || UNITY_2017 || UNITY_2018
    /// <summary>
    /// Used to syncronize the actors across the network. This is sometimes needed due to
    /// fast changing animator parameters.
    /// 
    /// NOTE: This is a basic network sync and can be used for small network games or as an example
    /// </summary>
    [RequireComponent(typeof(NetworkIdentity))]
    public class BasicActorNetworkSync : NetworkBehaviour
    {
        /// <summary>
        /// Updates to send per second
        /// </summary>
        public int _NetworkSendRate = 11;
        public int NetworkSendRate
        {
            get { return _NetworkSendRate; }

            set
            {
                if (value <= 0) { return; }

                _NetworkSendRate = value;
                mSendDelay = 1f / (float)_NetworkSendRate;
            }
        }

        /// <summary>
        /// Determines if we sync the position
        /// </summary>
        public bool _SyncPosition = true;
        public bool SyncPosition
        {
            get { return _SyncPosition; }
            set { _SyncPosition = value; }
        }

        /// <summary>
        /// Determines if we sync the rotation
        /// </summary>
        public bool _SyncRotation = true;
        public bool SyncRotation
        {
            get { return _SyncRotation; }
            set { _SyncRotation = value; }
        }

        /// <summary>
        /// Factor to multiply the Time.deltaTime lerp value by
        /// </summary>
        public float _SyncFactor = 0.2f;
        public float SyncFactor
        {
            get { return _SyncFactor; }
            set { _SyncFactor = value; }
        }

        /// <summary>
        /// Determines if we sync the motion phase IDs of the animator
        /// </summary>
        public bool _SyncMotionPhase = true;
        public bool SyncMotionPhase
        {
            get { return _SyncMotionPhase; }
            set { _SyncMotionPhase = value; }
        }

        /// <summary>
        /// Time in seconds a phase will stay before we can change it again. This
        /// gives time for the animator to pick up the previous value.
        /// </summary>
        public float _PhaseChangeDelay = 0.075f;
        public float PhaseChangeDelay
        {
            get { return _PhaseChangeDelay; }
            set { _PhaseChangeDelay = value; }
        }

        /// <summary>
        /// Determines if we sync the additional MC animator parameters
        /// </summary>
        public bool _SyncAnimatorParams = true;
        public bool SyncAnimatorParams
        {
            get { return _SyncAnimatorParams; }
            set { _SyncAnimatorParams = value; }
        }

        // Transform that is this actor
        protected Transform mTransform = null;

        // Animator associated with the character
        protected Animator mAnimator = null;

        // Animator parameters that the local sends to the server
        protected ActorNetworkState[] mLocalState = new ActorNetworkState[60];

        // Animator parameters that remotes get back from the server
        protected ActorNetworkState[] mRemoteState = null;

        // Last time of the server parameter that was processed
        protected float mLastServerParamTime = 0f;

        // Time in seconds to delay before sending again
        protected float mSendDelay = 1f / 11f;

        // Time since the last send
        protected float mSendElapsedTime = 0f;

        // Time before we'll change the motion phase
        protected float mPhaseElapsedTime = 0f;

        /// <summary>
        /// An internal method called on client (remote) objects to resolve GameObject references.
        /// 
        /// It is not safe to put user code in this function as it may be replaced by the 
        /// network system's code generation process.
        /// </summary>
        public override void PreStartClient()
        {
            //com.ootii.Utilities.Debug.Log.FileWrite(Time.time.ToString("f3") + " " + gameObject.name + "[" + gameObject.GetInstanceID() + "].PreStartClient() isLocalPlayer:" + isLocalPlayer + " isServer:" + isServer);

            // Tell the remote player that we want to auto recieve parameters
            NetworkAnimator lNetworkAnimator = gameObject.GetComponent<NetworkAnimator>();
            if (lNetworkAnimator != null && lNetworkAnimator.enabled)
            {
                for (int i = 0; i < 14; i++)
                {
                    lNetworkAnimator.SetParameterAutoSend(i, true);
                }
            }
        }

        /// <summary>
        /// Called on every NetworkBehaviour when it is activated on a client.
        /// </summary>
        public override void OnStartClient()
        {
            //com.ootii.Utilities.Debug.Log.FileWrite(Time.time.ToString("f3") + " " + gameObject.name + "[" + gameObject.GetInstanceID() + "].OnStartClient() isLocalPlayer:" + isLocalPlayer + " isServer:" + isServer);

            // Store the animator for future use
            MotionController lMotionController = gameObject.GetComponent<MotionController>();
            mAnimator = lMotionController.Animator;
        }

        /// <summary>
        /// Called when the local player object has been set up.
        /// 
        /// This happens after OnStartClient(), as it is triggered by an ownership message from the server.
        /// This is an appropriate place to activate components or functionality that should only be active for 
        /// the local player, such as cameras and input.
        /// </summary>
        public override void OnStartLocalPlayer()
        {
            //com.ootii.Utilities.Debug.Log.FileWrite(Time.time.ToString("f3") + " " + gameObject.name + "[" + gameObject.GetInstanceID() + "].OnStartLocalPlayer() isLocalPlayer:" + isLocalPlayer + " isServer:" + isServer);

            // Tell the local player that we want to auto send parameters
#if UNITY_5_5 || UNITY_5_6
            int lParamCount = (mAnimator != null ? mAnimator.parameterCount : 14);
#else
            int lParamCount = 14;
#endif

            NetworkAnimator lNetworkAnimator = gameObject.GetComponent<NetworkAnimator>();
            if (lNetworkAnimator != null && lNetworkAnimator.enabled)
            {
                for (int i = 0; i < lParamCount; i++)
                {
                    lNetworkAnimator.SetParameterAutoSend(i, true);
                }
            }

            // Initialize the params array
            for (int i = 0; i < mLocalState.Length; i++)
            {
                mLocalState[i] = new ActorNetworkState();
            }
        }

        /// <summary>
        /// Called once when the component is initialized
        /// </summary>
        private void Start()
        {
            //com.ootii.Utilities.Debug.Log.FileWrite(Time.time.ToString("f3") + " " + gameObject.name + "[" + gameObject.GetInstanceID() + "].Start() isLocalPlayer:" + isLocalPlayer + " isServer:" + isServer);

            mTransform = gameObject.transform;

            mSendDelay = 1f / (float)_NetworkSendRate;

            if (isLocalPlayer)
            {
                LocalStart();
            }
            else
            {
                RemoteStart();
            }
        }

        /// <summary>
        /// Runs the update both locally and remotely
        /// </summary>
        protected void Update()
        {
            if (isLocalPlayer)
            {
                LocalUpdate();
            }
            else
            {
                RemoteUpdate();
            }
        }

        /// <summary>
        /// Called to initialize the actor when it's the local actor
        /// </summary>
        protected void LocalStart()
        {
            // Disable the 'Use Transform' option
            ActorController lActorController = gameObject.GetComponent<ActorController>();
            lActorController.enabled = true;
            lActorController.UseTransformPosition = false;
            lActorController.UseTransformRotation = false;

            // Grab the MC and ensure it's enabled
            MotionController lMotionController = gameObject.GetComponent<MotionController>();
            lMotionController.enabled = true;

            if (lMotionController.CameraRig != null)
            {
                lMotionController.CameraRig.Anchor = lMotionController._Transform;
            }
        }

        /// <summary>
        /// Called to initialize the actor when it's a remote actor
        /// </summary>
        protected void RemoteStart()
        {
            // Disable the 'Use Transform' option
            ActorController lActorController = gameObject.GetComponent<ActorController>();
            lActorController.enabled = true;
            lActorController.UseTransformPosition = true;
            lActorController.UseTransformRotation = true;

            // Grab the MC and ensure it is not enabled
            MotionController lMotionController = gameObject.GetComponent<MotionController>();
            lMotionController.enabled = false;
        }

        /// <summary>
        /// Update function run on the local instance
        /// </summary>
        protected void LocalUpdate()
        {
            // Shift the array to the right
            ActorNetworkState lLastParam = mLocalState[mLocalState.Length - 1];
            for (int i = mLocalState.Length - 1; i > 0; i--)
            {
                mLocalState[i] = mLocalState[i - 1];
            }

            // Add the new param
            lLastParam.Time = Time.time;

            if (_SyncPosition) { lLastParam.Position = mTransform.position; }
            if (_SyncRotation) { lLastParam.Rotation = mTransform.rotation; }

            if (_SyncMotionPhase)
            {
                lLastParam.L0MotionPhase = mAnimator.GetInteger("L0MotionPhase");
                lLastParam.L0MotionForm = mAnimator.GetInteger("L0MotionForm");

                lLastParam.L1MotionPhase = mAnimator.GetInteger("L1MotionPhase");
                lLastParam.L1MotionForm = mAnimator.GetInteger("L1MotionForm");
            }

            if (_SyncAnimatorParams)
            {
                lLastParam.IsGrounded = mAnimator.GetBool("IsGrounded");
                lLastParam.Stance = mAnimator.GetInteger("Stance");
                lLastParam.InputX = mAnimator.GetFloat("InputX");
                lLastParam.InputY = mAnimator.GetFloat("InputY");
                lLastParam.InputMagnitude = mAnimator.GetFloat("InputMagnitude");
                lLastParam.InputMagnitudeAvg = mAnimator.GetFloat("InputMagnitudeAvg");
                lLastParam.InputAngleFromAvatar = mAnimator.GetFloat("InputAngleFromAvatar");
                lLastParam.InputAngleFromCamera = mAnimator.GetFloat("InputAngleFromCamera");
                lLastParam.L0MotionParameter = mAnimator.GetInteger("L0MotionParameter");
                lLastParam.L0MotionStateTime = mAnimator.GetFloat("L0MotionStateTime");
                lLastParam.L1MotionParameter = mAnimator.GetInteger("L1MotionParameter");
                lLastParam.L1MotionStateTime = mAnimator.GetFloat("L1MotionStateTime");
            }

            mLocalState[0] = lLastParam;

            // Determine if its time to send the update
            mSendElapsedTime = mSendElapsedTime + Time.deltaTime;
            if (mSendElapsedTime >= mSendDelay)
            {
                mSendElapsedTime = 0f;
                CmdUpdateActor(mLocalState);
            }
        }

        /// <summary>
        /// Update function run on the remote instance
        /// </summary>
        protected void RemoteUpdate()
        {
            if (mRemoteState != null && mRemoteState.Length > 0)
            {
                int lStateIndex = 0;
                ActorNetworkState lNetworkState = mRemoteState[lStateIndex];

                mPhaseElapsedTime = mPhaseElapsedTime + Time.deltaTime;

                if (_SyncPosition)
                {
                    mTransform.position = Vector3.Lerp(mTransform.position, mRemoteState[lStateIndex].Position, _SyncFactor);
                }

                if (_SyncRotation)
                {
                    mTransform.rotation = Quaternion.Lerp(mTransform.rotation, mRemoteState[lStateIndex].Rotation, _SyncFactor);
                }

                // Set the current animator parameters
                int lL0MotionPhase = mAnimator.GetInteger("L0MotionPhase");
                int lL1MotionPhase = mAnimator.GetInteger("L1MotionPhase");

                // Cycle through the states (oldest to newest) to find the first unprocessed state
                for (int i = mRemoteState.Length - 1; i >= 0; i--)
                {
                    if (mRemoteState[i].Time <= mLastServerParamTime) { continue; }

                    bool lStop = false;

                    // If the value is different than our current parameter, set it
                    if (mRemoteState[i].L0MotionPhase != lL0MotionPhase)
                    {
                        if (_PhaseChangeDelay <= 0f || mPhaseElapsedTime > _PhaseChangeDelay)
                        {
                            lStop = true;
                            lNetworkState = mRemoteState[i];

                            mPhaseElapsedTime = 0f;

                            if (_SyncMotionPhase)
                            {
                                mAnimator.SetInteger("L0MotionPhase", lNetworkState.L0MotionPhase);
                                mAnimator.SetInteger("L0MotionForm", lNetworkState.L0MotionForm);
                            }
                        }
                    }

                    // If the value is different than our current parameter, set it
                    if (mRemoteState[i].L1MotionPhase != lL1MotionPhase)
                    {
                        lStop = true;

                        if (_SyncMotionPhase)
                        {
                            mAnimator.SetInteger("L1MotionPhase", mRemoteState[i].L1MotionPhase);
                            mAnimator.SetInteger("L1MotionForm", mRemoteState[i].L1MotionForm);
                        }
                    }

                    if (lStop)
                    {
                        mLastServerParamTime = mRemoteState[i].Time;
                        break;
                    }
                }

                if (_SyncAnimatorParams)
                {
                    mAnimator.SetBool("IsGrounded", lNetworkState.IsGrounded);
                    mAnimator.SetInteger("Stance", lNetworkState.Stance);
                    mAnimator.SetFloat("InputX", lNetworkState.InputX);
                    mAnimator.SetFloat("InputY", lNetworkState.InputY);
                    mAnimator.SetFloat("InputMagnitude", lNetworkState.InputMagnitude);
                    mAnimator.SetFloat("InputMagnitudeAvg", lNetworkState.InputMagnitudeAvg);
                    mAnimator.SetFloat("InputAngleFromAvatar", lNetworkState.InputAngleFromAvatar);
                    mAnimator.SetFloat("InputAngleFromCamera", lNetworkState.InputAngleFromCamera);
                    mAnimator.SetInteger("L0MotionParameter", lNetworkState.L0MotionParameter);
                    mAnimator.SetFloat("L0MotionStateTime", lNetworkState.L0MotionStateTime);
                    mAnimator.SetInteger("L1MotionParameter", lNetworkState.L1MotionParameter);
                    mAnimator.SetFloat("L1MotionStateTime", lNetworkState.L1MotionStateTime);
                }
            }            
        }

        /// <summary>
        /// Called by a client, but executed on the server
        /// </summary>
        /// <param name="rName"></param>
        [Command]
        private void CmdUpdateActor(ActorNetworkState[] rParams)
        {
            // Pass the animator parameters down to the clients
            RpcRemoteActorUpdated(rParams);

            //com.ootii.Utilities.Debug.Log.FileScreenWrite(Time.time.ToString("f3") + " " + gameObject.name + ":" + gameObject.GetInstanceID() + " CmdAnimatorUpdate:" + rParams[0].L0MotionPhase, mDebugStart + 6);
        }

        /// <summary>
        /// Called by the server, but executed on each client
        /// </summary>
        /// <param name="rName"></param>
        [ClientRpc]
        public void RpcRemoteActorUpdated(ActorNetworkState[] rParams)
        {
            if (isLocalPlayer) { return; }

            // Only remote clients store these parameters
            mRemoteState = rParams;

            //com.ootii.Utilities.Debug.Log.FileWrite(Time.time.ToString("f3") + " " + gameObject.name + ":" + gameObject.GetInstanceID() + " time:" + mRemoteParams[0].Time.ToString("f3"));
        }
    }
#endif
}