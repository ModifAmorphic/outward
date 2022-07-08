using ModifAmorphic.Outward.Logging;
using System;
using System.Linq;

namespace ModifAmorphic.Outward.Events
{
    public static class SideLoaderEx
    {
        [MultiLogger]
        private static IModifLogger Logger { get; set; } = new NullLogger();

        public static bool TryHookOnPacksLoaded(object subscriber, Action action)
        {
            try
            {
                var slAssembly = AppDomain.CurrentDomain.GetAssemblies()
                    .FirstOrDefault(a => a.GetName().Name.Equals("SideLoader", StringComparison.InvariantCultureIgnoreCase));
                if (slAssembly == default)
                    return false;
                Logger.LogDebug($"TryHookSideLoaderOnPacksLoaded: Got Assembly {slAssembly}");

                var slType = slAssembly.GetTypes()
                    .FirstOrDefault(t => t.FullName.Equals("SideLoader.SL", StringComparison.InvariantCultureIgnoreCase));
                if (slType == default)
                    return false;
                Logger.LogDebug($"TryHookSideLoaderOnPacksLoaded: Got Type {slType}");

                var onPacksLoaded = slType.GetEvent("OnPacksLoaded", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
                Logger.LogDebug($"TryHookSideLoaderOnPacksLoaded: Got Event {onPacksLoaded}");
                var eventDelegateType = onPacksLoaded.EventHandlerType;
                Logger.LogDebug($"TryHookSideLoaderOnPacksLoaded: Got EventHandlerType {eventDelegateType}");

                var slDelegate = Delegate.CreateDelegate(eventDelegateType, subscriber, action.Method);
                Logger.LogDebug($"TryHookSideLoaderOnPacksLoaded: Got Delegate {slDelegate}");
                onPacksLoaded
                    .GetAddMethod()
                    .Invoke(null, new object[] { slDelegate });

                return true;
            }
            catch (Exception ex)
            {
                Logger.LogException($"Exception " +
                        $"attempting to hook subscriber [{subscriber?.GetType()}] action [{action?.GetType()}] to SideLoader OnPacksLoaded event.", ex);
                if (Logger is NullLogger)
                    new BepInExLogger(LogLevel.Error, DefaultLoggerInfo.ModName).LogException($"Exception " +
                        $"attempting to hook subscriber [{subscriber?.GetType()}] action [{action?.GetType()}] to SideLoader OnPacksLoaded event.", ex);
            }
            return false;
        }
    }
}
