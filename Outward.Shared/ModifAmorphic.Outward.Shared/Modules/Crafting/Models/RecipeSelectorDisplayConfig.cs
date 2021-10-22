using System;
using System.Collections.Generic;
using System.Text;

namespace ModifAmorphic.Outward.Modules.Crafting
{
    public class RecipeSelectorDisplayConfig
    {
        public ExtraIngredientSlotOptions ExtraIngredientSlotOption { get; set; } = ExtraIngredientSlotOptions.None;
        public List<StaticIngredient> StaticIngredients { get; set; }
    }
    public class StaticIngredient
    {
        public Guid IngredientID;
        public RecipeIngredient.ActionTypes ActionType;
        public int SpecificItemID;
        public TagSourceSelector IngredientTypeSelector;
        public IngredientSlotPositions IngredientSlotPosition = IngredientSlotPositions.Any;
    }
}
