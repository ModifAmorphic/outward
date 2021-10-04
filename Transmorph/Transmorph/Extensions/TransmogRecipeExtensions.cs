using ModifAmorphic.Outward.Extensions;
using ModifAmorphic.Outward.Transmorph.Recipes;
using ModifAmorphic.Outward.Transmorph.Transmog.Recipes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModifAmorphic.Outward.Transmorph.Extensions
{
    internal static class TransmogRecipeExtensions
    {
        public static TransmogRecipe SetVisualItemID(this TransmogRecipe recipe, int itemID)
        {
            recipe.VisualItemID = itemID;
            return recipe;
        }

        public static TransmogRecipe SetNames(this TransmogRecipe recipe, Item sourceItem)
        {
            if (recipe.RecipeID == default)
                throw new InvalidOperationException($"The {nameof(recipe)} instance's RecipeID has not been set. A Recipe's object " +
                    $"name is partially derived from the Recipe's RecipeID. Either set the RecipeID prior to setting the name, " +
                    $"or provide the objectName.");

            return recipe.SetNames("Transmog - " + sourceItem.DisplayName
                , recipe.RecipeID + "_" + sourceItem.name.Replace(" ", "_"));
        }

        public static TransmogArmorRecipe SetEquipmentSlot(this TransmogArmorRecipe recipe, EquipmentSlot.EquipmentSlotIDs equipmentSlot)
        {
            recipe.EquipmentType = equipmentSlot;
            return recipe;
        }
        public static TransmogWeaponRecipe SetWeaponType(this TransmogWeaponRecipe recipe, Weapon.WeaponType weaponType)
        {
            recipe.WeaponType = weaponType;
            return recipe;
        }
    }
}
