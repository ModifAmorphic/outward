using System;

namespace ModifAmorphic.Outward.Logging
{
    public interface IModifLogger
    {
        string LoggerName { get; }
        LogLevel LogLevel { get; }

        void Log(LogLevel logLevel, string message);
        void LogDebug(string message);
        void LogError(string message);
        void LogException(Exception ex);
        void LogException(string message, Exception ex);
        void LogInfo(string message);
        void LogTrace(string message);
        void LogWarning(string message);
    }
}