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
    public class NewProfileInput : MonoBehaviour, ISettingsView
    {
        public InputField ProfileInputField;

        public Button OkButton;
        public MainSettingsMenu MainSettingsMenu;

        private IActionMenusProfileService _profileService => MainSettingsMenu.PlayerMenu.ProfileManager.ProfileService;

        public bool IsShowing => gameObject.activeSelf && _isInit;

        private bool _isInit;

        public UnityEvent OnShow { get; } = new UnityEvent();

        public UnityEvent OnHide { get; } = new UnityEvent();


        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "<Pending>")]
        private void Awake()
        {
            Debug.Log("NewProfileInput::Awake");
            OkButton.onClick.AddListener(() => CreateProfile());
            ProfileInputField.onEndEdit.AddListener((profileName) => CreateProfile());
            ProfileInputField.onValidateInput += (string input, int charIndex, char addedChar) => ValidateEntry(addedChar);
            Hide(false);
            _isInit = true;
        }

        public void Show()
        {
            Debug.Log("NewProfileInput::Show");
            gameObject.SetActive(true);
            ProfileInputField.text = String.Empty;
            ProfileInputField.Select();
            ProfileInputField.ActivateInputField();

            //HookControls();
            OnShow?.TryInvoke();
        }

        public void Hide() => Hide(true);

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
            Debug.Log("NewProfileInput::Hide");
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