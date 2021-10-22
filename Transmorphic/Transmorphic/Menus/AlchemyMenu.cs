using ModifAmorphic.Outward.Modules.Crafting;
using ModifAmorphic.Outward.Transmorphic.Settings;
using System;
using System.Collections.Generic;
using System.Text;

namespace ModifAmorphic.Outward.Transmorphic
{
    internal class AlchemyMenu : CustomCraftingMenu
    {
        private string _modId = ModInfo.ModId;
        protected override string ModId { get => _modId; set => _modId = value; }
        public AlchemyMenu() =>
            (PermanentCraftingStationType,  LoggerFactory) = 
            (Recipe.CraftingType.Alchemy, () => Logging.LoggerFactory.GetLogger(ModInfo.ModId));
    }
}
