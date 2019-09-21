using System.Collections.Generic;

namespace Devdog.Rucksack.Collections
{
    /// <summary>
    /// The CollectionRegistry is used to register collection instances by their ID / Name. This helps find collections back dynamically.
    /// </summary>
    public static class CollectionRegistry
    {
        public class CollectionRegisteryHelper<TKey, TValue>
        {
            private Dictionary<TKey, TValue> _dict = new Dictionary<TKey, TValue>();
            public IEnumerable<TKey> keys
            {
                get { return _dict.Keys; }
            }

            public IEnumerable<TValue> values
            {
                get { return _dict.Values; }
            }

            public delegate void OnAction(TKey key, TValue value);
            
            public event OnAction OnAddedItem;
            public event OnAction OnRemovedItem;
            
            public TValue Get(TKey identifier)
            {
                TValue o;
                _dict.TryGetValue(identifier, out o);
                return o;
            }
            
            public IEnumerable<TValue> Get(IEnumerable<TKey> identifiers)
            {
                foreach (var identifier in identifiers)
                {
                    TValue o;
                    if (_dict.TryGetValue(identifier, out o))
                    {
                        yield return o;
                    }
                }
            }
            
            public void Register(TKey identifier, TValue val)
            {
                _dict[identifier] = val;
                OnAddedItem?.Invoke(identifier, val);
            }
        
            public void UnRegister(TKey identifier)
            {
                TValue currentValue;
                if (_dict.TryGetValue(identifier, out currentValue))
                {
                    _dict.Remove(identifier);
                    OnRemovedItem?.Invoke(identifier, currentValue);
                }
            }

            public void Clear()
            {
                foreach (var kvp in _dict)
                {
                    OnRemovedItem?.Invoke(kvp.Key, kvp.Value);
                }
                
                _dict.Clear();
            }
        
            public bool Contains(TKey identifier)
            {
                return _dict.ContainsKey(identifier);
            }
            
            public bool Contains(TValue value)
            {
                return _dict.ContainsValue(value);
            }
        }
        
        private static CollectionRegisteryHelper<System.Guid, ICollection> _idCols = new CollectionRegisteryHelper<System.Guid, ICollection>();
        private static CollectionRegisteryHelper<string, ICollection> _nameCols = new CollectionRegisteryHelper<string, ICollection>();

        public static CollectionRegisteryHelper<System.Guid, ICollection> byID
        {
            get { return _idCols; }
        }

        public static CollectionRegisteryHelper<string, ICollection> byName
        {
            get { return _nameCols; }
        }
    }
}