using HarmonyLib;
using Localizer;
using ModifAmorphic.Outward.Events;
using ModifAmorphic.Outward.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ModifAmorphic.Outward.Patches
{
    [HarmonyPatch(typeof(LocalizationManager))]
    public static class LocalizationManagerPatches
    {
        [PatchLogger]
        private static IModifLogger Logger { get; set; } = new NullLogger();

        public static event Action<Dictionary<int, ItemLocalization>> LoadItemLocalizationAfter;

        [HarmonyPatch("LoadItemLocalization")]
        [HarmonyPostfix]
#pragma warning disable IDE0051 // Remove unused private members
        private static void LoadItemLocalizationPostfix(ref Dictionary<int, ItemLocalization> ___m_itemLocalization)
#pragma warning restore IDE0051 // Remove unused private members
        {
            LoadItemLocalizationAfter?.Invoke(___m_itemLocalization);
        }

    }
}
