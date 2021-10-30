﻿using BepInEx;
using ModifAmorphic.Outward.Coroutines;
using ModifAmorphic.Outward.Extensions;
using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.Modules.Crafting;
using ModifAmorphic.Outward.Modules.Items;
using ModifAmorphic.Outward.Transmorphic.Enchanting.Ingredients;
using ModifAmorphic.Outward.Transmorphic.Enchanting.Recipes;
using ModifAmorphic.Outward.Transmorphic.Enchanting.Results;
using ModifAmorphic.Outward.Transmorphic.Enchanting.SaveData;
using ModifAmorphic.Outward.Transmorphic.Menu;
using ModifAmorphic.Outward.Transmorphic.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ModifAmorphic.Outward.Transmorphic.Enchanting
{
    internal class EnchantingStartup : IStartable
    {
        private ServicesProvider _services;
        private SettingsService _settingsService;
        private readonly ConfigSettings _config;

        private IModifLogger Logger => _loggerFactory.Invoke();
        private Func<IModifLogger> _loggerFactory;

        public EnchantingStartup(ServicesProvider services, SettingsService settingsService, ConfigSettings config) =>
            (_services, _settingsService, _config) = (services, settingsService, config);

        public void Start()
        {
            _loggerFactory = _services.GetServiceFactory<IModifLogger>();
            _services
               .AddSingleton(_settingsService.ConfigureEnchantingSettings(_config));
            
            ConfigureRecipes();
            ConfigureMenu();
        }
        private void ConfigureRecipes()
        {
            _services
                .AddSingleton(new EnchantResultService(_services.GetService<IModifLogger>))
                .AddSingleton(new EnchantRecipeData(System.IO.Path.GetDirectoryName(
                                                    _services.GetService<BaseUnityPlugin>().Config.ConfigFilePath),
                                                    _services.GetService<IModifLogger>)
                )
                .AddSingleton(new EnchantRecipeGenerator(
                                _services.GetService<EnchantResultService>(),
                                _services.GetService<PreFabricator>(),
                                _services.GetService<EnchantingSettings>(),
                                _services.GetService<IModifLogger>
                    ))
                .AddSingleton(new EnchantRecipeService(_services.GetService<BaseUnityPlugin>(),
                                _services.GetService<EnchantRecipeGenerator>(),
                                _services.GetService<CustomCraftingModule>(),
                                _services.GetService<LevelCoroutines>(),
                                _services.GetService<EnchantRecipeData>(),
                                _services.GetService<EnchantingSettings>(),
                                _services.GetService<IModifLogger>
                ));
        }
        private void ReEquip(CharacterInventory inventory, Equipment equipment)
        {
            var coroutines = _services.GetService<LevelCoroutines>();
            Func<bool> isUnequipped = () =>
            {
                return !(equipment.IsEquipped && inventory.Equipment.IsHandFree(equipment));
            };
            Action reEquip = () => inventory.EquipItem(equipment);

            coroutines.StartRoutine(
                coroutines.InvokeAfter(isUnequipped, reEquip, 5, .1f)
            );
        }
        private void ConfigureMenu()
        {
            //_services.AddSingleton(new MenuIngredientMatcher(
            //                        new List<ITransmogMatcher>()
            //                        {
            //                            new ArmorMatcher(_services.GetService<IModifLogger>),
            //                            new WeaponMatcher(_services.GetService<IModifLogger>),
            //                            new BagMatcher(_services.GetService<IModifLogger>),
            //                            new LexiconMatcher(_services.GetService<IModifLogger>),
            //                            new LanternMatcher(_services.GetService<IModifLogger>),
            //                            new RemoverMatcher(_services.GetService<IModifLogger>)
            //                        }
            //                        , _services.GetService<IModifLogger>)
            //          )
            //         .AddSingleton(new TransmogCrafter(_services.GetService<ItemVisualizer>(),
            //                            _services.GetService<IModifLogger>)
            //          );
            _services.AddSingleton(new EnchantIngredientMatcher(_services.GetService<IModifLogger>))
                     .AddSingleton(new EnchantCrafter(ReEquip, _services.GetService<IModifLogger>));

            var craftingModule = _services.GetService<CustomCraftingModule>();
            var settings = _services.GetService<EnchantingSettings>();

            craftingModule.RegisterCraftingMenu<EnchantingMenu>("Enchant", EnchantingSettings.EnchantMenuIcons);
            craftingModule.RegisterMenuIngredientFilters<EnchantingMenu>(EnchantingSettings.MenuFilters);
            craftingModule.RegisterRecipeSelectorDisplayConfig<EnchantingMenu>(EnchantingSettings.RecipeSelectorDisplayConfig);
            craftingModule.RegisterStaticIngredients<EnchantingMenu>(EnchantingSettings.StaticIngredients);
            craftingModule.RegisterCompatibleIngredientMatcher<EnchantingMenu>(_services.GetService<EnchantIngredientMatcher>());

            craftingModule.RegisterConsumedItemSelector<EnchantingMenu>(_services.GetService<EnchantIngredientMatcher>());
            craftingModule.RegisterCustomCrafter<EnchantingMenu>(_services.GetService<EnchantCrafter>());


            settings.EnchantingMenuEnabledChanged += (isEnabled) => ToggleCraftingMenu<EnchantingMenu>(craftingModule, isEnabled);
            craftingModule.MenuLoaded += (menu) =>
            {
                if (menu is EnchantingMenu)
                {
                    ToggleCraftingMenu<EnchantingMenu>(craftingModule, settings.EnchantingMenuEnabled);
                    DumpEnchantments();
                }
            };

        }
        private void ToggleCraftingMenu<T>(CustomCraftingModule craftingModule, bool isEnabled) where T : CustomCraftingMenu
        {
            if (isEnabled)
            {
                craftingModule.EnableCraftingMenu<T>();
                Logger.LogInfo("Enabled Enchanting Crafting menu.");
            }
            else
            {
                craftingModule.DisableCraftingMenu<T>();
                Logger.LogInfo("Disabled Enchanting Crafting menu.");
            }
        }
        private Dictionary<int, EnchantmentRecipeItem> GetEnchantmentRecipeItem()
        {
            var field = typeof(ResourcesPrefabManager).GetField("ITEM_PREFABS", BindingFlags.NonPublic | BindingFlags.Static);
            var itemPrefabs = (Dictionary<string, Item>)field.GetValue(null);

            var recipeItems = itemPrefabs.Values.Select(p => p as EnchantmentRecipeItem).Where(r => r != null && r.Recipes?.Length > 0);

            var enchantsRecipes = new Dictionary<int, EnchantmentRecipeItem>();

            foreach (var i in recipeItems)
            {
                foreach(var r in i.Recipes)
                {
                    if (!enchantsRecipes.ContainsKey(r.RecipeID))
                        enchantsRecipes.Add(r.RecipeID, i);
                }
            }

            PatchRecipeItems(ref enchantsRecipes);

            return enchantsRecipes;
        }
        private void PatchRecipeItems(ref Dictionary<int, EnchantmentRecipeItem> recipesItems)
        {
            //fix for Formless EnchantmentRecipeItem containing 2 Helm Recipes instead of 1 Helm, 1 Boots
            if (recipesItems.TryGetValue(75, out var formless))
                recipesItems.Add(76, formless);

            //fix for Filter EnchantmentRecipeItem Missing Helm and Boots Recipes
            if (recipesItems.TryGetValue(52, out var filter))
            {
                recipesItems.Add(53, filter); //53_Filter(Helmet)_EnchantmentRecipe
                recipesItems.Add(54, filter); //54_Filter(Boots)_EnchantmentRecipe
            }
        }
        private void DumpEnchantments()
        {
            var recipeItems = GetEnchantmentRecipeItem();

            var sb = new StringBuilder();
            sb.AppendLine("Start Enchantment Recipe Dump:");
            var recipes = RecipeManager.Instance.GetEnchantmentRecipes();
            sb.AppendLine("RecipeName, RecipeID, RecipeItemID, RecipeItemName, Pillar1Ingredient, Pillar1Type, Pillar1IngredientName, Pillar2Ingredient, Pillar2Type, Pillar2IngredientName, " +
                "Pillar3Ingredient, Pillar3Type, Pillar3IngredientName, Pillar4Ingredient, Pillar4Type, Pillar4IngredientName, " +
                "EquipmentTag, IngredientType, SpecificItemID, SpecificItem, IngredientTag");
            foreach (var r in recipes)
            {
                recipeItems.TryGetValue(r.RecipeID, out var recipeItem);
                sb.Append($"\"{r.name}\", {r.RecipeID}, {recipeItem?.ItemID}, {recipeItem?.DisplayName ?? "No Recipe Item Found"}");
                for (var i = 0; i < 4; i++)
                {
                    if (r.PillarDatas.Length > i
                        && r.PillarDatas[i].CompatibleIngredients != null
                        && r.PillarDatas[i].CompatibleIngredients.Length == 1)
                    {
                        var item = r.PillarDatas[i].CompatibleIngredients[0].SpecificIngredient;
                        var itemType = r.PillarDatas[i].CompatibleIngredients[0].Type;
                        if (item != null)
                            sb.Append($", {item.ItemID}, {itemType}, \"{item.DisplayName}\"");
                        else
                            sb.Append(",,,");
                    }
                    else if (r.PillarDatas.Length > i
                        && r.PillarDatas[i].CompatibleIngredients != null
                        && r.PillarDatas[i].CompatibleIngredients.Length > 1)
                    {
                        sb.Append("###################UNEXPECTED MULTIPLE INGREDIENTS FOR LAST COMPATIBLE INGREDIENTS############################");
                    }
                    else
                        sb.Append(",,,");

                }
                sb.Append($", {r.CompatibleEquipments.EquipmentTag?.Tag.TagName}");
                var equips = r.CompatibleEquipments.CompatibleEquipments;
                for (var i = 0; i < equips.Length; i++)
                {
                    //if (equips.Length > i)
                    sb.Append($", {equips[i].Type.GetName()}, {equips[i].SpecificIngredient?.ItemID}, {equips[i].SpecificIngredient?.DisplayName}, {equips[i].IngredientTag?.Tag.TagName}");
                    //else
                    //  sb.Append(",,,");
                }
                sb.AppendLine();
                //if (r.CompatibleEquipments.CompatibleEquipments.Length > 2)
                //    Logger.LogError("Contained more than 2 equipment ingredients!");
            }

            Logger.LogDebug(sb.ToString());
        }
    }
}