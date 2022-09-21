using ModifAmorphic.Outward.Unity.ActionUI;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ModifAmorphic.Outward.Unity.ActionMenus
{
    [UnityScriptComponent]
    public class ActionMenuResources : MonoBehaviour
    {
        public Dictionary<string, Sprite> SpriteResources;

        public static ActionMenuResources Instance { get; private set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "<Pending>")]
        void Awake()
        {
            SpriteResources = new Dictionary<string, Sprite>();
            var spriteImagesGo = transform.Find("SpriteImages").gameObject;
            var spriteImages = spriteImagesGo.GetComponentsInChildren<Image>();
            for (int i = 0; i < spriteImages.Length; i++)
            {
                SpriteResources.Add(spriteImages[i].name, spriteImages[i].sprite);
                spriteImages[i].gameObject.SetActive(false);
            }
            Instance = this;
        }
    }
}