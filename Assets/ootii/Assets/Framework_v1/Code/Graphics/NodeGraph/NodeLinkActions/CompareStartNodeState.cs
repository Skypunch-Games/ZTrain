using System;
using com.ootii.Base;
using com.ootii.Helpers;

namespace com.ootii.Graphics.NodeGraph
{
    /// <summary>
    /// A NodeLinkAction is used to determine if links can be
    /// traversed. We can also use them to modify data.
    /// </summary>
    [Serializable]
    [BaseName("Compare Start Node State")]
    [BaseDescription("Checks if the start node's state matches the one we expect.")]
    public class CompareStartNodeState : NodeLinkAction
    {
        /// <summary>
        /// State we will compare
        /// </summary>
        public int State = 0;

        /// <summary>
        /// Simple test to determine if the link can be traversed
        /// </summary>
        /// <param name="rUserData">Optional data to help with the test</param>
        /// <returns>Determines if the link can be traversed</returns>
        public override bool TestActivate(object rUserData = null)
        {
            // Succeeded or failed
            if (State == 4) 
            {
                if (_Link.StartNode.State == EnumNodeState.SUCCEEDED ||
                    _Link.StartNode.State == EnumNodeState.FAILED)
                {
                    return true;
                }
            }
            // Exact state
            else if (_Link.StartNode.State == State)
            {
                return true;
            }

            return false;
        }

#if UNITY_EDITOR

        /// <summary>
        /// Called when the inspector needs to draw
        /// </summary>
        public override bool OnInspectorGUI(UnityEngine.Object rTarget)
        {
            bool lIsDirty = false;

            if (EditorHelper.PopUpField("Node State", "State of the node we were compare to. If equal, the test passes.", State, EnumNodeState.ExtendedNames, rTarget))
            {
                lIsDirty = true;
                State = EditorHelper.FieldIntValue;
            }

            return lIsDirty;
        }

#endif
    }
}
