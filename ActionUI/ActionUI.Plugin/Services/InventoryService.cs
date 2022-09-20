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
    internal class InventoryService : IDisposable
    {
        private IModifLogger Logger => _getLogger.Invoke();
        private readonly Func<IModifLogger> _getLogger;

        private Character _character;
        private readonly ProfileManager _profileManager;
        private bool disposedValue;

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

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    CharacterInventoryPatches.AfterInventoryIngredients -= AddStashIngredients;
                }
                _character = null;
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~InventoryService()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
