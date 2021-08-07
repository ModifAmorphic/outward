using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.StashPacks.Extensions;
using ModifAmorphic.Outward.StashPacks.Patch.Events;
using ModifAmorphic.Outward.StashPacks.SaveData.Data;
using ModifAmorphic.Outward.StashPacks.Settings;
using ModifAmorphic.Outward.StashPacks.State;
using ModifAmorphic.Outward.StashPacks.Sync;
using ModifAmorphic.Outward.StashPacks.Sync.Models;
using ModifAmorphic.Outward.StashPacks.WorldInstance.Data;
using ModifAmorphic.Outward.StashPacks.WorldInstance.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ModifAmorphic.Outward.StashPacks.WorldInstance.MajorEvents
{
    internal class LevelLoadingActions : MajorBagActions
    {
        private bool _packProcessingEnabled = false;
        private bool _networkLoadEventAlreadyOccured = false;
        private bool _coroutineStarted = false;

        private readonly object _coroutineLock = new object();

        public LevelLoadingActions(InstanceFactory instances, Func<IModifLogger> getLogger) : base(instances, getLogger)
        {
            if (!_networkLoadEventAlreadyOccured)
            {
                _networkLoadEventAlreadyOccured = true;
                _packProcessingEnabled = true;
            }
        }
        public void SubscribeToEvents()
        {
            
            //TODO: Not sure if this happens for remote players.
            NetworkLevelLoaderEvents.MidLoadLevelBefore += MidLoadLevelBefore;
            ItemManagerEvents.IsAllItemSyncedAfter += AfterIsAllItemSynced;
            //EnvironmentSaveEvents.ApplyDataBefore += envSave => ApplyDataBefore(ref envSave);
        }

        private void ApplyDataBefore(ref EnvironmentSave envSave)
        {
            //Wipe all stashpack data from the ItemList before they get loaded into the world.
            var stashPackSaves = envSave.ItemList.Where(s =>
                            s.TryGetPreviousOwnerUID(out _)
                            && s.IsStashPack(_instances.AreaStashPackItemIds.Keys))
                .ToDictionary(b => b.Identifier.ToString(), b => b);;

            foreach (var uid in stashPackSaves.Keys)
            {
                int removed = envSave.ItemList.RemoveAll(i => 
                                    i.GetHierarchyData().ParentUID == uid + StashPacksConstants.BagUidSuffix
                                );
                Logger.LogDebug($"{nameof(LevelLoadingActions)}::{nameof(ApplyDataBefore)}: Removed {removed} items from StashPack Bag '{uid}'.");
            }
            
            foreach (var kvp in stashPackSaves)
            {
                var stashIndex = envSave.ItemList.FindIndex(i => kvp.Key == i.Identifier.ToString());
                envSave.ItemList[stashIndex] = kvp.Value.ToUpdatedContainerSilver(0);
            }
        }

        private void MidLoadLevelBefore(NetworkLevelLoader networkLevelLoader)
        {
            _packProcessingEnabled = true;
        }

        private void ClearFlags()
        {
            _packProcessingEnabled = false;
            _networkLoadEventAlreadyOccured = false;
            _coroutineStarted = false;
        }

        private bool AfterIsAllItemSynced(ItemManager itemManager, bool baseResult)
        {
            //ignore all events that have not been triggered by level loading events
            //also, even if triggered by a level load, don't start until the base game
            //has had a chance to sync all items.
            if (!_packProcessingEnabled || !baseResult)
                return baseResult;

            try
            {
                if (!_coroutineStarted)
                {
                    lock (_coroutineLock)
                    {
                        if (!_coroutineStarted)
                        {
                            _instances.UnityPlugin.StartCoroutine(SyncLocalStashPacks());
                        }
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                ClearFlags();
                Logger.LogException($"{nameof(LevelLoadingActions)}::{nameof(AfterIsAllItemSynced)}: Encountered Critical Exception while attempting to sync" +
                    $" local area StashPacks. Abandoning Stash to StashPack processing", ex);
                return baseResult;
            }
        }

        private IEnumerator SyncLocalStashPacks()
        {
            try
            {
                _coroutineStarted = true;
                UpdateAreaStashPacks();
                Logger.LogDebug($"{nameof(LevelLoadingActions)}::{nameof(SyncLocalStashPacks)}: Completed injection of StashPack of changes.");
            }
            catch (Exception ex)
            {
                Logger.LogException($"{nameof(LevelLoadingActions)}::{nameof(SyncLocalStashPacks)}: Encountered Critical Exception while attempting to sync" +
                    $" local area StashPacks. Abandoning Stash to StashPack processing", ex);

            }
            ClearFlags();
            yield return null;
        }
        private void UpdateAreaStashPacks()
        {
            if (!_instances.TryGetStashPackWorldData(out var stashPackWorldData))
            {
                Logger.LogError($"{nameof(LevelLoadingActions)}::{nameof(UpdateAreaStashPacks)}: Could not retrieve a {nameof(StashPackWorldData)} instance.");
                return;
            }
            var stashPacks = stashPackWorldData.GetAllStashPacks();
            if (stashPacks == null)
            {
                Logger.LogDebug($"{nameof(LevelLoadingActions)}::{nameof(UpdateAreaStashPacks)}: No {nameof(StashPack)} found in world.");
                return;
            }

            var activePacks = GetActivePacksDisableOthers(stashPacks);
            var uniqueOwnedPacks = DisownDuplicatePacks(activePacks);
            var syncPlans = GenerateSyncPlans(uniqueOwnedPacks);
            ExecuteAllPlans(syncPlans);
            SaveBagStates(uniqueOwnedPacks);

            foreach(var p in uniqueOwnedPacks.Values.SelectMany(b => b))
            {
                BagVisualizer.BagLoaded(p.StashBag);
            }

        }
        private void ExecuteAllPlans(IEnumerable<ContainerSyncPlan> syncPlans)
        {
            var syncPlanner = _instances.GetSyncPlanner();
            if (!_instances.TryGetStashPackWorldExecuter(out var planExecuter))
            {
                Logger.LogError($"{nameof(LevelLoadingActions)}::{nameof(ExecuteAllPlans)}: Critical failure. Could not retrieve a {nameof(StashPackWorldExecuter)} instance." +
                    $" No StashPacks could be restored from stash.");
                return;
            }
            foreach (var syncPlan in syncPlans)
            {
                syncPlanner.LogSyncPlan(syncPlan, true);
                planExecuter.ExecutePlan(syncPlan);

            }
        }
        private void SaveBagStates(Dictionary<string, List<StashPack>> characterPacks)
        {
            foreach (var characterUID in characterPacks.Keys.Where(uid => !string.IsNullOrWhiteSpace(uid)))
            {
                foreach (var pack in characterPacks[characterUID])
                {
                    DoAfterBagLoaded(pack.StashBag, () => SaveStateEnableTracking(characterUID, pack.StashBag.UID));
                }
            }
        }

        /// <summary>
        /// Excludes bags with matching criteria and disables StashPack functionaly unless noted otherwise:
        /// <list type="bullet">
        /// <item>Unowned Bags which contain Items.</item>
        /// <item>Unowned Bags without Items. StashPack functionality left enabled.</item>
        /// <item>Bag owned by hosting character where the loaded scene matches the home area of the bag. </item>
        /// <item>Bags owned by remote player characters that are in the game.</item>
        /// <item>Bags owned by characters not in the game and player is not the master client.*</item>
        /// </list>
        /// *Special processing for master client. Bags are included, but with changes.
        /// <list type="number">
        /// <item>Removes PreviousOwnerUID value from Bag</item>
        /// <item>Moves bag to empty character list of StashPacks.</item>
        /// </list>
        /// </summary>
        /// <param name="stashPacks"></param>
        /// <returns></returns>
        private Dictionary<string, List<StashPack>> GetActivePacksDisableOthers(IEnumerable<StashPack> stashPacks)
        {
            var filteredPacks = new Dictionary<string, List<StashPack>>();
            foreach (var pack in stashPacks)
            {
                var characterUID = pack.StashBag.PreviousOwnerUID;
                
                
                var bagUID = pack.StashBag.UID;
                var hasPrevOwner = !string.IsNullOrWhiteSpace(characterUID);


                if ((pack.StashBag.IsInContainer || pack.StashBag.IsEquipped) && pack.StashBag.HasContents())
                {
                    Logger.LogDebug($"{nameof(LevelLoadingActions)}::{nameof(GetActivePacksDisableOthers)}: Empty Bag '{pack.StashBag.Name}' ({bagUID}) is in another container, character's inventory or equipped." +
                        $" Skipping bag.");
                    continue;
                }
                if (!pack.StashBag.IsUpdateable() && pack.StashBag.HasContents())
                {
                    Logger.LogDebug($"{nameof(LevelLoadingActions)}::{nameof(GetActivePacksDisableOthers)}: Bag '{pack.StashBag.Name}' ({bagUID}) either has no previous owner, is equipped or in another container and contains items." +
                        $" Disabling Local StashPack functionality and treating like regular BackPack.");
                    BagStateService.DisableBag(pack.StashBag.UID);
                    continue;
                }
                if (!hasPrevOwner && !pack.StashBag.HasContents())
                {
                    Logger.LogDebug($"{nameof(LevelLoadingActions)}::{nameof(GetActivePacksDisableOthers)}: Bag '{pack.StashBag.Name}' ({bagUID}) has no owner assigned and contains no contents." +
                        $" Leaving StashPack functionality enabled for next owner.");
                    continue;
                }
                if (IsLocalPlayerCharacter(characterUID))
                {
                    var character = SplitScreenManager.Instance.LocalPlayers.First(p => p.AssignedCharacter.UID.ToString() == characterUID).AssignedCharacter;
                    if (DisableHostBagIfInHomeArea(character, pack.StashBag))
                        continue;
                }
                if (!IsLocalPlayerCharacter(characterUID) && IsPlayerCharacterInGame(characterUID))
                {
                    Logger.LogDebug($"{nameof(LevelLoadingActions)}::{nameof(GetActivePacksDisableOthers)}: Non Local Character '{characterUID}' owns bag '{pack.StashBag.Name}' ({bagUID})." +
                        $" Disabling Local StashPack functionality, but leaving contents alone for remote host to handle.");
                    BagStateService.DisableBag(pack.StashBag.UID);
                    continue;
                }

                //Check if character in game. If not and are the master client
                //, clear current bag instance contents and PreviousOwnerUID.
                //Then, "move" to the empty character bag key so it can be processed later
                //and get synced to everyone.
                if (!IsPlayerCharacterInGame(characterUID))
                {
                    if (PhotonNetwork.isMasterClient)
                    {
                        Logger.LogDebug($"{nameof(LevelLoadingActions)}::{nameof(GetActivePacksDisableOthers)}: Player Character '{characterUID}' owns bag '{pack.StashBag.Name}' ({bagUID})" +
                            $" but is not in game. Removing {nameof(Bag.PreviousOwnerUID)}.");
                        pack.StashBag.PreviousOwnerUID = string.Empty;
                        if (!filteredPacks.ContainsKey(string.Empty))
                            filteredPacks.Add(string.Empty, new List<StashPack>());
                        filteredPacks[string.Empty].Add(pack);
                    }
                    else
                    {
                        Logger.LogDebug($"{nameof(LevelLoadingActions)}::{nameof(GetActivePacksDisableOthers)}: Ignoring bag '{pack.StashBag.Name}' ({bagUID}) owned by player Character '{characterUID}'." +
                            $" Character is not and game and this is not the Master Client.");
                    }
                    continue;
                }

                if (!filteredPacks.ContainsKey(characterUID))
                    filteredPacks.Add(characterUID, new List<StashPack>());

                filteredPacks[characterUID].Add(pack);
            }
            Logger.LogDebug($"{nameof(LevelLoadingActions)}::{nameof(GetActivePacksDisableOthers)}: Filtered {stashPacks.Count()} StashPacks down to {filteredPacks.Values.Select(p => p.Count).Sum()}.");
            
            return filteredPacks;
        }
        /// <summary>
        /// Scans list for characters with multiple packs for the same area.
        /// Selects the first found StashPack as the active StashPack, removes the
        /// PreviousOwnerUID from duplicate area bags and moves them to the 
        /// Unassigned (string.Empty) character list.
        /// </summary>
        /// <param name="charactersStashPacks">Collection of character StashPacks potentially containing duplicate Area StashPacks.</param>
        /// <returns>For non empty CharacterUIDs, a unique list of StashPacks. All duplicate StashPacks moved to the empty CharacterUID.</returns>
        private Dictionary<string, List<StashPack>> DisownDuplicatePacks(Dictionary<string, List<StashPack>> charactersStashPacks)
        {
            var returnPacks = new Dictionary<string, List<StashPack>>()
            {
                { string.Empty, new List<StashPack>() }
            };
            
            //Add packs that are already unclaimed to the return collection
            if (charactersStashPacks.TryGetValue(string.Empty, out var unclaimedPacks))
            {
                returnPacks[string.Empty].AddRange(unclaimedPacks);
            }

            foreach (var characterUID in charactersStashPacks.Keys.Where(k => !string.IsNullOrEmpty(k)))
            {
                if (!returnPacks.ContainsKey(characterUID))
                    returnPacks.Add(characterUID, new List<StashPack>());

                var packs = charactersStashPacks[characterUID];
                
                //Add all the packs that don't have dupes
                var uniquePacks = packs
                        .GroupBy(p => p.HomeArea)
                        .Where(g => g.Count() == 1)
                        .Select(g => g.First());
                returnPacks[characterUID].AddRange(uniquePacks);

                var areaDupes = packs
                        .GroupBy(p => p.HomeArea)
                        .Where(g => g.Count() > 1)
                        .Select(g => new { Area = g.Key, Packs = g.ToList() });
                foreach (var dupePacks in areaDupes)
                {
                    //first unowned, uncontained pack is the winner.
                    var survivor = dupePacks.Packs.FirstOrDefault(p => !p.StashBag.IsEquipped
                                                                        && !p.StashBag.IsInContainer);
                    var skipIndex = -1;
                    if (survivor == default)
                    {
                        skipIndex = dupePacks.Packs.FindIndex(p => p.StashBag.UID == survivor.StashBag.UID);
                        returnPacks[characterUID].Add(survivor);
                    }

                    int dupeCnt = 0;
                    for (int i = 1; i < dupePacks.Packs.Count(); i++)
                    {
                        if (i != skipIndex)
                        {
                            dupeCnt++;
                            dupePacks.Packs[i].StashBag.PreviousOwnerUID = string.Empty;
                            returnPacks[string.Empty].Add(dupePacks.Packs[i]);
                        }
                    }
                    Logger.LogDebug($"{nameof(LevelLoadingActions)}::{nameof(GenerateSyncPlans)}: Removed ownership of {dupeCnt} duplicate bags for Character '{characterUID}'.");
                }
            }
            var totalDupes = 0;
            if (returnPacks.TryGetValue(string.Empty, out var allDupes))
                totalDupes = allDupes.Count;
            Logger.LogDebug($"{nameof(LevelLoadingActions)}::{nameof(GenerateSyncPlans)}: Removed ownership of {totalDupes} duplicate bags out of {charactersStashPacks.Count} total stashpacks for all characters.");
            return returnPacks;
        }
        private List<ContainerSyncPlan> GenerateSyncPlans(Dictionary<string, List<StashPack>> charactersStashPacks)
        {
            var syncPlans = new List<ContainerSyncPlan>();
            var syncPlanner = _instances.GetSyncPlanner();

            foreach (var characterUID in charactersStashPacks.Keys)
            {
                var packs = charactersStashPacks[characterUID];
                if (string.IsNullOrWhiteSpace(characterUID))
                {
                    foreach (var unownedPack in packs)
                    {
                        var bagSave = unownedPack.StashBag.ToBagState(string.Empty, _instances.AreaStashPackItemIds);
                        syncPlans.Add(syncPlanner.PlanSync(bagSave, 0, null));
                    }
                    continue;
                }
                //Unable to get a StashSaveData at this point is unexpected. Shouldn't ever happen,
                //but if it does log an exception and disable all this character's bags as a precaution.
                if (!_instances.TryGetStashSaveData(characterUID, out var stashSaveData))
                {
                    Logger.LogError($"{nameof(LevelLoadingActions)}::{nameof(GenerateSyncPlans)}: Could not retrieve a {nameof(StashSaveData)} instance. " +
                        $" A Sync Plan will not be generated for any of character's ({characterUID}) bags. Disabling StashPack functionality for all character's bags.");
                    foreach (var p in packs)
                        BagStateService.DisableBag(p.StashBag.UID);
                    continue;
                }
                foreach (var pack in packs)
                {
                    var bagSave = pack.StashBag.ToBagState(characterUID, _instances.AreaStashPackItemIds);
                    var stashSave = stashSaveData.GetStashSave(pack.HomeArea);
                    if (stashSave != null)
                    {
                        var stashItems = stashSave.ItemsSaveData.Select(i => i.ToUpdatedHierachy(pack.StashBag.UID + StashPacksConstants.BagUidSuffix, pack.StashBag.Container.ItemID));
                        syncPlans.Add(
                            syncPlanner.PlanSync(bagSave, stashSave.BasicSaveData.GetContainerSilver(), stashItems)
                            );
                    }
                    else
                    {
                        Logger.LogDebug($"{nameof(LevelLoadingActions)}::{nameof(GenerateSyncPlans)}: No Sync Plan can be generated for character's bag.  Character does not have a" +
                            $" {pack.HomeArea.GetName()} area save for bag '{pack.StashBag.Name}' ({pack.StashBag.UID})." +
                            $" Disabling StashBag functionality.");
                        BagStateService.DisableBag(pack.StashBag.UID);
                    }
                }
            }
            return syncPlans;
        }
    }
}
