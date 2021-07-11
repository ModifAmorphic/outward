using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.Config.Models;
using System;

namespace ModifAmorphic.Outward.ExtraSlots.Config
{
    internal class ExtraSlotsSettings
    {
        const string RestartSection = "**Changes to the # of Slots Require Game Restart**";
        const int RestartTopOrder = int.MaxValue - 1;
        public ConfigSetting<int> ExtraQuickSlots { get; } = new ConfigSetting<int>()
        {
            Name = nameof(ExtraQuickSlots),
            DefaultValue = 3,
            Section = RestartSection,
            DisplayName = "QuickSlots to add",
            Description = "The number of extra quickslots to add.",
            Order = RestartTopOrder,
            IsAdvanced = false
        };
        //UI Alignment Settings
        const string UiSection = "QuickSlot and Stability Bar";
        const int UiTopOrder = RestartTopOrder - 1000;
        public ConfigSetting<bool> CenterQuickSlotPanel { get; } = new ConfigSetting<bool>()
        {
            Name = nameof(CenterQuickSlotPanel),
            DefaultValue = false,
            Section = UiSection,
            DisplayName = "Center QuickSlot bar",
            Description = "Move the quickslot bar to the center of the screen.",
            Order = UiTopOrder,
            IsAdvanced = false
        };
        public ConfigSetting<float> CenterQuickSlot_X_Offset { get; } = new ConfigSetting<float>()
        {
            Name = nameof(CenterQuickSlot_X_Offset),
            DefaultValue = 0f,
            Section = UiSection,
            DisplayName = "    X Offset QuickSlot Bar from center by",
            Description = "Adjust the horizontal position of the quickslot bar when centered.  Offsets the X position of the bar (negative numbers can be used).",
            Order = UiTopOrder - 1,
            IsAdvanced = false
        };
        public ConfigSetting<QuickSlotBarAlignmentOptions> QuickSlotBarAlignmentOption { get; }
            = new ConfigSetting<QuickSlotBarAlignmentOptions>()
            {
                Name = nameof(QuickSlotBarAlignmentOption),
                DefaultValue = QuickSlotBarAlignmentOptions.None,
                Section = UiSection,
                DisplayName = "QuickSlot and Stability Bar Alignment",
                Description = "Alignment of the Quickslot and Stability bar relative to each other.",
                Order = UiTopOrder - 2,
                IsAdvanced = false
            };
        public ConfigSetting<float> MoveStabilityBarUp_Y_Offset { get; } = new ConfigSetting<float>()
        {
            Name = nameof(MoveStabilityBarUp_Y_Offset),
            DefaultValue = -40f,
            Section = UiSection,
            DisplayName = "    Y Offset Stability Bar by",
            Description = $"The distance the stability bar is offset above the Quickslot bar when " +
                $"{Enum.GetName(typeof(QuickSlotBarAlignmentOptions), QuickSlotBarAlignmentOptions.MoveStabilityAboveQuickSlot)} " +
                $"alignment is selected.",
            Order = UiTopOrder - 3,
            IsAdvanced = false
        };
        public ConfigSetting<float> MoveQuickSlotBarUp_Y_Offset { get; } = new ConfigSetting<float>()
        {
            Name = nameof(MoveQuickSlotBarUp_Y_Offset),
            DefaultValue = 37f,
            Section = UiSection,
            DisplayName = "    Y Offset QuickSlot Bar by",
            Description = $"The distance the Quick Slot Bar is offset above the Stability Bar when " +
                $"{Enum.GetName(typeof(QuickSlotBarAlignmentOptions), QuickSlotBarAlignmentOptions.MoveQuickSlotAboveStability)} " +
                $"alignment is selected.",
            Order = UiTopOrder - 4,
            IsAdvanced = false
        };
        const string UiAbsoluteQuickSlotSection = "QuickSlot Bar Position";
        const int uiAbsoluteQsTopOrder = UiTopOrder - 1000;
        public ConfigSetting<float> QuickSlotBarAbsolute_X { get; } = new ConfigSetting<float>()
        {
            Name = nameof(QuickSlotBarAbsolute_X),
            DefaultValue = 0f,
            Section = UiAbsoluteQuickSlotSection,
            DisplayName = "    Bottom Left Corner X Position",
            Description = $"The absolute X position of the bottom left corner of the QuickSlot bar when " +
                $"{Enum.GetName(typeof(QuickSlotBarAlignmentOptions), QuickSlotBarAlignmentOptions.AbsolutePositioning)} " +
                $"alignment is selected, unless the \"Center QuickSlot bar\" option is enabled.",
            Order = uiAbsoluteQsTopOrder - 1,
            IsAdvanced = false
        };
        public ConfigSetting<float> QuickSlotBarAbsolute_Y { get; } = new ConfigSetting<float>()
        {
            Name = nameof(QuickSlotBarAbsolute_Y),
            DefaultValue = 0f,
            Section = UiAbsoluteQuickSlotSection,
            DisplayName = "    Bottom Left Corner Y Position",
            Description = $"The absolute Y position of the bottom left corner of the QuickSlot bar when " +
                $"{Enum.GetName(typeof(QuickSlotBarAlignmentOptions), QuickSlotBarAlignmentOptions.AbsolutePositioning)} " +
                $"alignment is selected.",
            Order = uiAbsoluteQsTopOrder - 2,
            IsAdvanced = false
        };
        const string UiAbsoluteStabilitySection = "Stability Bar Position";
        const int uiAbsoluteStabilityTopOrder = uiAbsoluteQsTopOrder - 1000;
        public ConfigSetting<float> StabilityBarAbsolute_X { get; } = new ConfigSetting<float>()
        {
            Name = nameof(StabilityBarAbsolute_X),
            DefaultValue = 0f,
            Section = UiAbsoluteStabilitySection,
            DisplayName = "    Center X Position",
            Description = $"The absolute X position of the center of the Stability bar when " +
                $"{Enum.GetName(typeof(QuickSlotBarAlignmentOptions), QuickSlotBarAlignmentOptions.AbsolutePositioning)} " +
                $"alignment is selected.",
            Order = uiAbsoluteStabilityTopOrder - 1,
            IsAdvanced = false
        };
        public ConfigSetting<float> StabilityBarAbsolute_Y { get; } = new ConfigSetting<float>()
        {
            Name = nameof(StabilityBarAbsolute_Y),
            DefaultValue = 0f,
            Section = UiAbsoluteStabilitySection,
            DisplayName = "    Bottom Edge Y Position",
            Description = $"The absolute Y position of the bottom edge of the Stability bar when " +
                $"{Enum.GetName(typeof(QuickSlotBarAlignmentOptions), QuickSlotBarAlignmentOptions.AbsolutePositioning)} " +
                $"alignment is selected.",
            Order = uiAbsoluteStabilityTopOrder - 2,
            IsAdvanced = false
        };

        //Advanced Settings
        const string AdvancedSection = "zz--Advanced Settings--zz";
        const int AdvancedTopOrder = UiTopOrder - 1000;

        public ConfigSetting<string> ExtraQuickSlotMenuText { get; } = new ConfigSetting<string>()
        {
            Name = nameof(ExtraQuickSlotMenuText),
            DefaultValue = "Ex Quickslot {ExtraSlotNumber}",
            Section = AdvancedSection,
            DisplayName = "Main Menu Hotkey Label Description",
            Description = $"Label text used under Main Menu, Hotkeys section. **Requires Restart to take effect**",
            Order = AdvancedTopOrder,
            IsAdvanced = true
        };
        public ConfigSetting<LogLevel> LogLevel { get; } = new ConfigSetting<LogLevel>()
        {
            Name = nameof(LogLevel),
            DefaultValue = Logging.LogLevel.Info,
            Section = AdvancedSection,
            DisplayName = "Minimum level for logging",
            Description = $"The threshold for logging events to the UnityEngine.Debug logger. " +
                $"{Enum.GetName(typeof(LogLevel), Logging.LogLevel.Info)} is the default.",
            Order = AdvancedTopOrder - 1,
            IsAdvanced = true
        };

        public ConfigSetting<string> ConfigVersion { get; } = new ConfigSetting<string>()
        {
            Name = nameof(ConfigVersion),
            DefaultValue = ModInfo.ModVersion,
            Section = AdvancedSection,
            DisplayName = "Created with version",
            Description = $"The version of ExtraSlots this configuration file was created for.  **Warning - Changing this could result in resetting all config values.**",
            Order = AdvancedTopOrder - 2,
            IsAdvanced = true
        };
    }
}
