using System;
using UnityEngine;
using System.Linq;
using com.ootii.Actors.AnimationControllers;
using com.ootii.Helpers;
using com.ootii.Input;

namespace com.ootii.Setup
{
    /// <summary>
    /// Helper functions for managing input settings and input sources
    /// </summary>
    public class InputSetupHelper
    {
#if UNITY_EDITOR
        /// <summary>
        /// View Activator options for IInputSource
        /// </summary>
        public static string[] Activators = { "None", "Left Mouse Button", "Right Mouse Button", "Left or Right Mouse Button" };

        /// <summary>
        /// Check if the default Motion Controller Input Manager settings have been applied
        /// </summary>
        /// <returns></returns>
        public static bool HasDefaultInputSettings()
        {
            return InputManagerHelper.IsDefined("ActivateRotation");
        }

        /// <summary>
        /// Get a reference to the Input Source already in the scene, or create a new one if needed
        /// </summary>
        /// <param name="rViewActivatorOption"></param>
        /// <returns></returns>
        public static GameObject GetOrCreateInputSource(int rViewActivatorOption)
        {
            // Find or create the input source; will use Easy Input if it is installed
            GameObject lInputSourceGO = null;
            IInputSource lInputSource = InputSetupHelper.CreateInputSource("com.ootii.Input.EasyInputSource, " + AssemblyHelper.AssemblyInfo,
                    ref lInputSourceGO);

            if (lInputSource == null) { lInputSource = InputSetupHelper.CreateInputSource<UnityInputSource>(ref lInputSourceGO); }
            ReflectionHelper.SetProperty(lInputSource, "ViewActivator", rViewActivatorOption);

            return lInputSourceGO;
        }

        /// <summary>
        /// Create an input source if needed
        /// </summary>        
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IInputSource CreateInputSource<T>(ref GameObject rInputSourceGO) where T : IInputSource
        {
            // Look for existing input source of the 
            IInputSource[] lInputSources = InterfaceHelper.GetComponents<IInputSource>();
            if (lInputSources != null && lInputSources.Length > 0)
            {
                rInputSourceGO = ((MonoBehaviour)lInputSources[0]).gameObject;
                if (lInputSources[0].GetType() == typeof(T))
                {
                    return lInputSources[0];
                }
                // Object has in input source, but it's a different type, so we'll destroy the object and recreate it
                GameObject.Destroy(rInputSourceGO);
            }

            // Create the input source
            rInputSourceGO = new GameObject("Input Source");
            T lInputSource = (T)((object)rInputSourceGO.AddComponent(typeof(T)));

            return lInputSource;
        }

        /// <summary>
        /// Creates an input source of the specified type (if needed)
        /// </summary>
        /// <param name="rType"></param>        
        /// <returns></returns>
        public static IInputSource CreateInputSource(string rType, ref GameObject rInputSourceGO)
        {
            if (!ReflectionHelper.IsTypeValid(rType)) { return null; }

            Type lType = Type.GetType(rType);

            IInputSource[] lInputSources = InterfaceHelper.GetComponents<IInputSource>();
            if (lInputSources != null && lInputSources.Length > 0)
            {
                rInputSourceGO = ((MonoBehaviour)lInputSources[0]).gameObject;
                if (lInputSources[0].GetType() == lType)
                {
                    return lInputSources[0];
                }
                // Object has in input source, but it's a different type, so we'll destroy the object and recreate it
                GameObject.Destroy(rInputSourceGO);
            }

            // Create the input source
            rInputSourceGO = new GameObject("Input Source");
            IInputSource lInputSource = rInputSourceGO.AddComponent(lType) as IInputSource;

            return lInputSource;
        }

        /// <summary>
        /// Create the Input Manager Settings for each Motion assigned to the Motion Controller instance
        /// </summary>
        /// <param name="rMotionController"></param>
        public static void CreateDefaultInputSettings(MotionController rMotionController)
        {
            CreateInputSetting("Change Stance", "t");
            CreateInputSetting("Run", "left shift");
            CreateInputSetting("Interact", "f");
            CreateInputSetting("Cover Toggle", "m");
            CreateInputSetting("ActivateRotation", 0);

#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
            CreateAxisInputSetting("Run", 5);
            CreateAxisInputSetting("ActivateRotation", 3);
#else
            CreateAxisInputSetting("Run", 9);
            CreateAxisInputSetting("ActivateRotation", 4);
#endif

            if (rMotionController == null || rMotionController.MotionLayers == null) { return; }

            foreach (MotionControllerMotion lMotion in rMotionController.MotionLayers.SelectMany(lMotionLayer => lMotionLayer.Motions))
            {
                lMotion.CreateInputManagerSettings();
            }
        }


        public static void CreateDefaultCombatAliases()
        {
#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
            CreateInputSetting("Combat Attack", "left ctrl", 0, 16);
            CreateInputSetting("Combat Block", "left alt", 1, 13);
            CreateInputSetting("Combat Lock", "y", 2, 12);
            CreateInputSetting("Dodge", "q");
            //CreateInputSetting("Draw Weapon", "tab");
#else
            CreateInputSetting("Combat Attack", "left ctrl", 0, 0);
            CreateInputSetting("Combat Block", "left alt", 1, 4);
            CreateInputSetting("Combat Lock", "y", 2, 9);
            CreateInputSetting("Dodge", "q");
            //CreateInputSetting("Draw Weapon", "tab");
#endif
        }


        public static void CreateInputSetting(string rName, string rKey, uint rMouseButton, uint rJoystickButton)
        {
            CreateInputSetting_Internal(rName, rKey, "mouse " + rMouseButton, (int)rJoystickButton);
        }
        /// <summary>
        /// Create an Input Manager setting (for a key)
        /// Defaults to Gravity = 1000; Dead = 0.001; Sensitivity = 1000
        /// </summary>
        /// <param name="rName"></param>
        /// <param name="rKey"></param>
        /// <param name="rAltKey"></param>
        public static void CreateInputSetting(string rName, string rKey, string rAltKey = "")
        {
            CreateInputSetting_Internal(rName, rKey, rAltKey);
        }

        /// <summary>
        /// Create an Input Manager setting for a key and a mouse button
        /// Defaults to Gravity = 1000; Dead = 0.001; Sensitivity = 1000
        /// </summary>
        /// <param name="rName"></param>
        /// <param name="rKey"></param>
        /// <param name="rMouseButton"></param>
        public static void CreateInputSetting(string rName, string rKey, uint rMouseButton)
        {
            CreateInputSetting_Internal(rName, rKey, "mouse " + rMouseButton);
        }

        /// <summary>
        /// Create an Input Manager setting for a mouse button
        /// Defaults to Gravity = 1000; Dead = 0.001; Sensitivity = 1000
        /// </summary>
        /// <param name="rName"></param>
        /// <param name="rMouseButton"></param>
        public static void CreateInputSetting(string rName, uint rMouseButton)
        {
            CreateInputSetting_Internal(rName, string.Empty, "mouse " + rMouseButton);
        }

        /// <summary>
        /// (Internal) Create the Input Manager entry for keys, mouse buttons, and joystick buttons
        ///  Defaults to Gravity = 1000; Dead = 0.001; Sensitivity = 1000
        /// </summary>
        /// <param name="rName">Name of the Input Manager setting (input alias)</param>
        /// <param name="rKey">The primary key</param>
        /// <param name="rAltKey">The alternate key (or mouse button)</param>
        /// <param name="rJoystickButton">(Optional) The joystick button</param>
        private static void CreateInputSetting_Internal(string rName, string rKey, string rAltKey, int rJoystickButton = -1)
        {
            if (InputManagerHelper.IsDefined(rName)) { return; }

            // Keyboard and/or mouse button
            InputManagerEntry lEntry = new InputManagerEntry
            {
                Name = rName,
                PositiveButton = rKey,
                AltPositiveButton = rAltKey,
                Gravity = 1000,
                Dead = 0.001f,
                Sensitivity = 1000,
                Type = InputManagerEntryType.KEY_MOUSE_BUTTON,
                Axis = 0,
                JoyNum = 0
            };
            InputManagerHelper.AddEntry(lEntry, true);

            // Joystick button
            if (rJoystickButton <= -1) { return; }
            lEntry = new InputManagerEntry
            {
                Name = rName,
                Gravity = 1000,
                Dead = 0.001f,
                Sensitivity = 1000,
                Type = InputManagerEntryType.KEY_MOUSE_BUTTON,
                Axis = 0,
                JoyNum = 0,
                PositiveButton = "joystick button " + rJoystickButton
            };
            InputManagerHelper.AddEntry(lEntry, true);
        }

        /// <summary>
        /// Create a new Input Manager setting for a joystick axis.
        /// Defaults to Gravity = 1; Dead = 0.3; Sensitivity = 1
        /// </summary>
        /// <param name="rName">Name of the Input Manager setting (input alias)</param>
        /// <param name="rAxis">Index of the joystick axis</param>
        /// <param name="rJoystickNum">(optional) Which joystick?</param>
        public static void CreateAxisInputSetting(string rName, int rAxis, int rJoystickNum = 0)
        {
            if (InputManagerHelper.IsDefined(rName)) { return; }

            InputManagerEntry lEntry = new InputManagerEntry
            {
                Name = rName,
                PositiveButton = string.Empty,
                AltPositiveButton = string.Empty,
                Gravity = 1,
                Dead = 0.3f,
                Sensitivity = 1,
                Type = InputManagerEntryType.JOYSTICK_AXIS,
                Axis = rAxis,
                JoyNum = rJoystickNum
            };
            InputManagerHelper.AddEntry(lEntry, true);
        }
#endif
    }
}
