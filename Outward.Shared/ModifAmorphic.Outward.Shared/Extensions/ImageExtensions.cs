using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace ModifAmorphic.Outward.Extensions
{
    public static class ImageExtensions
    {
        public static Image LoadSpriteIcon(this Image image, string pngFilePath, string spriteName, string textureName)
        {
            if (!File.Exists(pngFilePath))
                throw new FileNotFoundException($"Sprite {spriteName} could not be loaded.", pngFilePath);

            var newtexture = new Texture2D(128, 84, TextureFormat.DXT5, false);
            newtexture.LoadImage(File.ReadAllBytes(pngFilePath));
            newtexture.hideFlags = HideFlags.None;
            newtexture.anisoLevel = 1;
            newtexture.filterMode = FilterMode.Bilinear;
            newtexture.wrapMode = TextureWrapMode.Clamp;
            newtexture.name = textureName;

            var newSprite = Sprite.Create(
                newtexture,
                new Rect(0f, 0f, 84f, 128f), new Vector2(42f, 64f),
                100,
                0,
                SpriteMeshType.FullRect);

            newSprite.name = spriteName;

            image.sprite = newSprite;

            return image;
        }
    }
}
