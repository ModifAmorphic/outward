using ModifAmorphic.Outward.StashPacks.Settings;
using System;
using System.Collections.Generic;
using System.Text;

namespace ModifAmorphic.Outward.StashPacks.Extensions
{
    static class StashPackHostSettingsExtentions
    {
        public static void SetChangedValues(this StashPackHostSettings hostSettings, StashPackHostSettings newHostSettings)
        {
            if (hostSettings.AllScenesEnabled != newHostSettings.AllScenesEnabled)
                hostSettings.AllScenesEnabled = newHostSettings.AllScenesEnabled;
            if (hostSettings.DisableBagScalingRotation != newHostSettings.DisableBagScalingRotation)
                hostSettings.DisableBagScalingRotation = newHostSettings.DisableBagScalingRotation;
        }

        public static StashPackHostSettings Clone(this StashPackHostSettings hostSettings) => new StashPackHostSettings()
        {
            AllScenesEnabled = hostSettings.AllScenesEnabled,
            DisableBagScalingRotation = hostSettings.DisableBagScalingRotation
        };

        public static object[] Serialize(this StashPackHostSettings hostSettings) => StashPackHostSettings.Serialize(hostSettings);
        
    }
}
