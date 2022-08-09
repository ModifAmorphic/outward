using ModifAmorphic.Outward.Unity.ActionMenus;
using ModifAmorphic.Outward.Unity.ActionMenus.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Testing
{
    internal class RandomSlotActionGenerator
    {
        private readonly Sprite[] _slotImages;
        private readonly string[] _slotNames;
        private readonly MonoBehaviour _monoBehaviour;

        public RandomSlotActionGenerator(Sprite[] slotImages, string[] slotNames, MonoBehaviour monoBehaviour) => (_slotImages, _slotNames, _monoBehaviour) = (slotImages, slotNames, monoBehaviour);

        static System.Random random = new System.Random();

        public List<ISlotAction> Generate(int amount)
        {
            var actions = new List<ISlotAction>();
            for (int i = 0; i < amount; i++)
            { 
                var slotAction = GetSlotAction(_monoBehaviour);
                actions.Add(slotAction);
            }
            return actions;
        }
        private ISlotAction GetSlotAction(MonoBehaviour monoBehaviour)
        {
            int imageIndex = random.Next(0, _slotImages.Length - 1);
            int nameIndex = random.Next(0, _slotNames.Length - 1);
            var slotAction = new SampleSlotAction()
            {
                DisplayName = _slotNames[nameIndex],
                ActionIcon = _slotImages[imageIndex],
                Cooldown = new RandomCooldown(),
                Stack = new RandomStack(monoBehaviour)
            };
            
            return slotAction;

        }
        private TestActionSlotConfig GetConfig(string hotkey)
        {
            int showZero = random.Next(1, 100);
            int showTime = random.Next(1, 100);
            int showImage = random.Next(1, 100);

            return new TestActionSlotConfig()
            {
                ShowZeroStackAmount = showZero > 50,
                ShowCooldownTime = showTime > 50,
                EmptySlotOption = showImage > 50 ? EmptySlotOptions.Transparent : EmptySlotOptions.Image,
                HotkeyText = hotkey,
            };
        }
    }
}
