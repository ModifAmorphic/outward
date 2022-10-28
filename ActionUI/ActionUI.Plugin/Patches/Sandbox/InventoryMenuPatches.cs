using HarmonyLib;
using ModifAmorphic.Outward.Logging;
using System;

namespace ModifAmorphic.Outward.ActionUI.Patches.Sandbox
{
    [HarmonyPatch(typeof(EquipmentMenu))]
    internal static class InventoryMenuPatches
    {
        private static IModifLogger Logger => LoggerFactory.GetLogger(ModInfo.ModId);

        public static event Action<InventoryMenu> AfterShow;

        [HarmonyPatch(nameof(InventoryMenu.Show))]
        [HarmonyPostfix]
        private static void ShowPostfix(InventoryMenu __instance)
        {
            try
            {
                if (__instance.LocalCharacter == null || __instance.LocalCharacter.OwnerPlayerSys == null || !__instance.LocalCharacter.IsLocalPlayer)
                    return;
#if DEBUG
                Logger.LogTrace($"{nameof(InventoryMenuPatches)}::{nameof(ShowPostfix)}(): Invoking {nameof(AfterShow)} for character {__instance.LocalCharacter?.UID}.");
#endif
                AfterShow?.Invoke(__instance);
            }
            catch (Exception ex)
            {
                Logger.LogException($"{nameof(InventoryMenuPatches)}::{nameof(ShowPostfix)}(): Exception invoking {nameof(AfterShow)} for character {__instance?.LocalCharacter?.UID}.", ex);
            }
        }

        public static event Action<InventoryMenu> AfterOnHide;

        [HarmonyPatch("OnHide")]
        [HarmonyPostfix]
        private static void OnHidePostfix(InventoryMenu __instance)
        {
            try
            {
                if (__instance.LocalCharacter == null || __instance.LocalCharacter.OwnerPlayerSys == null || !__instance.LocalCharacter.IsLocalPlayer)
                    return;
#if DEBUG
                Logger.LogTrace($"{nameof(InventoryMenuPatches)}::{nameof(OnHidePostfix)}(): Invoking {nameof(AfterOnHide)} for character {__instance.LocalCharacter?.UID}.");
#endif
                AfterOnHide?.Invoke(__instance);
            }
            catch (Exception ex)
            {
                Logger.LogException($"{nameof(InventoryMenuPatches)}::{nameof(OnHidePostfix)}(): Exception invoking {nameof(AfterOnHide)} for character {__instance?.LocalCharacter?.UID}.", ex);
            }
        }
    }
}
