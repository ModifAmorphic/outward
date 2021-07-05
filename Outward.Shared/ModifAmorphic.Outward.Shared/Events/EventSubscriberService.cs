using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ModifAmorphic.Outward.Events
{
    public class EventSubscriberService
    {
        public static void RegisterSubscriptions(Logging.Logger logger = null)
        {
            logger?.LogDebug($"RegisterSubscriptions - Searching for classes with [EventSubscription] methods.");
            var assemblyTypes = Assembly.GetCallingAssembly().GetTypes();

            var allClasses = assemblyTypes.Where(t => t.IsClass && (!t.IsAbstract || (t.IsSealed && t.IsAbstract)));
            foreach(var c in allClasses)
            {
                var subscriberMethods = c.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)
                    .Where(m => m.GetCustomAttributes(typeof(EventSubscriptionAttribute), false).Count() > 0);
                logger?.LogTrace($"RegisterSubscriptions - Found {subscriberMethods?.Count()} [EventSubscription] methods in class {c.Name}.");
                foreach (var m in subscriberMethods)
                {
                    logger?.LogDebug($"RegisterSubscriptions - {c.Name}: Invoking [EventSubscription]{m.Name}.");
                    m.Invoke(null, null);
                }
            }
        }
    }
}
