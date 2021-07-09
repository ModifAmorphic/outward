using ModifAmorphic.Outward.Logging;
using System;

namespace ModifAmorphic.Outward.Events
{
    public static class LoggerEvents
    {
        public static event EventHandler<Logger> LoggerLoaded;
        public static void RaiseLoggerConfigured(object sender, Logger logger)
        {
            LoggerLoaded?.Invoke(sender, logger);
        }
    }
}
