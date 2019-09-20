using System;
using UnityEngine;

namespace com.ootii.Physics
{
    /// <summary>
    /// Provides a way to manage physics based evaluations and
    /// functionality such as collision response.
    /// </summary>
    public class PhysicsExt
    {
        /// <summary>
        /// Updates the linear velocity of the two objects to account for a simple
        /// collision of spheres. This is actually a good simulation for most objects.
        /// </summary>
        /// <param name="rObject1"></param>
        /// <param name="rObject2"></param>
        public static void SolveSphericalCollision(ref PhysicsObject rCollidee, ref PhysicsObject rCollider, float rCoefficientOfRestitution)
        {
            Vector3 lCollisionNormal = rCollidee.Position - rCollider.Position;
            Vector3 lRelativeVelocity = rCollidee.Velocity - rCollider.Velocity;

            float lCollideeMassInv = 1 / rCollidee.Mass;
            float lColliderMassInv = 1 / rCollider.Mass;

            float lRelativeNormalVelocity = Vector3.Dot(lRelativeVelocity, lCollisionNormal);

            float lImpulse = (-(1f + rCoefficientOfRestitution) * lRelativeNormalVelocity) / (Vector3.Dot(lCollisionNormal, lCollisionNormal) * (lCollideeMassInv + lColliderMassInv));

            rCollidee.Velocity += (lImpulse * lCollisionNormal) * lCollideeMassInv;
            rCollider.Velocity -= (lImpulse * lCollisionNormal) * lColliderMassInv;
        }
    }
}
