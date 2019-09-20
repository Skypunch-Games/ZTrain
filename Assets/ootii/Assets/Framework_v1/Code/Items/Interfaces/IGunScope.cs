using UnityEngine;
using System.Collections;

namespace com.ootii.Items
{
    /// <summary>
    /// Determines the zoom capability of the item
    /// </summary>
    public interface IGunScope : IItemComponent
    {
        /// <summary>
        /// Modifies the weapon's recoil
        /// </summary>
        float Stability { get; set; }

        /// <summary>
        /// Modifies a weapon's spread
        /// </summary>
        float Accuracy { get; set; }

        /// <summary>
        /// Determines if the scope will manage the reticle and allow a change
        /// </summary>
        bool ManageReticle { get; set; }

        /// <summary>
        /// Texture that will be used as the Reticle
        /// </summary>
        Texture2D ReticleBGTexture { get; set; }

        /// <summary>
        /// Texture that will be used as the Reticle
        /// </summary>
        Texture2D ReticleFillTexture { get; set; }

        ///// <summary>
        ///// Determines the current zoom setting of the scope
        ///// </summary>
        //float Zoom { get; set; }

        ///// <summary>
        ///// Sets the minimum value of the zoom (ie the normal value)
        ///// </summary>
        //float MinZoom { get; set; }

        ///// <summary>
        ///// Sets the max value of the zoom (ie the highest value for the most zoom)
        ///// </summary>
        //float MaxZoom { get; set; }
    }
}
