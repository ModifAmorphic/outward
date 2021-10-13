using ModifAmorphic.Outward.Modules.Crafting;
using System;
using System.Collections.Generic;
using System.Text;

namespace ModifAmorphic.Outward.Transmorph
{
    internal class CookingMenu : CustomCraftingMenu
    {
        public CookingMenu() =>
            (PermanentCraftingStationType, LoggerFactory) = (Recipe.CraftingType.Cooking, () => Logging.LoggerFactory.GetLogger(ModInfo.ModId));
    }
}
