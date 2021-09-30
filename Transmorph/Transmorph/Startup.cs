using BepInEx;
using ModifAmorphic.Outward.Extensions;
using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.Modules;
using ModifAmorphic.Outward.Modules.Crafting;
using ModifAmorphic.Outward.Modules.Items;
using ModifAmorphic.Outward.Transmorph.Menu;
using ModifAmorphic.Outward.Transmorph.Settings;
using System;
using System.Collections.Generic;
using System.Text;
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
            
            var recipeField = typeof(CraftingMenu).GetField("m_freeRecipesLocKey", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            var freeRecipesLocKey = (string[])recipeField.GetValue(null);
            Array.Resize(ref freeRecipesLocKey, freeRecipesLocKey.Length + 1);
            freeRecipesLocKey[freeRecipesLocKey.Length - 1] = "CraftingMenu_FreeRecipe_Transmorph";
            recipeField.SetValue(null, freeRecipesLocKey);

            services.AddSingleton(settingsService.ConfigureSettings())
                .AddFactory(() => LoggerFactory.GetLogger(ModInfo.ModId))
                .AddSingleton(ModifModules.GetPreFabricatorModule(ModInfo.ModId))
                .AddSingleton(ModifModules.GetItemVisualizerModule(ModInfo.ModId))
                .AddSingleton(ModifModules.GetCustomCraftingModule(ModInfo.ModId))
                .AddSingleton(new Transmorpher(services.GetService<BaseUnityPlugin>(),
                                services.GetService<CustomCraftingModule>(),
                                services.GetService<ItemVisualizer>(),
                                services.GetService<TransmorphConfigSettings>(),
                                services.GetService<IModifLogger>
                ));
            services.GetService<CustomCraftingModule>().RegisterCraftingMenu<CookingMenu>("Cooking");
            services.GetService<CustomCraftingModule>().RegisterCraftingMenu<AlchemyMenu>("Alchemy");
            services.GetService<CustomCraftingModule>().RegisterCraftingMenu<TransmorphMenu>("Transmorph");
            services.GetService<CustomCraftingModule>().RegisterCraftingMenu<FashionMenu>("Fashion");
            services.GetService<Transmorpher>().SetTransmorph(3100080, "zsMOujD2ykSsRZvKVu3ifQ");

            RecipeManagerPatches.LoadCraftingRecipeAfter += (r) => AddRecipe(services.GetService<CustomCraftingModule>());

            //services.ConfigureStashPackNet();
            //BagStateService.ConfigureNet(services.GetService<StashPackNet>());

            //services.AddSingleton(
            //    settingsService.ConfigureHostSettings(
            //        services.GetService<StashPacksConfigSettings>(),
            //        services.GetService<StashPackNet>())
            //    );

            //services.AddSingleton(new PlayerCoroutines(
            //    services.GetService<BaseUnityPlugin>(),
            //    services.GetServiceFactory<IModifLogger>()));
            //services.AddSingleton(new LevelCoroutines(
            //    services.GetService<BaseUnityPlugin>(),
            //    services.GetServiceFactory<IModifLogger>()));
            //services.AddSingleton(new ItemCoroutines(
            //    services.GetService<BaseUnityPlugin>(),
            //    () => ItemManager.Instance,
            //    services.GetServiceFactory<IModifLogger>()));

            //var instanceFactory = new InstanceFactory(services);

            //var actionInstances = new ActionInstanceManager(instanceFactory, services.GetService<IModifLogger>);
            //actionInstances.StartActions();

        }

        private void AddRecipe(CustomCraftingModule craftingModule)
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

            foreach (var ingredient in armorRecipe.Ingredients)
            {
                ingredient.AddedIngredient.hideFlags = HideFlags.HideAndDontSave;
                UnityEngine.Object.DontDestroyOnLoad(ingredient.AddedIngredient.gameObject);
            }

            craftingModule.RegisterRecipe<FashionMenu>(armorRecipe);
            craftingModule.RegisterRecipe<FashionMenu>(helmRecipe);
        }
        private void AddRecipePoc(RecipeManager recipeManager)
        {
            _services.GetService<IModifLogger>().LogDebug($"TagSourceManager.Instance {(TagSourceManager.Instance == null ? "IS" : "IS NOT")} null.");
            var m_craftingStationIngredientTags = TagSourceManager.Instance.GetPrivateField<TagSourceManager, TagSourceSelector[]>("m_craftingStationIngredientTags");

            int expand = Enum.GetValues(typeof(Recipe.CraftingType)).Length - m_craftingStationIngredientTags.Length + 1;
            Array.Resize(ref m_craftingStationIngredientTags, m_craftingStationIngredientTags.Length + expand);

            m_craftingStationIngredientTags[m_craftingStationIngredientTags.Length - 1] = m_craftingStationIngredientTags[2];
            TagSourceManager.Instance.SetPrivateField("m_craftingStationIngredientTags", m_craftingStationIngredientTags);
            _services.GetService<IModifLogger>().LogDebug($"Expanded m_craftingStationIngredientTags by {expand}." +
                $" Added new tag with index of {m_craftingStationIngredientTags.Length - 1}.");

            var ingredients = new RecipeIngredient[2]
                {
                    new RecipeIngredient()
                    {
                        ActionType = RecipeIngredient.ActionTypes.AddSpecificIngredient,
                        AddedIngredient = ResourcesPrefabManager.Instance.GetItemPrefab("6400110") ?? ResourcesPrefabManager.Instance.GenerateItem("6400110")
                    },
                    new RecipeIngredient()
                    {
                        ActionType = RecipeIngredient.ActionTypes.AddSpecificIngredient,
                        AddedIngredient = ResourcesPrefabManager.Instance.GetItemPrefab("3000010") ?? ResourcesPrefabManager.Instance.GenerateItem("3000010")
                    }
                };
            foreach(var i in ingredients)
            {
                i.AddedIngredient.hideFlags = HideFlags.HideAndDontSave;
                UnityEngine.Object.DontDestroyOnLoad(i.AddedIngredient.gameObject);
            }
            
            var recipe = ScriptableObject.CreateInstance<Recipe>();
            recipe.SetCraftingType(TransmorphConstants.FashionRecipeType); //Recipe.CraftingType.Survival
            recipe.SetRecipeID(-130310000);
            recipe.SetRecipeName("Blue Sand Armor Transmog");
            recipe.name = recipe.RecipeID + "_" + "Blue Sand Armor Transmog".Replace(" ", "_");
            recipe.SetRecipeIngredients(ingredients);
            recipe.SetRecipeResults(new int[1] { 3100080 }, new int[] { 1 });
            recipe.SetPrivateField<Recipe, UID>("m_uid", new UID("D_AK74Pm6U23iSwrH13ORA"));

            //services.GetService<PreFabricator>().CreatePrefab(5700339, -130310000, "Transmog - Blue Sand Armor", "Transmog");


            var m_recipes = recipeManager.GetPrivateField<RecipeManager, Dictionary<string, Recipe>>("m_recipes");
            var m_recipeUIDsPerUstensils = recipeManager.GetPrivateField<RecipeManager, Dictionary<Recipe.CraftingType, List<UID>>>("m_recipeUIDsPerUstensils");

            m_recipes.Add(recipe.UID, recipe);
            if (m_recipeUIDsPerUstensils.TryGetValue(recipe.CraftingStationType, out var recipeUIDs))
                recipeUIDs.Add(recipe.UID);
            else
            {
                m_recipeUIDsPerUstensils.Add(recipe.CraftingStationType, new List<UID>() { recipe.UID });
            }
        }
    }
}
