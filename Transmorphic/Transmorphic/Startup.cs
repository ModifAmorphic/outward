#if DEBUG
using UnityEngine;
using ModifAmorphic.Outward.Extensions;
using ModifAmorphic.Outward.Transmorph.Extensions;
using ModifAmorphic.Outward.Transmorph.Transmog.Models;
#endif
using System;
using BepInEx;
using System.Collections.Generic;
using ModifAmorphic.Outward.Coroutines;
using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.Modules;
using ModifAmorphic.Outward.Modules.Crafting;
using ModifAmorphic.Outward.Modules.Items;
using ModifAmorphic.Outward.Transmorph.Menu;
using ModifAmorphic.Outward.Transmorph.Settings;
using ModifAmorphic.Outward.Transmorph.Transmog;
using ModifAmorphic.Outward.Transmorph.Transmog.SaveData;

namespace ModifAmorphic.Outward.Transmorph
{
    internal class Startup
    {
        private ServicesProvider _services;
        internal void Start(ServicesProvider services)
        {
            _services = services;
            var settingsService = new SettingsService(services.GetService<BaseUnityPlugin>(), ModInfo.MinimumConfigVersion);
            var confSettings = settingsService.ConfigureSettings();

            services
                .AddSingleton(settingsService.ConfigureGlobalSettings(confSettings))
                .AddSingleton(settingsService.ConfigureTransmogrifySettings(confSettings))
                .AddFactory(() => LoggerFactory.GetLogger(ModInfo.ModId))
                .AddSingleton(ModifModules.GetPreFabricatorModule(ModInfo.ModId))
                .AddSingleton(ModifModules.GetItemVisualizerModule(ModInfo.ModId))
                .AddSingleton(ModifModules.GetCustomCraftingModule(ModInfo.ModId))
                .AddSingleton(new LevelCoroutines(services.GetService<BaseUnityPlugin>(), 
                                                  services.GetService<IModifLogger>))
                .AddSingleton(new TransmogItemListener(
                                services.GetService<ItemVisualizer>(),
                                services.GetService<LevelCoroutines>(),
                                () => ItemManager.Instance,
                                services.GetService<TransmogSettings>(),
                                services.GetService<IModifLogger>
                ));

            ConfigureRecipes();
            ConfigureCraftingMenus();

#if DEBUG
            //TmogRecipeManagerPatches.LoadCraftingRecipeAfter += (r) => AddAdvancedCraftingRecipes(
            //    services.GetService<BaseUnityPlugin>(),
            //    craftingModule,
            //    services.GetService<IModifLogger>);

            //TestUIDs(services.GetService<IModifLogger>());
#endif
        }

        private void ConfigureRecipes()
        {
            _services
                .AddSingleton(new RemoverResultService(_services.GetService<IModifLogger>))
                .AddSingleton(new ArmorResultService(_services.GetService<IModifLogger>))
                .AddSingleton(new WeaponResultService(_services.GetService<IModifLogger>))
                .AddSingleton(new TransmogRecipeData(System.IO.Path.GetDirectoryName(
                                                    _services.GetService<BaseUnityPlugin>().Config.ConfigFilePath),
                                                    _services.GetService<IModifLogger>)
                )
                .AddSingleton(new TmogRecipeService(_services.GetService<BaseUnityPlugin>(),
                                _services.GetService<RemoverResultService>(),
                                _services.GetService<ArmorResultService>(),
                                _services.GetService<WeaponResultService>(),
                                _services.GetService<CustomCraftingModule>(),
                                _services.GetService<PreFabricator>(),
                                _services.GetService<LevelCoroutines>(),
                                _services.GetService<TransmogRecipeData>(),
                                _services.GetService<TransmogSettings>(),
                                _services.GetService<IModifLogger>
                ));
        }
        private void ConfigureCraftingMenus()
        {
            _services.AddSingleton(new IngredientMatcher(_services.GetService<IModifLogger>))
                     .AddSingleton(new TransmogCrafter(_services.GetService<ItemVisualizer>(),
                                        _services.GetService<IModifLogger>)
                      );

            var globalSettings = _services.GetService<GlobalSettings>();
            var craftingModule = _services.GetService<CustomCraftingModule>();
            //Set up Menu toggle events
            craftingModule.RegisterCraftingMenu<CookingMenu>("Cooking", GlobalSettings.CookingMenuIcons);
            globalSettings.CookingMenuEnabledChanged += ToggleCraftingMenu<CookingMenu>;
            craftingModule.RegisterCraftingMenu<AlchemyMenu>("Alchemy", GlobalSettings.AlchemyMenuIcons);
            globalSettings.AlchemyMenuEnabledChanged += ToggleCraftingMenu<AlchemyMenu>;
            //One time toggle after menus finished loading
            craftingModule.AllMenuTypesLoaded += (menusTypes) =>
            {
                ToggleCraftingMenu<AlchemyMenu>(globalSettings.AlchemyMenuEnabled);
                ToggleCraftingMenu<CookingMenu>(globalSettings.CookingMenuEnabled);
            };

            craftingModule.RegisterCraftingMenu<TransmogrifyMenu>("Transmogrify", TransmogSettings.TransmogMenuIcons);

            craftingModule.RegisterCompatibleIngredientMatcher<TransmogrifyMenu>(_services.GetService<IngredientMatcher>());
            craftingModule.RegisterConsumedItemSelector<TransmogrifyMenu>(_services.GetService<IngredientMatcher>());
            craftingModule.RegisterCustomCrafter<TransmogrifyMenu>(_services.GetService<TransmogCrafter>());
            craftingModule.RegisterMenuIngredientFilters<TransmogrifyMenu>(
                new MenuIngredientFilters()
                {
                    //BaseInventoryFilterTag = new Tag("70", "Item"),
                    AdditionalInventoryIngredientFilter = null,
                    EquippedIngredientFilter = new AvailableIngredientFilter()
                    {
                        EnchantFilter = AvailableIngredientFilter.EnchantFilters.IncludeEnchanted,
                        ItemTypes = new HashSet<Type>() { typeof(Equipment) }
                    },
                });
        }
        private void ToggleCraftingMenu<T>(bool isEnabled) where T : CustomCraftingMenu
        {
            var craftingModule = _services.GetService<CustomCraftingModule>();
            if (isEnabled)
                craftingModule.EnableCraftingMenu<T>();
            else
                craftingModule.DisableCraftingMenu<T>();
        }
#if DEBUG
        private void TestUIDs(IModifLogger logger)
        {
            var random = new System.Random();
            for (int i = 0; i < 50; i++)
            {
                var visualMap = new ItemVisualMap() { ItemID = random.Next(-99999999, 99999999), VisualItemID = random.Next(-99999999, 99999999) };

                var morgUID = visualMap.ToUID();
                logger.LogDebug($"{nameof(TestUIDs)}: VisualMap: {{ItemID = {visualMap.ItemID}, VisualItemID = {visualMap.VisualItemID}}}\n" +
                    $"Generated UID: {morgUID}. Decoded UID (GUID): {UID.Decode(morgUID)}");

                logger.LogDebug($"{nameof(TestUIDs)}: Generated UID: {morgUID}. UID IsTransMorg? {morgUID.IsTransmogrified()}");

                var decodedVisualItemID = morgUID.ToVisualItemID();
                logger.LogDebug($"{nameof(TestUIDs)}: Generated UID: {morgUID}\n" +
                    $"VisualItemID = {decodedVisualItemID}");

                morgUID.TryGetVisualItemID(out var tryVisualItemID);
                logger.LogDebug($"{nameof(TestUIDs)}: Generated UID: {morgUID}\n" +
                    $"VisualItemID = {tryVisualItemID}");
            }

        }
        private void AddAdvancedCraftingRecipes(BaseUnityPlugin plugin, CustomCraftingModule craftingModule, Func<IModifLogger> loggerFactory)
        {
            var armorRecipe = ScriptableObject.CreateInstance<Recipe>()
                .SetRecipeIDEx(-130310000)
                .SetUID("D_AK74Pm6U23iSwrH13ORA")
                .SetNames("Blue Sand Armor")
                .AddIngredient(ResourcesPrefabManager.Instance.GetItemPrefab("6400110") ?? ResourcesPrefabManager.Instance.GenerateItem("6400110"), 3)
                .AddIngredient(ResourcesPrefabManager.Instance.GetItemPrefab("3000010") ?? ResourcesPrefabManager.Instance.GenerateItem("3000010"))
                .AddResult(3100080);

            var helmRecipe = ScriptableObject.CreateInstance<Recipe>()
                .SetRecipeIDEx(-130310001)
                .SetUID(UID.Encode(Guid.Parse("68bedd71-7b68-4365-a06c-d0e4fb2e6c23")))
                .SetNames("Blue Sand Helm")
                .AddIngredient(ResourcesPrefabManager.Instance.GetItemPrefab("6400110") ?? ResourcesPrefabManager.Instance.GenerateItem("6400110"), 2)
                .AddIngredient(ResourcesPrefabManager.Instance.GetItemPrefab("3000011") ?? ResourcesPrefabManager.Instance.GenerateItem("3000010"))
                .AddResult(3100081);

            var recipes = new List<Recipe>()
            {
                armorRecipe,
                helmRecipe
            };

            foreach (var ingredient in armorRecipe.Ingredients)
            {
                ingredient.AddedIngredient.hideFlags = HideFlags.HideAndDontSave;
                UnityEngine.Object.DontDestroyOnLoad(ingredient.AddedIngredient.gameObject);
            }

            craftingModule.RegisterRecipe<AdvancedCraftingMenu>(armorRecipe);
            craftingModule.RegisterRecipe<AdvancedCraftingMenu>(helmRecipe);

            var levelRoutine = new LevelCoroutines(plugin, loggerFactory);
            levelRoutine.InvokeAfterLevelAndPlayersLoaded(NetworkLevelLoader.Instance, () =>
            {
                foreach (var r in recipes)
                {
                    var character = SplitScreenManager.Instance.LocalPlayers[0].AssignedCharacter;
                    if (!character.Inventory.RecipeKnowledge.IsRecipeLearned(r.UID))
                    {
                        character.Inventory.RecipeKnowledge.LearnRecipe(r);
                    }
                }
            }, 500, 1);
        }
#endif
    }
}
