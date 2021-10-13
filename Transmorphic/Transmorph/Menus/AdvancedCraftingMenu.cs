using ModifAmorphic.Outward.Modules.Crafting;
using System;
using System.Collections.Generic;
using System.Text;

namespace ModifAmorphic.Outward.Transmorph.Menu
{
    internal class AdvancedCraftingMenu : CustomCraftingMenu
    {
        public AdvancedCraftingMenu() =>
            (LoggerFactory) = 
            (() => Logging.LoggerFactory.GetLogger(ModInfo.ModId));
    }
}
