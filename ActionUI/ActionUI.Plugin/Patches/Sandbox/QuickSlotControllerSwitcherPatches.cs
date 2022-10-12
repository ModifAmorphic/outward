#if DEBUG
using HarmonyLib;
using ModifAmorphic.Outward.Logging;
using System;

namespace ModifAmorphic.Outward.ActionUI.Patches
{
    [HarmonyPatch(typeof(QuickSlotControllerSwitcher))]
    internal class QuickSlotControllerSwitcherPatches
    {
        private static IModifLogger Logger => LoggerFactory.GetLogger(ModInfo.ModId);

        public delegate void StartInit(QuickSlotControllerSwitcher controllerSwitcher);
        public static event StartInit StartInitAfter;
        [HarmonyPatch("StartInit")]
        [HarmonyPostfix]
        private static void StartInitPostfix(QuickSlotControllerSwitcher __instance)
        {
            try
            {
                if (__instance?.LocalCharacter == null || __instance.LocalCharacter.OwnerPlayerSys == null || !__instance.LocalCharacter.IsLocalPlayer)
                    return;

                Logger.LogTrace($"{nameof(QuickSlotControllerSwitcherPatches)}::{nameof(StartInitPostfix)}(): Invoked. Invoking {nameof(StartInitAfter)}({nameof(KeyboardQuickSlotPanel)}).");
                StartInitAfter?.Invoke(__instance);
            }
            catch (Exception ex)
            {
                Logger.LogException($"{nameof(QuickSlotControllerSwitcherPatches)}::{nameof(StartInitPostfix)}(): Exception Invoking {nameof(StartInitAfter)}({nameof(KeyboardQuickSlotPanel)}).", ex);
            }
        }
    }
}
#endif