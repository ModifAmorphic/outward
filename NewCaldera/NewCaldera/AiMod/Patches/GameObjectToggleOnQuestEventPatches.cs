using HarmonyLib;
using ModifAmorphic.Outward.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ModifAmorphic.Outward.NewCaldera.AiMod.Patches
{
    [HarmonyPatch(typeof(GameObjectToggleOnQuestEvent))]
    internal static class GameObjectToggleOnQuestEventPatches
    {
        private static IModifLogger Logger => LoggerFactory.GetLogger(ModInfo.ModId);

        public delegate void RefreshStateDelegate(GameObjectToggleOnQuestEvent goToggleQuestEvent, QuestEventReference listenedEvent, QuestEventData eventData);
        public static event RefreshStateDelegate AfterRefreshState;

        [HarmonyPatch("RefreshState", MethodType.Normal)]
        [HarmonyPatch(new Type[] { typeof(QuestEventData) })]
        [HarmonyPostfix]
        private static void RefreshStatePostfix(GameObjectToggleOnQuestEvent __instance, QuestEventReference ___m_listenedEvent, QuestEventData _eventData)
        {
            try
            {
#if DEBUG
                Logger.LogTrace($"{nameof(GameObjectToggleOnQuestEventPatches)}::{nameof(RefreshStatePostfix)}(): EventUID: {___m_listenedEvent?.EventUID}.");
#endif
                AfterRefreshState?.Invoke(__instance, ___m_listenedEvent, _eventData);
        }
            catch (Exception ex)
            {
                Logger.LogException($"{nameof(GameObjectToggleOnQuestEventPatches)}::{nameof(RefreshStatePostfix)}(): Exception invoking {nameof(AfterRefreshState)} for EventUID: {_eventData?.EventUID}.", ex);
            }
}
    }
}
