using HarmonyLib;
using ModifAmorphic.Outward.Logging;
using System;

namespace ModifAmorphic.Outward.ActionUI.Patches
{
    [HarmonyPatch(typeof(SkillMenu))]
    internal static class SkillMenuPatches
    {
        private static IModifLogger Logger => LoggerFactory.GetLogger(ModInfo.ModId);


//        public static event Action<SkillMenu> AfterShow;

//        [HarmonyPatch(nameof(SkillMenu.Show))]
//        [HarmonyPostfix]
//        private static void ShowPostfix(SkillMenu __instance)
//        {
//            try
//            {
//                if (__instance.LocalCharacter == null || __instance.LocalCharacter.OwnerPlayerSys == null || !__instance.LocalCharacter.IsLocalPlayer)
//                    return;
//#if DEBUG
//                Logger.LogTrace($"{nameof(SkillMenuPatches)}::{nameof(ShowPostfix)}(): Invoking {nameof(AfterShow)} for character {__instance.LocalCharacter?.UID}.");
//#endif
//                AfterShow?.Invoke(__instance);
//            }
//            catch (Exception ex)
//            {
//                Logger.LogException($"{nameof(SkillMenuPatches)}::{nameof(ShowPostfix)}(): Exception invoking {nameof(AfterShow)} for character {__instance?.LocalCharacter?.UID}.", ex);
//            }
//        }

//        public static event Action<SkillMenu> AfterOnHide;

//        [HarmonyPatch("OnHide")]
//        [HarmonyPostfix]
//        private static void OnHidePostfix(SkillMenu __instance)
//        {
//            try
//            {
//                if (__instance.LocalCharacter == null || __instance.LocalCharacter.OwnerPlayerSys == null || !__instance.LocalCharacter.IsLocalPlayer)
//                    return;
//#if DEBUG
//                Logger.LogTrace($"{nameof(SkillMenuPatches)}::{nameof(OnHidePostfix)}(): Invoking {nameof(AfterOnHide)} for character {__instance.LocalCharacter?.UID}.");
//#endif
//                AfterOnHide?.Invoke(__instance);
//            }
//            catch (Exception ex)
//            {
//                Logger.LogException($"{nameof(SkillMenuPatches)}::{nameof(OnHidePostfix)}(): Exception invoking {nameof(AfterOnHide)} for character {__instance?.LocalCharacter?.UID}.", ex);
//            }
//        }

        public static event Action<ItemListDisplay> AfterOnSectionSelected;

        [HarmonyPatch(nameof(SkillMenu.OnSectionSelected))]
        [HarmonyPatch(new Type[] { typeof(int) })]
        [HarmonyPostfix]
        private static void OnSectionSelectedPostfix(ItemListDisplay ___m_skillList, int _sectionID)
        {
            try
            {
                if (___m_skillList?.LocalCharacter == null || ___m_skillList.LocalCharacter.OwnerPlayerSys == null || !___m_skillList.LocalCharacter.IsLocalPlayer)
                    return;
#if DEBUG
                Logger.LogTrace($"{nameof(SkillMenuPatches)}::{nameof(OnSectionSelectedPostfix)}(): Invoked. Invoking {nameof(AfterOnSectionSelected)}.");
#endif
                AfterOnSectionSelected?.Invoke(___m_skillList);
            }
            catch (Exception ex)
            {
                Logger.LogException($"{nameof(SkillMenuPatches)}::{nameof(OnSectionSelectedPostfix)}(): Exception invoking {nameof(AfterOnSectionSelected)}.", ex);
            }
        }
    }
}
