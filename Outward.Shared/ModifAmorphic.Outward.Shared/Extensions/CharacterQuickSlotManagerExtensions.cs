using ModifAmorphic.Outward.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ModifAmorphic.Outward.Extensions
{
    public static class CharacterQuickSlotManagerExtensions
    {
        public static Transform GetQuickslotTrans(this CharacterQuickSlotManager characterQuickSlotManager)
        {
            return ReflectUtil.GetReflectedPrivateField<Transform, CharacterQuickSlotManager>(CharacterQuickSlotManagerFieldNames.QuickslotTrans, characterQuickSlotManager);
        }
        public static void SetQuickslotTrans(this CharacterQuickSlotManager characterQuickSlotManager, Transform value)
        {
            ReflectUtil.SetReflectedPrivateField(value, CharacterQuickSlotManagerFieldNames.QuickslotTrans, characterQuickSlotManager);
        }

        static class CharacterQuickSlotManagerFieldNames
        {
            public const string QuickslotTrans = "m_quickslotTrans";
        }
        

    }
}
