using System;
using System.Collections.Generic;
using System.Text;

namespace ModifAmorphic.Outward.Modules.Crafting
{
    public interface IRecipeVisibiltyController
    {
        bool GetRecipeIsVisible(CustomCraftingMenu menu, Recipe recipe);
    }
}
