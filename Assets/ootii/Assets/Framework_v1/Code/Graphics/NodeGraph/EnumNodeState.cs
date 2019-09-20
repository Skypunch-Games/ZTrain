namespace com.ootii.Graphics.NodeGraph
{
    /// <summary>
    /// Provides simple state information about the node
    /// </summary>
    public class EnumNodeState
    {
        /// <summary>
        /// Enum values
        /// </summary>
        public const int IDLE = 0;
        public const int WORKING = 1;
        public const int SUCCEEDED = 2;
        public const int FAILED = 3;

        /// <summary>
        /// Contains a mapping from ID to names
        /// </summary>
        public static string[] Names = new string[]
        {
            "Idle",
            "Working",
            "Succeeded",
            "Failed"
        };

        /// <summary>
        /// Contains a mapping from ID to names
        /// </summary>
        public static string[] ExtendedNames = new string[]
        {
            "Idle",
            "Working",
            "Succeeded",
            "Failed",
            "Succeeded or Failed"
        };

        /// <summary>
        /// Retrieve the index of the specified name
        /// </summary>
        /// <param name="rName">Name of the enumeration</param>
        /// <returns>ID of the enumeration or 0 if it's not found</returns>
        public static int GetEnum(string rName)
        {
            for (int i = 0; i < Names.Length; i++)
            {
                if (Names[i].ToLower() == rName.ToLower()) { return i; }
            }

            return 0;
        }
    }
}
