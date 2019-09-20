using com.ootii.Helpers;
using UnityEngine;

namespace com.ootii.Input
{
    /// <summary>
    /// Helper utility for obtaining an Input Source from a GameObject
    /// </summary>
    public static class InputSourceLocator
    {
        /// <summary>
        /// Attempt to obtain an Input Source from the specified owner GameObject
        /// </summary>
        /// <param name="rOwner"></param>
        /// <returns></returns>
        public static IInputSource GetInputSource(GameObject rOwner)
        {
            return rOwner != null ? InterfaceHelper.GetComponent<IInputSource>(rOwner) : null;
        }

        /// <summary>
        /// Attempt to obtain the owner GameObject of an in-scene Input Source.
        /// </summary>
        /// <param name="rGameObject"></param>
        /// <param name="rInputSource">Reference to the Input Source</param>
        /// <returns></returns>
        public static GameObject GetInputSourceOwner(GameObject rGameObject, out IInputSource rInputSource)
        {
            rInputSource = null;
            
            // First, attempt to get the Input Source locally           
            rInputSource = InterfaceHelper.GetComponent<IInputSource>(rGameObject);
            if (rInputSource != null) { return rGameObject; }
            
            IInputSource[] lInputSources = InterfaceHelper.GetComponents<IInputSource>();
            for (int i = 0; i < lInputSources.Length; i++)
            {
                GameObject lInputSourceOwner = ((MonoBehaviour)lInputSources[i]).gameObject;
                if (lInputSourceOwner.activeSelf && lInputSources[i].IsEnabled)
                {
                    rInputSource = lInputSources[i];
                    return lInputSourceOwner;
                }
            }
            
            return null;
        }       
    }
}