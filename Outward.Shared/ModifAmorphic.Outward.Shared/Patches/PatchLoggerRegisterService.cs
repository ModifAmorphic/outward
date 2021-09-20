using ModifAmorphic.Outward.Logging;
using System;
using System.Linq;
using System.Reflection;

namespace ModifAmorphic.Outward.Events
{
    internal static class PatchLoggerRegisterService
    {
#if DEBUG
        private readonly static IModifLogger _logger = LoggerFactory.ConfigureLogger(DebugLoggerInfo.ModId, DebugLoggerInfo.ModName, DebugLoggerInfo.DebugLogLevel);
#endif
        private static readonly object lockRegistration = new object();
        public static void AddOrUpdatePatchLogger(Type classType, string modId, Func<IModifLogger> loggerFactory)
        {
            lock (lockRegistration)
            {
                if (!classType.IsClass)
                    throw new ArgumentException($"{nameof(classType)} must be a class.", nameof(classType));
#if DEBUG
                _logger.LogDebug($"{nameof(PatchLoggerRegisterService)}::{nameof(AddOrUpdatePatchLogger)}:" +
                    $" Adding or updating patch logger factory with loggerName: {modId} for type {classType.Name}.");
#endif

                var patchLoggerProps = classType.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)
                        .Where(p => p.GetCustomAttributes(typeof(PatchLoggerAttribute), false).Any());
                var patchLoggerFields = classType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)
                        .Where(m => m.GetCustomAttributes(typeof(PatchLoggerAttribute), false).Any());

                foreach (var p in patchLoggerProps)
                {
#if DEBUG
                    _logger.LogTrace($"{nameof(PatchLoggerRegisterService)}::{nameof(AddOrUpdatePatchLogger)}:" +
                        $" Property p.GetValue(null) as PatchLogger is {(p.GetValue(null) as PatchLogger == null ? "null" : "not null")}." +
                        $" propery name is {p?.Name}");
#endif
                    var patchLogger = p.GetValue(null) as PatchLogger;
                    if (patchLogger == null)
                    {
                        patchLogger = new PatchLogger();
                        p.SetValue(null, patchLogger);
                    }
                    patchLogger.AddOrUpdateLogger(modId, loggerFactory);
                    
                }
                foreach (var f in patchLoggerFields)
                {
#if DEBUG
                    _logger.LogTrace($"{nameof(PatchLoggerRegisterService)}::{nameof(AddOrUpdatePatchLogger)}:" +
                        $" Field f.GetValue(null) as PatchLogger is {(f.GetValue(null) as PatchLogger == null ? "null" : "not null")}." +
                        $" propery name is {f?.Name}");
#endif
                    var patchLogger = f.GetValue(null) as PatchLogger;
                    if (patchLogger == null)
                    {
                        patchLogger = new PatchLogger();
                        f.SetValue(null, patchLogger);
                    }
                    patchLogger.AddOrUpdateLogger(modId, loggerFactory);
                }
            }
        }
    }
}
