using ModifAmorphic.Outward.Logging;
using System;
using System.Linq;
using System.Reflection;

namespace ModifAmorphic.Outward.Events
{
    internal static class PatchLoggerRegisterService
    {
        private static readonly object lockRegistration = new object();
        public static void AddPatchLogger(Type classType, string modId, Func<IModifLogger> loggerFactory)
        {
            lock (lockRegistration)
            {
                if (!classType.IsClass)
                    throw new ArgumentException($"{nameof(classType)} must be a class.", nameof(classType));
#if DEBUG
                UnityEngine.Debug.Log($"[{DebugLoggerInfo.ModName}][Trace] - {nameof(PatchLoggerRegisterService)}::{nameof(AddPatchLogger)}:" +
                    $" Adding patch logger factory with loggerName: {modId} for type {classType.Name}.");
#endif

                var patchLoggerProps = classType.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)
                        .Where(p => p.GetCustomAttributes(typeof(PatchLoggerAttribute), false).Any());
                var patchLoggerFields = classType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)
                        .Where(m => m.GetCustomAttributes(typeof(PatchLoggerAttribute), false).Any());

                foreach (var p in patchLoggerProps)
                {
#if DEBUG
                    UnityEngine.Debug.Log($"[{DebugLoggerInfo.ModName}][Trace] - {nameof(PatchLoggerRegisterService)}::{nameof(AddPatchLogger)}:" +
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
                    UnityEngine.Debug.Log($"[{DebugLoggerInfo.ModName}][Trace] -{nameof(PatchLoggerRegisterService)}::{nameof(AddPatchLogger)}:" +
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
