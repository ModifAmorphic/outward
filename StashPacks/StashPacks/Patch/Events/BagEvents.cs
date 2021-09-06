using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.StashPacks.Extensions;
using System;

namespace ModifAmorphic.Outward.StashPacks.Patch.Events
{
    internal static class BagEvents
    {
        private static IModifLogger Logger => LoggerFactory?.Invoke();
        public static Func<IModifLogger> LoggerFactory { get; set; }

        public static event Func<Character, Bag, bool> ShowContentBefore;

        public static bool HandleShowContentBefore(Character character, ref Bag bag)
        {
            try
            {
                if (!bag.IsStashBag())
                {
                    return true;
                }

                return ShowContentBefore?.Invoke(character, bag) ?? true;
            }
            catch (Exception ex)
            {
                Logger?.LogException($"Exception in {nameof(BagEvents)}::{nameof(HandleShowContentBefore)}.", ex);
                if (Logger == null)
                {
                    UnityEngine.Debug.LogError($"Exception in {nameof(BagEvents)}::{nameof(HandleShowContentBefore)}:\n{ex}");
                }
            }

            return true;
        }
    }
}
