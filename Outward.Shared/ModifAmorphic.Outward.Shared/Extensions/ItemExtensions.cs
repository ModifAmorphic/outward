using System;
using System.Collections.Concurrent;
using System.IO;
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


        //TODO: This doesn't belong here. Rework all this so the extensions don't need a random dictionary stashed in
        //the extensions class.

        private static readonly ConcurrentDictionary<int, Sprite> _itemIcons = new ConcurrentDictionary<int, Sprite>();

        public static Item ConfigureItemIcon(this Item item, string iconPath)
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

        public static string GetSpecialVisualPrefabDefaultPath(this Item item)
        {
            return item.GetPrivateField<Item, string>("m_specialVisualPrefabDefaultPath");
        }
        public static Item SetSpecialVisualPrefabDefaultPath(this Item item, string path)
        {
            item.SetPrivateField<Item, string>("m_specialVisualPrefabDefaultPath", path);
            return item;
        }
        public static string GetSpecialVisualPrefabFemalePath(this Item item)
        {
            return item.GetPrivateField<Item, string>("m_specialVisualPrefabFemalePath");
        }
        public static Item SetSpecialVisualPrefabFemalePath(this Item item, string path)
        {
            item.SetPrivateField<Item, string>("m_specialVisualPrefabFemalePath", path);
            return item;
        }
        public static string GetVisualPrefabName(this Item item)
        {
            return item.GetPrivateField<Item, string>("m_visualPrefabName");
        }
        public static Item SetVisualPrefabName(this Item item, string path)
        {
            item.SetPrivateField<Item, string>("m_visualPrefabName", path);
            return item;
        }
        public static string GetVisualPrefabPath(this Item item)
        {
            return item.GetPrivateField<Item, string>("m_visualPrefabPath");
        }
        public static Item SetVisualPrefabPath(this Item item, string path)
        {
            item.SetPrivateField<Item, string>("m_visualPrefabPath", path);
            return item;
        }
        public static ItemVisual GetLoadedVisual(this Item item)
        {
            return item.GetPrivateField<Item, ItemVisual>("m_loadedVisual");
        }
        public static Item SetLoadedVisual(this Item item, ItemVisual itemVisual)
        {
            item.SetPrivateField<Item, ItemVisual>("m_loadedVisual", itemVisual);
            return item;
        }
    }
}
