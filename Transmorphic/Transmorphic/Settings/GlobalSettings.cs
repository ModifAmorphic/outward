using ModifAmorphic.Outward.Modules.Crafting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ModifAmorphic.Outward.Transmorph.Settings
{
    internal class GlobalSettings
    {
        public static readonly string PluginPath = Path.GetDirectoryName(TransmorphPlugin.Instance.Info.Location);

        public static MenuIcons AlchemyMenuIcons = new MenuIcons()
        {
            UnpressedIcon = new MenuIcon() { IconFilePath = Path.Combine(PluginPath, Path.Combine("assets", "tex_men_iconsUnpressedAlchemy.png")) },
            HoverIcon = new MenuIcon() { IconFilePath = Path.Combine(PluginPath, Path.Combine("assets", "tex_men_iconsHoverAlchemy.png")) },
            PressedIcon = new MenuIcon() { IconFilePath = Path.Combine(PluginPath, Path.Combine("assets", "tex_men_iconsPressedAlchemy.png")) }
        };
        public static MenuIcons CookingMenuIcons = new MenuIcons()
        {
            UnpressedIcon = new MenuIcon() { IconFilePath = Path.Combine(PluginPath, Path.Combine("assets", "tex_men_iconsUnpressedCooking.png")) },
            HoverIcon = new MenuIcon() { IconFilePath = Path.Combine(PluginPath, Path.Combine("assets", "tex_men_iconsHoverCooking.png")) },
            PressedIcon = new MenuIcon() { IconFilePath = Path.Combine(PluginPath, Path.Combine("assets", "tex_men_iconsPressedCooking.png")) }
        };

        public event Action<bool> CookingMenuEnabledChanged;
        private bool _cookingMenuEnabled;
        public bool CookingMenuEnabled
        {
            get => _cookingMenuEnabled;
            set
            {
                var oldValue = _cookingMenuEnabled;
                _cookingMenuEnabled = value;
                if (oldValue != _cookingMenuEnabled)
                    CookingMenuEnabledChanged?.Invoke(_cookingMenuEnabled);
            }
        }

        public event Action<bool> AlchemyMenuEnabledChanged;
        private bool _alchemygMenuEnabled;
        public bool AlchemyMenuEnabled
        {
            get => _alchemygMenuEnabled;
            set
            {
                var oldValue = _alchemygMenuEnabled;
                _alchemygMenuEnabled = value;
                if (oldValue != _alchemygMenuEnabled)
                    AlchemyMenuEnabledChanged?.Invoke(_alchemygMenuEnabled);
            }
        }
    }
}
