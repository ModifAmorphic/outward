using HarmonyLib;
using ModifAmorphic.Outward.StashPacks.Patch.Events;

namespace ModifAmorphic.Outward.StashPacks.Patch
{
    [HarmonyPatch(typeof(Character))]
    internal static class CharacterPatches
    {
        [HarmonyPatch(nameof(Character.HandleBackpack), MethodType.Normal)]
        [HarmonyPrefix]
        private static bool HandleBackpackPrefix(ref Character __instance, ref CharacterInventory ___m_inventory
            , float ___m_lastHandleBagTime
            , bool ___m_interactCoroutinePending
            , bool ___m_IsHoldingInteract
            , bool ___m_IsHoldingDragCorpse)
        {

            return CharacterEvents.InvokeHandleBackpackBefore(ref __instance, ref ___m_inventory
            , ___m_lastHandleBagTime
            , ___m_interactCoroutinePending
            , ___m_IsHoldingInteract
            , ___m_IsHoldingDragCorpse);
        }
    }
}
