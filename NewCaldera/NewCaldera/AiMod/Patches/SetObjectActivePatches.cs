using HarmonyLib;
using ModifAmorphic.Outward.Logging;
using NodeCanvas.Framework;
using NodeCanvas.Tasks.Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ModifAmorphic.Outward.NewCaldera.AiMod.Patches
{
    [HarmonyPatch(typeof(SetObjectActive))]
    internal static class SetObjectActivePatches
    {
        private static IModifLogger Logger => LoggerFactory.GetLogger(ModInfo.ModId);

        public delegate bool OnExecuteDelegate(SetObjectActive setObjectActive, bool setActive);
        public static OnExecuteDelegate TryBeforeOnExecute;

        [HarmonyPatch("OnExecute", MethodType.Normal)]
        [HarmonyPatch(new Type[] { })]
        [HarmonyPrefix]
        private static bool OnExecutePrefix(SetObjectActive __instance)
        {
            try
            {
                var setActive = __instance.setTo != SetObjectActive.SetActiveMode.Toggle ? __instance.setTo == SetObjectActive.SetActiveMode.Activate : !__instance.agent.gameObject.activeSelf;
                //Logger.LogTrace($"{nameof(SetObjectActivePatches)}::{nameof(OnExecutePrefix)}(): SetObjectActive {__instance.name}, agent type {__instance.agent.GetType()} '{__instance.agent.name}'. Base method will call SetActive({setActive}).");
                //var aiSquad = __instance.agent.GetComponent<AISquad>();
                //if (aiSquad == null)
                //    return true;

#if DEBUG
                //Logger.LogTrace($"{nameof(SetObjectActivePatches)}::{nameof(OnExecutePrefix)}(): Invoking {nameof(TryBeforeOnExecute)} for object {__instance.name}, agent type {__instance.agent.GetType()} '{__instance.agent.name}'. Base method will call SetActive({setActive}).");
#endif
                if (TryBeforeOnExecute?.Invoke(__instance, setActive) ?? false)
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                Logger.LogException($"{nameof(SetObjectActivePatches)}::{nameof(OnExecutePrefix)}(): Exception invoking {nameof(TryBeforeOnExecute)}.", ex);
            }

            return true;
        }
    }
}
