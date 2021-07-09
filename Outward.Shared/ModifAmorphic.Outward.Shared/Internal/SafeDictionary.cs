using System.Collections.Generic;

namespace ModifAmorphic.Outward.Internal
{
    internal class SafeDictionary<TKey, TValue>

    {
        private readonly object _lock = new object();
        private readonly Dictionary<TKey, TValue> _Dictionary = new Dictionary<TKey, TValue>();

        public TValue this[TKey key]
        {
            get
            {
                lock (_lock)
                {
                    return _Dictionary[key];
                }
            }
            set
            {
                lock (_lock)
                {
                    _Dictionary[key] = value;
                }
            }
        }
        public bool TryGetValue(TKey key, out TValue value)
        {
            lock (_lock)
            {
                return _Dictionary.TryGetValue(key, out value);
            }
        }
        public void AddOrUpdate(TKey key, TValue value)
        {
            lock (_lock)
            {
                if (_Dictionary.ContainsKey(key))
                {
                    _Dictionary[key] = value;
                }
                else
                {
                    _Dictionary.Add(key, value);
                }
            }
        }
    }
}
