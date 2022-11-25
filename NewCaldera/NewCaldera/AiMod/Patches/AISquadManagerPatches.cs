//using HarmonyLib;
//using ModifAmorphic.Outward.Logging;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using UnityEngine;

//namespace ModifAmorphic.Outward.NewCaldera.AiMod.Patches
//{
//    [HarmonyPatch(typeof(AISquadManager))]
//    internal static class AISquadManagerPatches
//    {
//        private static IModifLogger Logger => LoggerFactory.GetLogger(ModInfo.ModId);

//        public delegate void SetSquadActiveDelegate(AISquad squad, bool active, bool resetPositions);
//        public static event SetSquadActiveDelegate BeforeSetSquadActive;

//        [HarmonyPatch("Awake", MethodType.Normal)]
//        [HarmonyPatch(new Type[] { })]
//        [HarmonyPrefix]
//        private static void AwakePrefix(AISquadManager __instance, DictionaryExt<UID, AISquad> ___m_allSquads)
//        {
//            try
//            {
//#if DEBUG
//                var allActiveSquads = __instance.GetComponentsInChildren<AISquad>();
//                var allInActiveSquads = __instance.GetComponentsInChildren<AISquad>(true)?.Where(s => !s.gameObject.activeSelf);

//                Logger.LogTrace($"{nameof(AISquadManagerPatches)}::{nameof(AwakePrefix)}(): m_allSquads.Count == {___m_allSquads.Count}" +
//                    $"\n\tallActiveSquads == {allActiveSquads?.Count()}" +
//                    $"\n\tallInActiveSquads == {allInActiveSquads.Count()}");

//                var logSquads = "ActiveSquads: ";
//                foreach (var squad in allActiveSquads)
//                    logSquads += "\n\t" + squad.name;
//                Logger.LogTrace($"{nameof(AISquadManagerPatches)}::{nameof(AwakePrefix)}():\n" + logSquads);

//#endif
//                //BeforeSetSquadActive?.Invoke(__instance, _active, _resetPositions);
//            }
//            catch (Exception ex)
//            {
//                Logger.LogException($"{nameof(AISquadManagerPatches)}::{nameof(CheckSquadsDisablePrefix)}(): Exception invoking {nameof(BeforeSetSquadActive)}.", ex);
//            }
//        }

//        [HarmonyPatch("CheckSquadsDisable", MethodType.Normal)]
//        [HarmonyPatch(new Type[] {  })]
//        [HarmonyPrefix]
//        private static void CheckSquadsDisablePrefix(AISquadManager __instance, DictionaryExt<UID, AISquad> ___m_allSquads)
//        {
//            try
//            {
//#if DEBUG
//                //Logger.LogTrace($"{nameof(AISquadManagerPatches)}::{nameof(CheckSquadsDisablePrefix)}(): m_allSquads.Count == {___m_allSquads.Count}.");
//#endif
//                //BeforeSetSquadActive?.Invoke(__instance, _active, _resetPositions);
//            }
//            catch (Exception ex)
//            {
//                Logger.LogException($"{nameof(AISquadManagerPatches)}::{nameof(CheckSquadsDisablePrefix)}(): Exception invoking {nameof(BeforeSetSquadActive)}.", ex);
//            }
//        }

//        [HarmonyPatch("FindSpawn", MethodType.Normal)]
//        [HarmonyPatch(new Type[] { typeof(bool)})]
//        [HarmonyPostfix]
//        private static void FindSpawnPostfix(AISquadManager __instance, bool _ignoreSight)
//        {
//            try
//            {
//#if DEBUG
//                Logger.LogTrace($"{nameof(AISquadManagerPatches)}::{nameof(FindSpawnPostfix)}(_ignoreSight: {_ignoreSight}): .");
//#endif
//                //BeforeSetSquadActive?.Invoke(__instance, _active, _resetPositions);
//            }
//            catch (Exception ex)
//            {
//                Logger.LogException($"{nameof(AISquadManagerPatches)}::{nameof(FindSpawnPostfix)}(): Exception invoking {nameof(BeforeSetSquadActive)}.", ex);
//            }
//        }

//        [HarmonyPatch(nameof(AISquadManager.TryActivateSquad), MethodType.Normal)]
//        [HarmonyPatch(new Type[] { typeof(AISquad), typeof(bool) })]
//        [HarmonyPostfix]
//        private static void TryActivateSquadPostfix(AISquadManager __instance, AISquad _squad, bool _resetPositions = true)
//        {
//            try
//            {
//#if DEBUG
//                Logger.LogTrace($"{nameof(AISquadManagerPatches)}::{nameof(TryActivateSquadPostfix)}(_squad: '{_squad.name}', _resetPositions: {_resetPositions}): .");
//#endif
//                //BeforeSetSquadActive?.Invoke(__instance, _active, _resetPositions);
//            }
//            catch (Exception ex)
//            {
//                Logger.LogException($"{nameof(AISquadManagerPatches)}::{nameof(TryActivateSquadPostfix)}(): Exception invoking {nameof(BeforeSetSquadActive)}.", ex);
//            }
//        }

//        [HarmonyPatch(nameof(AISquadManager.TryDeactivateSquad), MethodType.Normal)]
//        [HarmonyPatch(new Type[] { typeof(AISquad) })]
//        [HarmonyPostfix]
//        private static void TryDeactivateSquadPostfix(AISquadManager __instance, AISquad _squad)
//        {
//            try
//            {
//#if DEBUG
//                Logger.LogTrace($"{nameof(AISquadManagerPatches)}::{nameof(TryDeactivateSquadPostfix)}(_squad: '{_squad.name}'): .");
//#endif
//                //BeforeSetSquadActive?.Invoke(__instance, _active, _resetPositions);
//            }
//            catch (Exception ex)
//            {
//                Logger.LogException($"{nameof(AISquadManagerPatches)}::{nameof(TryDeactivateSquadPostfix)}(): Exception invoking {nameof(BeforeSetSquadActive)}.", ex);
//            }
//        }

//    }
//}
