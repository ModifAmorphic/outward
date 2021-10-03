using ModifAmorphic.Outward.Modules.Crafting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModifAmorphic.Outward.Extensions
{
    public static class RecipeExtensions
    {
        /// <summary>
        /// Sets the RecipeID by calling the <see cref="Recipe.SetRecipeID(int)"/> on the <paramref name="recipe"/> instance.
        /// </summary>
        /// <returns>The <paramref name="recipe"/>.</returns>
        public static T SetRecipeIDEx<T>(this T recipe, int recipeID) where T : Recipe
        {
            recipe.SetRecipeID(recipeID);
            return recipe;
        }
        /// <summary>
        /// Sets the CraftingType by calling the <see cref="Recipe.SetCraftingType(Recipe.CraftingType)"/> on the <paramref name="recipe"/> instance.
        /// </summary>
        /// <returns>The <paramref name="recipe"/>.</returns>
        public static T SetCraftingTypeEx<T>(this T recipe, Recipe.CraftingType craftingType) where T : Recipe
        {
            recipe.SetCraftingType(craftingType);
            return recipe;
        }
        public static T AddIngredient<T>(this T recipe, Item ingredient) where T : Recipe
        {
            var ingredients = recipe.Ingredients?.ToList() ?? new List<RecipeIngredient>();
            ingredients.Add(new RecipeIngredient()
            {
                ActionType = RecipeIngredient.ActionTypes.AddSpecificIngredient,
                AddedIngredient = ingredient
            });

            recipe.SetRecipeIngredients(ingredients.ToArray());

            return recipe;
        }
        public static T AddIngredient<T>(this T recipe, Item ingredient, int quantity) where T : Recipe
        {
            for (int i = 0; i < quantity; i++)
                recipe.AddIngredient(ingredient);

            return recipe;
        }
        public static T AddIngredient<T>(this T recipe, TagSourceSelector ingredientTypeSelector) where T : Recipe
        {
            var ingredients = recipe.Ingredients?.ToList() ?? new List<RecipeIngredient>();
            ingredients.Add(new RecipeIngredient()
            {
                ActionType = RecipeIngredient.ActionTypes.AddGenericIngredient,
                AddedIngredientType = ingredientTypeSelector
            });

            recipe.SetRecipeIngredients(ingredients.ToArray());

            return recipe;
        }
        public static T SetUID<T>(this T recipe, UID uid) where T : Recipe
        {
            recipe.SetPrivateField<Recipe, UID>("m_uid", uid);
            return recipe;
        }
        public static T SetUID<T>(this T recipe, string uid) where T : Recipe
        {
            recipe.SetUID(new UID(uid));
            return recipe;
        }

        public static T AddResult<T>(this T recipe, int itemID, int quantity = 1) where T : Recipe
        {
            var itemIds = recipe.Results?.Select(r => r.ItemID).ToList() ?? new List<int>();
            var quantities = recipe.Results?.Select(r => r.Quantity).ToList() ?? new List<int>();

            itemIds.Add(itemID);
            quantities.Add(quantity);

            recipe.SetRecipeResults(itemIds.ToArray(), quantities.ToArray());

            return recipe;
        }

        public static T AddDynamicResult<T>(this T recipe, IDynamicResultService service, int itemID, int quantity = 1) where T : CustomRecipe
        {
            var result = new DynamicCraftingResult(service, itemID, quantity);

            recipe.SetRecipeResult(result);

            return recipe;
        }

        public static T SetNames<T>(this T recipe, string recipeName) where T : Recipe
        {
            if (recipe.RecipeID == default)
                throw new InvalidOperationException($"The {nameof(recipe)} instance's RecipeID has not been set. A Recipe's object " +
                    $"name is partially derived from the Recipe's RecipeID. Either set the RecipeID prior to setting the name, " +
                    $"or provide the objectName.");
            return recipe.SetNames(recipeName, recipe.RecipeID + "_" + recipeName.Replace(" ", "_"));
        }
        public static T SetNames<T>(this T recipe, string recipeName, string objectName) where T : Recipe
        {
            recipe.SetRecipeName(recipeName);
            recipe.name = objectName;

            return recipe;
        }
    }
}
