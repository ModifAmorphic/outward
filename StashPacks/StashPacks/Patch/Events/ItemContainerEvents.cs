using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.StashPacks.Extensions;
using System;

namespace ModifAmorphic.Outward.StashPacks.Patch.Events
{
    internal static class ItemContainerEvents
    {
        private static IModifLogger Logger => LoggerFactory?.Invoke();
        public static Func<IModifLogger> LoggerFactory { get; set; }

        /// <summary>
        /// Triggers whenever a stack changes within a stashbag.
        /// </summary>
        //public static event Action<Bag> RemoveStackAmountAfter;
        public static event Action<Bag> RefreshWeightAfter;

        public static void RaiseRefreshWeightAfter(ItemContainer itemContainer)
        {
            try
            {
                if (itemContainer.RefBag != null && itemContainer.RefBag.IsStashBag())
                {
                    Logger?.LogTrace($"{nameof(ItemContainerEvents)}::{nameof(RaiseRefreshWeightAfter)} triggered for bag '{itemContainer.RefBag.Name}'.");
                    RefreshWeightAfter?.Invoke(itemContainer.RefBag);
                }

            }
            catch (Exception ex)
            {
                Logger?.LogException($"Exception in {nameof(ItemContainerEvents)}::{nameof(RaiseRefreshWeightAfter)}.", ex);
                if (Logger == null)
                {
                    UnityEngine.Debug.LogError($"Exception in {nameof(ItemContainerEvents)}::{nameof(RaiseRefreshWeightAfter)}:\n{ex}");
                }
            }
        }

    }
}
