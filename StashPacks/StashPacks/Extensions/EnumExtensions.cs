using System;

namespace ModifAmorphic.Outward.StashPacks.Extensions
{
    public static class EnumExtensions
    {
        public static string GetName<T>(this T enumValue) where T : Enum
        {
            return Enum.GetName(typeof(T), enumValue);
        }
    }
}
