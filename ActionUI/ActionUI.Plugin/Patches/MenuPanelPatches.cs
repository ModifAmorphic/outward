using HarmonyLib;
using ModifAmorphic.Outward.Logging;
using System;

namespace ModifAmorphic.Outward.ActionUI.Patches
{
    [HarmonyPatch(typeof(MenuPanel))]
    internal static class MenuPanelPatches
    {
        private static IModifLogger Logger => LoggerFactory.GetLogger(ModInfo.ModId);


        public static event Action<InventoryMenu> AfterShowInventoryMenu;
        public static event Action<SkillMenu> AfterShowSkillMenu;

        [HarmonyPatch(nameof(MenuPanel.Show))]
        [HarmonyPatch(new Type[] { })]
        [HarmonyPostfix]
        private static void ShowPostfix(MenuPanel __instance)
        {
            try
            {
                if (__instance.LocalCharacter == null || __instance.LocalCharacter.OwnerPlayerSys == null || !__instance.LocalCharacter.IsLocalPlayer)
                    return;

                if (__instance is InventoryMenu inventoryMenu)
                {
                    Logger.LogDebug($"{nameof(MenuPanelPatches)}::{nameof(ShowPostfix)}(): Invoking {nameof(AfterShowInventoryMenu)} event for character {__instance?.LocalCharacter?.UID}.");
                    AfterShowInventoryMenu?.Invoke(inventoryMenu);
                }
                else if (__instance is SkillMenu skillMenu)
                {
                    Logger.LogDebug($"{nameof(MenuPanelPatches)}::{nameof(ShowPostfix)}(): Invoking {nameof(AfterShowSkillMenu)} event for character {__instance?.LocalCharacter?.UID}.");
                    AfterShowSkillMenu?.Invoke(skillMenu);
                }
            }
            catch (Exception ex)
            {
                Logger.LogException($"{nameof(MenuPanelPatches)}::{nameof(ShowPostfix)}(): Exception invoking Show event for character {__instance?.LocalCharacter?.UID}.", ex);
            }
        }

        public static event Action<InventoryMenu> AfterOnHideInventoryMenu;
        public static event Action<SkillMenu> AfterOnHideSkillMenu;
        public static event Action<MenuPanel> AfterOnHideMenuPanel;

        [HarmonyPatch("OnHide")]
        [HarmonyPatch(new Type[] { })]
        [HarmonyPostfix]
        private static void OnHidePostfix(MenuPanel __instance)
        {
            try
            {
                if (__instance.LocalCharacter == null || __instance.LocalCharacter.OwnerPlayerSys == null || !__instance.LocalCharacter.IsLocalPlayer)
                    return;

                Logger.LogDebug($"{nameof(MenuPanelPatches)}::{nameof(OnHidePostfix)}(): Invoking {nameof(AfterOnHideMenuPanel)} event for character {__instance?.LocalCharacter?.UID}.");

                AfterOnHideMenuPanel?.Invoke(__instance);
                if (__instance is InventoryMenu inventoryMenu)
                    AfterOnHideInventoryMenu?.Invoke(inventoryMenu);
                else if (__instance is SkillMenu skillMenu)
                    AfterOnHideSkillMenu?.Invoke(skillMenu);
            }
            catch (Exception ex)
            {
                Logger.LogException($"{nameof(MenuPanelPatches)}::{nameof(OnHidePostfix)}(): Exception invoking OnHide event for character {__instance?.LocalCharacter?.UID}.", ex);
            }
        }
    }
}
