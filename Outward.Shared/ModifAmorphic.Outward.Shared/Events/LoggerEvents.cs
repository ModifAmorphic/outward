using ModifAmorphic.Outward.Logging;
using System;

namespace ModifAmorphic.Outward.Events
{
    internal static class LoggerEvents
    {
        public static event Action<(string ModId, IModifLogger Logger)> LoggerCreated;
        public static void RaiseLoggerCreated(string modId, IModifLogger logger)
        {
            LoggerCreated?.Invoke((modId, logger));
        }

        public static event Action<(string ModId, IModifLogger Logger)> LoggerConfigured;
        public static void RaiseLoggerConfigured(string modId, IModifLogger logger)
        {
            LoggerConfigured?.Invoke((modId, logger));
        }
    }
}
