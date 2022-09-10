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
        private readonly Dictionary<BarPositions, ProgressBarService> _progressBarServices = new Dictionary<BarPositions, ProgressBarService>();
        private StackService _stackService;
        private EnableToggleService _toggleService;
        private Coroutine _iconCoroutine;

        public bool IsUpdateEnabled => ActionSlot.SlotAction?.TargetAction != null && ActionSlot.SlotAction.CheckOnUpdate;
        public bool IsActionNeeded => 
            ActionSlot.ParentCanvas != null && ActionSlot.ParentCanvas.enabled && ActionSlot.SlotAction?.TargetAction != null 
            && ActionSlot.SlotAction.GetIsActionRequested() && ActionSlot.ActionButton.interactable
            && !ActionSlot.HotbarsContainer.IsInActionSlotEditMode;

        public ActionSlotController(ActionSlot actionSlot)
        {
            ActionSlot = actionSlot ?? throw new ArgumentNullException(nameof(actionSlot));

            actionSlot.MouseClickListener.OnRightClick.AddListener(OnRemoveRequested);
            actionSlot.ActionButton.onClick.AddListener(OnActionButtonClicked);

            actionSlot.KeyButton.onClick.AddListener(OnHotkeyEditRequested);
        }

        public void AssignEmptyAction()
        {
            UnassignSlotAction();
            dynamicSprites = null;
            //ActionSlot.ActionImage.overrideSprite = null;
            //ActionSlot.ActionImage.sprite = null;
            ActionSlot.ActionImages.ClearImages();
            DisableCooldownService();
            DisableStackService();
            ActionSlot.StackText.enabled = false;

            foreach (var bar in ActionSlot.ProgressBars.Values)
                bar.gameObject.SetActive(false);

            if (ActionSlot.Config.EmptySlotOption == EmptySlotOptions.Transparent)
            {
                var color = Color.grey;
                color.a = .05f;
                //ActionSlot.ActionImage.color = color;
                //ActionSlot.ActionImage.enabled = true;
                ActionSlot.EmptyImage.gameObject.SetActive(false);
                var emptyImage = ActionSlot.ActionImages.AddOrUpdateImage(new ActionSlotIcon() { Name = "emptyTransparentAction", Icon = null });
                emptyImage.color = color;
                ActionSlot.CanvasGroup.alpha = 1;
            }
            else if (ActionSlot.Config.EmptySlotOption == EmptySlotOptions.Image)
            {
                //ActionSlot.ActionImage.enabled = false;
                ActionSlot.EmptyImage.gameObject.SetActive(true);
                ActionSlot.CanvasGroup.alpha = 1;
            }
            else if (ActionSlot.Config.EmptySlotOption == EmptySlotOptions.Hidden)
            {
                //ActionSlot.ActionImage.enabled = false;
                ActionSlot.EmptyImage.gameObject.SetActive(false);
                ActionSlot.CanvasGroup.alpha = 0;
            }

            //ActionSlot.HotbarsContainer.HasChanges = true;

        }

        public void AssignSlotAction(ISlotAction slotAction)
        {
            if (slotAction == null)
                throw new ArgumentNullException(nameof(slotAction));

            UnassignSlotAction();

            EnableActiveSlot();

            ActionSlot.SlotAction = slotAction;
            ActionSlot.SlotAction.OnActionRequested += OnActionRequested;
            ActionSlot.SlotAction.OnEditRequested += OnEditRequested;
            if (!slotAction.HasDynamicIcon)
                ActionSlot.SlotAction.OnIconsChanged += AssignSlotIcons;

            if (!slotAction.HasDynamicIcon)
                AssignSlotIcons(slotAction.ActionIcons);
            else
                AssignDynamicIcon(slotAction.GetDynamicIcons);
            
            ActionSlot.EmptyImage.gameObject.SetActive(false);

            if (slotAction.Cooldown != null && slotAction.Cooldown.HasCooldown)
                EnableCooldownService(slotAction.Cooldown);
            else
                DisableCooldownService();

            if (slotAction.Stack != null && slotAction.Stack.IsStackable)
                EnableStackService(slotAction.Stack.GetAmount);
            else
                DisableStackService();

            foreach (var kvp in ActionSlot.ProgressBars)
            {
                if (slotAction.ActiveBars != null && slotAction.ActiveBars.TryGetValue(kvp.Key, out var bar) && bar.IsEnabled)
                    EnableBarService(bar);
                else
                    DisableBarService(kvp.Key);
            }
     
            StartEnableToggleService(slotAction.GetEnabled);

            slotAction.SlotActionAssigned(ActionSlot);

            ActionSlot.HotbarsContainer.HasChanges = true;
            //ActionSlot.HotbarsContainer.HasChanges = true;
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
                SetHotkeyText(ActionSlot.Config.HotkeyText);
            }
            else
                SetHotkeyText(string.Empty);

            ToggleHotkeyEditMode(false);
        }

        public void ActionSlotUpdate()
        {
            if (!IsUpdateEnabled)
                return;

            if (IsActionNeeded)
            {
                //ExecuteEvents.Execute(ActionSlot.ActionButton.gameObject, new PointerEventData(EventSystem.current), ExecuteEvents.submitHandler);
                ActionSlot.SlotAction.TargetAction.Invoke();
            }
            else if (ActionSlot.SlotAction.GetIsEditRequested())
            {
                ActionSlot.HotbarsContainer.ActionsViewer.Show(ActionSlot.SlotId);
            }
        }

        private void OnActionButtonClicked()
        {
            if (ActionSlot.HotbarsContainer.IsInActionSlotEditMode)
            {
                OnEditRequested();
            }
            else if (ActionSlot.SlotAction != null)
            {
                OnActionRequested();
            }
        }

        public void Configure(IActionSlotConfig config)
        {
            ActionSlot.Config = config ?? throw new ArgumentNullException(nameof(config));

            Refresh();
            _cooldownService?.Configure(ActionSlot.Config.ShowCooldownTime, ActionSlot.Config.PreciseCooldownTime);
            _stackService?.Configure(ActionSlot.Config.ShowZeroStackAmount);
            SetHotkeyText(ActionSlot.Config.HotkeyText);
            //ActionSlot.HotbarsContainer.HasChanges = true;
        }

        public void HideCooldown()
        {
            ActionSlot.CooldownImage.fillAmount = 0f;
            if (ActionSlot.CooldownText.gameObject.activeSelf)
            {
                ActionSlot.CooldownText.text = string.Empty;
                var bgColor = ActionSlot.CooldownTextBackground.color;
                bgColor.a = 0f;
                ActionSlot.CooldownTextBackground.color = bgColor;
            }
        }

        public void HideStackAmount() => ActionSlot.StackText.text = string.Empty;

        public void ShowSlider(BarPositions barPosition)
        {
            if (!ActionSlot.ProgressBars[barPosition].gameObject.activeSelf)
                ActionSlot.ProgressBars[barPosition].gameObject.SetActive(true);

        }

        public void HideSlider(BarPositions barPosition)
        {
            if (ActionSlot.ProgressBars[barPosition].gameObject.activeSelf)
                ActionSlot.ProgressBars[barPosition].gameObject.SetActive(false);
        }

        public void ToggleInteractive(bool interactive)
        {
            if (interactive && !ActionSlot.ActionButton.interactable)
            {
                ActionSlot.ActionButton.interactable = true;
            }
            else if (!interactive && ActionSlot.ActionButton.interactable)
            {
                ActionSlot.ActionButton.interactable = false;
            }
        }
        
        public void ToggleEnabled(bool enabled)
        {
            ActionSlot.ActionImages.ToggleEnabled(enabled);
            //if (enabled && ActionSlot.ActionButton.colors.normalColor != Color.white)
            //    ActionSlot.SetButtonNormalColor(Color.white);
            //else if (!enabled && ActionSlot.ActionButton.colors.normalColor != ActionSlot.DisabledColor)
            //    ActionSlot.SetButtonNormalColor(ActionSlot.DisabledColor);
        }
        
        public void ToggleHotkeyEditMode(bool toggle)
        {
            ActionSlot.KeyButton.gameObject.SetActive(toggle);
            if (toggle && ActionSlot.CanvasGroup.alpha == 0f)
                ActionSlot.CanvasGroup.alpha = 1;
            else if (!toggle && ActionSlot.Config.EmptySlotOption == EmptySlotOptions.Hidden && ActionSlot.SlotAction == null)
                ActionSlot.CanvasGroup.alpha = 0;
        }

        private void OnActionRequested()
        {
            if (ActionSlot.ParentCanvas != null && ActionSlot.ParentCanvas.enabled && ActionSlot.SlotAction?.TargetAction != null && ActionSlot.ActionButton.interactable)
                ActionSlot.SlotAction.TargetAction.Invoke();
        }

        private void OnEditRequested()
        {
            if (ActionSlot.ParentCanvas != null && ActionSlot.ParentCanvas.enabled && ActionSlot.ActionButton.interactable)
                ActionSlot.HotbarsContainer.ActionsViewer.Show(ActionSlot.SlotId);
        }

        private void OnRemoveRequested()
        {
            Debug.Log($"[Debug  :ActionMenus] ActionSlotController:OnRemoveRequested");
            if (ActionSlot.ParentCanvas != null && ActionSlot.ParentCanvas.enabled && ActionSlot.ActionButton.interactable)
            {
                AssignEmptyAction();
                ActionSlot.HotbarsContainer.HasChanges = true;
            }
        }

        private void OnHotkeyEditRequested()
        {
            if (ActionSlot.ParentCanvas != null && ActionSlot.ParentCanvas.enabled && ActionSlot.ActionButton.interactable)
                ActionSlot.HotkeyCapture.ShowDialog(ActionSlot.SlotIndex, HotkeyCategories.ActionSlot);
        }

        private void EnableActiveSlot()
        {
            ActionSlot.CooldownImage.enabled = true;
            ActionSlot.CooldownText.enabled = true;
            ActionSlot.StackText.enabled = true;
            //ActionSlot.ActionImage.enabled = true;
            ActionSlot.CanvasGroup.alpha = 1;

            ActionSlot.CooldownText.text = String.Empty;
            ActionSlot.StackText.text = String.Empty;
        }

        private void UnassignSlotAction()
        {
            if (ActionSlot.SlotAction != null)
            {
                ActionSlot.SlotAction.OnActionRequested -= OnActionRequested;
                ActionSlot.SlotAction.OnEditRequested -= OnEditRequested;
                
                if (!ActionSlot.SlotAction.HasDynamicIcon)
                    ActionSlot.SlotAction.OnIconsChanged -= AssignSlotIcons;
                
                ActionSlot.SlotAction.SlotActionUnassigned();
                ActionSlot.SlotAction = null;
            }
            if (_iconCoroutine != null)
            {
                ActionSlot.StopCoroutine(_iconCoroutine);
                _iconCoroutine = null;
            }
            ActionSlot.CooldownText.text = String.Empty;
            ActionSlot.StackText.text = String.Empty;
        }

        private ActionSlotIcon[] dynamicSprites;
        private void AssignSlotIcons(ActionSlotIcon[] spriteIcons)
        {
            string logMsg = string.Empty;
            try
            {
                bool spritesChanged = false;
                if (dynamicSprites != null && dynamicSprites.Length == spriteIcons.Length)
                {
                    for (int i = 0; i < spriteIcons.Length; i++)
                    {
                        if (spriteIcons[i].Name != dynamicSprites[i].Name)
                        {
                            spritesChanged = true;
                            break;
                        }
                    }
                }
                else
                    spritesChanged = true;

                dynamicSprites = spriteIcons;

                if (spritesChanged)
                {
                    ActionSlot.ActionImages.ClearImages();
                    for (int i = 0; i < spriteIcons.Length; i++)
                    {
                        logMsg = $"AssignSlotIcon: ActionSlot.ActionImages == null == {ActionSlot.ActionImages == null}.  spriteIcon[{i}] == null == {spriteIcons[i] == null}";
                        ActionSlot.ActionImages.AddOrUpdateImage(spriteIcons[i]);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Could not assign sprite icons to ActionSlot {ActionSlot?.name}. {logMsg}");
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

            ActionSlot.CooldownImage.gameObject.SetActive(true);

            if (ActionSlot.Config.ShowCooldownTime)
            {
                ActionSlot.CooldownText.gameObject.SetActive(true);
                ActionSlot.CooldownTextBackground.gameObject.SetActive(true);
                var bgColor = ActionSlot.CooldownTextBackground.color;
                bgColor.a = .0f;
                ActionSlot.CooldownTextBackground.color = bgColor;
            }

            _cooldownService = new CooldownService(ActionSlot.CooldownImage, ActionSlot.CooldownText, ActionSlot.Config.ShowCooldownTime, ActionSlot.Config.PreciseCooldownTime, this);
            _cooldownService.TrackCooldown(cooldown);
        }

        private void DisableCooldownService()
        {
            _cooldownService?.StopTracking();
            ActionSlot.CooldownImage.gameObject.SetActive(false);
            ActionSlot.CooldownText.gameObject.SetActive(false);
            ActionSlot.CooldownTextBackground.gameObject.SetActive(false);
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

        private void EnableBarService(IBarProgress bar)
        {
            if (bar == null)
                throw new ArgumentNullException(nameof(bar));

            if (_progressBarServices.TryGetValue(bar.BarPosition, out var barService))
            {
                barService.StopTracking();
                barService = null;
                _progressBarServices.Remove(bar.BarPosition);
            }

            _progressBarServices.Add(bar.BarPosition, new ProgressBarService(ActionSlot.ProgressBars[bar.BarPosition], bar.BarPosition, this));
            _progressBarServices[bar.BarPosition].TrackSlider(bar);
        }

        private void DisableBarService(BarPositions barPosition)
        {
            if (_progressBarServices.TryGetValue(barPosition, out var barService))
            {
                barService.StopTracking();
                barService = null;
                _progressBarServices.Remove(barPosition);
            }
            HideSlider(barPosition);
        }

        private void SetHotkeyText(string text)
        {
            ActionSlot.KeyText.text = text;
            if (!string.IsNullOrEmpty(text))
                ActionSlot.HotkeyPanel.enabled = true;
            else
                ActionSlot.HotkeyPanel.enabled = false;
        }

        private void StartEnableToggleService(Func<bool> getEnabled)
        {
            if (getEnabled == null)
                throw new ArgumentNullException(nameof(getEnabled));

            if (_toggleService == null)
                _toggleService = new EnableToggleService(this);

            _toggleService.TrackEnableToggle(getEnabled);
        }

        private void AssignDynamicIcon(Func<ActionSlotIcon[]> getIcon)
        {
            _iconCoroutine = ActionSlot.StartCoroutine(AssignIconCoroutine(getIcon));
        }
        private IEnumerator AssignIconCoroutine(Func<ActionSlotIcon[]> getIcons)
        {
            while (true)
            {
                AssignSlotIcons(getIcons.Invoke());
                yield return new WaitForSeconds(Timings.DynamicIconWait);
            }
        }

    }
}
