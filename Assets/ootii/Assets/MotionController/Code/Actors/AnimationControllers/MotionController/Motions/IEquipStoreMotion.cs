namespace com.ootii.Actors.AnimationControllers
{
    /// <summary>
    /// Interface to help identify equip/store motions that we may need to
    /// blend into from other motions.
    /// </summary>
    public interface IEquipStoreMotion
    {
        /// <summary>
        /// Determines if the motion is enabled
        /// </summary>
        bool IsEnabled { get; }

        /// <summary>
        /// Determines if the motion is active
        /// </summary>
        bool IsActive { get; }

        /// <summary>
        /// ItemID that will tell the motion what item to created
        /// </summary>
        string OverrideItemID { get; set; }

        /// <summary>
        /// SlotID that the motion will use for equipping or storing
        /// </summary>
        string OverrideSlotID { get; set; }
    }

    /// <summary>
    /// Interface to identify an equip item motion, allowing it to be distinguished
    /// from a store item motion.
    /// </summary>
    public interface IEquipMotion : IEquipStoreMotion
    {
        bool IsEquipped { get; }
    }

    /// <summary>
    /// Interface to identify a store item motion, allowing it to be distinguished
    /// from an equip item motion.
    /// </summary>
    public interface IStoreMotion : IEquipStoreMotion
    {
        bool IsEquipped { get; }
    }
}
