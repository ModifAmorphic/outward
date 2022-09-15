using Assets.Testing;
using ModifAmorphic.Outward.Unity.ActionMenus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModifAmorphic.Outward.Unity.ActionMenus
{
    internal class SlotConfigService
    {
        public static IActionSlotConfig[,] GetActionSlotConfigs(int hotbars, int rows, int slots)
        {

            var config = new IActionSlotConfig[hotbars, slots * rows];
            for (int b = 0; b < hotbars; b++)
            {
                for (int s = 0; s < slots * rows; s++)
                {
                    config[b, s] = (new TestActionSlotConfig()
                    {
                        ShowZeroStackAmount = false,
                        ShowCooldownTime = false,
                        EmptySlotOption = EmptySlotOptions.Image,
                        HotkeyText = (s + 1).ToString(),
                    });
                }
            }
            return config;
        }
    }
}
