using BepInEx;
using ModifAmorphic.Outward.Events;
using ModifAmorphic.Outward.ExtraSlots.Config;
using ModifAmorphic.Outward.ExtraSlots.Events;
using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.Modules;
using System;

namespace ModifAmorphic.Outward.ExtraSlots
{
    internal class ExtraSlots
    {

        private IModifLogger _logger => LoggerFactory.GetLogger(ModInfo.ModId);
        private ExtraSlotsSettings _extraSlotsSettings = new ExtraSlotsSettings();

        public void Start(BaseUnityPlugin plugin)
        {
            ExtraSlotsConfigEvents.ExtraSlotsSettingsChanged += (object sender, ExtraSlotsSettings extraSlotsSettings) => _extraSlotsSettings = extraSlotsSettings;

            var configService = new SettingsService(plugin, ModInfo.MinimumConfigVersion);
#if DEBUG
            LoggerFactory.ConfigureLogger(ModInfo.ModId, ModInfo.ModName, LogLevel.Trace);
            _logger.LogDebug($"Registering Event Subscriptions.");
            EventSubscriberService.RegisterSubscriptions(_logger);
#else
            EventSubscriberService.RegisterSubscriptions();
#endif
            configService.Configure();
            _logger.LogDebug($"{ModInfo.ModName} {ModInfo.ModVersion} configured.");

            plugin.Config.ConfigReloaded += (object sender, EventArgs e) => configService.Configure();

            var quickSlotExtender = ModifModules.GetQuickSlotExtenderModule(ModInfo.ModId);
            _logger.LogInfo($"Extending Quickslots by {_extraSlotsSettings.ExtraQuickSlots.Value}.");
            quickSlotExtender.ExtendQuickSlots(_extraSlotsSettings.ExtraQuickSlots.Value, _extraSlotsSettings.ExtraQuickSlotMenuText.Value);

        }
    }
}
