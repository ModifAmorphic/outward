using ModifAmorphic.Outward.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModifAmorphic.Outward.Events
{
    public static class LoggerEvents
    {
        public static event EventHandler<Logger> LoggerLoaded;
        public static void RaiseLoggerConfigured(object sender, Logger logger)
        {
            LoggerLoaded?.Invoke(sender, logger);
        }
    }
}
