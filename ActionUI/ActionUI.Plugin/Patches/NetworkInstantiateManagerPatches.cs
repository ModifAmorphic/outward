using HarmonyLib;
using ModifAmorphic.Outward.Logging;
using System;

namespace ModifAmorphic.Outward.ActionUI.Patches
{
    [HarmonyPatch(typeof(NetworkInstantiateManager))]
    internal static class NetworkInstantiateManagerPatches
    {
        private static IModifLogger Logger => LoggerFactory.GetLogger(ModInfo.ModId);

        public delegate void AddLocalPlayerDelegate(NetworkInstantiateManager manager, int playerID, CharacterSave save);
        public static event AddLocalPlayerDelegate BeforeAddLocalPlayer;

        [HarmonyPatch(nameof(NetworkInstantiateManager.AddLocalPlayer), MethodType.Normal)]
        [HarmonyPrefix]
        private static void AddLocalPlayerPrefix(NetworkInstantiateManager __instance, int _playerID, CharacterSave _save)
        {
            try
            {
                Logger?.LogTrace($"{nameof(NetworkInstantiateManagerPatches)}::{nameof(AddLocalPlayerPrefix)} called. Invoking {nameof(BeforeAddLocalPlayer)}.");
                BeforeAddLocalPlayer?.Invoke(__instance, _playerID, _save);
            }
            catch (Exception ex)
            {
                Logger?.LogException($"{nameof(NetworkInstantiateManagerPatches)}::{nameof(AddLocalPlayerPrefix)}. Exception invoking {nameof(BeforeAddLocalPlayer)}.", ex);
            }
        }

    }
}
