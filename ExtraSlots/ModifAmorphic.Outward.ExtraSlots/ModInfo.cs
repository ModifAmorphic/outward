﻿using System;
using System.Collections.Generic;
using System.Text;

namespace ModifAmorphic.Outward.ExtraSlots
{
    static class ModInfo
    {
        public const string ModId = "modifamorphic.outward.extraquickslots";
        public const string ModName = "Extra QuickSlots";
#if PIPELINE
        public const string ModVersion = "${PACKAGE_VERSION}";
#else
        public const string ModVersion = "0.2.0";
#endif
    }
}