using System;
using System.Collections.Concurrent;

namespace ModifAmorphic.Outward.Logging
{
    internal class MultiLogger : IModifLogger
    {

        public string LoggerName => "ModifAmorphic-MultiLogger";

        public LogLevel LogLevel => LogLevel.Warning;

        public ConcurrentDictionary<string, Func<IModifLogger>> _loggerFactories = new ConcurrentDictionary<string, Func<IModifLogger>>();

        public void AddOrUpdateLogger(string modId, Func<IModifLogger> loggerFactory)
        {
            _loggerFactories.AddOrUpdate(modId, loggerFactory, (k, v) => loggerFactory);
        }
        public void RemoveLogger(string loggerName)
        {
            _loggerFactories.TryRemove(loggerName, out _);
        }
        public void Log(LogLevel logLevel, string message)
        {
            foreach (var logger in _loggerFactories.Values)
                logger?.Invoke().Log(logLevel, message);
        }
        public void LogTrace(string message)
        {
            Log(LogLevel.Trace, message);
        }
        public void LogDebug(string message)
        {
            Log(LogLevel.Debug, message);
        }
        public void LogInfo(string message)
        {
            Log(LogLevel.Info, message);
        }
        public void LogWarning(string message)
        {
            Log(LogLevel.Warning, message);
        }
        public void LogError(string message)
        {
            Log(LogLevel.Error, message);
        }
        public void LogException(Exception ex)
        {
            Log(LogLevel.Error, ex.ToString());
        }
        public void LogException(string message, Exception ex)
        {
            foreach (var logger in _loggerFactories.Values)
                logger?.Invoke().LogException(message, ex);
        }
    }
}
