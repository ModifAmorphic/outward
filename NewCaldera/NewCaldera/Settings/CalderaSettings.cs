using System.IO;

namespace ModifAmorphic.Outward.NewCaldera.Settings
{
    internal class CalderaSettings
    {
        public static readonly string PluginPath = Path.GetDirectoryName(NewCalderaPlugin.Instance.Info.Location);
        public static readonly string BepInExConfigPath = Path.GetDirectoryName(NewCalderaPlugin.Instance.Config.ConfigFilePath);
        public static readonly string ModConfigPath = Path.Combine(BepInExConfigPath, ModInfo.ModId);
    }
}
