using ModifAmorphic.Outward.Logging;
using System;

namespace ModifAmorphic.Outward.StashPacks.Patch.Events
{
    internal static class ItemManagerEvents
    {
        private static IModifLogger Logger => LoggerFactory?.Invoke();
        public static Func<IModifLogger> LoggerFactory { get; set; }

        public static event Func<ItemManager, bool, bool> IsAllItemSyncedAfter;
        public static event Action<ItemManager> AwakeAfter;

        public static void RaiseIsAllItemSyncedAfter(ItemManager itemManager, ref bool result)
        {
            var baseResult = result;
            try
            {
                result = IsAllItemSyncedAfter?.Invoke(itemManager, baseResult)?? baseResult;
                if (baseResult != result)
                    Logger?.LogTrace($"{nameof(ItemManagerEvents)}::{nameof(RaiseIsAllItemSyncedAfter)} triggered. Base result was changed from {baseResult} to {result}.");
            }
            catch (Exception ex)
            {
                result = baseResult;
                Logger?.LogException($"Exception in {nameof(ItemManagerEvents)}::{nameof(RaiseIsAllItemSyncedAfter)}.", ex);
                if (Logger == null)
                    UnityEngine.Debug.LogError($"Exception in {nameof(ItemManagerEvents)}::{nameof(RaiseIsAllItemSyncedAfter)}:\n{ex}");
            }
        }

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
