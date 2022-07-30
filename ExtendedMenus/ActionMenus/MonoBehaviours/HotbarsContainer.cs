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
	public class HotbarsContainer : MonoBehaviour
	{
		private RectTransform _leftDisplay;
		private Image _hotbarIcon;

		private RectTransform _barNumber;

		private IHotbarController _controller;
		public IHotbarController Controller { get => _controller; }

        private IActionViewData _actionViewData;
        public IActionViewData ActionViewData { get => _actionViewData; internal set => _actionViewData = value; }

        private Button _settingsButton;
		public Button SettingsButton => _settingsButton;

		//private ActionsViewer _actionsViewer;
		public ActionsViewer ActionsViewer;

		private Canvas _baseHotbarCanvas;
		internal Canvas BaseHotbarCanvas => _baseHotbarCanvas;

		private GridLayoutGroup _baseGrid;
		internal GridLayoutGroup BaseGrid => _baseGrid;

		private GridLayoutGroup[] _hotbarGrid;
		internal GridLayoutGroup[] HotbarGrid => _hotbarGrid;

		private GameObject _baseActionSlot;
		internal GameObject BaseActionSlot => _baseActionSlot;

		private ActionSlot[][] _hotbars;
		/// <summary>
		/// [Hotbar Index][Slot Index]
		/// </summary>
		public ActionSlot[][] Hotbars => _hotbars;

        private Dictionary<int, ActionSlot> _actionSlots = new Dictionary<int, ActionSlot>();
        public Dictionary<int, ActionSlot> ActionSlots => _actionSlots;

        private int _selectedHotbar;
		public int SelectedHotbar {
			get => _selectedHotbar;
			internal set
			{
				_selectedHotbar = value;
			}
		}

		//public HotbarsContainer() => _controller = new HotbarsController(this);
		private Text posText;
		private RectTransform rectTransform;

		private Text canvasPosText;
		private RectTransform canvasRectTransform;

		private Text gridPosText;

		public bool IsInEditMode { get; internal set; }

		private void Awake()
		{
			SetComponents();
			_controller = new HotbarsController(this);
			//GetComponentInChildren<HotbarsController>(true);

			posText = transform.parent.GetComponentsInChildren<Text>().First(t => t.name == "HotbarsPosText");
			rectTransform = GetComponent<RectTransform>();

			canvasPosText = transform.parent.GetComponentsInChildren<Text>().First(t => t.name == "Hotbar0CanvasPosText");
			gridPosText = transform.parent.GetComponentsInChildren<Text>().First(t => t.name == "Hotbar0GridPosText");
		}
		private void Update()
        {
			canvasRectTransform = GetComponentsInChildren<Canvas>()?.FirstOrDefault(c => c.name == "HotbarCanvas0")?
				.GetComponent<RectTransform>();

			posText.text = $"Hotbars Pos: {rectTransform.position.x}, {rectTransform.position.y}. Size {rectTransform.sizeDelta.x}, {rectTransform.sizeDelta.y}";
			if (canvasRectTransform != null)
				canvasPosText.text = $"Hotbars0 Canvas Pos: {canvasRectTransform.position.x}, {canvasRectTransform.position.y}. Size {canvasRectTransform.sizeDelta.x}, {canvasRectTransform.sizeDelta.y}";

			if (HotbarGrid?.Length > 0)
			{
				var grid = HotbarGrid[0].GetComponent<RectTransform>();
				gridPosText.text = $"HotbarsGrid0 Pos: {grid.position.x}, {grid.position.y}. Size {grid.sizeDelta.x}, {grid.sizeDelta.y}";
			}

			Controller.HotbarsContainerUpdate();
		}
		private void SetComponents()
		{
			//_hotbarsGridPanel = this.GetComponentsInChildren<RectTransform>().First(c => c.name == "GridPanel");

			_leftDisplay = transform.Find("LeftDisplay").GetComponent<RectTransform>();
			_settingsButton = _leftDisplay.Find("Settings").GetComponent<Button>();
			_barNumber = transform.Find("LeftDisplay/BarNumber").GetComponent<RectTransform>();

			_hotbarIcon = _barNumber.Find("BarIcon").GetComponent<Image>();

			_baseHotbarCanvas = transform.Find("BaseHotbarCanvas").GetComponent<Canvas>();

			_baseGrid = _baseHotbarCanvas.GetComponentsInChildren<GridLayoutGroup>().First(g => g.name == "BaseHotbarGrid");
			_baseActionSlot = _baseGrid.GetComponentInChildren<ActionSlot>().gameObject;
			_baseHotbarCanvas.gameObject.SetActive(false);
			_baseGrid.gameObject.SetActive(false);
			_baseActionSlot.SetActive(false);
		}
		
		internal void SetBarDisplayNumber(int hotbarIndex)
        {
            _hotbarIcon.GetComponentInChildren<Text>().text = (hotbarIndex + 1).ToString();
        }

		internal void ResetCollections()
		{
			_hotbarGrid = new GridLayoutGroup[0];
			_hotbars = new ActionSlot[0][];
			_actionSlots = new Dictionary<int, ActionSlot>();
		}
		internal void Resize(float hotbarWidth)
		{
			//float settingsWidth = _settingsButton.GetComponent<RectTransform>().rect.width;
			//float hotbarIconWidth = _hotbarIcon.GetComponent<RectTransform>().rect.width;
			//float padding = hotbarIconWidth > settingsWidth ? hotbarIconWidth : settingsWidth;

			GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, hotbarWidth + _leftDisplay.rect.width + _leftDisplay.rect.width * 0.15f);
		}

		internal void ConfigureHotbars(int barsAmount)
		{
			_hotbarGrid = new GridLayoutGroup[barsAmount];
			_hotbars = new ActionSlot[barsAmount][];
		}

		internal void ConfigureActionSlots(int bar, int slotsAmount) => _hotbars[bar] = new ActionSlot[slotsAmount];
	}
}