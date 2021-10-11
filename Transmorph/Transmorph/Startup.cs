using BepInEx;
using ModifAmorphic.Outward.Coroutines;
using ModifAmorphic.Outward.Extensions;
using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.Modules;
using ModifAmorphic.Outward.Modules.Crafting;
using ModifAmorphic.Outward.Modules.Items;
using ModifAmorphic.Outward.Transmorph.Extensions;
using ModifAmorphic.Outward.Transmorph.Menu;
using ModifAmorphic.Outward.Transmorph.Patches;
using ModifAmorphic.Outward.Transmorph.Settings;
using ModifAmorphic.Outward.Transmorph.Transmog;
using ModifAmorphic.Outward.Transmorph.Transmog.Models;
using ModifAmorphic.Outward.Transmorph.Transmog.SaveData;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ModifAmorphic.Outward.Transmorph
{
    internal class Startup
    {
        private ServicesProvider _services;
        internal void Start(ServicesProvider services)
        {
            _services = services;
            var settingsService = new SettingsService(services.GetService<BaseUnityPlugin>(), ModInfo.MinimumConfigVersion);
            
            //var recipeField = typeof(CraftingMenu).GetField("m_freeRecipesLocKey", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            //var freeRecipesLocKey = (string[])recipeField.GetValue(null);
            //Array.Resize(ref freeRecipesLocKey, freeRecipesLocKey.Length + 1);
            //freeRecipesLocKey[freeRecipesLocKey.Length - 1] = "CraftingMenu_FreeRecipe_Transmorph";
            //recipeField.SetValue(null, freeRecipesLocKey);

            services.AddSingleton(settingsService.ConfigureSettings())
                .AddFactory(() => LoggerFactory.GetLogger(ModInfo.ModId))
                .AddSingleton(ModifModules.GetPreFabricatorModule(ModInfo.ModId))
                .AddSingleton(ModifModules.GetItemVisualizerModule(ModInfo.ModId))
                .AddSingleton(ModifModules.GetCustomCraftingModule(ModInfo.ModId))
                .AddSingleton(new LevelCoroutines(services.GetService<BaseUnityPlugin>(), 
                                                  services.GetService<IModifLogger>))
                //.AddSingleton(new Transmorpher(services.GetService<BaseUnityPlugin>(),
                //                services.GetService<CustomCraftingModule>(),
                //                services.GetService<ItemVisualizer>(),
                //                services.GetService<TransmorphConfigSettings>(),
                //                services.GetService<IModifLogger>
                //))

                .AddSingleton(new IngredientMatcher(services.GetService<IModifLogger>))
                .AddSingleton(new RemoverResultService(services.GetService<IModifLogger>))
                .AddSingleton(new ArmorResultService(services.GetService<IModifLogger>))
                .AddSingleton(new WeaponResultService(services.GetService<IModifLogger>))
                .AddSingleton(new TransmogCrafter(services.GetService<ItemVisualizer>(),
                                services.GetService<IModifLogger>)
                )
                .AddSingleton(new TransmogRecipeData(System.IO.Path.GetDirectoryName(
                                                    services.GetService<BaseUnityPlugin>().Config.ConfigFilePath),
                                                    services.GetService<IModifLogger>)
                )
                .AddSingleton(new TmogRecipeService(services.GetService<BaseUnityPlugin>(),
                                services.GetService<RemoverResultService>(),
                                services.GetService<ArmorResultService>(),
                                services.GetService<WeaponResultService>(),
                                services.GetService<CustomCraftingModule>(),
                                services.GetService<PreFabricator>(),
                                services.GetService<LevelCoroutines>(),
                                services.GetService<TransmogRecipeData>(),
                                services.GetService<TransmorphConfigSettings>(),
                                services.GetService<IModifLogger>
                ))
                .AddSingleton(new TransmogItemListener(
                                services.GetService<ItemVisualizer>(),
                                services.GetService<LevelCoroutines>(),
                                () => ItemManager.Instance,
                                services.GetService<TransmorphConfigSettings>(),
                                services.GetService<IModifLogger>
                ));

            var craftingModule = services.GetService<CustomCraftingModule>();
            craftingModule.RegisterCraftingMenu<CookingMenu>("Cooking");
            craftingModule.RegisterCraftingMenu<AlchemyMenu>("Alchemy");
            craftingModule.RegisterCraftingMenu<AdvancedCraftingMenu>("Advanced");
            craftingModule.RegisterCraftingMenu<TransmogrifyMenu>("Transmogrify");

            craftingModule.RegisterCompatibleIngredientMatcher<TransmogrifyMenu>(services.GetService<IngredientMatcher>());
            craftingModule.RegisterCustomCrafter<TransmogrifyMenu>(services.GetService<TransmogCrafter>());

            //TmogRecipeManagerPatches.LoadCraftingRecipeAfter += (r) =>
            //LoadStartingTransmogRecipes(
            //    services.GetService<BaseUnityPlugin>(),
            //    services.GetService<TmogRecipeService>(),
            //    craftingModule,
            //    services.GetService<IModifLogger>);


            TmogRecipeManagerPatches.LoadCraftingRecipeAfter += (r) => AddAdvancedCraftingRecipes(
                services.GetService<BaseUnityPlugin>(),
                craftingModule,
                services.GetService<IModifLogger>);

            //TestUIDs(services.GetService<IModifLogger>());
        }
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
    }
}
