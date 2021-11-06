using System;
using System.Collections.Generic;
using System.Text;

namespace ModifAmorphic.Outward.Modules.Crafting
{ 
    internal class RecipeMetadata
    {
        public Recipe Recipe { get; set; }
        public Recipe.CraftingType CustomCraftingType { get; set; }
        public Type CraftingStationType { get; set; }
    }
}
