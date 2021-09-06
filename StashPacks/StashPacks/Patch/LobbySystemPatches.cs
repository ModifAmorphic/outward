using HarmonyLib;
using ModifAmorphic.Outward.StashPacks.Patch.Events;

namespace ModifAmorphic.Outward.StashPacks.Patch
{
    [HarmonyPatch(typeof(LobbySystem))]
    internal static class LobbySystemPatches
    {
        [HarmonyPatch(nameof(LobbySystem.PlayerSystemHasBeenDestroyed), MethodType.Normal)]
        [HarmonyPostfix]
        private static void PlayerSystemHasBeenDestroyedPostfix(string _uid)
        {
            LobbySystemEvents.RaisePlayerSystemHasBeenDestroyedAfter(_uid);
        }

        [HarmonyPatch(nameof(LobbySystem.OnLeftRoom), MethodType.Normal)]
        [HarmonyPostfix]
        private static void OnLeftRoom()
        {
            LobbySystemEvents.RaiseOnLeftRoomAfter();
        }
    }
}
