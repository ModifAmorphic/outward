using ModifAmorphic.Outward.ActionUI.Patches;
using ModifAmorphic.Outward.ActionUI.Settings;
using ModifAmorphic.Outward.Coroutines;
using ModifAmorphic.Outward.Extensions;
using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.Unity.ActionUI.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

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
        private bool _isRemoved;
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
            try
            {
                CharacterInventoryPatches.AfterInventoryIngredients += AddStashIngredients;
                _profileManager.ProfileService.OnActiveProfileChanged += TryConfigureStashPreserver;
                _profileManager.ProfileService.OnActiveProfileSwitched += TryConfigureStashPreserver;

                _coroutines.DoWhen(() => _characterInventory.Stash != null, ConfigureStashPreserver, 180);
            }
            catch (Exception ex)
            {
                Logger.LogException($"Failed to start {nameof(InventoryService)}.", ex);
            }
        }

        private void AddStashIngredients(CharacterInventory characterInventory, Character character, Tag craftingStationTag, ref DictionaryExt<int, CompatibleIngredient> sortedIngredients)
        {
            if (!GetCraftFromStashEnabled()
                || character.Stash == null
                || character.UID != _character.UID)
                return;

            var stashItems = character.Stash.GetContainedItems().ToList();

            var inventoryIngredients = characterInventory.GetType().GetMethod("InventoryIngredients", BindingFlags.NonPublic | BindingFlags.Instance);
            inventoryIngredients.Invoke(characterInventory, new object[] { craftingStationTag, sortedIngredients, stashItems });
        }

        private void TryConfigureStashPreserver(IActionUIProfile profile)
        {
            try
            {
                Logger.LogDebug($"Trying to Configure Stash Preserver for profile '{profile?.Name}'.");
                ConfigureStashPreserver();
            }
            catch (Exception ex)
            {
                Logger.LogException("Failed to configure stash preserver.", ex);
            }
        }
        private void ConfigureStashPreserver()
        {
            var stash = _characterInventory.Stash;
            var stashSettings = _profile.StashSettingsProfile;
            var preserver = stash.GetPrivateField<ItemContainer, Preserver>("m_preservationExt");
            var foodTag = TagSourceManager.Instance.GetPrivateField<TagSourceManager, TagSourceSelector>("m_foodTag");

            if (stashSettings.PreservesFoodEnabled)
            {
                bool addPreserver = false;
                if (preserver == null)
                {
                    preserver = stash.gameObject.GetOrAddComponent<Preserver>();
                    addPreserver = true;
                }

                var preservedElements = preserver.GetPrivateField<Preserver, List<Preserver.PreservedElement>>("m_preservedElements");
                if (!addPreserver)
                {
                    if (preservedElements.Count == 0)
                    {
                        addPreserver = true;
                    }
                    else
                    {
                        var existing = preservedElements.FirstOrDefault(p => p.Tag.Tag == foodTag.Tag);
                        if (existing == null)
                        {
                            addPreserver = true;
                        }
                        else if (preserver.NullifyPerishing && stashSettings.PreservesFoodAmount != 100)
                        {
                            addPreserver = true;
                        }
                        else if (!preserver.NullifyPerishing && stashSettings.PreservesFoodAmount == 100)
                        {
                            addPreserver = true;
                        }
                        else if (!Mathf.Approximately(existing.Preservation, stashSettings.PreservesFoodAmount)
                            && !(preserver.NullifyPerishing && stashSettings.PreservesFoodAmount == 100))
                        {
                            addPreserver = true;
                        }
                    }
                }

                if (addPreserver)
                {
                    preserver.NullifyPerishing = stashSettings.PreservesFoodAmount == 100;
                    preservedElements.RemoveAll(p => p.Tag == foodTag);
                    preservedElements.Add(new Preserver.PreservedElement()
                    {
                        Preservation = preserver.NullifyPerishing ? 0f : stashSettings.PreservesFoodAmount,
                        Tag = foodTag
                    });

                    stash.AddItemExtension(preserver);
                    Logger.LogInfo($"Stash preservation amount set to {stashSettings.PreservesFoodAmount}% for character '{_character.UID}'");
                }
            }
            else
            {
                if (TryRemoveStashPreserver())
                    Logger.LogInfo($"Stash set to not preserve food.");
            }
        }

        private bool TryRemoveStashPreserver()
        {
            try
            {
                var stash = _characterInventory.Stash;
                if (stash?.gameObject == null)
                {
                    Logger.LogInfo($"Stash was null. Exiting TryRemoveStashPreserver.");
                    return false;
                }
                var preserver = stash.GetPrivateField<ItemContainer, Preserver>("m_preservationExt");

                if (preserver != null)
                    stash.SetPrivateField<ItemContainer, Preserver>("m_preservationExt", null);

                var preserverComponent = stash.gameObject.GetComponent<Preserver>();
                if (preserverComponent != null)
                {
                    UnityEngine.Object.Destroy(preserverComponent);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("Failed to remove stash preserver.", ex);
            }
            return false;
        }

        public static Dictionary<string, Item> GetItemPrefabs()
        {
            var field = typeof(ResourcesPrefabManager).GetField("ITEM_PREFABS", BindingFlags.NonPublic | BindingFlags.Static);
            return (Dictionary<string, Item>)field.GetValue(null);
        }

        public bool GetAreaContainsStash() => TryGetCurrentAreaEnum(out var area) && InventorySettings.StashAreas.Contains(area);

        public bool GetStashInventoryEnabled() =>
            _profile.StashSettingsProfile.CharInventoryEnabled && (GetAreaContainsStash() || _profile.StashSettingsProfile.CharInventoryAnywhereEnabled);


        public bool GetMerchantStashEnabled() =>
            _profile.StashSettingsProfile.MerchantEnabled && (GetAreaContainsStash() || _profile.StashSettingsProfile.MerchantAnywhereEnabled);

        public bool GetCraftFromStashEnabled() =>
            _profile.StashSettingsProfile.CraftingInventoryEnabled && (GetAreaContainsStash() || _profile.StashSettingsProfile.CraftingInventoryAnywhereEnabled);

        public static bool TryGetCurrentAreaEnum(out AreaManager.AreaEnum area)
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
                    //TryRemoveStashPreserver();
                    if (_profileManager?.ProfileService != null)
                    {
                        _profileManager.ProfileService.OnActiveProfileChanged -= TryConfigureStashPreserver;
                        _profileManager.ProfileService.OnActiveProfileSwitched -= TryConfigureStashPreserver;
                    }
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
