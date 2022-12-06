//using HarmonyLib;
//using ModifAmorphic.Outward.Logging;
//using System;
//using System.Collections.Generic;
//using System.Linq;

//namespace ModifAmorphic.Outward.NewCaldera.BuildingsMod.Patches
//{
//    [HarmonyPatch(typeof(BuildingPhaseResourceReqDisplay))]
//    internal static class BuildingPhaseResourceReqDisplayPatches
//    {
//        private static IModifLogger Logger => LoggerFactory.GetLogger(ModInfo.ModId);

//        public delegate bool TryOverrideGetBuildingResourceDelegate(BuildingPhaseResourceReqDisplay resourceDisplay, out BuildingResource.ResourceTypes resourceTypeResult);
//        public static TryOverrideGetBuildingResourceDelegate TryOverrideGetBuildingResource;

//        [HarmonyPatch(nameof(BuildingPhaseResourceReqDisplay.GetBuildingResource), MethodType.Normal)]
//        [HarmonyPatch(new Type[] { })]
//        [HarmonyPrefix]
//        private static bool GetBuildingResourcePrefix(BuildingPhaseResourceReqDisplay __instance, ref BuildingResource.ResourceTypes __result)
//        {
//            try
//            {
//#if DEBUG
//                Logger.LogTrace($"{nameof(BuildingPhaseResourceReqDisplayPatches)}::{nameof(GetBuildingResourcePrefix)}(): Invoking {nameof(TryOverrideGetBuildingResource)}.");
//#endif
//                if (TryOverrideGetBuildingResource?.Invoke(__instance, out var resourceType) ?? false)
//                {
//                    __result = resourceType;
//                    Logger.LogTrace($"{nameof(BuildingPhaseResourceReqDisplayPatches)}::{nameof(GetBuildingResourcePrefix)}(): {nameof(TryOverrideGetBuildingResource)} success. Overriding GetBuildingResource result to {__result}.");
//                    return false;
//                }
//            }
//            catch (Exception ex)
//            {
//                Logger.LogException($"{nameof(BuildingPhaseResourceReqDisplayPatches)}::{nameof(GetBuildingResourcePrefix)}(): Exception invoking {nameof(TryOverrideGetBuildingResource)}.", ex);
//            }

//            return true;
//        }
//    }
//}
