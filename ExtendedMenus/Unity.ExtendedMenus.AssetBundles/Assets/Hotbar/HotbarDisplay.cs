using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class HotbarDisplay : MonoBehaviour
{
	private int Hotbars = 2;
	private int Slots = 10;
	//public GameObject SettingsButton;
	private GameObject hotbarContainer;
	private Button settingsButton;
	private GridLayoutGroup hotbarsGroup;
	private List<GameObject> quickSlots = new List<GameObject>();
	public List<GameObject> QuickSlots { get => quickSlots; }
	public void Awake()
    {
		SetComponents();
		hotbarsGroup.constraintCount = Slots;
		var slot = hotbarsGroup.GetComponentInChildren<Button>().gameObject;

		//GetComponent<GridLayoutGroup>().constraintCount = Slots;
		//var slot = transform.GetChild(0).gameObject;
		//var queue = new Queue<GameObject>();
		if (slot != null)
        {
			slot.SetActive(false);
			for (int h = 0; h < Hotbars; h++)
			{
				for (int i = 0; i < Slots; i++)
				{
					var newSlot = Instantiate(slot, slot.transform.parent);
					var slotBtn = newSlot.GetComponent<SlotButton>();
					slotBtn.SlotNo = i + 1;
					slotBtn.HotbarId = h;
					newSlot.SetActive(true);
					quickSlots.Add(newSlot);
				}
			}
			StartCoroutine(ResizeLayoutGroup());

		}
	}
	private void SetComponents()
    {
		hotbarContainer = this.gameObject;
		hotbarsGroup = GetComponentInChildren<GridLayoutGroup>();
		settingsButton = transform.Find("Settings").GetComponent<Button>();
		var slotButton = hotbarsGroup.GetComponentInChildren<Button>();
	}
	IEnumerator ResizeLayoutGroup()
    {
		yield return new WaitForEndOfFrame();

		//var slotButton = transform.GetComponentInChildren<Button>(false);
		float btnWidth = QuickSlots.First().GetComponent<RectTransform>().rect.width;
		Debug.Log($"Slot Button RectTransform has a width of {btnWidth}");

		var glgRect = hotbarsGroup.GetComponent<RectTransform>().rect;
		float width = glgRect.width;
		float settingsWidth = settingsButton.GetComponent<RectTransform>().rect.width;
		float hotbarWidth = settingsWidth + (btnWidth + hotbarsGroup.spacing.x) * ((float)Slots) + hotbarsGroup.padding.horizontal * 2 - hotbarsGroup.spacing.x;
		//float extraWidth = width - glg.padding.horizontal - (glg.constraintCount - 1) * glg.spacing.x;
		Debug.Log($"Changing width of GridLayoutGroup's RectTransform from {width} to {hotbarWidth}");

		GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, hotbarWidth);
		//glg.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, hotbarWidth);
	}
}
