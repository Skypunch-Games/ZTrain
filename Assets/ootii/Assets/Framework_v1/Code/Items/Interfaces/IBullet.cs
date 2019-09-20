using UnityEngine;
using com.ootii.Actors.Combat;

namespace com.ootii.Items
{
    /// <summary>
    /// Determines the damage capabilty of the weapon
    /// </summary>
    public interface IBullet : IItemComponent
    {
        /// <summary>
        /// Item ID in the enventory that represents the bullet
        /// </summary>
        string InventoryID { get; set; }

        /// <summary>
        /// Layers the bullet can collide with
        /// </summary>
        int CollisionLayers { get; set; }

        /// <summary>
        /// Sets the minimum amount of damage that can be done
        /// </summary>
        float MinDamage { get; set; }

        /// <summary>
        /// Sets the maximum amount of damage that can be done
        /// </summary>
        float MaxDamage { get; set; }

        /// <summary>
        /// Sets the minimum amount of impact force that can be applied
        /// </summary>
        float MinImpactPower { get; set; }

        /// <summary>
        /// Sets the maximum amount of impact force that can be applied
        /// </summary>
        float MaxImpactPower { get; set; }

        /// <summary>
        /// Defines the decal that will be used to represent the bullet hole
        /// </summary>
        GameObject ImpactPrefab { get; set; }

        /// <summary>
        /// Function to handle what happens when the bullet hits
        /// </summary>
        /// <param name="rMessage">Information about the impact</param>
        void OnImpact(CombatMessage rMessage);
    }
}
