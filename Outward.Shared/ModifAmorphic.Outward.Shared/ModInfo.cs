using System;
using System.Collections.Generic;
using System.Text;

namespace ModifAmorphic.Outward.Shared
{
    public static class ModInfo
    {
        public const string ModId = "modifamorphic.outward.shared";
        public const string ModName = "ModifAmorphic Shared Library";
#if PIPELINE
        public const string ModVersion = "${PACKAGE_VERSION}";
#else
        public const string ModVersion = "0.2.0";
#endif
    }
}
