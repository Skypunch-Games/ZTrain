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
    [BaseName("Compare Activations")]
    [BaseDescription("Compares the activation count against the value.")]
    public class CompareActivations : NodeLinkAction
    {
        // Condition to test against
        public static string[] Comparisons = new string[] { "=", "!=", "<", "<=", ">", ">=" };

        /// <summary>
        /// Condition index
        /// </summary>
        public int _ComparisonIndex = 0;
        public int ComparisonIndex
        {
            get { return _ComparisonIndex; }
            set { _ComparisonIndex = value; }
        }

        /// <summary>
        /// Value we will compare
        /// </summary>
        public int _Value = 1;
        public int Value
        {
            get { return _Value; }
            set { _Value = value; }
        }

        /// <summary>
        /// Simple test to determine if the link can be traversed
        /// </summary>
        /// <param name="rUserData">Optional data to help with the test</param>
        /// <returns>Determines if the link can be traversed</returns>
        public override bool TestActivate(object rUserData = null)
        {
            int lValue = _Link.ActivationCount;

            bool lIsValid = false;
            switch (ComparisonIndex)
            {
                case 0:
                    lIsValid = (lValue == Value);
                    break;
                case 1:
                    lIsValid = (lValue != Value);
                    break;
                case 2:
                    lIsValid = (lValue < Value);
                    break;
                case 3:
                    lIsValid = (lValue <= Value);
                    break;
                case 4:
                    lIsValid = (lValue > Value);
                    break;
                case 5:
                    lIsValid = (lValue >= Value);
                    break;
            }

            return lIsValid;
        }

#if UNITY_EDITOR

        /// <summary>
        /// Called when the inspector needs to draw
        /// </summary>
        public override bool OnInspectorGUI(UnityEngine.Object rTarget)
        {
            bool lIsDirty = false;

            if (EditorHelper.PopUpField("Comparison", "Comparison to make.", ComparisonIndex, Comparisons, rTarget))
            {
                lIsDirty = true;
                ComparisonIndex = EditorHelper.FieldIntValue;
            }

            if (EditorHelper.IntField("Max Activations", "Maximum number of times the link can be traversed.", Value, rTarget))
            {
                lIsDirty = true;
                Value = EditorHelper.FieldIntValue;
            }

            return lIsDirty;
        }

#endif
    }
}
