using ModifAmorphic.Outward.Internal;
using System.Collections.Generic;

namespace ModifAmorphic.Outward.Extensions
{
    public static class LocalizationManagerExtensions
    {
        public static Dictionary<string, string> GetGeneralLocalizations(this LocalizationManager localizationManager)
        {
            return ReflectUtil.GetReflectedPrivateField<Dictionary<string, string>, LocalizationManager>(LocalizationManagerFieldNames.GeneralLocalizations, localizationManager);
        }
        public static void SetGeneralLocalizations(this LocalizationManager localizationManager, Dictionary<string, string> value)
        {
            ReflectUtil.SetReflectedPrivateField(value, LocalizationManagerFieldNames.GeneralLocalizations, localizationManager);
        }
        static class LocalizationManagerFieldNames
        {
            public const string GeneralLocalizations = "m_generalLocalization";
        }
    }
}
