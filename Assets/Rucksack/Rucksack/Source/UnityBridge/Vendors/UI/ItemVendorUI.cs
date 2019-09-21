using Devdog.General2;
using Devdog.General2.UI;
using Devdog.Rucksack.Collections;
using Devdog.Rucksack.Items;
using Devdog.Rucksack.Vendors;
using UnityEngine;

namespace Devdog.Rucksack.UI
{
    public class ItemVendorUI : VendorUIBase<IItemInstance>
    {
        [SerializeField]
        [Required]
        private ItemVendorCollectionUI _itemCollectionUI;
        public ItemVendorCollectionUI itemCollectionUI
        {
            get { return _itemCollectionUI; }
        }

        public string collectionName
        {
            get { return _itemCollectionUI.collectionName; }
        }

        public ICollection<IVendorProduct<IItemInstance>> collection
        {
            get { return _itemCollectionUI.collection; }
            set { _itemCollectionUI.collection = value; }
        }

        public override IVendor<IItemInstance> vendor { get; set; }

        public UIWindow window { get; protected set; }
        protected virtual void Awake()
        {
            window = GetComponent<UIWindow>();
        }
        
        protected virtual void Start()
        {
            
        }
    }
}