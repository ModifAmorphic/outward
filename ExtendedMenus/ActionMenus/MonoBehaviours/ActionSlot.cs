using ModifAmorphic.Outward.Unity.ActionMenus.Controllers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace ModifAmorphic.Outward.Unity.ActionMenus
{
    [UnityScriptComponent]
    public class ActionSlot : MonoBehaviour
    {
        public HotbarsContainer HotbarsContainer { get; internal set; }

        public HotkeyCaptureMenu HotkeyCapture;

        private MouseClickListener _mouseClickListener;
        public MouseClickListener MouseClickListener => _mouseClickListener;

        public int HotbarIndex { get; internal set; }
        public int SlotIndex { get; internal set; }
        public int SlotId => HotbarIndex * 10000 + SlotIndex;

        private ISlotAction _slotAction;
        public ISlotAction SlotAction { get => _slotAction; internal set => _slotAction = value; }

        private IActionSlotConfig _config;
        public IActionSlotConfig Config { get => _config; internal set => _config = value; }

        //The parent transform
        private Transform _slotPanel;
        public Transform SlotPanel => _slotPanel;

        private Canvas _parentCanvas;
        public Canvas ParentCanvas => _parentCanvas;

        private CanvasGroup _canvasGroup;
        public CanvasGroup CanvasGroup => _canvasGroup;

        //Displays the assigned hotkey / button.
        private Text _keyText;
        public Text KeyText => _keyText;

        private Button _keyButton;
        public Button KeyButton => _keyButton;

        #region ActionButton Resources
        private Button _actionButton;
        public Button ActionButton => _actionButton;

        private Image _actionImage;
        public Image ActionImage  => _actionImage;
        
        //Cooldown Resources
        private Image _cooldownImage;
        public Image CooldownImage => _cooldownImage;

        private Text _cooldownText;
        public Text CooldownText => _cooldownText;

        //Stack Amount Resources
        private Text _stackText;
        public Text StackText => _stackText;

        //Empty Image for when no slot action is assigned.
        private Image _emptyImage;
        public Image EmptyImage => _emptyImage;
        #endregion

        //Border around the ActionSlot
        private Image _borderImage;
        public Image BorderImage => _borderImage;

        private IActionSlotController _controller;
        public IActionSlotController Controller => _controller;

        public bool IsInEditMode => HotbarsContainer.IsInHotkeyEditMode;

        public Func<Action, bool> ActionRequested { get; private set;  }

        #region MonoBehaviour Methods
        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "<Pending>")]
        private void Awake()
        {
            SetComponents();
            if (name != "BaseActionSlot")
            {
                name = $"ActionSlot_{HotbarIndex}_{SlotIndex}";
                _controller = new ActionSlotController(this);
                _controller.ActionSlotAwake();
            }

        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "<Pending>")]
        private void Update()
        {
            _controller.ActionSlotUpdate();
        }
        #endregion

        private void SetComponents()
        {
            _parentCanvas = transform.parent?.parent?.GetComponent<Canvas>();
            _slotPanel = transform.Find("SlotPanel");
            _canvasGroup = GetComponent<CanvasGroup>();

            _keyText = GetComponentsInChildren<Text>(true).First(t => t.name == "KeyText");
            _keyButton = GetComponentsInChildren<Button>(true).First(t => t.name == "KeyButton");
            _borderImage = _slotPanel.GetComponentsInChildren<Image>(true).First(i => i.name == "ActionBorder");
            _actionButton = _slotPanel.GetComponentInChildren<Button>(true);
            _mouseClickListener = _actionButton.GetComponent<MouseClickListener>();

            _actionImage = _actionButton.GetComponent<Image>();

            _stackText = _actionButton.GetComponentsInChildren<Text>().First(t => t.name == "StackText");
            _cooldownText = _actionButton.GetComponentsInChildren<Text>().First(t => t.name == "CooldownText");
            _cooldownImage = _actionButton.GetComponentsInChildren<Image>().First(i => i.name == "CooldownImage");
            _emptyImage = _actionButton.GetComponentsInChildren<Image>().First(i => i.name == "EmptyImage");
        }
        
        public static readonly Color DisabledColor = new Color(0.7843137f, 0.7843137f, 0.7843137f, 0.5019608f); //#C8C8C8;
        internal void SetButtonNormalColor(Color color)
        {
            var colorBlock = ActionButton.colors;
            colorBlock.normalColor = color;
            ActionButton.colors = colorBlock;
        }
    }
}