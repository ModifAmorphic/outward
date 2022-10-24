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
    public class InventoryService : IDisposable, IStartable
    {
        private IModifLogger Logger => _getLogger.Invoke();
        private readonly Func<IModifLogger> _getLogger;

        private Character _character;
        private readonly int _playerID;
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

            _playerID = _character.OwnerPlayerSys.PlayerID;
        }

        public void Start()
        {
            try
            {
                CharacterInventoryPatches.AfterInventoryIngredients += AddStashIngredients;
                CharacterManagerPatches.AfterAddCharacter += ConfigureStashPreserverDelayed;
                ItemDisplayOptionPanelPatches.TryGetActiveActions.Add(_playerID, TryAddStashActionID);
                ItemDisplayOptionPanelPatches.PlayersPressedAction.Add(_playerID, StashContextButtonPressed);
                _profileManager.ProfileService.OnActiveProfileChanged += TryConfigureStashPreserver;
                _profileManager.ProfileService.OnActiveProfileSwitched += TryConfigureStashPreserver;
                //ItemPatches.GetAdjustedReduceDurability.Add(_playerID, CalculateDurabilityReduction);

                _coroutines.DoWhen(() => _characterInventory.Stash != null, () => TryConfigureStashPreserver(_profile), 180);
            }
            catch (Exception ex)
            {
                Logger.LogException($"Failed to start {nameof(InventoryService)}.", ex);
            }
        }

        private bool TryAddStashActionID(int rewiredId, Item item, List<int> baseActiveActions, out List<int> activeActions)
        {
            const int moveToPouchID = 7;
            const int moveToBagID = 8;

            activeActions = null;
            if (rewiredId != _playerID || !GetStashInventoryEnabled() || item == null)
                return false;

            if (ItemDisplayOptionPanelPatches.PlayersMoveToStashID == -1)
                return false;

            var pouchIndex = baseActiveActions.IndexOf(moveToPouchID);
            var bagIndex = baseActiveActions.IndexOf(moveToBagID);
            var insertAfterIndex = bagIndex != -1 ? bagIndex : pouchIndex;

            //No Pouch or Back IDs found. Assume can't be place in container.
            if (insertAfterIndex == -1)
                return false;

            if (_characterInventory.Stash.Contains(item.UID))
                return false;

            activeActions = new List<int>();
            for (int i = 0; i < baseActiveActions.Count(); i++)
            {
                activeActions.Add(baseActiveActions[i]);
                if (i == insertAfterIndex)
                    activeActions.Add(ItemDisplayOptionPanelPatches.PlayersMoveToStashID);
            }

            return true;
        }

        private void StashContextButtonPressed(int actionID, ItemDisplay itemDisplay)
        {
            if (ItemDisplayOptionPanelPatches.PlayersMoveToStashID == -1)
                return;
            if (ItemDisplayOptionPanelPatches.PlayersMoveToStashID != actionID)
                return;

            itemDisplay.TryMoveTo(_characterInventory.Stash);
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
                ConfigureStashPreserver(_character);
                if (_character.OwnerPlayerSys.IsHostPlayer())
                {
                    var allCharacters = CharacterManager.Instance.Characters.Values.Where(c => !c.IsAI);
                    foreach (var character in allCharacters)
                    {
                        ConfigureStashPreserver(character);
                    }
                }
                else if (PhotonNetwork.isNonMasterClientInRoom)
                    ConfigureStashPreserver(_character);
            }
            catch (Exception ex)
            {
                Logger.LogException("Failed to configure stash preserver.", ex);
            }
        }

        private void ConfigureStashPreserverDelayed(Character character)
        {
            if (character.InstantiationType != CharacterManager.CharacterInstantiationTypes.Player)
                return;

            bool stashCreated() => character.Inventory?.Stash != null && character.Inventory.Stash.IsFirstSyncDone;
            _coroutines.DoWhen(stashCreated, () => ConfigureStashPreserver(character), 120);
        }

        private float CalculateDurabilityReduction(Item item, Character character, float durabilityReduction)
        {
            //var foodTag = TagSourceManager.Instance.GetPrivateField<TagSourceManager, TagSourceSelector>("m_foodTag");
            var stashSettings = _profile.StashSettingsProfile;
            
            if (_character.UID != _character.UID || item.PerishScript == null || !stashSettings.PreservesFoodEnabled || !item.Tags.Contains(TagSourceManager.Food))
                return durabilityReduction;

            var time = item.PerishScript.GetPrivateProperty<Perishable, float>("DeltaGameTime");

            var adjustedReduction = item.PerishScript.DepletionRate * time;
            if (!Mathf.Approximately(durabilityReduction, adjustedReduction) && durabilityReduction != adjustedReduction)
                Logger.LogDebug($"Calculated new durability reduction for character {character.UID} Item {item.name}. Original reduction: {durabilityReduction}. Adjusted reduction: {adjustedReduction}");

            return adjustedReduction;
        }

        private void ConfigureStashPreserver(Character character)
        {
            var stash = character.Stash;
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
                        Preservation = preserver.NullifyPerishing ? 100f : stashSettings.PreservesFoodAmount,
                        Tag = foodTag
                    });

                    stash.AddItemExtension(preserver);
                    var foods = stash.GetItemsFromTag(TagSourceManager.Food);
                    var perishables = stash.GetComponentsInChildren<Perishable>();
                    foreach (var perishable in perishables)
                    {
                        try
                        {
                            if (perishable != null)
                                perishable.ItemParentChanged();
                        }
                        catch (Exception ex)
                        {
                            Logger.LogException($"Unable to set preservation for perishable {perishable?.name}.", ex);
                        }
                    }
                    Logger.LogInfo($"Stash preservation amount set to {stashSettings.PreservesFoodAmount}% for character '{character?.Name}' '{character?.UID}'");
                }
                //if nullify or host character remove perishable from everyone elses stash items.
                
                //if (preserver.NullifyPerishing || character.UID != _character.UID)
                //{
                //    var foods = stash.GetItemsFromTag(TagSourceManager.Food);
                //    foreach (var food in foods)
                //    {
                //        var perishable = food.GetComponent<Perishable>();
                //        if (perishable != null)
                //            UnityEngine.Object.Destroy(perishable);
                //    }
                //}
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
                    CharacterManagerPatches.AfterAddCharacter -= ConfigureStashPreserverDelayed;
                    if (_profileManager?.ProfileService != null)
                    {
                        _profileManager.ProfileService.OnActiveProfileChanged -= TryConfigureStashPreserver;
                        _profileManager.ProfileService.OnActiveProfileSwitched -= TryConfigureStashPreserver;
                    }
                    ItemDisplayOptionPanelPatches.TryGetActiveActions.TryRemove(_playerID, out _);
                    ItemDisplayOptionPanelPatches.PlayersPressedAction.TryRemove(_playerID, out _);
                    //ItemPatches.GetAdjustedReduceDurability.TryRemove(_playerID, out _);
                }
                _character = null;
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
