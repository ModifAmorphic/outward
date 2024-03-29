﻿using BepInEx.Configuration;

namespace ModifAmorphic.Outward.Config.Extensions
{
    public static class ConfigEntryExtentions
    {
        public static ConfigurationManagerAttributes ConfigAttributes<T>(this ConfigEntry<T> configEntry)
        {
            return configEntry.Description.ConfigurationManagerAttributes();
        }
    }
}
