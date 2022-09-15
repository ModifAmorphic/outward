using ModifAmorphic.Outward.Unity.ActionMenus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Testing
{
    internal class RandomCooldown : ICooldown
    {
        public bool HasCooldown => true;
        static System.Random random = new System.Random();

        public RandomCooldown()
        {
           
            
        }

        private Func<bool> getIsInCooldown;
        public bool GetIsInCooldown()
        {
            return getIsInCooldown != null ? getIsInCooldown() : false;
        }


        private Func<float> getProgress;
        public float GetProgress()
        {
            return getProgress != null ? getProgress() : 0;
        }

        private Func<float> getSecondsRemaining;
        public float GetSecondsRemaining()
        {
            return getSecondsRemaining != null ? getSecondsRemaining() : 0;
        }

        public void StartCooldown()
        {
            Random random = new Random();
            var cooldown = (float)random.Next(1000, 10000);
            var endTime = DateTime.Now.AddMilliseconds(cooldown);

            getIsInCooldown = () => DateTime.Now < endTime;
            getProgress = () =>
            {
                var msRemain = (float)endTime.Subtract(DateTime.Now).TotalMilliseconds;
                return msRemain / cooldown;
            };

            getSecondsRemaining = () =>
            {
                var remaining = (float)endTime.Subtract(DateTime.Now).TotalMilliseconds / 1000f;
                return remaining;
            };
        }
    }
}
