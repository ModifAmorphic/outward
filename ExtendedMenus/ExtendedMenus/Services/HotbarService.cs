using ModifAmorphic.Outward.ActionMenus.Patches;
using ModifAmorphic.Outward.ActionMenus.Settings;
using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.Unity.ActionMenus;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace ModifAmorphic.Outward.ActionMenus.Services
{
    internal class HotbarService
    {
        private readonly HotbarSettings _settings;

        private IModifLogger Logger => _getLogger.Invoke();
        private readonly Func<IModifLogger> _getLogger;

        private readonly Hotbars _hotbars;
        private readonly CharacterUI _characterUI;

        public HotbarService(Hotbars hotbars, CharacterUI characterUI, HotbarSettings settings, Func<IModifLogger> getLogger)
        {
            _hotbars = hotbars;
            _characterUI = characterUI;
            _settings = settings;
            _getLogger = getLogger;

            settings.HotbarsChanged += (bars) => ConfigureSlots();
            settings.ActionSlotsChanged += (slots) => ConfigureSlots();

            QuickSlotPanelPatches.StartInitAfter += DisableKeyboardQuickslots;
            QuickSlotControllerSwitcherPatches.StartInitAfter += SwapCanvasGroup;
            ConfigureSlots();
        }

        private void SwapCanvasGroup(QuickSlotControllerSwitcher controllerSwitcher, ref CanvasGroup canvasGroup)
        {
            var keyboard = canvasGroup.GetComponent<KeyboardQuickSlotPanel>();
            if (keyboard != null)
                if (keyboard.CharacterUI.RewiredID == _characterUI.RewiredID)
                {
                    canvasGroup = _hotbars.GetComponent<CanvasGroup>();
                    DisableKeyboardQuickslots(keyboard);
                }
        }

        public void DisableKeyboardQuickslots(KeyboardQuickSlotPanel keyboard)
        {
            Logger.LogDebug($"Checking if Keyboard QuickSlots for RewiredID {keyboard.CharacterUI.RewiredID} should be disabled. Comparing to RewiredID {_characterUI.RewiredID}");
            if (keyboard.CharacterUI.RewiredID == _characterUI.RewiredID)
                keyboard.gameObject.SetActive(false);
        }


        public void ConfigureSlots()
        {
            Logger.LogDebug($"Setting Hotbars to {_settings.Hotbars}, Slots per hotbar to {_settings.ActionSlots}");
            _hotbars?.ConfigureHotbars(Hotbars.HotbarType.Grid, _settings.Hotbars, _settings.ActionSlots);
        }
    }
}
