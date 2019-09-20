using UnityEngine;

namespace com.ootii.Items
{
    /// <summary>
    /// Determines the ammo capacity of the item
    /// </summary>
    public interface IGunMuzzle : IItemComponent
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
        /// Audio clip for when the weapon is fired
        /// </summary>
        AudioClip FiredAudio { get; set; }

        /// <summary>
        /// Function to tell the muzzel that the weapon has been fired
        /// </summary>
        void OnFired(AudioSource rAudioSource);
    }
}
