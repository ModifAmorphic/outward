using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace ModifAmorphic.Outward.Extensions
{
    public static class SpriteExtensions
    {
        public static Sprite Clone(this Sprite sprite, string filePath)
        {
            return Clone(sprite, filePath, sprite.name, sprite.texture.name);
        }
        public static Sprite Clone(this Sprite sprite, string filePath, string spriteName, string textureName)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"Sprite could not be loaded.", filePath);

            var oldTex = sprite.texture;

            var newtexture = new Texture2D(oldTex.width, oldTex.height, oldTex.format, false);
            newtexture.LoadImage(File.ReadAllBytes(filePath));
            newtexture.hideFlags = HideFlags.None;
            newtexture.anisoLevel = oldTex.anisoLevel;
            newtexture.filterMode = oldTex.filterMode;
            newtexture.wrapMode = oldTex.wrapMode;
            newtexture.name = textureName;

            var oldRect = sprite.rect;
            var newSprite = Sprite.Create(
                newtexture,
                new Rect(oldRect.x, oldRect.y, oldRect.width, oldRect.height),
                new Vector2(sprite.pivot.x, sprite.pivot.y),
                sprite.pixelsPerUnit,
                0,
                SpriteMeshType.FullRect);

            newSprite.name = spriteName;

            return newSprite;
        }
    }
}
