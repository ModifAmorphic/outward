using HarmonyLib;
using ModifAmorphic.Outward.Logging;
using System;

namespace ModifAmorphic.Outward.UI.Patches
{
    [HarmonyPatch(typeof(SplitScreenManager))]
    internal static class SplitScreenManagerPatches
    {
        private static IModifLogger Logger => LoggerFactory.GetLogger(ModInfo.ModId);

        public delegate void AwakeCharacterUI(SplitScreenManager splitScreenManager, ref CharacterUI characterUI);
        public static event AwakeCharacterUI AwakeAfter;
        [HarmonyPatch("Awake")]
        [HarmonyPostfix]
        private static void AwakePostfix(SplitScreenManager __instance, ref CharacterUI ___m_charUIPrefab)
        {
            try
            {
                if (__instance.enabled)
                {
                    Logger.LogTrace($"{nameof(SplitScreenManagerPatches)}::{nameof(AwakePostfix)}(): Invoked. Invoking {nameof(AwakeAfter)}({nameof(SplitScreenManager)}, ref {nameof(CharacterUI)}).");
                    AwakeAfter?.Invoke(__instance, ref ___m_charUIPrefab);
                }
            }
            catch (Exception ex)
            {
                Logger.LogException($"{nameof(SplitScreenManagerPatches)}::{nameof(AwakePostfix)}(): Exception Invoking {nameof(AwakeAfter)}({nameof(SplitScreenManager)}, ref {nameof(CharacterUI)}).", ex);
            }
        }

        public static event Action<SplitScreenManager> AddLocalPlayerAfter;
        [HarmonyPatch("AddLocalPlayer")]
        [HarmonyPostfix]
        private static void AddLocalPlayerPostfix(SplitScreenManager __instance)
        {
            try
            {
                if (__instance.enabled)
                {
                    Logger.LogTrace($"{nameof(SplitScreenManagerPatches)}::{nameof(AddLocalPlayerPostfix)}(): Invoked. Invoking {nameof(AddLocalPlayerAfter)}({nameof(SplitScreenManager)}).");
                    AddLocalPlayerAfter?.Invoke(__instance);
                }
            }
            catch (Exception ex)
            {
                Logger.LogException($"{nameof(SplitScreenManagerPatches)}::{nameof(AddLocalPlayerPostfix)}(): Exception Invoking {nameof(AddLocalPlayerAfter)}({nameof(SplitScreenManager)}).", ex);
            }
        }

        public delegate void RemoveLocalPlayer(SplitScreenManager splitScreenManager, SplitPlayer player, string playerUID);
        public static event RemoveLocalPlayer RemoveLocalPlayerAfter;
        [HarmonyPatch("RemoveLocalPlayer")]
        [HarmonyPatch(new Type[] { typeof(SplitPlayer), typeof(string) })]
        [HarmonyPostfix]
        private static void RemoveLocalPlayerPostfix(SplitScreenManager __instance, SplitPlayer _player, string _playerUID)
        {
            try
            {
                if (__instance.enabled)
                {
                    Logger.LogTrace($"{nameof(SplitScreenManagerPatches)}::{nameof(RemoveLocalPlayerPostfix)}(): Invoked. Invoking {nameof(RemoveLocalPlayerAfter)}({nameof(SplitScreenManager)}, {nameof(SplitPlayer)}, string).");
                    RemoveLocalPlayerAfter?.Invoke(__instance, _player, _playerUID);
                }
            }
            catch (Exception ex)
            {
                Logger.LogException($"{nameof(SplitScreenManagerPatches)}::{nameof(RemoveLocalPlayerPostfix)}(): Exception Invoking {nameof(RemoveLocalPlayerAfter)}({nameof(SplitScreenManager)}, {nameof(SplitPlayer)}, string).", ex);
            }
        }
    }
}
