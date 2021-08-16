using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.StashPacks.Extensions;
using System;

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

        public static event Action<CharacterInventory, Character, Tag, DictionaryExt<int, CompatibleIngredient>> InventoryIngredientsAfter;

        public static void RaiseDropBagItemBefore(Character character, ref Item item)
        {
            try
            {
                if (item.IsStashBag())
                    DropBagItemBefore?.Invoke(character, (Bag)item);
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
