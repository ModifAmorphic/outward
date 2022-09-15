using BepInEx;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ModifAmorphic.Outward.Extensions
{
    public static class BaseUnityPluginExtensions
    {
        /// <summary>
        /// Gets the directory the plugin has been installed to. 
        /// </summary>
        /// <param name="baseUnityPlugin"></param>
        /// <returns>The parent directory the plugin is located in.</returns>
        public static string GetPluginDirectory(this BaseUnityPlugin baseUnityPlugin)
        {
            return Path.GetDirectoryName(baseUnityPlugin.Info.Location);
        }
    }
}
