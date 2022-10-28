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

namespace ModifAmorphic.Outward.Unity.ActionMenus
{
    [UnityScriptComponent]
    public class SkillChainNameInput : MonoBehaviour
    {
        public InputField NameInput;
        public Button OkButton;
        public Text Caption;
        public Text DisplayText;

        public SkillChainMenu SkillChainMenu;
        private PlayerActionMenus _playerMenus => SkillChainMenu.PlayerMenus;

        public bool IsShowing => gameObject.activeSelf && _isInit;

        private bool _isInit;

        private bool _isRename = false;
        private string _chainName;
        private bool _isOkClicked;

        public UnityEvent OnShow { get; } = new UnityEvent();

        public UnityEvent OnHide { get; } = new UnityEvent();


        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "<Pending>")]
        private void Awake()
        {
            OkButton.onClick.AddListener(() =>
            {
                _isOkClicked = true;
                EndInput();
                _isOkClicked = false;
            });
            NameInput.onEndEdit.AddListener((profileName) =>
            {
                //Checks if Ok button was already pressed and handled.
                if (!_isOkClicked)
                    EndInput();
            });
            NameInput.onValidateInput += (string input, int charIndex, char addedChar) => ValidateEntry(addedChar);
            Hide(false);
            _isInit = true;
        }

        public void Show()
        {
            _isRename = false;
            gameObject.SetActive(true);
            Caption.text = "New Skill Chain";
            NameInput.text = String.Empty;
            NameInput.Select();
            NameInput.ActivateInputField();

            OnShow?.TryInvoke();
        }

        public void Show(string chainName)
        {
            _isRename = true;
            _isOkClicked = false;
            gameObject.SetActive(true);
            Caption.text = "Rename Skill Chain";

            _chainName = chainName;
            NameInput.text = chainName;
            NameInput.Select();
            NameInput.ActivateInputField();

            OnShow?.TryInvoke();
        }

        public void Hide() => Hide(true);

        private ISkillChainService GetSkillChainService() => _playerMenus.ProfileManager.SkillChainService;

        private void EndInput()
        {
            if (!_isRename)
            {
                CreateSkillChain();
            }
            else
            {
                RenameSkillChain();
            }

        }

        private void RenameSkillChain()
        {
            if (string.IsNullOrWhiteSpace(Caption.text))
                return;


            if (_isRename && _chainName.Equals(NameInput.text, StringComparison.InvariantCultureIgnoreCase))
            {
                Hide();
                return;
            }

            var skillChains = GetSkillChainService().GetSkillChainProfile().SkillChains;

            if (skillChains.Any(s => s.Name.Equals(NameInput.text, StringComparison.InvariantCultureIgnoreCase)))
            {
                return;
            }

            GetSkillChainService().RenameSkillChain(_chainName, NameInput.text);

            Hide();
        }

        private void CreateSkillChain()
        {
            if (string.IsNullOrWhiteSpace(NameInput.text))
                return;

            var skillChains = GetSkillChainService().GetSkillChainProfile().SkillChains;

            if (skillChains.Any(s => s.Name.Equals(NameInput.text, StringComparison.InvariantCultureIgnoreCase)))
            {
                return;
            }

            GetSkillChainService().CreateSkillChain(NameInput.text);

            Hide();
        }

        private void Hide(bool raiseEvent)
        {
            //DebugLogger.Log("ProfileInput::Hide");

            gameObject.SetActive(false);
            if (raiseEvent)
            {
                OnHide?.TryInvoke();
            }
        }

        private static readonly List<char> _validChars = new List<char>()
        {
            ' ', '.', '-', '_'
        };
        private char ValidateEntry(char chr)
        {
            if (!char.IsLetterOrDigit(chr) && !_validChars.Contains(chr) && chr != '\0')
            {
                return '\0';
            }
            return chr;
        }
    }
}