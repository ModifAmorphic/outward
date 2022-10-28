﻿using ModifAmorphic.Outward.ActionUI.Patches;
using ModifAmorphic.Outward.Coroutines;
using ModifAmorphic.Outward.Extensions;
using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.Unity.ActionUI.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace ModifAmorphic.Outward.ActionUI.Services
{
    public enum StashDisplayTypes
    {
        Disabled,
        Inventory,
        Equipment,
        Merchant
    }
    internal class CharacterMenuStashService : IDisposable
    {

        private IModifLogger Logger => _getLogger.Invoke();
        private readonly Func<IModifLogger> _getLogger;

        private Character _character;
        private CharacterUI _characterUI => _character.CharacterUI;
        private CharacterInventory _characterInventory => _character.Inventory;
        private CharacterEquipment _characterEquipment => _character.Inventory.Equipment;
        private readonly ProfileManager _profileManager;
        private IActionUIProfile _profile => _profileManager.ProfileService.GetActiveProfile();
        private EquipmentSetsSettingsProfile _equipSetsProfile => _profileManager.ProfileService.GetActiveProfile().EquipmentSetsSettingsProfile;
        private InventoryService _inventoryService;
        private readonly LevelCoroutines _coroutines;
        //private ContainerDisplay _stashDisplay;
        private Dictionary<StashDisplayTypes, ContainerDisplay> _stashDisplays = new Dictionary<StashDisplayTypes, ContainerDisplay>();
        private Dictionary<StashDisplayTypes, bool?> _lastToggle = new Dictionary<StashDisplayTypes, bool?>()
        {
            { StashDisplayTypes.Disabled, null },
            { StashDisplayTypes.Inventory, null },
            { StashDisplayTypes.Equipment, null },
            { StashDisplayTypes.Merchant, null },
        };

        private const string _equipmentDisplayPath = "Canvas/GameplayPanels/Menus/CharacterMenus/MainPanel/Content/MiddlePanel/Equipment/Content/SectionContent";
        private const string _inventoryDisplayPath = "Canvas/GameplayPanels/Menus/CharacterMenus/MainPanel/Content/MiddlePanel/Inventory/Content/SectionContent";
        private const string _merchantDisplayPath = "Canvas/GameplayPanels/Menus/ModalMenus/ShopMenu/MiddlePanel/PlayerInventory/SectionContent";

        private bool _hasPatchEvents;

        private bool disposedValue;

        public CharacterMenuStashService(Character character, ProfileManager profileManager, InventoryService inventoryService, LevelCoroutines coroutines, Func<IModifLogger> getLogger)
        {
            _character = character;
            _profileManager = profileManager;
            _inventoryService = inventoryService;
            _coroutines = coroutines;
            _getLogger = getLogger;

            _profileManager.ProfileService.OnActiveProfileChanged += TrySetEnableState;
            _profileManager.ProfileService.OnActiveProfileSwitched += TrySetEnableState;
            //NetworkLevelLoader.Instance.onOverallLoadingDone += SetEnableState;
            //_coroutines.InvokeAfterLevelAndPlayersLoaded(NetworkLevelLoader.Instance, SetEnableState, 300);
            //_coroutines.InvokeAfterLevelAndPlayersLoaded(NetworkLevelLoader.Instance, TrySubscribe, 300);

        }

        public void Start()
        {
            CharacterUIPatches.BeforeShowMenu += CharacterUIPatches_BeforeShowMenu;
        }

        private void CharacterUIPatches_BeforeShowMenu(CharacterUI characterUI, CharacterUI.MenuScreens menu, Item item)
        {
            if (menu != CharacterUI.MenuScreens.Inventory && menu != CharacterUI.MenuScreens.Equipment
                && menu != CharacterUI.MenuScreens.Shop)
                return;

            TrySetEnableState();
        }

        private void TrySetEnableState()
        {
            try
            {
                ToggleStashes();
            }
            catch (Exception ex)
            {
                Logger.LogException($"TrySetEnableState failed for character '{_character?.UID}'.", ex);
            }
        }

        private void TrySetEnableState(IActionUIProfile profile)
        {
            try
            {
                ToggleStashes();
            }
            catch (Exception ex)
            {
                Logger.LogException($"TrySetEnableState to set equipment stash enable state for profile '{profile?.Name}' and character '{_character?.UID}'.", ex);
            }
        }

        private void ToggleStashes()
        {

            try
            {
                Logger.LogDebug($"CharacterMenuStashService_{this.GetHashCode()}: Toggling Stashes for character '{_character?.UID}'.");
                ToggleStash(StashDisplayTypes.Inventory, _inventoryDisplayPath, _inventoryService.GetStashInventoryEnabled());
            }
            catch (Exception ex)
            {
                Logger.LogException($"CharacterMenuStashService_{this.GetHashCode()}: ToggleStash failed for Inventory stash.", ex);
            }
            try
            {
                ToggleStash(StashDisplayTypes.Equipment, _equipmentDisplayPath, _inventoryService.GetStashInventoryEnabled());
            }
            catch (Exception ex)
            {
                Logger.LogException($"CharacterMenuStashService_{this.GetHashCode()}: ToggleStash failed for Equipment stash.", ex);
            }
            try
            {
                ToggleStash(StashDisplayTypes.Merchant, _merchantDisplayPath, _inventoryService.GetMerchantStashEnabled());
            }
            catch (Exception ex)
            {
                Logger.LogException($"CharacterMenuStashService_{this.GetHashCode()}: ToggleStash failed for Merchant stash.", ex);
            }

            if (_inventoryService.GetStashInventoryEnabled() || _inventoryService.GetMerchantStashEnabled())
                SubscribeToPatchEvents();
            else
                UnsubscribeToPatchEvents();
        }

        private void ToggleStash(StashDisplayTypes stashType, string contentDisplayPath, bool isEnabled)
        {
            Logger.LogInfo($"{(isEnabled ? "Enabling" : "Disabling")} {stashType} Stash use for character '{_character.UID}'.");

            if (_lastToggle[stashType] != null && _lastToggle[stashType] == isEnabled)
                return;

            if (!isEnabled)
            {
                RemoveStashDisplay(stashType);
            }
            else
            {
                var inventoryContentDisplay = _characterUI.transform.Find(contentDisplayPath).GetComponent<InventoryContentDisplay>();
                AddConfigureStashDisplay(inventoryContentDisplay);
            }
            _lastToggle[stashType] = isEnabled;
        }


        private bool GetIsStashDisplayToggled(StashDisplayTypes stashType, InventoryContentDisplay inventoryContentDisplay)
        {
            if (_stashDisplays == null || !_stashDisplays.ContainsKey(stashType))
                return false;

            return GetIsDisplayEnabled(stashType, inventoryContentDisplay);
        }

        private bool GetIsDisplayEnabled(StashDisplayTypes stashType, InventoryContentDisplay inventoryContentDisplay)
        {
            var inventoryPath = inventoryContentDisplay.transform.GetGameObjectPath();

            if (stashType == StashDisplayTypes.Disabled)
            {
                Logger.LogTrace($"{nameof(CharacterMenuStashService)}::{nameof(GetIsDisplayEnabled)}: false. Stash Type is {stashType} for InventoryContentDisplay '{inventoryPath}'.");
                return false;
            }

            if (inventoryContentDisplay.LocalCharacter.UID != _character.UID)
            {
                Logger.LogTrace($"{nameof(CharacterMenuStashService)}::{nameof(GetIsDisplayEnabled)}: false. inventoryContentDisplay.LocalCharacter.UID '{inventoryContentDisplay.LocalCharacter.UID}' does not" +
                    $"equal character.UID '{_character.UID}' for InventoryContentDisplay {inventoryContentDisplay.name}, path: '{inventoryPath}'.");
                return false;
            }
            if ((stashType == StashDisplayTypes.Inventory || stashType == StashDisplayTypes.Equipment) && _inventoryService.GetStashInventoryEnabled())
            {
                Logger.LogTrace($"{nameof(CharacterMenuStashService)}::{nameof(GetIsDisplayEnabled)}: true for Inventory Display '{inventoryPath}'");
                return true;
            }

            if (stashType == StashDisplayTypes.Merchant & _inventoryService.GetMerchantStashEnabled())
            {
                Logger.LogTrace($"{nameof(CharacterMenuStashService)}::{nameof(GetIsDisplayEnabled)}: true for Merchant Display '{inventoryPath}'");
                return true;
            }

            Logger.LogTrace($"{nameof(CharacterMenuStashService)}::{nameof(GetIsDisplayEnabled)}: false for InventoryContentDisplay '{inventoryPath}'.");
            return false;
        }

        private void SubscribeToPatchEvents()
        {
            if (_hasPatchEvents)
                return;

            Logger.LogDebug("Subscribing to Stash Container events");

            InventoryContentDisplayPatches.AfterOnHide += HideStashDisplay;
            InventoryContentDisplayPatches.AfterFocusMostRelevantItem += FocusMostRelevantItem;
            InventoryContentDisplayPatches.AfterSetContainersVisibility += SetContainersVisibility;
            InventoryContentDisplayPatches.AfterRefreshReferences += RefreshReferences;
            InventoryContentDisplayPatches.AfterRefreshContainerDisplays += RefreshContainerDisplays;
            CharacterInventoryPatches.OwnsItemDelegates.AddOrUpdate(_character.OwnerPlayerSys.PlayerID, IsStashOwned);

            _hasPatchEvents = true;
        }

        private void UnsubscribeToPatchEvents()
        {
            if (!_hasPatchEvents)
                return;

            Logger.LogDebug("Unsubscribing from Stash Container events");

            InventoryContentDisplayPatches.AfterOnHide -= HideStashDisplay;
            InventoryContentDisplayPatches.AfterFocusMostRelevantItem -= FocusMostRelevantItem;
            InventoryContentDisplayPatches.AfterSetContainersVisibility -= SetContainersVisibility;
            InventoryContentDisplayPatches.AfterRefreshReferences -= RefreshReferences;
            InventoryContentDisplayPatches.AfterRefreshContainerDisplays -= RefreshContainerDisplays;
            CharacterInventoryPatches.OwnsItemDelegates.TryRemove(_character.OwnerPlayerSys.PlayerID, out var _);

            _hasPatchEvents = false;
        }

        private void AddConfigureStashDisplay(InventoryContentDisplay inventoryContentDisplay)
        {
            var stashType = GetStashType(inventoryContentDisplay);
            AddStashDisplay(stashType, inventoryContentDisplay);
            ConfigureStashDisplay(inventoryContentDisplay);
        }

        private StashDisplayTypes GetStashType(InventoryContentDisplay inventoryContentDisplay)
        {
            var inventoryPath = inventoryContentDisplay.transform.GetGameObjectPath();
            if (inventoryPath.EndsWith(_inventoryDisplayPath))
            {
                return StashDisplayTypes.Inventory;
            }
            else if (inventoryPath.EndsWith(_equipmentDisplayPath))
            {
                return StashDisplayTypes.Equipment;
            }
            else if (inventoryPath.EndsWith(_merchantDisplayPath))
            {
                return StashDisplayTypes.Merchant;
            }
            return StashDisplayTypes.Disabled;

        }
        private void AddStashDisplay(StashDisplayTypes stashType, InventoryContentDisplay inventoryContentDisplay)
        {
            if (!GetIsDisplayEnabled(stashType, inventoryContentDisplay))
                return;

            if (!_stashDisplays.ContainsKey(stashType))
            {
                RectTransform parentTransform = inventoryContentDisplay.GetPrivateField<InventoryContentDisplay, RectTransform>("m_overrideContentHolder");
                if (parentTransform == null)
                    parentTransform = inventoryContentDisplay.GetPrivateField<InventoryContentDisplay, ScrollRect>("m_inventoriesScrollRect")?.content;

                var containerDisplayPrefab = inventoryContentDisplay.GetPrivateField<InventoryContentDisplay, RectTransform>("ContainerDisplayPrefab");
                var stashDisplay = UnityEngine.Object.Instantiate(containerDisplayPrefab).GetComponent<ContainerDisplay>();
                stashDisplay.transform.SetParent(parentTransform);
                stashDisplay.transform.ResetLocal();
                stashDisplay.name = "StashDisplay";

                var lblWeight = stashDisplay.transform.Find("lblWeight");
                if (lblWeight != null)
                    UnityEngine.Object.Destroy(lblWeight);

                stashDisplay.SetPrivateField<ContainerDisplay, Text>("m_lblWeight", null);

                _stashDisplays.Add(stashType, stashDisplay);

                Logger.LogDebug($"Added stash display for character {_character.name}.");
            }
        }

        private void RemoveStashDisplay(StashDisplayTypes stashType)
        {
            if (_stashDisplays.TryRemove(stashType, out var stashDisplay))
            {
                stashDisplay.gameObject.SetActive(false);
                stashDisplay.gameObject.Destroy();
            }
        }

        private void RemoveStashDisplays()
        {
            Logger.LogDebug($"Removing stash displays for character '{_character.UID}'.");
            foreach (var stashDisplay in _stashDisplays.Values)
            {
                if (stashDisplay != null)
                {
                    stashDisplay.gameObject.SetActive(false);
                    stashDisplay.gameObject.Destroy();
                }
            }

            _stashDisplays.Clear();
        }

        private void ConfigureStashDisplay(InventoryContentDisplay inventoryContentDisplay)
        {
            var stashType = GetStashType(inventoryContentDisplay);
            if (!_stashDisplays.TryGetValue(stashType, out var stashDisplay))
                return;

            Logger.LogDebug($"Attempting to configure filters for stash display {stashDisplay?.name} - '{stashDisplay?.transform.GetGameObjectPath()}'. ShouldInvoke(inventoryContentDisplay) == {GetIsStashDisplayToggled(stashType, inventoryContentDisplay)}.");
            if (!GetIsStashDisplayToggled(stashType, inventoryContentDisplay))
                return;

            var filter = inventoryContentDisplay.GetPrivateField<InventoryContentDisplay, ItemFilter>("m_filter");
            var exceptionFilter = inventoryContentDisplay.GetPrivateField<InventoryContentDisplay, ItemFilter>("m_exceptionFilter");

            stashDisplay.SetFilter(filter);
            stashDisplay.SetExceptionFilter(exceptionFilter);
            Logger.LogDebug($"Configure filters for stash display {stashDisplay?.name} - '{stashDisplay.transform.GetGameObjectPath()}'.");
        }

        private void HideStashDisplay(InventoryContentDisplay inventoryContentDisplay)
        {
            var stashType = GetStashType(inventoryContentDisplay);

            if (!GetIsStashDisplayToggled(stashType, inventoryContentDisplay))
                return;

            if (_stashDisplays.TryGetValue(stashType, out var stashDisplay))
                stashDisplay?.ReleaseAllDisplays();
        }

        private void FocusMostRelevantItem(InventoryContentDisplay inventoryContentDisplay, ItemListDisplay excludedList, bool result)
        {
            var stashType = GetStashType(inventoryContentDisplay);
            if (result || !GetIsStashDisplayToggled(stashType, inventoryContentDisplay) || _stashDisplays == null)
                return;

            if (_stashDisplays.TryGetValue(stashType, out var stashDisplay) && stashDisplay.isActiveAndEnabled && excludedList != stashDisplay)
                stashDisplay.Focus();

        }

        private void SetContainersVisibility(InventoryContentDisplay inventoryContentDisplay, bool showPouch, bool showBag, bool showEquipment)
        {
            var stashType = GetStashType(inventoryContentDisplay);
            if (!GetIsStashDisplayToggled(stashType, inventoryContentDisplay) || _stashDisplays == null)
                return;

            if (showPouch && _stashDisplays.TryGetValue(stashType, out var stashDisplay))
                stashDisplay.Show();
        }

        private void RefreshReferences(InventoryContentDisplay inventoryContentDisplay, bool forceRefresh)
        {
            var stashType = GetStashType(inventoryContentDisplay);

            if (!GetIsStashDisplayToggled(stashType, inventoryContentDisplay) || _stashDisplays == null)
                return;

            if (!(inventoryContentDisplay.GetPrivateField<InventoryContentDisplay, bool>("m_startDone") | forceRefresh))
                return;

            if (_stashDisplays.TryGetValue(stashType, out var stashDisplay))
            {
                stashDisplay.SetReferencedContainer(_character.Stash);
                stashDisplay.Show();
            }
        }

        private void RefreshContainerDisplays(InventoryContentDisplay inventoryContentDisplay, bool clearAssignedDisplay)
        {
            var stashType = GetStashType(inventoryContentDisplay);

            if (!_stashDisplays.TryGetValue(stashType, out var stashDisplay) || !stashDisplay.isActiveAndEnabled || !GetIsStashDisplayToggled(stashType, inventoryContentDisplay))
                return;

            if (clearAssignedDisplay)
                stashDisplay.ReleaseAllDisplays();
            stashDisplay.Refresh();
        }

        private bool IsStashOwned(string itemUID)
        {
            var displayedStash = _stashDisplays.Values.FirstOrDefault(s => s.isActiveAndEnabled);

            if (displayedStash == null)
                return false;

            return _character.Stash.Contains(itemUID);
        }

        protected virtual void Dispose(bool disposing)
        {
            Logger.LogDebug($"Disposing of CharacterMenuStashService_{this.GetHashCode()}");
            if (!disposedValue)
            {
                if (disposing)
                {
                    UnsubscribeToPatchEvents();
                    if (_profileManager?.ProfileService != null)
                    {
                        _profileManager.ProfileService.OnActiveProfileChanged -= TrySetEnableState;
                        _profileManager.ProfileService.OnActiveProfileSwitched -= TrySetEnableState;

                    }
                    //NetworkLevelLoader.Instance.onOverallLoadingDone -= ToggleStashes;
                    CharacterUIPatches.BeforeShowMenu -= CharacterUIPatches_BeforeShowMenu;
                }
                RemoveStashDisplays();
                _character = null;
                _inventoryService = null;
                _stashDisplays = null;
                _lastToggle = null;

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}