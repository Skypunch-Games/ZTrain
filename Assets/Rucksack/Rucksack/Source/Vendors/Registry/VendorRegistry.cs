using Devdog.Rucksack.Collections;

namespace Devdog.Rucksack.Vendors
{
    public static partial class VendorRegistry
    {
        private static CollectionRegistry.CollectionRegisteryHelper<System.Guid, IVendor> _itemVendors = new CollectionRegistry.CollectionRegisteryHelper<System.Guid, IVendor>();
        public static CollectionRegistry.CollectionRegisteryHelper<System.Guid, IVendor> itemVendors
        {
            get { return _itemVendors; }
        }
    }
}