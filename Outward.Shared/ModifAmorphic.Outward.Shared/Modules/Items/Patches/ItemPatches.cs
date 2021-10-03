using HarmonyLib;
using ModifAmorphic.Outward.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace ModifAmorphic.Outward.Modules.Items
{
    [HarmonyPatch(typeof(Item))]
    public static class ItemPatches
    {
        [PatchLogger]
        private static IModifLogger Logger { get; set; } = new NullLogger();

        public static event Action<Item> GetItemIconBefore;


        [HarmonyPatch(nameof(Item.ItemIcon), MethodType.Getter)]
        [HarmonyPrefix]
        private static void ItemIconPrefix(Item __instance)
        {
#if DEBUG
            //Logger.LogTrace($"{nameof(ItemPatches)}::{nameof(ItemIconPrefix)}: Triggered for Item {__instance.DisplayName} ({__instance.ItemID}).");
#endif
            GetItemIconBefore?.Invoke(__instance);
        }


        //public static event Func<(Item item, Transform itemVisual, bool special), Transform> GetItemVisualBefore;
        public static event Action<(Item item, bool special)> GetItemVisualBefore;
        [HarmonyPatch(nameof(Item.GetItemVisual), MethodType.Normal)]
        [HarmonyPatch(new Type[] { typeof(bool) })]
        [HarmonyPrefix]
        //private static void GetItemVisualPrefix(ref Item __instance, bool _special, ref Transform __result)
        private static void GetItemVisualPrefix(ref Item __instance, bool _special)
        {
            try
            {
                //ignore prefabs.
                if (!string.IsNullOrEmpty(__instance?.UID)) 
                {
                    Logger.LogTrace($"{nameof(ItemPatches)}::{nameof(GetItemVisualPrefix)}(): Invoking {nameof(GetItemVisualBefore)}() for item " +
                        $"{__instance?.DisplayName} ({__instance?.UID}).");
                    GetItemVisualBefore?.Invoke((__instance, _special));
                }
            }
            catch (Exception ex)
            {
                Logger.LogException($"{nameof(ItemPatches)}::{nameof(GetItemVisualPrefix)}(): Exception Invoking {nameof(GetItemVisualBefore)}().", ex);
            }
        }

        public static event Func<Item, bool> LoadVisualsOverride;
        //[HarmonyPatch("LoadVisuals", MethodType.Normal)]
        //[HarmonyPrefix]
        private static bool LoadVisualsPrefix(ref Item __instance)
        {
            try
            {
                if (!(__instance is Armor || __instance is Weapon))
                    return true;

                Logger.LogTrace($"{nameof(ItemPatches)}::{nameof(LoadVisualsPrefix)}(): Invoked on Item UID '{__instance?.UID}', '{__instance?.DisplayName}' of type {__instance?.GetType()}. Invoking {nameof(LoadVisualsOverride)}()");

                if ((LoadVisualsOverride?.Invoke(__instance) ?? false))
                {
                    Logger.LogTrace($"{nameof(ItemPatches)}::{nameof(LoadVisualsPrefix)}(): {nameof(LoadVisualsOverride)}() result: was true. Returning false to override base method.");
                    return false;
                }

            }
            catch (Exception ex)
            {
                Logger.LogException($"{nameof(ItemPatches)}::{nameof(LoadVisualsPrefix)}(): Exception Invoking {nameof(LoadVisualsOverride)}().", ex);
            }
            Logger.LogTrace($"{nameof(ItemPatches)}::{nameof(LoadVisualsPrefix)}(): {nameof(LoadVisualsOverride)}() result: was false. Returning true and continuing base method invocation.");
            return true;
        }
    }
}
