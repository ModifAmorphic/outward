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
            (InventoryFilterTag, IncludeEnchantedIngredients, LoggerFactory) = 
            (
                new Tag("70", "Item"),
                true,
                () => Logging.LoggerFactory.GetLogger(ModInfo.ModId)
            );
    }
}
