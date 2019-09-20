namespace com.ootii.Actors.Inventory
{
    public interface IWeaponSetSource
    {
        int ActiveWeaponSet { get; }

        bool IsWeaponSetEquipped(int rIndex = -1);
        void StoreWeaponSet(int rIndex = -1);
        void EquipWeaponSet(int rIndex = -1);
        void ToggleWeaponSet(int rIndex = -1);
    }
}