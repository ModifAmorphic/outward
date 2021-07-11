using ModifAmorphic.Outward.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace ModifAmorphic.Outward.Modules
{
    internal abstract class ModifModule
    {
        public IModifLogger Logger { get; }

        public ModifModule(IModifLogger modifLogger) => Logger = modifLogger;
    }
}
