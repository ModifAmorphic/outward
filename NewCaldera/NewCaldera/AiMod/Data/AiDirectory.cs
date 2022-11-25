using ModifAmorphic.Outward.Logging;
using System;
using System.IO;

namespace ModifAmorphic.Outward.NewCaldera.Data
{
    internal class AiDirectory : ModDirectory
    {
        protected override string _subModFolder => ModInfo.AiModName;

        public AiDirectory(string modPath, Func<IModifLogger> getLogger) : base(modPath, getLogger)
        {
        }

        
    }
}
