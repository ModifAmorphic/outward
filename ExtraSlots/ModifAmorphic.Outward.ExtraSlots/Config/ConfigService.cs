using BepInEx.Configuration;
using ModifAmorphic.Outward.ExtraSlots.Events;
using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.Shared.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModifAmorphic.Outward.ExtraSlots.Config
{
    class ConfigService
    {
        private readonly ConfigFile _config;
        public ConfigService(ConfigFile config) => _config = config;

        private ConfigEntry<T> BindConfigEntry<T>(string name, T defaultValue, string section = "", string description = "", int order = 0, bool isAdvanced = false)
        {
            var configDefinition = new ConfigDefinition(section, name);
            var configDescription = new ConfigDescription(description, null, new ConfigurationManagerAttributes { Order = order, IsAdvanced = isAdvanced });
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
        public void Configure()
        {
            var extraConfig = new ExtraSlotsConfig();
            const string restartSection = "**Changes to the # of Slots Require Game Restart**";

            ConfigEntry<int> extraQuickSlotsEntry = BindConfigEntry(nameof(ExtraSlotsConfig.ExtraQuickSlots), extraConfig.ExtraQuickSlots, restartSection, "The number of extra quickslots to add.", int.MaxValue);
            extraConfig.ExtraQuickSlots = extraQuickSlotsEntry.Value;
            extraQuickSlotsEntry.SettingChanged += ((object sender, EventArgs e) =>
            {
                extraConfig.ExtraQuickSlots = GetConfigEntryLatest(extraQuickSlotsEntry).Value;
                ExtraSlotsConfigEvents.RaiseMainConfigChanged(this, extraConfig);
                ExtraSlotsConfigEvents.RaiseExtraSlotsConfigChanged(this, extraConfig);
            });

            #region UI Settings
            const string uiSection = "UI";
            int uiOrder = int.MaxValue;
            //Quickslot Bar Centering
            ConfigEntry<bool> centerQsEntry =
                BindConfigEntry(nameof(ExtraSlotsConfig.CenterQuickSlotPanel), extraConfig.CenterQuickSlotPanel
                , uiSection, "Move the quickslot bar to the middle of the screen."
                , uiOrder);
            extraConfig.CenterQuickSlotPanel = centerQsEntry.Value;
            centerQsEntry.SettingChanged += ((object sender, EventArgs e) =>
            {
                extraConfig.CenterQuickSlotPanel = GetConfigEntryLatest(centerQsEntry).Value;
                ExtraSlotsConfigEvents.RaiseUiConfigChanged(this, extraConfig);
                ExtraSlotsConfigEvents.RaiseExtraSlotsConfigChanged(this, extraConfig);
            });

            //Quickslot & Stability bar Alignment Options
            ConfigEntry<QuickSlotBarAlignmentOptions> alignmentEntry = 
                BindConfigEntry(nameof(ExtraSlotsConfig.QuickSlotBarAlignmentOption), extraConfig.QuickSlotBarAlignmentOption
                , uiSection, "Alignment the quickslot and stability bar relative to each other."
                , uiOrder--);
            extraConfig.QuickSlotBarAlignmentOption = alignmentEntry.Value;
            alignmentEntry.SettingChanged += ((object sender, EventArgs e) =>
            {
                extraConfig.QuickSlotBarAlignmentOption = GetConfigEntryLatest(alignmentEntry).Value;
                ExtraSlotsConfigEvents.RaiseUiConfigChanged(this, extraConfig);
                ExtraSlotsConfigEvents.RaiseExtraSlotsConfigChanged(this, extraConfig);
            });

            ConfigEntry<float> stabilityBarOffsetY = BindConfigEntry(nameof(ExtraSlotsConfig.MoveStabilityBarUp_Y_Offset), extraConfig.MoveStabilityBarUp_Y_Offset
                , uiSection, $"The distance the stability bar is offset above the Quickslot bar when {Enum.GetName(typeof(QuickSlotBarAlignmentOptions), QuickSlotBarAlignmentOptions.MoveStabilityAboveQuickSlot)} alignment is selected."
                , order: uiOrder--
                , isAdvanced: true);
            extraConfig.MoveStabilityBarUp_Y_Offset = stabilityBarOffsetY.Value;
            stabilityBarOffsetY.SettingChanged += ((object sender, EventArgs e) =>
            {
                extraConfig.MoveStabilityBarUp_Y_Offset = GetConfigEntryLatest(stabilityBarOffsetY).Value;
                ExtraSlotsConfigEvents.RaiseUiConfigChanged(this, extraConfig);
                ExtraSlotsConfigEvents.RaiseExtraSlotsConfigChanged(this, extraConfig);
            });

            ConfigEntry<float> quickSlotBarOffsetY = BindConfigEntry(nameof(ExtraSlotsConfig.MoveQuickSlotBarUp_Y_Offset), extraConfig.MoveQuickSlotBarUp_Y_Offset
                , uiSection, $"The distance the Quick Slot Bar is offset above the Stability Bar when {Enum.GetName(typeof(QuickSlotBarAlignmentOptions), QuickSlotBarAlignmentOptions.MoveQuickSlotAboveStability)} alignment is selected."
                , order: uiOrder--
                , isAdvanced: true);
            extraConfig.MoveQuickSlotBarUp_Y_Offset = quickSlotBarOffsetY.Value;
            quickSlotBarOffsetY.SettingChanged += ((object sender, EventArgs e) =>
            {
                extraConfig.MoveQuickSlotBarUp_Y_Offset = GetConfigEntryLatest(quickSlotBarOffsetY).Value;
                ExtraSlotsConfigEvents.RaiseUiConfigChanged(this, extraConfig);
                ExtraSlotsConfigEvents.RaiseExtraSlotsConfigChanged(this, extraConfig);
            });

            ConfigEntry<float> quickSlotXOffset = BindConfigEntry(nameof(ExtraSlotsConfig.CenterQuickSlot_X_Offset), extraConfig.CenterQuickSlot_X_Offset
                , uiSection, $"Can be used to \"fine tune\" the position of the quickslot bar when centered.  Offsets the X position of the bar (negative numbers can be used)."
                , order: uiOrder--
                , isAdvanced: true);
            extraConfig.CenterQuickSlot_X_Offset = quickSlotXOffset.Value;
            quickSlotXOffset.SettingChanged += ((object sender, EventArgs e) =>
            {
                extraConfig.CenterQuickSlot_X_Offset = GetConfigEntryLatest(quickSlotXOffset).Value;
                ExtraSlotsConfigEvents.RaiseUiConfigChanged(this, extraConfig);
                ExtraSlotsConfigEvents.RaiseExtraSlotsConfigChanged(this, extraConfig);
            });

            ConfigEntry<string> qsMenuTextEntry = BindConfigEntry(nameof(ExtraSlotsConfig.ExtraQuickSlotMenuText), extraConfig.ExtraQuickSlotMenuText
                , uiSection, "Hotkey text used for labels under Main Menu, Hotkeys section. **Requires Restart**"
                , order: uiOrder--
                , isAdvanced: true);
            extraConfig.ExtraQuickSlotMenuText = qsMenuTextEntry.Value;
            qsMenuTextEntry.SettingChanged += ((object sender, EventArgs e) =>
            {
                extraConfig.ExtraQuickSlotMenuText = GetConfigEntryLatest(qsMenuTextEntry).Value;
                ExtraSlotsConfigEvents.RaiseUiConfigChanged(this, extraConfig);
                ExtraSlotsConfigEvents.RaiseExtraSlotsConfigChanged(this, extraConfig);
            });
            #endregion

            const string internalSection = "zInternal";
            //Logging Level
            ConfigEntry<LogLevel> qsLogLevelEntry = BindConfigEntry(nameof(ExtraSlotsConfig.LogLevel), extraConfig.LogLevel
                , internalSection, "Minimum log level. Info is default. **Requires Restart**"
                , order: int.MaxValue
                , isAdvanced: true);
            extraConfig.LogLevel = qsLogLevelEntry.Value;
            qsLogLevelEntry.SettingChanged += ((object sender, EventArgs e) =>
            {
                extraConfig.LogLevel = GetConfigEntryLatest(qsLogLevelEntry).Value;
                ExtraSlotsConfigEvents.RaiseLoggerConfigChanged(this, extraConfig);
                ExtraSlotsConfigEvents.RaiseInternalConfigChanged(this, extraConfig);
                ExtraSlotsConfigEvents.RaiseExtraSlotsConfigChanged(this, extraConfig);
            });

            ExtraSlotsConfigEvents.RaiseLoggerConfigChanged(this, extraConfig);
            ExtraSlotsConfigEvents.RaiseMainConfigChanged(this, extraConfig);
            ExtraSlotsConfigEvents.RaiseUiConfigChanged(this, extraConfig);
            ExtraSlotsConfigEvents.RaiseInternalConfigChanged(this, extraConfig);
            ExtraSlotsConfigEvents.RaiseExtraSlotsConfigChanged(this, extraConfig);
        }
    }
}
