using ModifAmorphic.Outward.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModifAmorphic.Outward.Extensions
{
    public static class LocalCharacterControlExtensions
    {
        public static Character GetCharacter(this LocalCharacterControl localCharacterControl)
        {
            return ReflectUtil.GetReflectedPrivateField<Character, LocalCharacterControl>(LocalCharacterControlFieldNames.Character, localCharacterControl);
        }
        public static void SetCharacter(this LocalCharacterControl localCharacterControl, Character value)
        {
            ReflectUtil.SetReflectedPrivateField(value, LocalCharacterControlFieldNames.Character, localCharacterControl);
        }

        static class LocalCharacterControlFieldNames
        {
            public const string Character = "m_character";
        }
    }
}
