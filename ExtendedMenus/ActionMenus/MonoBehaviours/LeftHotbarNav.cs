using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ModifAmorphic.Outward.Unity.ActionMenus
{
    [UnityScriptComponent]
    public class LeftHotbarNav : MonoBehaviour
    {
        public PlayerMenu PlayerMenu;
        public HotkeyCaptureDialog HotkeyCaptureDialog;
        public HotbarSettingsViewer HotbarSettingsViewer;
        public HotbarsContainer HotbarsContainer;

        private Button _settingsButton;
        private Button _nextButton;
        private Button _nextHotkeyButton;
        private Button _previousButton;
        private Button _previousHotkeyButton;
        private Button _hotkeyButton;
        private Text _barText;
        private Text _nextHotkeyText;
        private Text _previousHotkeyText;
        private Text _hotkeyText;


        private IHotbarNavActions GetHotbarNavActions()
        {
            var psp = Psp.Instance?.GetServicesProvider(PlayerMenu.PlayerID);
            if (psp != null && psp.TryGetService<IHotbarNavActions>(out var navActions))
                return navActions;
            
            return null;
        }

        private bool _inEditMode;
        public bool InEditMode => _inEditMode;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "<Pending>")]
        private void Awake()
        {
            SetComponents();

            SetBarText(string.Empty);
            SetNextHotkeyText(string.Empty);
            SetPreviousHotkeyText(string.Empty);
            SetHotkeyText(string.Empty);

            HookButtonEvents();

            ToggleEditMode(true);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "<Pending>")]
        private void Update()
        {
            var navActions = GetHotbarNavActions();
            if (navActions != null)
            {
                if (navActions.IsNextRequested())
                    HotbarsContainer.Controller.SelectNext();
                else if (navActions.IsPreviousRequested())
                    HotbarsContainer.Controller.SelectPrevious();
                else if (navActions.IsHotbarRequested(out int hotbarIndex))
                {
                    HotbarsContainer.Controller.SelectHotbar(hotbarIndex);
                }
            }
        }

        private void SetComponents()
        {
            _settingsButton = GetComponentsInChildren<Button>().First(b => b.name.Equals("Settings"));

            var nextHotbar = transform.Find("BarNumber/NextHotbar").gameObject;
            _nextButton = nextHotbar.GetComponentsInChildren<Button>().First(b => b.name.Equals("NextButton"));
            _nextHotkeyButton = nextHotbar.GetComponentsInChildren<Button>().First(b => b.name.Equals("HotkeyButton"));
            _nextHotkeyText = nextHotbar.GetComponentInChildren<Text>();

            var previousHotbar = transform.Find("BarNumber/PreviousHotbar").gameObject;
            _previousButton = previousHotbar.GetComponentsInChildren<Button>().First(b => b.name.Equals("PreviousButton"));
            _previousHotkeyButton = previousHotbar.GetComponentsInChildren<Button>().First(b => b.name.Equals("HotkeyButton"));
            _previousHotkeyText = previousHotbar.GetComponentInChildren<Text>();

            var barHotkey = transform.Find("BarNumber/BarHotkey").gameObject;
            _hotkeyButton = barHotkey.GetComponentsInChildren<Button>().First(b => b.name.Equals("HotkeyButton"));
            _hotkeyText = barHotkey.GetComponentInChildren<Text>();

            var barIcon = transform.Find("BarNumber/BarIcon").gameObject;
            _barText = previousHotbar.GetComponentInChildren<Text>();
        }

        public void SetBarText(string text) => _barText.text = text;
        public void SetNextHotkeyText(string text) => _nextHotkeyText.text = text;
        public void SetPreviousHotkeyText(string text) => _previousHotkeyText.text = text;
        public void SetHotkeyText(string text) => _hotkeyText.text = text;

        public void ToggleEditMode(bool enableEdits)
        {
            _inEditMode = enableEdits;
            if (!enableEdits)
            {
                _settingsButton.gameObject.SetActive(false);
                _nextButton.gameObject.SetActive(false);
                _nextHotkeyButton.gameObject.SetActive(false);
                _previousButton.gameObject.SetActive(false);
                _previousHotkeyButton.gameObject.SetActive(false);
                _hotkeyButton.gameObject.SetActive(false);
            }
            else
            {
                _settingsButton.gameObject.SetActive(true);
                _nextButton.gameObject.SetActive(true);
                _nextHotkeyButton.gameObject.SetActive(true);
                _previousButton.gameObject.SetActive(true);
                _previousHotkeyButton.gameObject.SetActive(true);
                _hotkeyButton.gameObject.SetActive(true);
            }
        }

        private void HookButtonEvents()
        {
            _settingsButton.onClick.AddListener(() => HotbarSettingsViewer.Show());

            _nextButton.onClick.AddListener(() => HotbarsContainer.Controller.SelectNext());
            _previousButton.onClick.AddListener(() => HotbarsContainer.Controller.SelectPrevious());

            _nextHotkeyButton.onClick.AddListener(() => HotkeyCaptureDialog.Show(0, Models.HotkeyCategories.Hotbar));
            _previousHotkeyButton.onClick.AddListener(() => HotkeyCaptureDialog.Show(1, Models.HotkeyCategories.Hotbar));
            _hotkeyButton.onClick.AddListener(() => HotkeyCaptureDialog.Show(2, Models.HotkeyCategories.Hotbar));
        }
    }
}