using ModifAmorphic.Outward.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace ModifAmorphic.Outward.StashPacks.Patch.Events
{
    internal static class ItemManagerEvents
    {
        private static IModifLogger Logger => LoggerFactory?.Invoke();
        public static Func<IModifLogger> LoggerFactory { get; set; }

        public static event Action<ItemManager> AwakeAfter;

        public static void RaiseAwakeAfter(ref ItemManager itemManager)
        {
            try
            {
                AwakeAfter?.Invoke(itemManager);
            }
            catch (Exception ex)
            {
                Logger?.LogException($"Exception in {nameof(ItemManagerEvents)}::{nameof(RaiseAwakeAfter)}.", ex);
                if (Logger == null)
                    UnityEngine.Debug.LogError($"Exception in {nameof(ItemManagerEvents)}::{nameof(RaiseAwakeAfter)}:\n{ex}");
            }
        }
    }
}
