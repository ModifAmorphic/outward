using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.Modules.Crafting;
using ModifAmorphic.Outward.Transmorphic.Settings;
using System;
using System.Collections.Generic;
using System.Text;

namespace ModifAmorphic.Outward.Transmorphic.Alchemy
{
    internal class AlchemyStartup : IStartable
    {
        private ServicesProvider _services;
        private SettingsService _settingsService;
        private readonly ConfigSettings _config;

        private IModifLogger Logger => _loggerFactory.Invoke();
        private Func<IModifLogger> _loggerFactory;

        public AlchemyStartup(ServicesProvider services, SettingsService settingsService, ConfigSettings config) => 
            (_services, _settingsService, _config) = (services, settingsService, config);

        public void Start()
        {
            _loggerFactory = _services.GetServiceFactory<IModifLogger>();
            _services
               .AddSingleton(_settingsService.ConfigureAlchemySettings(_config))
               .AddSingleton(new AlchemyIngredientMatcher());

            var alchemySettings = _services.GetService<AlchemySettings>();
            var craftingModule = _services.GetService<CustomCraftingModule>();

            //Register the menu and it's services, configs
            craftingModule.RegisterCraftingMenu<AlchemyMenu>("Alchemy", AlchemySettings.AlchemyMenuIcons);
            craftingModule.RegisterRecipeSelectorDisplayConfig<AlchemyMenu>(AlchemySettings.RecipeSelectorDisplayConfig);
            ToggleKitFuelRequirement(false);

            //Set up Menu toggle events
            alchemySettings.AlchemyMenuEnabledChanged += (isEnabled) => ToggleCraftingMenu<AlchemyMenu>(craftingModule, isEnabled);
            craftingModule.MenuLoaded += (menu) =>
            {
                if (menu is AlchemyMenu)
                {
                    ToggleCraftingMenu<AlchemyMenu>(craftingModule, alchemySettings.AlchemyMenuEnabled);

                    if (alchemySettings.DisableKitFuelRequirement)
                        ToggleKitFuelRequirement(alchemySettings.DisableKitFuelRequirement);
                }
            };
            alchemySettings.DisableKitFuelReqChanged += ToggleKitFuelRequirement;

        }
        private void ToggleCraftingMenu<T>(CustomCraftingModule craftingModule, bool isEnabled) where T : CustomCraftingMenu
        {
            if (isEnabled)
            {
                craftingModule.EnableCraftingMenu<T>();
                Logger.LogInfo("Enabled Alchemy Crafting menu.");
            }
            else
            {
                craftingModule.DisableCraftingMenu<T>();
                Logger.LogInfo("Disabled Alchemy Crafting menu.");
            }
        }
        private void ToggleKitFuelRequirement(bool kitFuelDisabled)
        {
            var craftingModule = _services.GetService<CustomCraftingModule>();
            if (kitFuelDisabled)
            {
                craftingModule.UnregisterStaticIngredients<AlchemyMenu>();
                craftingModule.UnregisterMenuIngredientFilters<AlchemyMenu>();
                craftingModule.UnregisterConsumedItemSelector<AlchemyMenu>();
                Logger.LogInfo("Disabled Alchemy Kit and Fuel Requirements for Alchemy Crafting menu.");
            }
            else
            {
                craftingModule.RegisterStaticIngredients<AlchemyMenu>(AlchemySettings.StaticIngredients);
                craftingModule.RegisterMenuIngredientFilters<AlchemyMenu>(AlchemySettings.MenuFilters);
                craftingModule.RegisterConsumedItemSelector<AlchemyMenu>(_services.GetService<AlchemyIngredientMatcher>());
                Logger.LogInfo("Enabled Alchemy Kit and Fuel Requirements for Alchemy Crafting menu.");
            }
        }
    }
}
