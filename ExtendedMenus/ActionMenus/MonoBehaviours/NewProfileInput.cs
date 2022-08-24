using ModifAmorphic.Outward.Unity.ActionMenus.Data;
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
    public class NewProfileInput : MonoBehaviour, IActionMenu
    {
        public InputField ProfileInputField;

        public Button OkButton;
        public HotbarSettingsViewer HotbarSettingsViewer;

        private IHotbarProfileDataService _profileService;

        public bool IsShowing => gameObject.activeSelf;

        public UnityEvent OnShow { get; } = new UnityEvent();

        public UnityEvent OnHide { get; } = new UnityEvent();


        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "<Pending>")]
        private void Awake()
        {
            //Hide();
            OkButton.onClick.AddListener(() => CreateProfile());
            ProfileInputField.onEndEdit.AddListener((profileName) => CreateProfile());
            ProfileInputField.onValidateInput += (string input, int charIndex, char addedChar) => ValidateEntry(addedChar);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "<Pending>")]
        private void Update()
        {
            
        }

        public void Show(IHotbarProfileDataService profileService)
        {
            Debug.Log("NewProfileInput::Show");
            _profileService = profileService;
            gameObject.SetActive(true);
            ProfileInputField.text = String.Empty;
            ProfileInputField.Select();
            ProfileInputField.ActivateInputField();

            //HookControls();
            OnShow?.Invoke();
        }
        public void Hide()
        {
            Debug.Log("HotbarSettingsViewer::Hide");
            gameObject.SetActive(false);
            OnHide?.Invoke();
        }
        public void CreateProfile()
        {
            if (string.IsNullOrWhiteSpace(ProfileInputField.text))
                return;

            var activeProfile = _profileService.GetActiveProfile();
            activeProfile.Name = ProfileInputField.text;
            _profileService.SaveProfile(activeProfile);
            _profileService.SetActiveProfile(activeProfile.Name);
            Hide();
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