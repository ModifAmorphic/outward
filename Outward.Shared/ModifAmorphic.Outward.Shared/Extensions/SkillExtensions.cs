using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace ModifAmorphic.Outward.Extensions
{
    public static class SkillExtensions
    {
        public static float GetCooldownSecondsRemaining(this Skill skill) => skill.RealCooldown * (1f - skill.CoolDownProgress);

        public static float GetCooldownProgress(this Skill skill)
        {
            if (!skill.InCooldown())
                return 0f;

            return Mathf.Clamp01(1f - skill.CoolDownProgress);
        }
    }
}
