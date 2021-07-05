using ModifAmorphic.Outward.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ModifAmorphic.Outward.Extensions
{
    public static class QuickSlotPanelExtensions
    {
        
        public static bool GetActive(this QuickSlotPanel quickSlotPanel)
        {
            return ReflectUtil.GetReflectedPrivateField<bool, QuickSlotPanel>(QuickSlotPanelPropertyNames.Active, quickSlotPanel);
        }
        
        public static QuickSlotDisplay[] GetQuickSlotDisplays(this QuickSlotPanel quickSlotPanel)
        {
            return ReflectUtil.GetReflectedPrivateField<QuickSlotDisplay[], QuickSlotPanel>(QuickSlotPanelPropertyNames.QuickSlotDisplays, quickSlotPanel);
        }

        public static Character GetLastCharacter(this QuickSlotPanel quickSlotPanel)
        {
            return ReflectUtil.GetReflectedPrivateField<Character, QuickSlotPanel>(QuickSlotPanelPropertyNames.LastCharacter, quickSlotPanel);
        }
        public static void SetLastCharacter(this QuickSlotPanel quickSlotPanel, Character value)
        {
            ReflectUtil.SetReflectedPrivateField(value, QuickSlotPanelPropertyNames.LastCharacter, quickSlotPanel);
        }

        public static bool GetInitialized(this QuickSlotPanel quickSlotPanel)
        {
            return ReflectUtil.GetReflectedPrivateField<bool, QuickSlotPanel>(QuickSlotPanelPropertyNames.Initialized, quickSlotPanel);
        }
        public static void SetInitialized(this QuickSlotPanel quickSlotPanel, bool value)
        {
            ReflectUtil.SetReflectedPrivateField(value, QuickSlotPanelPropertyNames.Initialized, quickSlotPanel);
        }
    }
    static class QuickSlotPanelPropertyNames
    {
        public const string Active = "m_active";
        public const string QuickSlotDisplays = "m_quickSlotDisplays";
        public const string LastCharacter = "m_lastCharacter";
        public const string Initialized = "m_initialized";
    }
}
