using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace ModifAmorphic.Outward.Unity.ActionMenus
{
	[UnityScriptComponent(ComponentPath = "OverhaulCanvas/Hotbars")]
	public class Hotbars : MonoBehaviour
	{
		//public GameObject SettingsButton;
		private Button _settingsButton;

		private IHotbar _enabledHotbar;
		public IHotbar EnabledHotbar { get => _enabledHotbar; }

		private GameObject _gridPanel;
		private IHotbar _gridHotbar;

		private GameObject _singlePanel;
		private IHotbar _singleHotbar;

        public enum HotbarType 
		{ 
			Grid,
			Single
		}


        private void Awake()
		{
			SetComponents();
		}
		private void Start()
        {
			

		}
		private void SetComponents()
		{
			//_hotbarsGridPanel = this.GetComponentsInChildren<RectTransform>().First(c => c.name == "GridPanel");
			_gridHotbar = GetComponentInChildren<GridHotbar>(true);
			_gridPanel = _gridHotbar.GameObject;

			_singleHotbar = GetComponentInChildren<SingleHotbar>(true);
			_singlePanel = _singleHotbar.GameObject;

			_settingsButton = transform.Find("Settings").GetComponent<Button>();

			_gridHotbar.OnResizeWidthRequest += Resize;
			_singleHotbar.OnResizeWidthRequest += Resize;

		}
		public IHotbar GetEnabledHotbar()
        {
			return _gridHotbar;
        }
		public void ConfigureHotbars(HotbarType hotbarType, int hotbars, int actionSlots)
        {
			_enabledHotbar = hotbarType == HotbarType.Grid ? _gridHotbar : _singleHotbar;
			if (hotbarType == HotbarType.Grid) // && _enabledHotbar is SingleHotbar)
            {
				_singleHotbar.GameObject.SetActive(false);
				_enabledHotbar = _gridHotbar;
				_enabledHotbar.GameObject.SetActive(true);
			}
			else if (hotbarType == HotbarType.Single) // && _enabledHotbar is GridHotbar)
            {
				_gridHotbar.GameObject.SetActive(false);
				_enabledHotbar = _singleHotbar;
				_enabledHotbar.GameObject.SetActive(true);
			}
			_enabledHotbar.ConfigureHotbar(hotbars, actionSlots);
		}
		

        private void Resize(float hotbarWidth)
		{

			float settingsWidth = _settingsButton.GetComponent<RectTransform>().rect.width;

			GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, hotbarWidth + settingsWidth);
		}
	}
}