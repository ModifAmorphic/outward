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
        private Text buttonText;

        public HotbarsContainer HotbarsContainer { get; internal set; }

        private MouseClickListener _mouseClickListener;
        public MouseClickListener MouseClickListener => _mouseClickListener;

        public int HotbarIndex { get; internal set; }
        public int SlotIndex { get; internal set; }
        public int SlotId => HotbarIndex * 10000 + SlotIndex;

        //The parent transform
        private Transform _slotPanel;
        public Transform SlotPanel => _slotPanel;

        private Canvas _parentCanvas;
        public Canvas ParentCanvas => _parentCanvas;
        //private CanvasGroup _canvasGroup;

        //Displays the assigned hotkey / button.
        private Text _keyText;
        public Text KeyText => _keyText;

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

        public bool IsInEditMode => HotbarsContainer.IsInEditMode;

        //public ActionSlot() => _controller = new ActionSlotController(this);

        public Func<Action, bool> ActionRequested { get; private set;  }

        #region MonoBehaviour Methods
        private void Awake()
        {
            SetComponents();
            if (name != "BaseActionSlot")
            {
                name = $"ActionSlot_{HotbarIndex}_{SlotIndex}";
                _controller = new ActionSlotController(this);
                _controller.Refresh();

                //#if DEBUG
                //                buttonText = _actionButton.GetComponentInChildren<Text>(true);
                //                buttonText.text = $"Bar {HotbarIndex}\nSlot: {SlotIndex}"; //itemTypes[typeIndex];
                //#endif
            }

        }
        private void Start()
        {
            
        }
        private void Update()
        {
            _controller.OnActionSlotUpdate();
        }
        #endregion

        private void SetComponents()
        {
            _parentCanvas = transform.parent.GetComponentInParent<Canvas>();
            _slotPanel = transform.Find("SlotPanel");
            //_canvasGroup = _slotPanel.GetComponent<CanvasGroup>();
            
            _keyText = GetComponentsInChildren<Text>(true).First(t => t.name == "KeyText");
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