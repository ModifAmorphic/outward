using ModifAmorphic.Outward.Unity.ActionMenus.Services;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ModifAmorphic.Outward.Unity.ActionMenus.Controllers
{
    internal class ActionSlotController : IActionSlotController
    {

        public ActionSlot ActionSlot { get; private set; }

        private CooldownService _cooldownService;
        private StackService _stackService;
        private EnableToggleService _toggleService;
        private Func<bool> _getEditRequested;
        private Coroutine _iconCoroutine;

        public bool IsUpdateEnabled => ActionSlot.SlotAction?.TargetAction != null && ActionSlot.SlotAction.CheckOnUpdate;
        public bool IsActionNeeded => ActionSlot.ParentCanvas.enabled && ActionSlot.SlotAction?.TargetAction != null && ActionSlot.SlotAction.GetIsActionRequested() && ActionSlot.ActionButton.interactable;

        public ActionSlotController(ActionSlot actionSlot)
        {
            if (actionSlot == null)
                throw new ArgumentNullException(nameof(actionSlot));

            ActionSlot = actionSlot;
            //ActionSlot.Config = ActionSlot.Config?? new IActionSlotConfig();

            actionSlot.MouseClickListener.OnRightClick.AddListener(OnEditRequested);
        }

        public void AssignEmptyAction(Func<bool> getEditRequested = null)
        {
            UnassignSlotAction();

            _getEditRequested = getEditRequested;
            ActionSlot.ActionImage.overrideSprite = null;
            ActionSlot.ActionImage.sprite = null;
            ActionSlot.CooldownImage.enabled = false;
            ActionSlot.CooldownText.enabled = false;
            ActionSlot.StackText.enabled = false;

            if (ActionSlot.Config.EmptySlotOption == EmptySlotOptions.Transparent)
            {
                var color = Color.grey;
                color.a = .05f;
                ActionSlot.ActionImage.color = color;
                ActionSlot.ActionImage.enabled = true;
                ActionSlot.EmptyImage.gameObject.SetActive(false);
            }
            else
            {
                ActionSlot.ActionImage.enabled = false;
                ActionSlot.EmptyImage.gameObject.SetActive(true);
            }
            ActionSlot.HotbarsContainer.HasChanges = true;

        }
        public void AssignSlotAction(ISlotAction slotAction, Func<bool> getEditRequested = null)
        {
            if (slotAction == null)
                throw new ArgumentNullException(nameof(slotAction));

            _getEditRequested = getEditRequested;

            UnassignSlotAction();

            EnableActiveSlot();
            if (!slotAction.HasDynamicIcon)
                AssignSlotIcon(slotAction.ActionIcon);
            else
                AssignDynamicIcon(slotAction.GetDynamicIcon);
            
            ActionSlot.EmptyImage.gameObject.SetActive(false);

            if (slotAction.Cooldown != null && slotAction.Cooldown.HasCooldown)
                EnableCooldownService(slotAction.Cooldown);
            else
                DisableCooldownService();

            if (slotAction.Stack != null && slotAction.Stack.IsStackable)
                EnableStackService(slotAction.Stack.GetAmount);
            else
                DisableStackService();

            StartEnableToggleService(slotAction.GetEnabled);

            ActionSlot.SlotAction = slotAction;
            ActionSlot.SlotAction.OnActionRequested += OnActionRequested;
            ActionSlot.SlotAction.OnEditRequested += OnEditRequested;

            slotAction.SlotActionAssigned(ActionSlot);
            ActionSlot.HotbarsContainer.HasChanges = true;
        }

        public void Refresh()
        {
            if (ActionSlot.SlotAction != null)
                AssignSlotAction(ActionSlot.SlotAction);
            else
                AssignEmptyAction();
        }
        public void ActionSlotAwake()
        {
            Refresh();
            if (ActionSlot.Config != null)
            {
                _cooldownService?.Configure(ActionSlot.Config.ShowCooldownTime, ActionSlot.Config.PreciseCooldownTime);
                _stackService?.Configure(ActionSlot.Config.ShowZeroStackAmount);
                ActionSlot.KeyText.text = ActionSlot.Config.HotkeyText;
            }
        }

        public void ActionSlotUpdate()
        {
            if (!IsUpdateEnabled)
                return;

            if (IsActionNeeded)
            {
                ExecuteEvents.Execute(ActionSlot.ActionButton.gameObject, new PointerEventData(EventSystem.current), ExecuteEvents.submitHandler);
                ActionSlot.SlotAction.TargetAction.Invoke();
            }
            else if (ActionSlot.SlotAction.GetIsEditRequested())
            {
                ActionSlot.HotbarsContainer.ActionsViewer.Show(ActionSlot.SlotId);
            }
        }
        public void Configure(IActionSlotConfig config)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));

            ActionSlot.Config = config;
            Refresh();
            _cooldownService?.Configure(ActionSlot.Config.ShowCooldownTime, ActionSlot.Config.PreciseCooldownTime);
            _stackService?.Configure(ActionSlot.Config.ShowZeroStackAmount);
            ActionSlot.KeyText.text = ActionSlot.Config.HotkeyText;
            ActionSlot.HotbarsContainer.HasChanges = true;
        }

        public void HideCooldown()
        {
            ActionSlot.CooldownImage.fillAmount = 0f;
            ActionSlot.CooldownText.text = string.Empty;
        }
        public void HideStackAmount() => ActionSlot.StackText.text = string.Empty;

        public void ToggleInteractive(bool interactive)
        {
            if (interactive && !ActionSlot.ActionButton.interactable)
            {
                ActionSlot.ActionButton.interactable = true;
            }
            else if (!interactive && ActionSlot.ActionButton.interactable)
            {
                ActionSlot.ActionButton.interactable = true;
            }
        }
        
        public void ToggleEnabled(bool enabled)
        {
            if (enabled && ActionSlot.ActionButton.colors.normalColor != Color.white)
                ActionSlot.SetButtonNormalColor(Color.white);
            else if (!enabled && ActionSlot.ActionButton.colors.normalColor != ActionSlot.DisabledColor)
                ActionSlot.SetButtonNormalColor(ActionSlot.DisabledColor);
        }
        
        private void OnActionRequested()
        {
            if (ActionSlot.ParentCanvas.enabled && ActionSlot.SlotAction?.TargetAction != null && ActionSlot.ActionButton.interactable)
                ActionSlot.SlotAction.TargetAction.Invoke();
        }
        private void OnEditRequested()
        {
            if (ActionSlot.ParentCanvas.enabled && ActionSlot.ActionButton.interactable)
                ActionSlot.HotbarsContainer.ActionsViewer.Show(ActionSlot.SlotId);
        }

        private void EnableActiveSlot()
        {
            ActionSlot.CooldownImage.enabled = true;
            ActionSlot.CooldownText.enabled = true;
            ActionSlot.StackText.enabled = true;
            ActionSlot.ActionImage.enabled = true;

            ActionSlot.CooldownText.text = String.Empty;
            ActionSlot.StackText.text = String.Empty;
        }
        private void UnassignSlotAction()
        {
            if (ActionSlot.SlotAction != null)
            {
                ActionSlot.SlotAction.OnActionRequested -= OnActionRequested;
                ActionSlot.SlotAction.SlotActionUnassigned();
                ActionSlot.SlotAction = null;
            }
            if (_iconCoroutine != null)
            {
                ActionSlot.StopCoroutine(_iconCoroutine);
                _iconCoroutine = null;
            }
        }

        private void AssignSlotIcon(Sprite spriteIcon)
        {
            try
            {
                if (ActionSlot.ActionImage.sprite != spriteIcon)
                {
                    var color = Color.white;
                    color.a = 1f;
                    ActionSlot.ActionImage.color = color;
                    ActionSlot.ActionImage.sprite = spriteIcon;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Could not assign spritIcon {spriteIcon?.name} to ActionSlot {ActionSlot?.name}.");
                Debug.LogException(ex);
            }
        }

        private void EnableCooldownService(ICooldown cooldown)
        {

            if (cooldown == null)
                throw new ArgumentNullException(nameof(cooldown));

            if (_cooldownService != null)
            {
                _cooldownService.StopTracking();
                _cooldownService = null;
            }

            _cooldownService = new CooldownService(ActionSlot.CooldownImage, ActionSlot.CooldownText, ActionSlot.Config.ShowCooldownTime, ActionSlot.Config.PreciseCooldownTime, this);
            _cooldownService.TrackCooldown(cooldown);
        }
        private void DisableCooldownService()
        {
            _cooldownService?.StopTracking();
        }
        private void EnableStackService(Func<int> getStackAmount)
        {
            if (getStackAmount == null)
                throw new ArgumentNullException(nameof(getStackAmount));

            if (_stackService == null)
                _stackService = new StackService(ActionSlot.StackText, ActionSlot.Config.ShowZeroStackAmount, this);
            else
                _stackService.Configure(ActionSlot.Config.ShowZeroStackAmount);

            _stackService.TrackStackAmount(getStackAmount);
        }
        private void DisableStackService()
        {
            _stackService?.StopTracking();
        }
        private void StartEnableToggleService(Func<bool> getEnabled)
        {
            if (getEnabled == null)
                throw new ArgumentNullException(nameof(getEnabled));

            if (_toggleService == null)
                _toggleService = new EnableToggleService(this);

            _toggleService.TrackEnableToggle(getEnabled);
        }

        private void AssignDynamicIcon(Func<Sprite> getIcon)
        {
            _iconCoroutine = ActionSlot.StartCoroutine(AssignIconCoroutine(getIcon));
        }
        private IEnumerator AssignIconCoroutine(Func<Sprite> getIcon)
        {
            while (true)
            {
                AssignSlotIcon(getIcon.Invoke());
                yield return new WaitForSeconds(Timings.DynamicIconWait);
            }
        }

    }
}
