using BepInEx;
using HarmonyLib;
using System;

namespace ModifAmorphic.Outward.StashPacks
{
    [BepInIncompatibility("com.sinai.sharedstashes")]
    [BepInDependency("com.bepis.bepinex.configurationmanager", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("io.mefino.configurationmanager", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("com.sinai.SideLoader", BepInDependency.DependencyFlags.HardDependency)]
    [BepInPlugin(ModInfo.ModId, ModInfo.ModName, ModInfo.ModVersion)]
    public class StashPool : BaseUnityPlugin
    {
        internal void Awake()
        {
            var harmony = new Harmony(ModInfo.ModId);
            try
            {
                UnityEngine.Debug.Log($"[{ModInfo.ModName}][Info] - Patching...");
                harmony.PatchAll();

                var startup = new Startup();
                UnityEngine.Debug.Log($"[{ModInfo.ModName}][Info] - Starting...");
                startup.Start(this);
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"[{ModInfo.ModName}][Error] Failed to enable {ModInfo.ModId} {ModInfo.ModName}. Exception: {ex}");
                harmony.UnpatchSelf();
                throw;
            }
        }
    }
}
