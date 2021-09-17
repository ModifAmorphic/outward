using ModifAmorphic.Outward.Modules.Character;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ModifAmorphic.Outward.RespecPotions
{
    public class ForgetSchoolEffect : Effect
    {
        public float AffectQuantity = 10f;
        public float AffectQuantityOnAI = -99999f;
        public bool IsModifier;
        public bool InformSourceCharacter;
        [SerializeField]
        public int SchoolIndex;

        protected override KeyValuePair<string, System.Type>[] GenerateSignature()
        {
            return new KeyValuePair<string, System.Type>[1]
            {
                new KeyValuePair<string, System.Type>("Value", typeof (float))
            };
        }

        public override void SetValue(string[] _data)
        {
            if (_data != null && _data.Length < 1)
                return;
            int.TryParse(_data[0], out this.SchoolIndex);
        }

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
                { }
            }
            
        }
    }
}
