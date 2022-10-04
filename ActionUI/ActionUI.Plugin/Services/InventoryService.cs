using ModifAmorphic.Outward.ActionUI.Patches;
using ModifAmorphic.Outward.ActionUI.Settings;
using ModifAmorphic.Outward.Coroutines;
using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.Unity.ActionUI.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ModifAmorphic.Outward.ActionUI.Services
{
    public class InventoryService : IDisposable
    {
        private IModifLogger Logger => _getLogger.Invoke();
        private readonly Func<IModifLogger> _getLogger;

        private Character _character;
        private CharacterInventory _characterInventory => _character.Inventory;
        private CharacterEquipment _characterEquipment => _character.Inventory.Equipment;
        private readonly ProfileManager _profileManager;
        private IActionUIProfile _profile => _profileManager.ProfileService.GetActiveProfile();
        private readonly LevelCoroutines _coroutines;

        private bool disposedValue;

        public InventoryService(Character character, ProfileManager profileManager, LevelCoroutines coroutines, Func<IModifLogger> getLogger)
        {
            _character = character;
            _profileManager = profileManager;
            _coroutines = coroutines;
            _getLogger = getLogger;
        }

        public void Start()
        {
            CharacterInventoryPatches.AfterInventoryIngredients += AddStashIngredients;
        }

        private void AddStashIngredients(CharacterInventory characterInventory, Character character, Tag craftingStationTag, ref DictionaryExt<int, CompatibleIngredient> sortedIngredients)
        {
            if (!_profileManager.ProfileService.GetActiveProfile().StashCraftingEnabled
                || character.Stash == null
                || character.UID != _character.UID
                || !GetAreaContainsStash())
                return;

            var stashItems = character.Stash.GetContainedItems().ToList();

            var inventoryIngredients = characterInventory.GetType().GetMethod("InventoryIngredients", BindingFlags.NonPublic | BindingFlags.Instance);
            inventoryIngredients.Invoke(characterInventory, new object[] { craftingStationTag, sortedIngredients, stashItems });
        }

        public Dictionary<string, Item> GetItemPrefabs()
        {
            var field = typeof(ResourcesPrefabManager).GetField("ITEM_PREFABS", BindingFlags.NonPublic | BindingFlags.Static);
            return (Dictionary<string, Item>)field.GetValue(null);
        }

        public bool GetAreaContainsStash() => TryGetCurrentAreaEnum(out var area) && InventorySettings.StashAreas.Contains(area);

        private bool TryGetCurrentAreaEnum(out AreaManager.AreaEnum area)
        {
            area = default;
            var sceneName = AreaManager.Instance?.CurrentArea?.SceneName;
            if (string.IsNullOrEmpty(sceneName))
                return false;

            area = (AreaManager.AreaEnum)AreaManager.Instance.GetAreaIndexFromSceneName(sceneName);
            return true;
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
