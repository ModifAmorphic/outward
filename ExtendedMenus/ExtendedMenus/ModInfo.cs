using System;
using System.Collections.Generic;
using System.Text;

namespace ModifAmorphic.Outward.ExtendedMenus
{
    internal static class ModInfo
    {
        public const string ModId = "modifamorphic.outward.extendedmenus";
        public const string ModName = "ExtendedMenus";
#if PIPELINE
        public const string ModVersion = "${PACKAGE_VERSION}";
#else
        public const string ModVersion = "1.0.0";
#endif
        public const string MinimumConfigVersion = "1.0.0";
    }
}
