using ModifAmorphic.Outward.Unity.ActionMenus;
using ModifAmorphic.Outward.Unity.ActionUI;
using ModifAmorphic.Outward.Unity.ActionUI.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Testing
{
    internal class RandomActionAssignmentGenerator
    {
        private readonly IHotbarController _hotbar;
        private Sprite[] _slotImages;
        public RandomActionAssignmentGenerator(IHotbarController hotbar) => _hotbar = hotbar;
        static System.Random random = new System.Random();

        public void Generate(Sprite[] slotImages, MonoBehaviour monoBehaviour)
        {
            _slotImages = slotImages;
            foreach (var hotbar in _hotbar.GetActionSlots())
            {
                foreach(var slot in hotbar)
                {
                    var slotAction = GetSlotAction(slot, monoBehaviour);
                    var prefix = "";
                    if (slot.HotbarIndex == 0)
                        prefix = "Shift + ";
                    else if (slot.HotbarIndex == 1)
                        prefix = "Ctrl + ";
                    else if (slot.HotbarIndex == 2)
                        prefix = "Alt + ";
                    
                    var hotkey = prefix + slot.SlotIndex.ToString();
                    //slot.Controller.Configure(GetConfig(hotkey));
                    if (slotAction != null)
                    {
                        slot.Controller.AssignSlotAction(slotAction);
                        slot.ActionButton.onClick.AddListener(() => ((RandomCooldown)slot.SlotAction.Cooldown).StartCooldown());
                        //slot.MouseClickListener.OnRightClick.AddListener(() => ((SampleSlotAction)slotAction).RequestEdit());
                    }
                }
            }
        }
        private ISlotAction GetSlotAction(ActionSlot actionSlot, MonoBehaviour monoBehaviour)
        {
            int noAction = random.Next(0, 10);
            if (noAction < 4)
                return null;
            int imageIndex = random.Next(0, _slotImages.Length - 1);
            var slotAction = new SampleSlotAction()
            {
                ActionIcons = new ActionSlotIcon[1] { new ActionSlotIcon() { Icon = _slotImages[imageIndex], Name = _slotImages[imageIndex].name } },
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
