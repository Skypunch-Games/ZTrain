using UnityEngine;
using System.Collections;

namespace com.ootii.Items
{
    /// <summary>
    /// Determines the ammo capacity of the item
    /// </summary>
    public interface IGunMagazine : IItemComponent
    {
        /// <summary>
        /// Amount of ammo the magazine can hold
        /// </summary>
        int Capacity { get; set; }

        /// <summary>
        /// Current amount of ammo in the magazine
        /// </summary>
        int Quantity { get; set; }
    }
}
