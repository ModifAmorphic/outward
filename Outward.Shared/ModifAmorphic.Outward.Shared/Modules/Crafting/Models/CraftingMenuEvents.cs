namespace ModifAmorphic.Outward.Modules.Crafting
{
    public class CraftingMenuEvents
    {
        public delegate void MenuHidingDelegate(CustomCraftingMenu menu);
        /// <summary>
        /// Crafting menu is about to be hidden.
        /// </summary>
        public event MenuHidingDelegate MenuHiding;

        public delegate void MenuLoadedDelegate(CustomCraftingMenu menu);
        /// <summary>
        /// Crafting menu has finished loading and is now accessible.
        /// </summary>
        public event MenuLoadedDelegate MenuLoaded;

        public delegate void MenuStartingDelegate(CustomCraftingMenu menu);
        /// <summary>
        /// CraftingMenu's StartInit() method has been called.
        /// </summary>
        public event MenuStartingDelegate MenuStarting;

        public delegate void MenuShowingDelegate(CustomCraftingMenu menu);
        /// <summary>
        /// CraftingMenu's Show() method has been called, and the menu is about to be displayed.
        /// </summary>
        public event MenuShowingDelegate MenuShowing;

        public delegate void DynamicCraftCompleteDelegate(DynamicCraftingResult result, int resultMultiplier, CustomCraftingMenu menu);
        public event DynamicCraftCompleteDelegate DynamicCraftComplete;

        internal void InvokeMenuHiding(CustomCraftingMenu menu) => MenuHiding?.Invoke(menu);
        internal void InvokeMenuLoading(CustomCraftingMenu menu) => MenuLoaded?.Invoke(menu);
        internal void InvokeMenuStarting(CustomCraftingMenu menu) => MenuStarting?.Invoke(menu);
        internal void InvokeMenuShowing(CustomCraftingMenu menu) => MenuShowing?.Invoke(menu);
        internal void InvokeDynamicCraftComplete(DynamicCraftingResult result, int resultMultiplier, CustomCraftingMenu menu)
            => DynamicCraftComplete?.Invoke(result, resultMultiplier, menu);
    }
}
