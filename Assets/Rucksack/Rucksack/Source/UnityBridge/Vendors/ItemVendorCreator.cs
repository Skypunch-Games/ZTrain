using System;
using Devdog.Rucksack.Collections;
using Devdog.Rucksack.Currencies;
using Devdog.Rucksack.Items;
using UnityEngine;

namespace Devdog.Rucksack.Vendors
{
    public class ItemVendorCreator : MonoBehaviour
    {
        [Header("Vendor")]
        [SerializeField]
        private SerializedGuid _vendorGuid;
        public System.Guid vendorGuid
        {
            get { return _vendorGuid.guid; }
        }
        
        [SerializeField]
        private VendorConfig _config;
        
        [Header("Vendor collection")]
        [SerializeField]
        private int _slotCount;

        [SerializeField]
        private string _vendorCollectionName;

        [SerializeField]
        private SerializedGuid _vendorCollectionGuid;

        [Header("Item")]
        [SerializeField]
        private UnityItemDefinition[] _itemDefs = new UnityItemDefinition[0];

        
        // TODO: Add option to specify vendor's items / generate set of items
        public ICollection<IVendorProduct<IItemInstance>> collection { get; private set; }
        public Vendor<IItemInstance> vendor { get; private set; }
        private void Awake()
        {
            collection = new Collection<IVendorProduct<IItemInstance>>(Mathf.Max(_slotCount, _itemDefs.Length));
            CollectionRegistry.byName.Register(_vendorCollectionName, collection);
            CollectionRegistry.byID.Register(_vendorCollectionGuid.guid, collection);
            
            // TODO: Make better vendor item generator (item amounts, randomization, etc)
            foreach (var itemDef in _itemDefs)
            {
                if (itemDef == null)
                {
                    continue;
                }
                
                var inst = ItemFactory.CreateInstance(itemDef, System.Guid.NewGuid());
                collection.Add(new VendorProduct<IItemInstance>(inst, itemDef.buyPrice, itemDef.sellPrice));
            }
            
            vendor = new UnityVendor<IItemInstance>(_vendorGuid.guid, _vendorCollectionName, _vendorCollectionGuid.guid, _config, collection, new InfiniteCurrencyCollection()); // TODO: Make currency customizable
            VendorRegistry.itemVendors.Register(vendorGuid, vendor);
        }
    }
}