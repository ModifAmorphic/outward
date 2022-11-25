using HarmonyLib;
using ModifAmorphic.Outward.UnityScripts.Data;
using ModifAmorphic.Outward.UnityScripts.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace ModifAmorphic.Outward.UnityScripts.Services
{
    public class LocalizationService
    {
        private readonly Func<Logging.Logger> _loggerFactory;
        private Logging.Logger Logger => ModifScriptsManager.Instance.Logger;
        private readonly PrefabManager _prefabManager;
        public HashSet<string> _localePaths = new HashSet<string>();

        public LocalizationService(PrefabManager prefabManager, Func<Logging.Logger> loggerFactory)
        {
            _prefabManager = prefabManager;
            _loggerFactory = loggerFactory;
            prefabManager.OnCustomPrefabsLoaded += AddLocalizations;
            AfterLoadItemLocalization += AddLocalizations;
            Patch();
            AddLocalizations();
        }

        public void AddLocalePath(string localPath)
        {
            _localePaths.Add(localPath);
            AddLocalizations();
        }
        

        private void AddLocalizations()
        {
            if (!GetItemLocalizationsLoaded() || !_prefabManager.IsCustomPrefabsProcessed)
                return;

            var language = GetCurrentLanguage();
            foreach (var localPath in _localePaths)
            {
                var customLocalizations = GetCustomLocalizations(language, localPath);
                var uncastLocalizations = GetItemLocalizations();
                var itemLocalizations = uncastLocalizations.CastDictionary<int, object>();
                foreach (var localization in customLocalizations)
                {
                    if (itemLocalizations.TryGetValue(localization.ItemID, out var itemLoc))
                    {
                        if (!string.IsNullOrWhiteSpace(localization.Name))
                            uncastLocalizations[localization.ItemID].SetProperty(OutwardAssembly.Types.ItemLocalization, "Name", localization.Name);
                        if (!string.IsNullOrWhiteSpace(localization.Name))
                            uncastLocalizations[localization.ItemID].SetProperty(OutwardAssembly.Types.ItemLocalization, "Description", localization.Description);
                    }
                    else
                    {
                        itemLoc = Activator.CreateInstance(OutwardAssembly.Types.ItemLocalization, localization.Name, localization.Description);
                        uncastLocalizations.Add(localization.ItemID, itemLoc);
                    }
                }
            }
        }

        private object GetLocalizationsManager() => ReflectionExtensions.GetStaticPropertyValue(OutwardAssembly.Types.LocalizationManager, "Instance");
        private IDictionary GetItemLocalizations() => GetLocalizationsManager().GetFieldValue<IDictionary>(OutwardAssembly.Types.LocalizationManager, "m_itemLocalization");
        private string GetCurrentLanguage() => GetLocalizationsManager().GetPropertyValue<string>(OutwardAssembly.Types.LocalizationManager, "CurrentLanguageDisplayName");
        private bool GetItemLocalizationsLoaded() => GetLocalizationsManager().GetFieldValue<bool>(OutwardAssembly.Types.LocalizationManager, "m_itemLocalizationLoaded");

        private List<ItemLocalization> GetCustomLocalizations(string language, string localesDir)
        {

            var localizations = BuildingLocalizationsData.GetLocalizations(language, localesDir);

            foreach (var localization in localizations)
            {
                var item = _prefabManager.GetItemPrefab(localization.ItemID);
                if (item == null)
                {
                    Logger.LogWarning($"No item found for item localization with specified item id {localization.ItemID}");
                    continue;
                }

                if (item.GetType() != OutwardAssembly.Types.Blueprint)
                    continue;

                int buildingId = localization.ItemID > 0 ? localization.ItemID + 1 : localization.ItemID - 1;
                var building = _prefabManager.GetItemPrefab(buildingId);
                
                if (building == null)
                {
                    Logger.LogWarning($"No building with ItemID {buildingId} found for blueprint localization item id {localization.ItemID}");
                    continue;
                }

                var br = GetBuildingResources(building);

                localization.Description = localization.Description
                    .Replace("{food_production}", br.ProductionAmounts.Food.ToString())
                    .Replace("{funds_production}", br.ProductionAmounts.Funds.ToString())
                    .Replace("{stone_production}", br.ProductionAmounts.Stone.ToString())
                    .Replace("{timber_production}", br.ProductionAmounts.Timber.ToString())
                    //Capacity Increase
                    .Replace("{food_capacity}", br.ProductionAmounts.Food.ToString())
                    //.Replace("{funds_capacity}", capacity.Funds.ToString())
                    .Replace("{stone_capacity}", br.ProductionAmounts.Stone.ToString())
                    .Replace("{timber_capacity}", br.ProductionAmounts.Timber.ToString())
                    //Costs to Build
                    .Replace("{food_cost}", br.BuildingCosts.Food.ToString())
                    .Replace("{funds_cost}", br.BuildingCosts.Funds.ToString())
                    .Replace("{stone_cost}", br.BuildingCosts.Stone.ToString())
                    .Replace("{timber_cost}", br.BuildingCosts.Timber.ToString())
                    .Replace("{house_amount}", br.HousingIncrease.ToString())
                    .Replace("{build_days}", br.BuildDays.ToString())
                    //Upkeep
                    .Replace("{food_upkeep}", br.UpkeepCosts.Food.ToString())
                    .Replace("{funds_upkeep}", br.UpkeepCosts.Funds.ToString())
                    .Replace("{stone_upkeep}", br.UpkeepCosts.Stone.ToString())
                    .Replace("{timber_upkeep}", br.UpkeepCosts.Timber.ToString());


                for (int i = 0; i < br.ItemCosts.Count; i++)
                {
                    string itemCostToken = "{item" + (i + 1) + "_cost}";
                    var itemName = br.ItemCosts[i].GetPropertyValue<string>(OutwardAssembly.Types.Item, "Name");
                    Logger.LogDebug($"Replacing token {itemCostToken} with {br.ItemCosts[i]?.name} Name '{itemName}'");
                    localization.Description = localization.Description
                        .Replace(itemCostToken, br.ItemCosts[i].GetPropertyValue<string>(OutwardAssembly.Types.Item, "Name"));
                }
            }
            return localizations;
        }

        private BuildingResources GetBuildingResources(Component building)
        {
            var br = new BuildingResources();
            Type brvType = OutwardAssembly.Types.BuildingResourceValues;
            var phases = building.GetFieldValue(OutwardAssembly.Types.Building, "m_constructionPhases") as IEnumerable<object>;

            foreach (var phase in phases)
            {
                //Debug.Log("Phase: " + phase.DebugName);
                string phaseName = phase.GetFieldValue<string>(OutwardAssembly.Types.ConstructionPhase, "DebugName");
                Debug.Log("Calculating costs and production amounts for construction phase: " + phase.GetFieldValue<string>(OutwardAssembly.Types.ConstructionPhase, "DebugName"));
                var constructionCosts = phase.GetFieldValue(OutwardAssembly.Types.ConstructionPhase, "m_constructionCosts");
                if (constructionCosts != null)
                {
                    br.BuildingCosts.Funds += constructionCosts.GetFieldValue<int>(brvType, "Funds");
                    br.BuildingCosts.Food += constructionCosts.GetFieldValue<int>(brvType, "Food");
                    br.BuildingCosts.Stone += constructionCosts.GetFieldValue<int>(brvType, "Stone");
                    br.BuildingCosts.Timber += constructionCosts.GetFieldValue<int>(brvType, "Timber");
                }

                var rareMaterial = phase.GetFieldValue(OutwardAssembly.Types.ConstructionPhase, "RareMaterialRequirement");
                if (rareMaterial != null)
                {
                    Logger.LogDebug($"Found RareMaterialRequirement for building phase {phaseName}.");
                    var rareItem = rareMaterial.GetFieldValue<MonoBehaviour>(OutwardAssembly.Types.ItemQuantity, "Item");
                    if (rareItem != null)
                    {
                        Logger.LogDebug($"Adding Rare Material {rareItem.name} for building phase {phaseName}.");
                        br.ItemCosts.Add(rareItem);
                    }
                }
                br.BuildDays = phase.GetFieldValue<int>(OutwardAssembly.Types.ConstructionPhase, "m_constructionTime");

                var upkeepCosts = phase.GetFieldValue(OutwardAssembly.Types.ConstructionPhase, "UpkeepCosts");
                if (upkeepCosts != null)
                {
                    br.UpkeepCosts.Funds += upkeepCosts.GetFieldValue<int>(brvType, "Funds");
                    br.UpkeepCosts.Food += upkeepCosts.GetFieldValue<int>(brvType, "Food");
                    br.UpkeepCosts.Stone += upkeepCosts.GetFieldValue<int>(brvType, "Stone");
                    br.UpkeepCosts.Timber += upkeepCosts.GetFieldValue<int>(brvType, "Timber");
                }
                

                var upkeepProductions = phase.GetFieldValue(OutwardAssembly.Types.ConstructionPhase, "UpkeepProductions");
                if (upkeepProductions != null)
                {
                    br.ProductionAmounts.Funds += upkeepProductions.GetFieldValue<int>(brvType, "Funds");
                    br.ProductionAmounts.Food += upkeepProductions.GetFieldValue<int>(brvType, "Food");
                    br.ProductionAmounts.Stone += upkeepProductions.GetFieldValue<int>(brvType, "Stone");
                    br.ProductionAmounts.Timber += upkeepProductions.GetFieldValue<int>(brvType, "Timber");
                    br.HousingIncrease += phase.GetFieldValue<int>(OutwardAssembly.Types.ConstructionPhase, "HousingValue");
                }

                var capacityBonus = phase.GetFieldValue(OutwardAssembly.Types.ConstructionPhase, "CapacityBonus");
                if (capacityBonus != null)
                {
                    br.CapacityIncrease.Funds += capacityBonus.GetFieldValue<int>(brvType, "Funds");
                    br.CapacityIncrease.Food += capacityBonus.GetFieldValue<int>(brvType, "Food");
                    br.CapacityIncrease.Stone += capacityBonus.GetFieldValue<int>(brvType, "Stone");
                    br.CapacityIncrease.Timber += capacityBonus.GetFieldValue<int>(brvType, "Timber");
                }
            }

            return br;
        }

        #region Patches
        private void Patch()
        {
            Logger.LogInfo("Patching LocalizationManager");

            var loadItemLocalization = OutwardAssembly.Types.LocalizationManager.GetMethod("LoadItemLocalization", BindingFlags.NonPublic | BindingFlags.Instance);
            var loadItemLocalizationPostFix = this.GetType().GetMethod(nameof(LoadItemLocalizationPostfix), BindingFlags.NonPublic | BindingFlags.Static);
            HarmonyPatcher.Instance.Harmony.Patch(loadItemLocalization, postfix: new HarmonyMethod(loadItemLocalizationPostFix));
        }

        //private delegate void RegisterItemLocalizations(ref Dictionary<int, ItemLocalization> itemLocalizations);
        private static event Action AfterLoadItemLocalization;

        private static void LoadItemLocalizationPostfix()
        {
            try
            {
                ModifScriptsManager.Instance.Logger.LogTrace($"{nameof(LocalizationService)}::{nameof(LoadItemLocalizationPostfix)}(): Invoked. Invoking {nameof(AfterLoadItemLocalization)}()");
                AfterLoadItemLocalization?.Invoke();
            }
            catch (Exception ex)
            {
                ModifScriptsManager.Instance.Logger.LogException($"{nameof(LocalizationService)}::{nameof(LoadItemLocalizationPostfix)}(): Exception Invoking {nameof(AfterLoadItemLocalization)}().", ex);
            }
        }


        #endregion
    }
}
