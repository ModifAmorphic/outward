using ModifAmorphic.Outward.Modules.Character;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ModifAmorphic.Outward.RespecPotions.Effects
{
    public class ForgetSchoolEffect : Effect
    {
        [SerializeField]
        public int SchoolIndex;

        protected override void ActivateLocally(Character _affectedCharacter, object[] _infos)
        {
            if (!((UnityEngine.Object)_affectedCharacter != (UnityEngine.Object)null) || !_affectedCharacter.Alive)
                return;

            int breakThroughsReclaimed = 0;
            var schoolService = RespecPotionsPlugin.Services.GetService<SchoolService>();

            if (schoolService.TryForgetSchool((int)SchoolIndex, _affectedCharacter.UID, out var skills, true))
            {
                Startup.Logger.LogDebug($"Forgot {skills.Count()} skills.");
                foreach (var s in skills)
                    Startup.Logger.LogTrace($"Skill {s.Skill.Name} was forgotten. Skill was {(s.IsBreakthrough ? "" : "not")} a breakthrough.");
                var breakthroughs = skills.Count(s => s.IsBreakthrough);

                if (breakthroughs > 0 && schoolService.TryRefundBreakthroughs(_affectedCharacter.UID, breakthroughs, out breakThroughsReclaimed))
                { 
                    //TODO Alert about breakthrough being refunded
                }
            }
            
        }
    }
}
