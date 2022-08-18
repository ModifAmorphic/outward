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

		private IHotbarController _controller;
		public IHotbarController Controller { get => _controller; }

  //      private Button _settingsButton;
		//public Button SettingsButton => _settingsButton;

		public ActionsViewer ActionsViewer;

		//private LeftHotbarNav _leftHotbarNav;
		public LeftHotbarNav LeftHotbarNav; // => _leftHotbarNav;

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

		//private Text posText;
		//private RectTransform rectTransform;

		//private Text canvasPosText;
		//private RectTransform canvasRectTransform;

		//private Text gridPosText;

		public bool IsInHotkeyEditMode { get; internal set; }
		public bool IsInActionSlotEditMode { get; internal set; }

		public bool HasChanges { get; internal set; }
		public void ClearChanges() => HasChanges = false;

		public event Action OnAwake;
		public bool IsAwake { get; private set; }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "<Pending>")]
		private void Awake()
		{
			SetComponents();
			_controller = new HotbarsController(this);

			//posText = transform.parent.GetComponentsInChildren<Text>().First(t => t.name == "HotbarsPosText");
			//rectTransform = GetComponent<RectTransform>();

			//canvasPosText = transform.parent.GetComponentsInChildren<Text>().First(t => t.name == "Hotbar0CanvasPosText");
			//gridPosText = transform.parent.GetComponentsInChildren<Text>().First(t => t.name == "Hotbar0GridPosText");
			IsAwake = true;
			OnAwake?.Invoke();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "<Pending>")]
		private void Update()
        {
			//canvasRectTransform = GetComponentsInChildren<Canvas>()?.FirstOrDefault(c => c.name == "HotbarCanvas0")?
			//	.GetComponent<RectTransform>();

			//posText.text = $"Hotbars Pos: {rectTransform.position.x}, {rectTransform.position.y}. Size {rectTransform.sizeDelta.x}, {rectTransform.sizeDelta.y}";
			//if (canvasRectTransform != null)
			//	canvasPosText.text = $"Hotbars0 Canvas Pos: {canvasRectTransform.position.x}, {canvasRectTransform.position.y}. Size {canvasRectTransform.sizeDelta.x}, {canvasRectTransform.sizeDelta.y}";

			//if (HotbarGrid?.Length > 0)
			//{
			//	var grid = HotbarGrid[0].GetComponent<RectTransform>();
			//	gridPosText.text = $"HotbarsGrid0 Pos: {grid.position.x}, {grid.position.y}. Size {grid.sizeDelta.x}, {grid.sizeDelta.y}";
			//}

			Controller.HotbarsContainerUpdate();
		}
		private void SetComponents()
		{
			//_leftHotbarNav = GetComponentInChildren<LeftHotbarNav>();
			_leftDisplay = transform.Find("LeftDisplay").GetComponent<RectTransform>();
			//_settingsButton = _leftDisplay.Find("Settings").GetComponent<Button>();

			_baseHotbarCanvas = transform.Find("BaseHotbarCanvas").GetComponent<Canvas>();

			_baseGrid = _baseHotbarCanvas.GetComponentsInChildren<GridLayoutGroup>().First(g => g.name == "BaseHotbarGrid");
			_baseActionSlot = _baseGrid.GetComponentInChildren<ActionSlot>().gameObject;
			_baseHotbarCanvas.gameObject.SetActive(false);
			_baseGrid.gameObject.SetActive(false);
			_baseActionSlot.SetActive(false);
		}

		internal void ResetCollections()
		{
			_hotbarGrid = new GridLayoutGroup[0];
			_hotbars = new ActionSlot[0][];
			_actionSlots = new Dictionary<int, ActionSlot>();
		}
		internal void Resize(float hotbarWidth) =>
			GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, hotbarWidth + _leftDisplay.rect.width + _leftDisplay.rect.width * 0.15f);

		internal void ConfigureHotbars(int barsAmount)
		{
			_hotbarGrid = new GridLayoutGroup[barsAmount];
			_hotbars = new ActionSlot[barsAmount][];
		}

		internal void ConfigureActionSlots(int bar, int slotsAmount) => _hotbars[bar] = new ActionSlot[slotsAmount];
	}
}