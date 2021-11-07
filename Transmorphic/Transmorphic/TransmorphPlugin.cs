using BepInEx;
using HarmonyLib;
using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.Transmorphic.Patches;
using System;
using System.Collections.Generic;
using System.Text;

namespace ModifAmorphic.Outward.Transmorphic
{
    [BepInDependency("com.bepis.bepinex.configurationmanager", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("io.mefino.configurationmanager", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("com.sinai.SideLoader", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInPlugin(ModInfo.ModId, ModInfo.ModName, ModInfo.ModVersion)]
    public class TransmorphPlugin : BaseUnityPlugin
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

                harmony.PatchAll(typeof(TransmorphNetworkLevelLoaderPatches));
                harmony.PatchAll(typeof(TransmorphRecipeManagerPatches));
                harmony.PatchAll(typeof(TmogCharacterRecipeKnowledgePatches));
                harmony.PatchAll(typeof(TmogCharacterEquipmentPatches));
                harmony.PatchAll(typeof(TmogItemManagerPatches));
                harmony.PatchAll(typeof(EnchantCharacterRecipeKnowledgePatches));

                //harmony.PatchAll(typeof(WeaponPatches));
                //harmony.PatchAll(typeof(TmogItemDisplayPatches));
                
                //harmony.PatchAll(typeof(TransmogCraftingMenuPatches));
                //harmony.PatchAll(typeof(CharacterUIPatches));
                //harmony.PatchAll(typeof(CraftingMenuPatches));
                //harmony.PatchAll(typeof(SplitScreenManagerPatches));
                //harmony.PatchAll(typeof(SplitPlayerPatches));

                var startup = new Startup();
                logger.LogInfo($"Starting {ModInfo.ModName} {ModInfo.ModVersion}...");
                _servicesProvider = new ServicesProvider(this);
                startup.Start(_servicesProvider);
            }
            catch (Exception ex)
            {
                logger?.LogException($"Failed to enable {ModInfo.ModId} {ModInfo.ModName} {ModInfo.ModVersion}.", ex);
                harmony.UnpatchSelf();
                throw;
            }
        }
        private Modules.Crafting.CustomCraftingModule GetCustomCraftingModule()
        {
            return _servicesProvider.GetService<Modules.Crafting.CustomCraftingModule>();
        }
        private Modules.Items.ItemVisualizer GetItemVisualizer()
        {
            return _servicesProvider.GetService<Modules.Items.ItemVisualizer>();
        }
    }
}
