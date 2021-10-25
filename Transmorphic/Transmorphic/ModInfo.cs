using System;
using System.Collections.Generic;
using System.Text;

namespace ModifAmorphic.Outward.Transmorphic
{
    internal static class ModInfo
    {
        public const string ModId = "modifamorphic.outward.transmorphic";
        public const string ModName = "Transmorphic";
#if PIPELINE
        public const string ModVersion = "${PACKAGE_VERSION}";
#else
        public const string ModVersion = "1.0.1";
#endif
        public const string MinimumConfigVersion = "1.0.1";
    }
}
