using HarmonyLib;
using ModifAmorphic.Outward.UnityScripts.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace ModifAmorphic.Outward.UnityScripts.Services
{
    public class BuildLimitsManager
    {
        private readonly Func<Logging.Logger> _loggerFactory;
        private Logging.Logger Logger => ModifScriptsManager.Instance.Logger;
        private readonly PrefabManager _prefabManager;
        private Dictionary<int, BuildingLimits> _buildingLimits = new Dictionary<int, BuildingLimits>();

        public BuildLimitsManager(PrefabManager prefabManager, Func<Logging.Logger> loggerFactory)
        {
            _prefabManager = prefabManager;
            _loggerFactory = loggerFactory;
            TryGetIsBuildingConstructedOrInConstruction = TryGetIsConstructionAllowed;
            Patch();
        }

        public void AddBuildLimit(int buildingItemID, BuildingLimits limits)
        {
            _buildingLimits.Add(buildingItemID, limits);
            Logger.LogDebug($"Added build limit of {limits.BuildLimit} for building itemID {buildingItemID}.");
        }

        private bool TryGetIsConstructionAllowed(MonoBehaviour building, int upgradeIndex, out bool denyBuild)
        {
            denyBuild = false;
            int itemID = building.GetFieldValue<int>(OutwardAssembly.Types.Building, "ItemID");
            Logger.LogTrace($"Checking if deploying a building with itemID {itemID} is allowed.");
            //No custom building limits set. Let the base method handle it.
            if (!_buildingLimits.TryGetValue(itemID, out var limits))
                return false;

            var buildings = GetBuildingsGrouped();
            //No buildings exist yet for the itemID. Let the base method handle it.
            if (!buildings.TryGetValue(itemID, out var buildingGroup))
                return false;

            if (buildingGroup.Count < limits.BuildLimit)
            {
                denyBuild = false;
                return true;
            }

            //If BuildLimit is hit, then still let base method run so notification message is shown.
            return false;
        }

        private MonoBehaviour GetBuildingResourcesManager() => (MonoBehaviour)ReflectionExtensions.GetStaticFieldValue(OutwardAssembly.Types.BuildingResourcesManager, "Instance");
        private IList GetBuildings() => GetBuildingResourcesManager().GetFieldValue<IList>(OutwardAssembly.Types.BuildingResourcesManager, "m_buildings");
        private Dictionary<int, List<MonoBehaviour>> GetBuildingsGrouped()
        {
            var grouped = new Dictionary<int, List<MonoBehaviour>>();
            var buildings = GetBuildings();
            foreach (var b in buildings)
            {
                int itemID = b.GetFieldValue<int>(OutwardAssembly.Types.Building, "ItemID");
                if (grouped.TryGetValue(itemID, out var buildingGroup))
                {
                    buildingGroup.Add((MonoBehaviour)b);
                }
                else
                    grouped[itemID] = new List<MonoBehaviour>() { (MonoBehaviour)b };
            }

            return grouped;
        }


        #region Patches
        private void Patch()
        {
            Logger.LogInfo("Patching BuildingResourcesManager");
            var getIsBuildingConstructedOrInConstruction = OutwardAssembly.Types.BuildingResourcesManager
                .GetMethod("GetIsBuildingConstructedOrInConstruction"
                    , BindingFlags.Public | BindingFlags.Instance
                    , null
                    , new Type[3] { OutwardAssembly.Types.Building, typeof(int), OutwardAssembly.Types.Character }
                    , null);
            var getIsBuildingConstructedOrInConstructionPrefix = this.GetType()
                .GetMethod(nameof(GetIsBuildingConstructedOrInConstructionPrefix)
                    , BindingFlags.NonPublic | BindingFlags.Static);
            HarmonyPatcher.Instance.Harmony.Patch(getIsBuildingConstructedOrInConstruction, prefix: new HarmonyMethod(getIsBuildingConstructedOrInConstructionPrefix));
        }

        private delegate bool TryGetIsBuildingConstructedOrInConstructionDelegate(MonoBehaviour building, int upgradeIndex, out bool allowBuild);
        private static TryGetIsBuildingConstructedOrInConstructionDelegate TryGetIsBuildingConstructedOrInConstruction;

        private static bool GetIsBuildingConstructedOrInConstructionPrefix(MonoBehaviour _building, int _upgradeIndex, MonoBehaviour _characterToNotify, ref bool __result)
        {
            try
            {
                if (_building == null || _characterToNotify == null)
                    return true;

                ModifScriptsManager.Instance.Logger.LogTrace($"{nameof(LocalizationService)}::{nameof(GetIsBuildingConstructedOrInConstructionPrefix)}(): Invoked. Invoking {nameof(TryGetIsBuildingConstructedOrInConstruction)}()");
                if (TryGetIsBuildingConstructedOrInConstruction?.Invoke(_building, _upgradeIndex, out var allowBuild)??false)
                {
                    __result = allowBuild;
                    return false;
                }
            }
            catch (Exception ex)
            {
                ModifScriptsManager.Instance.Logger.LogException($"{nameof(LocalizationService)}::{nameof(GetIsBuildingConstructedOrInConstructionPrefix)}(): Exception Invoking {nameof(TryGetIsBuildingConstructedOrInConstruction)}().", ex);
            }
            return true;
        }

        #endregion
    }
}
