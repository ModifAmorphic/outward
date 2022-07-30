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
    public class ExtendedMenusPlugin : BaseUnityPlugin
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

                //var mapHelperconstructor = typeof(Rewired.Player.ControllerHelper.MapHelper)
                //    .GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic).FirstOrDefault();
                //var postfix = typeof(MapHelperConstructorPatch).GetMethod("Postfix", BindingFlags.Public | BindingFlags.Static);
                //logger.LogInfo($"Patching {mapHelperconstructor?.Name} to {postfix?.Name}");
                //harmony.Patch(mapHelperconstructor, null, new HarmonyMethod(postfix));
                harmony.PatchAll(typeof(InputManager_BasePatches));
                //harmony.PatchAll(typeof(UserDataPatches)); 
                //harmony.PatchAll(typeof(NetworkLevelLoaderPatches));
                //harmony.PatchAll(typeof(MapHelperConstructorPatch));
                //harmony.PatchAll(typeof(PlayerConstructorPatch));

                harmony.PatchAll(typeof(SplitPlayerPatches));
                //harmony.PatchAll(typeof(SplitScreenManagerPatches));
                harmony.PatchAll(typeof(QuickSlotControllerSwitcherPatches));
                harmony.PatchAll(typeof(QuickSlotPanelPatches));
                harmony.PatchAll(typeof(ControlsInputPatches));

                harmony.PatchAll(typeof(CharacterManagerPatches));
                //harmony.PatchAll(typeof(MenuManagerPatches));
                //harmony.PatchAll(typeof(TransmorphRecipeManagerPatches));

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
