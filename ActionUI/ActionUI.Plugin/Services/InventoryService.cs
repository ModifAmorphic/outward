using ModifAmorphic.Outward.ActionUI.Patches;
using ModifAmorphic.Outward.ActionUI.Settings;
using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.Unity.ActionMenus.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ModifAmorphic.Outward.ActionUI.Services
{
    internal class InventoryService
    {
        private IModifLogger Logger => _getLogger.Invoke();
        private readonly Func<IModifLogger> _getLogger;

        private readonly Character _character;
        private readonly ProfileManager _profileManager;

        public InventoryService(Character character, ProfileManager profileManager, Func<IModifLogger> getLogger)
        {
            _character = character;
            _profileManager = profileManager;
            _getLogger = getLogger;

            CharacterInventoryPatches.AfterInventoryIngredients += AddStashIngredients;
        }

        private void AddStashIngredients(CharacterInventory characterInventory, Character character, Tag craftingStationTag, ref DictionaryExt<int, CompatibleIngredient> sortedIngredients)
        {
            if (!_profileManager.ProfileService.GetActiveProfile().StashCraftingEnabled 
                || character.Stash == null
                || character.UID != _character.UID)
                return;
            
            var stashItems = character.Stash.GetContainedItems().ToList();

            var inventoryIngredients = characterInventory.GetType().GetMethod("InventoryIngredients", BindingFlags.NonPublic | BindingFlags.Instance);
            inventoryIngredients.Invoke(characterInventory, new object[] { craftingStationTag, sortedIngredients, stashItems});
        }

        protected AreaManager.AreaEnum GetCurrentAreaEnum()
        {
            var sceneName = AreaManager.Instance.CurrentArea.SceneName;
            return (AreaManager.AreaEnum)AreaManager.Instance.GetAreaIndexFromSceneName(sceneName);
        }
    }
}
