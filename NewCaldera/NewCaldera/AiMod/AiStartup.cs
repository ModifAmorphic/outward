using HarmonyLib;
using ModifAmorphic.Outward.Coroutines;
using ModifAmorphic.Outward.GameObjectResources;
using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.NewCaldera.AiMod.Data;
using ModifAmorphic.Outward.NewCaldera.AiMod.Data.Character;
using ModifAmorphic.Outward.NewCaldera.AiMod.Patches;
using ModifAmorphic.Outward.NewCaldera.AiMod.Services;
using ModifAmorphic.Outward.NewCaldera.Data;
using ModifAmorphic.Outward.NewCaldera.Settings;
using System;
using UnityEngine;

namespace ModifAmorphic.Outward.NewCaldera.AiMod
{
    internal class AiStartup : IStartable
    {
        private readonly Harmony _harmony;
        private readonly ServicesProvider _services;
        private readonly Func<IModifLogger> _loggerFactory;
        private readonly ModifGoService _modifGoService;
        private readonly LevelCoroutines _coroutines;
        private readonly ItemLocalizationsData _localizations;
        private readonly string SubModId = ModInfo.ModId + "." + ModInfo.AiModName;

        private IModifLogger Logger => _loggerFactory.Invoke();

        public AiStartup(ServicesProvider services, ModifGoService modifGoService, LevelCoroutines coroutines, ItemLocalizationsData localizations, Func<IModifLogger> loggerFactory)
        {
            (_services, _modifGoService, _coroutines, _localizations, _loggerFactory) = (services, modifGoService, coroutines, localizations, loggerFactory);
            _harmony = new Harmony(SubModId);
        }

        public void Start()
        {
            Logger.LogInfo("Starting New Caldera AI Mod...");
            Logger.LogInfo($"Patching...");

            //_harmony.PatchAll(typeof(AISquadPatches));
            //_harmony.PatchAll(typeof(AISquadSpawnPointPatches));
            //_harmony.PatchAll(typeof(AISquadManagerPatches));
            _harmony.PatchAll(typeof(SetObjectActivePatches));
            _harmony.PatchAll(typeof(GameObjectToggleOnQuestEventPatches));

            _harmony.PatchAll(typeof(CharacterAIPatches));
            _harmony.PatchAll(typeof(CharacterPatches));

            _harmony.PatchAll(typeof(LootableOnDeathPatches));
            _harmony.PatchAll(typeof(DropablePatches));

            Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.Full);

            _services
                     .AddSingleton(new AiDirectory(CalderaSettings.PluginPath, _loggerFactory))
                     .AddSingleton(new ProbablityTableData(_services.GetService<AiDirectory>(), _loggerFactory))
                     .AddSingleton(new UniqueEnemyHydrator(_services.GetService<ProbablityTableData>(), _loggerFactory))
                     .AddSingleton(new RegionalUniqueEnemyData(_services.GetService<AiDirectory>(),
                                        _services.GetService<UniqueEnemyHydrator>(),
                                        _loggerFactory))
                     .AddSingleton(new CyclesHydrator(_services.GetService<RegionalUniqueEnemyData>(), _loggerFactory))
                     .AddSingleton(new PlayerServicesInjector(_services, _services.GetService<PlayerServicesProvider>(), _loggerFactory));

            _ = _services.GetService<ProbablityTableData>().GetData();
            _ = _services.GetService<RegionalUniqueEnemyData>().GetData();

            //var buildingsDirectory = new BuildingsDirectory(CalderaSettings.ModConfigPath, _services.GetService<IModifLogger>);
            //var bpData = new BlueprintsData(buildingsDirectory, _loggerFactory);
            //var tieredData = new BuildingsData(buildingsDirectory, _loggerFactory);
            //_services.AddSingleton(buildingsDirectory)
            //         .AddSingleton(bpData)
            //         .AddSingleton(tieredData)
            //         .AddSingleton(new BuildingUpdater(tieredData, _loggerFactory))
            //         .AddSingleton(new BlueprintUpdater(bpData, tieredData, _localizations, _loggerFactory))
            //         .AddSingleton(new LedgerMenuService(_loggerFactory));
        }

        public void Stop()
        {
            _harmony.UnpatchSelf();
        }
    }
}
