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

        public HashSet<Type> PatchDependencies => new HashSet<Type>() {
            typeof(LocalizationManagerPatches),
            typeof(ItemPatches)
        };

        public HashSet<Type> EventSubscriptions => new HashSet<Type>() {
            typeof(LocalizationManagerPatches),
            typeof(ItemPatches)
        };

        public HashSet<Type> DepsWithMultiLogger => new HashSet<Type>() {
            typeof(LocalizationManagerPatches),
            typeof(ItemPatches)
        };

        internal PreFabricator(string modId, ItemPrefabService itemPrefabService, Func<IModifLogger> loggerFactory)
        {
            this._modId = modId;
            this._loggerFactory = loggerFactory;
            this._itemPrefabService = itemPrefabService;
        }

        public T CreatePrefab<T>(int baseItemID, int newItemID, string name, string description, bool setFields = false) where T : Item
        {
            return _itemPrefabService.CreatePrefab<T>(baseItemID, newItemID, name, description, setFields);
        }
        public T CreatePrefab<T>(T basePrefab, int newItemID, string name, string description, bool setFields = false) where T : Item
        {
            return _itemPrefabService.CreatePrefab<T>(basePrefab, newItemID, name, description, setFields);
        }
    }
}
