namespace com.ootii.Actors.AnimationControllers
{
    // CDL 08/23/2018 - refactored this over from DefaultSettings.cs, formerly "DefaultAnimatorLayers"
    /// <summary>
    /// The default layer indexes and names used on both Animator Controllers and Motion Layers
    /// </summary>
    public static partial class EnumMotionLayer 
    {
        // Basic (standard) layers
        public const int BASE = 0;
        public const int UPPER_BODY = 1;

        // Extended layers:
        // Used by Shooter Motion Pack
        public const int ARMS_ONLY = 2;

        // Used for hand pose corrections by Motion Packs for weapon/combat styles: 
        // e.g. Sword & Shield, Archery, Shooter, etc
        public const int LEFT_HAND = 3;
        public const int RIGHT_HAND = 4;

        // Used to additively blend arm/torso position with all of the lower-indexed layers
        // e.g. Sword & Shield Animset Pro Motion Pack (on the Motion Vault)
        public const int UPPER_ADDITIVE = 5;

        public static string[] Names = new string[]
        {
            "Base Layer",
            "Upper Body",
            "Arms Only",
            "Left Hand",
            "Right Hand",
            "Upper Body Additive"
        };
    }
}
