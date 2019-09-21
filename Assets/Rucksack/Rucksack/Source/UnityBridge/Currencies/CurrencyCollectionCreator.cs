using UnityEngine;

namespace Devdog.Rucksack.Currencies
{
    /// <summary>
    /// Creates a local item collection on Awake and registers it in the CollectionRegistry
    /// </summary>
    public sealed class CurrencyCollectionCreator : MonoBehaviour
    {
        [SerializeField]
        private string _collectionName;

        [SerializeField]
        private SerializedGuid _guid;

        public ICurrencyCollection<ICurrency, double> collection { get; private set; }

        private readonly ILogger _logger;
        public CurrencyCollectionCreator()
        {
            _logger = new UnityLogger("[Collection] ");
        }
        
        private void Awake()
        {
            collection = new CurrencyCollection()
            {
                collectionName = _collectionName
            };
            
            CurrencyCollectionRegistry.byName.Register(_collectionName, collection);
            CurrencyCollectionRegistry.byID.Register(_guid.guid, collection);
            
            _logger.LogVerbose($"Created and registered currency collection with name {_collectionName} and guid {_guid.guid}", this);
        }
    }
}