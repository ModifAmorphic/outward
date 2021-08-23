using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.StashPacks.Extensions;
using ModifAmorphic.Outward.StashPacks.Settings;
using System;

namespace ModifAmorphic.Outward.StashPacks.Patch.Events
{
    internal static class ItemEvents
    {
        private static IModifLogger Logger => LoggerFactory?.Invoke();
        public static Func<IModifLogger> LoggerFactory { get; set; }

        /// <summary>
        /// Triggers whenever a special Stash <see cref="Bag"/> is about to be equipped. The <see cref="Bag"/> is a reference to the bag.
        /// </summary>
        public static event Func<Bag, string, string> DisplayNameAfter;
        public static void SetDisplayNameAfter(Item item, string displayNameBefore, out string displayNameAfter)
        {
            try
            {
                var bag = item as Bag;
                if (bag != null && bag.IsStashBag())
                {
                    displayNameAfter = DisplayNameAfter?.Invoke(bag, displayNameBefore) ?? displayNameBefore;
                    return;
                }

            }
            catch (Exception ex)
            {
                Logger?.LogException($"Exception in {nameof(ItemEvents)}::{nameof(SetDisplayNameAfter)}.", ex);
                if (Logger == null)
                    UnityEngine.Debug.LogError($"Exception in {nameof(ItemEvents)}::{nameof(SetDisplayNameAfter)}:\n{ex}");
            }

            displayNameAfter = displayNameBefore;
        }
    }
}
