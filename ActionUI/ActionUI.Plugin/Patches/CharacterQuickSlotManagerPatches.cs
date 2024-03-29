﻿using HarmonyLib;
using ModifAmorphic.Outward.Logging;
using System;
using System.Collections.Generic;

namespace ModifAmorphic.Outward.ActionUI.Patches
{
    [HarmonyPatch(typeof(CharacterQuickSlotManager))]
    internal static class CharacterQuickSlotManagerPatches
    {
        private static IModifLogger Logger => LoggerFactory.GetLogger(ModInfo.ModId);


        public delegate bool AllowItemDestroyedDelegate(int rewiredId);
        public static Dictionary<int, AllowItemDestroyedDelegate> AllowItemDestroyed = new Dictionary<int, AllowItemDestroyedDelegate>();

        [HarmonyPatch(nameof(CharacterQuickSlotManager.ItemDestroyed))]
        [HarmonyPatch(new Type[] { typeof(Item), typeof(int) })]
        [HarmonyPrefix]
        private static bool ItemDestroyedPrefix(Character ___m_character, QuickSlot[] ___m_quickSlots, Item _item, int _quickSlotID)
        {
            try
            {
                if (___m_character == null || ___m_character?.OwnerPlayerSys == null || !___m_character.IsLocalPlayer)
                    return true;
#if DEBUG
                Logger.LogTrace($"{nameof(CharacterQuickSlotManagerPatches)}::{nameof(ItemDestroyedPrefix)}(): Invoked for PlayerID {___m_character?.OwnerPlayerSys?.PlayerID}.");
#endif
                if (___m_character?.OwnerPlayerSys != null)
                {
                    if (___m_quickSlots.Length > _quickSlotID && ___m_quickSlots[_quickSlotID] != null)
                        return true;

                    var playerId = ___m_character.OwnerPlayerSys.PlayerID;
                    if (AllowItemDestroyed.TryGetValue(playerId, out var allowItemDestroyed))
                    {
                        var result = allowItemDestroyed(playerId);
#if DEBUG
                        Logger.LogTrace($"{nameof(CharacterQuickSlotManagerPatches)}::{nameof(ItemDestroyedPrefix)}(): PlayerID {playerId}: Allow item in quickslot {_quickSlotID} to be destroyed? {result}.");
#endif
                        return result;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogException($"{nameof(CharacterQuickSlotManagerPatches)}::{nameof(ItemDestroyedPrefix)}(): Exception calling {nameof(AllowItemDestroyed)} Delegate.", ex);
            }
            return true;
        }
    }
}
