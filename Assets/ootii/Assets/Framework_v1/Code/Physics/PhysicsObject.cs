using System;
using System.Collections.Generic;
using UnityEngine;
using com.ootii.Collections;

namespace com.ootii.Physics
{
    /// <summary>
    /// Contains information about an object that we want to apply
    /// physics based calculations to. For example, this can help us determine
    /// collision response.
    /// </summary>
    public struct PhysicsObject
    {
        public float Mass;

        public Vector3 CenterOfMass;

        public Vector3 Position;

        public Vector3 Velocity;

        // ******************************** OBJECT POOL ********************************

        /// <summary>
        /// Allows us to reuse objects without having to reallocate them over and over
        /// </summary>
        private static ObjectPool<PhysicsObject> sPool = new ObjectPool<PhysicsObject>(20, 5);

        /// <summary>
        /// Returns the number of items allocated
        /// </summary>
        /// <value>The allocated.</value>
        public static int Length
        {
            get { return sPool.Length; }
        }

        /// <summary>
        /// Pulls an object from the pool.
        /// </summary>
        /// <returns></returns>
        public static PhysicsObject Allocate()
        {
            // Grab the next available object
            PhysicsObject lInstance = sPool.Allocate();

            // Set values
            lInstance.Mass = 1f;
            lInstance.CenterOfMass = Vector3.zero;
            lInstance.Position = Vector3.zero;
            lInstance.Velocity = Vector3.zero;

            return lInstance;
        }

        /// <summary>
        /// Returns an element back to the pool.
        /// </summary>
        /// <param name="rEdge"></param>
        public static void Release(PhysicsObject rInstance)
        {
            sPool.Release(rInstance);
        }
    }
}
