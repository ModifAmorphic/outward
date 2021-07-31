using HarmonyLib;
using ModifAmorphic.Outward.StashPacks.Patch.Events;

namespace ModifAmorphic.Outward.StashPacks.Patch
{
    [HarmonyPatch(typeof(Item))]
    internal static class ItemPatches
    {
        [HarmonyPatch(nameof(Item.PerformEquip), MethodType.Normal)]
        [HarmonyPrefix]
        private static bool PerformEquipPrefix(ref Item __instance, EquipmentSlot _slot, bool _alreadySynced = true)
        {

            ItemEvents.RaisePerformEquipBefore(ref __instance, _slot);

            return true;
        }
    }
}
