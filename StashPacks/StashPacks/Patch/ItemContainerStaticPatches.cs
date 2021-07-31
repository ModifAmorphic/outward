//using HarmonyLib;
//using ModifAmorphic.Outward.StashPacks.Patch.Events;
//using System;
//using System.Collections.Generic;
//using System.Text;

//namespace ModifAmorphic.Outward.StashPacks.Patch
//{
//    [HarmonyPatch(typeof(ItemContainerStatic))]
//    internal static class ItemContainerStaticPatches
//    {
//        [HarmonyPatch(nameof(ItemContainerStatic.AddItem))]
//        [HarmonyPostfix]
//        private static void AddItemPostFix(ItemContainerStatic __instance)
//        {
//            ItemContainerStaticEvents.RaiseBagContentsChangedAfter(__instance.RefBag, ItemContainerStaticEvents.ContentChanges.ItemAdded);
//        }
//        [HarmonyPatch(nameof(ItemContainerStatic.RemoveItem))]
//        [HarmonyPostfix]
//        private static void RemoveItemPostFix(ItemContainerStatic __instance)
//        {
//            ItemContainerStaticEvents.RaiseBagContentsChangedAfter(__instance.RefBag, ItemContainerStaticEvents.ContentChanges.ItemRemoved);
//        }
//        //[HarmonyPatch(nameof(ItemContainerStatic.RemoveStackAmount))]
//        //[HarmonyPostfix]
//        //private static void RemoveStackAmountPostfix(ItemContainer __instance)
//        //{
//        //    ItemContainerStaticEvents.RaiseBagContentsChangedAfter(__instance.RefBag, ItemContainerStaticEvents.ContentChanges.ItemRemoved);
//        //}
//    }
//}
