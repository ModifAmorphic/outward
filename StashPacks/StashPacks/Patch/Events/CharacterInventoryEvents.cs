using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.StashPacks.Extensions;
using System;
using UnityEngine;

namespace ModifAmorphic.Outward.StashPacks.Patch.Events
{
    internal static class CharacterInventoryEvents
    {
        private static IModifLogger Logger => LoggerFactory?.Invoke();
        public static Func<IModifLogger> LoggerFactory { get; set; }

        /// <summary>
        /// Triggers before a Stash Bag has been dropped from a character's inventory.
        /// </summary>
        public static event Action<Character, Bag> DropBagItemBefore;
        /// <summary>
        /// Triggers after a Stash Bag has been dropped. Bag may not be registered into the <see cref="ItemManager.Instance"/>'s  <see cref="ItemManager.WorldItems"/> yet.
        /// </summary>
        public static event Action<Character, Bag> DropBagItemAfter;

        /// <summary>
        /// Gets the preferred container location for a bag. 
        /// <list type="number">
        ///   <item><see cref="Bag"/> - StashBag to determine Relevant Container for</item>
        ///   <item><see cref="ItemContainer"/> - Pouch ItemContainer</item>
        ///   <item><see cref="ItemContainer"/> - The ItemContainer the base method determined was the most relevant</item>
        ///   <item><see cref="ItemContainer"/> - The ItemContainer result</item>
        /// </list>
        /// 
        /// </summary>
        public static event Func<Bag, ItemContainer, ItemContainer, ItemContainer> GetMostRelevantContainerAfter;

        public static event Action<CharacterInventory, Character, Tag, DictionaryExt<int, CompatibleIngredient>> InventoryIngredientsAfter;

        public static void RaiseDropBagItemBefore(Character character, ref Item item, Transform newParent, bool playAnim)
        {
            try
            {
                if (item.IsStashBag())
                {
                    var bag = (Bag)item;
                    Logger?.LogTrace($"{nameof(CharacterInventoryEvents)}::{nameof(RaiseDropBagItemBefore)}:" +
                        $" {item.Name} dropping. newParent.name = '{newParent?.name}', newParent.position = {newParent?.position}, playAnim = {playAnim}.");
                    DropBagItemBefore?.Invoke(character, bag);
                }
            }
            catch (Exception ex)
            {
                Logger?.LogException($"Exception in {nameof(CharacterInventoryEvents)}::{nameof(RaiseDropBagItemBefore)}.", ex);
                if (Logger == null)
                    UnityEngine.Debug.LogError($"Exception in {nameof(CharacterInventoryEvents)}::{nameof(RaiseDropBagItemBefore)}:\n{ex}");
            }
        }
        public static void RaiseDropBagItemAfter(Character character, Item item)
        {
            try
            {
                if (item.IsStashBag())
                    DropBagItemAfter?.Invoke(character, (Bag)item);

            }
            catch (Exception ex)
            {
                Logger?.LogException($"Exception in {nameof(CharacterInventoryEvents)}::{nameof(RaiseDropBagItemAfter)}.", ex);
                if (Logger == null)
                    UnityEngine.Debug.LogError($"Exception in {nameof(CharacterInventoryEvents)}::{nameof(RaiseDropBagItemAfter)}:\n{ex}");
            }
        }
        public static void RaiseGetMostRelevantContainerAfter(Item item, ItemContainer inventoryPouch, ref ItemContainer result)
        {
            try
            {
                if (item.IsStashBag())
                    result = GetMostRelevantContainerAfter?.Invoke((Bag)item, inventoryPouch, result) ?? result;
            }
            catch (Exception ex)
            {
                Logger?.LogException($"Exception in {nameof(CharacterInventoryEvents)}::{nameof(RaiseInventoryIngredientsAfter)}.", ex);
                if (Logger == null)
                    UnityEngine.Debug.LogError($"Exception in {nameof(CharacterInventoryEvents)}::{nameof(RaiseInventoryIngredientsAfter)}:\n{ex}");
            }
        }
        public static void RaiseInventoryIngredientsAfter(CharacterInventory characterInventory, Character character, Tag craftingStationTag, ref DictionaryExt<int, CompatibleIngredient> sortedIngredients)
        {
            try
            {
                InventoryIngredientsAfter?.Invoke(characterInventory, character, craftingStationTag, sortedIngredients);
            }
            catch (Exception ex)
            {
                Logger?.LogException($"Exception in {nameof(CharacterInventoryEvents)}::{nameof(RaiseInventoryIngredientsAfter)}.", ex);
                if (Logger == null)
                    UnityEngine.Debug.LogError($"Exception in {nameof(CharacterInventoryEvents)}::{nameof(RaiseInventoryIngredientsAfter)}:\n{ex}");
            }
        }
    }
}
