using HarmonyLib;
using ModifAmorphic.Outward.Logging;
using System;

namespace ModifAmorphic.Outward.ActionUI.Patches
{
    [HarmonyPatch(typeof(EquipmentMenu))]
    internal static class EquipmentMenuPatches
    {
        private static IModifLogger Logger => LoggerFactory.GetLogger(ModInfo.ModId);

        public static event Action<EquipmentMenu> AfterShow;

        [HarmonyPatch(nameof(EquipmentMenu.Show))]
        [HarmonyPostfix]
        private static void ShowPostfix(EquipmentMenu __instance)
        {
            try
            {
                if (__instance.LocalCharacter == null || __instance.LocalCharacter.OwnerPlayerSys == null || !__instance.LocalCharacter.IsLocalPlayer)
                    return;
#if DEBUG
                Logger.LogTrace($"{nameof(EquipmentMenuPatches)}::{nameof(ShowPostfix)}(): Invoking {nameof(AfterShow)} for character {__instance.LocalCharacter?.UID}.");
#endif
                AfterShow?.Invoke(__instance);
            }
            catch (Exception ex)
            {
                Logger.LogException($"{nameof(EquipmentMenuPatches)}::{nameof(ShowPostfix)}(): Exception invoking {nameof(AfterShow)} for character {__instance?.LocalCharacter?.UID}.", ex);
            }
        }

        public static event Action<EquipmentMenu> AfterOnHide;

        [HarmonyPatch("OnHide")]
        [HarmonyPostfix]
        private static void OnHidePostfix(EquipmentMenu __instance)
        {
            try
            {
                if (__instance.LocalCharacter == null || __instance.LocalCharacter.OwnerPlayerSys == null || !__instance.LocalCharacter.IsLocalPlayer)
                    return;
#if DEBUG
                Logger.LogTrace($"{nameof(EquipmentMenuPatches)}::{nameof(OnHidePostfix)}(): Invoking {nameof(AfterOnHide)} for character {__instance.LocalCharacter?.UID}.");
#endif
                AfterOnHide?.Invoke(__instance);
            }
            catch (Exception ex)
            {
                Logger.LogException($"{nameof(EquipmentMenuPatches)}::{nameof(OnHidePostfix)}(): Exception invoking {nameof(AfterOnHide)} for character {__instance?.LocalCharacter?.UID}.", ex);
            }
        }
    }
}
