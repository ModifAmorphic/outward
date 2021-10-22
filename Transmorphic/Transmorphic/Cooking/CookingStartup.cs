using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.Modules.Crafting;
using ModifAmorphic.Outward.Transmorphic.Settings;
using System;
using System.Collections.Generic;
using System.Text;

namespace ModifAmorphic.Outward.Transmorphic.Cooking
{
    internal class CookingStartup : IStartable
    {
        private ServicesProvider _services;
        private SettingsService _settingsService;
        private readonly ConfigSettings _config;

        private IModifLogger Logger => _loggerFactory.Invoke();
        private Func<IModifLogger> _loggerFactory;

        public CookingStartup(ServicesProvider services, SettingsService settingsService, ConfigSettings config) => 
            (_services, _settingsService, _config) = (services, settingsService, config);

        public void Start()
        {
            _loggerFactory = _services.GetServiceFactory<IModifLogger>();
            _services
               .AddSingleton(_settingsService.ConfigureCookingSettings(_config))
               .AddSingleton(new CookingIngredientMatcher());

            var cookingSettings = _services.GetService<CookingSettings>();
            var craftingModule = _services.GetService<CustomCraftingModule>();
            
            //Register the menu and it's services, configs
            craftingModule.RegisterCraftingMenu<CookingMenu>("Cooking", CookingSettings.CookingMenuIcons);
            craftingModule.RegisterRecipeSelectorDisplayConfig<CookingMenu>(CookingSettings.RecipeSelectorDisplayConfig);
            craftingModule.RegisterMenuIngredientFilters<CookingMenu>(CookingSettings.MenuFilters);
            craftingModule.RegisterConsumedItemSelector<CookingMenu>(_services.GetService<CookingIngredientMatcher>());

            //Set up Menu toggle events
            cookingSettings.CookingMenuEnabledChanged += (isEnabled) => ToggleCraftingMenu<CookingMenu>(craftingModule, isEnabled);
            cookingSettings.DisableKitFuelReqChanged += ToggleKitFuelRequirement;
            craftingModule.AllMenuTypesLoaded += (menusTypes) =>
            {
                ToggleCraftingMenu<CookingMenu>(craftingModule, cookingSettings.CookingMenuEnabled);
                if (!cookingSettings.DisableKitFuelRequirement)
                    ToggleKitFuelRequirement(cookingSettings.DisableKitFuelRequirement);
            };
        }
        private void ToggleCraftingMenu<T>(CustomCraftingModule craftingModule, bool isEnabled) where T : CustomCraftingMenu
        {
            if (isEnabled)
                craftingModule.EnableCraftingMenu<T>();
            else
                craftingModule.DisableCraftingMenu<T>();
        }
        private void ToggleKitFuelRequirement(bool kitFuelDisabled)
        {
            var craftingModule = _services.GetService<CustomCraftingModule>();
            if (kitFuelDisabled)
            {
                craftingModule.UnregisterRecipeSelectorDisplayConfig<CookingMenu>();
                craftingModule.UnregisterMenuIngredientFilters<CookingMenu>();
                craftingModule.UnregisterConsumedItemSelector<CookingMenu>();
            }
            else
            {
                craftingModule.RegisterRecipeSelectorDisplayConfig<CookingMenu>(CookingSettings.RecipeSelectorDisplayConfig);
                craftingModule.RegisterMenuIngredientFilters<CookingMenu>(CookingSettings.MenuFilters);
                craftingModule.RegisterConsumedItemSelector<CookingMenu>(_services.GetService<CookingIngredientMatcher>());
            }
        }
    }
}
