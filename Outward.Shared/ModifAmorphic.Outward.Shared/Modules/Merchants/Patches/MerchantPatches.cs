using HarmonyLib;
using ModifAmorphic.Outward.Logging;
using System;
using UnityEngine;

namespace ModifAmorphic.Outward.Modules.Merchants
{
    [HarmonyPatch(typeof(Merchant))]
    internal static class MerchantPatches
    {
        [MultiLogger]
        private static IModifLogger Logger { get; set; } = new NullLogger();

        public static event Action<(Merchant Merchant, Transform MerchantInventoryTablePrefab, Dropable DropableInventory)> InitDropTableGameObjectAfter;
        //check the path Interactions/NPCs/NPC_Minor/MarketTradersHighPrices/UNPC_DLC_Market_Alchemy/NPC/MerchantSettings/
        //m_merchantInventoryTablePrefab.GetComponent<Dropable>() //Contains droptables
        //dropable.m_allGuaranteedDrops //list of guaranteed drops


        [HarmonyPatch("InitDropTableGameObject", MethodType.Normal)]
        [HarmonyPostfix]
        private static void InitDropTableGameObjectPostfix(Merchant __instance, ref Transform ___m_merchantInventoryTablePrefab, ref Dropable ___m_dropableInventory)
        {
            try
            {
                Logger.LogTrace($"{nameof(MerchantPatches)}::{nameof(InitDropTableGameObjectPostfix)}: Raising event {nameof(InitDropTableGameObjectAfter)}.");
                InitDropTableGameObjectAfter?.Invoke((__instance, ___m_merchantInventoryTablePrefab, ___m_dropableInventory));
            }
            catch (Exception ex)
            {
                Logger.LogException($"{nameof(MerchantPatches)}::{nameof(InitDropTableGameObjectPostfix)}: Excepting triggering event {nameof(InitDropTableGameObjectAfter)}.", ex);
            }
        }
    }
}
