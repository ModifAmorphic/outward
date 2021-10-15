﻿using ModifAmorphic.Outward.Extensions;
using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.Modules.Crafting;
using ModifAmorphic.Outward.Modules.Items;
using ModifAmorphic.Outward.Transmorph.Extensions;
using ModifAmorphic.Outward.Transmorph.Settings;
using ModifAmorphic.Outward.Transmorph.Transmog.Models;
using ModifAmorphic.Outward.Transmorph.Transmog.Recipes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModifAmorphic.Outward.Transmorph.Transmog
{
    class TransmogCrafter : ICustomCrafter
    {
        private readonly Func<IModifLogger> _loggerFactory;
        private IModifLogger Logger => _loggerFactory.Invoke();

        private readonly ItemVisualizer _itemVisualizer;

        public TransmogCrafter(ItemVisualizer itemVisualizer, Func<IModifLogger> loggerFactory) =>
            (_itemVisualizer, _loggerFactory) = (itemVisualizer, loggerFactory);

        public bool TryCraftItem(Recipe recipe, ItemReferenceQuantity recipeResult, out Item item)
        {
            if (!(recipe is TransmogRecipe || recipe is TransmogRemoverRecipe) 
                || !(recipeResult is DynamicCraftingResult dynamicResult)
                || dynamicResult.DynamicItemID == -1)
            {
                Logger.LogError($"{nameof(TransmogCrafter)}::{nameof(TryCraftItem)}(): " +
                        $"Could not craft item. Either recipe was not a TransmogRecipe or TransmogRemoverRecipe, result was not a Dynamic result or DynamicItemID was not set. " +
                        $"recipe is TransmogRecipe? {recipe is TransmogRecipe}. recipe is TransmogRemoverRecipe? {recipe is TransmogRemoverRecipe}. " +
                        $"recipeResult is DynamicCraftingResult? {recipeResult is DynamicCraftingResult}. " +
                        $"DynamicItemID: {(recipeResult as DynamicCraftingResult)?.DynamicItemID}");
                item = null;
                return false;
            }

            if (recipe is TransmogRecipe transmogRecipe)
            {
                TryCraftTransmog(transmogRecipe, dynamicResult, out item);
            }
            else
            {
                TryCraftRemoveTransmog((TransmogRemoverRecipe)recipe, dynamicResult, out item);
            }

            return true;
        }

        private bool TryCraftTransmog(TransmogRecipe recipe, DynamicCraftingResult dynamicResult, out Item item)
        {
            item = GenerateItemTransmog(new ItemVisualMap() { VisualItemID = recipe.VisualItemID, ItemID = dynamicResult.DynamicItemID });

            if (item is Equipment tmogEquip)
            {
                if (dynamicResult.IngredientCraftData.IngredientEnchantData.TryGetValue(item.ItemID, out var enchantData))
                {
                    tmogEquip.ApplyEnchantmentsFromSaveData(enchantData);
                    Logger.LogDebug($"{nameof(TransmogCrafter)}::{nameof(TryCraftItem)}(): " +
                        $"Ingredient {item.ItemID} was enchanted. " +
                        $"Adding echantments to new transmog item.\n" +
                        $"\tEnchantmentData: {enchantData}");
                }
            }

            Logger.LogInfo($"Crafted new Transmog. Item '{item.DisplayName}' was transmogrified with {dynamicResult.RefItem?.DisplayName} ({recipe.VisualItemID})'s visuals.");

            return true;
        }
        private Item GenerateItemTransmog(ItemVisualMap visualMap)
        {
            var item = ItemManager.Instance.GenerateItem(visualMap.ItemID);

            if (item == null)
                return item;

            var newUID = visualMap.ToUID();

            //This was really unnecessary before, but now there's much more than a 1 in 4294967295 chance, so totally necessary now
            while (ItemManager.Instance.WorldItems.ContainsKey(newUID))
                newUID = visualMap.ToUID();

            item.SetHolderUID(newUID);

            _itemVisualizer.RegisterItemVisual(visualMap.VisualItemID, item.UID);
            _itemVisualizer.RegisterAdditionalIcon(item.UID, TransmogSettings.ItemIconName, TransmogSettings.ItemIconImageFilePath);

            if (PhotonNetwork.isNonMasterClientInRoom)
            {
                item.ClientGenerated = true;
                item.SetKeepAlive();
            }

            return item;
        }

        private bool TryCraftRemoveTransmog(TransmogRemoverRecipe recipe, DynamicCraftingResult dynamicResult, out Item item)
        {
            item = GenerateItemNoTransmog(dynamicResult.DynamicItemID);

            if (dynamicResult.IngredientCraftData.ConsumedItems.TryGetValue(dynamicResult.DynamicItemID, out var counsumed))
            {
                foreach (var uid in counsumed.Keys)
                {
                    _itemVisualizer.UnregisterItemVisual(uid);
                    _itemVisualizer.UnregisterAdditionalIcon(uid, TransmogSettings.ItemIconName);
                }
            }

            if (item is Equipment equip)
            {
                if (dynamicResult.IngredientCraftData.IngredientEnchantData.TryGetValue(item.ItemID, out var enchantData))
                {
                    equip.ApplyEnchantmentsFromSaveData(enchantData);
                    Logger.LogDebug($"{nameof(TransmogCrafter)}::{nameof(TryCraftItem)}(): " +
                        $"Ingredient {item.ItemID} was enchanted. " +
                        $"Adding echantments to new transmog item.\n" +
                        $"\tEnchantmentData: {enchantData}");
                }
            }

            Logger.LogInfo($"Crafted new Item '{item.DisplayName}' without Transmogrification.");

            return true;
        }
        private Item GenerateItemNoTransmog(int itemID)
        {
            var item = ItemManager.Instance.GenerateItem(itemID);

            if (item == null)
                return item;

            if (PhotonNetwork.isNonMasterClientInRoom)
            {
                item.ClientGenerated = true;
                item.SetKeepAlive();
            }

            return item;
        }
    }
}