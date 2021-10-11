using ModifAmorphic.Outward.Modules.Crafting;
using ModifAmorphic.Outward.Transmorph.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModifAmorphic.Outward.Transmorph.Menu
{
    internal class TransmogrifyMenu : CustomCraftingMenu
    {
        public new bool IsSurvivalCrafting => true;
        public TransmogrifyMenu() => 
            (InventoryFilterTag, IncludeEnchantedIngredients, HideFreeCraftingRecipe, LoggerFactory) = 
            (
                new Tag("70", "Item"),
                true, true,
                () => Logging.LoggerFactory.GetLogger(ModInfo.ModId)
            );
    }
}
