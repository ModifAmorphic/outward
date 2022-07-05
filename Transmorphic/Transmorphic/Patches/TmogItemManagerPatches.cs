using HarmonyLib;
using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.Transmorphic.Settings;
using System;
using System.Collections.Generic;
using System.Text;

namespace ModifAmorphic.Outward.Transmorphic.Patches
{
    [HarmonyPatch(typeof(ItemManager))]
    internal static class TmogItemManagerPatches
    {
        private static IModifLogger Logger => LoggerFactory.GetLogger(ModInfo.ModId);


        public static event Action<Item> TransmogItemAdded;

        [HarmonyPatch(nameof(ItemManager.RequestItemInitialization), MethodType.Normal)]
        [HarmonyPostfix]
        [HarmonyPatch(new Type[] { typeof(Item) })]
        public static void RequestItemInitializationPostfix(ItemManager __instance, Item _item)
        {
            try
            {
                if (!IsTmogItemAdded(_item, __instance))
                    return;

                Logger.LogTrace($"{nameof(TmogItemManagerPatches)}::{nameof(RequestItemInitializationPostfix)}(): Invoked for Item {_item.ItemID} - {_item.DisplayName} ({_item.UID}). Invoking {nameof(TransmogItemAdded)}().");
                TransmogItemAdded?.Invoke(_item);
            }
            catch (Exception ex)
            {
                Logger.LogException($"{nameof(TmogItemManagerPatches)}::{nameof(RequestItemInitializationPostfix)}(): Exception Invoking {nameof(TransmogItemAdded)}().", ex);
            }
        }

        [HarmonyPatch(nameof(ItemManager.ItemHasBeenAdded), MethodType.Normal)]
        [HarmonyPostfix]
        //[HarmonyPatch(typeof(bool), new Type[] { typeof(Item) })]
        public static void ItemHasBeenAddedPostfix(ItemManager __instance, ref Item _newItem, ref bool __result)
        {
            try
            {
                if (!__result || !IsTmogItemAdded(_newItem, __instance))
                    return;

                Logger.LogTrace($"{nameof(TmogItemManagerPatches)}::{nameof(ItemHasBeenAddedPostfix)}(): Invoked for Item {_newItem.ItemID} - {_newItem.DisplayName} ({_newItem.UID}). Invoking {nameof(TransmogItemAdded)}().");
                TransmogItemAdded?.Invoke(_newItem);
            }
            catch (Exception ex)
            {
                Logger.LogException($"{nameof(TmogItemManagerPatches)}::{nameof(ItemHasBeenAddedPostfix)}(): Exception Invoking {nameof(TransmogItemAdded)}().", ex);
            }
        }

        [HarmonyPatch(nameof(ItemManager.AddStaticItemUID), MethodType.Normal)]
        [HarmonyPostfix]
        [HarmonyPatch(new Type[] { typeof(Item), typeof(string) })]
        public static void AddStaticItemUIDPostfix(ItemManager __instance, Item _newItem, string _UID)
        {
            try
            {
                if (!IsTmogItemAdded(_newItem, __instance))
                    return;

                Logger.LogTrace($"{nameof(TmogItemManagerPatches)}::{nameof(AddStaticItemUIDPostfix)}(): Invoked for Item {_newItem.ItemID} - {_newItem.DisplayName} ({_newItem.UID}). Invoking {nameof(TransmogItemAdded)}().");
                TransmogItemAdded?.Invoke(_newItem);
            }
            catch (Exception ex)
            {
                Logger.LogException($"{nameof(TmogItemManagerPatches)}::{nameof(AddStaticItemUIDPostfix)}(): Exception Invoking {nameof(TransmogItemAdded)}().", ex);
            }
        }

        [HarmonyPatch("CreateItemFromData", MethodType.Normal)]
        [HarmonyPostfix]
        public static void CreateItemFromDataPostfix(ItemManager __instance, string itemUID, string[] itemInfos, ItemManager.ItemSyncType _syncType, string _ownerUID, ref bool __result)
        {
            try
            {
                if (!__result || !IsTmogItemAdded(itemUID, __instance))
                    return;

                var item = __instance.WorldItems[itemUID];
                Logger.LogTrace($"{nameof(TmogItemManagerPatches)}::{nameof(CreateItemFromDataPostfix)}(): Invoked for Item {item.ItemID} - {item.DisplayName} ({item.UID}). Invoking {nameof(TransmogItemAdded)}().");
                TransmogItemAdded?.Invoke(item);
            }
            catch (Exception ex)
            {
                Logger.LogException($"{nameof(TmogItemManagerPatches)}::{nameof(CreateItemFromDataPostfix)}(): Exception Invoking {nameof(TransmogItemAdded)}().", ex);
            }
        }

        private static bool IsTmogItemAdded(Item item, ItemManager itemManager)
        {
            return
                item != null && IsTmogItemAdded(item.UID, itemManager);
        }
        private static bool IsTmogItemAdded(string itemUID, ItemManager itemManager)
        {
            return
                !string.IsNullOrEmpty(itemUID)
                && itemUID.StartsWith(TransmogSettings.ItemStringPrefixUID)
                && itemManager.WorldItems.ContainsKey(itemUID);
        }
    }
}
