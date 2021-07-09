using HarmonyLib;
using ModifAmorphic.Outward.Events;
using System;
using System.Linq;
using System.Text;
using UnityEngine;
using ModifAmorphicLogging = ModifAmorphic.Outward.Logging;

namespace ModifAmorphic.Outward.KeyBindings
{
    /// <summary>
    /// This class is responsible for adding extra quickslots to the character's QuickSlotManager.
    /// </summary>
    [HarmonyPatch(typeof(CharacterQuickSlotManager), "Awake")]
    internal static class CharacterQuickSlotManagerPatches
    {
        private static int _quickslotsToAdd;
        private static ModifAmorphicLogging.Logger _logger;

        private static void LoggerEvents_LoggerLoaded(object sender, ModifAmorphicLogging.Logger logger) => _logger = logger;
        private static void QuickSlotExtenderEvents_SlotsChanged(object sender, QuickSlotExtendedArgs e) => (_quickslotsToAdd) = (e.ExtendedQuickSlots.Count());

        [EventSubscription]
        public static void SubscribeToEvents()
        {
            LoggerEvents.LoggerLoaded += LoggerEvents_LoggerLoaded;
            QuickSlotExtenderEvents.SlotsChanged += QuickSlotExtenderEvents_SlotsChanged;
        }

        /// <summary>
        /// Add extra quickslots to the CharacterQuickSlotManager QuickSlots transform.
        /// </summary>
        /// <param name="__instance"></param>
        [HarmonyPrefix]
        public static void OnAwake_AddQuickSlots(CharacterQuickSlotManager __instance)
        {
            if (_quickslotsToAdd < 1)
                return;

            try
            {
                var quickslotTransform = __instance.transform.Find("QuickSlots");
                var qsStartId = quickslotTransform.childCount + 1;
                _logger.LogDebug($"Adding {_quickslotsToAdd} extra quickslots to the CharacterQuickSlotManager QuickSlots transform. Starting with Id {qsStartId}.");
                var sb = new StringBuilder();
#if DEBUG
                sb.AppendLine($"\n\t//////*************quickslotTransform Start*************//////");
                for (int i = 0; i < quickslotTransform.childCount; i++)
                {
                    var child = quickslotTransform.GetChild(i);
                    var quickslot = child.gameObject.GetComponent<QuickSlot>();
                    sb.AppendLine($"\tIndex: {i}; GameObject Name: {child.gameObject.name}; QuickSlot Name: {quickslot.name}");
                }
                sb.AppendLine($"\t//////*************quickslotTransform End*************//////");
                _logger.LogTrace(sb.ToString());
#endif

                for (int s = 0; s < _quickslotsToAdd; s++)
                {
                    GameObject gameObject = new GameObject($"ExtraQuickSlot_{s}");
                    _logger.LogDebug($"{nameof(OnAwake_AddQuickSlots)}(): created new GameObject(ExtraQuickSlot_{s}).  gameObject.name: '{gameObject.name}'");
                    QuickSlot qs = gameObject.AddComponent<QuickSlot>();
                    qs.name = (qsStartId + s).ToString();
                    gameObject.transform.SetParent(quickslotTransform);
                }
                //Once all the extra slots are added, set the quickslotTransform parent back to the self.transform.
                quickslotTransform.SetParent(__instance.transform);
                var quickslotTransformAfterSetParent = __instance.transform.Find("QuickSlots");

                sb = new StringBuilder();
                sb.AppendLine($"\n\t//////*************quickslotTransformAfterSetParent after quickslotTransform.SetParent(self.transform) Start*************//////");
                if (quickslotTransformAfterSetParent != null)
                {
                    for (int i = 0; i < quickslotTransformAfterSetParent.childCount; i++)
                    {
                        var child = quickslotTransformAfterSetParent.GetChild(i);
                        var quickslot = child?.gameObject?.GetComponent<QuickSlot>();
                        sb.AppendLine($"\tIndex: {i}; Child Name: {child?.name}; GameObject Name: {child?.gameObject?.name}; QuickSlot Name: {quickslot?.name}");
                    }
                }
                else
                {
                    sb.AppendLine("\tquickslotTransformAfterSetParent null.  No GameObjects.");
                }
                sb.AppendLine($"\t//////*************quickslotTransformAfterSetParent after quickslotTransform.SetParent(self.transform) End*************//////");
                _logger.LogDebug(sb.ToString());

                _logger.LogDebug($"{nameof(OnAwake_AddQuickSlots)}(): complete.");
            }
            catch (Exception ex)
            {
                _logger.LogException($"{nameof(OnAwake_AddQuickSlots)}() error.", ex);
                throw;
            }
        }
    }
}
