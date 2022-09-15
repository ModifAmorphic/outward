using HarmonyLib;
using ModifAmorphic.Outward.Logging;
using System;

namespace ModifAmorphic.Outward.Modules.QuickSlots.KeyBindings
{
    [HarmonyPatch(typeof(KeyboardQuickSlotPanel), "InitializeQuickSlotDisplays")]
    internal static class KeyboardQuickSlotPanelPatches
    {
        private static int _quickslotsToAdd;
        private static int _quickslotStartId;

        private static bool _isInitialized = false;

        public static void Configure(int qsToAdd, int qsStartId)
        {
            if (_isInitialized)
                throw new InvalidOperationException($"{nameof(KeyboardQuickSlotPanelPatches)}.{nameof(Configure)} cannot be called after the {nameof(KeyboardQuickSlotPanel)}'s InitializeQuickSlotDisplays method has been called.");

            _quickslotsToAdd = qsToAdd;
            _quickslotStartId = qsStartId;
        }

        [MultiLogger]
        private static IModifLogger Logger { get; set; } = new NullLogger();

        /// <summary>
        /// Adds extra slots to the displayed keyboard quick slot panel.
        /// </summary>
        /// <param name="__instance"></param>
        [HarmonyPrefix]
        public static void OnInitializeQuickSlotDisplays_AddExtraSlots(KeyboardQuickSlotPanel __instance)
        {
            _isInitialized = true;
            if (_quickslotsToAdd < 1)
                return;

            try
            {
                int exStartIndex = __instance.DisplayOrder.Length;
                int exEndIndex = __instance.DisplayOrder.Length + _quickslotsToAdd;
                int exSlotId = _quickslotStartId;
                Array.Resize(ref __instance.DisplayOrder, exEndIndex);
                Logger.LogTrace($"{nameof(OnInitializeQuickSlotDisplays_AddExtraSlots)}(): exStartIndex={exStartIndex}; exEndIndex={exEndIndex}; Starting Quickslot Id={exSlotId})");
                for (int n = exStartIndex; n < exEndIndex; n++)
                {
                    __instance.DisplayOrder[n] = (QuickSlot.QuickSlotIDs)(exSlotId++);
                }
            }
            catch (Exception ex)
            {
                Logger.LogException($"{nameof(OnInitializeQuickSlotDisplays_AddExtraSlots)}() error.", ex);
                throw;
            }
        }
    }
}
