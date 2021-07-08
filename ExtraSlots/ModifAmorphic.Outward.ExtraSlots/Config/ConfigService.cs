using BepInEx;
using BepInEx.Configuration;
using ModifAmorphic.Outward.ExtraSlots.Events;
using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.Shared.Config;
using System;

namespace ModifAmorphic.Outward.ExtraSlots.Config
{
    class ConfigService
    {
        private readonly BaseUnityPlugin _plugin;
        private readonly ConfigFile _config;
        private readonly ExtraSlotsConfig _extraConfig = new ExtraSlotsConfig();
        public ConfigService(BaseUnityPlugin plugin) => (_plugin, _config) = (plugin, plugin.Config);

        public void Configure()
        {
            const string restartSection = "**Changes to the # of Slots Require Game Restart**";

            ConfigEntry<int> extraQuickSlotsEntry = BindConfigEntry(nameof(ExtraSlotsConfig.ExtraQuickSlots), _extraConfig.ExtraQuickSlots
                , section: restartSection
                , description: "The number of extra quickslots to add."
                , displayName: "QuickSlots to add"
                , order: int.MaxValue);
            _extraConfig.ExtraQuickSlots = extraQuickSlotsEntry.Value;
            extraQuickSlotsEntry.SettingChanged += ((object sender, EventArgs e) =>
            {
                _extraConfig.ExtraQuickSlots = GetConfigEntryLatest(extraQuickSlotsEntry).Value;
                ExtraSlotsConfigEvents.RaiseMainConfigChanged(this, _extraConfig);
                ExtraSlotsConfigEvents.RaiseExtraSlotsConfigChanged(this, _extraConfig);
            });

            #region UI Settings
            const string uiSection = "UI";
            int uiOrder = int.MaxValue;
            #region Quickslot Bar Centering
            //Quickslot Bar Centering
            ConfigEntry<bool> centerQsEntry =
                BindConfigEntry(nameof(ExtraSlotsConfig.CenterQuickSlotPanel), _extraConfig.CenterQuickSlotPanel
                , section: uiSection
                , description: "Move the quickslot bar to the center of the screen."
                , displayName: "Center QuickSlot Bar"
                , order: uiOrder--);
            _extraConfig.CenterQuickSlotPanel = centerQsEntry.Value;
            //Quickslot Bar Centering Offset
            ConfigEntry<float> quickSlotXOffset = BindConfigEntry(nameof(ExtraSlotsConfig.CenterQuickSlot_X_Offset), _extraConfig.CenterQuickSlot_X_Offset
                , section: uiSection
                , description: $"Can be used to \"fine tune\" the position of the quickslot bar when centered.  Offsets the X position of the bar (negative numbers can be used)."
                , displayName: "X Offset QuickSlot Bar from center by"
                , order: uiOrder--);
            _extraConfig.CenterQuickSlot_X_Offset = quickSlotXOffset.Value;
            
            ToggleCenteringOptions(_extraConfig.CenterQuickSlotPanel, quickSlotXOffset);
            
            centerQsEntry.SettingChanged += ((object sender, EventArgs e) =>
            {
                _extraConfig.CenterQuickSlotPanel = GetConfigEntryLatest(centerQsEntry).Value;
                ToggleCenteringOptions(_extraConfig.CenterQuickSlotPanel, quickSlotXOffset);
                ExtraSlotsConfigEvents.RaiseUiConfigChanged(this, _extraConfig);
                ExtraSlotsConfigEvents.RaiseExtraSlotsConfigChanged(this, _extraConfig);
            });
            quickSlotXOffset.SettingChanged += ((object sender, EventArgs e) =>
            {
                _extraConfig.CenterQuickSlot_X_Offset = GetConfigEntryLatest(quickSlotXOffset).Value;
                ExtraSlotsConfigEvents.RaiseUiConfigChanged(this, _extraConfig);
                ExtraSlotsConfigEvents.RaiseExtraSlotsConfigChanged(this, _extraConfig);
            });
            #endregion
            #region Quickslot & Stability bar Alignment Options
            //Quickslot & Stability bar Alignment Options
            ConfigEntry<QuickSlotBarAlignmentOptions> alignmentEntry = 
                BindConfigEntry(nameof(ExtraSlotsConfig.QuickSlotBarAlignmentOption), _extraConfig.QuickSlotBarAlignmentOption
                , section: uiSection
                , description: "Alignment the quickslot and stability bar relative to each other."
                , displayName: "QuickSlot and Stability Bar Alignment"
                , order: uiOrder--);
            _extraConfig.QuickSlotBarAlignmentOption = alignmentEntry.Value;
            // Stability Bar Y Offset
            ConfigEntry<float> stabilityBarOffsetY = BindConfigEntry(nameof(ExtraSlotsConfig.MoveStabilityBarUp_Y_Offset), _extraConfig.MoveStabilityBarUp_Y_Offset
                , section: uiSection
                , description: $"The distance the stability bar is offset above the Quickslot bar when {Enum.GetName(typeof(QuickSlotBarAlignmentOptions), QuickSlotBarAlignmentOptions.MoveStabilityAboveQuickSlot)} alignment is selected."
                , displayName: "Y Offset Stability Bar by"
                , order: uiOrder--);
            _extraConfig.MoveStabilityBarUp_Y_Offset = stabilityBarOffsetY.Value;
            // Quickslot Bar Y Offset
            ConfigEntry<float> quickSlotBarOffsetY = BindConfigEntry(nameof(ExtraSlotsConfig.MoveQuickSlotBarUp_Y_Offset), _extraConfig.MoveQuickSlotBarUp_Y_Offset
                , section: uiSection
                , description: $"The distance the Quick Slot Bar is offset above the Stability Bar when {Enum.GetName(typeof(QuickSlotBarAlignmentOptions), QuickSlotBarAlignmentOptions.MoveQuickSlotAboveStability)} alignment is selected."
                , displayName: "Y Offset QuickSlot Bar by"
                , order: uiOrder--);
            _extraConfig.MoveQuickSlotBarUp_Y_Offset = quickSlotBarOffsetY.Value;
            ToggleAlignmentOptions(_extraConfig.QuickSlotBarAlignmentOption, stabilityBarOffsetY, quickSlotBarOffsetY);

            //Quickslot & Stability bar Alignment Change Events
            alignmentEntry.SettingChanged += ((object sender, EventArgs e) =>
            {
                _extraConfig.QuickSlotBarAlignmentOption = GetConfigEntryLatest(alignmentEntry).Value;
                ToggleAlignmentOptions(_extraConfig.QuickSlotBarAlignmentOption, stabilityBarOffsetY, quickSlotBarOffsetY);
                //configManager.BuildSettingList();
                ExtraSlotsConfigEvents.RaiseUiConfigChanged(this, _extraConfig);
                ExtraSlotsConfigEvents.RaiseExtraSlotsConfigChanged(this, _extraConfig);
            });
            stabilityBarOffsetY.SettingChanged += ((object sender, EventArgs e) =>
            {
                _extraConfig.MoveStabilityBarUp_Y_Offset = GetConfigEntryLatest(stabilityBarOffsetY).Value;
                ExtraSlotsConfigEvents.RaiseUiConfigChanged(this, _extraConfig);
                ExtraSlotsConfigEvents.RaiseExtraSlotsConfigChanged(this, _extraConfig);
            });
            quickSlotBarOffsetY.SettingChanged += ((object sender, EventArgs e) =>
            {
                _extraConfig.MoveQuickSlotBarUp_Y_Offset = GetConfigEntryLatest(quickSlotBarOffsetY).Value;
                ExtraSlotsConfigEvents.RaiseUiConfigChanged(this, _extraConfig);
                ExtraSlotsConfigEvents.RaiseExtraSlotsConfigChanged(this, _extraConfig);
            });
            #endregion
            
            //advance UI option - change Hotkey label text in main settings menu
            ConfigEntry<string> qsMenuTextEntry = BindConfigEntry(nameof(ExtraSlotsConfig.ExtraQuickSlotMenuText), _extraConfig.ExtraQuickSlotMenuText
                , uiSection, "Hotkey text used for labels under Main Menu, Hotkeys section. **Requires Restart**"
                , order: uiOrder--
                , isAdvanced: true);
            _extraConfig.ExtraQuickSlotMenuText = qsMenuTextEntry.Value;
            qsMenuTextEntry.SettingChanged += ((object sender, EventArgs e) =>
            {
                _extraConfig.ExtraQuickSlotMenuText = GetConfigEntryLatest(qsMenuTextEntry).Value;
                ExtraSlotsConfigEvents.RaiseUiConfigChanged(this, _extraConfig);
                ExtraSlotsConfigEvents.RaiseExtraSlotsConfigChanged(this, _extraConfig);
            });
            #endregion

            #region Internal Section
            const string internalSection = "zInternal";
            //Logging Level
            ConfigEntry<LogLevel> qsLogLevelEntry = BindConfigEntry(nameof(ExtraSlotsConfig.LogLevel), _extraConfig.LogLevel
                , internalSection, "Minimum log level. Info is default."
                , order: int.MaxValue
                , isAdvanced: true);
            _extraConfig.LogLevel = qsLogLevelEntry.Value;
            qsLogLevelEntry.SettingChanged += ((object sender, EventArgs e) =>
            {
                _extraConfig.LogLevel = GetConfigEntryLatest(qsLogLevelEntry).Value;
                ExtraSlotsConfigEvents.RaiseLoggerConfigChanged(this, _extraConfig);
                ExtraSlotsConfigEvents.RaiseInternalConfigChanged(this, _extraConfig);
                ExtraSlotsConfigEvents.RaiseExtraSlotsConfigChanged(this, _extraConfig);
            });
            #endregion

            var configManager = _plugin.GetComponent<ConfigurationManager.ConfigurationManager>();
            _config.SettingChanged += ((object sender, SettingChangedEventArgs e) => configManager.BuildSettingList());

            ExtraSlotsConfigEvents.RaiseLoggerConfigChanged(this, _extraConfig);
            ExtraSlotsConfigEvents.RaiseMainConfigChanged(this, _extraConfig);
            ExtraSlotsConfigEvents.RaiseUiConfigChanged(this, _extraConfig);
            ExtraSlotsConfigEvents.RaiseInternalConfigChanged(this, _extraConfig);
            ExtraSlotsConfigEvents.RaiseExtraSlotsConfigChanged(this, _extraConfig);

            
        }
        private void ToggleCenteringOptions(bool quickslotCentered, ConfigEntry<float> quickSlotXOffset)
        {
            ((ConfigurationManagerAttributes)quickSlotXOffset.Description.Tags[0]).Browsable = quickslotCentered;
        }
        private void ToggleAlignmentOptions(QuickSlotBarAlignmentOptions alignment, ConfigEntry<float> stabilityBarOffsetY, ConfigEntry<float> quickSlotBarOffsetY)
        {
            stabilityBarOffsetY.ConfigAttributes().Browsable = QuickSlotBarAlignmentOptions.MoveStabilityAboveQuickSlot == alignment;
            quickSlotBarOffsetY.ConfigAttributes().Browsable = QuickSlotBarAlignmentOptions.MoveQuickSlotAboveStability == alignment;
        }
        private ConfigEntry<T> BindConfigEntry<T>(string name, T defaultValue, string section = "", string description = "", string displayName = "", int order = 0, bool isAdvanced = false)
        {
            var configDefinition = new ConfigDefinition(section, name);
            var configDescription = new ConfigDescription(description, null, new ConfigurationManagerAttributes
            {
                Order = order
                , DispName = string.IsNullOrWhiteSpace(displayName) ? name : displayName
                , IsAdvanced = isAdvanced
            });
            if (!_config.TryGetEntry<T>(configDefinition, out var configEntry))
            {
                configEntry = _config.Bind<T>(configDefinition, defaultValue, configDescription);
            }
            return configEntry;
        }
        private ConfigEntry<T> GetConfigEntryLatest<T>(ConfigEntry<T> configEntry)
        {
            var configDefinition = new ConfigDefinition(configEntry.Definition.Section, configEntry.Definition.Key);
            _config.TryGetEntry<T>(configDefinition, out var latestConfigEntry);
            return latestConfigEntry;
        }
    }
}
