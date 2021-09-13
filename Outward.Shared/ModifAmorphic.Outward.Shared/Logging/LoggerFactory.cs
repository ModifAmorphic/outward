using ModifAmorphic.Outward.Events;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace ModifAmorphic.Outward.Logging
{
    public static class LoggerFactory
    {
        //readonly static ConcurrentDictionary<string, Func<IModifLogger>> _loggerFactories = new ConcurrentDictionary<string, Func<IModifLogger>>();
        readonly static ConcurrentDictionary<string, IModifLogger> _loggers = new ConcurrentDictionary<string, IModifLogger>();
        readonly static IModifLogger _nullLogger = new NullLogger();

        public static IModifLogger ConfigureLogger(string modId, string loggerName, LogLevel logLevel)
        {
#if DEBUG
            (new Logger(LogLevel.Debug, DebugLoggerInfo.ModName)).LogDebug(
                $"{nameof(LoggerFactory)}::{nameof(ConfigureLogger)}: Configuring named logger " +
                $" '{loggerName}'. There are {_loggers.Count} {nameof(IModifLogger)} already configured.");
#endif
            return _loggers.AddOrUpdate(modId, 
                CreateLogger(modId, loggerName, logLevel),
                (k, v) => UpdateLogLevel(modId, (Logger)v, logLevel));
        }
        public static IModifLogger GetLogger(string modId)
        {
            return _loggers.TryGetValue(modId, out var logger) ? logger : new Logger(LogLevel.Warning, modId);
        }
        private static Logger CreateLogger(string modId, string loggerName, LogLevel logLevel)
        {
            var logger = new Logger(logLevel, loggerName);
            LoggerEvents.RaiseLoggerCreated(modId, logger);
            return logger;
        }
        private static Logger UpdateLogLevel(string modId, Logger logger, LogLevel logLevel)
        {
            if (logger.LogLevel != logLevel)
            {
                logger.LogLevel = logLevel;
                LoggerEvents.RaiseLoggerConfigured(modId, logger);
            }
            return logger;
        }
    }
}
