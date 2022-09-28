using ModifAmorphic.Outward.ActionUI.Patches;
using ModifAmorphic.Outward.Unity.ActionMenus;
using ModifAmorphic.Outward.Unity.ActionUI;
using System;
using System.Collections.Generic;
using System.Text;

namespace ModifAmorphic.Outward.ActionUI.Models
{
    public class EquipSkillPreview : ISlotAction
    {
        public string DisplayName { get; internal set; }

        public ICooldown Cooldown { get; internal set; }

        public IStackable Stack => null;

        public Dictionary<BarPositions, IBarProgress> ActiveBars => null;

        public bool HasDynamicIcon => true;

        public ActionSlotIcon[] ActionIcons { get; internal set; }

        public bool CheckOnUpdate => true;

        public Action TargetAction => null;

        public event Action OnActionRequested;
        public event Action OnEditRequested;
        public event Action<ActionSlotIcon[]> OnIconsChanged;

        private EquipmentSlot _equipmentSlot;

        public EquipSkillPreview(EquipmentSlot equipmentSlot)
        {
            _equipmentSlot = equipmentSlot;
            EquipmentPatches.AfterOnEquip += EquipmentChanged;
            EquipmentPatches.AfterOnUnequip += EquipmentChanged;
            UpdateIcons();
        }

        private void EquipmentChanged(Character character, Equipment equipment)
        {
            if (equipment.CurrentEquipmentSlot.SlotType != _equipmentSlot.SlotType)
                return;

            UpdateIcons();
        }

        private void UpdateIcons()
        {
            ActionIcons = new ActionSlotIcon[2]
            {
                new ActionSlotIcon()
                {
                    Name = _equipmentSlot.EquippedItem?.ItemIcon?.name ?? _equipmentSlot.EquippedItem?.name ?? _equipmentSlot.SlotName,
                    Icon = _equipmentSlot.EquippedItem?.ItemIcon ?? ActionMenuResources.Instance.SpriteResources["EmptySlotIcon"],
                    IsTopSprite = false,
                },
                new ActionSlotIcon()
                {
                    Name = "EquipmentSetIcon",
                    Icon = ActionMenuResources.Instance.SpriteResources["EquipmentSetIcon"],
                    IsTopSprite = false,
                },
            };
            OnIconsChanged?.Invoke(ActionIcons);
        }

        public ActionSlotIcon[] GetDynamicIcons() => ActionIcons;

        public bool GetEnabled() => true;

        public bool GetIsActionRequested() => false;

        public bool GetIsEditRequested() => false;

        public void SlotActionAssigned(ActionSlot assignedSlot)
        {
            throw new NotImplementedException();
        }

        public void SlotActionUnassigned()
        {
            throw new NotImplementedException();
        }
    }
}
