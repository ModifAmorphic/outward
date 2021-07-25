using ModifAmorphic.Outward.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace ModifAmorphic.Outward.StashPacks.Patch.Events
{
    internal static class SaveInstanceEvents
    {
        private static IModifLogger Logger => LoggerFactory?.Invoke();
        public static Func<IModifLogger> LoggerFactory { get; set; }

        /// <summary>
        /// Triggers prior to a save being written to disk.
        /// </summary>
        public static event Action<SaveInstance> SaveBefore;

        public static void RaiseSaveBefore(SaveInstance saveInstance)
        {
            try
            {
                SaveBefore?.Invoke(saveInstance);
            }
            catch (Exception ex)
            {
                Logger?.LogException($"Exception in {nameof(SaveInstanceEvents)}::{nameof(RaiseSaveBefore)}.", ex);
                if (Logger == null)
                    UnityEngine.Debug.LogError($"Exception in {nameof(SaveInstanceEvents)}::{nameof(RaiseSaveBefore)}:\n{ex}");
            }
        }
    }
}
