using BepInEx;
using HarmonyLib;
using ModifAmorphic.Outward.ActionMenus.Patches;
using ModifAmorphic.Outward.Logging;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace ModifAmorphic.Outward.ActionMenus
{
    [BepInDependency("com.bepis.bepinex.configurationmanager", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("io.mefino.configurationmanager", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInPlugin(ModInfo.ModId, ModInfo.ModName, ModInfo.ModVersion)]
    public class ActionMenusPlugin : BaseUnityPlugin
    {
        internal static ServicesProvider Services => _servicesProvider;
        private static ServicesProvider _servicesProvider;

        internal static BaseUnityPlugin Instance;

        internal void Awake()
        {
            Instance = this;

            IModifLogger logger = null;

            var harmony = new Harmony(ModInfo.ModId);
            try
            {
                logger = LoggerFactory.ConfigureLogger(ModInfo.ModId, ModInfo.ModName, LogLevel.Info);
                logger.LogInfo($"Patching...");

                harmony.PatchAll(typeof(PauseMenuPatches));
                harmony.PatchAll(typeof(SplitPlayerPatches));

                var startup = new Startup();
                logger.LogInfo($"Starting {ModInfo.ModName} {ModInfo.ModVersion}...");
                _servicesProvider = new ServicesProvider(this);
                startup.Start(harmony, _servicesProvider);
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
