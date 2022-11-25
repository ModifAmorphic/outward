using HarmonyLib;
using ModifAmorphic.Outward.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace ModifAmorphic.Outward.NewCaldera.Patches
{
    [HarmonyPatch(typeof(LobbySystem))]
    internal static class LobbySystemPatches
    {
        private static IModifLogger Logger => LoggerFactory.GetLogger(ModInfo.ModId);

        public static event Action<LobbySystem> BeforeClearPlayerSystems;

        [HarmonyPatch(nameof(LobbySystem.ClearPlayerSystems), MethodType.Normal)]
        [HarmonyPrefix]
        private static void ClearPlayerSystemsPrefix(LobbySystem __instance, bool _hostLost = false)
        {
            try
            {
                Logger.LogDebug($"{nameof(LobbySystemPatches)}::{nameof(ClearPlayerSystemsPrefix)}: Invoking {nameof(BeforeClearPlayerSystems)}.");
                BeforeClearPlayerSystems?.Invoke(__instance);

            }
            catch (Exception ex)
            {
                Logger.LogException($"{nameof(LobbySystemPatches)}::{nameof(ClearPlayerSystemsPrefix)}: Exception Invoking {nameof(BeforeClearPlayerSystems)}", ex);
            }
        }
    }
}
