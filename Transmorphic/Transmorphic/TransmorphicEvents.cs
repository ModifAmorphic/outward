using ModifAmorphic.Outward.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace ModifAmorphic.Outward.Transmorphic
{
    public static class TransmorphicEvents
    {

        private static IModifLogger Logger => LoggerFactory.GetLogger(ModInfo.ModId);

        public delegate void TransmogrifyDelegate(int consumedItemID, string consumedItemUID, int transmogItemID, string transmogItemUID);
        public static event TransmogrifyDelegate OnTransmogrified;

        internal static void RaiseOnTransmogrified(int consumedItemID, string consumedItemUID, int transmogItemID, string transmogItemUID)
        {
            try
            {
                Logger.LogDebug("Raising event {nameof(OnTransmogrified)}" +
                    $"(consumedItemID: {consumedItemID}, consumedItemUID: '{consumedItemUID}', transmogItemID: {transmogItemID}, transmogItemUID: '{transmogItemUID}')");
                OnTransmogrified?.Invoke(consumedItemID, consumedItemUID, transmogItemID, transmogItemUID);
            }
            catch (Exception ex)
            {
                Logger.LogException($"Exception raising event {nameof(OnTransmogrified)}" +
                    $"(consumedItemID: {consumedItemID}, consumedItemUID: '{consumedItemUID}', transmogItemID: {transmogItemID}, transmogItemUID: '{transmogItemUID}')", ex);
            }
        }
    }
}
