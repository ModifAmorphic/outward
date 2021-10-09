using ModifAmorphic.Outward.Extensions;
using ModifAmorphic.Outward.GameObjectResources;
using ModifAmorphic.Outward.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace ModifAmorphic.Outward.Modules.Items
{
    internal class IconService
    {
        private readonly string _modId;
        private readonly Func<IModifLogger> _loggerFactory;
        private IModifLogger Logger => _loggerFactory.Invoke();

        private readonly ModifGoService _modifGoService;
        private readonly GameObject _iconsGo;
        private Transform Icons => _iconsGo.transform;

        private readonly ConcurrentDictionary<string, GameObject> _itemIcons;

        internal IconService(string modId, ModifGoService modifGoService, Func<IModifLogger> loggerFactory)
        {
            this._modId = modId;
            this._loggerFactory = loggerFactory;
            this._modifGoService = modifGoService;
            this._iconsGo = modifGoService.GetModResources(modId, false).GetOrAddComponent<IconResources>().gameObject;
        }

        /// <summary>
        /// Gets or creates a new Icon <see cref="GameObject"/> attached to the <paramref name="itemDisplay"/> with the <paramref name="iconName"/>
        /// </summary>
        /// <param name="itemDisplay">The <see cref="ItemDisplay"/> to get or attach a new <see cref="GameObject"/> to. The <see cref="GameObject"/> contains the icon <see cref="Image"/> component.</param>
        /// <param name="iconName">The name used to find the existing or create new <see cref="GameObject"/>. Also used to name child components.</param>
        /// <param name="iconFilePath">The path to the physical image file.</param>
        /// <param name="activate">Whether or not to immediately activate the <see cref="GameObject"/> after being retrieved or created.</param>
        /// <returns></returns>
        public GameObject GetOrAddIcon(ItemDisplay itemDisplay, string iconName, string iconFilePath, bool activate = false)
        {
            var iconGoName = GetIconGameObjectName(iconName);

            var existing = itemDisplay.transform.Find(iconGoName)?.gameObject;
            if (existing)
            {
                if (activate && !existing.activeSelf)
                    existing.SetActive(true);

                return existing;
            }

            var baseIconGo = GetOrAddBaseIcon(itemDisplay, iconName, iconFilePath);
            var iconGo = UnityEngine.Object.Instantiate(baseIconGo, itemDisplay.transform);

            var imgref = baseIconGo.GetComponent<Image>();
            var image = iconGo.GetComponent<Image>();

            Logger.LogDebug($"{nameof(IconService)}::{nameof(GetOrAddIcon)}(): Setting new image's sprite to existing {imgref.sprite} sprite reference from base icon.");
            image.sprite = imgref.sprite;
            image.name = imgref.name;

            iconGo.DeCloneNames(true);
            if (!iconGo.gameObject.activeSelf && activate)
            {
                iconGo.gameObject.SetActive(true);
            }
            return iconGo;
            
        }
        public bool TryDeactivateIcon(ItemDisplay itemDisplay, string iconName)
        {
            var iconGoName = GetIconGameObjectName(iconName);
            var existing = itemDisplay.transform.Find(iconGoName)?.gameObject;
            if (existing == null || !existing.activeSelf)
            {
                Logger.LogTrace($"{nameof(IconService)}::{nameof(TryDeactivateIcon)}(): No existing icon GameObject '{iconGoName}' found attached, or existing icon's GameObject {existing?.name?? " "}was already deactivated. No action taken.");
                return false;
            }

            existing.SetActive(false);
            Logger.LogDebug($"{nameof(IconService)}::{nameof(TryDeactivateIcon)}(): Deactivated existing icon's GameObject {existing.name}.");
            //UnityEngine.Object.DestroyImmediate(existing);
            
            return true;
        }
        private string GetIconGameObjectName(string iconName)
        {
            var iconGoName = iconName.Length == 1 ? iconName.ToUpper() : iconName.Substring(0, 1).ToUpper() + iconName.Substring(1);
            if (iconGoName.StartsWith("Img"))
                iconGoName = iconName.Length == 3 ? "" :
                    iconGoName.Length > 3 ? iconGoName.Substring(3, 1).ToUpper() + iconGoName.Substring(4) : iconGoName.Substring(3, 1).ToUpper();

            Logger.LogTrace($"{nameof(IconService)}::{nameof(GetIconGameObjectName)}(): {"img" + iconGoName}.");
            return "img" + iconGoName;
        }
        private GameObject GetOrAddBaseIcon(ItemDisplay itemDisplay, string baseIconName, string iconPath)
        {
            var iconName = GetIconGameObjectName(baseIconName);

            var spriteName = $"icon_{baseIconName}Item";
            var textureName = $"tex_men_{baseIconName}Item_v_icn";

            var baseIconGo = Icons.Find(iconName)?.gameObject;
            Logger.LogDebug($"{nameof(IconService)}::{nameof(GetOrAddBaseIcon)}(): Tried to get base game object for icon {iconName}. Got {baseIconGo}.");
            if (baseIconGo)
                return baseIconGo;

            if (!File.Exists(iconPath))
                throw new FileNotFoundException($"Sprite for icon {baseIconName} could not be loaded.", iconPath);

            var enchantedGo = itemDisplay.transform.Find("imgEnchanted").gameObject;
            Logger.LogDebug($"{nameof(IconService)}::{nameof(GetOrAddBaseIcon)}(): Cloned echanting icon gameobject {enchantedGo}.");
            baseIconGo = UnityEngine.Object.Instantiate(enchantedGo, Icons);
            var existingImage = baseIconGo.GetComponent<Image>();

            Logger.LogDebug($"{nameof(IconService)}::{nameof(GetOrAddBaseIcon)}(): Destroying existing image {existingImage.name}.");
            UnityEngine.Object.DestroyImmediate(existingImage);

            baseIconGo.name = iconName;
            var newImage = baseIconGo.AddComponent<Image>();
            newImage.name = iconName;
            newImage.LoadSpriteIcon(iconPath, spriteName, textureName);

            return baseIconGo;
        }
    }
}
