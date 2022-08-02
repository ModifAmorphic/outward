using System;

namespace ModifAmorphic.Outward.Modules.Crafting
{
    public class StaticIngredient
    {
        public Guid IngredientID;
        public RecipeIngredient.ActionTypes ActionType;
        public int SpecificItemID;
        public TagSourceSelector IngredientTypeSelector;
        public IngredientSlotPositions IngredientSlotPosition = IngredientSlotPositions.Any;
        public bool CountAsIngredient;
    }
}
