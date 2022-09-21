using ModifAmorphic.Outward.Unity.ActionUI;
using UnityEngine;

namespace ModifAmorphic.Outward.UI.Models
{
    internal class ItemCooldownTracker : ICooldown
    {
        private readonly Skill skill;

        public bool HasCooldown => skill != null;

        public bool GetIsInCooldown() => skill?.InCooldown() ?? false;

        public float GetProgress()
        {
            if (!GetIsInCooldown())
                return 0f;

            return Mathf.Clamp01(1f - skill.CoolDownProgress);
        }

        public float GetSecondsRemaining() => skill.RealCooldown * (1f - skill.CoolDownProgress);

        public ItemCooldownTracker(Item item) => skill = item as Skill;
    }
}
