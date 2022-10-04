using HarmonyLib;
using ModifAmorphic.Outward.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace ModifAmorphic.Outward.ActionUI.Patches
{
    [HarmonyPatch(typeof(LobbySystem))]
    internal static class LobbySystemPatches
    {
        private static IModifLogger Logger => LoggerFactory.GetLogger(ModInfo.ModId);

        //[HarmonyPatch(nameof(LobbySystem.PlayerSystemHasBeenDestroyed), MethodType.Normal)]
        //[HarmonyPostfix]
        //private static void PlayerSystemHasBeenDestroyedPostfix(LobbySystem __instance, string _uid)
        //{
        //    try
        //    {
        //        var playerSystem = __instance.GetPlayerSystem(_uid);
        //        Logger.LogDebug($"{nameof(LobbySystemPatches)}::{nameof(PlayerSystemHasBeenDestroyedPostfix)}: PlayerUID: {_uid}, PlayerID == {playerSystem?.PlayerID}, IsLocalPlayer == {playerSystem?.IsLocalPlayer}.");
        //    }
        //    catch (Exception ex)
        //    {
        //        Logger.LogException($"{nameof(LobbySystemPatches)}::{nameof(PlayerSystemHasBeenDestroyedPostfix)} Exception PlayerUID: {_uid}.", ex);
        //    }
        //}

        //[HarmonyPatch(nameof(LobbySystem.OnLeftRoom), MethodType.Normal)]
        //[HarmonyPostfix]
        //private static void OnLeftRoom()
        //{
        //    try
        //    {
        //        Logger.LogDebug($"{nameof(LobbySystemPatches)}::{nameof(OnLeftRoom)}:");
        //    }
        //    catch (Exception ex)
        //    {
        //        Logger.LogException($"{nameof(LobbySystemPatches)}::{nameof(OnLeftRoom)}", ex);
        //    }
        //}

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
