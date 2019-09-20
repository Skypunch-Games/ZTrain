using System;
using com.ootii.Base;

namespace com.ootii.Graphics.NodeGraph
{
    /// <summary>
    /// A NodeLinkAction is used to determine if links can be
    /// traversed. We can also use them to modify data.
    /// </summary>
    [Serializable]
    [BaseName("Block")]
    [BaseDescription("Blocks the link from automatically being followed. Instead, the start node must activate it directly.")]
    public class Block : NodeLinkAction
    {
        /// <summary>
        /// Simple test to determine if the link can be traversed
        /// </summary>
        /// <param name="rUserData">Optional data to help with the test</param>
        /// <returns>Determines if the link can be traversed</returns>
        public override bool TestActivate(object rUserData = null)
        {
            return false;
        }

#if UNITY_EDITOR

        /// <summary>
        /// Called when the inspector needs to draw
        /// </summary>
        public override bool OnInspectorGUI(UnityEngine.Object rTarget)
        {
            bool lIsDirty = false;

            return lIsDirty;
        }

#endif
    }
}
