using System;

namespace ModifAmorphic.Outward.Modules.Crafting
{
    [Serializable]
    public class CustomRecipeIngredient : RecipeIngredient
    {
        /// <summary>
        /// Optional unique identifier for the ingredient.
        /// </summary>
        public Guid CustomRecipeIngredientID { get; set; }
    }
}
