using ModifAmorphic.Outward.Modules.Crafting;
using System;
using System.Collections.Generic;
using System.Text;

namespace ModifAmorphic.Outward.Transmorph.Menu
{
    internal class TransmorphMenu : CustomCraftingMenu
    {
        public TransmorphMenu() =>
            (PermanentCraftingStationType, LoggerFactory) = 
            (Recipe.CraftingType.Survival, () => Logging.LoggerFactory.GetLogger(ModInfo.ModId));
    }
}
