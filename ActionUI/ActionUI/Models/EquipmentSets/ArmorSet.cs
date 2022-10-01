using ModifAmorphic.Outward.Unity.ActionUI.Models.EquipmentSets;
using System.Collections.Generic;
using UnityEngine;

namespace ModifAmorphic.Outward.Unity.ActionUI.EquipmentSets
{
    public class ArmorSet : IEquipmentSet
    {
        private Dictionary<EquipSlots, EquipSlot> _equipSlots = new Dictionary<EquipSlots, EquipSlot>()
        {
            {EquipSlots.Head, null },
            {EquipSlots.Chest, null },
            {EquipSlots.Feet, null }
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
        public EquipSlots IconSlot { get; set; } = EquipSlots.Chest;

        public EquipSlot Head
        {
            get => _equipSlots.TryGetValue(EquipSlots.Head, out var equip) ? equip : null;
            set
            {
                if (_equipSlots.ContainsKey(EquipSlots.Head))
                    _equipSlots[EquipSlots.Head] = value;
                else
                    _equipSlots.Add(EquipSlots.Head, value);
            }
        }

        public EquipSlot Chest
        {
            get => _equipSlots.TryGetValue(EquipSlots.Chest, out var equip) ? equip : null;
            set
            {
                if (_equipSlots.ContainsKey(EquipSlots.Chest))
                    _equipSlots[EquipSlots.Chest] = value;
                else
                    _equipSlots.Add(EquipSlots.Chest, value);
            }
        }

        public EquipSlot Feet
        {
            get => _equipSlots.TryGetValue(EquipSlots.Feet, out var equip) ? equip : null;
            set
            {
                if (_equipSlots.ContainsKey(EquipSlots.Feet))
                    _equipSlots[EquipSlots.Feet] = value;
                else
                    _equipSlots.Add(EquipSlots.Feet, value);
            }
        }

        private GameObject _notificationObject;
        public GameObject GetNotificationObject() => _notificationObject;

        public EquipSlot GetIconEquipSlot() => _equipSlots.TryGetValue(IconSlot, out var iconSlot) ? iconSlot : null;

        public IEnumerable<EquipSlot> GetEquipSlots() => _equipSlots.Values;
    }
}
