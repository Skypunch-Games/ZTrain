﻿using Devdog.Rucksack.Items;
using Devdog.Rucksack.Vendors;
using UnityEngine;
using UnityEngine.UI;

namespace Devdog.Rucksack.UI
{
    /// <summary>
    /// A collection itemGuid for IUnityItemInstance collections.
    /// </summary>
    [AddComponentMenu(RucksackConstants.AddPath + nameof(ItemVendorCollectionSlotUI), 0)]
    public class ItemVendorCollectionSlotUI : CollectionSlotUIBase<IVendorProduct<IItemInstance>>
    {
        [Header("Item info")]
        [SerializeField]
        private Text _itemName;
        public Text itemName
        {
            get { return _itemName; }
        }

        [SerializeField]
        private Text _itemDescription;
        public Text itemDescription
        {
            get { return _itemDescription; }
        }

        [SerializeField]
        private Image _itemIcon;
        public Image itemIcon
        {
            get { return _itemIcon; }
        }

        // TODO: Use complex object to set progress
        [Header("Item Properties")]
        [SerializeField]
        private Image _cooldown;
        public Image cooldown
        {
            get { return _cooldown; }
        }

        [SerializeField]
        private Text _stackSize;
        public Text stackSize
        {
            get { return _stackSize; }
        }

        /// <summary>
        /// The stack size format used for string.Format().
        /// {0} = Current stack size
        /// {1} = Max stack size
        /// </summary>
        [SerializeField]
        private string _stackSizeFormat = "{0}/{1}";
        public string stackSizeFormat
        {
            get { return _stackSizeFormat; }
        }

        [Header("Currency")]
        [SerializeField]
        private CurrencyUI _currencyUIPrefab;
        
        [SerializeField]
        private Transform _buyPriceContainer;
        public Transform buyPriceContainer
        {
            get { return _buyPriceContainer; }
        }

        protected CurrencyUI[] currencyUIs = new CurrencyUI[3];
        
        protected override void Awake()
        {
            base.Awake();

            if (_currencyUIPrefab != null)
            {
                for (int i = 0; i < 3; i++)
                {
                    currencyUIs[i] = Instantiate<CurrencyUI>(_currencyUIPrefab, _buyPriceContainer, false);
                    currencyUIs[i].gameObject.SetActive(false);
                }   
            }
        }

        public sealed override void Repaint(IVendorProduct<IItemInstance> item, int amount)
        {
            if (item != null)
            {
                var def = item.item.itemDefinition as IUnityItemDefinition;
                if (def == null)
                {
                    logger.Warning($"Trying to add a item definition to collection that doesn't implement {nameof(IUnityItemDefinition)} - Can't add item to collection UI", this);
                    return;
                }
            
                Set(_itemName, def.name);
                Set(_itemDescription, def.description);
                Set(_itemIcon, def.icon);
                SetActive(_cooldown, true);
                Set(_stackSize, string.Format(_stackSizeFormat, amount, def.maxStackSize));

                SetActive(_buyPriceContainer, true);
                RepaintBuyCurrencies(item);
            }
            else
            {
                Set(_itemName, string.Empty);
                Set(_itemDescription, string.Empty);
                Set(_itemIcon, null);
                SetActive(_cooldown, false);
                Set(_stackSize, string.Empty);
                
                SetActive(_buyPriceContainer, false);
            }

            base.Repaint(item, amount);
        }

        protected virtual void RepaintBuyCurrencies(IVendorProduct<IItemInstance> item)
        {
//            var currencies = buyPriceContainer.GetComponentsInChildren<CurrencyUI>();
            for (int i = 0; i < item.buyPrice.Length; i++)
            {
                if (currencyUIs[i] == null)
                {
                    continue;
                }
                
                currencyUIs[i].Repaint(item.buyPrice[i].amount, item.buyPrice[i].currency);
                currencyUIs[i].gameObject.SetActive(true);
            }

            for (int i = item.buyPrice.Length; i < currencyUIs.Length; i++)
            {
                if (currencyUIs[i] == null)
                {
                    continue;
                }
                
                currencyUIs[i].gameObject.SetActive(false);
            }
        }
    }
}