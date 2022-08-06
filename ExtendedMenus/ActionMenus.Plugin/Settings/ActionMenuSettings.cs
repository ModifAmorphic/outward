﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ModifAmorphic.Outward.ActionMenus.Settings
{
    internal class ActionMenuSettings
    {
        public const string GameObjectName = "MenuOverhaul";

        public static readonly string PluginPath = Path.GetDirectoryName(ActionMenusPlugin.Instance.Info.Location);
        public static readonly string ConfigPath = Path.GetDirectoryName(ActionMenusPlugin.Instance.Config.ConfigFilePath);
        public static readonly string ProfilesPath = Path.Combine(ConfigPath, ModInfo.ModId, "profiles");

        public static class ActionViewer
        {
            public const string SkillsTab = "Skills";
            public const string ConsumablesTab = "Consumables";
            public const string DeployablesTab = "Deployables";
            public const string EquippedTab = "Equipped";
            public const string ArmorTab = "Armor";
            public const string WeaponsTab = "Weapons";
        }
    }
}
