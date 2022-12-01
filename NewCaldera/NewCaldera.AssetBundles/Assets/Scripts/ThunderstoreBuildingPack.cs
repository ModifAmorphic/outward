using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;

[JsonObject(MemberSerialization.OptIn)]
public class ThunderstoreBuildingPack : MonoBehaviour
{
    [Header("Thunderstore Fields")]
    [Tooltip("Author / Team. Prefixes Mod Name: {Namespace}-{ModName}")]
    [JsonProperty("namespace")]
    public string Namespace;
    
    [Tooltip("Name for the mod / pack.")]
    [JsonProperty("name")]
    public string ModName;
    
    [TextArea]
    [Tooltip("Short description.")]
    [JsonProperty("description")]
    public string Description;
    
    [JsonProperty("version_number")]
    public string VersionNumber = "1.0.0";
    
    [Tooltip("Mod dependencies that should be installed for this mod pack to work. \nExample - \"ModifAmorphic-NewCaldera-1.0.0\" ")]
    [JsonProperty("dependencies")]
    public List<string> Dependencies = new List<string>() { "Namespace-ModName-1.0.0" };
    
    [JsonProperty("website_url")]
    public string ModWebsiteUri;
    
    [Header("Publish Settings")]
    [Tooltip("The root directory where this packs files will be published. Files will be published to a {Namespace}-{ModName} subdirectory ")]
    public string PublishDirectory = @"..\BuildingPacks";
    
    [Tooltip("The directory where this packs locales folders will be published under the PublishDirectory.")]
    public string LocalesDirectory = "Locales";
    
    [Tooltip("The directory where this packs asset-bundle will be published under the PublishDirectory.")]
    public string AssetBundleDirectory = "asset-bundles";
    
    public bool CreateZip = false;

    public string FullName => Namespace + "-" + ModName;
    public string AssetFilePath => AssetBundleDirectory + "/" + FullName.ToLower();
    public string GemeratedPrefabsPath => "Assets/" + Namespace + "/" + ModName + "/Prefabs";
    public string GeneratedItemsPath => "Assets/" + Namespace + "/" + ModName + "/Prefabs/Items";
    public string GeneratedVisualsPath => "Assets/" + Namespace + "/" + ModName + "/Prefabs/Visuals";

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public string GetOrCreatePackPath()
    {
        string publishRoot = Path.Combine(PublishDirectory, FullName);
        if (Directory.Exists(publishRoot))
        {
            Directory.CreateDirectory(publishRoot);
        }

        return publishRoot;
    }

    public string GetOrCreatePackPluginsPath()
    {
        string publishRoot = Path.Combine(PublishDirectory, FullName);
        string publishPath = Path.Combine(publishRoot, "files", "plugins");
        if (Directory.Exists(publishPath))
        {
            Directory.CreateDirectory(publishPath);
        }

        return publishPath;
    }
}
