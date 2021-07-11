using ModifAmorphic.Outward.Logging;
using System;
using System.Linq;
using System.Reflection;

namespace ModifAmorphic.Outward.Events
{
    public class EventSubscriberService
    {
        public static void RegisterSubscriptions(IModifLogger logger = null)
        {
            logger?.LogDebug($"RegisterSubscriptions - Searching for classes with [EventSubscription] methods.");
            var assemblyTypes = Assembly.GetCallingAssembly().GetTypes();

            var allClasses = assemblyTypes.Where(t => t.IsClass && (!t.IsAbstract || (t.IsSealed && t.IsAbstract)));
            foreach (var c in allClasses)
            {
                RegisterClassSubscriptions(c, logger);
            }
        }
        public static void RegisterClassSubscriptions(Type classType, IModifLogger logger = null)
        {
            if (!classType.IsClass)
                throw new ArgumentException($"{nameof(classType)} must be a class.", nameof(classType));

            var subscriberMethods = classType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)
                    .Where(m => m.GetCustomAttributes(typeof(EventSubscriptionAttribute), false).Count() > 0);
            logger?.LogTrace($"RegisterSubscriptions - Found {subscriberMethods?.Count()} [EventSubscription] methods in class {classType.Name}.");
            foreach (var m in subscriberMethods)
            {
                logger?.LogDebug($"RegisterSubscriptions - {classType.Name}: Invoking [EventSubscription]{m.Name}.");
                m.Invoke(null, null);
            }
        }
    }
}
