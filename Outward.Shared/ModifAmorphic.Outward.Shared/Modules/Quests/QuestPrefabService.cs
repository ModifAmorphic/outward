using Localizer;
using ModifAmorphic.Outward.Extensions;
using ModifAmorphic.Outward.GameObjectResources;
using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.Modules.Items.Patches;
using ModifAmorphic.Outward.Patches;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace ModifAmorphic.Outward.Modules.Quests
{
    internal class QuestPrefabService
    {
        private readonly string _modId;
        private readonly Func<IModifLogger> _loggerFactory;
        private IModifLogger Logger => _loggerFactory.Invoke();

        private readonly ModifGoService _modifGoService;
        private readonly Transform _itemPrefabParent;

        private readonly Func<ResourcesPrefabManager> _prefabManagerFactory;
        private ResourcesPrefabManager PrefabManager => _prefabManagerFactory.Invoke();

        private readonly Dictionary<int, ItemLocalization> _itemLocalizations = new Dictionary<int, ItemLocalization>();

        internal QuestPrefabService(string modId, ModifGoService modifGoService, Func<ResourcesPrefabManager> prefabManagerFactory, Func<IModifLogger> loggerFactory)
        {
            this._modId = modId;
            this._loggerFactory = loggerFactory;
            this._modifGoService = modifGoService;
            this._itemPrefabParent = modifGoService.GetModResources(modId, true).GetOrAddComponent<ItemPrefabs>().gameObject.transform;
            this._prefabManagerFactory = prefabManagerFactory;

            LocalizationManagerPatches.LoadItemLocalizationAfter += RegisterItemLocalizations;
            //ItemPatches.GetItemIconBefore += SetCustomItemIcon;
        }
        private void SetCustomItemIcon(Item item)
        {
            if (item.TryGetCustomIcon(out var icon))
                item.SetItemIcon(icon);
        }
        public T CreatePrefab<T>(int baseItemID, int newItemID, string name, string description, bool setFields) where T : Quest
        {
            var basePrefab = (T)PrefabManager.GetItemPrefab(baseItemID);

            return CreatePrefab(basePrefab, newItemID, name, description, setFields);
        }
        public T CreatePrefab<T>(T basePrefab, int newItemID, string name, string description, bool setFields) where T : Quest
        {
            if (string.IsNullOrEmpty(description))
                description = basePrefab.Description;

            var baseActiveStatus = basePrefab.gameObject.activeSelf;
            basePrefab.gameObject.SetActive(false);
            var prefab = (T)GameObject.Instantiate(basePrefab.gameObject, _itemPrefabParent, false).GetComponent<Item>();
            basePrefab.gameObject.SetActive(baseActiveStatus);
            prefab.transform.ResetLocal();
            prefab.gameObject.DeCloneNames();

            if (setFields)
            {
                basePrefab.CopyFieldsTo(prefab);
                prefab.SetPrivateField<Item, ItemVisual>("m_loadedVisual", null);
                if (basePrefab.Stats != null)
                {
                    var prefabStats = UnityEngine.Object.Instantiate(basePrefab.Stats, prefab.transform);
                    //prefabStats.hideFlags = HideFlags.HideAndDontSave;
                    prefabStats.gameObject.DeCloneNames();
                    prefab.SetPrivateField<Item, ItemStats>("m_stats", prefabStats);

                }
                if (basePrefab.DisplayedInfos != null)
                {
                    var prefabDisplayedInfos = new ItemDetailsDisplay.DisplayedInfos[basePrefab.DisplayedInfos.Length];
                    Array.Copy(basePrefab.DisplayedInfos, prefabDisplayedInfos, prefabDisplayedInfos.Length);
                    prefab.SetPrivateField<Item, ItemDetailsDisplay.DisplayedInfos[]>("m_displayedInfos", prefabDisplayedInfos);
                    prefab.SetPrivateField<Item, bool>("m_displayedInfoInitialzed", false);
                }
            }
            prefab.SetDLC(basePrefab.DLCID);
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
        private void ProcessWeaponFields(Weapon weapon)
        {
            weapon.SetPrivateField("m_enchantmentDamageBonus", new DamageList());
            weapon.SetPrivateField<Weapon, DamageList>("m_baseDamage", null);
            weapon.SetPrivateField<Weapon, DamageList>("m_activeBaseDamage", null);
            weapon.SetPrivateField<Weapon, List<Character>>("m_alreadyHitChars", new List<Character>());
            weapon.SetPrivateField("m_lastDealtDamages", new DamageList());
            weapon.SetPrivateField("m_imbueStack", new DictionaryExt<string, ImbueStack>());
            weapon.SetPrivateField("tmpattackTags", new List<Tag>());
            weapon.SetPrivateField<Weapon, DamageList>("baseDamage", null);
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
        private void RegisterItemLocalizations(ref Dictionary<int, ItemLocalization> targetLocalizations)
        {
            foreach (var kvp in _itemLocalizations)
            {
                targetLocalizations.AddOrUpdate(kvp.Key, kvp.Value);
            }
        }

    }
}
