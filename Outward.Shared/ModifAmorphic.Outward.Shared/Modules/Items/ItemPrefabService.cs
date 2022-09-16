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
using UnityEngine;

namespace ModifAmorphic.Outward.Modules.Items
{
    internal class ItemPrefabService
    {
        private readonly string _modId;
        private readonly Func<IModifLogger> _loggerFactory;
        private IModifLogger Logger => _loggerFactory.Invoke();

        private readonly ModifGoService _modifGoService;
        private readonly ModifItemPrefabs _modifItemPrefabs;
        public ModifItemPrefabs ModifItemPrefabs => _modifItemPrefabs;
        //private readonly Transform _itemPrefabParent;

        private readonly Func<ResourcesPrefabManager> _prefabManagerFactory;
        private ResourcesPrefabManager PrefabManager => _prefabManagerFactory.Invoke();

        private readonly Dictionary<int, ItemLocalization> _itemLocalizations = new Dictionary<int, ItemLocalization>();

        private Dictionary<string, Item> _itemPrefabs;

        internal ItemPrefabService(string modId, ModifGoService modifGoService, Func<ResourcesPrefabManager> prefabManagerFactory, Func<IModifLogger> loggerFactory)
        {
            this._modId = modId;
            this._loggerFactory = loggerFactory;
            this._modifGoService = modifGoService;
            this._modifItemPrefabs = modifGoService.GetModResources(modId, true).GetOrAddComponent<ModifItemPrefabs>();
            this._prefabManagerFactory = prefabManagerFactory;

            LocalizationManagerPatches.LoadItemLocalizationAfter += RegisterItemLocalizations;
            ItemPatches.GetItemIconBefore += SetCustomItemIcon;
            ResourcesPrefabManagerPatches.TryGetItemPrefabActions.Add((int itemID, out Item item) =>
            {
                if (_modifItemPrefabs.Prefabs.ContainsKey(itemID))
                {
                    item = _modifItemPrefabs.Prefabs[itemID];
                    return true;
                }
                item = null;
                return false;
            });
        }
        private void SetCustomItemIcon(Item item)
        {
            if (item.TryGetCustomIcon(out var icon))
                item.SetItemIcon(icon);
        }
        public T CreatePrefab<T>(int baseItemID, int newItemID, string name, string description, bool addToResoucesPrefabManager, bool setFields) where T : Item
        {
            var basePrefab = (T)PrefabManager.GetItemPrefab(baseItemID);

            return CreatePrefab(basePrefab, newItemID, name, description, addToResoucesPrefabManager, setFields);
        }
        public T CreatePrefab<T>(T basePrefab, int newItemID, string name, string description, bool addToResoucesPrefabManager, bool setFields) where T : Item
        {
            if (string.IsNullOrEmpty(description))
                description = basePrefab.Description;

            var baseActiveStatus = basePrefab.gameObject.activeSelf;
            basePrefab.gameObject.SetActive(false);
            var prefab = (T)GameObject.Instantiate(basePrefab, _modifItemPrefabs.transform, false).GetComponent<Item>();
            prefab.gameObject.DeCloneNames();
            prefab.transform.ResetLocal();
            basePrefab.gameObject.SetActive(baseActiveStatus);

            //UnityEngine.Object.DontDestroyOnLoad(prefab.gameObject);
            //basePrefab.gameObject.SetActive(baseActiveStatus);
            //prefab.hideFlags |= HideFlags.HideAndDontSave;


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
                if (basePrefab is Equipment sEquip && prefab is Equipment tEquip)
                    ProcessEquipmentFields(sEquip, tEquip);
                if (prefab is Weapon weapon)
                    ProcessWeaponFields(weapon);
                if (basePrefab is Armor sArmor && prefab is Armor tarmor)
                    ProcessArmorFields(sArmor, tarmor);
            }
            prefab.SetDLC(basePrefab.DLCID);
            prefab.ItemID = newItemID;
            prefab.IsPrefab = true;
            prefab.SetNames(name)
                  .SetDescription(description);

            //var preActiveName = prefab.name;
            //Logger.LogDebug($"Prefab name before: {prefab.name}");
            ////prefab.gameObject.SetActive(true);

            //prefab.name = preActiveName;
            //Logger.LogDebug($"Prefab name after: {prefab.name}");

            var localization = new ItemLocalization(name, description);
            _itemLocalizations.AddOrUpdate(prefab.ItemID, new ItemLocalization(name, description));
            var localizations = LocalizationManager.Instance
                                    .GetPrivateField<LocalizationManager, Dictionary<int, ItemLocalization>>("m_itemLocalization");
            localizations.AddOrUpdate(prefab.ItemID, localization);

            if (addToResoucesPrefabManager)
                GetItemPrefabs().AddOrUpdate(prefab.ItemIDString, prefab);
            else
                _modifItemPrefabs.Add(prefab);

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
        private void ProcessArmorFields(Armor sourceArmor, Armor targetArmor)
        {
            var sourceBaseData = sourceArmor.GetPrivateField<Armor, ArmorBaseData>("m_baseData");
            if (sourceBaseData == null)
                return;

            var targetBaseData = new ArmorBaseData()
            {
                Class = sourceBaseData.Class,
                ColdProtection = sourceBaseData.ColdProtection,
                DamageReduction = sourceBaseData.DamageReduction?.ToList(),
                DamageResistance = sourceBaseData.DamageResistance?.ToList()
            };
            targetArmor.SetPrivateField<Armor, ArmorBaseData>("m_baseData", targetBaseData);
        }
        private void ProcessEquipmentFields(Equipment source, Equipment target)
        {
            target.AssociatedEquipment = null;
            target.SetPrivateField("m_activeEnchantments", source.GetPrivateField<Equipment, List<Enchantment>>("m_activeEnchantments")?.ToList());
            target.SetPrivateField("m_appliedAffectStats", source.GetPrivateField<Equipment, List<Effect>>("m_appliedAffectStats")?.ToList());
            target.SetPrivateField("m_enchantmentIDs", source.GetPrivateField<Equipment, List<int>>("m_enchantmentIDs")?.ToList());
            target.SetPrivateField<Equipment, SummonedEquipment>("m_summonedEquipment", null);
        }
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
