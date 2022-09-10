using ModifAmorphic.Outward.Extensions;
using ModifAmorphic.Outward.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ModifAmorphic.Outward.Modules.Crafting.Services
{
    internal class CustomRecipeService
    {
        private readonly Func<RecipeManager> _recipeManagerFactory;
        private RecipeManager _recipeManager => _recipeManagerFactory.Invoke();

        private readonly Func<IModifLogger> _loggerFactory;
        private IModifLogger Logger => _loggerFactory.Invoke();

        public CustomRecipeService(Func<RecipeManager> recipeManagerFactory, Func<IModifLogger> loggerFactory) =>
            (_recipeManagerFactory, _loggerFactory) = (recipeManagerFactory, loggerFactory);

        public void AddRecipe(Recipe recipe)
        {
            var m_recipes = _recipeManager.GetPrivateField<RecipeManager, Dictionary<string, Recipe>>("m_recipes");

            m_recipes.Add(recipe.UID, recipe);
            var stationRecipes = AddOrGetCraftingStationRecipes(recipe.CraftingStationType);
            stationRecipes.Add(recipe.UID);
            Logger.LogDebug($"{nameof(CustomRecipeService)}:{nameof(AddRecipe)}:: Added recipe '{recipe.UID} - {recipe.Name}' to RecipeManager for CraftingStationType {recipe.CraftingStationType}.");
        }
        public void AddRecipes(IEnumerable<Recipe> recipes)
        {
            Logger.LogDebug($"{nameof(CustomRecipeService)}:{nameof(AddRecipe)}:: Adding {recipes.Count()} recipes to RecipeManager.");
            foreach (var r in recipes)
                AddRecipe(r);
        }
        public List<UID> AddOrGetCraftingStationRecipes(Recipe.CraftingType craftingStationType)
        {
            var m_recipeUIDsPerUstensils = _recipeManager.GetPrivateField<RecipeManager, Dictionary<Recipe.CraftingType, List<UID>>>("m_recipeUIDsPerUstensils");
            if (!m_recipeUIDsPerUstensils.ContainsKey(craftingStationType))
                m_recipeUIDsPerUstensils.Add(craftingStationType, new List<UID>());

            return m_recipeUIDsPerUstensils[craftingStationType];
        }

    }
}
