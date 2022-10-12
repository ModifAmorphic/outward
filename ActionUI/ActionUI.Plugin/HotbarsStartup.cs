using HarmonyLib;
using ModifAmorphic.Outward.ActionUI.Patches;
using ModifAmorphic.Outward.ActionUI.Services;
using ModifAmorphic.Outward.ActionUI.Services.Injectors;
using ModifAmorphic.Outward.Coroutines;
using ModifAmorphic.Outward.GameObjectResources;
using ModifAmorphic.Outward.Logging;
using System;

namespace ModifAmorphic.Outward.ActionUI
{
    internal class HotbarsStartup : IStartable
    {
        private readonly Harmony _harmony;
        private readonly ServicesProvider _services;
        private readonly PlayerMenuService _playerMenuService;
        private readonly Func<IModifLogger> _loggerFactory;
        private readonly ModifGoService _modifGoService;
        private readonly LevelCoroutines _coroutines;
        private readonly string HotbarsModId = ModInfo.ModId + ".Hotbars";

        private IModifLogger Logger => _loggerFactory.Invoke();

        public HotbarsStartup(ServicesProvider services, PlayerMenuService playerMenuService, ModifGoService modifGoService, LevelCoroutines coroutines, Func<IModifLogger> loggerFactory)
        {
            (_services, _playerMenuService, _modifGoService, _coroutines, _loggerFactory) = (services, playerMenuService, modifGoService, coroutines, loggerFactory);
            _harmony = new Harmony(HotbarsModId);
        }

        public void Start()
        {
            Logger.LogInfo("Starting Hotbars...");
            Logger.LogInfo($"Patching...");

            _harmony.PatchAll(typeof(InputManager_BasePatches));
            _harmony.PatchAll(typeof(RewiredInputsPatches));
            _harmony.PatchAll(typeof(QuickSlotPanelPatches));
            _harmony.PatchAll(typeof(ControlsInputPatches));
            _harmony.PatchAll(typeof(CharacterQuickSlotManagerPatches));
            _harmony.PatchAll(typeof(SkillMenuPatches));
            _harmony.PatchAll(typeof(ItemDisplayDropGroundPatches));

            _services.AddSingleton(new HotbarServicesInjector(_services,
                                                    _playerMenuService,
                                                    _coroutines,
                                                    _loggerFactory));

        }

        public void Stop()
        {
            _harmony.UnpatchSelf();
        }
    }
}
