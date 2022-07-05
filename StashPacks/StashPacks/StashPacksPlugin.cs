using BepInEx;
using HarmonyLib;
using ModifAmorphic.Outward.Logging;
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
            IModifLogger logger = null;
            
            var harmony = new Harmony(ModInfo.ModId);
            try
            {
                logger = LoggerFactory.ConfigureLogger(ModInfo.ModId, ModInfo.ModName, LogLevel.Info);
                logger.LogInfo($"Patching...");
                harmony.PatchAll();

                var startup = new Startup();
                logger.LogInfo($"Starting {ModInfo.ModName} {ModInfo.ModVersion}...");
                var servicesProvider = new ServicesProvider(this);
                startup.Start(servicesProvider);
            }
            catch (Exception ex)
            {
                logger?.LogException($"Failed to enable {ModInfo.ModId} {ModInfo.ModName} {ModInfo.ModVersion}.", ex);
                harmony.UnpatchSelf();
                throw;
            }
        }
    }
}
