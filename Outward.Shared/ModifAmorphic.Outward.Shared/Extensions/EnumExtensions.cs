using System;
using System.Collections.Generic;
using System.Linq;

namespace ModifAmorphic.Outward.Extensions
{
    public static class EnumExtensions
    {
        public static string GetName<T>(this T enumValue) where T : Enum
        {
            return Enum.GetName(typeof(T), enumValue);
        }
        public static bool IsDefinedValue<T>(this T enumValue) where T : Enum
        {
            return Enum.IsDefined(typeof(T), enumValue);
        }
        public static bool TryGetValue<T>(this Enum enum1, string name, out T value) where T : struct
        {
            return Enum.TryParse<T>(name, out value);
        }
        public static IEnumerable<T> GetValues<T>(this T enum1) where T : Enum
        {
            return Enum.GetValues(typeof(T)).Cast<T>();
        }
    }
}
