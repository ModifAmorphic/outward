using ModifAmorphic.Outward.Unity.ActionMenus.Data;
using ModifAmorphic.Outward.Unity.ActionMenus.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ModifAmorphic.Outward.Unity.ActionMenus
{
    [UnityScriptComponent]
    public class ProfileInput : MonoBehaviour, ISettingsView
    {
        public InputField ProfileInputField;
        public Button OkButton;
        public Text Caption;

        public MainSettingsMenu MainSettingsMenu;

        private IActionMenusProfileService _profileService => MainSettingsMenu.PlayerMenu.ProfileManager.ProfileService;

        public bool IsShowing => gameObject.activeSelf && _isInit;

        private bool _isInit;

        private bool _isRename = false;
        private string _profileName;

        public UnityEvent OnShow { get; } = new UnityEvent();

        public UnityEvent OnHide { get; } = new UnityEvent();


        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "<Pending>")]
        private void Awake()
        {
            OkButton.onClick.AddListener(EndInput);
            ProfileInputField.onEndEdit.AddListener((profileName) => EndInput());
            ProfileInputField.onValidateInput += (string input, int charIndex, char addedChar) => ValidateEntry(addedChar);
            Hide(false);
            _isInit = true;
        }

        public void Show()
        {
            _isRename = false;
            gameObject.SetActive(true);
            Caption.text = "New Profile";
            ProfileInputField.text = String.Empty;
            ProfileInputField.Select();
            ProfileInputField.ActivateInputField();

            OnShow?.TryInvoke();
        }

        public void Show(string profileName)
        {
            _isRename = true;
            gameObject.SetActive(true);
            Caption.text = "Rename Profile";

            _profileName = profileName;
            ProfileInputField.text = profileName;
            ProfileInputField.Select();
            ProfileInputField.ActivateInputField();

            OnShow?.TryInvoke();
        }

        public void Hide() => Hide(true);

        private void EndInput()
        {
            if (!_isRename)
            {
                Debug.Log("ProfileInput: Calling CreateProfile()");
                CreateProfile();
            }
            else
            {
                Debug.Log("ProfileInput: Calling RenameProfile()");
                RenameProfile();
            }
        }

        public void RenameProfile()
        {
            if (string.IsNullOrWhiteSpace(ProfileInputField.text))
                return;


            var activeProfile = _profileService.GetActiveProfile();
            Debug.Log($"ProfileInput::RenameProfile: _profileName=='{_profileName}', activeProfile.Name == '{activeProfile.Name}'");
            if (!activeProfile.Name.Equals(_profileName, StringComparison.InvariantCultureIgnoreCase))
                _profileService.SetActiveProfile(_profileName);

            if (activeProfile.Name.Equals(ProfileInputField.text, StringComparison.InvariantCultureIgnoreCase))
            {
                Debug.Log($"ProfileInput::RenameProfile: activeProfile Name '{activeProfile.Name}' equals renamed profile. Exiting without changes.");
                Hide();
                return;
            }

            var names = _profileService.GetProfileNames();
            if (names.Contains(ProfileInputField.text))
            {
                Debug.Log($"ProfileInput::RenameProfile: Renamed profile '{ProfileInputField.text}' already exists. Keeping profile window open.");
                return;
            }

            Debug.Log($"ProfileInput::RenameProfile: Attempting rename of profile '{activeProfile.Name}' to '{ProfileInputField.text}'.");
            _profileService.Rename(ProfileInputField.text);
            Hide();
        }

        public void CreateProfile()
        {
            if (string.IsNullOrWhiteSpace(ProfileInputField.text))
                return;

            var activeProfile = _profileService.GetActiveProfile();
            activeProfile.Name = ProfileInputField.text;

            var hotbarService = MainSettingsMenu.PlayerMenu.ProfileManager.HotbarProfileService;
            var hotbarProfile = hotbarService.GetProfile();

            _profileService.SaveNew(activeProfile);
            if (hotbarProfile != null)
                hotbarService.SaveNew(hotbarProfile);

            Hide();
        }

        private void Hide(bool raiseEvent)
        {
            Debug.Log("ProfileInput::Hide");
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