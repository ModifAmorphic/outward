﻿using HarmonyLib;
using ModifAmorphic.Outward.ActionUI.Patches;
using ModifAmorphic.Outward.ActionUI.Services;
using ModifAmorphic.Outward.ActionUI.Services.Injectors;
using ModifAmorphic.Outward.ActionUI.Settings;
using ModifAmorphic.Outward.Coroutines;
using ModifAmorphic.Outward.GameObjectResources;
using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.Modules;
using ModifAmorphic.Outward.Modules.Localization;
using System;

namespace ModifAmorphic.Outward.ActionUI
{
    internal class InventoryStartup : IStartable
    {
        private readonly Harmony _harmony;
        private readonly ServicesProvider _services;
        private readonly PlayerMenuService _playerMenuService;
        private readonly Func<IModifLogger> _loggerFactory;
        private readonly ModifGoService _modifGoService;
        private readonly LevelCoroutines _coroutines;
        private readonly string ModId = ModInfo.ModId + ".CharacterInventory";
        private readonly LocalizationModule _localizationsModule;

        private IModifLogger Logger => _loggerFactory.Invoke();

        public InventoryStartup(ServicesProvider services, PlayerMenuService playerMenuService, ModifGoService modifGoService, LevelCoroutines coroutines, Func<IModifLogger> loggerFactory)
        {
            (_services, _playerMenuService, _modifGoService, _coroutines, _loggerFactory) = (services, playerMenuService, modifGoService, coroutines, loggerFactory);
            _harmony = new Harmony(ModId);
            _localizationsModule = ModifModules.GetLocalizationModule(ModId);
            _localizationsModule.RegisterLocalization(InventorySettings.MoveToStashKey, "Move to Stash");
        }

        public void Start()
        {
            Logger.LogInfo("Starting Inventory Mods...");

            _harmony.PatchAll(typeof(CharacterInventoryPatches));
            _harmony.PatchAll(typeof(EquipmentMenuPatches));
            _harmony.PatchAll(typeof(InventoryContentDisplayPatches));
            _harmony.PatchAll(typeof(ItemDisplayPatches));
            _harmony.PatchAll(typeof(ItemDisplayClickPatches));
            _harmony.PatchAll(typeof(CurrencyDisplayClickPatches));
            _harmony.PatchAll(typeof(NetworkInstantiateManagerPatches));
            _harmony.PatchAll(typeof(CharacterManagerPatches));
            _harmony.PatchAll(typeof(MenuPanelPatches));
            _harmony.PatchAll(typeof(ItemDisplayOptionPanelPatches));

            _services
                     .AddSingleton(new InventoryServicesInjector(_services, _playerMenuService, _modifGoService, _coroutines, _loggerFactory))
                     .AddSingleton(new EquipSetPrefabService(_services.GetService<InventoryServicesInjector>(), _coroutines, _loggerFactory));
                     //.AddSingleton(new SkillChainPrefabricator(_services.GetService<InventoryServicesInjector>(), _coroutines, _loggerFactory));
        }
    }
}
