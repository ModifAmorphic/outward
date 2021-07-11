using ModifAmorphic.Outward.Logging;
using System;

namespace ModifAmorphic.Outward.Events
{
    internal static class LoggerEvents
    {
        public static event EventHandler<Func<IModifLogger>> LoggerReady;
        public static void RaiseLoggerReady(object sender, Func<IModifLogger> loggerFactory)
        {
            LoggerReady?.Invoke(sender, loggerFactory);
        }
    }
}
