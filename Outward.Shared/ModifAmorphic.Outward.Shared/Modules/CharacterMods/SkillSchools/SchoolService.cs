using ModifAmorphic.Outward.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ModifAmorphic.Outward.Modules.CharacterMods
{
    public class SchoolService
    {
        private CharacterInstances _charInstances;
        private Func<IModifLogger> _getLogger;
        private IModifLogger Logger => _getLogger?.Invoke();

        public SchoolService(ServicesProvider servicesProvider) => (_charInstances, _getLogger) = (servicesProvider.GetService<CharacterInstances>(), servicesProvider.GetService<IModifLogger>);

        public Dictionary<int, SkillSchool> GetIndexedBreakthroughSchools(string characterUID)
        {
            _charInstances.TryGetCharacterManager(out var characterManager);

            if (characterManager.Characters.TryGetValue(characterUID, out var character))
            {
                _charInstances.TryGetSkillSchools(out var skillSchools);

                var learnedSkills = character.Inventory.SkillKnowledge.GetLearnedItems().Select(s => ((Skill)s));
                
                var breakThroughs = skillSchools.Select(s => s.Value.BreakthroughSkill as SkillSlot).ToList();
                var charBreakThroughs = learnedSkills.Where(s => breakThroughs.Any(b => b.Skill.ItemID == s.ItemID));
                return skillSchools.Where(kvp => charBreakThroughs.Any(c => c.SchoolIndex == kvp.Key)).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            }
            return new Dictionary<int, SkillSchool>();
        }
        public bool TryGetSchool(int schoolIndex, out SkillSchool skillSchool)
        {
            skillSchool = default;

            if (!_charInstances.TryGetSkillTreeHolder(out var skillTreeHolder))
                return false;

            skillSchool = skillTreeHolder.GetSkillTree(schoolIndex);
            if (skillSchool == null)
                return false;

            return true;
        }
        public bool TryForgetSkill(int skillItemID, string characterUID, out SkillSlot skillSlotForgotten, bool notifyOnRemove = false)
        {
            Logger.LogTrace($"{nameof(SchoolService)}::{nameof(TryForgetSkill)}: skillItemID: {skillItemID}, characterUID: '{characterUID}'");
            
            skillSlotForgotten = default;
            
            if (!_charInstances.TryGetCharacterManager(out var characterManager) || !characterManager.Characters.TryGetValue(characterUID, out var character))
                return false;
            
            var skillItem = character.Inventory.SkillKnowledge.GetItemFromItemID(skillItemID) as Skill;
            if (skillItem == null || ! TryGetSchool(skillItem.SchoolIndex, out var skillSchool))
                return false;

            var skillSlot = skillSchool.GetComponentsInChildren<SkillSlot>()?.FirstOrDefault(s => s.Skill.ItemID == skillItem.ItemID);
            if (skillSlot == default)
                return false;

            character.Inventory.SkillKnowledge.RemoveItem(skillItemID);
            ItemManager.Instance.DestroyItem(skillItem.UID);

            if (notifyOnRemove)
                character.Inventory.NotifyItemRemoved(skillSlot.Skill, 1, false);

            Logger.LogDebug($"{nameof(SchoolService)}::{nameof(TryForgetSkill)}: Forgot Skill Slot: {skillSlot.Skill.Name} for characterUID: '{characterUID}'.");
            skillSlotForgotten = skillSlot;
            return true;
        }

        public bool TryForgetSchool(int schoolIndex, string characterUID, out IEnumerable<SkillSlot> skillSlotsForgotten, bool notifyOnRemove = false)
        {
            Logger.LogTrace($"{nameof(SchoolService)}::{nameof(TryForgetSchool)}: schoolIndex: {schoolIndex}, characterUID: '{characterUID}'");
            _charInstances.TryGetSkillTreeHolder(out var skillTreeHolder);
            var school = skillTreeHolder.GetSkillTree(schoolIndex);
            Logger.LogInfo($"Forgetting school {school.Name} for character '{characterUID}'");
            skillSlotsForgotten = new List<SkillSlot>();

            _charInstances.TryGetCharacterManager(out var characterManager);
            if (!characterManager.Characters.TryGetValue(characterUID, out var character))
                return false;

            var learnedItems = character.Inventory.SkillKnowledge.GetLearnedItems();
            var forgetSkillSlots = school.GetComponentsInChildren<SkillSlot>()
                .Where(s => s != null && learnedItems.Any(ls => s.Skill.ItemID == ls.ItemID));
            var forgetSkills = learnedItems.Where(s => forgetSkillSlots.Any(f => f.Skill.ItemID == s.ItemID)).ToDictionary(s => s.ItemID, s => s);
            foreach (var slot in forgetSkillSlots)
            {
                character.Inventory.SkillKnowledge.RemoveItem(slot.Skill.ItemID);
                ItemManager.Instance.DestroyItem(forgetSkills[slot.Skill.ItemID].UID);
                if (notifyOnRemove)
                    character.Inventory.NotifyItemRemoved(slot.Skill, 1, false);
                Logger.LogDebug($"{nameof(SchoolService)}::{nameof(TryForgetSchool)}: Forgot Skill Slot: {slot.Skill.Name} for characterUID: '{characterUID}'.");
                ((List<SkillSlot>)skillSlotsForgotten).Add(slot);
            }
            Logger.LogDebug($"{nameof(SchoolService)}::{nameof(TryForgetSchool)}: {skillSlotsForgotten.Count()} were forgotten for characterUID: '{characterUID}'.");
            return true;
        }

        public bool TryRefundBreakthroughs(string characterUID, int refundAmount, out int remainingBreakthroughs)
        {
            Logger.LogTrace($"{nameof(SchoolService)}::{nameof(TryRefundBreakthroughs)}: characterUID: {characterUID}, refundAmount: '{refundAmount}'");
            _charInstances.TryGetCharacterManager(out var characterManager);
            if (!characterManager.Characters.TryGetValue(characterUID, out var character))
            {
                remainingBreakthroughs = -1;
                return false;
            }

            var breakThroughsField = typeof(PlayerCharacterStats).GetField("m_usedBreakthroughCount", BindingFlags.NonPublic | BindingFlags.Instance);
            var usedBreakThroughs = Convert.ToInt32(breakThroughsField.GetValue(character.PlayerStats));

            var usedPoints = usedBreakThroughs - refundAmount < 0 ? 0 : usedBreakThroughs - refundAmount;
            breakThroughsField.SetValue(character.PlayerStats, usedPoints);

            remainingBreakthroughs = character.PlayerStats.RemainingBreakthrough;
            Logger.LogDebug($"{nameof(SchoolService)}::{nameof(TryRefundBreakthroughs)}: Refunded {refundAmount} breakthroughs for characterUID: {characterUID}. Remaining Breakthroughs: {remainingBreakthroughs}.");
            return true;
        }
    }
}
