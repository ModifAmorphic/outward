using BepInEx;
using ModifAmorphic.Outward.Events;
using ModifAmorphic.Outward.ExtraSlots.Config;
using ModifAmorphic.Outward.ExtraSlots.Events;
using ModifAmorphic.Outward.KeyBindings;
using System;
using ModifAmorphicLogging = ModifAmorphic.Outward.Logging;

namespace ModifAmorphic.Outward.ExtraSlots
{
    internal class ExtraSlots
    {

        private ModifAmorphicLogging.Logger _logger;
        private ExtraSlotsSettings _extraSlotsSettings = new ExtraSlotsSettings();

        public void Start(BaseUnityPlugin plugin)
        {
            ExtraSlotsConfigEvents.ExtraSlotsSettingsChanged += (object sender, ExtraSlotsSettings extraSlotsSettings) => _extraSlotsSettings = extraSlotsSettings;
            ExtraSlotsConfigEvents.LoggerSettingsChanged += (object sender, ExtraSlotsSettings extraSlotsSettings) => _logger = new ModifAmorphicLogging.Logger(extraSlotsSettings.LogLevel.Value, ModInfo.ModName);

            var configService = new ConfigSettingsService(plugin);
#if DEBUG
            _logger = new ModifAmorphicLogging.Logger(ModifAmorphicLogging.LogLevel.Trace, ModInfo.ModName);
            _logger.LogDebug($"Registering Event Subscriptions.");
            EventSubscriberService.RegisterSubscriptions(new ModifAmorphicLogging.Logger(ModifAmorphicLogging.LogLevel.Trace, ModInfo.ModName));
#else
            EventSubscriberService.RegisterSubscriptions();
#endif
            configService.Configure();
            _logger.LogDebug($"{ModInfo.ModName} {ModInfo.ModVersion} configured.");

            plugin.Config.ConfigReloaded += (object sender, EventArgs e) => configService.Configure();

            _logger.LogInfo($"Extending Quickslots by {_extraSlotsSettings.ExtraQuickSlots.Value}.");
            var quickSlotExtender = new QuickSlotExtender(this._logger);
            quickSlotExtender.ExtendQuickSlots(_extraSlotsSettings.ExtraQuickSlots.Value, _extraSlotsSettings.ExtraQuickSlotMenuText.Value);
        }
    }
}
