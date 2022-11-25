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
    public static Dictionary<string, string> calderaLocales = new Dictionary<string, string>()
    {
        { "English",  Path.Combine("Assets", "Locales", "Caldera-Foilage", "BuildingLocales_English.csv")},
    };
    public static Dictionary<string, string> hallowedLocales = new Dictionary<string, string>()
    {
        { "English",  Path.Combine("Assets", "Locales", "HallowedMarsh-Foilage", "BuildingLocales_English.csv")}
    };
    public static Dictionary<string, Dictionary<string, string>> modLocales = new Dictionary<string, Dictionary<string, string>>()
    {
        { "Caldera-Foilage", calderaLocales },
        { "HallowedMarsh-Foilage", hallowedLocales },
    };

    private readonly static string LocalesPublishDirectory = Path.Combine("..", "Assets", "Locales");

    //private static string ItemDescriptionsXlsPath = Path.Combine("Assets", "Locales", "ItemLocales.xslx");
    private static string ItemDescriptionsJsonOutPath = Path.Combine("Assets", "Locales");

    [MenuItem("Assets/Export Locales/Item Descriptions")]
    public static void ExportItemDescriptions()
    {
        var gos = BuildingPrefabsData.GetBuildingsGameObjects();
        Dictionary<string, Dictionary<string, string>> buildingPackages = new Dictionary<string, Dictionary<string, string>>();

        foreach (var go in gos)
        {
            buildingPackages.Add(go.name, modLocales[go.name]);
        }

        foreach (var package in buildingPackages)
        {
            foreach (var locale in package.Value)
            {
                var json = ConvertToJson(locale.Value);
                var outDir = Path.Combine(LocalesPublishDirectory, package.Key, locale.Key);
                if (!Directory.Exists(outDir))
                    Directory.CreateDirectory(outDir);
                File.WriteAllText(Path.Combine(outDir, "BuildingLocalizationsTemplate.json"), json);
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
}
