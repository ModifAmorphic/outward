using ModifAmorphic.Outward.ExtraSlots.Config;
using System;
using System.Collections.Generic;
using System.Text;

namespace ModifAmorphic.Outward.ExtraSlots.Events
{
    static class ExtraSlotsConfigEvents
    {
        public static event EventHandler<ExtraSlotsConfig> ExtraSlotsConfigChanged;
        public static void RaiseExtraSlotsConfigChanged(object sender, ExtraSlotsConfig extraSlotsConfig)
        {
            ExtraSlotsConfigChanged?.Invoke(sender, extraSlotsConfig);
        }

        public static event EventHandler<ExtraSlotsConfig> UiConfigChanged;
        public static void RaiseUiConfigChanged(object sender, ExtraSlotsConfig extraSlotsConfig)
        {
            UiConfigChanged?.Invoke(sender, extraSlotsConfig);
        }

        public static event EventHandler<ExtraSlotsConfig> InternalConfigChanged;
        public static void RaiseInternalConfigChanged(object sender, ExtraSlotsConfig extraSlotsConfig)
        {
            InternalConfigChanged?.Invoke(sender, extraSlotsConfig);
        }

        public static event EventHandler<ExtraSlotsConfig> MainConfigChanged;
        public static void RaiseMainConfigChanged(object sender, ExtraSlotsConfig extraSlotsConfig)
        {
            MainConfigChanged?.Invoke(sender, extraSlotsConfig);
        }

        public static event EventHandler<ExtraSlotsConfig> LoggerConfigChanged;
        public static void RaiseLoggerConfigChanged(object sender, ExtraSlotsConfig extraSlotsConfig)
        {
            LoggerConfigChanged?.Invoke(sender, extraSlotsConfig);
        }
    }
}
