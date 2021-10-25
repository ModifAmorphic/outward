using ModifAmorphic.Outward.Modules.Crafting;
using System;
using System.Collections.Generic;
using System.Text;

namespace ModifAmorphic.Outward.Transmorphic.Menu
{
    internal class AdvancedCraftingMenu : CustomCraftingMenu
    {
        private string _modId = ModInfo.ModId;
        protected override string ModId { get => _modId; set => _modId = value; }

        public AdvancedCraftingMenu() =>
            (LoggerFactory) = 
            (() => Logging.LoggerFactory.GetLogger(ModInfo.ModId));
    }
}
