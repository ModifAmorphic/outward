using ModifAmorphic.Outward.ActionUI.Patches;
using ModifAmorphic.Outward.Coroutines;
using ModifAmorphic.Outward.Extensions;
using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.Unity.ActionUI.Data;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace ModifAmorphic.Outward.ActionUI.Services
{
    internal class EquipmentMenuStashService : IDisposable
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
        private readonly ModifCoroutine _coroutines;
        private ContainerDisplay _stashDisplay;

        private const string inventoryDisplayPath = "Canvas/GameplayPanels/Menus/CharacterMenus/MainPanel/Content/MiddlePanel/Equipment/Content/SectionContent";
        private const string stashItemDisplayPath = "Canvas/GameplayPanels/Menus/CharacterMenus/MainPanel/Content/MiddlePanel/Equipment/Content/SectionContent/Scroll View/Viewport/Content/StashDisplay/Content";

        private bool _hasPatchEvents;

        private bool disposedValue;

        public EquipmentMenuStashService(Character character, ProfileManager profileManager, InventoryService inventoryService, ModifCoroutine coroutines, Func<IModifLogger> getLogger)
        {
            _character = character;
            _profileManager = profileManager;
            _inventoryService = inventoryService;
            _coroutines = coroutines;
            _getLogger = getLogger;

            _profileManager.ProfileService.OnActiveProfileChanged.AddListener(SetEnableState);
            _profileManager.ProfileService.OnActiveProfileSwitched.AddListener(SetEnableState);
            _profileManager.ProfileService.OnNewProfile.AddListener(SetEnableState);
            if (_inventoryService.GetIsStashEquipEnabled())
                SubscribeToPatchEvents();
        }

        private void SetEnableState(IActionUIProfile profile)
        {
            if (!_inventoryService.GetIsStashEquipEnabled())
            {
                RemoveStashDisplay();
                UnsubscribeToPatchEvents();
            }
            else
            {
                var inventoryContentDisplay = _characterUI.transform.Find(inventoryDisplayPath).GetComponent<InventoryContentDisplay>();
                AddConfigureStashDisplay(inventoryContentDisplay);
                SubscribeToPatchEvents();
            }
        }

        private bool ShouldInvoke(InventoryContentDisplay inventoryContentDisplay) =>
            inventoryContentDisplay.LocalCharacter.UID == _character.UID
                    && inventoryContentDisplay.transform.GetGameObjectPath().EndsWith(inventoryDisplayPath) //only the equipment -> inventory menu
                    && _inventoryService.GetIsStashEquipEnabled();

        private void SubscribeToPatchEvents()
        {
            if (_hasPatchEvents)
                return;

            InventoryContentDisplayPatches.AfterStartInit += AddConfigureStashDisplay;
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

            InventoryContentDisplayPatches.AfterStartInit -= AddConfigureStashDisplay;
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
            AddStashDisplay(inventoryContentDisplay);
            ConfigureStashDisplay(inventoryContentDisplay);
        }

        private void AddStashDisplay(InventoryContentDisplay inventoryContentDisplay)
        {
            if (!ShouldInvoke(inventoryContentDisplay))
                return;

            if (_stashDisplay == null)
            {
                RectTransform parentTransform = inventoryContentDisplay.GetPrivateField<InventoryContentDisplay, RectTransform>("m_overrideContentHolder");
                if (parentTransform == null)
                    parentTransform = inventoryContentDisplay.GetPrivateField<InventoryContentDisplay, ScrollRect>("m_inventoriesScrollRect")?.content;

                var containerDisplayPrefab = inventoryContentDisplay.GetPrivateField<InventoryContentDisplay, RectTransform>("ContainerDisplayPrefab");
                _stashDisplay = UnityEngine.Object.Instantiate(containerDisplayPrefab).GetComponent<ContainerDisplay>();
                _stashDisplay.transform.SetParent(parentTransform);
                _stashDisplay.transform.ResetLocal();
                _stashDisplay.name = "StashDisplay";

                Logger.LogDebug($"Added stash display for character {_character.name}.");
            }
        }

        private void RemoveStashDisplay()
        {
            if (_stashDisplay == null)
                return;

            _stashDisplay.gameObject.SetActive(false);
            _stashDisplay.gameObject.Destroy();
            _stashDisplay = null;
        }

        private void ConfigureStashDisplay(InventoryContentDisplay inventoryContentDisplay)
        {
            Logger.LogDebug($"Attempting to configure filters for stash display {_stashDisplay?.name} - '{_stashDisplay.transform.GetGameObjectPath()}'. ShouldInvoke(inventoryContentDisplay) == {ShouldInvoke(inventoryContentDisplay)}. _stashDisplay == null == {_stashDisplay == null}");
            if (!ShouldInvoke(inventoryContentDisplay) || _stashDisplay == null)
                return;

            var filter = inventoryContentDisplay.GetPrivateField<InventoryContentDisplay, ItemFilter>("m_filter");
            var exceptionFilter = inventoryContentDisplay.GetPrivateField<InventoryContentDisplay, ItemFilter>("m_exceptionFilter");

            _stashDisplay.SetFilter(filter);
            _stashDisplay.SetExceptionFilter(exceptionFilter);
            Logger.LogDebug($"Configure filters for stash display {_stashDisplay?.name} - '{_stashDisplay.transform.GetGameObjectPath()}'.");
        }

        private void HideStashDisplay(InventoryContentDisplay inventoryContentDisplay)
        {
            if (!ShouldInvoke(inventoryContentDisplay))
                return;

            _stashDisplay?.ReleaseAllDisplays();
        }

        private void FocusMostRelevantItem(InventoryContentDisplay inventoryContentDisplay, ItemListDisplay excludedList, bool result)
        {
            if (result || !ShouldInvoke(inventoryContentDisplay) || _stashDisplay == null)
                return;

            if (_stashDisplay.IsDisplayed && excludedList != _stashDisplay)
                _stashDisplay.Focus();

        }

        private void SetContainersVisibility(InventoryContentDisplay inventoryContentDisplay, bool showPouch, bool showBag, bool showEquipment)
        {
            if (!ShouldInvoke(inventoryContentDisplay) || _stashDisplay == null)
                return;

            if (showPouch)
                _stashDisplay.Show();
        }

        private void RefreshReferences(InventoryContentDisplay inventoryContentDisplay, bool forceRefresh)
        {
            if (!ShouldInvoke(inventoryContentDisplay) || _stashDisplay == null)
                return;

            if (!(inventoryContentDisplay.GetPrivateField<InventoryContentDisplay, bool>("m_startDone") | forceRefresh))
                return;

            _stashDisplay.SetReferencedContainer(_character.Stash);
            _stashDisplay.Show();
        }

        private void RefreshContainerDisplays(InventoryContentDisplay inventoryContentDisplay, bool clearAssignedDisplay)
        {
            if (!_stashDisplay.IsDisplayed || !ShouldInvoke(inventoryContentDisplay) || _stashDisplay == null)
                return;

            if (clearAssignedDisplay)
                _stashDisplay.ReleaseAllDisplays();
            _stashDisplay.Refresh();
        }

        private bool IsStashOwned(string itemUID)
        {
            if (_stashDisplay == null || !_stashDisplay.IsDisplayed)
                return false;

            return _character.Stash.Contains(itemUID);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    UnsubscribeToPatchEvents();
                    _profileManager?.ProfileService?.OnActiveProfileChanged?.RemoveListener(SetEnableState);
                    _profileManager?.ProfileService?.OnActiveProfileSwitched?.RemoveListener(SetEnableState);
                    _profileManager?.ProfileService?.OnNewProfile?.RemoveListener(SetEnableState);
                }

                _character = null;
                _inventoryService = null;
                _stashDisplay = null;

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
