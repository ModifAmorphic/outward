﻿using ModifAmorphic.Outward.Modules.Crafting;
using ModifAmorphic.Outward.Transmorph.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModifAmorphic.Outward.Transmorph.Menu
{
    internal class FashionMenu : CustomCraftingMenu
    {

        public new bool IsSurvivalCrafting => true;
        public FashionMenu() => 
            (LoggerFactory) = 
            (
                () => Logging.LoggerFactory.GetLogger(ModInfo.ModId)
            );

        

    }
}
