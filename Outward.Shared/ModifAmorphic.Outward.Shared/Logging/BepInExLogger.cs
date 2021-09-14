using BepInEx.Logging;
using ModifAmorphic.Outward.Extensions;
using System;

namespace ModifAmorphic.Outward.Logging
{
    public class BepInExLogger : IModifLogger
    {
        public LogLevel LogLevel { get; set; }
        public string LoggerName { get; }

        private readonly ManualLogSource _bepInExLogger;
        public BepInExLogger(LogLevel logLevel, string loggerName)
        {
            this.LogLevel = logLevel;
            this.LoggerName = loggerName;
            _bepInExLogger = BepInEx.Logging.Logger.CreateLogSource(loggerName);
        }
        public void Log(LogLevel logLevel, string message)
        {
            //string logMsg = $"[{this.LoggerName}][{Enum.GetName(typeof(LogLevel), logLevel)}] - {message}";
            if (logLevel <= this.LogLevel)
            {
                _bepInExLogger.Log(logLevel.ToBepLogLevel(), message);
            }
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
            Log(LogLevel.Error, message + " | Exception: " + ex.ToString());
        }
    }
}
