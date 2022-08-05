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
        public static ActionSlotConfig[,] GetActionSlotConfigs(int hotbars, int rows, int slots)
        {

            var config = new ActionSlotConfig[hotbars, slots * rows];
            for (int b = 0; b < hotbars; b++)
            {
                for (int s = 0; s < slots * rows; s++)
                {
                    config[b, s] = (new ActionSlotConfig()
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
