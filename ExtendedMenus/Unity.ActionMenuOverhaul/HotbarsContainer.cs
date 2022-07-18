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

		private IHotbarController _hotBar;
		public IHotbarController Hotbar { get => _hotBar; }

		private Button _settingsButton;
		public Button SettingsButton => _settingsButton;

		
		


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

			_hotBar = GetComponentInChildren<HotbarsController>(true);

			_leftDisplay = transform.Find("LeftDisplay").GetComponent<RectTransform>();
			_settingsButton = _leftDisplay.Find("Settings").GetComponent<Button>();
			_barNumber = transform.Find("LeftDisplay/BarNumber").GetComponent<RectTransform>();

			_hotbarIcon = _barNumber.Find("BarIcon").GetComponent<Image>();
			

			_hotBar.OnResizeWidthRequest += Resize;
            _hotBar.OnHotbarSelected += UpdateBarIcon;
		}

        private void UpdateBarIcon(int hotbarIndex)
        {
            _hotbarIcon.GetComponentInChildren<Text>().text = (hotbarIndex + 1).ToString();
        }

        private void Resize(float hotbarWidth)
		{
			//float settingsWidth = _settingsButton.GetComponent<RectTransform>().rect.width;
			//float hotbarIconWidth = _hotbarIcon.GetComponent<RectTransform>().rect.width;
			//float padding = hotbarIconWidth > settingsWidth ? hotbarIconWidth : settingsWidth;

			GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, hotbarWidth + _leftDisplay.rect.width + _leftDisplay.rect.width * 0.15f);
		}
	}
}