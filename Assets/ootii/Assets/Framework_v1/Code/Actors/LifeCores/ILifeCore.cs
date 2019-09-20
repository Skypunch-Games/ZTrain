﻿using UnityEngine;

namespace com.ootii.Actors.LifeCores
{
    /// <summary>
    /// Foundation for actors/items that have a heart-beat and whose life needs to be managed
    /// </summary>
    public interface ILifeCore
    {
        /// <summary>
        /// GameObject the life core is tied to
        /// </summary>
        GameObject gameObject { get; }
    }
}
