using HarmonyLib;
using ModifAmorphic.Outward.Coroutines;
using ModifAmorphic.Outward.GameObjectResources;
using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.Modules.Localization;
using ModifAmorphic.Outward.NewCaldera.BuildingsMod.Data;
using ModifAmorphic.Outward.NewCaldera.BuildingsMod.Models;
using ModifAmorphic.Outward.NewCaldera.BuildingsMod.Patches;
using ModifAmorphic.Outward.NewCaldera.BuildingsMod.Services;
using ModifAmorphic.Outward.NewCaldera.Data;
using ModifAmorphic.Outward.NewCaldera.Settings;
using System;
using System.Collections.Generic;

namespace ModifAmorphic.Outward.NewCaldera.BuildingsMod
{
    internal class BuildingStartup : IStartable
    {
        private readonly Harmony _harmony;
        private readonly ServicesProvider _services;
        private readonly Func<IModifLogger> _loggerFactory;
        private readonly ModifGoService _modifGoService;
        private readonly LevelCoroutines _coroutines;
        private readonly ItemLocalizationsData _localizations;
        private readonly string SubModId = ModInfo.ModId + "." + ModInfo.BuildingsModName;

        private IModifLogger Logger => _loggerFactory.Invoke();

        public BuildingStartup(ServicesProvider services, ModifGoService modifGoService, LevelCoroutines coroutines, ItemLocalizationsData localizations, Func<IModifLogger> loggerFactory)
        {
            (_services, _modifGoService, _coroutines, _localizations, _loggerFactory) = (services, modifGoService, coroutines, localizations, loggerFactory);
            _harmony = new Harmony(SubModId);
        }

        public void Start()
        {
            Logger.LogInfo("Starting Caldera Buildings Mods...");
            Logger.LogInfo($"Patching...");

            _harmony.PatchAll(typeof(ResourcesPrefabManagerPatches));
            _harmony.PatchAll(typeof(LedgerMenuPatches));
            _harmony.PatchAll(typeof(BuildingPhaseResourceReqDisplayPatches));

            var buildingsDirectory = new BuildingsDirectory(CalderaSettings.PluginPath, _services.GetService<IModifLogger>);
            var bpData = new BlueprintsData(buildingsDirectory, _loggerFactory);
            var tieredData = new BuildingsData(buildingsDirectory, _loggerFactory);
            _services.AddSingleton(buildingsDirectory)
                     .AddSingleton(bpData)
                     .AddSingleton(tieredData)
                     .AddSingleton(new BuildingUpdater(tieredData, _loggerFactory))
                     .AddSingleton(new BlueprintUpdater(bpData, tieredData, _localizations, _loggerFactory))
                     .AddSingleton(new LedgerMenuService(_loggerFactory));

            //Logger.LogDebug($"Loading blueprints to Mod Directory '{buildingsDirectory.GetOrAddDir()}'. Config Directory is '{CalderaSettings.ModConfigPath}'");

//Generate blueprint and building config data only with debug builds
#if DEBUG
            var blueprints = new BuildingBlueprints()
            {
                Blueprints = new List<BuildingBlueprint>()
                {
                    BuildingDefaults.HouseA_Blueprint,
                    BuildingDefaults.HouseB_Blueprint,
                    BuildingDefaults.HouseC_Blueprint,
                    BuildingDefaults.HuntingLodgeBlueprint,
                    BuildingDefaults.MasonsBlueprint,
                    BuildingDefaults.WoodcuttersBlueprint,
                    BuildingDefaults.CityHallBlueprint,
                    BuildingDefaults.BlacksmithBlueprint,
                    BuildingDefaults.AlchemistBlueprint,
                    BuildingDefaults.EnchantingGuildBlueprint,
                    BuildingDefaults.FoodStoreBlueprint,
                    BuildingDefaults.GladiatorsArenaBlueprint,
                    BuildingDefaults.ChapelBlueprint,
                    BuildingDefaults.GeneralStoreBlueprint,
                    BuildingDefaults.WaterPurifierBlueprint
                }
            };


            var tiers = new TieredBuildings()
            {
                Buildings = new List<TieredBuilding>()
                {
                    BuildingDefaults.HuntingBuilding,
                    BuildingDefaults.MasonBuilding,
                    BuildingDefaults.WoodcuttersBuilding,
                }
            };

            bpData.SaveNew(blueprints);
            tieredData.SaveNew(tiers);
#endif
        }

        public void Stop()
        {
            _harmony.UnpatchSelf();
        }
    }
}
