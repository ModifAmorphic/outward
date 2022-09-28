using HarmonyLib;
using ModifAmorphic.Outward.ActionUI.Models;
using ModifAmorphic.Outward.Logging;
using System;

namespace ModifAmorphic.Outward.ActionUI.Patches
{
    [HarmonyPatch(typeof(ItemDisplay))]
    internal static class ItemDisplayPatches
    {
        private static IModifLogger Logger => LoggerFactory.GetLogger(ModInfo.ModId);

        public delegate void AfterSetReferencedItemDelegate(ItemDisplay itemDisplay, EquipmentSetSkill setSkill);
        public static event AfterSetReferencedItemDelegate AfterSetReferencedItem;

        [HarmonyPatch(nameof(ItemDisplay.SetReferencedItem))]
        [HarmonyPatch(new Type[] { typeof(Item) })]
        [HarmonyPostfix]
        private static void SetReferencedItemPostfix(ItemDisplay __instance, Item _item)
        {
            try
            {
                if (_item == null || !(_item is EquipmentSetSkill equipSetSkill))
                    return;

                Logger.LogTrace($"{nameof(ItemDisplayPatches)}::{nameof(SetReferencedItemPostfix)}(): Invoking {nameof(AfterSetReferencedItem)} for EquipmentSetSkill {equipSetSkill.name}.");
                AfterSetReferencedItem?.Invoke(__instance, equipSetSkill);
            }
            catch (Exception ex)
            {
                Logger.LogException($"{nameof(ItemDisplayPatches)}::{nameof(SetReferencedItemPostfix)}(): Exception invoking {nameof(AfterSetReferencedItem)} for Item {_item?.name}.", ex);
            }
        }
    }
}
