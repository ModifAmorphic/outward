using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModifAmorphic.Outward.Logging
{
    public class Logger
    {
        public LogLevel LogLevel { get; }
        public string LoggerName { get; }
        public Logger(LogLevel logLevel, string loggerName)
        {
            this.LogLevel = logLevel;
            this.LoggerName = loggerName;
        }
        public void Log(LogLevel logLevel, string message)
        {
            string logMsg =  $"[{this.LoggerName}][{Enum.GetName(typeof(LogLevel), logLevel)}] - {message}";
            if (logLevel <= this.LogLevel)
            {
                switch (logLevel)
                {
                    case Logging.LogLevel.Error:
                        UnityEngine.Debug.LogError(logMsg);
                        break;
                    case Logging.LogLevel.Warning:
                        UnityEngine.Debug.LogWarning(logMsg);
                        break;
                    default:
                        UnityEngine.Debug.Log(logMsg);
                        break;
                }
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
