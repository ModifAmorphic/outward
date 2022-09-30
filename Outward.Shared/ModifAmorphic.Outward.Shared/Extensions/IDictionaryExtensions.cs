using System.Collections.Generic;

namespace ModifAmorphic.Outward.Extensions
{
    public static class IDictionaryExtensions
    {
        public static void AddOrUpdate<K, V>(this IDictionary<K, V> keyValuePairs, K key, V value)
        {
            if (keyValuePairs.ContainsKey(key))
                keyValuePairs[key] = value;
            else
                keyValuePairs.Add(key, value);
        }

        public static bool TryRemove<K, V>(this IDictionary<K, V> keyValuePairs, K key, out V removed)
        {
            if (keyValuePairs.ContainsKey(key))
            {
                removed = keyValuePairs[key];
                keyValuePairs.Remove(key);
                return true;
            }
            else
            {
                removed = default;
                return false;
            }
        }
    }
}
