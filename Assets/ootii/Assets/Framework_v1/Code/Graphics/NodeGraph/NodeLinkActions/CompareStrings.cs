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
    [BaseName("Compare String Tags")]
    [BaseDescription("Checks if passed in string matches the value expected for this action.")]
    public class CompareStrings : NodeLinkAction
    {
        /// <summary>
        /// Value we want to compare with
        /// </summary>
        public string StringValue = "";

        /// <summary>
        /// Simple test to determine if the link can be traversed
        /// </summary>
        /// <param name="rUserData">Optional data to help with the test</param>
        /// <returns>Determines if the link can be traversed</returns>
        public override bool TestActivate(object rUserData = null)
        {
            if (rUserData == null) { return false; }

            string lUserString = rUserData as string;
            if (lUserString == null) { return false; }

            if (StringValue == lUserString) { return true; }

            return false;
        }

#if UNITY_EDITOR

        /// <summary>
        /// Called when the inspector needs to draw
        /// </summary>
        public override bool OnInspectorGUI(UnityEngine.Object rTarget)
        {
            bool lIsDirty = false;

            if (EditorHelper.TextField("Value", "Value we'll compare the passed in data to. If equal, the test passes.", StringValue, rTarget))
            {
                lIsDirty = true;
                StringValue = EditorHelper.FieldStringValue;
            }

            return lIsDirty;
        }

#endif
    }
}
