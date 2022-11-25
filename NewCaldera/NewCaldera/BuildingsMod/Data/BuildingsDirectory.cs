using ModifAmorphic.Outward.Logging;
using System;
using System.IO;

namespace ModifAmorphic.Outward.NewCaldera.Data
{
    internal class BuildingsDirectory : ModDirectory
    {
        protected override string _subModFolder => ModInfo.BuildingsModName;

        public BuildingsDirectory(string modPath, Func<IModifLogger> getLogger) : base(modPath, getLogger)
        {
        }

        
    }
}
