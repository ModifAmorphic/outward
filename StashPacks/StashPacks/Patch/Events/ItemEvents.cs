using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.StashPacks.Settings;
using System;
using System.Collections.Generic;
using System.Text;

namespace ModifAmorphic.Outward.StashPacks.Patch.Events
{
    internal static class ItemEvents
    {
        private static IModifLogger Logger => LoggerFactory?.Invoke();
        public static Func<IModifLogger> LoggerFactory { get; set; }

        /// <summary>
        /// Triggers whenever a special Stash <see cref="Bag"/> is about to be equipped. The <see cref="Bag"/> is a reference to the bag.
        /// </summary>
        public static event Action<Bag, EquipmentSlot> PerformEquipBefore;

        public static void RaisePerformEquipBefore(ref Item item, EquipmentSlot slot)
        {
            try
            {
                var bag = item as Bag;

                //only care about stash bags.
                if (bag != null && StashPacksConstants.StashBackpackItemIds.ContainsKey(bag.ItemID))
                    PerformEquipBefore?.Invoke(bag, slot);
            }
            catch (Exception ex)
            {
                Logger?.LogException($"Exception in {nameof(ItemEvents)}::{nameof(RaisePerformEquipBefore)}.", ex);
                if (Logger == null)
                    UnityEngine.Debug.LogError($"Exception in {nameof(ItemEvents)}::{nameof(RaisePerformEquipBefore)}:\n{ex}");
            }
        }
    }
}
