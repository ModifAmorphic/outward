namespace ModifAmorphic.Outward.Logging
{
    static class InternalLoggerFactory
    {
        const string loggerNameFormat = "{0} - {1}";
        static readonly string modifInternalLoggerName = $"{nameof(ModifAmorphic)}.{nameof(Outward)}";
        public static Logging.Logger GetLogger(Logging.Logger sourceLogger)
        {
            return new Logger(sourceLogger.LogLevel, string.Format(loggerNameFormat, sourceLogger.LoggerName, modifInternalLoggerName));
        }
        public static Logging.Logger GetLogger(Logging.LogLevel loglevel, string loggerName)
        {
            return new Logger(loglevel, loggerName);
        }
        public static Logging.Logger GetLogger(Logging.LogLevel logLevel, string callingAssemblyName, string internalLoggerName)
        {
            return new Logger(logLevel, string.Format(loggerNameFormat, callingAssemblyName, internalLoggerName));
        }

    }
}
