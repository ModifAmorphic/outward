using ModifAmorphic.Outward.Unity.ActionMenus;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace ModifAmorphic.Outward.ActionMenus.Models
{
    internal class ItemCooldown : ICooldown
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
        
        public ItemCooldown(Item item) => skill = item as Skill;
    }
}
