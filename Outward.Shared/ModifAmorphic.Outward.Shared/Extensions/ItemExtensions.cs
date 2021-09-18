using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace ModifAmorphic.Outward.Extensions
{
    public static class ItemExtensions
    {
        public static Item SetNames(this Item item, string name)
        {
            item.SetPrivateField("m_name", name);
            item.SetPrivateField("m_localizedName", name);
            item.gameObject.name = $"{item.ItemID}_{item.Name.Replace(" ", "")}";
            return item;
        }
        public static Item SetDescription(this Item item, string description)
        {
            item.SetPrivateField("m_localizedDescription", description);
            return item;
        }
        public static Item SetItemIcon(this Item item, Sprite icon)
        {
            item.SetPrivateField("m_itemIcon", icon);
            return item;
        }
        public static Item ClearEffects(this Item item)
        {
            var effectsHolder = item.transform.Find("Effects");
            var effects = effectsHolder.GetComponents<Effect>();

            foreach (var eff in effects)
            {
                UnityEngine.Object.DestroyImmediate(eff);
            }
            return item;
        }
        public static T AddEffect<T>(this Item item) where T : Effect
        {
            var effectsHolder = item.transform.Find("Effects");
            return effectsHolder.GetOrAddComponent<T>();
        }


        private static readonly ConcurrentDictionary<int, Sprite> _itemIcons = new ConcurrentDictionary<int, Sprite>();

        public static Item ConfigureCustomIcon(this Item item, string iconPath)
        {
            if (item is Skill)
                throw new ArgumentException("Getting or Setting Icons for Skills is currently not supported.", nameof(item));

            if (!File.Exists(iconPath))
                throw new FileNotFoundException($"Item {item.ItemID} - {item.DisplayName}'s Icon could not be set. Icon file not found.", iconPath);
            
            var sprite = item.ItemIcon;
            var texture = item.ItemIcon.texture;

            var newtexture = new Texture2D(texture.height, texture.width, texture.format, texture.mipmapCount > 1);
            newtexture.LoadImage(File.ReadAllBytes(iconPath));
            newtexture.hideFlags = texture.hideFlags;
            newtexture.anisoLevel = texture.anisoLevel;
            newtexture.filterMode = texture.filterMode;
            newtexture.wrapMode = texture.wrapMode;
            newtexture.name = $"tex_men_iconItem{item.Name.Replace(" ", "")}_v_icn";

            var newSprite = Sprite.Create(newtexture, sprite.rect, sprite.pivot, sprite.pixelsPerUnit, 0, SpriteMeshType.FullRect);
            newSprite.name = "icon";
            
            item.ItemIconPath = iconPath;
            item.SetItemIcon(newSprite);

            _itemIcons.AddOrUpdate(item.ItemID, item.ItemIcon);

            return item;
        }
        public static Item SetCustomIcon(this Item item, Sprite icon)
        {
            return item.SetItemIcon(icon);
        }
        public static Sprite GetCustomIcon(this Item item)
        {
            if (item is Skill)
                throw new ArgumentException("Getting or Setting Icons for Skills is currently not supported.", nameof(item));

            if (_itemIcons.TryGetValue(item.ItemID, out var icon))
                return icon;

            return default;
        }
        public static bool TryGetCustomIcon(this Item item, out Sprite icon)
        {
            if (item is Skill)
            {
                icon = default;
                return false;
            }

            return _itemIcons.TryGetValue(item.ItemID, out icon);
        }
    }
}
