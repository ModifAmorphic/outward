using HarmonyLib;
using ModifAmorphic.Outward.Events;
using ModifAmorphic.Outward.Logging;
using System;

namespace ModifAmorphic.Outward.Modules.Character
{
    [HarmonyPatch(typeof(SkillTreeHolder))]
    internal static class SkillTreeHolderPatches
    {
        [PatchLogger]
        private static IModifLogger Logger { get; set; } = new NullLogger();
        public static event Action<SkillTreeHolder> AwakeAfter;


        //[EventSubscription]
        //public static void SubscribeToEvents()
        //{
        //    LoggerEvents.LoggerConfigured += (object sender, Func<IModifLogger> getLogger) => _getLogger = getLogger;
        //}

        [HarmonyPatch("Awake", MethodType.Normal)]
        [HarmonyPostfix]
#pragma warning disable IDE0051 // Remove unused private members
        private static void AwakePostfix(SkillTreeHolder __instance)
#pragma warning restore IDE0051 // Remove unused private members
        {
            AwakeAfter?.Invoke(__instance);
        }
    }
}
