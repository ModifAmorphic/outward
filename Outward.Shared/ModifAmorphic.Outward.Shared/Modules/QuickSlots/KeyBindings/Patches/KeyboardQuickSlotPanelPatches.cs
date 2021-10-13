using HarmonyLib;
using ModifAmorphic.Outward.Events;
using ModifAmorphic.Outward.Logging;
using System;
using System.Linq;

namespace ModifAmorphic.Outward.Modules.QuickSlots.KeyBindings
{
    [HarmonyPatch(typeof(KeyboardQuickSlotPanel), "InitializeQuickSlotDisplays")]
    internal static class KeyboardQuickSlotPanelPatches
    {
        private static int _quickslotsToAdd;
        private static int _exQuickslotStartId;

        [MultiLogger]
        private static IModifLogger Logger { get; set; } = new NullLogger();
        private static void QuickSlotExtenderEvents_SlotsChanged(object sender, QuickSlotExtendedArgs e) => (_quickslotsToAdd, _exQuickslotStartId) = (e.ExtendedQuickSlots.Count(), e.StartId);

        [EventSubscription]
        public static void SubscribeToEvents()
        {
            QuickSlotExtenderEvents.SlotsChanged += QuickSlotExtenderEvents_SlotsChanged;
        }

        /// <summary>
        /// Adds extra slots to the displayed keyboard quick slot panel.
        /// </summary>
        /// <param name="__instance"></param>
        [HarmonyPrefix]
        public static void OnInitializeQuickSlotDisplays_AddExtraSlots(KeyboardQuickSlotPanel __instance)
        {
            if (_quickslotsToAdd < 1)
                return;

            try
            {
                int exStartIndex = __instance.DisplayOrder.Length;
                int exEndIndex = __instance.DisplayOrder.Length + _quickslotsToAdd;
                int exSlotId = _exQuickslotStartId;
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
