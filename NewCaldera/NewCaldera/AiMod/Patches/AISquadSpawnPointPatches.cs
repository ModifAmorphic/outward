//using HarmonyLib;
//using ModifAmorphic.Outward.Logging;
//using System;
//using System.Collections.Generic;
//using System.Linq;

//namespace ModifAmorphic.Outward.NewCaldera.AiMod.Patches
//{
//    /// <summary>
//    /// These never seem to be used in dungeons, maybe on resets?
//    /// </summary>
//    [HarmonyPatch(typeof(AISquadSpawnPoint))]
//    internal static class AISquadSpawnPointPatches
//    {
//        private static IModifLogger Logger => LoggerFactory.GetLogger(ModInfo.ModId);

//        public delegate void SetSquadActiveDelegate(AISquad squad, bool active, bool resetPositions);
//        public static event SetSquadActiveDelegate BeforeSetSquadActive;

//        [HarmonyPatch(nameof(AISquadSpawnPoint.ChooseSquadToSpawn), MethodType.Normal)]
//        [HarmonyPatch(new Type[] {  })]
//        [HarmonyPrefix]
//        private static void ChooseSquadToSpawnPrefix(AISquadSpawnPoint __instance)
//        {
//            try
//            {
//#if DEBUG
//                Logger.LogTrace($"{nameof(AISquadSpawnPointPatches)}::{nameof(ChooseSquadToSpawnPrefix)}(): .");
//#endif
//                //BeforeSetSquadActive?.Invoke(__instance, _active, _resetPositions);
//            }
//            catch (Exception ex)
//            {
//                Logger.LogException($"{nameof(AISquadSpawnPointPatches)}::{nameof(ChooseSquadToSpawnPrefix)}(): Exception invoking {nameof(BeforeSetSquadActive)}.", ex);
//            }
//        }

//        [HarmonyPatch("SpawnSquad", MethodType.Normal)]
//        [HarmonyPatch(new Type[] { typeof(AISquad)})]
//        [HarmonyPostfix]
//        private static void SpawnSquadPostfix(AISquadSpawnPoint __instance, AISquad _squad)
//        {
//            try
//            {
//#if DEBUG
//                Logger.LogTrace($"{nameof(AISquadSpawnPointPatches)}::{nameof(SpawnSquadPostfix)}(): squad {_squad.name}.");
//#endif
//                //BeforeSetSquadActive?.Invoke(__instance, _active, _resetPositions);
//            }
//            catch (Exception ex)
//            {
//                Logger.LogException($"{nameof(AISquadSpawnPointPatches)}::{nameof(SpawnSquadPostfix)}(): Exception invoking {nameof(BeforeSetSquadActive)}.", ex);
//            }
//        }

//        [HarmonyPatch(nameof(AISquadSpawnPoint.CheckValidSpawn), MethodType.Normal)]
//        [HarmonyPatch(new Type[] {  })]
//        [HarmonyPostfix]
//        private static void CheckValidSpawnPostfix(AISquadSpawnPoint __instance, bool __result)
//        {
//            try
//            {
//#if DEBUG
//                Logger.LogTrace($"{nameof(AISquadSpawnPointPatches)}::{nameof(CheckValidSpawnPostfix)}(): Result {__result}.");
//#endif
//                //BeforeSetSquadActive?.Invoke(__instance, _active, _resetPositions);
//            }
//            catch (Exception ex)
//            {
//                Logger.LogException($"{nameof(AISquadSpawnPointPatches)}::{nameof(CheckValidSpawnPostfix)}(): Exception invoking {nameof(BeforeSetSquadActive)}.", ex);
//            }
//        }

//    }
//}
