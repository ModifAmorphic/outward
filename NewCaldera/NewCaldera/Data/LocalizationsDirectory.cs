using ModifAmorphic.Outward.Logging;
using System;
using System.IO;

namespace ModifAmorphic.Outward.NewCaldera.Data
{
    internal class LocalizationsDirectory : ModDirectory
    {
        public string CurrentLanguageOverride;
        protected override string _subModFolder
        {
            get
            {
                if (string.IsNullOrWhiteSpace(CurrentLanguageOverride))
                    return Path.Combine("Locales", LocalizationManager.Instance.KnownLanguages[LocalizationManager.Instance.CurrentLanguageIndex]);
                else
                    return Path.Combine("Locales", CurrentLanguageOverride);
            }
        }

        public LocalizationsDirectory(string modPath, Func<IModifLogger> getLogger) : base(modPath, getLogger)
        {
            
        }

        
    }
}
