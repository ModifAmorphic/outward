using ModifAmorphic.Outward.Modules.Crafting;

namespace ModifAmorphic.Outward.Extensions
{
    public static class MenusExtensions
    {
        public static MenuIcons TrySetIconNames(this MenuIcons menuIcons, string baseName)
        {
            if (string.IsNullOrEmpty(menuIcons.UnpressedIcon.SpriteName))
                menuIcons.UnpressedIcon.SpriteName = "tex_men_iconsUnpressed" + baseName;
            if (string.IsNullOrEmpty(menuIcons.UnpressedIcon.TextureName))
                menuIcons.UnpressedIcon.TextureName = "tex_men_iconsUnpressed" + baseName;

            if (string.IsNullOrEmpty(menuIcons.HoverIcon.SpriteName))
                menuIcons.HoverIcon.SpriteName = "tex_men_iconsHover" + baseName;
            if (string.IsNullOrEmpty(menuIcons.HoverIcon.TextureName))
                menuIcons.HoverIcon.TextureName = "tex_men_iconsHover" + baseName;

            if (string.IsNullOrEmpty(menuIcons.PressedIcon.SpriteName))
                menuIcons.PressedIcon.SpriteName = "tex_men_iconsPressed" + baseName;
            if (string.IsNullOrEmpty(menuIcons.PressedIcon.TextureName))
                menuIcons.PressedIcon.TextureName = "tex_men_iconsPressed" + baseName;

            return menuIcons;
        }
    }
}
