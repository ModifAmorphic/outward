using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.StashPacks.Extensions;
using ModifAmorphic.Outward.StashPacks.Settings;
using ModifAmorphic.Outward.StashPacks.State;
using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ModifAmorphic.Outward.StashPacks.WorldInstance.MajorActions
{
    internal abstract class MajorBagActions
    {
        protected readonly InstanceFactory _instances;

        protected IModifLogger Logger => _getLogger.Invoke();
        protected readonly Func<IModifLogger> _getLogger;
        private bool _lastKnownAllScenesEnabled = false;
        protected bool LastKnownAllScenesEnabled => _lastKnownAllScenesEnabled;

        public MajorBagActions(InstanceFactory instances, Func<IModifLogger> getLogger)
        {
            (_instances, _getLogger) = (instances, getLogger);
            SceneManager.sceneLoaded += (s, l) =>
            {
                _lastKnownAllScenesEnabled = instances.HostSettings.AllScenesEnabled;
            };
        }

        public abstract void SubscribeToEvents();

        protected PlayerSystem GetPlayerSystem(string characterUID)
        {
            return Global.Lobby.PlayersInLobby.FirstOrDefault(ps => ps.CharUID == characterUID);
        }
        protected bool IsHomeStashInWorld(Character character, Bag bag)
        {
            return character.IsHostCharacter() && GetBagAreaEnum(bag) == GetCurrentAreaEnum();
        }
        protected bool IsWorldLoaded()
        {
            return NetworkLevelLoader.Instance.IsOverallLoadingDone;
        }
        protected bool IsCurrentSceneStashPackEnabled()
        {
            return StashPacksConstants.PermenantStashUids.ContainsKey(GetCurrentAreaEnum()) || LastKnownAllScenesEnabled;
        }
        protected bool IsLocalPlayerCharacter(string characterUID)
        {
            return SplitScreenManager.Instance.LocalPlayers.Any(p => p.AssignedCharacter.UID.ToString() == characterUID);
        }
        protected bool IsPlayerCharacterInGame(string characterUID)
        {
            return Global.Lobby.PlayersInLobby.Any(ps => ps.CharUID == characterUID);
        }
        protected AreaManager.AreaEnum GetCurrentAreaEnum()
        {
            var sceneName = _instances.AreaManager.CurrentArea.SceneName;
            return (AreaManager.AreaEnum)_instances.AreaManager.GetAreaIndexFromSceneName(sceneName);
        }
        protected AreaManager.AreaEnum GetBagAreaEnum(Bag bag)
        {
            return _instances.AreaStashPackItemIds[bag.ItemID];
        }
        protected void DoAfterBagLoaded(string bagUID, Action<Bag> action)
        {
            _instances.UnityPlugin.StartCoroutine(AfterBagLoadedCoroutine(bagUID, action));
        }
        protected bool DisableHostBagIfInHomeArea(Character character, Bag bag)
        {
            if (IsHomeStashInWorld(character, bag))
            {
                Logger.LogDebug($"{nameof(MajorBagActions)}::{nameof(DisableHostBagIfInHomeArea)}: Character '{character.UID}' is hosting the game" +
                    $" and is in '{bag.Name}' ({bag.UID}) home area {GetBagAreaEnum(bag).GetName()}. StashBag functionalty disabled for bag.");

                UnclaimAndClearBag(bag);
                return true;
            }
            return false;
        }
        protected void NetworkSyncBagContents(Bag bag)
        {
            if (_instances.TryGetItemManager(out var itemManger))
            {
                foreach (var i in bag.Container.GetContainedItems())
                {
                    if (PhotonNetwork.isNonMasterClientInRoom)
                    {
                        Logger.LogTrace($"{nameof(MajorBagActions)}::{nameof(NetworkSyncBagContents)}: Sending network data" +
                            $" to master client for item '{i.Name}'.");
                        Global.RPCManager.SendItemSyncToMaster(i.ToNetworkData());
                    }
                    else
                    {
                        Logger.LogTrace($"{nameof(MajorBagActions)}::{nameof(NetworkSyncBagContents)}: Sending network data" +
                            $" to clients for item '{i.Name}'.");
                        itemManger.AddItemToSyncToClients(i.UID);
                    }
                }
            }
        }
        protected void NetworkSyncBag(Bag bag)
        {
            if (_instances.TryGetItemManager(out var itemManger))
            {
                if (PhotonNetwork.isNonMasterClientInRoom)
                {
                    Logger.LogTrace($"{nameof(MajorBagActions)}::{nameof(NetworkSyncBag)}: Sending network data" +
                        $" to master client for Bag '{bag.Name}' ({bag.UID}).");
                    Global.RPCManager.SendItemSyncToMaster(bag.ToNetworkData());
                    //Global.RPCManager.SendItemSyncToMaster(bag.Container.ToNetworkData());
                }
                else
                {
                    Logger.LogTrace($"{nameof(MajorBagActions)}::{nameof(NetworkSyncBag)}: Sending network data" +
                        $" to clients for Bag '{bag.Name}' ({bag.UID}).");
                    itemManger.AddItemToSyncToClients(bag.UID);
                    //itemManger.AddItemToSyncToClients(bag.Container.UID);
                }
            }
        }
        protected void SaveStateEnableTracking(string characterUID, string bagUID)
        {
            var bagStates = _instances.GetBagStateService(characterUID);
            if (!_instances.TryGetItemManager(out var itemManager))
            {
                Logger.LogWarning($"{nameof(MajorBagActions)}::{nameof(SaveStateEnableTracking)}: Unable to retrieve an '{nameof(ItemManager)}' instance for bag '{bagUID}'. Bag state will not be saved." +
                    $"  Disabling StashBag functionalty this bag.");
                BagStateService.DisableBag(bagUID);
                return;
            }

            var bagInstance = (Bag)itemManager.GetItem(bagUID);
            if (bagInstance == null || !bagStates.TrySaveState(bagInstance))
            {
                Logger.LogWarning($"{nameof(MajorBagActions)}::{nameof(SaveStateEnableTracking)}: Unable to save state for character '{characterUID}' bag '{bagInstance.Name}' ({bagInstance.UID})." +
                    $"  Disabling StashBag functionalty this bag.");
                BagStateService.DisableBag(bagInstance.UID);
                return;
            }
            NetworkSyncBagContents(bagInstance);
            bagStates.EnableContentChangeTracking(bagInstance.ItemID);
        }

        protected void UnclaimClearOtherBags(string characterUID, Bag claimedBag)
        {
            if (_instances.TryGetStashPackWorldData(out var stashPackWorldData))
            {
                var packs = stashPackWorldData.GetStashPacks(characterUID);
                if (packs != null)
                {
                    var bagsToFree = packs.Where(p => p.StashBag.ItemID == claimedBag.ItemID && p.StashBag.UID != claimedBag.UID && p.StashBag.IsUsable()).Select(p => p.StashBag);
                    foreach (var bag in bagsToFree)
                    {
                        UnclaimAndClearBag(bag);
                    }
                }
            }
        }
        protected void UnclaimAndClearBag(Bag bag)
        {
            var previousOwnerUID = bag.PreviousOwnerUID;
            bag.PreviousOwnerUID = string.Empty;
            bag.EmptyContents();
            bag.Container.AllowOverCapacity = false;
            Logger.LogDebug($"{nameof(MajorBagActions)}::{nameof(UnclaimClearOtherBags)}: Removed character's ({previousOwnerUID}) claim from bag {bag.Name} ({bag.UID}) and emptied its contents.");
            NetworkSyncBag(bag);
            _instances.StashPackNet.SendStashPackLinkChanged(bag.UID, previousOwnerUID, false);
        }

        #region Coroutines
        protected IEnumerator AfterLevelLoadedCoroutine(NetworkLevelLoader networkLevelLoader, Action action)
        {
            const int maxWaits = 1000;
            const float waitTime = .25f;

            var waits = 0;

            while (!networkLevelLoader.IsOverallLoadingDone && waits++ < maxWaits)
            {
                yield return new WaitForSeconds(waitTime);
            }
            if (waits < maxWaits)
            {
                Logger.LogDebug($"{nameof(MajorBagActions)}::{nameof(AfterLevelLoadedCoroutine)}: Level Scene {networkLevelLoader.TargetScene} finished loading." +
                    $" Invoking action {action.Method.Name}.");
                try
                {
                    action.Invoke();
                }
                catch (Exception ex)
                {
                    Logger.LogException($"{nameof(MajorBagActions)}::{nameof(AfterLevelLoadedCoroutine)}:" +
                        $" Unexpected exception invoking Action {action.Method.Name}.", ex);
                }
            }
            else
            {
                Logger.LogError($"{nameof(MajorBagActions)}::{nameof(AfterLevelLoadedCoroutine)}: Timed out after waiting {waits} times for Level Scene {networkLevelLoader.TargetScene} to finish loading." +
                    $" Action not invoked: {action.Method.Name}.");
            }
        }
        protected IEnumerator AfterPlayerLeftCoroutine(string playerUID, Action action)
        {
            var waits = 0;
            const int maxWaits = 30;
            const float waitTime = 1f;

            while (Global.Lobby.PlayersInLobby.Any(p => p.UID == playerUID)
                && waits++ < maxWaits)
            {
                Logger.LogTrace($"{nameof(MajorBagActions)}::{nameof(AfterPlayerLeftCoroutine)}: Waited {waits} times for " +
                    $"playerUID {playerUID} to leave the game." +
                    $"\n\tGlobal.Lobby.PlayersInLobby {Global.Lobby.PlayersInLobby.Count}.");
                yield return new WaitForSeconds(waitTime);
            }
            if (!Global.Lobby.PlayersInLobby.Any(p => p.UID == playerUID))
            {
                Logger.LogDebug($"{nameof(MajorBagActions)}::{nameof(AfterPlayerLeftCoroutine)}: playerUID {playerUID} left the game." +
                    $" Invoking action {action.Method.Name}." +
                    $"\n\tGlobal.Lobby.PlayersInLobby {Global.Lobby.PlayersInLobby.Count}.");
                try
                {
                    action.Invoke();
                }
                catch (Exception ex)
                {
                    Logger.LogException($"{nameof(MajorBagActions)}::{nameof(AfterPlayerLeftCoroutine)}:" +
                        $" Unexpected exception invoking Action {action.Method.Name} for playerUID '{playerUID}'.", ex);
                }
            }
            else
            {
                Logger.LogError($"{nameof(MajorBagActions)}::{nameof(AfterBagLoadedCoroutine)}: Timed out after waiting {waits} times for PhotonPlayerID {playerUID} to leave game." +
                        $" Action not invoked: {action.Method.Name}.");
            }
        }
        protected IEnumerator AfterBagLoadedCoroutine(string bagUID, Action<Bag> action)
        {
            const int maxWaits = 240;
            const float waitTime = .25f;

            if (!_instances.TryGetItemManager(out var itemManager))
                yield break;

            var worldBag = itemManager.GetItem(bagUID);
            int waits = 0;

            while ((!itemManager.IsAllItemSynced || worldBag == null || string.IsNullOrWhiteSpace(worldBag.PreviousOwnerUID) || !worldBag.FullyInitialized || !worldBag.IsWorldDetectable)
                && waits++ < maxWaits)
            {
                if (worldBag == null)
                    worldBag = itemManager.GetItem(bagUID);
                yield return new WaitForSeconds(waitTime);
            }
            try
            {
                if (waits >= maxWaits)
                    Logger.LogError($"{nameof(MajorBagActions)}::{nameof(AfterBagLoadedCoroutine)}: Timed out after waiting {waits} times for Bag '{bagUID}' to load." +
                        $" Action not invoked: {action.Method.Name}.");
                else if (worldBag is Bag)
                {
                    Logger.LogDebug($"{nameof(MajorBagActions)}::{nameof(AfterBagLoadedCoroutine)}: Bag '{worldBag.Name}' ({worldBag.UID}) finished" +
                            $" loading into world. Invoking action {action.Method.Name}.");
                    action.Invoke(worldBag as Bag);
                }
                else
                    Logger.LogError($"{nameof(MajorBagActions)}::{nameof(AfterBagLoadedCoroutine)}: Unexpected error. Item with " +
                            $"UID '{bagUID}' is not a Bag. Not invoking action {action.Method.Name}.");
            }
            catch (Exception ex)
            {
                Logger.LogException($"{nameof(MajorBagActions)}::{nameof(AfterBagLoadedCoroutine)}:" +
                    $" Unexpected exception invoking Action {action.Method.Name} for Bag '{worldBag?.Name}' ({worldBag?.UID})", ex);
            }
        }

        protected IEnumerator WhileBagFallingCoroutine(string bagUID, Action<Bag> fallingAction, Action<Bag> landedAction = null)
        {
            const int maxWaits = 75;
            const float waitTime = .1f;

            if (!_instances.TryGetItemManager(out var itemManager))
                yield break;

            var bag = itemManager.GetItem(bagUID) as Bag;
            var rigidBody = bag?.GetComponent<Rigidbody>();

            var bagFound = bag != null;
            var bagDestroyed = false;

            var waits = 0;
            while ((bag == null || rigidBody == null
                || rigidBody?.velocity.magnitude > 0f
                || rigidBody?.velocity.x > 0f || rigidBody?.velocity.y > 0f || rigidBody?.velocity.z > 0f)
                && waits++ < maxWaits)
            {

                if (bag == null)
                {
                    if (bagFound)
                    {
                        bagDestroyed = true;
                        break;
                    }

                    bag = itemManager.GetItem(bagUID) as Bag;
                }
                if (bag != null)
                {
                    rigidBody = bag?.GetComponent<Rigidbody>();
                    try
                    {
                        bagFound = true;
                        fallingAction.Invoke(bag);
                    }
                    catch (Exception ex)
                    {
                        Logger.LogException($"{nameof(MajorBagActions)}::{nameof(WhileBagFallingCoroutine)}:" +
                            $" Unexpected exception invoking {nameof(fallingAction)} Action {fallingAction.Method.Name} for Bag '{bag?.Name}' ({bag?.UID})", ex);
                    }
                }

                Logger.LogTrace($"{nameof(MajorBagActions)}::{nameof(WhileBagFallingCoroutine)}: Bag '{bag?.Name}' ({bagUID}) Velocity " +
                    $"({rigidBody?.velocity.magnitude}, {rigidBody?.velocity.x}, {rigidBody?.velocity.y}, {rigidBody?.velocity.z}). Waited {waits} times.");
                yield return new WaitForSeconds(waitTime);
            }

            try
            {
                if (landedAction != null && !bagDestroyed && waits < maxWaits)
                {
                    Logger.LogDebug($"{nameof(MajorBagActions)}::{nameof(WhileBagFallingCoroutine)}: Bag '{bag.Name}' ({bag.UID}) finished" +
                            $" falling. Invoking action {landedAction.Method.Name}");
                    landedAction.Invoke(bag);
                }
                else if (waits >= maxWaits)
                    if (landedAction != null)
                        Logger.LogError($"{nameof(MajorBagActions)}::{nameof(WhileBagFallingCoroutine)}: Timed out after waiting {waits} times for Bag '{bagUID}' to land." +
                            $" Landed Action not invoked: {landedAction.Method.Name}.");
                    else
                        Logger.LogWarning($"{nameof(MajorBagActions)}::{nameof(WhileBagFallingCoroutine)}: Timed out after waiting {waits} times for Bag '{bagUID}' to land.");
                else
                    Logger.LogDebug($"{nameof(MajorBagActions)}::{nameof(WhileBagFallingCoroutine)}: Bag '{bag.Name}' ({bag.UID}) finished" +
                            $" falling.");
            }
            catch (Exception ex)
            {
                Logger.LogException($"{nameof(MajorBagActions)}::{nameof(WhileBagFallingCoroutine)}:" +
                    $" Unexpected exception invoking {nameof(landedAction)} Action {landedAction.Method.Name} for Bag '{bag?.Name}' ({bag?.UID})", ex);
            }

        }

        protected IEnumerator AfterBagLandedCoroutine(Bag bag, Action action)
        {
            const int maxWaits = 75;
            const float waitTime = .3f;

            if (!_instances.TryGetItemManager(out var itemManager))
                yield break;

            yield return new WaitForSeconds(waitTime);

            var bagUID = bag.UID;
            var worldBag = itemManager.GetItem(bagUID);
            var rigidBody = worldBag?.GetComponent<Rigidbody>();
            var waits = 0;
            var bagFound = worldBag != null;
            var bagDestroyed = false;

            while ((worldBag == null || rigidBody == null
                || rigidBody.velocity.magnitude > 0f
                || rigidBody.velocity.x > 0f || rigidBody.velocity.y > 0f || rigidBody.velocity.z > 0f)
                && waits++ < maxWaits)
            {
                if (worldBag == null)
                {
                    if (bagFound)
                    {
                        bagDestroyed = true;
                        break;
                    }
                    worldBag = itemManager.GetItem(bagUID);
                }
                if (worldBag != null)
                    rigidBody = worldBag.GetComponent<Rigidbody>();

                Logger.LogTrace($"{nameof(MajorBagActions)}::{nameof(AfterBagLandedCoroutine)}: Bag '{bag.Name}' ({bag.UID}) Velocity " +
                    $"({rigidBody?.velocity.magnitude}, {rigidBody?.velocity.x}, {rigidBody?.velocity.y}, {rigidBody?.velocity.z}). Waited {waits} times.");
                yield return new WaitForSeconds(waitTime);
            }
            try
            {
                if (!bagDestroyed && waits < maxWaits)
                {
                    Logger.LogDebug($"{nameof(MajorBagActions)}::{nameof(AfterBagLandedCoroutine)}: Bag '{bag.Name}' ({bag.UID}) finished" +
                                    $" moving. Invoking action {action.Method.Name}.");
                    action.Invoke();
                }
                else if (waits >= maxWaits)
                    Logger.LogError($"{nameof(MajorBagActions)}::{nameof(AfterBagLandedCoroutine)}: Timed out after waiting {waits} times for Bag '{bagUID}' to land." +
                        $" Action not invoked: {action.Method.Name}.");
                else
                    Logger.LogDebug($"{nameof(MajorBagActions)}::{nameof(AfterBagLandedCoroutine)}: Bag '{bagUID}' was destroyed. " +
                        $" Action {action.Method.Name} NOT invoked.");
            }
            catch (Exception ex)
            {
                Logger.LogException($"{nameof(MajorBagActions)}::{nameof(AfterBagLandedCoroutine)}:" +
                    $" Unexpected exception invoking Action {action.Method.Name} for Bag '{bag?.Name}' ({bag?.UID})", ex);
            }
        }

        protected IEnumerator AfterBagDestroyedCoroutine(string bagUID, Action action)
        {
            const int maxWaits = 300;
            const float waitTime = .1f;

            if (!_instances.TryGetItemManager(out var itemManager))
                yield break;

            var waits = 0;

            while (itemManager.GetItem(bagUID) != null && waits++ < maxWaits)
            {
                yield return new WaitForSeconds(waitTime);
            }
            if (waits < maxWaits)
            {
                Logger.LogDebug($"{nameof(MajorBagActions)}::{nameof(AfterBagDestroyedCoroutine)}: Bag '{bagUID}' destroyed." +
                    $" Invoking action {action.Method.Name}.");
                try
                {
                    action.Invoke();
                }
                catch (Exception ex)
                {
                    Logger.LogException($"{nameof(MajorBagActions)}::{nameof(AfterBagDestroyedCoroutine)}:" +
                        $" Unexpected exception invoking Action {action.Method.Name} for Bag '{bagUID}'.", ex);
                }
            }
            else
            {
                Logger.LogError($"{nameof(MajorBagActions)}::{nameof(AfterBagDestroyedCoroutine)}: Timed out after waiting {waits} times for Bag '{bagUID}' to be destroyed." +
                    $" Action not invoked: {action.Method.Name}.");
            }
        }
        #endregion
    }
}
