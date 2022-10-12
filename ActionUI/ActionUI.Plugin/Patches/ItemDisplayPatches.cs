using HarmonyLib;
using ModifAmorphic.Outward.Logging;
using System;

namespace ModifAmorphic.Outward.ActionUI.Patches
{
    [HarmonyPatch(typeof(ItemDisplay))]
    internal static class ItemDisplayPatches
    {
        private static IModifLogger Logger => LoggerFactory.GetLogger(ModInfo.ModId);

        public delegate void AfterSetReferencedItemDelegate(ItemDisplay itemDisplay, Item item);
        public static event AfterSetReferencedItemDelegate AfterSetReferencedItem;

        [HarmonyPatch(nameof(ItemDisplay.SetReferencedItem))]
        [HarmonyPatch(new Type[] { typeof(Item) })]
        [HarmonyPostfix]
        private static void SetReferencedItemPostfix(ItemDisplay __instance, Item _item)
        {
            try
            {
                if (_item == null)
                    return;

                if (__instance?.LocalCharacter == null || __instance.LocalCharacter.OwnerPlayerSys == null || !__instance.LocalCharacter.IsLocalPlayer)
                    return;

                Logger.LogTrace($"{nameof(ItemDisplayPatches)}::{nameof(SetReferencedItemPostfix)}(): Invoking {nameof(AfterSetReferencedItem)} for Item {_item.name}.");
                AfterSetReferencedItem?.Invoke(__instance, _item);
            }
            catch (Exception ex)
            {
                Logger.LogException($"{nameof(ItemDisplayPatches)}::{nameof(SetReferencedItemPostfix)}(): Exception invoking {nameof(AfterSetReferencedItem)} for Item {_item?.name}.", ex);
            }
        }

        //public delegate void AfterRefreshEnchantedIconDelegate(ItemDisplay itemDisplay, EquipmentSetSkill setSkill);
        //public static event AfterRefreshEnchantedIconDelegate AfterRefreshEnchantedIcon;

        //[HarmonyPatch("RefreshEnchantedIcon", MethodType.Normal)]
        //[HarmonyPostfix]
        //public static void RefreshEnchantedIconPostfix(ItemDisplay __instance, Item ___m_refItem)
        //{
        //    try
        //    {
        //        if (___m_refItem == null || !(___m_refItem is EquipmentSetSkill equipSetSkill))
        //            return;

        //        Logger.LogTrace($"{nameof(ItemDisplayPatches)}::{nameof(RefreshEnchantedIconPostfix)}(): Invoked for ItemDisplay {__instance.name} and Item {___m_refItem?.ItemID} - {___m_refItem?.DisplayName} ({___m_refItem?.UID}). Invoking {nameof(AfterRefreshEnchantedIcon)}().");
        //        AfterRefreshEnchantedIcon?.Invoke(__instance, equipSetSkill);
        //    }
        //    catch (Exception ex)
        //    {
        //        Logger.LogException($"{nameof(ItemDisplayPatches)}::{nameof(RefreshEnchantedIconPostfix)}(): Exception Invoking {nameof(AfterRefreshEnchantedIcon)}().", ex);
        //    }
        //}
    }
}
