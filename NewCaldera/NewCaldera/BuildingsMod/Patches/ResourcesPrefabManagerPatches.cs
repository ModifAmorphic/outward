using HarmonyLib;
using ModifAmorphic.Outward.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ModifAmorphic.Outward.NewCaldera.BuildingsMod.Patches
{
    [HarmonyPatch(typeof(ResourcesPrefabManager))]
    internal static class ResourcesPrefabManagerPatches
    {
        private static IModifLogger Logger => LoggerFactory.GetLogger(ModInfo.ModId);

        public delegate void PrefabsLoadDelegate(ResourcesPrefabManager resourcesPrefabManager, ref Dictionary<string, Item> itemPrefabs);
        public static event PrefabsLoadDelegate AfterPrefabsLoaded;

        [HarmonyPatch(nameof(ResourcesPrefabManager.Load), MethodType.Normal)]
        [HarmonyPatch(new Type[] {  })]
        [HarmonyPostfix]
        private static void LoadPostFix(ResourcesPrefabManager __instance, ref Dictionary<string, Item> ___ITEM_PREFABS)
        {
            try
            {
                if (!__instance.Loaded || ___ITEM_PREFABS == null || ___ITEM_PREFABS.Count < 1)
                    return;
#if DEBUG
                Logger.LogTrace($"{nameof(ResourcesPrefabManagerPatches)}::{nameof(LoadPostFix)}(): Invoking {nameof(AfterPrefabsLoaded)}.");
#endif
                AfterPrefabsLoaded?.Invoke(__instance, ref ___ITEM_PREFABS);
            }
            catch (Exception ex)
            {
                Logger.LogException($"{nameof(ResourcesPrefabManagerPatches)}::{nameof(LoadPostFix)}(): Exception invoking {nameof(AfterPrefabsLoaded)}.", ex);
            }
        }

//        public delegate bool TryGetItemVisualPrefabDelegate(string prefabPath, out Transform prefab);
//        public static TryGetItemVisualPrefabDelegate TryGetItemVisualPrefab;

//        [HarmonyPatch(nameof(ResourcesPrefabManager.Load), MethodType.Normal)]
//        [HarmonyPatch(new Type[] { })]
//        [HarmonyPostfix]
//        private static void GetItemVisualPrefabPostFix(ResourcesPrefabManager __instance, string _visualPath, ref Transform __result)
//        {
//            try
//            {
//                if (!_visualPath.StartsWith("modifamorphicprefabs"))
//                    return;
//#if DEBUG
//                Logger.LogTrace($"{nameof(ResourcesPrefabManagerPatches)}::{nameof(GetItemVisualPrefabPostFix)}(): Invoking {nameof(TryGetItemVisualPrefab)}.");
//#endif
//                if (TryGetItemVisualPrefab != null && TryGetItemVisualPrefab.Invoke(_visualPath, out var prefab))
//                {
//                    __result = prefab;
//                }
//            }
//            catch (Exception ex)
//            {
//                Logger.LogException($"{nameof(ResourcesPrefabManagerPatches)}::{nameof(GetItemVisualPrefabPostFix)}(): Exception invoking {nameof(TryGetItemVisualPrefab)}.", ex);
//            }
//        }
    }
}
