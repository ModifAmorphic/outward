using System;
using System.Linq;
using UnityEngine;

namespace ModifAmorphic.Outward.ExtraSlots.Query
{
    internal class UIQuery
    {
        public static RectTransform GetCharacterStabilityRect(CharacterUI characterUI)
        {
            const string quickslotPath = "Canvas/GameplayPanels/HUD/Stability";
            if (characterUI.transform.Find(quickslotPath) is RectTransform rectTransform)
                return rectTransform;

            return null;
            //throw new ArgumentException($"Character {character.Name} ({character.UID}) has no Stability Bar at '{quickslotPath}'");
        }
        public static RectTransform GetQuickslotRectTransform(CharacterUI characterUI)
        {
            const string quickslotPath = "Canvas/GameplayPanels/HUD/QuickSlot";
            if (characterUI.transform.Find(quickslotPath) is RectTransform rectTransform)
                return rectTransform;

            return null;
            //throw new ArgumentException($"Character {character.Name} ({character.UID}) has no Quickslot Bar at '{quickslotPath}'");
        }
        //public static RectTransform GetHudRectTransform(CharacterUI characterUI)
        //{
        //    const string hudPath = "Canvas/GameplayPanels/HUD";
        //    if (characterUI.transform.Find(hudPath) is RectTransform rectTransform)
        //        return rectTransform;

        //    return null;
        //    //throw new ArgumentException($"Character {character.Name} ({character.UID}) has no Quickslot Bar at '{quickslotPath}'");
        //}
        //public static RectTransform GetParentRectTransform(Transform transform)
        //{
        //    int parentNo = 0;
        //    var parentTransform = transform.parent;
        //    while (!(transform is RectTransform) && parentNo < 5)
        //    {
        //        parentNo++;
        //        parentTransform = parentTransform.parent;
        //    }
        //    if (transform is RectTransform)
        //        return (RectTransform)parentTransform;

        //    return null;
        //}
    }
}
