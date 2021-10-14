using BepInEx;
using HarmonyLib;
using ModifAmorphic.Outward.ExtraSlots.Patches;
using ModifAmorphic.Outward.Logging;
using System;

namespace ModifAmorphic.Outward.ExtraSlots
{
    [BepInDependency("com.sinai.SideLoader", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("com.bepis.bepinex.configurationmanager", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("io.mefino.configurationmanager", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInPlugin(ModInfo.ModId, ModInfo.ModName, ModInfo.ModVersion)]
    public class ExtraSlotsPlugin : BaseUnityPlugin
    {
        internal void Awake()
        {
            IModifLogger logger = null;
            var harmony = new Harmony(ModInfo.ModId);
            try
            {
                logger = LoggerFactory.ConfigureLogger(ModInfo.ModId, ModInfo.ModName, LogLevel.Info);
                logger.LogInfo($"Patching...");
                harmony.PatchAll(typeof(QuickSlotPanelPatches));

                var extraSlots = new ExtraSlots();
                logger.LogInfo($"Starting {ModInfo.ModName} {ModInfo.ModVersion}...");
                extraSlots.Start(this);
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
