using HarmonyLib;
using ModifAmorphic.Outward.Events;
using ModifAmorphic.Outward.Logging;
using System;

namespace ModifAmorphic.Outward.Modules.Character
{
    [HarmonyPatch(typeof(CharacterManager))]
    internal static class CharacterManagerPatches
    {
        private static Func<IModifLogger> _getLogger;
        private static IModifLogger Logger => _getLogger?.Invoke() ?? new NullLogger();
        public static event Action<CharacterManager> AwakeAfter;


        [EventSubscription]
        public static void SubscribeToEvents()
        {
            LoggerEvents.LoggerReady += (object sender, Func<IModifLogger> getLogger) => _getLogger = getLogger;
        }

        [HarmonyPatch("Awake", MethodType.Normal)]
        [HarmonyPostfix]
        private static void AwakePostfix(CharacterManager __instance)
        {
            AwakeAfter?.Invoke(__instance);
        }
    }
}
