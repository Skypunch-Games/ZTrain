using System;
using UnityEngine;
using com.ootii.Cameras;
using com.ootii.Geometry;

namespace com.ootii.Setup
{
    /// <summary>
    /// Helper functions for setting up a camera rig
    /// </summary>
    public class CameraSetupHelper
    {
#if UNITY_EDITOR      
        /// <summary>
        /// Instantiate a camera rig using a prefab
        /// </summary>
        /// <param name="rCameraPrefab"></param>
        /// <returns></returns>
        public static BaseCameraRig InstantiateCamera(BaseCameraRig rCameraPrefab)
        {
            if (rCameraPrefab == null) { return null; }

            //// If there is another main camera in the scene, disable it first to avoid conflicts
            //Camera lMainCamera = Camera.main;
            //if (lMainCamera != null)
            //{
            //    lMainCamera.gameObject.SetActive(false);
            //    Transform lParent = lMainCamera.transform.parent;
            //    if (lParent != null)
            //    {
            //        lParent.gameObject.SetActive(false);
            //    }
            //}

            BaseCameraRig lCameraRig = GameObject.Instantiate(rCameraPrefab);

            if (lCameraRig != null)
            {
                lCameraRig.name = rCameraPrefab.name;
                lCameraRig.Transform.Reset();
            }
            return lCameraRig;
        }

        /// <summary>
        /// Create the camera rig, if none already exists.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T CreateCameraRig<T>() where T : BaseCameraRig
        {
            try
            {
                // Get or Create the Camera Rig
                GameObject lCameraRigGO = CreateRigGameObject();

                // Check if the camera rig is assigned
                T lCameraRig = lCameraRigGO.GetComponent(typeof(T)) as T;
                if (lCameraRig == null) { lCameraRig = lCameraRigGO.AddComponent(typeof(T)) as T; }

                if (lCameraRig != null)
                {
                    lCameraRig.enabled = true;
                }

                // Return the rig
                return lCameraRig;
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }

            return null;
        }

        /// <summary>
        /// Create the camera rig game object (to which the camera is parented)
        /// </summary>
        /// <returns></returns>
        private static GameObject CreateRigGameObject()
        {
            GameObject lCameraRigGO = null;
            // Find the camera
            //Camera lCamera = Object.FindObjectOfType<Camera>();
            //if (lCamera != null) { lCamera.nearClipPlane = 0.1f; }

            // Get the "MainCamera" Camera object; if it doesn't exist, create one
            GameObject lCameraGO = GetMainCamera();
            if (lCameraGO == null) { return null; }

            // Grab the camera's parent
            if (lCameraGO.transform.parent != null)
            {
                lCameraRigGO = lCameraGO.transform.parent.gameObject;
            }

            if (lCameraRigGO == null)
            {
                lCameraRigGO = new GameObject("Camera Rig");
                lCameraRigGO.transform.position = lCameraGO.transform.position;
                lCameraRigGO.transform.rotation = lCameraGO.transform.rotation;
            }

            // Parent the Camera to the rig and reset its local transform
            lCameraGO.name = "Main Camera";
            lCameraGO.transform.parent = lCameraRigGO.transform;
            lCameraGO.transform.localScale = Vector3.one;
            lCameraGO.transform.localRotation = Quaternion.identity;
            lCameraGO.transform.localPosition = Vector3.zero;

            // Disable any rigs that currently exist on the object
            BaseCameraRig[] lCameraRigs = lCameraRigGO.GetComponents<BaseCameraRig>();
            for (int i = 0; i < lCameraRigs.Length; i++)
            {
                lCameraRigs[i].enabled = false;
            }

            return lCameraRigGO;
        }

        /// <summary>
        /// Find the (first) camera tagged "MainCamera" and return its GameObject
        /// </summary>
        /// <returns></returns>
        private static GameObject GetMainCamera()
        {
            GameObject lCameraGO = GameObject.FindGameObjectWithTag("MainCamera");
            if (lCameraGO == null)
            {
                Camera lCamera = GameObject.FindObjectOfType<Camera>();
                if (lCamera != null)
                {
                    lCameraGO = lCamera.gameObject;
                }
                else
                {
                    // Create a new GameObject and add a Camera component if we haven't found a camera yet.
                    lCameraGO = new GameObject("Main Camera");
                    lCameraGO.transform.position = Vector3.zero;
                    lCameraGO.transform.rotation = Quaternion.identity;

                    lCameraGO.AddComponent<Camera>();
                }

                lCameraGO.tag = "MainCamera";

                //// Disable any others tagged "MainCamera" to avoid conflicts
                //List<GameObject> lOtherCameraObjects = GameObject.FindGameObjectsWithTag("MainCamera").ToList();
                //foreach (GameObject lGO in lOtherCameraObjects)
                //{
                //    if (lGO == lCameraGO) { continue; }
                //    lGO.SetActive(false);
                //    if (lGO.transform.parent != null)
                //    {
                //        lGO.transform.parent.gameObject.SetActive(false);
                //    }
                //}
            }


            return lCameraGO;
        }

        /// <summary>
        /// Find the camera rig in the scene
        /// </summary>        
        /// <returns></returns>
        public static BaseCameraRig FindSceneCameraRig()
        {
            BaseCameraRig[] lFoundRigs = null;

            // Search for all Active camera rig objecets in the scene
            lFoundRigs = GameObject.FindObjectsOfType<BaseCameraRig>();
            if (lFoundRigs != null)
            {
                // We found at least one camera rig; 
                foreach (BaseCameraRig lFoundRig in lFoundRigs)
                {
#if OOTII_CC
                    // If CameraController is present, assume that we don't want to use a basic OrbitRig or FollowRig
                    if (!lFoundRig.GetType().IsAssignableFrom(typeof(CameraController)))
                    {
                        continue;
                    }
#endif

                    Camera lCamera = lFoundRig.GetComponentInChildren<Camera>();

                    // Return the first camera rig where the camera is tagged as main
                    if (lCamera != null && Camera.main == lCamera)
                    {
                        return lFoundRig;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Get an existing Base Camera Anchor in the scene, or create a new one if none are found
        /// </summary>
        /// <param name="rCameraAnchorGO"></param>
        /// <returns></returns>
        public static BaseCameraAnchor GetOrCreateCameraAnchor(out GameObject rCameraAnchorGO)
        {            
            BaseCameraAnchor lBaseCameraAnchor = null;
            BaseCameraAnchor[] lFoundAnchors = GameObject.FindObjectsOfType<BaseCameraAnchor>();
            if (lFoundAnchors.Length > 0)
            {
                // Use the first found camera anchor
                lBaseCameraAnchor = lFoundAnchors[0];
                lBaseCameraAnchor.IsFollowingEnabled = true;
                rCameraAnchorGO = lBaseCameraAnchor.gameObject;
                rCameraAnchorGO.SetActive(true);

                // Disable the rest
                for (int i = 1; i < lFoundAnchors.Length; i++)
                {
                    lFoundAnchors[i].IsFollowingEnabled = false;
                    lFoundAnchors[i].gameObject.SetActive(false);
                }                                

                return lBaseCameraAnchor;
            }
            
            // None found; create a new Camera Anchor object            
            rCameraAnchorGO = new GameObject("Camera Anchor");
            rCameraAnchorGO.transform.Reset();
            lBaseCameraAnchor = rCameraAnchorGO.AddComponent<BaseCameraAnchor>();
            lBaseCameraAnchor.IsFollowingEnabled = true;
            
            return lBaseCameraAnchor;
        }

        /// <summary>
        /// Disable any Base Camera Anchors found in the scene
        /// </summary>
        /// <param name="rDisableGameObject"></param>
        public static void DisableCameraAnchors(bool rDisableGameObject = true)
        {
            BaseCameraAnchor[] lFoundAnchors = GameObject.FindObjectsOfType<BaseCameraAnchor>();
            if (lFoundAnchors != null)
            {
                foreach (var lAnchor in lFoundAnchors)
                {
                    lAnchor.IsFollowingEnabled = false;
                    if (rDisableGameObject)
                    {
                        lAnchor.gameObject.SetActive(false);
                    }
                }
            }
        }

#if OOTII_CC

        #region Camera Controller        

        /// <summary>
        /// Configure the camera rig with the default Third Person Camera settings
        /// </summary>
        /// <param name="rBaseCameraRig"></param>
        public static void SetupThirdPersonCamera(BaseCameraRig rBaseCameraRig)
        {
            CameraController lController = (CameraController)rBaseCameraRig;
            if (lController == null) { return; }

            lController.AnchorOffset = new Vector3(0, 1.8f, 0);

            // First disable all camera motors so they can be reset
            for (int i = 0; i < lController.Motors.Count; i++)
            {
                lController.Motors[i]._IsActive = false;
                lController.Motors[i].IsEnabled = false;

                if (i < lController.MotorDefinitions.Count)
                {
                    lController.MotorDefinitions[i] = lController.Motors[i].SerializeMotor();
                }
            }

            lController.IsCollisionsEnabled = true;

            // Follow motor
            OrbitFollowMotor lMotor = lController.GetMotor<OrbitFollowMotor>("3rd Person Follow");
            if (lMotor == null)
            {
                lMotor = new OrbitFollowMotor();
                lController.Motors.Add(lMotor);
                lController.MotorDefinitions.Add("");

                lMotor.Name = "3rd Person Follow";
                lMotor.RigController = lController;
                lMotor.MaxDistance = 3f;
            }

            lMotor.IsEnabled = true;
            lController.EditorMotorIndex = lController.Motors.IndexOf(lMotor);
            lController.MotorDefinitions[lController.EditorMotorIndex] = lMotor.SerializeMotor();

            lController._ActiveMotorIndex = lController.EditorMotorIndex;

            // Fixed motor
            OrbitFixedMotor lMotor2 = lController.GetMotor<OrbitFixedMotor>("3rd Person Fixed");
            if (lMotor2 == null)
            {
                lMotor2 = new OrbitFixedMotor();
                lController.Motors.Add(lMotor2);
                lController.MotorDefinitions.Add("");

                lMotor2.Name = "3rd Person Fixed";
                lMotor2.RigController = lController;
                lMotor2.MaxDistance = 3f;
            }

            lMotor2.IsEnabled = true;
            lController.MotorDefinitions[lController.Motors.IndexOf(lMotor2)] = lMotor2.SerializeMotor();

            // Targeting motor
            OrbitFixedMotor lMotor3 = lController.GetMotor<OrbitFixedMotor>("Targeting");
            if (lMotor3 == null)
            {
                lMotor3 = new OrbitFixedMotor();
                lController.Motors.Add(lMotor3);
                lController.MotorDefinitions.Add("");

                lMotor3.Name = "Targeting";
                lMotor3.RigController = lController;
                lMotor3.Offset = new Vector3(0.5f, 0f, 0f);
                lMotor3.MaxDistance = 1f;
                lMotor3.RotateAnchor = true;
                lMotor3.RotateAnchorAlias = "Camera Rotate Character";
            }

            lMotor3.IsEnabled = true;
            lController.MotorDefinitions[lController.Motors.IndexOf(lMotor3)] = lMotor3.SerializeMotor();

            // Targeting In transition
            TransitionMotor lTransition = lController.GetMotor<TransitionMotor>("Targeting In");
            if (lTransition == null)
            {
                lTransition = new TransitionMotor();
                lController.Motors.Add(lTransition);
                lController.MotorDefinitions.Add("");

                lTransition.Name = "Targeting In";
                lTransition.RigController = lController;
                lTransition.ActionAlias = "Camera Aim";
                lTransition.ActionAliasEventType = 0;
                lTransition.StartMotorIndex = 0;
                lTransition.EndMotorIndex = 2;
                lTransition.TransitionTime = 0.15f;
                lTransition.ActorStances = "2,10,15";
            }

            lTransition.IsEnabled = true;
            lController.MotorDefinitions[lController.Motors.IndexOf(lTransition)] = lTransition.SerializeMotor();

            // Targeting out transition
            TransitionMotor lTransition2 = lController.GetMotor<TransitionMotor>("Targeting Out");
            if (lTransition2 == null)
            {
                lTransition2 = new TransitionMotor();
                lController.Motors.Add(lTransition2);
                lController.MotorDefinitions.Add("");

                lTransition2.Name = "Targeting Out";
                lTransition2.RigController = lController;
                lTransition2.ActionAlias = "Camera Aim";
                lTransition2.ActionAliasEventType = 1;
                lTransition2.StartMotorIndex = 2;
                lTransition2.EndMotorIndex = 0;
                lTransition2.TransitionTime = 0.25f;
            }

            lTransition2.IsEnabled = true;
            lController.MotorDefinitions[lController.Motors.IndexOf(lTransition2)] = lTransition2.SerializeMotor();

            // Set to "Advanced" properties
            lController.EditorTabIndex = 1;
        }

        /// <summary>
        /// Create a camera motor of the specified type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="rMotorName"></param>
        /// <param name="rController"></param>
        /// <returns></returns>
        public static T CreateCameraMotor<T>(string rMotorName, CameraController rController) where T : CameraMotor
        {
            try
            {
                T lMotor = rController.GetMotor<T>(rMotorName);
                if (lMotor == null)
                {
                    lMotor = Activator.CreateInstance(typeof(T)) as T;
                    rController.Motors.Add(lMotor);
                    rController.MotorDefinitions.Add("");

                    if (lMotor != null)
                    {
                        lMotor.Name = rMotorName;
                        lMotor.RigController = rController;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }

            return null;
        }

        #endregion Camera Controller

#endif

#endif
    }

}