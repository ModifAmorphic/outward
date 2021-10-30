using ModifAmorphic.Outward.Extensions;
using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.Modules.Crafting;
using ModifAmorphic.Outward.Modules.Items;
using ModifAmorphic.Outward.Transmorphic.Extensions;
using ModifAmorphic.Outward.Transmorphic.Settings;
using ModifAmorphic.Outward.Transmorphic.Transmog.Recipes;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ModifAmorphic.Outward.Transmorphic.Enchanting.Recipes
{
    internal class EnchantRecipeGenerator
    {
        private readonly EnchantingSettings _settings;

        private IModifLogger Logger => _getLogger.Invoke();
        private readonly Func<IModifLogger> _getLogger;

        private readonly IDynamicResultService _enchantResultService;
        private readonly PreFabricator _preFabricator;

        public EnchantRecipeGenerator(
                                IDynamicResultService enchantResultService,
                                PreFabricator preFabricator,
                                EnchantingSettings settings, Func<IModifLogger> getLogger)
        {
            (_enchantResultService, _preFabricator, _settings, _getLogger) =
                (enchantResultService, preFabricator, settings, getLogger);

        }

        public EnchantRecipe GetEnchantRecipe(EnchantmentRecipe enchantmentRecipe)
        {
            var enchantment = ResourcesPrefabManager.Instance.GetEnchantmentPrefab(enchantmentRecipe.ResultID);
            var enchantRecipe = ScriptableObject.CreateInstance<EnchantRecipe>();
            var enchantResultItem = CreateRecipeResult(enchantmentRecipe, enchantment);
            return ConfigureEnchantRecipe<EnchantRecipe>(enchantmentRecipe, enchantment, enchantResultItem, enchantRecipe);

        }
        private (string Name, int ItemID, Type ItemType) GetResult(EnchantmentRecipe recipeSource, Enchantment enchantment)
        {
            int itemId = recipeSource.CompatibleEquipments.CompatibleEquipments[0].SpecificIngredient?.ItemID ?? -1;
            var tag = recipeSource.CompatibleEquipments.EquipmentTag.Tag;
            if (ItemTags.ArmorTag == tag)
                return (enchantment.Name + $" (Armor)", itemId != -1 ? itemId : EnchantingSettings.VirginArmorID, typeof(Armor));
            else if (ItemTags.HelmetTag == tag)
                return (enchantment.Name + $" (Helm)", itemId != -1 ? itemId : EnchantingSettings.VirginHelmetID, typeof(Armor));
            else if (ItemTags.BootsTag == tag)
                return (enchantment.Name + $" (Boots)", itemId != -1 ? itemId : EnchantingSettings.VirginBootsID, typeof(Armor));
            else if (ItemTags.BowTag == tag)
                return (enchantment.Name + $" (Bow)", itemId != -1 ? itemId : EnchantingSettings.SimpleBowID, typeof(Weapon));
            else if (ItemTags.ChakramTag == tag)
                return (enchantment.Name + $" (Chakram)", itemId != -1 ? itemId : EnchantingSettings.ChakramID, typeof(Weapon));
            else if (ItemTags.DagueTag == tag)
                return (enchantment.Name + $" (Dagger)", itemId != -1 ? itemId : EnchantingSettings.RondelDaggerID, typeof(Weapon));
            else if (ItemTags.WeaponTag == tag)
                return (enchantment.Name + $" (Weapon)", itemId != -1 ? itemId : EnchantingSettings.IronSwordID, typeof(Weapon));
            else if (ItemTags.LexiconTag == tag)
                return (enchantment.Name + $" (Lexicon)", itemId != -1 ? itemId : EnchantingSettings.LexiconID, typeof(Equipment));
            else if (recipeSource.RecipeID == 117)
                return (enchantment.Name + $" (Staff / Lexicon)", itemId != -1 ? itemId : EnchantingSettings.CompasswoodStaffID, typeof(Weapon));
            else if (ItemTags.TrinketTag == tag)
            {
                if (itemId == EnchantingSettings.VirginLanternID)
                {
                    return (enchantment.Name + $" (Lantern)", itemId, typeof(Equipment));
                }
                if (itemId == 5110005) // Zhornâ€™s Glowstone Dagger
                {
                    return (enchantment.Name + $" (Dagger)", itemId, typeof(Weapon));
                }
                var ingredientTag = recipeSource.CompatibleEquipments.CompatibleEquipments[0].IngredientTag;
                if (ingredientTag == ItemTags.DagueTag)
                {
                    return (enchantment.Name + $" (Dagger)", itemId != -1 ? itemId : EnchantingSettings.RondelDaggerID, typeof(Weapon));
                }
                if (ingredientTag == ItemTags.PistolTag || itemId == 5110120)  // Ornate Pistol
                {
                    return (enchantment.Name + $" (Pistol)", itemId != -1 ? itemId : EnchantingSettings.FlintlockPistolID, typeof(Weapon));
                }
                if (ingredientTag == ItemTags.ChakramTag)
                {
                    return (enchantment.Name + $" (Chakram)", itemId != -1 ? itemId : EnchantingSettings.ChakramID, typeof(Weapon));
                }
                if (ingredientTag == ItemTags.LexiconTag)
                {
                    return (enchantment.Name + $" (Lexicon)", itemId != -1 ? itemId : EnchantingSettings.LexiconID, typeof(Equipment));
                }
            }
            return (enchantment.Name, itemId != -1 ? itemId : EnchantingSettings.VirginArmorID, typeof(Equipment));
        }
        private Equipment CreateRecipeResult(EnchantmentRecipe recipeSource, Enchantment enchantment)
        {
            (var resultName, var sourceItemId, var equipType) = GetResult(recipeSource, enchantment);
            int resultItemID = GetRecipeID(recipeSource.RecipeID);

            if (!ResourcesPrefabManager.Instance.ContainsItemPrefab(resultItemID.ToString()))
            {
                Equipment resultPreFab = null;
                if (equipType == typeof(Armor))
                    resultPreFab = _preFabricator.CreatePrefab<Armor>(sourceItemId,
                                                                    resultItemID,
                                                                    resultName,
                                                                    enchantment.Description, true);
                else if (equipType == typeof(Weapon))
                    resultPreFab = _preFabricator.CreatePrefab<Weapon>(sourceItemId,
                                                                    resultItemID,
                                                                    resultName,
                                                                    enchantment.Description, true);
                else
                    resultPreFab = _preFabricator.CreatePrefab<Equipment>(sourceItemId,
                                                                    resultItemID,
                                                                    resultName,
                                                                    enchantment.Description, true);
                //.ConfigureItemIcon(TransmogSettings.RemoveRecipe.IconFile);
                //resultPreFab.AddEnchantment(recipeSource.RecipeID);
                Logger.LogDebug($"Created Enchantment placeholder prefab {resultPreFab.ItemID} - {resultPreFab.DisplayName}.");
                
            }
            return (Equipment)ResourcesPrefabManager.Instance.GetItemPrefab(resultItemID);
        }
        //public TransmogWeaponRecipe GetTransmogWeaponRecipe(Weapon weaponSource)
        //{
        //    if (weaponSource == null)
        //    {
        //        return null;
        //    }
        //    var weaponTag = weaponSource.Type.ToWeaponTag();
        //    TagSourceManager.Instance.TryAddTag(weaponTag, true);

        //    return ConfigureEnchantRecipe(weaponSource, weaponTag, ScriptableObject.CreateInstance<TransmogWeaponRecipe>(), _weaponResultService)
        //                .SetWeaponType(weaponSource.Type);
        //}
        //public TransmogBagRecipe GetTransmogBagRecipe(Bag bagSource)
        //{
        //    if (bagSource == null)
        //    {
        //        return null;
        //    }

        //    return ConfigureEnchantRecipe(bagSource, TransmogSettings.BackpackTag, ScriptableObject.CreateInstance<TransmogBagRecipe>(), _bagResultService);
        //}
        //public TransmogLanternRecipe GetTransmogLanternRecipe(Equipment equipSource)
        //{
        //    if (equipSource == null)
        //    {
        //        return null;
        //    }

        //    return ConfigureEnchantRecipe(equipSource, TransmogSettings.LanternTag, ScriptableObject.CreateInstance<TransmogLanternRecipe>(), _lanternResultService);
        //}
        //public TransmogLexiconRecipe GetTransmogLexiconRecipe(Equipment equipSource)
        //{
        //    if (equipSource == null)
        //    {
        //        return null;
        //    }

        //    return ConfigureEnchantRecipe(equipSource, TransmogSettings.LexiconTag, ScriptableObject.CreateInstance<TransmogLexiconRecipe>(), _lexiconResultService);
        //}
        public T ConfigureEnchantRecipe<T>(EnchantmentRecipe recipeSource, Enchantment enchantment, Equipment equipment, T recipe) where T : EnchantRecipe
        {
            var equipTypeName = recipeSource.CompatibleEquipments.EquipmentTag.Tag.TagName;

            var pillarIngredients = new List<RecipeIngredient>();

            foreach (var equip in recipeSource.PillarDatas)
            {
                if (equip.CompatibleIngredients.Length == 1)
                {
                    var ingredient = equip.CompatibleIngredients[0];
                    pillarIngredients.Add(new RecipeIngredient()
                    {
                        ActionType = ingredient.Type.ToActionType(),
                        AddedIngredientType = ingredient.IngredientTag,
                        AddedIngredient = ingredient.SpecificIngredient
                    });
                }
            }

            recipe
                .SetRecipeIDEx(GetRecipeID(recipeSource.RecipeID))
                .SetUID(UID.Generate())
                .SetNames(enchantment.Name + $" ({equipTypeName})")
                .AddDynamicResult(_enchantResultService, equipment.ItemID, 1);

            recipe.SetRecipeIngredients(pillarIngredients.ToArray());
            recipe.BaseEnchantmentRecipe = recipeSource;

            return recipe;
        }
        public int GetRecipeID(int recipeID)
        {
            return EnchantingSettings.RecipeStartID - recipeID;
        }
    }
}
