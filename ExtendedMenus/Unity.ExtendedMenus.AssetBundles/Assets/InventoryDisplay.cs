using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryDisplay : MonoBehaviour
{
    public void Awake()
    {
		var invButton = transform.GetChild(0).gameObject;
		if (invButton != null)
        {
			for (int i = 0; i < 16 ; i++)
            {
				Instantiate(invButton, invButton.transform.parent);
			}
        }
	}
    public void ShowOnly(int itemType)
	{
		for (int i = 0; i < transform.childCount; i++)
		{
			InventoryItemButton thisItem = transform.GetChild(i).GetComponent<InventoryItemButton>();
			thisItem.gameObject.SetActive(thisItem.typeIndex == itemType);
		}
	}

	public void ShowAll()
	{
		for (int i = 0; i < transform.childCount; i++)
		{
			transform.GetChild(i).gameObject.SetActive(true);
		}
	}
}
