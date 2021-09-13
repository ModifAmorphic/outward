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

        private IModifLogger _logger;
        private ExtraSlotsSettings _extraSlotsSettings = new ExtraSlotsSettings();

        public void Start(BaseUnityPlugin plugin)
        {
            ExtraSlotsConfigEvents.ExtraSlotsSettingsChanged += (object sender, ExtraSlotsSettings extraSlotsSettings) => _extraSlotsSettings = extraSlotsSettings;
            ExtraSlotsConfigEvents.LoggerSettingsChanged += (object sender, ExtraSlotsSettings extraSlotsSettings) => _logger = new Logger(extraSlotsSettings.LogLevel.Value, ModInfo.ModName);

            var configService = new SettingsService(plugin, ModInfo.MinimumConfigVersion);
#if DEBUG
            _logger = new Logger(LogLevel.Trace, ModInfo.ModName);
            _logger.LogDebug($"Registering Event Subscriptions.");
            EventSubscriberService.RegisterSubscriptions(new Logger(LogLevel.Trace, ModInfo.ModName));
#else
            EventSubscriberService.RegisterSubscriptions();
#endif
            configService.Configure();
            Internal.ManifestFiles.RemoveOtherVersions(_logger);
            _logger.LogDebug($"{ModInfo.ModName} {ModInfo.ModVersion} configured.");

            plugin.Config.ConfigReloaded += (object sender, EventArgs e) => configService.Configure();

            ModifModules.ConfigureLogging(ModInfo.ModId, () => _logger);
            _logger.LogInfo($"Extending Quickslots by {_extraSlotsSettings.ExtraQuickSlots.Value}.");
            var quickSlotExtender = ModifModules.GetQuickSlotExtenderModule(ModInfo.ModId); 
            quickSlotExtender.ExtendQuickSlots(_extraSlotsSettings.ExtraQuickSlots.Value, _extraSlotsSettings.ExtraQuickSlotMenuText.Value);

        }
    }
}
