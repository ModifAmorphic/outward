using UnityEngine;

namespace ModifAmorphic.Outward.GameObjectResources
{
    internal class ItemPrefabs : MonoBehaviour
    {
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
    }
}
