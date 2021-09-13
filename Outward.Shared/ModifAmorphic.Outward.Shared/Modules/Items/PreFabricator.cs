using Localizer;
using ModifAmorphic.Outward.Patches;
using ModifAmorphic.Outward.Logging;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;
using ModifAmorphic.Outward.Extensions;
using System.Reflection;

namespace ModifAmorphic.Outward.Modules.Items
{
    public class PreFabricator : IModifModule
    {
        private readonly string _modId;
        private readonly Func<IModifLogger> _loggerFactory;
        private IModifLogger Logger => _loggerFactory.Invoke();
        private readonly Func<ResourcesPrefabManager> _prefabManagerFactory;
        private ResourcesPrefabManager PrefabManager => _prefabManagerFactory.Invoke();

        public HashSet<Type> PatchDependencies => new HashSet<Type>() {
            typeof(LocalizationManagerPatches)
        };

        public HashSet<Type> EventSubscriptions => new HashSet<Type>() {
            typeof(LocalizationManagerPatches)
        };

        private Transform _parentTransform;
        private readonly Dictionary<int, ItemLocalization> _itemLocalizations = new Dictionary<int, ItemLocalization>();

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal PreFabricator(string modId, ServicesProvider services)
        {
            this._modId = modId;
            this._loggerFactory = services.GetService<IModifLogger>;
            this._prefabManagerFactory = services.GetService<ResourcesPrefabManager>;
            LocalizationManagerPatches.LoadItemLocalizationAfter += (itemLocalizations) =>
                    RegisterItemLocalizations(_itemLocalizations, itemLocalizations);
        }

        public Item CreatePrefab(int baseItemID, int newItemID, string name, string description)
        {
            var basePrefab = PrefabManager.GetItemPrefab(baseItemID);
            
            return CreatePrefab(basePrefab, newItemID, name, description);
        }
        public Item CreatePrefab(Item basePrefab, int newItemID, string name, string description)
        {
            var prefab = UnityEngine.Object.Instantiate(basePrefab, GetParentTransform(), false);
            prefab.ItemID = newItemID;
            prefab.IsPrefab = true;
            prefab.SetNames(name)
                  .SetDescription(description);

            var localization = new ItemLocalization(name, description);
            _itemLocalizations.AddOrUpdate(prefab.ItemID, new ItemLocalization(name, description));
            var localizations = LocalizationManager.Instance
                                    .GetPrivateField<LocalizationManager, Dictionary<int, ItemLocalization>>("m_itemLocalization");
            localizations.AddOrUpdate(prefab.ItemID, localization);

            GetItemPrefabs().AddOrUpdate(prefab.ItemIDString, prefab);

            return prefab;
        }
        private Transform GetParentTransform()
        {
            if (_parentTransform == null)
            {
                var parentTransform = new GameObject(_modId.Replace(".", "_") + "_prefabs").transform;
                UnityEngine.Object.DontDestroyOnLoad(parentTransform.gameObject);
                parentTransform.hideFlags |= HideFlags.HideAndDontSave;
                parentTransform.gameObject.SetActive(false);
                _parentTransform = parentTransform;
            }
            return _parentTransform;
        }
        private Dictionary<string, Item> _itemPrefabs;
        public Dictionary<string, Item> GetItemPrefabs()
        {
            if (_itemPrefabs == null)
            {
                var prefabsField = typeof(ResourcesPrefabManager).GetField("ITEM_PREFABS", BindingFlags.NonPublic | BindingFlags.Static);
                _itemPrefabs = prefabsField.GetValue(null) as Dictionary<string, Item>;
            }
            return _itemPrefabs;
        }
        private void RegisterItemLocalizations(Dictionary<int, ItemLocalization> sourceLocalizations, Dictionary<int, ItemLocalization> targetLocalizations)
        {
            foreach (var kvp in sourceLocalizations)
            {
                targetLocalizations.AddOrUpdate(kvp.Key, kvp.Value);
            }
        }
    }
}
