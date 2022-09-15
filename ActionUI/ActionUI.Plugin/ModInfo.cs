using System;
using System.Collections.Generic;
using System.Text;

namespace ModifAmorphic.Outward.UI
{
    internal static class ModInfo
    {
        public const string ModId = "ModifAmorphic.Outward.ActionUI";
        public const string ModName = "ActionUI";
#if PIPELINE
        public const string ModVersion = "${PACKAGE_VERSION}";
#else
        public const string ModVersion = "1.0.0";
#endif
        public const string MinimumConfigVersion = "1.0.0";
    }
}
