using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.StashPacks.WorldInstance.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace ModifAmorphic.Outward.StashPacks.Patch.Events
{
    internal static class CharacterInventoryEvents
    {
        private static IModifLogger Logger => LoggerFactory?.Invoke();
        public static Func<IModifLogger> LoggerFactory { get; set; }
        /// <summary>
        /// Triggers after a Stash Bag has been dropped. Bag may not be registered into the <see cref="ItemManager.Instance"/>'s  <see cref="ItemManager.WorldItems"/> yet.
        /// </summary>
        public static event Action<Bag> DropBagItemAfter;
        public static void RaiseDropBagItemAfter(Item item)
        {
            try
            {
                if (item.IsStashBag())
                    DropBagItemAfter?.Invoke((Bag)item);
            }
            catch (Exception ex)
            {
                Logger?.LogException($"Exception in {nameof(CharacterInventoryEvents)}::{nameof(RaiseDropBagItemAfter)}.", ex);
                if (Logger == null)
                    UnityEngine.Debug.LogError($"Exception in {nameof(CharacterInventoryEvents)}::{nameof(RaiseDropBagItemAfter)}:\n{ex}");
            }
        }

    }
}
