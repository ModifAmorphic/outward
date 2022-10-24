using ModifAmorphic.Outward.Unity.ActionUI;
using ModifAmorphic.Outward.Unity.ActionUI.Data;
using ModifAmorphic.Outward.Unity.ActionUI.EquipmentSets;
using ModifAmorphic.Outward.Unity.ActionUI.Extensions;
using ModifAmorphic.Outward.Unity.ActionUI.Models.EquipmentSets;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using static UnityEngine.UI.Dropdown;

namespace ModifAmorphic.Outward.Unity.ActionMenus
{

    [UnityScriptComponent]
    public class SkillChainMenu : MonoBehaviour, IActionMenu
    {
        public RectTransform ParentTransform;
        public Button ToggleChainsButton;

        public PlayerActionMenus PlayerMenus;

        public Dropdown SkillChainsDropdown;
        public Button NewChainButton;
        public Button RenameChainButton;
        public Button SaveChainButton;
        public Button DeleteChainButton;

        public ActionItemView ActionPrefab;
        public GridLayoutGroup ActionsGrid;

        public SkillChainNameInput SetNamePanel;
        public ConfirmationPanel ConfirmationPanel;


        public bool IsShowing => gameObject.activeSelf;

        public UnityEvent OnShow { get; private set; } = new UnityEvent();

        public UnityEvent OnHide { get; private set; } = new UnityEvent();

        private List<ActionItemView> _actions = new List<ActionItemView>();


        private bool _init = false;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "<Pending>")]
        private void Awake()
        {
            SkillChainsDropdown.ClearOptions();
            SkillChainsDropdown.onValueChanged.AddListener(LoadSkillChain);
            NewChainButton.onClick.AddListener(PromptNewChain);
            RenameChainButton.onClick.AddListener(PromptRenameChain);
            SaveChainButton.onClick.AddListener(SaveSkillChain);
            DeleteChainButton.onClick.AddListener(ConfirmDeleteSkillChain);

            //_actions.Add(Action0);

            RenameChainButton.interactable = false;
            SaveChainButton.interactable = false;
            DeleteChainButton.interactable = false;

            ActionPrefab.gameObject.SetActive(false);

            ToggleChainsButton.gameObject.SetActive(false);
            Hide();
        }

        private ISkillChainService GetSkillChainService() => PlayerMenus.ProfileManager.SkillChainService;

        public void Show()
        {
            gameObject.SetActive(true);
            if (!_init)
            {
                RefreshSkillChainsDropdown();
                if (SkillChainsDropdown.options.Count() > 0)
                    LoadSkillChain(0);

                GetSkillChainService().OnNewChain += (chain) => ChainCreated(chain.Name);
                GetSkillChainService().OnRenamedChain += ChainRenamed;
                _init = true;
            }
        }

        public void Hide() => Hide(false);

        public void Hide(bool forceHide)
        {
            if (SetNamePanel.IsShowing)
            {
                SetNamePanel.Hide();
                if (!forceHide)
                    return;
            }
            gameObject.SetActive(false);
        }

        public void ToggleMenu()
        {
            if (!IsShowing)
                Show();
            else
                Hide(true);
        }

        public void ChainCreated(string chainName)
        {
            if (!IsShowing)
                return;

            RefreshSkillChainsDropdown();
            ResetActions();
            SkillChainsDropdown.SelectOption(chainName, false);
        }

        private void ChainRenamed(SkillChain skillChain, string oldName, string newName)
        {
            if (!IsShowing)
                return;

            RefreshSkillChainsDropdown();
            SkillChainsDropdown.SelectOption(newName, false);
        }

        private void ResetActions()
        {
            for (int i = _actions.Count - 1; i >= 0; i--)
            {
                if (_actions[i] != null)
                {
                    _actions[i].OnSlotActionReset.RemoveAllListeners();
                    _actions[i].OnSlotActionSet.RemoveAllListeners();
                    UnityEngine.Object.Destroy(_actions[i].gameObject);
                }
            }
            _actions.Clear();
            AddEmptyAction();
        }

        private void AddEmptyAction()
        {
            var index = _actions.Count;
            var action = UnityEngine.GameObject.Instantiate(ActionPrefab.gameObject, ActionsGrid.transform);
            action.name = "Action_" + index;
            action.SetActive(true);
            var actionView = action.GetComponent<ActionItemView>();
            _actions.Add(actionView);
            actionView.SetViewID(index);
        }

        private void AddAction(ISlotAction slotAction, int index)
        {
            //If last slot, add a new one up to 6
            if (_actions.Count() == 0 || (index == _actions.Count() && _actions.Count() <= 6))
            {
                var action = UnityEngine.GameObject.Instantiate(ActionPrefab.gameObject, ActionsGrid.transform);
                action.name = "Action_" + index;
                action.SetActive(true);
                var actionView = action.GetComponent<ActionItemView>();
                _actions.Add(actionView);
                actionView.SetViewID(index);
            }

            _actions[index].SetViewItem(slotAction);
        }

        private void AddRemoveEmptySlot(int actionIndexSet)
        {
            DebugLogger.Log($"AddRemoveEmptySlot for index {actionIndexSet}. _actions.Any(a => a.SlotAction == null) == {_actions.Any(a => a.SlotAction == null)}, _actions.Count == {_actions.Count}");
            if (!_actions.Any(a => a.SlotAction == null) && _actions.Count <= 6)
            {
                AddEmptyAction();
                _actions.Last().OnSlotActionSet.AddListener(AddRemoveEmptySlot);
                _actions.Last().OnSlotActionReset.AddListener(AddRemoveEmptySlot);
            }
            else if (_actions.Count > 1)
            {
                int lastIndex = _actions.Count - 1;
                int secondToLastIndex = lastIndex - 1;
                if (_actions[lastIndex].SlotAction == null && _actions[secondToLastIndex].SlotAction == null)
                {
                    _actions[lastIndex].OnSlotActionReset.RemoveAllListeners();
                    _actions[lastIndex].OnSlotActionSet.RemoveAllListeners();
                    UnityEngine.Object.Destroy(_actions[lastIndex].gameObject);
                    _actions.RemoveAt(lastIndex);
                }
            }
        }

        private void SetSkillChainActions()
        {
            var actions = GetSkillChainService().GetSlotActions(SkillChainsDropdown.GetSelectedOption().text);
            for (int i = 0; i < actions.Count; i++)
            {
                AddAction(actions[i], i);
            }
            if (actions.Count <= 6)
                AddEmptyAction();

            for (int i = 0; i < _actions.Count; i++)
            {
                _actions[i].OnSlotActionSet.AddListener(AddRemoveEmptySlot);
                _actions[i].OnSlotActionReset.AddListener(AddRemoveEmptySlot);
            }
        }
        private void LoadSkillChain(int index)
        {
            ResetActions();

            SetSkillChainActions();

            RenameChainButton.interactable = true;
            SaveChainButton.interactable = true;
            DeleteChainButton.interactable = true;
        }

        private void RefreshSkillChainsDropdown()
        {
            DebugLogger.Log($"Refreshing Skill Chains Dropdown.");
            var chainsProfile = GetSkillChainService().GetSkillChainProfile();

            SkillChainsDropdown.ClearOptions();
            var options = new List<OptionData>();
            options.AddRange(chainsProfile.SkillChains.OrderBy(e => e.Name).Select(e => new OptionData(e.Name)));
            SkillChainsDropdown.AddOptions(options);
        }


        private void PromptRenameChain()
        {
            if (SkillChainsDropdown.options.Any())
                SetNamePanel.Show(SkillChainsDropdown.GetSelectedOption().text);
        }

        private void PromptNewChain() => SetNamePanel.Show();

        private void SaveSkillChain()
        {
            if (!SkillChainsDropdown.options.Any())
                return;

            var chainService = GetSkillChainService();
            var chain = chainService.GetSkillChain(SkillChainsDropdown.GetSelectedOption().text);
            chain.ActionChain.Clear();
            for (int i = 0; i < _actions.Count; i++)
            {
                if (_actions[i]?.SlotAction != null)
                    chain.ActionChain.Add(i, chainService.ConvertToChainAction(_actions[i].SlotAction));
            }

            if (string.IsNullOrEmpty(chain.StatusEffectIcon) && chain.ActionChain.Any())
                chain.IconItemID = chain.ActionChain.First().Value.ItemID;
            
            chainService.SaveSkillChain(chain);
        }
       
        
        private void ConfirmDeleteSkillChain()
        {
            if (!SkillChainsDropdown.options.Any())
                return;

            var chainName = SkillChainsDropdown.GetSelectedOption().text;

            var chain = GetSkillChainService().GetSkillChain(chainName);
            if (chain == null)
            {
                RefreshSkillChainsDropdown();
                return;
            }

            var deleteAction = new Action(() =>
            {
                GetSkillChainService().DeleteSkillChain(chain.ItemID);
                RefreshSkillChainsDropdown();
            });
            DebugLogger.Log($"Showing Delete Confirmation for skill chain \"{chainName}\".");
            ConfirmationPanel.Show(deleteAction, $"Delete Skill Chain \"{chainName}\"?");
        }
    }
}