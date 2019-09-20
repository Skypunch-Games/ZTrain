namespace com.ootii.Actors.AnimationControllers
{        
    /// <summary>
    /// Motion tags used by the Setup scripts, some Reactors, etc
    /// </summary>
    public static class DefaultMotionTags
    {
        /// <summary>
        /// Used by motions which should have their root motion data applied even if 
        /// ActorController.UseTransform is set (attacks, damaged, death, etc)
        /// </summary>
        public static readonly string UseRootMotion = "UseRootMotion";

        /// <summary>
        /// Used by motions which make use of Addtive blended layers
        /// </summary>
        public static readonly string SetAdditive = "SetAdditive";

        /// <summary>
        /// Used by motions which need to disable an Additive blended layer so it doesn't mess up 
        /// the animations (jump, climb, etc). Use a reactor to "listen" for this tag
        /// </summary>
        public static readonly string DisableAdditive = "DisableAdditive";

        /// <summary>
        /// Used by motions which need to disable Grounder/Foot IK when activating, such as Jump, 
        /// Climb, etc.
        /// </summary>
        public static readonly string DisableFootIK = "DisableFootIK";

        /// <summary>
        /// Used by motions which need to specifically enable Grounder/Foot IK when activating.
        /// </summary>
        public static readonly string EnableFootIK = "EnableFootIK";

        /// <summary>
        /// Used by motions which need to disable Lef Hand IK when activating; this is most commonly used
        /// to stop the left hand from gripping a two-handed weapon when the animation doesn't have the character's
        /// hand near the handle
        /// </summary>
        public static readonly string DisableLeftHandIK = "DisableLeftHandIK";

        /// <summary>
        /// Used by motions which need to enable Left Hand IK when activating; this will generally be applied to the "Equip"
        /// motion so that it starts up the Left Hand IK
        /// </summary>
        public static readonly string EnableLeftHandIK = "EnableLeftHandIK";

        public static readonly string EquipMotion = "EquipItem";
        public static readonly string StoreMotion = "StoreItem";
    }    
}