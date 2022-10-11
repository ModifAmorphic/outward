﻿using HarmonyLib;
using ModifAmorphic.Outward.Logging;
using System;

namespace ModifAmorphic.Outward.ActionUI.Patches
{
    [HarmonyPatch(typeof(SkillMenu))]
    internal static class SkillMenuPatches
    {
        private static IModifLogger Logger => LoggerFactory.GetLogger(ModInfo.ModId);

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

                Logger.LogTrace($"{nameof(SkillMenuPatches)}::{nameof(OnSectionSelectedPostfix)}(): Invoked. Invoking {nameof(AfterOnSectionSelected)}.");
                AfterOnSectionSelected?.Invoke(___m_skillList);
            }
            catch (Exception ex)
            {
                Logger.LogException($"{nameof(SkillMenuPatches)}::{nameof(OnSectionSelectedPostfix)}(): Exception invoking {nameof(AfterOnSectionSelected)}.", ex);
            }
        }
    }
}
