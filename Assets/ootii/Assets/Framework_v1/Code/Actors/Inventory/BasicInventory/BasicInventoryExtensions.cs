using System.Linq;

namespace com.ootii.Actors.Inventory
{
    /// <summary>
    /// Extension methods for BasicInventory and BasicInventorySet to streamline item management
    /// a character
    /// </summary>
    public static class BasicInventoryExtensions
    {
        /// <summary>
        /// Create a BasicInventoryItem and add it to the BasicInventory
        /// </summary>
        /// <param name="rInventory"></param>
        /// <param name="rItemID"></param>
        /// <param name="rMotionForm"></param>
        /// <param name="rResourcePath"></param>
        /// <param name="rUseMotions"></param>
        /// <param name="rAnimatorLayer"></param>
        public static BasicInventoryItem CreateBasicInventoryitem(this BasicInventory rInventory, string rItemID, int rMotionForm,
            string rResourcePath, bool rUseMotions = true, int rAnimatorLayer = 0)
        {
            if (string.IsNullOrEmpty(rItemID)) { return null; }

            // First remove the item if it already exists
            BasicInventoryItem lItem = rInventory.GetInventoryItem(rItemID);
            if (lItem != null)
            {
                rInventory.Items.Remove(lItem);
            }

            lItem = new BasicInventoryItem()
            {
                ID = rItemID,
                EquipMotion = rUseMotions ? "BasicItemEquip" + (rAnimatorLayer > 0 ? rAnimatorLayer.ToString() : "") : string.Empty,
                StoreMotion = rUseMotions ? "BasicItemStore" + (rAnimatorLayer > 0 ? rAnimatorLayer.ToString() : "") : string.Empty,
                EquipStyle = rMotionForm,
                StoreStyle = rMotionForm,
                ResourcePath = rResourcePath
            };

            rInventory.Items.Add(lItem);
            return lItem;
        }


        /// <summary>
        /// Check if an item already exists in the Basic Inventory
        /// </summary>
        /// <param name="rInventory"></param>
        /// <param name="rItemID"></param>
        /// <returns></returns>
        public static bool HasInventoryItem(this BasicInventory rInventory, string rItemID)
        {
            var lItem = rInventory.GetInventoryItem(rItemID);
            return lItem != null;
        }

        /// <summary>
        /// Create a BasicInventorySetItem for the specified slot
        /// </summary>
        /// <param name="rBasicInventorySet"></param>
        /// <param name="rSlotID"></param>
        /// <param name="rItemID"></param>
        /// <param name="rInstantiate"></param>
        public static BasicInventorySetItem CreateSetItem(this BasicInventorySet rBasicInventorySet, string rSlotID, string rItemID,
            bool rInstantiate = true)
        {
            if (string.IsNullOrEmpty(rSlotID)) { return null; }            

            BasicInventorySetItem lWeaponSetItem = rBasicInventorySet.Items.FirstOrDefault(x => x.SlotID == rSlotID);
            if (lWeaponSetItem != null)
            {
                lWeaponSetItem.ItemID = rItemID;
                lWeaponSetItem.Instantiate = rInstantiate;
                return lWeaponSetItem;
            }

            lWeaponSetItem = new BasicInventorySetItem
            {
                ItemID = rItemID,
                SlotID = rSlotID,
                Instantiate = rInstantiate
            };
            rBasicInventorySet.Items.Add(lWeaponSetItem);

            return lWeaponSetItem;
        }

        /// <summary>
        /// Create an empty BasicInventorySetItem for the specified slot
        /// </summary>
        /// <param name="rBasicInventorySet"></param>
        /// <param name="rSlotID"></param>
        public static BasicInventorySetItem CreateEmptySetItem(this BasicInventorySet rBasicInventorySet, string rSlotID)
        {
            if (string.IsNullOrEmpty(rSlotID)) { return null; }

            BasicInventorySetItem lWeaponSetItem = new BasicInventorySetItem
            {
                ItemID = "",
                SlotID = rSlotID,
                Instantiate = false
            };
            rBasicInventorySet.Items.Add(lWeaponSetItem);

            return lWeaponSetItem;
        }

        /// <summary>
        /// Create a new BasicInventorySet and initialize it by setting all slots to empty
        /// </summary>
        /// <param name="rBasicInventory"></param>
        /// <param name="rSetID"></param>
        /// <param name="rStance"></param>
        /// <param name="rDefaultForm"></param>
        /// <param name="rSetIndex"></param>
        /// <returns></returns>
        public static BasicInventorySet CreateInventorySet(this BasicInventory rBasicInventory, string rSetID, int rStance, int rDefaultForm, int rSetIndex)
        {
            BasicInventorySet lInventorySet = new BasicInventorySet
            {
                ID = rSetID,
                Stance = rStance,
                DefaultForm = rDefaultForm
            };

            foreach (BasicInventorySlot lSlot in rBasicInventory.Slots)
            {
                lInventorySet.CreateEmptySetItem(lSlot.ID);
            }

            rBasicInventory.WeaponSets.Insert(rSetIndex, lInventorySet);

            return lInventorySet;
        }      
    }
}
