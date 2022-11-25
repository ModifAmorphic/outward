using HarmonyLib;
using ModifAmorphic.Outward.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ModifAmorphic.Outward.NewCaldera.AiMod.Patches
{
    [HarmonyPatch(typeof(LootableOnDeath))]
    internal static class LootableOnDeathPatches
    {
        private static IModifLogger Logger => LoggerFactory.GetLogger(ModInfo.ModId);

        public delegate void OnDeathDelegate(LootableOnDeath lootableOnDeath, bool loadedDead);
        public static event OnDeathDelegate AfterOnDeath;

        [HarmonyPatch("OnDeath", MethodType.Normal)]
        [HarmonyPatch(new Type[] { typeof(bool) })]
        [HarmonyPostfix]
        private static void OnDeathPostfix(LootableOnDeath __instance, bool _loadedDead)
        {
            try
            {
#if DEBUG
                Logger.LogTrace($"{nameof(LootableOnDeathPatches)}::{nameof(OnDeathPostfix)}() LootableOnDeath: {__instance.name}.");
#endif
                AfterOnDeath?.Invoke(__instance, _loadedDead);
            }
            catch (Exception ex)
            {
                Logger.LogException($"{nameof(LootableOnDeathPatches)}::{nameof(OnDeathPostfix)}(): Exception.", ex);
            }
        }
    }
}
