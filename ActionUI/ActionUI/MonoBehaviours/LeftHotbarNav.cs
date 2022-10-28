using ModifAmorphic.Outward.Unity.ActionUI;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace ModifAmorphic.Outward.Unity.ActionMenus
{
    [UnityScriptComponent]
    public class LeftHotbarNav : MonoBehaviour
    {
        public PlayerActionMenus PlayerMenu;
        public HotbarsContainer HotbarsContainer;

        private Button _nextButton;
        private Button _nextHotkeyButton;
        private Button _previousButton;
        private Button _previousHotkeyButton;

        private Text _barText;
        private Text _nextHotkeyText;
        private Text _previousHotkeyText;


        private GameObject _barHotkeyGo;
        private Button _hotkeyButton;
        private Text _hotkeyText;

        private List<string> _hotbarHotkeys = new List<string>();

        private Canvas _leftNavCanvas;
        private bool _isHidden;

        private bool _awake = false;
        private IHotbarNavActions GetHotbarNavActions()
        {
            var psp = Psp.Instance?.GetServicesProvider(PlayerMenu.PlayerID);
            if (psp != null && psp.TryGetService<IHotbarNavActions>(out var navActions))
                return navActions;

            return null;
        }

        private bool _inEditMode;
        public bool InEditMode => _inEditMode;

        private bool _inHotkeyEditMode;
        public bool InHotkeyEditMode => _inHotkeyEditMode;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "<Pending>")]
        private void Awake()
        {
            SetComponents();

            SetBarText(string.Empty);
            SetNextHotkeyText(string.Empty);
            SetPreviousHotkeyText(string.Empty);
            SetHotkeyText(string.Empty);

            HookButtonEvents();

            _awake = true;

            ToggleActionSlotEditMode(false);
            ToggleHotkeyEditMode(false);
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
            var nextHotbar = transform.Find("BarNumber/NextHotbar").gameObject;
            _nextButton = nextHotbar.GetComponentsInChildren<Button>().First(b => b.name.Equals("NextButton"));
            _nextHotkeyButton = nextHotbar.GetComponentsInChildren<Button>().First(b => b.name.Equals("HotkeyButton"));
            _nextHotkeyText = nextHotbar.GetComponentInChildren<Text>();

            var previousHotbar = transform.Find("BarNumber/PreviousHotbar").gameObject;
            _previousButton = previousHotbar.GetComponentsInChildren<Button>().First(b => b.name.Equals("PreviousButton"));
            _previousHotkeyButton = previousHotbar.GetComponentsInChildren<Button>().First(b => b.name.Equals("HotkeyButton"));
            _previousHotkeyText = previousHotbar.GetComponentInChildren<Text>();

            _barHotkeyGo = transform.Find("BarNumber/BarHotkey").gameObject;
            _hotkeyButton = _barHotkeyGo.GetComponentsInChildren<Button>().First(b => b.name.Equals("HotkeyButton"));
            _hotkeyText = _barHotkeyGo.GetComponentInChildren<Text>();

            var barIcon = transform.Find("BarNumber/BarIcon").gameObject;
            _barText = barIcon.GetComponentInChildren<Text>();

            _leftNavCanvas = GetComponent<Canvas>();
        }

        public void Show()
        {
            _isHidden = false;
            _leftNavCanvas.enabled = true;
        }
        public void Hide()
        {
            _isHidden = true;
            _leftNavCanvas.enabled = false;
        }

        public void SetBarText(string text) => _barText.text = text;
        public void SetNextHotkeyText(string text) => _nextHotkeyText.text = text;
        public void SetPreviousHotkeyText(string text) => _previousHotkeyText.text = text;
        public void SetHotkeyText(string text)
        {
            _hotkeyText.text = text;

            if (string.IsNullOrWhiteSpace(text) && _barHotkeyGo.activeSelf && !_inEditMode)
                _barHotkeyGo.SetActive(false);
            else if (!string.IsNullOrWhiteSpace(text) && !_barHotkeyGo.activeSelf)
                _barHotkeyGo.SetActive(true);
        }
        public void SetHotkeys(IEnumerable<string> hotbarTexts) => _hotbarHotkeys = hotbarTexts.ToList();

        public void SelectHotbar(int barIndex)
        {
            _barText.text = (barIndex + 1).ToString();
            SetHotkeyText(_hotbarHotkeys?.Count > barIndex ? _hotbarHotkeys[barIndex] : string.Empty);
        }

        public void ToggleActionSlotEditMode(bool enableEdits)
        {
            if (!_awake)
                return;

            _inEditMode = enableEdits;

            //_settingsButton.gameObject.SetActive(enableEdits);
            _nextButton.gameObject.SetActive(enableEdits);
            _previousButton.gameObject.SetActive(enableEdits);

            ToggleShowHide(enableEdits);

        }
        public void ToggleHotkeyEditMode(bool enableEdits)
        {
            if (!_awake)
                return;

            _inHotkeyEditMode = enableEdits;

            _nextHotkeyButton?.gameObject?.SetActive(enableEdits);
            _previousHotkeyButton?.gameObject?.SetActive(enableEdits);
            _hotkeyButton?.gameObject?.SetActive(enableEdits);

            if (enableEdits)
                _barHotkeyGo?.SetActive(true);
            else if (!enableEdits && string.IsNullOrWhiteSpace(_hotkeyText?.text))
                _barHotkeyGo?.SetActive(false);

            ToggleShowHide(enableEdits);
        }

        private void ToggleShowHide(bool editsEnabled)
        {
            if (editsEnabled)
            {
                _leftNavCanvas.enabled = true;
            }
            else if (!editsEnabled && _isHidden)
            {
                _leftNavCanvas.enabled = false;
            }
        }

        private void HookButtonEvents()
        {
            //_settingsButton.onClick.AddListener(() => HotbarSettingsViewer.Show());

            _nextButton.onClick.AddListener(() => HotbarsContainer.Controller.SelectNext());
            _previousButton.onClick.AddListener(() => HotbarsContainer.Controller.SelectPrevious());

            _nextHotkeyButton.onClick.AddListener(() => PlayerMenu.MainSettingsMenu.HotkeyCaptureMenu.ShowDialog(0, HotkeyCategories.NextHotbar));
            _previousHotkeyButton.onClick.AddListener(() => PlayerMenu.MainSettingsMenu.HotkeyCaptureMenu.ShowDialog(1, HotkeyCategories.PreviousHotbar));
            _hotkeyButton.onClick.AddListener(() => PlayerMenu.MainSettingsMenu.HotkeyCaptureMenu.ShowDialog(HotbarsContainer.SelectedHotbar, HotkeyCategories.Hotbar));
        }
    }
}