using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace ModifAmorphic.Outward.Unity.ActionMenuOverhaul
{
	[UnityScriptComponent(ComponentPath = "OverhaulCanvas/Hotbars")]
	public class Hotbars : MonoBehaviour
	{
		//public GameObject SettingsButton;
		private GameObject hotbarContainer;
		private Button settingsButton;
		private GridLayoutGroup hotbarsGroup;
		private GameObject baseActionSlot;
		private List<GameObject> actionSlots = new List<GameObject>();
		public List<GameObject> ActionSlots { get => actionSlots; }


		public int ActionSlotsPerBar { get => hotbarsGroup.constraintCount; }
        public int HotbarCount { 
			get
            {
				var totalSlots = hotbarsGroup.transform.childCount;
				return totalSlots > hotbarsGroup.constraintCount ? totalSlots / hotbarsGroup.constraintCount : 1;
			}
		}

		public void Awake()
		{
			SetComponents();
			ConfigureHotbars(1, 8);
		}
		private void SetComponents()
		{
			hotbarContainer = this.gameObject;
			hotbarsGroup = GetComponentInChildren<GridLayoutGroup>();
			settingsButton = transform.Find("Settings").GetComponent<Button>();
			baseActionSlot = hotbarsGroup.GetComponentInChildren<Button>().gameObject;
			baseActionSlot.SetActive(false);
		}
		public void ConfigureHotbars(int hotbars, int actionSlots)
        {
			hotbarsGroup.constraintCount = actionSlots;
			ClearActionSlots();

			this.actionSlots = new List<GameObject>();
			for (int h = 0; h < hotbars; h++)
			{
				for (int i = 0; i < actionSlots; i++)
				{
					var newSlot = Instantiate(baseActionSlot, baseActionSlot.transform.parent);
					var slotBtn = newSlot.GetComponent<ActionSlot>();
					slotBtn.SlotNo = i + 1;
					slotBtn.HotbarId = h;
					newSlot.SetActive(true);
					this.actionSlots.Add(newSlot);
				}
			}
			StartCoroutine(ResizeLayoutGroup());
		}
		private void ClearActionSlots()
        {
			var actionSlots = hotbarsGroup.GetComponentsInChildren<Button>(false);
			for (int i = 0;i < actionSlots.Length;i++)
            {
				DestroyImmediate(actionSlots[i].gameObject);
            }

		}
		IEnumerator ResizeLayoutGroup()
		{
			yield return new WaitForEndOfFrame();

			float btnWidth = ActionSlots.First().GetComponent<RectTransform>().rect.width;
			Debug.Log($"Slot Button RectTransform has a width of {btnWidth}");

			var glgRect = hotbarsGroup.GetComponent<RectTransform>().rect;
			float width = glgRect.width;
			float settingsWidth = settingsButton.GetComponent<RectTransform>().rect.width;
			float hotbarWidth = settingsWidth + (btnWidth + hotbarsGroup.spacing.x) * ((float)hotbarsGroup.constraintCount) + hotbarsGroup.padding.horizontal * 2 - hotbarsGroup.spacing.x;
			Debug.Log($"Changing width of GridLayoutGroup's RectTransform from {width} to {hotbarWidth}");

			GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, hotbarWidth);
		}
	}
}