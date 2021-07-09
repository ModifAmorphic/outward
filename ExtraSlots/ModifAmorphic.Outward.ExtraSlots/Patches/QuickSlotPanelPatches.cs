using HarmonyLib;
using ModifAmorphic.Outward.Events;
using ModifAmorphic.Outward.ExtraSlots.Config;
using ModifAmorphic.Outward.ExtraSlots.Display;
using ModifAmorphic.Outward.ExtraSlots.Events;
using ModifAmorphic.Outward.ExtraSlots.Query;
using System;
using System.Collections.Generic;
using UnityEngine;
using ModifAmorphicLogging = ModifAmorphic.Outward.Logging;

namespace ModifAmorphic.Outward.ExtraSlots.Patches
{
    [HarmonyPatch(typeof(QuickSlotPanel))]
    public class QuickSlotPanelPatches
    {
        private static ModifAmorphicLogging.Logger _logger;
        private static readonly HashSet<UID> _characterHasBarsAligned = new HashSet<UID>();
        private static readonly HashSet<UID> _characterHasQsCentered = new HashSet<UID>();
        private static readonly HashSet<UID> _characterHasQsDefault = new HashSet<UID>();
        private static ExtraSlotsSettings _esSettings;
        private static Vector3? _originalQsBarPos = null;
        private static Vector3? _originalQsBarParentPos = null;
        private static Vector3? _originalStabilityBarPos = null;
        private static bool _qsBarNeedsAlignmentNextUpdate = false;

        private static void SaveOriginalQsBarPosition(QuickSlotPanel quickSlotPanel)
        {
            if (_originalQsBarPos == null)
            {
                _originalQsBarPos = new Vector3(quickSlotPanel.transform.position.x, quickSlotPanel.transform.position.y, quickSlotPanel.transform.position.z);
                _originalQsBarParentPos = UIQuery.GetParentRectTransform(quickSlotPanel.transform).transform.position;
            }
        }
        private static void SaveOriginalStabilityBarPosition(QuickSlotPanel quickSlotPanel)
        {
            if (_originalStabilityBarPos == null)
            {
                var stabilityDisplay = UIQuery.GetCharacterStabilityDisplay(quickSlotPanel.LocalCharacter).transform;
                _originalStabilityBarPos = new Vector3(stabilityDisplay.position.x, stabilityDisplay.position.y, stabilityDisplay.position.z);
            }
        }
        private static void MoveQsBarToOriginalPosition(UIElement quickSlotPanel)
        {
            _logger.LogTrace("Moving Quickslot Bar back to Original Position.");
            if (_originalQsBarPos != null)
            {
                _ = _characterHasQsCentered.RemoveWhere(c => c == quickSlotPanel.LocalCharacter.UID);
                var qsParentRectTransform = UIQuery.GetParentRectTransform(quickSlotPanel.transform);
                TransformMover.SetPosition(qsParentRectTransform.transform, _originalQsBarParentPos.Value);
                TransformMover.SetPosition(quickSlotPanel.transform, _originalQsBarPos.Value);
                _ = _characterHasQsDefault.Add(quickSlotPanel.LocalCharacter.UID);
            }
        }
        private static void MoveStabilityBarToOriginalPosition(UIElement stabilityBar)
        {
            if (_originalStabilityBarPos != null)
            {
                _logger.LogTrace("Moving Stability Bar back to Original Position.\n" +
                    $"\tOriginal Position: ({_originalStabilityBarPos.Value.x},{_originalStabilityBarPos.Value.y}, {_originalStabilityBarPos.Value.z})\n" +
                    $"\tCurrent Position: ({stabilityBar.transform.position.x},{stabilityBar.transform.position.y}, {stabilityBar.transform.position.z})");
                TransformMover.SetPosition(stabilityBar.transform, _originalStabilityBarPos.Value);
                _logger.LogTrace("Moved Stability Bar back to Original Position.\n" +
                    $"\tOriginal Position: ({_originalStabilityBarPos.Value.x},{_originalStabilityBarPos.Value.y}, {_originalStabilityBarPos.Value.z})\n" +
                    $"\tCurrent Position: ({stabilityBar.transform.position.x},{stabilityBar.transform.position.y}, {stabilityBar.transform.position.z})");
            }
        }

        [EventSubscription]
        public static void SubscribeToEvents()
        {
            ExtraSlotsConfigEvents.LoggerSettingsChanged += (object sender, ExtraSlotsSettings e) => _logger = new ModifAmorphicLogging.Logger(e.LogLevel.Value, ModInfo.ModName);
            ExtraSlotsConfigEvents.UiSettingsChanged += (object sender, ExtraSlotsSettings e) =>
            {
                _logger?.LogDebug($"{nameof(QuickSlotPanelPatches)} - UI config change notification received.");
                _esSettings = e;
                _characterHasBarsAligned.Clear();
                _characterHasQsCentered.Clear();
                _characterHasQsDefault.Clear();
                _qsBarNeedsAlignmentNextUpdate = true;
            };
        }

        [HarmonyPatch("Update")]
        [HarmonyPostfix]
        public static void QuickSlotPanel_Update(QuickSlotPanel __instance)
        {
            if (__instance.name != "Keyboard" || __instance.transform.parent.name != "QuickSlot")
                return;

            var qspMover = new QuickSlotPanelDisplayMover(__instance, _logger);
            if (_esSettings.CenterQuickSlotPanel.Value)
            {
                if (!_characterHasQsCentered.Contains(__instance.LocalCharacter.UID))
                {
                    _characterHasQsDefault.RemoveWhere(c => c == __instance.LocalCharacter.UID);
                    _logger.LogTrace($"{nameof(QuickSlotPanelPatches)}.{nameof(QuickSlotPanel_Update)}(): Centering {__instance.name};");
                    _ = qspMover.CenterHorizontally(_esSettings.CenterQuickSlot_X_Offset.Value);
                    _characterHasQsCentered.Add(__instance.LocalCharacter.UID);
                }
            }
            else if (_originalQsBarPos != null && !_characterHasQsDefault.Contains(__instance.LocalCharacter.UID))
            {
                MoveQsBarToOriginalPosition(__instance);
            }

            //check to see if the alingment option has changed
            if (_qsBarNeedsAlignmentNextUpdate)
                AlignQuickSlotAndStabilityBars(__instance);
        }
        [HarmonyPatch("AwakeInit")]
        [HarmonyPostfix]
        public static void QuickSlotPanel_AwakeInit(QuickSlotPanel __instance)
        {
            if (__instance.name != "Keyboard" || __instance.transform.parent.name != "QuickSlot")
                return;

            //Capture the original positions of the Stability and Quickslot Bars
            SaveOriginalStabilityBarPosition(__instance);
            SaveOriginalQsBarPosition(__instance);

            AlignQuickSlotAndStabilityBars(__instance);
        }
        private static void AlignQuickSlotAndStabilityBars(QuickSlotPanel quickSlotPanel)
        {
            try
            {
                var characterUID = quickSlotPanel.LocalCharacter.UID;

                if (!_characterHasBarsAligned.Contains(characterUID) || _qsBarNeedsAlignmentNextUpdate)
                {
                    var stabilityDisplay = UIQuery.GetCharacterStabilityDisplay(quickSlotPanel.LocalCharacter);
                    //Reset everything back to original before aligning.
                    MoveStabilityBarToOriginalPosition(stabilityDisplay);
                    MoveQsBarToOriginalPosition(quickSlotPanel);
                    _characterHasBarsAligned.RemoveWhere(c => c == characterUID);

                    var stabilityRect = (RectTransform)stabilityDisplay.transform;
                    var qsParentRectTransform = UIQuery.GetParentRectTransform(quickSlotPanel.transform);

                    var rectMover = new TransformMover(_logger);

                    switch (_esSettings.QuickSlotBarAlignmentOption.Value)
                    {
                        case QuickSlotBarAlignmentOptions.None:
                            {
                                _logger.LogInfo("Alignment set to None.  Not moving bars.");
                                break;
                            }
                        case QuickSlotBarAlignmentOptions.MoveQuickSlotAboveStability:
                            {
                                _logger.LogInfo("Moving QuickSlot Bar above Stability Bar.");
                                rectMover
                                    .MoveAbove(stabilityRect, qsParentRectTransform, _esSettings.MoveQuickSlotBarUp_Y_Offset.Value);
                                break;
                            }
                        case QuickSlotBarAlignmentOptions.MoveStabilityAboveQuickSlot:
                            {
                                _logger.LogInfo("Moving Stability Bar above QuickSlot Bar.");
                                rectMover
                                    .MoveAbove(quickSlotPanel.RectTransform, stabilityRect, _esSettings.MoveStabilityBarUp_Y_Offset.Value);
                                break;
                            }
                        default:
                            {
                                _logger.LogWarning($"Unknown Alignment Option: {(int?)_esSettings?.QuickSlotBarAlignmentOption?.Value}. Defaulting to no alignment.");
                                break;
                            }
                    }
                    _characterHasBarsAligned.Add(characterUID);
                    _qsBarNeedsAlignmentNextUpdate = false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogException($"{nameof(QuickSlotPanel_AwakeInit)}() error.", ex);
                throw;
            }
        }
    }
}
