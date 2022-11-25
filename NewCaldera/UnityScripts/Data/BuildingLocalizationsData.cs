using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace ModifAmorphic.Outward.UnityScripts.Data
{
    internal class BuildingLocalizationsData
    {
        public List<ItemLocalization> BuildingLocalizationTemplates { get; set; } = new List<ItemLocalization>();

        public static List<ItemLocalization> GetLocalizations(string language, string localesDir)
        {
            //var localesDir = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Locales");
            var templateFile = Path.Combine(localesDir, language, "BuildingLocalizationsTemplate.json");

            if (!File.Exists(templateFile))
            {
                ModifScriptsManager.Instance.Logger.LogDebug($"Localizations file '{templateFile}' not found. Using default localizations.");
                if (language == "English")
                    throw new FileNotFoundException($"Default ItemLocalizations file not found. Expected file at '{templateFile}'", templateFile);
                templateFile = Path.Combine(localesDir, "English", "ItemLocalizationsTemplate.json");
            }

            string json = File.ReadAllText(templateFile);
            return JsonConvert.DeserializeObject<BuildingLocalizationsData>(json).BuildingLocalizationTemplates;
        }
    }
}
