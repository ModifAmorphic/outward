using HarmonyLib;
using ModifAmorphic.Outward.Events;
using ModifAmorphic.Outward.ExtraSlots.Config;
using ModifAmorphic.Outward.ExtraSlots.Display;
using ModifAmorphic.Outward.ExtraSlots.Events;
using ModifAmorphic.Outward.ExtraSlots.Query;
using ModifAmorphic.Outward.Logging;
using System;
using System.Collections.Generic;
using UnityEngine;
using ModifAmorphicLogging = ModifAmorphic.Outward.Logging;

namespace ModifAmorphic.Outward.ExtraSlots.Patches
{
    [HarmonyPatch(typeof(QuickSlotPanel))]
    internal static class QuickSlotPanelPatches
    {
        private static IModifLogger _logger => LoggerFactory.GetLogger(ModInfo.ModId);
        private static readonly HashSet<UID> _characterHasBarsAligned = new HashSet<UID>();
        private static readonly HashSet<UID> _characterHasQsCentered = new HashSet<UID>();
        private static readonly HashSet<UID> _characterHasQsDefault = new HashSet<UID>();
        private static ExtraSlotsSettings _esSettings;
        private static Vector3? _originalQsKeyboardPos = null;
        private static Vector3? _originalQsBarPos = null;
        private static Vector3? _originalStabilityBarPos = null;
        private static bool _qsBarNeedsAlignmentNextUpdate = false;

        private static void SaveOriginalQsBarPosition(QuickSlotPanel quickSlotPanel)
        {
            if (_originalQsKeyboardPos == null)
            {
                _originalQsKeyboardPos = new Vector3(quickSlotPanel.RectTransform.position.x, quickSlotPanel.RectTransform.position.y, quickSlotPanel.RectTransform.position.z);
                _originalQsBarPos = UIQuery.GetQuickslotRectTransform(quickSlotPanel.CharacterUI).position;
            }
        }
        private static void SaveOriginalStabilityBarPosition(QuickSlotPanel quickSlotPanel)
        {
            if (_originalStabilityBarPos == null)
            {
                var stabilityDisplay = UIQuery.GetCharacterStabilityRect(quickSlotPanel.CharacterUI);
                _originalStabilityBarPos = new Vector3(stabilityDisplay.position.x, stabilityDisplay.position.y, stabilityDisplay.position.z);
            }
        }
        private static void MoveQsBarToOriginalPosition(UIElement quickSlotPanel)
        {
            _logger.LogTrace("Moving Quickslot Bar back to Original Position.");
            if (_originalQsKeyboardPos != null)
            {
                _ = _characterHasQsCentered.RemoveWhere(c => c == quickSlotPanel.LocalCharacter.UID);
                var qsQsBarRectTransform = UIQuery.GetQuickslotRectTransform(quickSlotPanel.CharacterUI);
                TransformMover.SetPosition(qsQsBarRectTransform.transform, _originalQsBarPos.Value);
                TransformMover.SetPosition(quickSlotPanel.transform, _originalQsKeyboardPos.Value);
                _ = _characterHasQsDefault.Add(quickSlotPanel.LocalCharacter.UID);
            }
        }
        private static void MoveStabilityBarToOriginalPosition(RectTransform stabilityBar)
        {
            if (_originalStabilityBarPos != null)
            {
                _logger.LogTrace("Moving Stability Bar back to Original Position.\n" +
                    $"\tOriginal Position: ({_originalStabilityBarPos.Value.x},{_originalStabilityBarPos.Value.y}, {_originalStabilityBarPos.Value.z})\n" +
                    $"\tCurrent Position: ({stabilityBar.position.x},{stabilityBar.position.y}, {stabilityBar.position.z})");
                TransformMover.SetPosition(stabilityBar, _originalStabilityBarPos.Value);
                _logger.LogTrace("Moved Stability Bar back to Original Position.\n" +
                    $"\tOriginal Position: ({_originalStabilityBarPos.Value.x},{_originalStabilityBarPos.Value.y}, {_originalStabilityBarPos.Value.z})\n" +
                    $"\tCurrent Position: ({stabilityBar.position.x},{stabilityBar.position.y}, {stabilityBar.position.z})");
            }
        }

        [EventSubscription]
        public static void SubscribeToEvents()
        {
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
            else if (_originalQsKeyboardPos != null && !_characterHasQsDefault.Contains(__instance.LocalCharacter.UID))
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
                var keyboardRect = quickSlotPanel.RectTransform;
                if (!_characterHasBarsAligned.Contains(characterUID) || _qsBarNeedsAlignmentNextUpdate)
                {
                    var stabilityRect = UIQuery.GetCharacterStabilityRect(quickSlotPanel.CharacterUI);
                    //Reset everything back to original before aligning.
                    MoveStabilityBarToOriginalPosition(stabilityRect);
                    MoveQsBarToOriginalPosition(quickSlotPanel);
                    _characterHasBarsAligned.RemoveWhere(c => c == characterUID);

                    var qsBarRectTransform = UIQuery.GetQuickslotRectTransform(quickSlotPanel.CharacterUI);
                    //var qsRectTransform = UIQuery.GetQuickslotRectTransform(quickSlotPanel.CharacterUI);

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
                                TransformMover
                                    .MoveAbove(stabilityRect, keyboardRect, _esSettings.MoveQuickSlotBarUp_Y_Offset.Value);
                                break;
                            }
                        case QuickSlotBarAlignmentOptions.MoveStabilityAboveQuickSlot:
                            {
                                _logger.LogInfo("Moving Stability Bar above QuickSlot Bar.");
                                TransformMover
                                    .MoveAbove(quickSlotPanel.RectTransform, stabilityRect, _esSettings.MoveStabilityBarUp_Y_Offset.Value);
                                break;
                            }
                        case QuickSlotBarAlignmentOptions.AbsolutePositioning:
                            {
                                _logger.LogInfo("Setting Absolution Positions of Quickslot and Stability bars.");
                                //For the quickslot bar, move the parent transform on the Y access and the actual quickslot bar on the X.
                                // This is done because the parent stretches the entire screen horizontal.
                                TransformMover.SetPosition(qsBarRectTransform
                                    , new Vector3(qsBarRectTransform.position.x, _esSettings.QuickSlotBarAbsolute_Y.Value, qsBarRectTransform.position.z));
                                TransformMover.SetRectPosition(keyboardRect
                                    , new Vector3(_esSettings.QuickSlotBarAbsolute_X.Value, keyboardRect.position.y, keyboardRect.position.z));

                                TransformMover.SetPosition(stabilityRect
                                    , new Vector3(_esSettings.StabilityBarAbsolute_X.Value, _esSettings.StabilityBarAbsolute_Y.Value, stabilityRect.position.z));
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
