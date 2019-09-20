namespace com.ootii.Actors.AnimationControllers
{
    // CDL 08/23/2018 - refactored this over from DefaultSettings.cs, formerly "MovementStyle"
    /// <summary>
    /// Movement style options used by Motion Controller
    /// </summary>
    public static partial class EnumMovementStyle
    {
        public static int Adventure = 0;
        public static int Shooter = 1;
        public static int MMO = 2;
        
        public static string[] Names =
        {
            "Adventure",
            "Shooter",
            "MMO"
        };
    }
}
