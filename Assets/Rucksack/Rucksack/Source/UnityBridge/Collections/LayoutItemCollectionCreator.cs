using Devdog.Rucksack.Items;
using UnityEngine;

namespace Devdog.Rucksack.Collections
{
    /// <summary>
    /// Creates a local item collection on Awake and registers it in the CollectionRegistry
    /// </summary>
    public sealed class LayoutItemCollectionCreator : MonoBehaviour
    {
        [SerializeField]
        private string _collectionName;
        public string collectionName
        {
            get { return _collectionName; }
            set { _collectionName = value; }
        }

        [SerializeField]
        private int _columnCount = 4;

        [SerializeField]
        private SerializedGuid _guid;

        [SerializeField]
        private int _slotCount;
        public int slotCount
        {
            get { return _slotCount; }
            set { _slotCount = value; }
        }

        public LayoutCollection<IItemInstance> collection { get; private set; }

        private readonly ILogger _logger;
        public LayoutItemCollectionCreator()
        {
            _logger = new UnityLogger("[Collection] ");
        }
        
        private void Awake()
        {
            var builder = new LayoutCollectionBuilder<IItemInstance>();
            collection = builder.SetLogger(_logger)
                .SetSize(slotCount)
                .SetSlotType<LayoutCollectionSlot<IItemInstance>>()
                .SetName(_collectionName)
                .SetColumnCount(_columnCount)
                .Build();
            
            CollectionRegistry.byName.Register(collectionName, collection);
            CollectionRegistry.byID.Register(_guid.guid, collection);
            _logger.LogVerbose($"Created and registered collection with name {collectionName} and guid {_guid.guid}", this);
        }
    }
}