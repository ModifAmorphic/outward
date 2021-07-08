using BepInEx.Configuration;
using ModifAmorphic.Outward.Shared.Config;
using System;
using System.Collections.Generic;
using System.Text;

namespace ModifAmorphic.Outward.ExtraSlots.Config
{
    public static class ConfigEntryExtentions
    {
        public static ConfigurationManagerAttributes ConfigAttributes<T>(this ConfigEntry<T> configEntry)
        {
            return ((ConfigurationManagerAttributes)configEntry.Description.Tags[0]);
        }
        public static ConfigurationManagerAttributes ConfigAttributes(this ConfigDescription configDescription)
        {
            return ((ConfigurationManagerAttributes)configDescription.Tags[0]);
        }
    }
}
