﻿using ModifAmorphic.Outward.Modules.Crafting;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace ModifAmorphic.Outward.Transmorphic.Enchanting.Recipes
{
    public class EnchantRecipe : CustomRecipe
    {
        [SerializeField]
        public EnchantmentRecipe BaseEnchantmentRecipe;

        [SerializeField]
        public DynamicCraftingResult DefaultCraftingResult;

        public ItemReferenceQuantity[] GetDefaultCraftingResults() =>
            new ItemReferenceQuantity[] { new DynamicCraftingResult(DefaultCraftingResult.ResultService, DefaultCraftingResult.ItemID, DefaultCraftingResult.Quantity) };

    }
}
