using ModifAmorphic.Outward.Modules.Crafting;
using ModifAmorphic.Outward.Transmorphic.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModifAmorphic.Outward.Transmorphic.Menu
{
    internal class TransmogrifyMenu : CustomCraftingMenu
    {
        private string _modId = ModInfo.ModId;
        protected override string ModId { get => _modId; set => _modId = value; }

        public TransmogrifyMenu() => 
            (HideFreeCraftingRecipe, LoggerFactory) = 
            (
                true,
                () => Logging.LoggerFactory.GetLogger(ModInfo.ModId)
            );
    }
}
