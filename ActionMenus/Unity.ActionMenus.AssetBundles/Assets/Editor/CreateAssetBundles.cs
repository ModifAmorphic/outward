using UnityEditor;
using System.IO;

public class CreateAssetBundles
{
    private readonly static string AssetBundleDirectory = Path.Combine("Assets", "AssetBundles");

    private const string ActionMenusBundleName = "action-menus";
    private readonly static string ActionMenusPublishDirectory = Path.Combine("..", "Assets", "asset-bundles");

    [MenuItem("Assets/Build AssetBundles")]
    static void BuildAllAssetBundles()
    {
        
        if (!Directory.Exists(AssetBundleDirectory))
        {
            Directory.CreateDirectory(AssetBundleDirectory);
        }
        BuildPipeline.BuildAssetBundles(AssetBundleDirectory,
                                        BuildAssetBundleOptions.None,
                                        BuildTarget.StandaloneWindows);
    }

    [MenuItem("Assets/Publish ActionMenus")]
    static void PublishActionMenus()
    {
        BuildAllAssetBundles();
        File.Copy(Path.Combine(AssetBundleDirectory, ActionMenusBundleName), Path.Combine(ActionMenusPublishDirectory, ActionMenusBundleName), true);
    }
}