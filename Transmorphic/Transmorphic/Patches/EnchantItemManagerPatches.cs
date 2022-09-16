using HarmonyLib;
using ModifAmorphic.Outward.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace ModifAmorphic.Outward.Transmorphic.Patches
{
    [HarmonyPatch(typeof(ItemManager))]
    internal static class EnchantItemManagerPatches
    {
        private static IModifLogger Logger => LoggerFactory.GetLogger(ModInfo.ModId);

        public static event Action<Item> AfterRequestItemInitialization;

        [HarmonyPatch(nameof(ItemManager.RequestItemInitialization))]
        [HarmonyPostfix]
        private static void RequestItemInitializationPostfix(Item _item)
        {
            try
            {
                Logger.LogTrace($"{nameof(EnchantItemManagerPatches)}::{nameof(RequestItemInitializationPostfix)}: Invoking {nameof(AfterRequestItemInitialization)}.");
                AfterRequestItemInitialization?.Invoke(_item);
            }
            catch (Exception ex)
            {
                Logger.LogException($"{nameof(EnchantItemManagerPatches)}::{nameof(RequestItemInitializationPostfix)}(): Exception invoking {nameof(AfterRequestItemInitialization)}.", ex);
            }
        }
    }
}
