using ModifAmorphic.Outward.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace ModifAmorphic.Outward.Modules
{
    internal interface IModifModule
    {
        HashSet<Type> PatchDependencies { get; }
        HashSet<Type> DepsWithMultiLogger { get; }
        HashSet<Type> EventSubscriptions { get; }
    }
}
