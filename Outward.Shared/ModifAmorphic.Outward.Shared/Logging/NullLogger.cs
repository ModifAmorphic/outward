using System;

namespace ModifAmorphic.Outward.Logging
{
    internal static class NullLogger
    {
        public static void LogTrace(Logging.Logger logger, string message)
        {
            if (logger != null)
                logger.LogTrace(message);
        }
        public static void LogDebug(Logging.Logger logger, string message)
        {
            if (logger != null)
                logger.LogDebug(message);
        }
        public static void LogWarning(Logging.Logger logger, string message)
        {
            if (logger != null)
                logger.LogWarning(message);
        }
        public static void LogError(Logging.Logger logger, string message)
        {
            if (logger != null)
                logger.LogError(message);
        }
        public static void LogException(Logging.Logger logger, Exception ex)
        {
            if (logger != null)
                logger.LogException(ex);
        }
        public static void LogException(Logging.Logger logger, string message, Exception ex)
        {
            if (logger != null)
                logger.LogException(message, ex);
        }
    }
}
