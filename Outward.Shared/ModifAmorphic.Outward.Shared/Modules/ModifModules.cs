using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.Modules.Character;
using ModifAmorphic.Outward.Modules.Items;
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
    }
}
