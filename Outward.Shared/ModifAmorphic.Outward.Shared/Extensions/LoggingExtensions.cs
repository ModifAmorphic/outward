using System;
using System.Collections.Generic;
using System.Text;

namespace ModifAmorphic.Outward.Extensions
{
    public static class LoggingExtensions
    {
        public static BepInEx.Logging.LogLevel ToBepLogLevel(this Logging.LogLevel logLevel)
        {
            switch (logLevel)
            {
                case Logging.LogLevel.Error:
                    return BepInEx.Logging.LogLevel.Error;
                case Logging.LogLevel.Warning:
                    return BepInEx.Logging.LogLevel.Warning;
                case Logging.LogLevel.Info:
                    return BepInEx.Logging.LogLevel.Info;
                case Logging.LogLevel.Debug:
                    return BepInEx.Logging.LogLevel.Debug;
                case Logging.LogLevel.Trace:
                    return BepInEx.Logging.LogLevel.Debug;
                default:
                    return BepInEx.Logging.LogLevel.Info;
            }
            
        }
    }
}
