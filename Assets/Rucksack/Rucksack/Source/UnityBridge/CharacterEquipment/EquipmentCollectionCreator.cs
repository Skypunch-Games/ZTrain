using Devdog.Rucksack.Characters;
using Devdog.Rucksack.Items;
using Devdog.Rucksack.CharacterEquipment;
using Devdog.Rucksack.CharacterEquipment.Items;
using UnityEngine;

namespace Devdog.Rucksack.Collections
{
    /// <summary>
    /// Creates a local item collection on Awake and registers it in the CollectionRegistry
    /// </summary>
    public sealed class EquipmentCollectionCreator : MonoBehaviour
    {
        [SerializeField]
        private string _collectionName;
        public string collectionName
        {
            get { return _collectionName; }
            set { _collectionName = value; }
        }

        [SerializeField]
        private SerializedGuid _guid;

        public IEquipmentCollection<IEquippableItemInstance> collection { get; private set; }

        private readonly ILogger _logger;
        public EquipmentCollectionCreator()
        {
            _logger = new UnityLogger("[Collection] ");
        }
        
#if UNITY_EDITOR
        
        private void OnValidate()
        {
            if (GetComponent<IEquippableCharacter<IEquippableItemInstance>>() == null)
            {
                _logger.Warning($"{typeof(EquipmentCollectionCreator).Name} can only be added on a IEquippableCharacter component", this);
            }
        }

#endif
        
        private void Awake()
        {
            collection = new EquipmentCollection<IEquippableItemInstance>(0, GetComponent<IEquippableCharacter<IEquippableItemInstance>>(), _logger)
            {
                collectionName = collectionName
            };
            
            // col.GenerateSlots<EquipmentCollectionSlot<IEquippableItemInstance>>();
            
            CollectionRegistry.byName.Register(collectionName, collection);
            CollectionRegistry.byID.Register(_guid.guid, collection);
            
            _logger.LogVerbose($"Created and registered equipment collection with name {collectionName} and guid {_guid.guid}", this);
        }
    }
}