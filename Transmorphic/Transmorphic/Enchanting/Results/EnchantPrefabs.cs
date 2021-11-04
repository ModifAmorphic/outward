using ModifAmorphic.Outward.Extensions;
using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.Modules.Items;
using ModifAmorphic.Outward.Transmorphic.Settings;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace ModifAmorphic.Outward.Transmorphic.Enchanting.Results
{
    internal class EnchantPrefabs
    {
        /// <summary>
        /// RecipeID, <ItemID, ResultID>
        /// </summary>
        private readonly ConcurrentDictionary<int, Dictionary<int, int>> _recipeResultItems = new ConcurrentDictionary<int, Dictionary<int, int>>();

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

        public static int GetRecipeResultID(int recipeID, int itemID)
        {
            return EnchantingSettings.RecipeStartID - recipeID;
        }
        public static (string Name, int ItemID, Type ItemType) GetResult(EnchantmentRecipe recipeSource, Enchantment enchantment)
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

        public bool TryGetResultItemID(int baseRecipeID, int sourceItemID, out int resultItemID)
        {
            if (_recipeResultItems.TryGetValue(baseRecipeID, out var recipeItems))
                return recipeItems.TryGetValue(sourceItemID, out resultItemID);

            resultItemID = -1;
            return false;
        }

        public Equipment GetOrCreateResultPrefab(EnchantmentRecipe recipeSource, Enchantment enchantment, bool activate = false)
        {
            (var resultName, var sourceItemId, var equipType) = GetResult(recipeSource, enchantment);

            return GetOrCreateResultPrefab(recipeSource, sourceItemId, activate);
        }

        public Equipment GetOrCreateResultPrefab(EnchantmentRecipe recipeSource, bool activate = false)
        {
            var enchantment = ResourcesPrefabManager.Instance.GetEnchantmentPrefab(recipeSource.ResultID);
            (var resultName, var sourceItemId, var equipType) = GetResult(recipeSource, enchantment);

            return GetOrCreateResultPrefab(recipeSource, sourceItemId, activate);
        }
            
        public Equipment GetOrCreateResultPrefab(EnchantmentRecipe recipeSource, int itemID, bool activate = false)
        {
            var enchantment = ResourcesPrefabManager.Instance.GetEnchantmentPrefab(recipeSource.ResultID);
            (var resultName, var sourceItemId, var equipType) = GetResult(recipeSource, enchantment);

            var resultItems = _recipeResultItems.GetOrAdd(recipeSource.RecipeID, new Dictionary<int, int>());
            if (!TryGetResultItemID(recipeSource.RecipeID, itemID, out var resultItemID))
            {
                resultItemID = GetNextResultID();

                Equipment resultPreFab = null;
                if (equipType == typeof(Armor))
                    resultPreFab = _preFabricator.CreatePrefab<Armor>(itemID,
                                                                    resultItemID,
                                                                    resultName,
                                                                    enchantment.Description, true);
                else if (equipType == typeof(Weapon))
                    resultPreFab = _preFabricator.CreatePrefab<Weapon>(itemID,
                                                                    resultItemID,
                                                                    resultName,
                                                                    enchantment.Description, true);
                else
                    resultPreFab = _preFabricator.CreatePrefab<Equipment>(itemID,
                                                                    resultItemID,
                                                                    resultName,
                                                                    enchantment.Description, true);
                

                resultItems.Add(itemID, resultPreFab.ItemID);

                Logger.LogDebug($"Created Enchantment placeholder prefab {resultPreFab.name}: {resultPreFab.ItemID} - {resultPreFab.DisplayName}. " +
                    $"Source ItemID was {itemID}. Applying Enchantment {enchantment.PresetID} - {enchantment.Name}. Item is active? {resultPreFab.gameObject.activeSelf}");
            }

            var prefab = (Equipment)ResourcesPrefabManager.Instance.GetItemPrefab(resultItemID);
            var prefabName = prefab.name;
            if (activate)
                prefab.InvokePrivateMethod<Equipment>("BaseInit");
            if (!prefab.gameObject.activeSelf && activate)
            {
                prefab.gameObject.SetActive(true);
                if (!prefab.ActiveEnchantmentIDs.Contains(enchantment.PresetID))
                {
                    prefab.AddEnchantment(enchantment.PresetID);
                    //var activeEnchant = ResourcesPrefabManager.Instance.GenerateEnchantment(enchantment.PresetID, prefab.transform);
                    //activeEnchant.ApplyEnchantment(prefab);
                    //activeEnchant.AppliedIncenses = recipeSource.Incenses;
                    //prefab.ActiveEnchantmentIDs.Add(enchantment.PresetID);
                    //prefab.ActiveEnchantments.Add(activeEnchant);
                    //prefab.SetPrivateField<Equipment, bool>("m_enchantmentsHaveChanged", true);
                }
                //var prefabName = prefab.name;
                //prefab.gameObject.SetActive(true);
                //prefab.name = prefabName;
                //prefab.InvokePrivateMethod<Equipment>("BaseInit");
                //prefab.InvokePrivateMethod<Equipment>("RefreshEnchantmentModifiers");
                //if (prefab is Weapon)
                //{
                //    //var enchantIds = prefab.GetPrivateField<Equipment, List<int>>("m_enchantmentIDs");
                //    coroutine.StartRoutine(
                //       coroutine.InvokeAfter(() => !prefab.gameObject.activeSelf,
                //           () =>
                //           {

                //               prefab.gameObject.SetActive(true);
                //               prefab.name = prefabName;
                //               if (!prefab.ActiveEnchantmentIDs.Contains(enchantment.PresetID))
                //               {
                //                   prefab.AddEnchantment(enchantment.PresetID);
                //               }
                //               Logger.LogDebug($"Reactivated result {prefab.ItemID} - {prefab.DisplayName} ({prefab.UID}). " +
                //                           $"Applied Enchantment {enchantment.PresetID} - {enchantment.Name}. Item is active? {prefab.gameObject.activeSelf}");
                //           }
                //           , 500, .1f
                //           ));
                //}
                //else
                //{
                //    if (!prefab.ActiveEnchantmentIDs.Contains(enchantment.PresetID))
                //        prefab.AddEnchantment(enchantment.PresetID);
                //}
                //if (!prefab.ActiveEnchantmentIDs.Contains(enchantment.PresetID))
                //    prefab.AddEnchantment(enchantment.PresetID);
            }
            if (activate)
                prefab.InvokePrivateMethod<Equipment>("RefreshEnchantmentModifiers");
            prefab.name = prefabName;
            return prefab;
        }
    }
}
