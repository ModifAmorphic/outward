﻿using ModifAmorphic.Outward.Logging;
using System;
using System.Linq;

namespace ModifAmorphic.Outward.Events
{
    public static class TransmorphicEventsEx
    {
        [MultiLogger]
        private static IModifLogger Logger { get; set; } = new NullLogger();

        public delegate void TransmogrifyDelegate(int consumedItemID, string consumedItemUID, int transmogItemID, string transmogItemUID);

        public static bool TryHookOnTransmogrified(object subscriber, TransmogrifyDelegate transmogrifyDelegate)
        {
            try
            {
                var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();
                var tmorphAssembly = loadedAssemblies
                    .FirstOrDefault(a => a.GetName().Name.Equals("ModifAmorphic.Outward.Transmorphic", StringComparison.InvariantCultureIgnoreCase));
                if (tmorphAssembly == default)
                {
                    var partialMatch = loadedAssemblies.FirstOrDefault(a => a.GetName().Name.Contains("Transmorphic", StringComparison.InvariantCultureIgnoreCase));
                    Logger.LogDebug($"{nameof(TryHookOnTransmogrified)}: Assembly ModifAmorphic.Outward.Transmorphic not found. Closest match found was {partialMatch}.");
                    return false;
                }
                Logger.LogDebug($"{nameof(TryHookOnTransmogrified)}: Got Assembly {tmorphAssembly}");

                var tmorphType = tmorphAssembly.GetTypes()
                    .FirstOrDefault(t => t.FullName.Equals("ModifAmorphic.Outward.Transmorphic.TransmorphicEvents", StringComparison.InvariantCultureIgnoreCase));
                if (tmorphType == default)
                    return false;
                Logger.LogDebug($"{nameof(TryHookOnTransmogrified)}: Got Type {tmorphType}");

                var onTransmogrified = tmorphType.GetEvent("OnTransmogrified", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
                Logger.LogDebug($"{nameof(TryHookOnTransmogrified)}: Got Event {onTransmogrified}");
                var eventDelegateType = onTransmogrified.EventHandlerType;
                Logger.LogDebug($"{nameof(TryHookOnTransmogrified)}: Got EventHandlerType {eventDelegateType}");

                var tmorphDelegate = Delegate.CreateDelegate(eventDelegateType, subscriber, transmogrifyDelegate.Method);
                Logger.LogDebug($"{nameof(TryHookOnTransmogrified)}: Got Delegate {tmorphDelegate}");
                onTransmogrified
                    .GetAddMethod()
                    .Invoke(null, new object[] { tmorphDelegate });
                Logger.LogDebug($"{nameof(TryHookOnTransmogrified)}: Subscribed {subscriber.GetType()} to OnTransmogrified event.");

                return true;
            }
            catch (Exception ex)
            {
                Logger.LogException($"Exception " +
                        $"attempting to hook subscriber [{subscriber?.GetType()}] action [{transmogrifyDelegate?.GetType()}] to Transmorphic OnTransmogrified event.", ex);
                if (Logger is NullLogger)
                    new BepInExLogger(LogLevel.Error, DefaultLoggerInfo.ModName).LogException($"Exception " +
                        $"attempting to hook subscriber [{subscriber?.GetType()}] action [{transmogrifyDelegate?.GetType()}] to Transmorphic OnTransmogrified event.", ex);
            }
            return false;
        }
    }
}