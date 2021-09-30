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
        public static Recipe SetRecipeIDEx(this Recipe recipe, int recipeID)
        {
            recipe.SetRecipeID(recipeID);
            return recipe;
        }
        /// <summary>
        /// Sets the CraftingType by calling the <see cref="Recipe.SetCraftingType(Recipe.CraftingType)"/> on the <paramref name="recipe"/> instance.
        /// </summary>
        /// <returns>The <paramref name="recipe"/>.</returns>
        public static Recipe SetCraftingTypeEx(this Recipe recipe, Recipe.CraftingType craftingType)
        {
            recipe.SetCraftingType(craftingType);
            return recipe;
        }
        public static Recipe AddIngredient(this Recipe recipe, Item ingredient)
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
        public static Recipe AddIngredient(this Recipe recipe, Item ingredient, int quantity)
        {
            for (int i = 0; i < quantity; i++)
                recipe.AddIngredient(ingredient);

            return recipe;
        }
        public static Recipe AddIngredient(this Recipe recipe, TagSourceSelector ingredientTypeSelector)
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
        public static Recipe SetUID(this Recipe recipe, UID uid)
        {
            recipe.SetPrivateField("m_uid", uid);
            return recipe;
        }
        public static Recipe SetUID(this Recipe recipe, string uid)
        {
            recipe.SetUID(new UID(uid));
            return recipe;
        }

        public static Recipe AddResult(this Recipe recipe, int itemID, int quantity = 1)
        {
            var itemIds = recipe.Results?.Select(r => r.ItemID).ToList() ?? new List<int>();
            var quantities = recipe.Results?.Select(r => r.Quantity).ToList() ?? new List<int>();

            itemIds.Add(itemID);
            quantities.Add(quantity);

            recipe.SetRecipeResults(itemIds.ToArray(), quantities.ToArray());

            return recipe;
        }
        public static Recipe SetNames(this Recipe recipe, string recipeName)
        {
            if (recipe.RecipeID == default)
                throw new InvalidOperationException($"The {nameof(recipe)} instance's RecipeID has not been set. A Recipe's object " +
                    $"name is partially derived from the Recipe's RecipeID. Either set the RecipeID prior to setting the name, " +
                    $"or provide the objectName.");
            return recipe.SetNames(recipeName, recipe.RecipeID + "_" + recipeName.Replace(" ", "_"));
        }
        public static Recipe SetNames(this Recipe recipe, string recipeName, string objectName)
        {
            recipe.SetRecipeName(recipeName);
            recipe.name = objectName;

            return recipe;
        }
    }
}
