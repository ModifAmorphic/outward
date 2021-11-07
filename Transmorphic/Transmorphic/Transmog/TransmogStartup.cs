using BepInEx;
using ModifAmorphic.Outward.Coroutines;
using ModifAmorphic.Outward.Extensions;
using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.Modules.Crafting;
using ModifAmorphic.Outward.Modules.Items;
using ModifAmorphic.Outward.Transmorphic.Menu;
using ModifAmorphic.Outward.Transmorphic.Settings;
using ModifAmorphic.Outward.Transmorphic.Transmog.MenuIngredientMatchers;
using ModifAmorphic.Outward.Transmorphic.Transmog.SaveData;
using System;
using System.Collections.Generic;
using System.Text;

namespace ModifAmorphic.Outward.Transmorphic.Transmog
{
    internal class TransmogStartup : IStartable
    {
        private ServicesProvider _services;
        private SettingsService _settingsService;
        private readonly ConfigSettings _config;

        private IModifLogger Logger => _loggerFactory.Invoke();
        private Func<IModifLogger> _loggerFactory;

        public TransmogStartup(ServicesProvider services, SettingsService settingsService, ConfigSettings config) =>
            (_services, _settingsService, _config) = (services, settingsService, config);

        public void Start()
        {
            _loggerFactory = _services.GetServiceFactory<IModifLogger>();
            _services
               .AddSingleton(_settingsService.ConfigureTransmogrifySettings(_config))
               .AddSingleton(new TransmogItemListener(
                                _services.GetService<ItemVisualizer>(),
                                _services.GetService<LevelCoroutines>(),
                                () => ItemManager.Instance,
                                _services.GetService<TransmogSettings>(),
                                _services.GetService<IModifLogger>
                ));
            
            ConfigureRecipes();
            ConfigureMenu();
        }
        private void ConfigureRecipes()
        {
            _services
                .AddSingleton(new RemoverResultService(_services.GetService<IModifLogger>))
                .AddSingleton(new ArmorResultService(_services.GetService<IModifLogger>))
                .AddSingleton(new WeaponResultService(_services.GetService<IModifLogger>))
                .AddSingleton(new BagResultService(_services.GetService<IModifLogger>))
                .AddSingleton(new LexiconResultService(_services.GetService<IModifLogger>))
                .AddSingleton(new LanternResultService(_services.GetService<IModifLogger>))
                .AddSingleton(new TransmogRecipeData(System.IO.Path.GetDirectoryName(
                                                    _services.GetService<BaseUnityPlugin>().Config.ConfigFilePath),
                                                    _services.GetService<IModifLogger>)
                )
                .AddSingleton(new TransmogRecipeGenerator(
                                _services.GetService<RemoverResultService>(),
                                _services.GetService<ArmorResultService>(),
                                _services.GetService<WeaponResultService>(),
                                _services.GetService<BagResultService>(),
                                _services.GetService<LexiconResultService>(),
                                _services.GetService<LanternResultService>(),
                                _services.GetService<TransmogSettings>(),
                                _services.GetService<IModifLogger>
                    ))
                .AddSingleton(new TransmogRecipeService(_services.GetService<BaseUnityPlugin>(),
                                _services.GetService<TransmogRecipeGenerator>(),
                                _services.GetService<CustomCraftingModule>(),
                                _services.GetService<PreFabricator>(),
                                _services.GetService<LevelCoroutines>(),
                                _services.GetService<TransmogRecipeData>(),
                                _services.GetService<TransmogSettings>(),
                                _services.GetService<IModifLogger>
                ));
        }
        private void ConfigureMenu()
        {
            _services.AddSingleton(new MenuIngredientMatcher(
                                    new List<ITransmogMatcher>()
                                    {
                                        new ArmorMatcher(_services.GetService<IModifLogger>),
                                        new WeaponMatcher(_services.GetService<IModifLogger>),
                                        new BagMatcher(_services.GetService<IModifLogger>),
                                        new LexiconMatcher(_services.GetService<IModifLogger>),
                                        new LanternMatcher(_services.GetService<IModifLogger>),
                                        new RemoverMatcher(_services.GetService<IModifLogger>)
                                    }
                                    , _services.GetService<IModifLogger>)
                      )
                     .AddSingleton(new TransmogCrafter(_services.GetService<ItemVisualizer>(),
                                        _services.GetService<IModifLogger>)
                      );


            var craftingModule = _services.GetService<CustomCraftingModule>();
            var transmogSettings = _services.GetService<TransmogSettings>();

            craftingModule.RegisterCraftingMenu<TransmogrifyMenu>("Transmogrify", TransmogSettings.TransmogMenuIcons);
            craftingModule.RegisterMenuIngredientFilters<TransmogrifyMenu>(TransmogSettings.MenuFilters);
            craftingModule.RegisterCompatibleIngredientMatcher<TransmogrifyMenu>(_services.GetService<MenuIngredientMatcher>());
            craftingModule.RegisterConsumedItemSelector<TransmogrifyMenu>(_services.GetService<MenuIngredientMatcher>());
            craftingModule.RegisterCustomCrafter<TransmogrifyMenu>(_services.GetService<TransmogCrafter>());
            
            transmogSettings.TransmogMenuEnabledChanged += (isEnabled) => ToggleCraftingMenu<TransmogrifyMenu>(craftingModule, isEnabled);
            craftingModule.CraftingMenuEvents.MenuLoaded += (menu) =>
            {
                if (menu is TransmogrifyMenu)
                    ToggleCraftingMenu<TransmogrifyMenu>(craftingModule, transmogSettings.TransmogMenuEnabled);
            };
            
        }
        private void ToggleCraftingMenu<T>(CustomCraftingModule craftingModule, bool isEnabled) where T : CustomCraftingMenu
        {
            if (isEnabled)
            {
                craftingModule.EnableCraftingMenu<T>();
                Logger.LogInfo("Enabled Transmogrify Crafting menu.");
            }
            else
            {
                craftingModule.DisableCraftingMenu<T>();
                Logger.LogInfo("Disabled Transmogrify Crafting menu.");
            }
        }
    }
}
