using ModifAmorphic.Outward.Logging;
using System;

namespace ModifAmorphic.Outward.StashPacks.Patch.Events
{
    internal static class EnvironmentSaveEvents
    {
        private static IModifLogger Logger => LoggerFactory?.Invoke();
        public static Func<IModifLogger> LoggerFactory { get; set; }

        public static event Action<EnvironmentSave> ApplyDataBefore;
        public static event Action<EnvironmentSave> ApplyDataAfter;

        public static void RaiseApplyDataBefore(EnvironmentSave environmentSave)
        {
            try
            {
                Logger?.LogTrace($"{nameof(EnvironmentSaveEvents)}::{nameof(RaiseApplyDataBefore)} raised. EnvironmentSave has {environmentSave?.ItemList?.Count ?? 0} items in it's ItemList.");
                ApplyDataBefore?.Invoke(environmentSave);
            }
            catch (Exception ex)
            {
                Logger?.LogException($"Exception in {nameof(EnvironmentSaveEvents)}::{nameof(RaiseApplyDataBefore)}.", ex);
                if (Logger == null)
                    UnityEngine.Debug.LogError($"Exception in {nameof(EnvironmentSaveEvents)}::{nameof(RaiseApplyDataBefore)}:\n{ex}");
            }
        }
        public static void RaiseApplyDataAfter(EnvironmentSave environmentSave)
        {
            try
            {
                ApplyDataAfter?.Invoke(environmentSave);
            }
            catch (Exception ex)
            {
                Logger?.LogException($"Exception in {nameof(EnvironmentSaveEvents)}::{nameof(RaiseApplyDataAfter)}.", ex);
                if (Logger == null)
                    UnityEngine.Debug.LogError($"Exception in {nameof(EnvironmentSaveEvents)}::{nameof(RaiseApplyDataAfter)}:\n{ex}");
            }
        }
    }
}
