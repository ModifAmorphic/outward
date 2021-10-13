using ModifAmorphic.Outward.Modules.Crafting;
using System;
using System.Collections.Generic;
using System.Text;

namespace ModifAmorphic.Outward.Transmorph
{
    internal class AlchemyMenu : CustomCraftingMenu
    {
        public AlchemyMenu() =>
            (PermanentCraftingStationType, LoggerFactory) = (Recipe.CraftingType.Alchemy, () => Logging.LoggerFactory.GetLogger(ModInfo.ModId));
    }
}
