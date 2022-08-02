using ModifAmorphic.Outward.Logging;

namespace ModifAmorphic.Outward.Modules
{
    internal abstract class ModifModule
    {
        public IModifLogger Logger { get; }

        public ModifModule(IModifLogger modifLogger) => Logger = modifLogger;
    }
}
