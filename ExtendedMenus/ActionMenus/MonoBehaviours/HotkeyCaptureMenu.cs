using ModifAmorphic.Outward.Unity.ActionMenus.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ModifAmorphic.Outward.Unity.ActionMenus
{
    public class KeyGroup
    {
        public List<KeyCode> Modifiers { get; set; } = new List<KeyCode>();
        public KeyCode KeyCode { get; set; }
    }
    
    [UnityScriptComponent]
    public class HotkeyCaptureMenu : MonoBehaviour, ISettingsView
    {
        public GameObject Dialog;
        public Image BackPanel;

        public Button CloseButton;
        public Button ClearButton;
        public Text CaptureHotkeyMenuText;

        public HotbarsContainer Hotbars;

        public delegate void OnKeysSelectedDelegate(int id, HotkeyCategories category, KeyGroup keyGroup);
        public event OnKeysSelectedDelegate OnKeysSelected;

        public delegate void OnClearPressedDelegate(int id, HotkeyCategories category);
        public event OnClearPressedDelegate OnClearPressed;

        private int _id;
        private HotkeyCategories _category;
        private KeyGroup _keyGroup = new KeyGroup();
        private bool _modifierUp = false;
        private Text _text;

        private bool _monitorKeys = false;
        private bool _isInit;

        public UnityEvent OnShow { get; } = new UnityEvent();

        public UnityEvent OnHide { get; } = new UnityEvent();

        public bool IsShowing => gameObject.activeSelf && _isInit;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "<Pending>")]
        private void Awake()
        {
            Debug.Log("HotkeyCaptureDialog::Awake");
            _text = GetComponentsInChildren<Text>().First(t => t.name.Equals("ContentText"));
            Hide(false);
            _isInit = true;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "<Pending>")]
        private void Start()
        {
            Debug.Log("HotkeyCaptureDialog::Start");
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "<Pending>")]
        private void Update()
        {
            if (!_monitorKeys)
                return;

            var _selectionStatePropInfo = typeof(Selectable).GetProperty("currentSelectionState", BindingFlags.NonPublic | BindingFlags.Instance);

            if (IsClosedOrClear())
                return;

            //Check for any keys down, and if found add them to the list of keys being held down
            if (Input.anyKeyDown)
            {
                foreach (var key in Enum.GetValues(typeof(KeyCode)).Cast<KeyCode>())
                {
                    if (Input.GetKeyDown(key) && _keyGroup.KeyCode != key && !_keyGroup.Modifiers.Contains(key))
                    {
                        AddKeyCode(key);
                        break;
                    }
                }
            }

            //Check if any keys have been released and raise the OnKeysSelected event if they have.
            for (int m = 0; m < _keyGroup.Modifiers.Count; m++)
            {
                if (Input.GetKeyUp(_keyGroup.Modifiers[m]))
                {
                    _modifierUp = true;
                    break;
                }
            }
            if (Input.GetKeyUp(_keyGroup.KeyCode) || (_modifierUp && _keyGroup.KeyCode != KeyCode.None))
            {
                try
                {
                    OnKeysSelected?.Invoke(_id, _category, _keyGroup);
                    Hotbars.Controller.ToggleHotkeyEdits(true);
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                }
                HideDialog();
            }
        }
        private bool IsClosedOrClear()
        {
            return (ClearButton.GetSelectionState() == ButtonExtensions.SelectionState.Selected
                || ClearButton.GetSelectionState() == ButtonExtensions.SelectionState.Highlighted
                || ClearButton.GetSelectionState() == ButtonExtensions.SelectionState.Pressed)
                || (CloseButton.GetSelectionState() == ButtonExtensions.SelectionState.Selected
                || CloseButton.GetSelectionState() == ButtonExtensions.SelectionState.Highlighted
                || CloseButton.GetSelectionState() == ButtonExtensions.SelectionState.Pressed);
        }

        private static Dictionary<KeyCode, string> MouseButtonNames = new Dictionary<KeyCode, string>()
        {
            { KeyCode.Mouse0, "LMB" },
            { KeyCode.Mouse1, "RMB" },
            { KeyCode.Mouse2, "MWheel" },
            { KeyCode.Mouse3, "Mouse 3" },
            { KeyCode.Mouse4, "Mouse 4" },
            { KeyCode.Mouse5, "Mouse 5" },
        };

        private void AddKeyCode(KeyCode key)
        {
            if (!string.IsNullOrWhiteSpace(_text.text))
            {
                _text.text += "+";
            }
            switch (key)
            {
                case KeyCode.RightAlt:
                case KeyCode.LeftAlt:
                    _keyGroup.Modifiers = new List<KeyCode>()
                        {
                            KeyCode.RightAlt, KeyCode.LeftAlt
                        };
                    _text.text += "Alt";
                    break;
                case KeyCode.RightControl:
                case KeyCode.LeftControl:
                    _keyGroup.Modifiers = new List<KeyCode>()
                        {
                            KeyCode.RightControl, KeyCode.LeftControl
                        };
                    _text.text += "Ctrl";
                    break;
                case KeyCode.RightShift:
                case KeyCode.LeftShift:
                    _keyGroup.Modifiers = new List<KeyCode>()
                        {
                            KeyCode.RightShift, KeyCode.LeftShift
                        };
                    _text.text += "Shift";
                    break;
                case KeyCode.Alpha0:
                case KeyCode.Alpha1:
                case KeyCode.Alpha2:
                case KeyCode.Alpha3:
                case KeyCode.Alpha4:
                case KeyCode.Alpha5:
                case KeyCode.Alpha6:
                case KeyCode.Alpha7:
                case KeyCode.Alpha8:
                case KeyCode.Alpha9:
                    _keyGroup.KeyCode = key;
                    _text.text += (key - 48).ToString();
                    break;
                case KeyCode.Mouse0:
                case KeyCode.Mouse1:
                case KeyCode.Mouse2:
                case KeyCode.Mouse3:
                case KeyCode.Mouse4:
                case KeyCode.Mouse5:
                case KeyCode.Mouse6:
                    _keyGroup.Modifiers.Clear();
                    _keyGroup.KeyCode = key;
                    _text.text = MouseButtonNames[key];
                    break;
                default:
                    _keyGroup.KeyCode = key;
                    _text.text += key.ToString();
                    break;
            }
        }
        public void ClearPressed()
        {
            OnClearPressed?.Invoke(_id, _category);
            OnKeysSelected?.Invoke(_id, _category, new KeyGroup() { KeyCode = KeyCode.None });
            Hotbars.Controller.ToggleHotkeyEdits(true);
            HideDialog();
        }

        public void Show()
        {
            gameObject.SetActive(true);
            BackPanel.gameObject.SetActive(true);
            CaptureHotkeyMenuText.gameObject.SetActive(true);
            Dialog.SetActive(false);
            Hotbars.Controller.ToggleHotkeyEdits(true);
            OnShow?.TryInvoke();
        }

        public void Hide() => Hide(true);

        public void ShowDialog(int id, HotkeyCategories category)
        {
            Debug.Log($"Capturing Hotkey for id {id} in category {category}.");
            _id = id;
            _category = category;
            Dialog.SetActive(true);
            _monitorKeys = true;
        }

        public void HideDialog()
        {
            _id = 0;
            _modifierUp = false;
            _category = default;
            _text.text = string.Empty;
            _keyGroup.KeyCode = default;
            _keyGroup.Modifiers.Clear();
            Dialog.SetActive(false);
            _monitorKeys = false;
        }

        private void Hide(bool raiseEvent)
        {
            Hotbars.Controller?.ToggleHotkeyEdits(false);
            
            if (_isInit)
                HideDialog();
            gameObject.SetActive(false);
            BackPanel.gameObject.SetActive(false);
            CaptureHotkeyMenuText.gameObject.SetActive(false);
            if (raiseEvent)
            {
                OnHide?.TryInvoke();
            }
        }

        private bool IsModifier(KeyCode keyCode)
        {
            switch (keyCode)
            {
                case KeyCode.RightAlt:
                case KeyCode.LeftAlt:
                case KeyCode.RightControl:
                case KeyCode.LeftControl:
                case KeyCode.RightShift:
                case KeyCode.LeftShift:
                    return true;
                default:
                    return false;
            }
        }
    }
}