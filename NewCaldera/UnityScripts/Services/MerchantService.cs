using HarmonyLib;
using ModifAmorphic.Outward.UnityScripts.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace ModifAmorphic.Outward.UnityScripts.Services
{
    public class MerchantService
    {
        private readonly Func<Logging.Logger> _loggerFactory;
        private Logging.Logger Logger => _loggerFactory.Invoke();

        public Dictionary<AreaEnum, List<AdditonalMerchantInventory>> AreaMerchants { get; set; } = new Dictionary<AreaEnum,List<AdditonalMerchantInventory>>();

        public MerchantService(Func<Logging.Logger> loggerFactory)
        {
            _loggerFactory = loggerFactory;
            Patch();
            AfterInitDropTableGameObject += AddMerchantDrops;
        }

        private void AddMerchantDrops(MonoBehaviour merchant, ref MonoBehaviour dropableInventory)
        {
            if (!AreaManager.TryGetCurrentArea(out var currentArea) || !AreaMerchants.TryGetValue(currentArea, out var merchInvs))
                return;

            for (int i = 0; i < merchInvs.Count; i++)
            {
                var inventory = merchInvs[i];
                if (IsMerchantMatch(merchant, merchInvs[i]))
                {
                    string shopName = merchant.GetPropertyValue<string>(OutwardAssembly.Types.Merchant, "ShopName");
                    Logger.LogDebug($"Adding {merchInvs[i].GuaranteedItems.Count} additional items to merchant shop {shopName}");
                    AddGuaranteedDrops(ref dropableInventory, merchInvs[i].GuaranteedItems, merchInvs[i].Name);
                }
            }
        }

        private void AddGuaranteedDrops(ref MonoBehaviour dropableInventory, List<ItemAmounts> additionalItems, string dropsName)
        {
            var guaranteedDrop = dropableInventory.gameObject.AddComponent(OutwardAssembly.Types.GuaranteedDrop);
            guaranteedDrop.SetField(OutwardAssembly.Types.GuaranteedDrop, "ItemGenatorName", dropsName);
            var drops = ReflectionExtensions.CreateList(OutwardAssembly.Types.BasicItemDrop);
            foreach (var item in additionalItems)
            {
                
                var itemDrop = Activator.CreateInstance(OutwardAssembly.Types.BasicItemDrop);
                itemDrop.SetField(OutwardAssembly.Types.BasicItemDrop, "ItemID", item.ItemID);
                itemDrop.SetField(OutwardAssembly.Types.BasicItemDrop, "MinDropCount", item.MinQuantity);
                itemDrop.SetField(OutwardAssembly.Types.BasicItemDrop, "MaxDropCount", item.MaxQuantity);
                drops.Add(itemDrop);
            }
            guaranteedDrop.SetField(OutwardAssembly.Types.GuaranteedDrop, "m_itemDrops", drops);
        }

        private bool IsMerchantMatch(MonoBehaviour merchant, AdditonalMerchantInventory additonalInventory)
        {
            if (additonalInventory.NpcSearchType == NpcSearchTypes.ByUID)
            {
                string merchantUID = merchant.GetFieldValue(OutwardAssembly.Types.Merchant, "UID").ToString();
                return !string.IsNullOrWhiteSpace(merchantUID) && additonalInventory.MerchantUIDs.Contains(merchantUID);
            }
            else if (additonalInventory.NpcSearchType == NpcSearchTypes.ByShopName)
            {
                string shopName = merchant.GetPropertyValue<string>(OutwardAssembly.Types.Merchant, "ShopName");
                return !string.IsNullOrWhiteSpace(shopName) && additonalInventory.ShopNames.Contains(shopName);
            }
            else if (additonalInventory.NpcSearchType == NpcSearchTypes.ByShopNpcName)
            {
                var snpc = merchant.GetComponentInParent(OutwardAssembly.Types.SNPC);
                if (snpc == null)
                    return false;
                
                return additonalInventory.ShopNpcNames.Contains(snpc.name);
            }
            else if (additonalInventory.NpcSearchType == NpcSearchTypes.All)
            {
                string merchantUID = merchant.GetFieldValue(OutwardAssembly.Types.Merchant, "UID").ToString();
                var uidMatch = !string.IsNullOrWhiteSpace(merchantUID) && additonalInventory.MerchantUIDs.Contains(merchantUID);

                string shopName = merchant.GetPropertyValue<string>(OutwardAssembly.Types.Merchant, "ShopName");
                var nameMatch = !string.IsNullOrWhiteSpace(shopName) && additonalInventory.ShopNames.Contains(shopName);

                var snpc = merchant.GetComponentInParent(OutwardAssembly.Types.SNPC);
                var snpcMatch = snpc != null && additonalInventory.ShopNpcNames.Contains(snpc.name);

                return uidMatch && nameMatch && snpcMatch;
            }

            return false;
        }

        public void AddMerchants(string merchantsFilePath)
        {
            try
            {
                Logger.LogDebug($"Loading merchants file '{merchantsFilePath}'.");
                var json = File.ReadAllText(merchantsFilePath);
                var invHolder = JsonConvert.DeserializeObject<MerchantInventoriesHolder>(json);
                if (invHolder != null && invHolder.AdditonalMerchantInventories.Any())
                {
                    var areaInvs = invHolder.AdditonalMerchantInventories
                        .GroupBy(m => m.Area);

                    foreach (var areaInv in areaInvs)
                    {
                        var addInventories = areaInv.Select(m => m).ToList();
                        if (AreaMerchants.TryGetValue(areaInv.Key, out var merchants))
                        {
                            merchants.AddRange(addInventories);
                        }
                        else
                        {
                            AreaMerchants.Add(areaInv.Key, addInventories);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogException($"Failed to load Merchants file '{merchantsFilePath}'.", ex);
            }
        }

        #region Patches
        private void Patch()
        {
            Logger.LogInfo("Patching Merchant");

            var initDropTableGameObject = OutwardAssembly.Types.Merchant.GetMethod("InitDropTableGameObject", BindingFlags.NonPublic | BindingFlags.Instance);
            var initDropTableGameObjectPostFix = this.GetType().GetMethod(nameof(InitDropTableGameObjectPostfix), BindingFlags.NonPublic | BindingFlags.Static);
            HarmonyPatcher.Instance.Harmony.Patch(initDropTableGameObject, postfix: new HarmonyMethod(initDropTableGameObjectPostFix));
        }

        //private delegate void RegisterItemLocalizations(ref Dictionary<int, ItemLocalization> itemLocalizations);
        private static event Action AfterLoadItemLocalization;

        public delegate void InitDropTableGameObjectDelegate(MonoBehaviour merchant, ref MonoBehaviour m_dropableInventory);
        public static event InitDropTableGameObjectDelegate AfterInitDropTableGameObject;

        private static void InitDropTableGameObjectPostfix(MonoBehaviour __instance, ref Transform ___m_merchantInventoryTablePrefab, ref MonoBehaviour ___m_dropableInventory)
        {
            try
            {
                ModifScriptsManager.Instance.Logger.LogTrace($"{nameof(MerchantService)}::{nameof(InitDropTableGameObjectPostfix)}(): Invoked. Invoking {nameof(AfterLoadItemLocalization)}()");
                AfterInitDropTableGameObject?.Invoke(__instance, ref ___m_dropableInventory);
            }
            catch (Exception ex)
            {
                ModifScriptsManager.Instance.Logger.LogException($"{nameof(MerchantService)}::{nameof(InitDropTableGameObjectPostfix)}(): Exception Invoking {nameof(AfterLoadItemLocalization)}().", ex);
            }
        }


        #endregion
    }
}
