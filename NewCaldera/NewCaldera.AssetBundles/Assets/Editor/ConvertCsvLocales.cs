using Newtonsoft.Json;
using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.OleDb;
using UnityEngine;
using UnityEditor;
using System.IO;
using GenericParsing;
using Newtonsoft.Json.Linq;

public class ConvertCsvLocales
{
    //public static Dictionary<string, string> calderaLocales = new Dictionary<string, string>()
    //{
    //    { "English",  Path.Combine("Assets", "Locales", "Caldera-Foilage", "BuildingLocales_English.csv")},
    //};
    //public static Dictionary<string, string> hallowedLocales = new Dictionary<string, string>()
    //{
    //    { "English",  Path.Combine("Assets", "Locales", "HallowedMarsh-Foilage", "BuildingLocales_English.csv")}
    //};
    //public static Dictionary<string, string> lightsLocales = new Dictionary<string, string>()
    //{
    //    { "English",  Path.Combine("Assets", "Locales", "Outward-Lights", "BuildingLocales_English.csv")}
    //};
    //public static Dictionary<string, Dictionary<string, string>> modLocales = new Dictionary<string, Dictionary<string, string>>()
    //{
    //    { "Caldera-Foilage", calderaLocales },
    //    { "HallowedMarsh-Foilage", hallowedLocales },
    //    { "Outward-Lights", lightsLocales },
    //};

    private readonly static string LocalesPublishDirectory = Path.Combine("..", "BuildingPacks");

    public readonly static string LocalsSourceDirectory = Path.Combine("Assets", "Locales");

    public class LocalesFileInfo
    {
        public string FilePath { get; set; }
        public string Language { get; set; }
    }

    [MenuItem("Assets/Export Locales/Item Descriptions")]
    public static void ExportItemDescriptions()
    {
        var gos = BuildingPrefabsData.GetBuildingsGameObjects();
        //Dictionary<string, Dictionary<string, string>> buildingPackages = new Dictionary<string, Dictionary<string, string>>();

        Dictionary<string, List<LocalesFileInfo>> buildingPackages = new Dictionary<string, List<LocalesFileInfo>>();
        var localesFiles = new List<LocalesFileInfo>();
        foreach (var go in gos)
        {
            buildingPackages.Add(go.name, GetPackLocaleFiles(go.name));
        }

        foreach (var package in buildingPackages)
        {
            foreach (var locale in package.Value)
            {
                var json = ConvertToJson(locale.FilePath);
                var outDir = Path.Combine(LocalesPublishDirectory, package.Key, "Locales", locale.Language);
                if (!Directory.Exists(outDir))
                    Directory.CreateDirectory(outDir);
                File.WriteAllText(Path.Combine(outDir, "BuildingLocalizationsTemplate.json"), json);
                Debug.Log($"Exported {package.Key} pack locale {locale.Language} to {Path.Combine(outDir, "BuildingLocalizationsTemplate.json")}");
            }
        }
    }


    public static string ConvertToJson(string file)
    {
        var items = new JArray();
        using (GenericParser parser = new GenericParser())
        {
            using (FileStream fileStream = new FileStream(file, FileMode.Open, FileAccess.Read))
            {
                using (TextReader reader = new StreamReader(fileStream))
                {
                    parser.SetDataSource(reader);

                    parser.ColumnDelimiter = ',';
                    parser.FirstRowHasHeader = true;
                    parser.SkipStartingDataRows = 0;
                    parser.MaxBufferSize = 4096;
                    parser.MaxRows = 500;
                    parser.TextQualifier = '"';

                    while (parser.Read())
                    {
                        var item = new JObject();
                        item["ItemID"] = parser["ItemID"];
                        item["Name"] = parser["Name"];
                        item["Description"] = parser["Description"];
                        items.Add(item);
                    }
                }
            }
        }

        var templates = new JObject();
        templates["BuildingLocalizationTemplates"] = items;
        return templates.ToString();
    }

    private static List<LocalesFileInfo> GetPackLocaleFiles(string packName)
    {
        var localesFiles = new List<LocalesFileInfo>();

        var localsDir = Path.Combine(LocalsSourceDirectory, packName);
        if (!Directory.Exists(localsDir))
            Directory.CreateDirectory(localsDir);

        var langDirs = Directory.GetDirectories(localsDir);
        if (langDirs.Length == 0)
        {
            string localesFileName = $"{packName}_BuildingLocales_English.csv";
            Directory.CreateDirectory(Path.Combine(localsDir, "English"));
            File.WriteAllText(Path.Combine(localsDir, "English", localesFileName), "ItemID,Name,Description");
            langDirs = Directory.GetDirectories(localsDir);
        }
        foreach (var langPath in langDirs)
        {
            var language = langPath.Split(Path.DirectorySeparatorChar).Last();
            string localesFileName = $"{packName}_BuildingLocales_{language}.csv";
            string langFilePath = Path.Combine(langPath, localesFileName);
            if (!File.Exists(langFilePath))
            {
                File.WriteAllText(langFilePath, "ItemID,Name,Description");
            }
            localesFiles.Add(new LocalesFileInfo()
            {
                FilePath = langFilePath,
                Language = language
            });
        }

        return localesFiles;
    }
}
