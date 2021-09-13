using HarmonyLib;
using ModifAmorphic.Outward.Events;
using ModifAmorphic.Outward.Logging;
using System;

namespace ModifAmorphic.Outward.SkillSchools.Patches
{
    [HarmonyPatch(typeof(SkillTreeHolder))]
    internal static class SkillTreeHolderPatches
    {
        private static Func<IModifLogger> _getLogger;
        private static IModifLogger Logger => _getLogger?.Invoke() ?? new NullLogger();
        public static event Action<SkillTreeHolder> AwakeAfter;


        [EventSubscription]
        public static void SubscribeToEvents()
        {
            LoggerEvents.LoggerReady += (object sender, Func<IModifLogger> getLogger) => _getLogger = getLogger;
        }

        [HarmonyPatch("Awake", MethodType.Normal)]
        [HarmonyPostfix]
        private static void AwakePostfix(SkillTreeHolder __instance)
        {
            AwakeAfter?.Invoke(__instance);
        }
    }
}
