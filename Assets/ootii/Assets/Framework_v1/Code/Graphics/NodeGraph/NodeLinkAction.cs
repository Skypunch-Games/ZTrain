using System;
using UnityEngine;
using com.ootii.Helpers;

namespace com.ootii.Graphics.NodeGraph
{
    /// <summary>
    /// A NodeLinkAction is used to determine if links can be
    /// traversed. We can also use them to modify data.
    /// </summary>
    [Serializable]
    public abstract class NodeLinkAction : ScriptableObject
    {
        /// <summary>
        /// Node link the action belongs to
        /// </summary>
        public NodeLink _Link = null;

        /// <summary>
        /// Name of the link action
        /// </summary>
        public string _Name = "";
        public string Name
        {
            get { return _Name; }
            set { _Name = value; }
        }

        /// <summary>
        /// Clear any usage data
        /// </summary>
        public virtual void Reset()
        {
        }

        /// <summary>
        /// Simple test to determine if the link can be traversed
        /// </summary>
        /// <param name="rUserData">Optional data to help with the test</param>
        /// <returns>Determines if the link can be traversed</returns>
        public virtual bool TestActivate(object rUserData = null)
        {
            return true;
        }

        /// <summary>
        /// Called when we actually traverse the link
        /// </summary>
        public virtual void Activate()
        {
        }

#if UNITY_EDITOR

        /// <summary>
        /// Called when the inspector needs to draw
        /// </summary>
        public virtual bool OnInspectorGUI(UnityEngine.Object rTarget)
        {
            bool lIsDirty = false;


            return lIsDirty;
        }

#endif
    }
}
