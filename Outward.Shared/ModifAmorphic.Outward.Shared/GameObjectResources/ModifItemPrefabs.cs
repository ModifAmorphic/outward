using ModifAmorphic.Outward.Extensions;
using System.Collections.Generic;
using UnityEngine;

namespace ModifAmorphic.Outward.GameObjectResources
{
    public class ModifItemPrefabs : MonoBehaviour
    {
        private Dictionary<int, Item> _prefabs = new Dictionary<int, Item>();
        public Dictionary<int, Item> Prefabs => _prefabs;

#pragma warning disable IDE0051 // Remove unused private members
        private void Awake()
#pragma warning restore IDE0051 // Remove unused private members
        {
            this.name = "ItemPrefabs";
        }
#pragma warning disable IDE0051 // Remove unused private members
        void OnDisable()
#pragma warning restore IDE0051 // Remove unused private members
        {
            Debug.Log("ItemPrefabs: script was disabled");
        }

#pragma warning disable IDE0051 // Remove unused private members
        void OnEnable()
#pragma warning restore IDE0051 // Remove unused private members
        {
            Debug.Log("ItemPrefabs: script was enabled");
        }

        public void Add(Item prefab)
        {
            if (_prefabs.ContainsKey(prefab.ItemID))
                return;

            _prefabs.Add(prefab.ItemID, prefab);
            prefab.transform.parent = this.transform;
        }

        public void Remove(int itemID)
        {
            if (!_prefabs.ContainsKey(itemID))
                return;

            _prefabs[itemID].gameObject.SetActive(false);
            _prefabs[itemID].gameObject.Destroy();
            _prefabs.Remove(itemID);
        }

    }
}
