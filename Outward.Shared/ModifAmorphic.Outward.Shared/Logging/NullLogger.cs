﻿using System;

namespace ModifAmorphic.Outward.Logging
{
    internal class NullLogger : IModifLogger
    {
        public string LoggerName => "NullLogger";

        public LogLevel LogLevel => default;

        public void Log(LogLevel logLevel, string message)
        {

        }

        public void LogDebug(string message)
        {

        }

        public void LogError(string message)
        {

        }

        public void LogException(Exception ex)
        {

        }

        public void LogException(string message, Exception ex)
        {

        }

        public void LogInfo(string message)
        {

        }

        public void LogTrace(string message)
        {

        }

        public void LogWarning(string message)
        {

        }
    }
}
