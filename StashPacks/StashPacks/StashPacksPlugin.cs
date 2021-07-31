using BepInEx;
using HarmonyLib;

namespace ModifAmorphic.Outward.StashPacks
{
    [BepInDependency("com.bepis.bepinex.configurationmanager", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("io.mefino.configurationmanager", BepInDependency.DependencyFlags.SoftDependency)]
    //[BepInDependency("modifamorphic.outward.shared", BepInDependency.DependencyFlags.HardDependency)]
    [BepInPlugin(ModInfo.ModId, ModInfo.ModName, ModInfo.ModVersion)]
    public class StashPool : BaseUnityPlugin
    {
        internal void Awake()
        {
            var harmony = new Harmony(ModInfo.ModId);
            try
            {
                UnityEngine.Debug.Log($"[{ModInfo.ModName}] - Starting...");
                harmony.PatchAll();

                var startup = new Startup();
                startup.Start(this);
            }
            catch
            {
                UnityEngine.Debug.LogError($"Failed to enable {ModInfo.ModId} {ModInfo.ModName}.");
                harmony.UnpatchSelf();
                throw;
            }
        }
    }
}
