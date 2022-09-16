using ModifAmorphic.Outward.Extensions;
using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.Modules.Crafting;
using ModifAmorphic.Outward.Modules.Items;
using ModifAmorphic.Outward.Transmorphic.Menu;
using ModifAmorphic.Outward.Transmorphic.Settings;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace ModifAmorphic.Outward.Transmorphic.Enchanting.Results
{
    internal class EnchantPrefabs
    {
        /// <summary>
        /// RecipeID, <ItemID, ResultID>
        /// </summary>
        private readonly ConcurrentDictionary<int, Dictionary<int, int>> _recipeResultItemIDs = new ConcurrentDictionary<int, Dictionary<int, int>>();

        /// <summary>
        /// RecipeID, HashSet of Result ItemIDs
        /// </summary>
        private readonly ConcurrentDictionary<int, HashSet<int>> _tempRecipeResultItemIDs = new ConcurrentDictionary<int, HashSet<int>>();

        private readonly EnchantingSettings _settings;

        private IModifLogger Logger => _getLogger.Invoke();
        private readonly Func<IModifLogger> _getLogger;

        private readonly PreFabricator _preFabricator;
        ModifAmorphic.Outward.Coroutines.ModifCoroutine coroutine;
        public EnchantPrefabs(PreFabricator preFabricator,
                              EnchantingSettings settings, Func<IModifLogger> getLogger)
        {
            (_preFabricator, _settings, _getLogger) =
                (preFabricator, settings, getLogger);

            coroutine = new ModifAmorphic.Outward.Coroutines.ModifCoroutine(TransmorphPlugin.Instance, _getLogger);
        }

        public void RemoveTemporaryItems()
        {
            foreach (var recipeResults in _tempRecipeResultItemIDs.Values)
            {
                foreach (var resultId in recipeResults)
                {
                    _preFabricator.RemovePrefab(resultId);
                }
            }
            _tempRecipeResultItemIDs.Clear();
        }

        public static int GetRecipeResultID(int recipeID, int itemID)
        {
            return EnchantingSettings.RecipeStartID - recipeID;
        }
        public static (string Name, int ItemID, Type ItemType) GetPlaceholderResult(EnchantmentRecipe recipeSource, Enchantment enchantment)
        {
            int itemId = recipeSource.CompatibleEquipments.CompatibleEquipments[0].SpecificIngredient?.ItemID ?? -1;
            var tag = recipeSource.CompatibleEquipments.EquipmentTag.Tag;
            if (ItemTags.ArmorTag == tag)
                return (enchantment.Name + $" (Armor)", itemId != -1 ? itemId : EnchantingSettings.VirginArmorID, typeof(Armor));
            else if (ItemTags.HelmetTag == tag)
                return (enchantment.Name + $" (Helm)", itemId != -1 ? itemId : EnchantingSettings.VirginHelmetID, typeof(Armor));
            else if (ItemTags.BootsTag == tag)
                return (enchantment.Name + $" (Boots)", itemId != -1 ? itemId : EnchantingSettings.VirginBootsID, typeof(Armor));
            else if (ItemTags.BowTag == tag)
                return (enchantment.Name + $" (Bow)", itemId != -1 ? itemId : EnchantingSettings.SimpleBowID, typeof(Weapon));
            else if (ItemTags.ChakramTag == tag)
                return (enchantment.Name + $" (Chakram)", itemId != -1 ? itemId : EnchantingSettings.ChakramID, typeof(Weapon));
            else if (ItemTags.DagueTag == tag)
                return (enchantment.Name + $" (Dagger)", itemId != -1 ? itemId : EnchantingSettings.RondelDaggerID, typeof(Weapon));
            else if (ItemTags.WeaponTag == tag)
                return (enchantment.Name + $" (Weapon)", itemId != -1 ? itemId : EnchantingSettings.IronSwordID, typeof(Weapon));
            else if (ItemTags.LexiconTag == tag)
                return (enchantment.Name + $" (Lexicon)", itemId != -1 ? itemId : EnchantingSettings.LexiconID, typeof(Equipment));
            else if (recipeSource.RecipeID == 117)
                return (enchantment.Name + $" (Staff / Lexicon)", itemId != -1 ? itemId : EnchantingSettings.CompasswoodStaffID, typeof(Weapon));
            else if (ItemTags.TrinketTag == tag)
            {
                if (itemId == EnchantingSettings.VirginLanternID)
                {
                    return (enchantment.Name + $" (Lantern)", itemId, typeof(Equipment));
                }
                if (itemId == 5110005) // Zhornâ€™s Glowstone Dagger
                {
                    return (enchantment.Name + $" (Dagger)", itemId, typeof(Weapon));
                }
                var ingredientTag = recipeSource.CompatibleEquipments.CompatibleEquipments[0].IngredientTag;
                if (ingredientTag == ItemTags.DagueTag)
                {
                    return (enchantment.Name + $" (Dagger)", itemId != -1 ? itemId : EnchantingSettings.RondelDaggerID, typeof(Weapon));
                }
                if (ingredientTag == ItemTags.PistolTag || itemId == 5110120)  // Ornate Pistol
                {
                    return (enchantment.Name + $" (Pistol)", itemId != -1 ? itemId : EnchantingSettings.FlintlockPistolID, typeof(Weapon));
                }
                if (ingredientTag == ItemTags.ChakramTag)
                {
                    return (enchantment.Name + $" (Chakram)", itemId != -1 ? itemId : EnchantingSettings.ChakramID, typeof(Weapon));
                }
                if (ingredientTag == ItemTags.LexiconTag)
                {
                    return (enchantment.Name + $" (Lexicon)", itemId != -1 ? itemId : EnchantingSettings.LexiconID, typeof(Equipment));
                }
            }
            return (enchantment.Name, itemId != -1 ? itemId : EnchantingSettings.VirginArmorID, typeof(Equipment));
        }

        private int _resultID = EnchantingSettings.RecipeResultStartID;
        private int GetNextResultID() => _resultID--;
        public int LastResultID => _resultID;

        public Equipment CreateRecipeResultEquipment(EnchantmentRecipe recipeSource, Enchantment enchantment, bool isTemporary = false)
        {
            (var resultName, var sourceItemId, var equipType) = GetPlaceholderResult(recipeSource, enchantment);

            return CreateRecipeResultEquipment(recipeSource, sourceItemId, isTemporary);
        }

        public Equipment CreateRecipeResultEquipment(EnchantmentRecipe recipeSource, bool isTemporary = false)
        {
            var enchantment = ResourcesPrefabManager.Instance.GetEnchantmentPrefab(recipeSource.ResultID);
            (var resultName, var sourceItemId, var equipType) = GetPlaceholderResult(recipeSource, enchantment);

            return CreateRecipeResultEquipment(recipeSource, sourceItemId, isTemporary);
        }

        public Equipment CreateRecipeResultEquipment(EnchantmentRecipe recipeSource, int sourceItemID, bool isTemporary = false)
        {
            var enchantment = ResourcesPrefabManager.Instance.GetEnchantmentPrefab(recipeSource.ResultID);
            (var resultName, var sourceItemId, var equipType) = GetPlaceholderResult(recipeSource, enchantment);

            //Equipment resultPreFab = null;

            if (!_preFabricator.TryGetPrefab(sourceItemID, out Equipment resultPreFab))
            {
                var resultItemID = GetNextResultID();

                if (equipType == typeof(Armor))
                    resultPreFab = _preFabricator.CreatePrefab<Armor>(sourceItemID,
                                                                    resultItemID,
                                                                    resultName,
                                                                    enchantment.Description, false);
                else if (equipType == typeof(Weapon))
                    resultPreFab = _preFabricator.CreatePrefab<Weapon>(sourceItemID,
                                                                    resultItemID,
                                                                    resultName,
                                                                    enchantment.Description, false);
                else
                    resultPreFab = _preFabricator.CreatePrefab<Equipment>(sourceItemID,
                                                                    resultItemID,
                                                                    resultName,
                                                                    enchantment.Description, false);

                Logger.LogDebug($"Created Enchantment placeholder prefab {resultPreFab.name}: {resultPreFab.ItemID} - {resultPreFab.DisplayName}. " +
                    $"Source ItemID was {sourceItemID}. Applying Enchantment {enchantment.PresetID} - {enchantment.Name}. Item is active? {resultPreFab.gameObject.activeSelf}");
            }

            if (isTemporary)
            {
                var tempItems = _tempRecipeResultItemIDs.GetOrAdd(recipeSource.RecipeID, new HashSet<int>());
                if (!tempItems.Contains(resultPreFab.ItemID))
                    tempItems.Add(resultPreFab.ItemID);
            }
            else
            {
                resultPreFab.gameObject.SetActive(true);

                //Some items (Weapons) have a couple transforms with the base item's name added that should be removed.
                var transforms = new List<Transform>();
                var children = resultPreFab.transform.childCount;

                for (int i = 0; i < children; i++)
                {
                    transforms.Add(resultPreFab.transform.GetChild(i));
                }

                var basePrefab = ResourcesPrefabManager.Instance.GetItemPrefab(sourceItemID);
                for (int i = 0; i < transforms.Count; i++)
                {
                    if (transforms[i].name == basePrefab.transform.name)
                        transforms[i].gameObject.Destroy();
                }
            }

            return resultPreFab;
        }

        public Equipment GenerateEnchantRecipeResult(EnchantmentRecipe recipeSource, int sourceItemID)
        {
            var enchantment = ResourcesPrefabManager.Instance.GetEnchantmentPrefab(recipeSource.ResultID);

            if (_tempRecipeResultItemIDs.TryGetValue(recipeSource.RecipeID, out var itemIds) && itemIds.Contains(sourceItemID))
            {
                if (_preFabricator.TryGetPrefab<Equipment>(sourceItemID, out var cachedEquipment))
                    return cachedEquipment;
            }

            var tempEquip = CreateRecipeResultEquipment(recipeSource, sourceItemID, true);

            tempEquip.hideFlags = UnityEngine.HideFlags.HideAndDontSave;
            tempEquip.IsPrefab = false;

            if (tempEquip.UID == UID.Empty)
                tempEquip.UID = UID.Generate();

            tempEquip.SetPrivateField<Item, bool>("m_initialized", true);
            tempEquip.SetPrivateField<Item, bool>("m_uidRegistered", true);
            if (!tempEquip.gameObject.activeSelf)
            {
                tempEquip.gameObject.SetActive(true);
                tempEquip.InvokePrivateMethod<Equipment>("StartInit");
                tempEquip.Stats.UpdateStats();
            }

            tempEquip.AddEnchantment(enchantment.PresetID);

            return tempEquip;
        }

        public Equipment GenerateEnchantRecipeResult(EnchantmentRecipe recipeSource)
        {
            var enchantment = ResourcesPrefabManager.Instance.GetEnchantmentPrefab(recipeSource.ResultID);
            (var resultName, var sourceItemId, var equipType) = GetPlaceholderResult(recipeSource, enchantment);

            if (_tempRecipeResultItemIDs.TryGetValue(recipeSource.RecipeID, out var itemIds) && itemIds.Count > 0)
                if (_preFabricator.TryGetPrefab<Equipment>(itemIds.First(), out var cachedEquipment))
                    return cachedEquipment;

            var tempEquip = CreateRecipeResultEquipment(recipeSource, sourceItemId, true);

            tempEquip.hideFlags = UnityEngine.HideFlags.HideAndDontSave;
            tempEquip.IsPrefab = false;

            if (tempEquip.UID == UID.Empty)
                tempEquip.UID = UID.Generate();

            tempEquip.SetPrivateField<Item, bool>("m_initialized", true);
            tempEquip.SetPrivateField<Item, bool>("m_uidRegistered", true);
            if (!tempEquip.gameObject.activeSelf)
            {
                tempEquip.gameObject.SetActive(true);
                //tempEquip.ProcessInit();
                tempEquip.InvokePrivateMethod<Equipment>("StartInit");
                tempEquip.Stats.UpdateStats();
            }

            tempEquip.AddEnchantment(enchantment.PresetID);

            if (tempEquip is Weapon weapon)
            {
                var baseDamage = weapon.GetPrivateField<Weapon, DamageList>("m_baseDamage");
                if (baseDamage == null)
                {
                    baseDamage = weapon.Stats == null ? new DamageList(DamageType.Types.Physical, weapon.GetPrivateField<Weapon, WeaponBaseData>("m_weaponStats").Damage) : weapon.Stats.BaseDamage.Clone();
                    baseDamage.IgnoreHalfResistances = weapon.IgnoreHalfResistances;
                    weapon.SetPrivateField<Weapon, DamageList>("m_baseDamage", baseDamage);
                    weapon.SetPrivateField<Weapon, DamageList>("m_activeBaseDamage", baseDamage.Clone());
                }
            }

            return tempEquip;
        }
    }
}
