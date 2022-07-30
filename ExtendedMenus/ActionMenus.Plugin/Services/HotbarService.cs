using ModifAmorphic.Outward.ActionMenus.Extensions;
using ModifAmorphic.Outward.ActionMenus.Models;
using ModifAmorphic.Outward.ActionMenus.Patches;
using ModifAmorphic.Outward.ActionMenus.Settings;
using ModifAmorphic.Outward.Coroutines;
using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.Unity.ActionMenus;
using ModifAmorphic.Outward.Unity.ActionMenus.Controllers;
using Rewired;
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

        private readonly HotbarsContainer _hotbarsContainer;
        private readonly IHotbarController _hotbars;
        private readonly Player _player;
        private readonly Character _character;
        private readonly CharacterUI _characterUI;

        private readonly LevelCoroutines _levelCoroutines;

        private ControllerType _activeController;
        public ControllerType ActiveController => _activeController;

        public HotbarService(HotbarsContainer hotbarsContainer, Player player, Character character, HotbarSettings settings, LevelCoroutines levelCoroutines, Func<IModifLogger> getLogger)
        {
            if (hotbarsContainer == null)
                throw new ArgumentNullException(nameof(hotbarsContainer));
            if (character == null)
                throw new ArgumentNullException(nameof(character));
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));

            _hotbarsContainer = hotbarsContainer;
            _hotbars = _hotbarsContainer.Controller;

            _player = player;
            _character = character;
            _characterUI = character.CharacterUI;
            _settings = settings;
            _levelCoroutines = levelCoroutines;
            _getLogger = getLogger;

            settings.HotbarsChanged += (bars) => ConfigureSlots();
            settings.ActionSlotsChanged += (slots) =>
            {
                ConfigureSlots();
                AssignSlotActions(_character);
            };

            QuickSlotPanelPatches.StartInitAfter += DisableKeyboardQuickslots;
            QuickSlotControllerSwitcherPatches.StartInitAfter += SwapCanvasGroup;
            CharacterManagerPatches.AfterApplyQuickSlots += QueueActionSlotAssignments;
            ConfigureSlots();
        }

        private void SwapCanvasGroup(QuickSlotControllerSwitcher controllerSwitcher, ref CanvasGroup canvasGroup)
        {
            var keyboard = canvasGroup.GetComponent<KeyboardQuickSlotPanel>();
            if (keyboard != null)
            {
                if (keyboard.CharacterUI.RewiredID == _characterUI.RewiredID)
                {
                    _activeController = ControllerType.Keyboard;
                    canvasGroup = _hotbarsContainer.GetComponent<CanvasGroup>();
                    DisableKeyboardQuickslots(keyboard);
                }
            }
            else
                _activeController = ControllerType.Mouse;
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

            var keyListener = new HotbarKeyListener(_player);
            _hotbars?.ConfigureHotbars(_settings.Hotbars, 1, _settings.ActionSlots, keyListener);

            for (int b = 0; b < _settings.Hotbars; b++)
            {
                for (int s = 0; s < _settings.ActionSlots; s++)
                {
                    _hotbars.GetActionSlots()[b][s].Controller.Configure(new ActionSlotConfig()
                    {
                        ShowZeroStackAmount = false,
                        ShowCooldownTime = false,
                        EmptySlotOption = EmptySlotOptions.Image,
                        HotkeyText = (s + 1).ToString(),
                    });
                }
            }
        }
        public void QueueActionSlotAssignments(Character character)
        {
            _levelCoroutines.InvokeAfterLevelLoaded(NetworkLevelLoader.Instance, () => AssignSlotActions(character), 300);
        }
        
        public void AssignSlotActions(Character character)
        {
            if (character.UID != _character.UID)
                return;

            _hotbarsContainer.Controller.RegisterActionViewData(new SlotActionViewData(_player, _character, _getLogger));
            _hotbarsContainer.ActionsViewer.ConfigureExit(() => _player.GetButtonDown(ControlsInput.GetMenuActionName(ControlsInput.MenuActions.Cancel)));

            string nameFormat = "ActionSlot_00";
            int slotIndex = 0;
            var learnedItems = character.Inventory.SkillKnowledge.GetLearnedItems();
            Logger.LogDebug($"Character has {learnedItems.Count} learned skills.");
            foreach (var item in learnedItems)
            {
                var actionSlot = _hotbars.GetActionSlots()[0][slotIndex];
                var actionName = (slotIndex + 1).ToString(nameFormat);
                TryAssignAction(actionSlot, actionName, item);
                slotIndex++;
                if (slotIndex >= _hotbars.GetActionSlots()[0].Length)
                    break;
            }
        }
        public bool TryAssignAction(ActionSlot actionSlot, string actionName, Item item)
        {
            try
            {
                Logger.LogDebug($"Assigning item {item?.name} to action slot {actionSlot?.name} and Rewired Action {actionName}.");
                actionSlot.Controller.AssignSlotAction(GetSlotAction(item));
                //This needs to be set to pass certain checks in the base game.
                item.SetQuickSlot(actionSlot.SlotIndex);
                return true;
            }
            catch (Exception ex)
            {
                Logger.LogException($"Encountered exception assigning action {actionName} for Item/Skill {item?.name}.", ex);
                return false;
            }
        }
        public void ToggleEditMode(bool enabled)
        {
            _hotbars.ToggleEditMode(enabled);
        }
        private ISlotAction GetSlotAction(Item item)
        {
            var slotAction = new ItemSlotAction(item, _player, _characterUI.TargetCharacter, _getLogger)
            {
                ActionIcon = item.ItemIcon,
                Cooldown = new ItemCooldown(item),
                Stack = item.IsStackable() ? item.ToStackable(_character.Inventory) : null,
                HasDynamicIcon = item.HasDynamicQuickSlotIcon,
                //TargetAction = () => Logger.LogInfo($"Action {actionName} triggered!")
            };
            return slotAction;

        }
        
    }
}
