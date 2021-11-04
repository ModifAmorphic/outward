using ModifAmorphic.Outward.Extensions;
using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.Modules.Crafting;
using ModifAmorphic.Outward.Transmorphic.Enchanting.Results;
using ModifAmorphic.Outward.Transmorphic.Extensions;
using ModifAmorphic.Outward.Transmorphic.Settings;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ModifAmorphic.Outward.Transmorphic.Enchanting.Recipes
{
    internal class EnchantRecipeGenerator
    {
        private readonly EnchantingSettings _settings;

        private IModifLogger Logger => _getLogger.Invoke();
        private readonly Func<IModifLogger> _getLogger;

        private readonly IDynamicResultService _enchantResultService;
        private readonly EnchantPrefabs _enchantResultFabricator;

        public EnchantRecipeGenerator(
                                IDynamicResultService enchantResultService,
                                EnchantPrefabs enchantResultFabricator,
                                EnchantingSettings settings, Func<IModifLogger> getLogger)
        {
            (_enchantResultService, _enchantResultFabricator, _settings, _getLogger) =
                (enchantResultService, enchantResultFabricator, settings, getLogger);

        }

        public EnchantRecipe GetEnchantRecipe(EnchantmentRecipe enchantmentRecipe)
        {
            var enchantment = ResourcesPrefabManager.Instance.GetEnchantmentPrefab(enchantmentRecipe.ResultID);
            var enchantRecipe = ScriptableObject.CreateInstance<EnchantRecipe>();
            var enchantResultPrefab = _enchantResultFabricator.GetOrCreateResultPrefab(enchantmentRecipe, enchantment);
            Logger.LogDebug($"Got prefab {enchantResultPrefab.name}. GenerateItemNetwork({enchantResultPrefab.ItemID})");
            var resultItem = (Equipment)ItemManager.Instance.GenerateItemNetwork(enchantResultPrefab.ItemID);
            Logger.LogDebug($"Created item {resultItem.name} from prefab {enchantResultPrefab.name}.");
            var recipe = ConfigureEnchantRecipe<EnchantRecipe>(enchantmentRecipe, enchantment, resultItem, enchantRecipe);
            resultItem.gameObject.SetActive(true);
            Logger.LogDebug($"Configured recipe {recipe.Name} with result item {resultItem.name} from prefab {enchantResultPrefab.name}.");
            return recipe;
        }
        public T ConfigureEnchantRecipe<T>(EnchantmentRecipe recipeSource, Enchantment enchantment, Equipment equipment, T recipe) where T : EnchantRecipe
        {
            var equipTypeName = recipeSource.CompatibleEquipments.EquipmentTag.Tag.TagName;

            var pillarIngredients = new List<RecipeIngredient>();

            foreach (var equip in recipeSource.PillarDatas)
            {
                if (equip.CompatibleIngredients.Length == 1)
                {
                    var ingredient = equip.CompatibleIngredients[0];
                    pillarIngredients.Add(new RecipeIngredient()
                    {
                        ActionType = ingredient.Type.ToActionType(),
                        AddedIngredientType = ingredient.IngredientTag,
                        AddedIngredient = ingredient.SpecificIngredient
                    });
                }
            }

            recipe
                .SetRecipeIDEx(GetRecipeID(recipeSource.RecipeID))
                .SetUID(UID.Generate())
                .SetNames(enchantment.Name + $" ({equipTypeName})")
                .AddDynamicResult(_enchantResultService, equipment.ItemID, 1);

            recipe.SetRecipeIngredients(pillarIngredients.ToArray());
            recipe.BaseEnchantmentRecipe = recipeSource;

            return recipe;
        }
        public static int GetRecipeID(int recipeID)
        {
            return EnchantingSettings.RecipeStartID - recipeID;
        }
        
    }
}
