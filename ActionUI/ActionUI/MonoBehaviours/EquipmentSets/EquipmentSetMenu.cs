using ModifAmorphic.Outward.Unity.ActionUI;
using ModifAmorphic.Outward.Unity.ActionUI.Extensions;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ModifAmorphic.Outward.Unity.ActionMenus
{
    [UnityScriptComponent]
    public class EquipmentSetMenu : MonoBehaviour, IActionMenu
    {
        private enum EquipmentSetViews
        {
            Weapons,
            Armor
        }

        public Toggle WeaponToggle;
        public Toggle ArmorToggle;
        public EquipmentSetView ArmorSetView;
        public EquipmentSetView WeaponSetView;

        private EquipmentSetViews _lastView;

        public bool IsShowing => gameObject.activeSelf;

        public UnityEvent OnShow { get; } = new UnityEvent();

        public UnityEvent OnHide { get; } = new UnityEvent();

        private void Awake()
        {
            WeaponToggle.onValueChanged.AddListener(ToggleWeaponsView);
            ArmorToggle.onValueChanged.AddListener(ToggleArmorView);

            Hide();
        }

        public void Show()
        {
            gameObject.SetActive(true);
            if (WeaponToggle.isOn)
            {
                ToggleArmorView(false);
                ToggleWeaponsView(true);
            }
            else
            {
                ToggleArmorView(true);
                ToggleWeaponsView(false);
            }
            //if (_lastView == EquipmentSetViews.Weapons)
            //{
            //    WeaponToggle.isOn = true;
            //    ArmorToggle.isOn = false;
            //}
            //else if (_lastView == EquipmentSetViews.Armor)
            //{
            //    if (!WeaponToggle.isOn)
            //        WeaponToggle.isOn = true;
            //    WeaponSetView.Show();
            //}
            OnShow.TryInvoke();
        }

        public void Hide()
        {
            gameObject.SetActive(false);
            OnHide.TryInvoke();
        }

        private void ToggleWeaponsView(bool isOn)
        {
            if (isOn)
            {
                //if (!WeaponToggle.isOn)
                //    WeaponToggle.isOn = true;
                WeaponSetView.Show();
            }
            else
                WeaponSetView.Hide();

        }

        private void ToggleArmorView(bool isOn)
        {
            if (isOn)
            {
                //if (!ArmorToggle.isOn)
                //    ArmorToggle.isOn = true;
                ArmorSetView.Show();
            }
            else
                ArmorSetView.Hide();
        }

    }
}