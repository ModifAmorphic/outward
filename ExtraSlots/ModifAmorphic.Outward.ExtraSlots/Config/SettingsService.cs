using BepInEx;
using ModifAmorphic.Outward.ExtraSlots.Events;
using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.Config;
using ModifAmorphic.Outward.Config.Extensions;
using ModifAmorphic.Outward.Config.Models;
using System;

namespace ModifAmorphic.Outward.ExtraSlots.Config
{
    internal class SettingsService
    {
        private readonly ConfigManagerService _configManagerService;
        private readonly ConfigSettingsService _configService;
        private readonly string _minConfigVersion;
#if DEBUG
        private readonly Logger _logger = new Logger(LogLevel.Trace, ModInfo.ModName);
#endif
        public SettingsService(BaseUnityPlugin plugin, string minConfigVersion) => 
            (_configManagerService, _configService, _minConfigVersion) = (new ConfigManagerService(plugin), new ConfigSettingsService(plugin), minConfigVersion);

        public void Configure()
        {
            ExtraSlotsSettings esSettings = new ExtraSlotsSettings();

            if (!MeetsMinimumVersion(_minConfigVersion))
            {
#if DEBUG
                _logger.LogInfo($"Minimum version requirement not met. Minumum version is {_minConfigVersion}. Resetting all settings.");
#endif
                _configService.RemoveAllSettings();
            }

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
                    ToggleAlignmentOptions(esSettings.QuickSlotBarAlignmentOption.Value
                        , esSettings.MoveQuickSlotBarUp_Y_Offset
                        , esSettings.MoveStabilityBarUp_Y_Offset
                        , esSettings.QuickSlotBarAbsolute_X
                        , esSettings.QuickSlotBarAbsolute_Y
                        , esSettings.StabilityBarAbsolute_X
                        , esSettings.StabilityBarAbsolute_Y
                        );
                    _configManagerService.RefreshConfigManager();
                    OnUiSettingsChanged(esSettings);
                });

            // Stability Bar Y Offset
            _configService.BindConfigSetting(esSettings.MoveStabilityBarUp_Y_Offset,
                (SettingValueChangedArgs<float> args) => OnUiSettingsChanged(esSettings));

            // Quickslot Bar Y Offset
            _configService.BindConfigSetting(esSettings.MoveQuickSlotBarUp_Y_Offset,
                (SettingValueChangedArgs<float> args) => OnUiSettingsChanged(esSettings));

            // QuickSlot Bar Absolute Postions
            _configService.BindConfigSetting(esSettings.QuickSlotBarAbsolute_X,
                (SettingValueChangedArgs<float> args) => OnUiSettingsChanged(esSettings));
            _configService.BindConfigSetting(esSettings.QuickSlotBarAbsolute_Y,
                (SettingValueChangedArgs<float> args) => OnUiSettingsChanged(esSettings));

            // Stability Bar Absolute Postions
            _configService.BindConfigSetting(esSettings.StabilityBarAbsolute_X,
                (SettingValueChangedArgs<float> args) => OnUiSettingsChanged(esSettings));
            _configService.BindConfigSetting(esSettings.StabilityBarAbsolute_Y,
                (SettingValueChangedArgs<float> args) => OnUiSettingsChanged(esSettings));

            #endregion
            #endregion

            #region Advanced Section
            //advance UI option - change Hotkey label text in main settings menu
            _configService.BindConfigSetting(esSettings.ExtraQuickSlotMenuText, null);

            //Logging Level
            _configService.BindConfigSetting(esSettings.LogLevel,
                (SettingValueChangedArgs<LogLevel> args) => ExtraSlotsConfigEvents.RaiseLoggerSettingsChanged(this, esSettings));

            //The Version the config was originally created with
            _configService.BindConfigSetting(esSettings.ConfigVersion, null);

            #endregion

            //Raise all Events and refresh the ConfigurationManager
            ExtraSlotsConfigEvents.RaiseLoggerSettingsChanged(this, esSettings);
            ExtraSlotsConfigEvents.RaiseMainSettingsChanged(this, esSettings);
            ExtraSlotsConfigEvents.RaiseUiSettingsChanged(this, esSettings);
            ExtraSlotsConfigEvents.RaiseAdvancedSettingsChanged(this, esSettings);
            ExtraSlotsConfigEvents.RaiseExtraSlotsSettingsChanged(this, esSettings);

            //Toggle sub options to correct initial states
            ToggleCenteringOptions(esSettings.CenterQuickSlotPanel.Value, esSettings.CenterQuickSlot_X_Offset);
            ToggleAlignmentOptions(esSettings.QuickSlotBarAlignmentOption.Value
                        , esSettings.MoveQuickSlotBarUp_Y_Offset
                        , esSettings.MoveStabilityBarUp_Y_Offset
                        , esSettings.QuickSlotBarAbsolute_X
                        , esSettings.QuickSlotBarAbsolute_Y
                        , esSettings.StabilityBarAbsolute_X
                        , esSettings.StabilityBarAbsolute_Y
                        );

            //Refresh the window
            _configManagerService.RefreshConfigManager();
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
        //TODO: Fix this mess.  Ugly.
        private void ToggleAlignmentOptions(QuickSlotBarAlignmentOptions alignment
            , ConfigSetting<float> quickSlotBarOffsetY
            , ConfigSetting<float> stabilityBarOffsetY
            , ConfigSetting<float> quickSlotBarAbsoluteX
            , ConfigSetting<float> quickSlotBarAbsoluteY
            , ConfigSetting<float> stabilityBarAbsoluteX
            , ConfigSetting<float> stabilityBarAbsoluteY
            )
        {
#if DEBUG
            _logger.LogTrace($"{nameof(ToggleAlignmentOptions)}: {nameof(alignment)}={alignment}.\n" +
                $"{nameof(stabilityBarOffsetY)}.Name: {stabilityBarOffsetY.Name}. {nameof(stabilityBarOffsetY)}.IsVisible: {stabilityBarOffsetY.IsVisible}\n" +
                $"{nameof(quickSlotBarOffsetY)}.Name:  {quickSlotBarOffsetY.Name}. {nameof(quickSlotBarOffsetY)}.IsVisible: {quickSlotBarOffsetY.IsVisible}\n " +
                $"{nameof(quickSlotBarAbsoluteX)}.Name:  {quickSlotBarAbsoluteX.Name}. {nameof(quickSlotBarAbsoluteX)}.IsVisible: {quickSlotBarAbsoluteX.IsVisible}\n " +
                $"{nameof(quickSlotBarAbsoluteY)}.Name:  {quickSlotBarAbsoluteY.Name}. {nameof(quickSlotBarAbsoluteY)}.IsVisible: {quickSlotBarAbsoluteY.IsVisible}\n " +
                $"{nameof(stabilityBarAbsoluteX)}.Name:  {stabilityBarAbsoluteX.Name}. {nameof(stabilityBarAbsoluteX)}.IsVisible: {stabilityBarAbsoluteX.IsVisible}\n " +
                $"{nameof(stabilityBarAbsoluteY)}.Name:  {stabilityBarAbsoluteY.Name}. {nameof(stabilityBarAbsoluteY)}.IsVisible: {stabilityBarAbsoluteY.IsVisible}"
                );
#endif
            if (QuickSlotBarAlignmentOptions.MoveQuickSlotAboveStability == alignment)
            {
                quickSlotBarOffsetY.Show();
                stabilityBarOffsetY.Hide();
                quickSlotBarAbsoluteX.Hide();
                quickSlotBarAbsoluteY.Hide();
                stabilityBarAbsoluteX.Hide();
                stabilityBarAbsoluteY.Hide();
            }
            else if (QuickSlotBarAlignmentOptions.MoveStabilityAboveQuickSlot == alignment)
            {
                quickSlotBarOffsetY.Hide();
                stabilityBarOffsetY.Show();
                quickSlotBarAbsoluteX.Hide();
                quickSlotBarAbsoluteY.Hide();
                stabilityBarAbsoluteX.Hide();
                stabilityBarAbsoluteY.Hide();
            }
            else if (QuickSlotBarAlignmentOptions.AbsolutePositioning == alignment)
            {
                quickSlotBarOffsetY.Hide();
                stabilityBarOffsetY.Hide();
                quickSlotBarAbsoluteX.Show();
                quickSlotBarAbsoluteY.Show();
                stabilityBarAbsoluteX.Show();
                stabilityBarAbsoluteY.Show();
            }
            else
            {
                quickSlotBarOffsetY.Hide();
                stabilityBarOffsetY.Hide();
                quickSlotBarAbsoluteX.Hide();
                quickSlotBarAbsoluteY.Hide();
                stabilityBarAbsoluteX.Hide();
                stabilityBarAbsoluteY.Hide();
            }
        }
        private bool MeetsMinimumVersion(string minimumVersion)
        {
            var configVersionValue = _configService.PeekSavedConfigValue(new ExtraSlotsSettings().ConfigVersion);
            if (string.IsNullOrWhiteSpace(configVersionValue))
                return false;
            
            if (!Version.TryParse(configVersionValue, out var configVersion)
                || !Version.TryParse(minimumVersion, out var minVersion))
                return false;
#if DEBUG
            _logger.LogDebug($"Current Config {nameof(MeetsMinimumVersion)}? {configVersion.CompareTo(minVersion) >= 0}. Compared: " +
                   $"ConfigVersion: {configVersion} to MinimumVersion: {minVersion}");
#endif
            return configVersion.CompareTo(minVersion) >= 0;

        }
    }
}
