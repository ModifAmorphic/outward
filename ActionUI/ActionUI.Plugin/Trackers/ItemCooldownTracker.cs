using ModifAmorphic.Outward.Extensions;
using ModifAmorphic.Outward.Unity.ActionUI;
using UnityEngine;

namespace ModifAmorphic.Outward.ActionUI.Models
{
    internal class ItemCooldownTracker : ICooldown
    {
        private Skill skill;

        public bool HasCooldown => skill != null;

        public bool GetIsInCooldown() => skill?.InCooldown() ?? false;

        public float GetProgress()
        {
            if (!GetIsInCooldown())
                return 0f;

            return skill.GetCooldownProgress();
        }

        public float GetSecondsRemaining() => skill.GetCooldownSecondsRemaining();

        public ItemCooldownTracker(Skill skill) => this.skill = skill;

        internal void SetSkill(Skill skill) => this.skill = skill;
    }
}
