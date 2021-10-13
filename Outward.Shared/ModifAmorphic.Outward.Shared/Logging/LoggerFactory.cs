using ModifAmorphic.Outward.Events;
using ModifAmorphic.Outward.Extensions;
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
            (new Logger(LogLevel.Debug, DefaultLoggerInfo.ModName)).LogDebug(
                $"{nameof(LoggerFactory)}::{nameof(ConfigureLogger)}: Configuring named logger " +
                $" '{loggerName}' with modId '{modId}'. There are {_loggers.Count} {nameof(IModifLogger)} already configured. " +
                $"A logger for '{modId}' {(_loggers.ContainsKey(modId) ? "exists and will be configured" : "will be created")}" +
                $" with a log level of '{logLevel.GetName()}'.");
#endif
            return _loggers.AddOrUpdate(modId, 
                CreateLogger(modId, loggerName, logLevel),
                (k, v) => UpdateLogLevel(modId, v, logLevel));
        }
        public static IModifLogger GetLogger(string modId)
        {
            return _loggers.TryGetValue(modId, out var logger) ? logger : new Logger(LogLevel.Warning, modId);
        }
        private static IModifLogger CreateLogger(string modId, string loggerName, LogLevel logLevel)
        {
            var logger = new BepInExLogger(logLevel, loggerName);
            LoggerEvents.RaiseLoggerCreated(modId, logger);
            return logger;
        }
        private static IModifLogger UpdateLogLevel(string modId, IModifLogger logger, LogLevel logLevel)
        {
            if (logger.LogLevel != logLevel)
            {
                if (logger is BepInExLogger)
                    ((BepInExLogger)logger).LogLevel = logLevel;
                else if (logger is Logger)
                    ((Logger)logger).LogLevel = logLevel;
                else
                    throw new ArgumentException($"'{nameof(logger)}' Argument's base type was unexpected. Supported base types are '{typeof(Logger).FullName}' or '{typeof(BepInExLogger).FullName}'."
                        , nameof(logger));
                LoggerEvents.RaiseLoggerConfigured(modId, logger);
            }
            return logger;
        }
    }
}
