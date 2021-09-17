using BepInEx;
using HarmonyLib;
using ModifAmorphic.Outward.Logging;
using System;

namespace ModifAmorphic.Outward.RespecPotions
{
    [BepInDependency("com.bepis.bepinex.configurationmanager", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("io.mefino.configurationmanager", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInPlugin(ModInfo.ModId, ModInfo.ModName, ModInfo.ModVersion)]
    public class RespecPotionsPlugin : BaseUnityPlugin
    {
        internal static ServicesProvider Services;
        internal void Awake()
        {
            IModifLogger logger = null;
            try
            {
                logger = LoggerFactory.ConfigureLogger(ModInfo.ModId, ModInfo.ModName, LogLevel.Info);

                var startup = new Startup();
                logger.LogInfo($"Starting {ModInfo.ModName} {ModInfo.ModVersion}...");
                Services = new ServicesProvider(this);
                startup.Start(Services);
            }
            catch (Exception ex)
            {
                if (logger != null)
                    logger?.LogException($"Failed to enable {ModInfo.ModId} {ModInfo.ModName} {ModInfo.ModVersion}.", ex);
                else
                    UnityEngine.Debug.LogError($"Failed to enable {ModInfo.ModId} {ModInfo.ModName} {ModInfo.ModVersion}. Exception: {ex}");
                throw;
            }
        }
    }
}
