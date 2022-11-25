using HarmonyLib;
using ModifAmorphic.Outward.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ModifAmorphic.Outward.NewCaldera.AiMod.Patches
{
    [HarmonyPatch(typeof(Dropable))]
    internal static class DropablePatches
    {
        private static IModifLogger Logger => LoggerFactory.GetLogger(ModInfo.ModId);

        public delegate bool GenerateDropDelegate(Dropable dropable, ItemContainer dropContainer);
        public static GenerateDropDelegate IsGenerateDropsAllowed;

        [HarmonyPatch(nameof(Dropable.GenerateContents), MethodType.Normal)]
        [HarmonyPatch(new Type[] { typeof(ItemContainer) })]
        [HarmonyPrefix]
        private static bool GenerateContentsPrefix(Dropable __instance, ItemContainer _container)
        {
            try
            {
#if DEBUG
                Logger.LogTrace($"{nameof(DropablePatches)}::{nameof(GenerateContentsPrefix)}() for owner character {_container?.OwnerCharacter?.name}.");
#endif
                return IsGenerateDropsAllowed.Invoke(__instance, _container);
            }
            catch (Exception ex)
            {
                Logger.LogException($"{nameof(DropablePatches)}::{nameof(GenerateContentsPrefix)}(): Exception invoking {nameof(IsGenerateDropsAllowed)} for character drop {_container?.OwnerCharacter?.name}.", ex);
            }

            return true;
        }
    }
}
