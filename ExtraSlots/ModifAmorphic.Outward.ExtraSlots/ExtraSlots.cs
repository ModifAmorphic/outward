using BepInEx;
using ModifAmorphic.Outward.KeyBindings;
using ModifAmorphic.Outward.ExtraSlots.Patches;
using System.IO;
using ModifAmorphicLogging = ModifAmorphic.Outward.Logging;
using System.Linq;
using System;
using BepInEx.Configuration;
using System.Collections.Generic;
using HarmonyLib;
using ModifAmorphic.Outward.ExtraSlots.Config;
using ModifAmorphic.Outward.Shared.Config;
using ModifAmorphic.Outward.Events;
using ModifAmorphic.Outward.ExtraSlots.Events;

namespace ModifAmorphic.Outward.ExtraSlots
{
    public class ExtraSlots
    {

        private ModifAmorphicLogging.Logger _logger;
        private ExtraSlotsConfig _extraSlotsConfig = new ExtraSlotsConfig();

        public void Enable(BaseUnityPlugin plugin)
        {
            ExtraSlotsConfigEvents.ExtraSlotsConfigChanged += (object sender, ExtraSlotsConfig extraSlotsSettings) => _extraSlotsConfig = extraSlotsSettings;
            ExtraSlotsConfigEvents.LoggerConfigChanged += (object sender, ExtraSlotsConfig extraSlotsSettings) => _logger = new ModifAmorphicLogging.Logger(extraSlotsSettings.LogLevel, ModInfo.ModName);

            var configService = new ConfigService(plugin.Config);
#if DEBUG  //So event subscription invokes get logged when debugging
            configService.Configure();
            _logger.LogDebug($"Registering Event Subscriptions.");
#endif
            EventSubscriberService.RegisterSubscriptions(_logger);
            configService.Configure();
            _logger.LogDebug($"{ModInfo.ModName} {ModInfo.ModVersion} configured.");
            plugin.Config.ConfigReloaded += (object sender, EventArgs e) => configService.Configure();

            _logger.LogInfo($"Extending Quickslots by {_extraSlotsConfig.ExtraQuickSlots}.");
            var quickSlotExtender = new QuickSlotExtender(this._logger);
            quickSlotExtender.ExtendQuickSlots(_extraSlotsConfig.ExtraQuickSlots, _extraSlotsConfig.ExtraQuickSlotMenuText);
        }
    }
}
