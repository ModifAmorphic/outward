using BepInEx.Configuration;
using ModifAmorphic.Outward.Shared.Config.Models;
using System;

namespace ModifAmorphic.Outward.Shared.Config.Extensions
{
    public static class ConfigSettingExtensions
    {
        public static ConfigDefinition ToConfigDefinition<T>(this ConfigSetting<T> configSetting)
        {
            return new ConfigDefinition(
                configSetting.Section
                , configSetting.Name);
        }
        public static ConfigDescription ToConfigDescription<T>(this ConfigSetting<T> configSetting, AcceptableValueBase acceptableValues = null)
        {
            return new ConfigDescription(configSetting.Description
                , acceptableValues
                , new ConfigurationManagerAttributes()
                {
                    Order = configSetting.Order
                        ,
                    DispName = string.IsNullOrWhiteSpace(configSetting.DisplayName) ? configSetting.Name : configSetting.DisplayName
                        ,
                    IsAdvanced = configSetting.IsAdvanced
                });
        }
        public static void Hide<T>(this ConfigSetting<T> configSetting)
        {
            if (configSetting.BoundConfigEntry == null)
                throw new InvalidOperationException($"Cannot hide unbound ConfigSettings.  ConfigSetting {configSetting.Name} has not been bound to a Configuration Entry.");
            configSetting.IsVisible = false;
            configSetting.BoundConfigEntry.Description.ConfigurationManagerAttributes().Browsable = configSetting.IsVisible;
#if DEBUG
            UnityEngine.Debug.Log($"[{ModInfo.ModName}] - Hid ConfigSetting {configSetting.Name}. " +
                $"IsVisible = {configSetting.IsVisible}. " +
                $"Browsable = {configSetting.BoundConfigEntry.Description.ConfigurationManagerAttributes().Browsable}.");
#endif
        }
        public static void Show<T>(this ConfigSetting<T> configSetting)
        {
            if (configSetting.BoundConfigEntry == null)
                throw new InvalidOperationException($"Cannot hide unbound ConfigSettings.  ConfigSetting {configSetting.Name} has not been bound to a Configuration Entry.");
            configSetting.IsVisible = true;
            configSetting.BoundConfigEntry.Description.ConfigurationManagerAttributes().Browsable = configSetting.IsVisible;
#if DEBUG
            UnityEngine.Debug.Log($"[{ModInfo.ModName}] - Showed ConfigSetting {configSetting.Name}. " +
                $"IsVisible = {configSetting.IsVisible}. " +
                $"Browsable = {configSetting.BoundConfigEntry.Description.ConfigurationManagerAttributes().Browsable}.");
#endif
        }
    }
}
