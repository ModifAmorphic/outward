using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace ModifAmorphic.Outward.RespecPotions.Effects
{
    public class BurnHealthEffect : Effect
    {
        [SerializeField]
        public float AffectQuantity = .05f;
        [SerializeField]
        public bool IsModifier = true;

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
            float.TryParse(_data[0], out this.AffectQuantity);
        }

        protected override void ActivateLocally(Character _affectedCharacter, object[] _infos)
        {
            if (!((UnityEngine.Object)_affectedCharacter != (UnityEngine.Object)null) || !_affectedCharacter.Alive || InvinciblePlayersInScene.Invincible)
                return;


            var affectAmount = -1f * this.AffectQuantity * (this.IsModifier ? _affectedCharacter.Stats.MaxHealth : 1f);

            if (affectAmount * -1f >= _affectedCharacter.Stats.CurrentHealth)
                affectAmount = -_affectedCharacter.Stats.CurrentHealth + 1f;

            if (_affectedCharacter.Stats.CurrentHealth == 1)
                affectAmount = -_affectedCharacter.Stats.CurrentHealth;

            Logging.LoggerFactory.GetLogger(ModInfo.ModId).LogTrace($"HEalth Burn - MaxHealth: {_affectedCharacter.Stats.MaxHealth}, " +
                $"CurrentHealth: {_affectedCharacter.Stats.CurrentHealth}, BurntHealth: {_affectedCharacter.Stats.BurntHealth}, affectAmount={affectAmount}");

            _affectedCharacter.Stats.RestoreBurntHealth(affectAmount, false);
        }
    }
}
