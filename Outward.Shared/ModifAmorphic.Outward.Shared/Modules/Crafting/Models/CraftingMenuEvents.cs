using System;
using System.Collections.Generic;
using System.Text;

namespace ModifAmorphic.Outward.Modules.Crafting
{
    public class CraftingMenuEvents
    {
        public delegate void MenuHidingDelegate(CustomCraftingMenu menu);
        public event MenuHidingDelegate MenuHiding;
        
        public delegate void MenuLoadedDelegate(CustomCraftingMenu menu);
        public event MenuLoadedDelegate MenuLoaded;


        internal void InvokeMenuHiding(CustomCraftingMenu menu) => MenuHiding?.Invoke(menu);
        internal void InvokeMenuLoading(CustomCraftingMenu menu) => MenuLoaded?.Invoke(menu);
    }
}
