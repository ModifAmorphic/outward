using HarmonyLib;
using ModifAmorphic.Outward.StashPacks.Patch.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace ModifAmorphic.Outward.StashPacks.Patch
{
    [HarmonyPatch(typeof(ConnectPhotonMaster))]
    internal static class ConnectPhotonMasterPatches
    {
        [HarmonyPatch(nameof(ConnectPhotonMaster.OnPhotonPlayerConnected), MethodType.Normal)]
        [HarmonyPostfix]
        private static void OnPhotonPlayerConnected(PhotonPlayer _newPlayer)
        {
            ConnectPhotonMasterEvents.RaiseOnPhotonPlayerConnectedAfter(_newPlayer);
        }
    }
}
