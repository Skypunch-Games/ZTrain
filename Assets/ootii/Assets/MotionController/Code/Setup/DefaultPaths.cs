namespace com.ootii.Setup
{
    /// <summary>
    /// Default asset paths used in this codebase.
    /// </summary>
    public static class DefaultPaths
    {
        public static string Root = "Assets/ootii/";       

        public static string FrameworkContent = Root + "Assets/Framework_v1/Content/";

        public static string MotionPacks = Root + "Assets/MotionControllerPacks/";
        
        public static string MotionControllerContent = Root + "Assets/MotionController/Content/";
        
        public static string AvatarMasks = MotionControllerContent + "Animations/Humanoid/";

        /// <summary>
        /// Path to the folder default humanoid animations distributed with the Motion Controller asset.
        /// </summary>
        public static string StandardAnimations = MotionControllerContent + "Animations/Humanoid/";

        public static string CustomRoot = Root + "_MyGame/";

        public static string CustomContent = CustomRoot + "Content/";
    }
}

