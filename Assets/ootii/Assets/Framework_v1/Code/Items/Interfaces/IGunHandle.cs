using UnityEngine;
using System.Collections;

namespace com.ootii.Items
{
    /// <summary>
    /// Determines the stability of the item
    /// </summary>
    public interface IGunHandle : IItemComponent
    {
        /// <summary>
        /// Modifies the weapon's recoil
        /// </summary>
        float Stability { get; set; }

        /// <summary>
        /// Modifies a weapon's spread
        /// </summary>
        float Accuracy { get; set; }
    }
}
