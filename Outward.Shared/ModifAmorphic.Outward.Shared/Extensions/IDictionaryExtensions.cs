using System;
using System.Collections.Generic;
using System.Text;

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
    }
}
