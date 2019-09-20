using UnityEngine;
using com.ootii.Actors.Combat;

namespace com.ootii.Items
{
    /// <summary>
    /// Determines the damage capabilty of the explosive
    /// </summary>
    public interface IExplosive : IItemComponent
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
        /// Curve used to determine the damage and force that is applied based on
        /// the victim's distance to the impact center.
        /// </summary>
        AnimationCurve ImpactCurve { get; set; }

        /// <summary>
        /// Sets the damage type we're applying
        /// </summary>
        int DamageType { get; set; }

        /// <summary>
        /// Sets the impact type we're applying
        /// </summary>
        int DamageImpactType { get; set; }

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
        /// GameObject to be used when the explosion detonates
        /// </summary>
       GameObject ExplosionPrefab { get; set; }

        /// <summary>
        /// Defines the decal that will be used to represent scorch marks
        /// </summary>
        GameObject ExplosionDecal { get; set; }

        /// <summary>
        /// Function to handle what happens when the explosive goes off
        /// </summary>
        /// <param name="rMessage">Information about the impact</param>
        void OnDetonation(CombatMessage rMessage);
    }
}
