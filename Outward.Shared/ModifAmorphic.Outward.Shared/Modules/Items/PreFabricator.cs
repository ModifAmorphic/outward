using ModifAmorphic.Outward.GameObjectResources;
using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.Modules.Items.Patches;
using ModifAmorphic.Outward.Patches;
using System;
using System.Collections.Generic;

namespace ModifAmorphic.Outward.Modules.Items
{
    public class PreFabricator : IModifModule
    {
        private readonly string _modId;
        private readonly Func<IModifLogger> _loggerFactory;
        private IModifLogger Logger => _loggerFactory.Invoke();
        private readonly ItemPrefabService _itemPrefabService;

        public ModifItemPrefabs ModifItemPrefabs => _itemPrefabService.ModifItemPrefabs;

        public HashSet<Type> PatchDependencies => new HashSet<Type>() {
            typeof(LocalizationManagerPatches),
            typeof(ItemPatches),
            typeof(ResourcesPrefabManagerPatches),
        };

        public HashSet<Type> EventSubscriptions => new HashSet<Type>() {
            typeof(LocalizationManagerPatches),
            typeof(ItemPatches),
            typeof(ResourcesPrefabManagerPatches)
        };

        public HashSet<Type> DepsWithMultiLogger => new HashSet<Type>() {
            typeof(LocalizationManagerPatches),
            typeof(ItemPatches),
            typeof(ResourcesPrefabManagerPatches)
        };

        internal PreFabricator(string modId, ItemPrefabService itemPrefabService, Func<IModifLogger> loggerFactory)
        {
            this._modId = modId;
            this._loggerFactory = loggerFactory;
            this._itemPrefabService = itemPrefabService;
        }

        public T CreatePrefab<T>(int baseItemID, int newItemID, string name, string description, bool addToResourcesPrefabManager, bool setFields = false) where T : Item
        {
            return _itemPrefabService.CreatePrefab<T>(baseItemID, newItemID, name, description, addToResourcesPrefabManager, setFields);
        }
        public T CreatePrefab<T>(T basePrefab, int newItemID, string name, string description, bool addToResourcesPrefabManager, bool setFields = false) where T : Item
        {
            return _itemPrefabService.CreatePrefab<T>(basePrefab, newItemID, name, description, addToResourcesPrefabManager, setFields);
        }
        public T GetPrefab<T>(int itemID) where T : Item
        {
            if (ModifItemPrefabs.Prefabs.TryGetValue(itemID, out var prefab) && prefab is T castPrefab)
                return castPrefab;

            return null;
        }
        public bool TryGetPrefab<T>(int itemID, out T prefab) where T : Item
        {
            prefab = GetPrefab<T>(itemID);
            return prefab != null;
        }
        public void RemovePrefab(int itemID) => ModifItemPrefabs.Remove(itemID);
    }
}
