using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.Modules.Crafting;
using ModifAmorphic.Outward.Modules.Items;
using ModifAmorphic.Outward.Transmorph.Extensions;
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
            if (!(recipe is TransmogRecipe tmogRecipe) || !(recipeResult is DynamicCraftingResult dynamicResult)
                || dynamicResult.DynamicItemID == -1)
            {
                Logger.LogDebug($"{nameof(TransmogCrafter)}::{nameof(TryCraftItem)}(): " +
                        $"Could not craft item. Either recipe was not a Transmog, result was not a Dynamic result or DynamicItemID was not set. " +
                        $"recipe is TransmogRecipe? {recipe is TransmogRecipe}. recipeResult is DynamicCraftingResult? {recipeResult is DynamicCraftingResult} " +
                        $"DynamicItemID: {(recipeResult as DynamicCraftingResult)?.DynamicItemID}");
                item = null;
                return false;
            }

            item = GenerateItemTransmog(new ItemVisualMap() { VisualItemID = tmogRecipe.VisualItemID, ItemID = dynamicResult.DynamicItemID });

            Logger.LogInfo($"Crafted new Transmog. Item '{item.DisplayName}' was transmogrified with {dynamicResult.RefItem?.DisplayName} ({tmogRecipe.VisualItemID})'s visuals.");

            return true;
        }

        private Item GenerateItemTransmog(ItemVisualMap visualMap)
        {
            var item = ItemManager.Instance.GenerateItem(visualMap.ItemID);

            if (item == null)
                return item;

            //item.Tags.Add(TransmorphConstants.TransmogTagSelector.Tag);

            var newUID = visualMap.ToUID();

            //This was really unnecessary before, but now there's much more than a 1 in 4294967295 chance, so totally necessary now
            while (ItemManager.Instance.WorldItems.ContainsKey(newUID))
                newUID = visualMap.ToUID();

            item.SetHolderUID(newUID);

            _itemVisualizer.RegisterItemVisual(visualMap.VisualItemID, item.UID);

            if (PhotonNetwork.isNonMasterClientInRoom)
            {
                item.ClientGenerated = true;
                item.SetKeepAlive();
            }

            return item;
        }
    }
}
