using ModifAmorphic.Outward.Internal;
using System.Reflection;

namespace ModifAmorphic.Outward.Extensions
{
    public static class UIElementExtensions
    {
        public static bool GetHideWanted(this UIElement uiElement)
        {
            return ReflectUtil.GetReflectedPrivateField<bool, UIElement>(UIElementFieldNames.HideWanted, uiElement);
        }
        public static void CallOnHide(this UIElement uiElement)
        {
            System.Reflection.MethodInfo method = uiElement.GetType().GetMethod("OnHide", BindingFlags.Instance | BindingFlags.NonPublic);
            if (method != null)
                method.Invoke(uiElement, null);
        }

    }
    static class UIElementFieldNames
    {
        public const string HideWanted = "m_hideWanted";
    }
}
