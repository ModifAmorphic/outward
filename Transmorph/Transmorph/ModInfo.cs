using System;
using System.Collections.Generic;
using System.Text;

namespace ModifAmorphic.Outward.Transmorph
{
    internal static class ModInfo
    {
        public const string ModId = "modifamorphic.outward.transmorph";
        public const string ModName = "Transmorph";
#if PIPELINE
        public const string ModVersion = "${PACKAGE_VERSION}";
#else
        public const string ModVersion = "0.0.1";
#endif
        public const string MinimumConfigVersion = "0.0.1";
    }
}
