using Devdog.Rucksack.Collections;
using Devdog.Rucksack.Items;

namespace Devdog.Rucksack.Vendors
{
    public static partial class ServerVendorRegistry
    {
        private static CollectionRegistry.CollectionRegisteryHelper<System.Guid, INetworkVendor<IItemInstance>> _itemVendors = new CollectionRegistry.CollectionRegisteryHelper<System.Guid, INetworkVendor<IItemInstance>>();
        public static CollectionRegistry.CollectionRegisteryHelper<System.Guid, INetworkVendor<IItemInstance>> itemVendors
        {
            get { return _itemVendors; }
        }
    }
}