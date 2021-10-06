using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace ModifAmorphic.Outward.Transmorph.Patches
{
    [HarmonyPatch(typeof(SplitScreenManager))]
    internal static class SplitScreenManagerPatches
    {

        public static event Action<SplitScreenManager, CharacterUI> AwakeAfter;
        [HarmonyPatch("Awake", MethodType.Normal)]
        [HarmonyPostfix]
        private static void AwakePostfix(SplitScreenManager __instance, ref CharacterUI ___m_charUIPrefab)
        {
            AwakeAfter?.Invoke(__instance, ___m_charUIPrefab);
        }


        public enum CacheStatus { New, Existing };

        [HarmonyPatch(nameof(SplitScreenManager.GetCachedUI), MethodType.Normal)]
        [HarmonyPrefix]
        private static void GetCachedUIPrefix(int _id, CharacterUI[] ___m_cachedUI, out CacheStatus __state)
        {
            if (___m_cachedUI != null && ___m_cachedUI.Length > _id && ___m_cachedUI[_id] != null)
            {
                __state = CacheStatus.Existing;
                return;
            }
            __state = CacheStatus.New;
        }

        public static event Action<(SplitScreenManager SplitScreenManager, int Id, CacheStatus CacheStatus, CharacterUI ResultRef)> GetCachedUIAfter;

        [HarmonyPatch(nameof(SplitScreenManager.GetCachedUI), MethodType.Normal)]
        [HarmonyPostfix]
        private static void GetCachedUIPostfix(SplitScreenManager __instance, int _id, CacheStatus __state, ref CharacterUI __result)
        {
            GetCachedUIAfter?.Invoke((__instance, _id, __state, __result));
        }

    }
}
