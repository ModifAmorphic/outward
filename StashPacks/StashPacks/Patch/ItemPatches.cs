using HarmonyLib;
using ModifAmorphic.Outward.StashPacks.Patch.Events;

namespace ModifAmorphic.Outward.StashPacks.Patch
{
    [HarmonyPatch(typeof(Item))]
    internal static class ItemPatches
    {
        [HarmonyPatch(nameof(Item.DisplayName), MethodType.Getter)]
        [HarmonyPostfix]
        public static void DisplayNamePostfix(Item __instance, ref string __result)
        {
            ItemEvents.SetDisplayNameAfter(__instance, __result, out __result);
        }

        //[HarmonyPatch(nameof(Item.OnReceiveParentChangeRequest), MethodType.Normal)]
        //[HarmonyPostfix]
        //private static void OnReceiveItemParentChangeRequestPostfix(ref Item __instance, string[] _infos)
        //{
        //    ItemEvents.RaiseOnReceiveItemParentChangeRequestAfter(ref __instance, _infos);
        //}
    }
}
