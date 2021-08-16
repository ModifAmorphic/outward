using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.StashPacks.Extensions;
using ModifAmorphic.Outward.StashPacks.Settings;
using System;

namespace ModifAmorphic.Outward.StashPacks.Patch.Events
{
    internal static class CharacterEvents
    {
        private static IModifLogger Logger => LoggerFactory?.Invoke();
        public static Func<IModifLogger> LoggerFactory { get; set; }

        /// <summary>
        /// Triggers whenever a special Stash <see cref="Bag"/> is about to be equipped. The <see cref="Bag"/> is a reference to the bag.
        /// </summary>
        public static event Func<Character, CharacterPrivates, bool> HandleBackpackBefore;
        public static bool InvokeHandleBackpackBefore(ref Character character, ref CharacterInventory m_inventory, float m_lastHandleBagTime
            , bool m_interactCoroutinePending, bool m_IsHoldingInteract, bool m_IsHoldingDragCorpse)
        {
            try
            {
                Logger?.LogTrace($"{nameof(CharacterEvents)}::{nameof(InvokeHandleBackpackBefore)}." +
                    $" CharacterInventory.EquippedBag.Name: {m_inventory.EquippedBag?.Name}," +
                    $" Character.Interactable.ItemToPreview: {character.Interactable?.ItemToPreview?.Name}");

                //var bag = Character as Bag;

                //only care about stash bags.
                var bag = character.Interactable?.ItemToPreview as Bag;
                if (bag != null && bag.IsStashBag())
                    return HandleBackpackBefore?.Invoke(character, new CharacterPrivates()
                    {
                        m_inventory = m_inventory,
                        m_lastHandleBagTime = m_lastHandleBagTime,
                        m_interactCoroutinePending = m_interactCoroutinePending,
                        m_IsHoldingInteract = m_IsHoldingInteract,
                        m_IsHoldingDragCorpse = m_IsHoldingDragCorpse
                    }) ??true;

            }
            catch (Exception ex)
            {
                Logger?.LogException($"Exception in {nameof(CharacterEvents)}::{nameof(InvokeHandleBackpackBefore)}.", ex);
                if (Logger == null)
                    UnityEngine.Debug.LogError($"Exception in {nameof(CharacterEvents)}::{nameof(InvokeHandleBackpackBefore)}:\n{ex}");
            }

            return true;
        }
    }
}
