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
            //Quickslot Bar Centering
            ConfigEntry<bool> centerQsEntry =
                BindConfigEntry(nameof(ExtraSlotsConfig.CenterQuickSlotPanel), extraConfig.CenterQuickSlotPanel
                , uiSection, "Move the quickslot bar to the middle of the screen."
                , int.MaxValue);
            extraConfig.CenterQuickSlotPanel = centerQsEntry.Value;
            centerQsEntry.SettingChanged += ((object sender, EventArgs e) =>
            {
                extraConfig.CenterQuickSlotPanel = GetConfigEntryLatest(centerQsEntry).Value;
                ExtraSlotsConfigEvents.RaiseUiConfigChanged(this, extraConfig);
                ExtraSlotsConfigEvents.RaiseExtraSlotsConfigChanged(this, extraConfig);
            });

            //Quickslot & Stability bar Alignment Options
            ConfigEntry<QuickSlotBarAlignmentOptions> alignmentEntry = 
                BindConfigEntry(nameof(ExtraSlotsConfig.ExtraSlotsAlignmentOption), extraConfig.ExtraSlotsAlignmentOption
                , uiSection, "Alignment the quickslot and stability bar relative to each other."
                , int.MaxValue - 1);
            extraConfig.ExtraSlotsAlignmentOption = alignmentEntry.Value;
            alignmentEntry.SettingChanged += ((object sender, EventArgs e) =>
            {
                extraConfig.ExtraSlotsAlignmentOption = GetConfigEntryLatest(alignmentEntry).Value;
                ExtraSlotsConfigEvents.RaiseUiConfigChanged(this, extraConfig);
                ExtraSlotsConfigEvents.RaiseExtraSlotsConfigChanged(this, extraConfig);
            });

            //Advanced Option - Change Hotkey Label text in Main Menu Hotkeys section.
            ConfigEntry<float> stabilityBarOffsetY = BindConfigEntry(nameof(ExtraSlotsConfig.MoveStabilityBarUp_Y_Offset), extraConfig.MoveStabilityBarUp_Y_Offset
                , uiSection, $"The distance the stability bar is offset above the Quickslot bar when the {Enum.GetName(typeof(QuickSlotBarAlignmentOptions), QuickSlotBarAlignmentOptions.MoveStabilityAboveQuickSlot)} is selected."
                , order: int.MaxValue - 2
                , isAdvanced: true);
            extraConfig.MoveStabilityBarUp_Y_Offset = stabilityBarOffsetY.Value;
            stabilityBarOffsetY.SettingChanged += ((object sender, EventArgs e) =>
            {
                extraConfig.MoveStabilityBarUp_Y_Offset = GetConfigEntryLatest(stabilityBarOffsetY).Value;
                ExtraSlotsConfigEvents.RaiseUiConfigChanged(this, extraConfig);
                ExtraSlotsConfigEvents.RaiseExtraSlotsConfigChanged(this, extraConfig);
            });

            //Advanced Option - Change Hotkey Label text in Main Menu Hotkeys section.
            ConfigEntry<float> quickSlotXOffset = BindConfigEntry(nameof(ExtraSlotsConfig.QuickSlotXOffset), extraConfig.QuickSlotXOffset
                , uiSection, $"Can be used to \"fine tune\" the position of the quickslot bar when centered.  Offsets the X position of the bar (negative numbers can be used)."
                , order: int.MaxValue - 3
                , isAdvanced: true);
            extraConfig.QuickSlotXOffset = quickSlotXOffset.Value;
            quickSlotXOffset.SettingChanged += ((object sender, EventArgs e) =>
            {
                extraConfig.QuickSlotXOffset = GetConfigEntryLatest(quickSlotXOffset).Value;
                ExtraSlotsConfigEvents.RaiseUiConfigChanged(this, extraConfig);
                ExtraSlotsConfigEvents.RaiseExtraSlotsConfigChanged(this, extraConfig);
            });

            ConfigEntry<string> qsMenuTextEntry = BindConfigEntry(nameof(ExtraSlotsConfig.ExtraQuickSlotMenuText), extraConfig.ExtraQuickSlotMenuText
                , uiSection, "Hotkey text used for labels under Main Menu, Hotkeys section. **Requires Restart**"
                , order: int.MaxValue - 2
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
            //ID in which to start adding Quickslots at
            ConfigEntry<int> qsStartingIdDef = BindConfigEntry(nameof(ExtraSlotsConfig.InternalQuickSlotStartingId), extraConfig.InternalQuickSlotStartingId
                , internalSection
                , "For new quickslots, the internal Id that they should start at.  Shouldn't ever need to change unless Outward adds more quickslots or changes the id's quickslots."
                , order: int.MaxValue
                , isAdvanced: true);
            extraConfig.InternalQuickSlotStartingId = qsStartingIdDef.Value;
            qsStartingIdDef.SettingChanged += ((object sender, EventArgs e) =>
            {
                extraConfig.InternalQuickSlotStartingId = GetConfigEntryLatest(qsStartingIdDef).Value;
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
