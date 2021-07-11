using HarmonyLib;
using ModifAmorphic.Outward.Events;
using ModifAmorphic.Outward.Logging;
using System;
using System.Collections.Concurrent;

namespace ModifAmorphic.Outward.Modules
{
    public static class ModifModules
    {
        private static ModuleService _lazyService = null;
        private static ModuleService ModuleService
        {
            get
            {
                if (_lazyService == null) _lazyService = new ModuleService();
                return _lazyService;
            }
        }
        public static QuickSlotExtender GetQuickSlotExtenderModule(string modId)
        {
            return ModuleService.GetModule(modId, () => 
                new QuickSlotExtender(ModuleService.GetLoggerFactory(modId)));
        }

        public static void ConfigureLogging(string modId, Func<IModifLogger> loggerFactory)
        {
            ModuleService.ConfigureLogging(modId, loggerFactory);
        }
        public static void ConfigureLogging(string modId, string loggerName, LogLevel logLevel)
        {
            ModuleService.ConfigureLogging(modId, () => new Logger(logLevel, loggerName));
        }

        
    }
}
