using ModifAmorphic.Outward.Logging;

namespace ModifAmorphic.Outward.StashPacks.Settings
{
    internal static class LoggerFactory
    {
        private static IModifLogger _logger;
        private static readonly IModifLogger _defaultLogger = new Logger(LogLevel.Warning, ModInfo.ModName);
        public static IModifLogger GetLogger()
        {
            return _logger ?? _defaultLogger;
        }
        public static void SetLogLevel(LogLevel logLevel)
        {
            _logger = new Logger(logLevel, ModInfo.ModName);
        }
    }
}
