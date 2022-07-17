using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace ModifAmorphic.Outward.Unity.ActionMenus
{
	[UnityScriptComponent]
	public class OldHotbars : MonoBehaviour
	{
		//public GameObject SettingsButton;
		private RectTransform _hotbarContainer;
		private Button _settingsButton;
		private GridLayoutGroup _hotbarsGrid;
		private HorizontalLayoutGroup _hotbarGroup;
		private GameObject _baseActionSlot;
		private List<GameObject> _actionSlots = new List<GameObject>();
		public List<GameObject> ActionSlots { get => _actionSlots; }

		private List<GameObject> _hactionSlots = new List<GameObject>();


		public int ActionSlotsPerBar { get => _hotbarsGrid.constraintCount; }
		public int HotbarCount
		{
			get
			{
				var totalSlots = _hotbarsGrid.transform.childCount;
				return totalSlots > _hotbarsGrid.constraintCount ? totalSlots / _hotbarsGrid.constraintCount : 1;
			}
		}

		private void Awake()
		{
			SetComponents();
			ConfigureHotbarsGrid(1, 8);
			ConfigureHotbarsGroup(1, 8);
		}
		private void Start()
		{


		}
		private void SetComponents()
		{
			_hotbarContainer = this.GetComponentsInChildren<RectTransform>().First(c => c.name == "GridPanel");
			_hotbarsGrid = _hotbarContainer.GetComponentInChildren<GridLayoutGroup>();
			_settingsButton = _hotbarContainer.transform.Find("Settings").GetComponent<Button>();
			_baseActionSlot = _hotbarsGrid.GetComponentInChildren<Button>().gameObject;
			_baseActionSlot.SetActive(false);

			_hotbarGroup = this.GetComponentInChildren<HorizontalLayoutGroup>();

			//var gameAssembly = AppDomain.CurrentDomain.GetAssemblies()
			//		.FirstOrDefault(a => a.GetName().Name.Equals("Assembly-CSharp", StringComparison.InvariantCultureIgnoreCase));
			//var qsDisplayType = gameAssembly.GetTypes()
			//		.FirstOrDefault(t => t.FullName.Equals("EditorQuickSlotDisplayPlacer", StringComparison.InvariantCultureIgnoreCase));
			//var qsDisplayType = Type.GetType("EditorQuickSlotDisplayPlacer, Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
			//if (qsDisplayType != null)
			//	baseActionSlot.AddComponent(qsDisplayType);
		}
		public void ConfigureHotbarsGrid(int hotbars, int actionSlots)
		{
			_hotbarsGrid.constraintCount = actionSlots;
			ClearGridActionSlots();

			this._actionSlots = new List<GameObject>();
			for (int h = 0; h < hotbars; h++)
			{
				for (int i = 0; i < actionSlots; i++)
				{
					var newSlot = Instantiate(_baseActionSlot, _baseActionSlot.transform.parent);
					var slotBtn = newSlot.GetComponent<ActionSlot>();
					slotBtn.SlotNo = i + 1;
					slotBtn.HotbarId = h;
					newSlot.SetActive(true);
					this._actionSlots.Add(newSlot);
				}
			}
			StartCoroutine(ResizeLayoutGroup());
		}
		public void ConfigureHotbarsGroup(int hotbars, int actionSlots)
		{
			//hotbarGroup.constraintCount = actionSlots;
			ClearGroupActionSlots();

			this._hactionSlots = new List<GameObject>();
			for (int h = 0; h < hotbars; h++)
			{
				for (int i = 0; i < actionSlots; i++)
				{
					var newSlot = Instantiate(_baseActionSlot, _hotbarGroup.transform);
					var slotBtn = newSlot.GetComponent<ActionSlot>();
					slotBtn.SlotNo = i + 1;
					slotBtn.HotbarId = h;
					newSlot.SetActive(true);
					this._hactionSlots.Add(newSlot);
				}
			}
			//StartCoroutine(ResizeLayoutGroup());
		}
		private void ClearGridActionSlots()
		{
			var actionSlots = _hotbarsGrid.GetComponentsInChildren<ActionSlot>(false);
			for (int i = 0; i < actionSlots.Length; i++)
			{
				DestroyImmediate(actionSlots[i].gameObject);
			}

		}
		private void ClearGroupActionSlots()
		{
			var actionSlots = _hotbarGroup.GetComponentsInChildren<ActionSlot>(false);
			for (int i = 0; i < actionSlots.Length; i++)
			{
				DestroyImmediate(actionSlots[i].gameObject);
			}

		}
		IEnumerator ResizeLayoutGroup()
		{
			yield return new WaitForEndOfFrame();

			float btnWidth = ActionSlots.First().GetComponent<RectTransform>().rect.width;
			Debug.Log($"Slot Button RectTransform has a width of {btnWidth}");

			var glgRect = _hotbarsGrid.GetComponent<RectTransform>().rect;
			float width = glgRect.width;
			float settingsWidth = _settingsButton.GetComponent<RectTransform>().rect.width;
			float hotbarWidth = settingsWidth + (btnWidth + _hotbarsGrid.spacing.x) * ((float)_hotbarsGrid.constraintCount) + _hotbarsGrid.padding.horizontal * 2 - _hotbarsGrid.spacing.x;
			Debug.Log($"Changing width of GridLayoutGroup's RectTransform from {width} to {hotbarWidth}");

			_hotbarContainer.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, hotbarWidth);
		}
	}
}