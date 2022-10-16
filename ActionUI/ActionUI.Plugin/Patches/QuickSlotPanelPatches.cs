using HarmonyLib;
using ModifAmorphic.Outward.Logging;
using System;

namespace ModifAmorphic.Outward.ActionUI.Patches
{
    [HarmonyPatch(typeof(QuickSlotPanel))]
    internal class QuickSlotPanelPatches
    {
        private static IModifLogger Logger => LoggerFactory.GetLogger(ModInfo.ModId);

        public static event Action<KeyboardQuickSlotPanel> StartInitAfter;
        [HarmonyPatch("StartInit")]
        [HarmonyPostfix]
        private static void StartInitPostfix(QuickSlotPanel __instance)
        {
            try
            {
                if (__instance?.LocalCharacter == null || __instance.LocalCharacter.OwnerPlayerSys == null || !__instance.LocalCharacter.IsLocalPlayer)
                    return;

                if (__instance is KeyboardQuickSlotPanel keyboard)
                {
#if DEBUG
                    Logger.LogTrace($"{nameof(QuickSlotPanelPatches)}::{nameof(StartInitPostfix)}(): Invoked. Invoking {nameof(StartInitAfter)}({nameof(KeyboardQuickSlotPanel)}).");
#endif
                    StartInitAfter?.Invoke(keyboard);
                }
            }
            catch (Exception ex)
            {
                Logger.LogException($"{nameof(QuickSlotPanelPatches)}::{nameof(StartInitPostfix)}(): Exception Invoking {nameof(StartInitAfter)}({nameof(KeyboardQuickSlotPanel)}).", ex);
            }
        }
    }
}
