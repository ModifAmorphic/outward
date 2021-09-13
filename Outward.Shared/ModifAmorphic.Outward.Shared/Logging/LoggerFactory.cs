using System;
using System.Collections.Generic;
using System.Text;

namespace ModifAmorphic.Outward.Logging
{
    public static class LoggerFactory
    {
        private static IModifLogger _logger;
        private static readonly IModifLogger _defaultLogger = new Logger(LogLevel.Warning, "ModifAmorphic-Outward");
        public static void ConfigureLogging(string loggerName, LogLevel logLevel)
        {
            _logger = new Logger(logLevel, loggerName);
        }
        public static IModifLogger GetLogger()
        {
            return _logger ?? _defaultLogger;
        }
    }
}
