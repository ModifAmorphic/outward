using BepInEx.Configuration;
using System.Linq;

namespace ModifAmorphic.Outward.Config.Extensions
{
    public static class ConfigDescriptionExtensions
    {
        public static ConfigurationManagerAttributes ConfigurationManagerAttributes(this ConfigDescription configDescription)
        {
            return configDescription.Tags.FirstOrDefault(t =>
                t.GetType() == typeof(ConfigurationManagerAttributes)) as ConfigurationManagerAttributes;
        }
        public static bool TryGetConfigurationManagerAttributes(this ConfigDescription configDescription, out ConfigurationManagerAttributes configurationManagerAttributes)
        {
            configurationManagerAttributes = configDescription.Tags.FirstOrDefault(t =>
                t.GetType() == typeof(ConfigurationManagerAttributes)) as ConfigurationManagerAttributes;

            return configurationManagerAttributes != default;
        }
    }
}
