using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.Modules.CharacterMods;
using ModifAmorphic.Outward.Modules.Crafting;
using ModifAmorphic.Outward.Modules.Crafting.Services;
using ModifAmorphic.Outward.Modules.Items;
using ModifAmorphic.Outward.Modules.Merchants;
using ModifAmorphic.Outward.Modules.QuickSlots;
using System;

namespace ModifAmorphic.Outward.Modules
{
    public static class ModifModules
    {
        private static ModuleService _lazyService = null;
        private static ModuleService ModuleService
        {
            get
            {
                if (_lazyService == null) _lazyService = new ModuleService();
                return _lazyService;
            }
        }
        public static QuickSlotExtender GetQuickSlotExtenderModule(string modId)
        {
            return ModuleService.GetModule(modId, () => 
                new QuickSlotExtender(() => LoggerFactory.GetLogger(modId)));
        }
        public static CharacterInstances GetCharacterInstancesModule(string modId)
        {
            return ModuleService.GetModule<CharacterInstances>(modId, () =>
                new CharacterInstances(() => LoggerFactory.GetLogger(modId)));
        }
        public static PreFabricator GetPreFabricatorModule(string modId)
        {
            return ModuleService.GetModule<PreFabricator>(modId, () =>
                new PreFabricator(modId, 
                    () => ResourcesPrefabManager.Instance, 
                    () => LoggerFactory.GetLogger(modId)));
        }
        public static ItemVisualizer GetItemVisualizerModule(string modId)
        {
            var iconService = new IconService(modId,
                                              new GameObjectResources.ModifGoService(() => LoggerFactory.GetLogger(modId)),
                                              () => LoggerFactory.GetLogger(modId));
            return ModuleService.GetModule<ItemVisualizer>(modId, () =>
                new ItemVisualizer(
                    () => ResourcesPrefabManager.Instance,
                    () => ItemManager.Instance,
                    iconService,
                    () => LoggerFactory.GetLogger(modId)));
        }
        public static MerchantModule GetMerchantModule(string modId)
        {
            return ModuleService.GetModule<MerchantModule>(modId, () =>
                new MerchantModule(() => LoggerFactory.GetLogger(modId)));
        }
        public static CustomCraftingModule GetCustomCraftingModule(string modId)
        {
            var menuTabService = new CraftingMenuUIService(() => LoggerFactory.GetLogger(modId));

            return ModuleService.GetModule<CustomCraftingModule>(modId, () =>
                new CustomCraftingModule(
                    new CraftingMenuUIService(() => LoggerFactory.GetLogger(modId)),
                    new RecipeDisplayService(() => LoggerFactory.GetLogger(modId)),
                    new CustomRecipeService(
                        () => RecipeManager.Instance,
                        () => LoggerFactory.GetLogger(modId)),
                    new CustomCraftingService(() => LoggerFactory.GetLogger(modId)),
                    () => LoggerFactory.GetLogger(modId)));
        }
    }
}
