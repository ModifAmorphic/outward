using ModifAmorphic.Outward.Logging;
using System;

namespace ModifAmorphic.Outward.StashPacks.Patch.Events
{
    internal static class AreaManagerEvents
    {
        private static IModifLogger Logger => LoggerFactory?.Invoke();
        public static Func<IModifLogger> LoggerFactory { get; set; }

        public static event Action<AreaManager> AwakeAfter;

        public static void RaiseAwakeAfter(ref AreaManager AreaManager)
        {
            try
            {
                AwakeAfter?.Invoke(AreaManager);
            }
            catch (Exception ex)
            {
                Logger?.LogException($"Exception in {nameof(AreaManagerEvents)}::{nameof(RaiseAwakeAfter)}.", ex);
                if (Logger == null)
                {
                    UnityEngine.Debug.LogError($"Exception in {nameof(AreaManagerEvents)}::{nameof(RaiseAwakeAfter)}:\n{ex}");
                }
            }
        }
    }
}
