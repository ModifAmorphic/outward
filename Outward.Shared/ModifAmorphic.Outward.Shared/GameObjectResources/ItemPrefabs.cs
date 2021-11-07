using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace ModifAmorphic.Outward.GameObjectResources
{
    internal class ItemPrefabs : MonoBehaviour
    {
        private void Awake()
        {
            this.name = "ItemPrefabs";
        }
        void OnDisable()
        {
            Debug.Log("ItemPrefabs: script was disabled");
        }

        void OnEnable()
        {
            Debug.Log("ItemPrefabs: script was enabled");
        }
    }
}
