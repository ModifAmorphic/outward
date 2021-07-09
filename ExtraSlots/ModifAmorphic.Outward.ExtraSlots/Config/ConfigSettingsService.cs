using BepInEx;
using ModifAmorphic.Outward.ExtraSlots.Events;
using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.Shared.Config;
using ModifAmorphic.Outward.Shared.Config.Models;


namespace ModifAmorphic.Outward.ExtraSlots.Config
{
    internal class ConfigSettingsService
    {
        private readonly ConfigManagerService _configManagerService;
        private readonly ConfigService _configService;
#if DEBUG
        private readonly Logger _logger = new Logger(LogLevel.Trace, ModInfo.ModName);
#endif
        public ConfigSettingsService(BaseUnityPlugin plugin) => (_configManagerService, _configService) = (new ConfigManagerService(plugin), new ConfigService(plugin));

        public void Configure()
        {
            ExtraSlotsSettings esSettings = new ExtraSlotsSettings();

            _configService.BindConfigSetting(esSettings.ExtraQuickSlots,
                (SettingValueChangedArgs<int> args) =>
                {
                    ExtraSlotsConfigEvents.RaiseMainSettingsChanged(this, esSettings);
                    ExtraSlotsConfigEvents.RaiseExtraSlotsSettingsChanged(this, esSettings);
                });

            #region UI Settings
            #region Quickslot Bar Centering
            //Quickslot Bar Centering
            _configService.BindConfigSetting(esSettings.CenterQuickSlotPanel,
                (SettingValueChangedArgs<bool> args) =>
                {
                    ToggleCenteringOptions(esSettings.CenterQuickSlotPanel.Value, esSettings.CenterQuickSlot_X_Offset);
                    OnUiSettingsChanged(esSettings);
                });

            //Quickslot Bar Centering Offset
            _configService.BindConfigSetting(esSettings.CenterQuickSlot_X_Offset,
                (SettingValueChangedArgs<float> args) => OnUiSettingsChanged(esSettings));
            #endregion
            #region Quickslot & Stability bar Alignment Options
            //Quickslot & Stability bar Alignment Options
            _configService.BindConfigSetting(esSettings.QuickSlotBarAlignmentOption,
                (SettingValueChangedArgs<QuickSlotBarAlignmentOptions> args) =>
                {
                    ToggleAlignmentOptions(esSettings.QuickSlotBarAlignmentOption.Value, esSettings.MoveStabilityBarUp_Y_Offset, esSettings.MoveQuickSlotBarUp_Y_Offset);
                    OnUiSettingsChanged(esSettings);
                });

            // Stability Bar Y Offset
            _configService.BindConfigSetting(esSettings.MoveStabilityBarUp_Y_Offset,
                (SettingValueChangedArgs<float> args) => OnUiSettingsChanged(esSettings));

            // Quickslot Bar Y Offset
            _configService.BindConfigSetting(esSettings.MoveQuickSlotBarUp_Y_Offset,
                (SettingValueChangedArgs<float> args) => OnUiSettingsChanged(esSettings));
            #endregion
            #endregion

            #region Advanced Section
            //advance UI option - change Hotkey label text in main settings menu
            _configService.BindConfigSetting(esSettings.ExtraQuickSlotMenuText, null);

            //Logging Level
            _configService.BindConfigSetting(esSettings.LogLevel,
                (SettingValueChangedArgs<LogLevel> args) => ExtraSlotsConfigEvents.RaiseLoggerSettingsChanged(this, esSettings));
            #endregion

            //Raise all Events and refresh the ConfigurationManager
            ExtraSlotsConfigEvents.RaiseLoggerSettingsChanged(this, esSettings);
            ExtraSlotsConfigEvents.RaiseMainSettingsChanged(this, esSettings);
            ExtraSlotsConfigEvents.RaiseUiSettingsChanged(this, esSettings);
            ExtraSlotsConfigEvents.RaiseAdvancedSettingsChanged(this, esSettings);
            ExtraSlotsConfigEvents.RaiseExtraSlotsSettingsChanged(this, esSettings);

            //Toggle sub options to correct initial states
            ToggleCenteringOptions(esSettings.CenterQuickSlotPanel.Value, esSettings.CenterQuickSlot_X_Offset);
            ToggleAlignmentOptions(esSettings.QuickSlotBarAlignmentOption.Value, esSettings.MoveStabilityBarUp_Y_Offset, esSettings.MoveQuickSlotBarUp_Y_Offset);

            //Refresh the window
            //_configManagerService.RefreshConfigManager();
        }
        private void OnUiSettingsChanged(ExtraSlotsSettings esSettings)
        {
            ExtraSlotsConfigEvents.RaiseUiSettingsChanged(this, esSettings);
            ExtraSlotsConfigEvents.RaiseExtraSlotsSettingsChanged(this, esSettings);
        }
        private void ToggleCenteringOptions(bool quickslotCentered, ConfigSetting<float> quickSlotXOffset)
        {
#if DEBUG
            _logger.LogTrace($"{nameof(ToggleCenteringOptions)}: {nameof(quickslotCentered)} = {quickslotCentered}.\n" +
                $"{nameof(quickSlotXOffset)}.Name: {quickSlotXOffset.Name}. {nameof(quickSlotXOffset)}.IsVisible: {quickSlotXOffset.IsVisible}");
#endif
            if (quickslotCentered)
                _configService.ShowSettingAndRefresh(quickSlotXOffset);
            else
                _configService.HideSettingAndRefresh(quickSlotXOffset);
        }
        private void ToggleAlignmentOptions(QuickSlotBarAlignmentOptions alignment, ConfigSetting<float> stabilityBarOffsetY, ConfigSetting<float> quickSlotBarOffsetY)
        {
#if DEBUG
            _logger.LogTrace($"{nameof(ToggleAlignmentOptions)}: {nameof(alignment)}={alignment}.\n" +
                $"{nameof(stabilityBarOffsetY)}.Name: {stabilityBarOffsetY.Name}. {nameof(stabilityBarOffsetY)}.IsVisible: {stabilityBarOffsetY.IsVisible}\n" +
                $"{nameof(quickSlotBarOffsetY)}.Name:  {quickSlotBarOffsetY.Name}. {nameof(quickSlotBarOffsetY)}.IsVisible: {quickSlotBarOffsetY.IsVisible}");
#endif
            if (QuickSlotBarAlignmentOptions.MoveQuickSlotAboveStability == alignment)
            {
                _configService.ShowSettingAndRefresh(quickSlotBarOffsetY);
                _configService.HideSettingAndRefresh(stabilityBarOffsetY);
            }
            else if (QuickSlotBarAlignmentOptions.MoveStabilityAboveQuickSlot == alignment)
            {
                _configService.ShowSettingAndRefresh(stabilityBarOffsetY);
                _configService.HideSettingAndRefresh(quickSlotBarOffsetY);
            }
            else
            {
                _configService.HideSettingAndRefresh(stabilityBarOffsetY);
                _configService.HideSettingAndRefresh(quickSlotBarOffsetY);
            }
        }
    }
}
