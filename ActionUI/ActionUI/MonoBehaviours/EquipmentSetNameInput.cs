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
    public class EquipmentSetNameInput : MonoBehaviour
    {
        public InputField NameInput;
        public Button OkButton;
        public Text Caption;
        public Text DisplayText;

        public EquipmentSetView ParentEquipmentSet;
        private PlayerActionMenus _playerMenu => ParentEquipmentSet.PlayerMenu;

        public bool IsShowing => gameObject.activeSelf && _isInit;

        private bool _isInit;

        private EquipmentSetTypes _equipmentSetType;
        private bool _isRename = false;
        private string _setName;
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

        public void Show(EquipmentSetTypes equipmentSetType)
        {
            _equipmentSetType = equipmentSetType;
            _isRename = false;
            gameObject.SetActive(true);
            Caption.text = "New Equipment Set";
            NameInput.text = String.Empty;
            NameInput.Select();
            NameInput.ActivateInputField();

            OnShow?.TryInvoke();
        }

        public void Show(EquipmentSetTypes equipmentSetType, string setName)
        {
            _equipmentSetType = equipmentSetType;
            _isRename = true;
            _isOkClicked = false;
            gameObject.SetActive(true);
            Caption.text = "Rename Equipment Set";

            _setName = setName;
            NameInput.text = setName;
            NameInput.Select();
            NameInput.ActivateInputField();

            OnShow?.TryInvoke();
        }

        public void Hide() => Hide(true);

        private IEquipmentSetService<T> GetEquipmentService<T>() where T : IEquipmentSet
        {
            if (typeof(T).Equals(typeof(ArmorSet)))
                return (IEquipmentSetService<T>)_playerMenu.ProfileManager.ArmorSetService;
            else if (typeof(T).Equals(typeof(WeaponSet)))
                return (IEquipmentSetService<T>)_playerMenu.ProfileManager.WeaponSetService;

            throw new ArgumentOutOfRangeException(nameof(T));
        }

        private void EndInput()
        {
            if (!_isRename)
            {
                if (_equipmentSetType == EquipmentSetTypes.Armor)
                    CreateEquipmentSet<ArmorSet>();
                else
                    CreateEquipmentSet<WeaponSet>();
            }
            else
            {
                if (_equipmentSetType == EquipmentSetTypes.Armor)
                    RenameProfile<ArmorSet>();
                else
                    RenameProfile<WeaponSet>();
            }

        }

        private void RenameProfile<T>() where T : IEquipmentSet
        {
            if (string.IsNullOrWhiteSpace(Caption.text))
                return;


            if (_isRename && _setName.Equals(NameInput.text, StringComparison.InvariantCultureIgnoreCase))
            {
                //DebugLogger.Log($"ProfileInput::RenameProfile: activeProfile Name '{activeProfile.Name}' equals renamed profile. Exiting without changes.");
                Hide();
                return;
            }

            IEnumerable<IEquipmentSet> sets = GetEquipmentService<T>().GetEquipmentSetsProfile().EquipmentSets.Cast<IEquipmentSet>();

            if (sets.Any(s => s.Name.Equals(NameInput.text, StringComparison.InvariantCultureIgnoreCase)))
            {
                //DebugLogger.Log($"ProfileInput::RenameProfile: Renamed profile '{ProfileInputField.text}' already exists. Keeping profile window open.");
                return;
            }

            //DebugLogger.Log($"ProfileInput::RenameProfile: Attempting rename of profile '{activeProfile.Name}' to '{ProfileInputField.text}'.");
            GetEquipmentService<T>().RenameEquipmentSet(_setName, NameInput.text);

            Hide();
        }

        private void CreateEquipmentSet<T>() where T : IEquipmentSet
        {
            if (string.IsNullOrWhiteSpace(NameInput.text))
                return;

            IEnumerable<IEquipmentSet> sets = GetEquipmentService<T>().GetEquipmentSetsProfile().EquipmentSets.Cast<IEquipmentSet>();

            if (sets.Any(s => s.Name.Equals(NameInput.text, StringComparison.InvariantCultureIgnoreCase)))
            {
                //DebugLogger.Log($"ProfileInput::RenameProfile: Renamed profile '{ProfileInputField.text}' already exists. Keeping profile window open.");
                return;
            }

            GetEquipmentService<T>().CreateEmptyEquipmentSet(NameInput.text);

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