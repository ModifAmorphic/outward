using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ModifAmorphic.Outward.Unity.ActionMenus
{
    public enum DurableEquipmentSlot
    {
        None, Head, Chest, RightHand, LeftHand, Feet
    }
    public enum DurableEquipmentType
    {
        None, Helm, Chest, RangedWeapon, MeleeWeapon, Shield, Boots
    }
    [UnityScriptComponent]
    public class DurabilitySlot : DynamicColorImage
    {
        private Dictionary<DurableEquipmentType, Image> _equipmentImages;

        public DurableEquipmentSlot EquipmentSlot;
        public DurableEquipmentType EquipmentType;

        protected override void OnAwake()
        {
            _equipmentImages = GetComponentsInChildren<Image>().ToDictionary(i => (DurableEquipmentType)Enum.Parse(typeof(DurableEquipmentType), i.name, true), i => i);
            SetDisplayImage(EquipmentType);
        }

        public void SetEquipmentType(DurableEquipmentType equipmentType)
        {
            EquipmentType = equipmentType;
            if (_isAwake)
                SetDisplayImage(equipmentType);
        }
        private void SetDisplayImage(DurableEquipmentType equipmentType)
        {
            foreach(var kvp in _equipmentImages)
            {
                kvp.Value.enabled = kvp.Key == equipmentType;
            }
            if (equipmentType != default(DurableEquipmentType))
                Image = _equipmentImages[equipmentType];
        }
    }
}