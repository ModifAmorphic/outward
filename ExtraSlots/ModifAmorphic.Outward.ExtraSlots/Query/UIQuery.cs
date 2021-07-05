using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ModifAmorphic.Outward.ExtraSlots.Query
{
    class UIQuery
    {
        public static StabilityDisplay_Simple GetCharacterStabilityDisplay(Character character)
        {
            return Resources.FindObjectsOfTypeAll<StabilityDisplay_Simple>().FirstOrDefault(s => s.LocalCharacter.UID == character.UID);
        }
        public static RectTransform GetParentRectTransform(Transform transform)
        {
            int parentNo = 0;
            var parentTransform = transform.parent;
            while (!(transform is RectTransform) && parentNo < 5)
            {
                parentNo++;
                parentTransform = parentTransform.parent;
            }
            if (transform is RectTransform)
                return (RectTransform)parentTransform;

            return null;
        }
    }
}
