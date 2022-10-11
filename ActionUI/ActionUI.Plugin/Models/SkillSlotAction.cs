using ModifAmorphic.Outward.ActionUI.Services;
using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.Unity.ActionMenus;
using Rewired;
using System;
using System.Linq;

namespace ModifAmorphic.Outward.ActionUI.Models
{
    internal class SkillSlotAction : SlotActionBase
    {
        private Skill skill;
        public Skill ActionSkill => skill;

        private readonly bool isEquipmentSetSkill;
        private EquipmentSetSkill equipmentSetSkill;

        public SkillSlotAction(Skill skill, Player player, Character character, SlotDataService slotData, bool combatModeEnabled, Func<IModifLogger> getLogger)
            : base(skill, player, character, slotData, combatModeEnabled, getLogger)
        {
            this.skill = skill;

            this.Cooldown = new ItemCooldownTracker(skill);
            this.Stack = null;
            this.equipmentSetSkill = skill as EquipmentSetSkill;
            this.isEquipmentSetSkill = equipmentSetSkill != null;

        }

        public override bool GetEnabled()
        {
            if (IsCombatModeEnabled && character.InCombat && (hotbarIndex != 0 || slotIndex > 7))
                return false;

            if (skill.IsChildToCharacter && skill.GetIsQuickSlotReq())
                return true;

            if (!skill.IsChildToCharacter)
            {
                var foundSkill = (Skill)inventory.SkillKnowledge.GetLearnedItems().FirstOrDefault(i => i.ItemID == skill.ItemID);
                if (foundSkill != null)
                {
                    skill = foundSkill;
                    skill.SetQuickSlot(0);
                    ((ItemCooldownTracker)Cooldown).SetSkill(skill);
                }
            }

            return false;
        }

        public override void ActivateTarget()
        {
            if (this.skill == null)
                return;

            if (inventory.LearnedSkill(skill))
            {
                skill.TryQuickSlotUse();
            }
            else
            {
                character.CharacterUI.ShowInfoNotification(LocalizationManager.Instance.GetLoc("Notification_Quickslot_NoItemInventory", Global.SetTextColor(skill.Name, Global.HIGHLIGHT)));
            }
        }

        protected override bool GetIsLocked() => !skill.IsChildToCharacter || !skill.GetIsQuickSlotReq();

        protected override void AssignRefItemDisplay(ActionSlot assignedSlot)
        {
            base.AssignRefItemDisplay(assignedSlot);
            if (isEquipmentSetSkill && refItemDisplay != null)
            {
                EquipSetPrefabService.Instance.AddEquipmentSetIcon(refItemDisplay, equipmentSetSkill);
            }
        }
    }
}
