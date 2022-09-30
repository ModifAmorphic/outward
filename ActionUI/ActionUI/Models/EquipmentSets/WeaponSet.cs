using ModifAmorphic.Outward.Unity.ActionUI.Models.EquipmentSets;
using System.Collections.Generic;
using UnityEngine;

namespace ModifAmorphic.Outward.Unity.ActionUI.EquipmentSets
{
    public class WeaponSet : IEquipmentSet
    {
        private Dictionary<EquipSlots, EquipSlot> _equipSlots = new Dictionary<EquipSlots, EquipSlot>()
        {
            {EquipSlots.RightHand, null },
            {EquipSlots.LeftHand, null }
        };

        public int SetID { get; set; }

        private string _name;
        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                _notificationObject = new GameObject(_name);
            }
        }
        public EquipSlots IconSlot { get; set; } = EquipSlots.RightHand;
        public EquipSlot LeftHand
        {
            get => _equipSlots.TryGetValue(EquipSlots.LeftHand, out var equip) ? equip : null;
            set
            {
                if (_equipSlots.ContainsKey(EquipSlots.LeftHand))
                    _equipSlots[EquipSlots.LeftHand] = value;
                else
                    _equipSlots.Add(EquipSlots.LeftHand, value);
            }
        }

        public EquipSlot RightHand
        {
            get => _equipSlots.TryGetValue(EquipSlots.RightHand, out var equip) ? equip : null;
            set
            {
                if (_equipSlots.ContainsKey(EquipSlots.RightHand))
                    _equipSlots[EquipSlots.RightHand] = value;
                else
                    _equipSlots.Add(EquipSlots.RightHand, value);
            }
        }

        public IEnumerable<EquipSlot> GetEquipSlots() => _equipSlots.Values;

        public EquipSlot GetIconEquipSlot() => _equipSlots.TryGetValue(IconSlot, out var iconSlot) ? iconSlot : null;

        private GameObject _notificationObject;
        public GameObject GetNotificationObject() => _notificationObject;
    }
}
