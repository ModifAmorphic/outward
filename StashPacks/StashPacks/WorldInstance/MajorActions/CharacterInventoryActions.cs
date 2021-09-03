using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.StashPacks.Patch.Events;
using ModifAmorphic.Outward.StashPacks.State;
using System;
using System.Linq;
using System.Reflection;

namespace ModifAmorphic.Outward.StashPacks.WorldInstance.MajorActions
{
    class CharacterInventoryActions : MajorBagActions
    {
        public CharacterInventoryActions(InstanceFactory instances, Func<IModifLogger> getLogger) : base(instances, getLogger) { }

        public override void SubscribeToEvents()
        {
            CharacterInventoryEvents.InventoryIngredientsAfter +=
                (ci, character, tag, sortedIngredients) => InventoryIngredientsAfter(ci, character, tag, ref sortedIngredients);
        }

        private void InventoryIngredientsAfter(CharacterInventory characterInventory, Character character, Tag craftingStationTag, ref DictionaryExt<int, CompatibleIngredient> sortedIngredients)
        {
            try
            {
                if (!_instances.StashPacksSettings.CraftingFromStashPackItems.Value)
                {
                    Logger.LogWarning($"{nameof(CharacterInventoryActions)}::{nameof(InventoryIngredientsAfter)}: CraftingFromStashPackItems is not enabled. StashBag items will not be used by Character '{character.UID}' for crafting.");
                    return;
                }
                if (!_instances.TryGetStashPackWorldData(out var stashPackData))
                {
                    Logger.LogWarning($"{nameof(CharacterInventoryActions)}::{nameof(InventoryIngredientsAfter)}: Could not get stashpack data for Character '{character.UID}'.");
                    return;
                }


                var packs = stashPackData.GetStashPacks(character.UID);
                var bagsItems = packs.SelectMany(p => p.StashBag.Container.GetContainedItems()).ToList();
                if (!bagsItems.Any())
                {
                    Logger.LogDebug($"{nameof(CharacterInventoryActions)}::{nameof(InventoryIngredientsAfter)}: Character '{character.UID}' has no stashbags on the ground, or all stashbags are empty.");
                    return;
                }

                Logger.LogTrace($"{nameof(CharacterInventoryActions)}::{nameof(InventoryIngredientsAfter)}: Got {bagsItems.Count} items in {packs.Count()} stashbags for Character '{character.UID}' to craft with.");
                var pInventoryIngredients = characterInventory.GetType().GetMethod("InventoryIngredients", BindingFlags.NonPublic | BindingFlags.Instance);
                pInventoryIngredients.Invoke(characterInventory, new object[] { craftingStationTag, sortedIngredients, bagsItems });

            }
            catch (Exception ex)
            {
                Logger.LogException($"{nameof(CharacterInventoryActions)}::{nameof(InventoryIngredientsAfter)}: Error getting character '{character?.UID}' stashbags items for crafting.", ex);
            }
        }
    }
}
