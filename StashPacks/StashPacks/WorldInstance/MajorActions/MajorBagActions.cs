using ModifAmorphic.Outward.Coroutines;
using ModifAmorphic.Outward.Extensions;
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
        protected void DestroyBag(Bag bag)
        {
            _instances.TryGetItemManager(out var itemManager);
            if (bag.HasContents())
            {
                var bagContents = bag.Container.GetContainedItems().ToList();
                foreach (var item in bagContents)
                {
                    if (!PhotonNetwork.isNonMasterClientInRoom)
                    {
                        itemManager.DestroyItem(item.UID);
                    }
                    else
                    {
                        itemManager.SendDestroyItem(item.UID);
                    }
                }
                bag.Container.RemoveAllSilver();
            }
            int bagItemID = bag.ItemID;
            if (!PhotonNetwork.isNonMasterClientInRoom)
            {
                Logger.LogDebug($"{nameof(MajorBagActions)}::{nameof(DestroyBag)}: Destroying Bag {bag.Name} ({bag.UID}).");
                itemManager.DestroyItem(bag.UID);
            }
            else
            {
                Logger.LogDebug($"{nameof(MajorBagActions)}::{nameof(DestroyBag)}: Sending Request to Destroy Bag {bag.Name} ({bag.UID}).");
                itemManager.SendDestroyItem(bag.UID);
            }
        }

        #region Coroutines
        protected void DoAfterLevelLoaded(NetworkLevelLoader networkLevelLoader, Action action)
        {
            const int timeoutSeconds = 250;
            const float waitTime = .25f;

            var levelCoroutine = _instances.GetCoroutine<LevelCoroutines>();
            levelCoroutine.AfterLevelLoaded(networkLevelLoader, action, timeoutSeconds, waitTime);
        }
        protected void DoAfterPlayerLeftCoroutine(string playerUID, Action action)
        {
            const int timeoutSeconds = 30;
            const float waitTime = 1f;

            var playerCoroutines = _instances.GetCoroutine<PlayerCoroutines>();

            playerCoroutines.InvokeAfterPlayerLeft(playerUID, action, timeoutSeconds, waitTime);
        }
        protected void DoAfterBagLoaded(Bag bag, Action<Item> action)
        {
            const int timeoutSeconds = 30;
            const float waitTime = .2f;

            _instances.TryGetItemManager(out var itemManager);
            var bagUID = bag.UID.ToString();
            var itemCoroutines = _instances.GetCoroutine<ItemCoroutines>();
            Item getBagItem(string uid) => itemManager.GetItem(uid);
            bool isBagReady()
            {
                var worldBag = getBagItem(bagUID);
                return worldBag != null &&
                    !string.IsNullOrWhiteSpace(worldBag.PreviousOwnerUID) &&
                    worldBag.FullyInitialized &&
                    worldBag.IsWorldDetectable;
            }
            bool cancelIf() => getBagItem(bagUID) == null;

            itemCoroutines.InvokeAfterItemLoaded(bagUID, isBagReady, action, timeoutSeconds, waitTime, cancelIf);
        }
        protected void DoAfterBagLoaded(string bagUID, Action<Item> action)
        {
            const int timeoutSeconds = 30;
            const float waitTime = .2f;

            _instances.TryGetItemManager(out var itemManager);

            var itemCoroutines = _instances.GetCoroutine<ItemCoroutines>();
            bool isBagReady()
            {
                var worldBag = itemManager.GetItem(bagUID);
                return worldBag != null &&
                    !string.IsNullOrWhiteSpace(worldBag.PreviousOwnerUID) &&
                    worldBag.FullyInitialized &&
                    worldBag.IsWorldDetectable;
            }
            itemCoroutines.InvokeAfterItemLoaded(bagUID, isBagReady, action, timeoutSeconds, waitTime);
        }
        protected void DoAfterBagTracked(Bag bag, BagStateService bagStateService, Action action)
        {
            const int timeoutSeconds = 15;
            const float ticTime = .1f;
            bool isBagTracked() =>
                !bagStateService.TryGetContentChangesTracked(bag.ItemID, out var isTracked) || isTracked
                || bagStateService.TryGetState(bag.ItemID, out _)
                || (!bagStateService.TryGetState(bag.ItemID, out _) && !bag.HasContents());

            var itemCoroutines = _instances.GetCoroutine<ItemCoroutines>();
            _instances.UnityPlugin.StartCoroutine(
                itemCoroutines.InvokeAfter(isBagTracked, action, timeoutSeconds, ticTime)
            );
        }
        private bool GetIsItemMoving(Item item, Rigidbody rigidbody) =>
                    (item == null || rigidbody == null
                    || rigidbody.velocity.magnitude > 0f
                    || rigidbody.velocity.x > 0f || rigidbody.velocity.y > 0f || rigidbody.velocity.z > 0f);

        protected void DoWhileBagFalling(string bagUID, Action<Bag> fallingAction, Action<Bag> landedAction = null)
        {
            const int timeoutSeconds = 10;
            const float ticTime = .1f;

            _instances.TryGetItemManager(out var itemManager);

            Bag getBag() => itemManager.GetItem(bagUID) as Bag;
            bool cancelIf() => getBag() == null;
            bool hasBagLanded()
            {
                var bag = getBag();
                var rigidbody = bag?.GetComponent<Rigidbody>();

                var isFalling = GetIsItemMoving(bag, rigidbody);
                if (isFalling && rigidbody != null)
                {
                    try
                    {
#if DEBUG
                        Logger.LogTrace($"{nameof(MajorBagActions)}::{nameof(DoWhileBagFalling)}:" +
                            $"Invoking Falling Action {fallingAction.Method.Name} for Bag '{bag?.Name}' ({bag?.UID})");
#endif
                        fallingAction.Invoke(bag);
                    }
                    catch (Exception ex)
                    {
                        Logger.LogException($"{nameof(MajorBagActions)}::{nameof(DoWhileBagFalling)}:" +
                            $" Unexpected exception invoking {nameof(fallingAction)} Action {fallingAction.Method.Name} for Bag '{bag?.Name}' ({bag?.UID})", ex);
                    }
                }
                return !isFalling;
            }

            var itemCoroutines = _instances.GetCoroutine<ItemCoroutines>();
            _instances.UnityPlugin.StartCoroutine(
                itemCoroutines.InvokeAfter(hasBagLanded, landedAction, getBag, timeoutSeconds, ticTime, cancelIf)
            );

        }

        protected void DoAfterBagLanded(string bagUID, Action action)
        {

            const int timeoutSeconds = 25;
            const float ticTime = .3f;

            _instances.TryGetItemManager(out var itemManager);

            bool hasBagLanded()
            {
                var worldBag = itemManager.GetItem(bagUID);
                var rigidBody = worldBag?.GetComponent<Rigidbody>();
                return !GetIsItemMoving(worldBag, rigidBody);
            }

            var itemCoroutines = _instances.GetCoroutine<ItemCoroutines>();
            bool isBagReady()
            {
                var worldBag = itemManager.GetItem(bagUID);
                return worldBag != null &&
                    !string.IsNullOrWhiteSpace(worldBag.PreviousOwnerUID) &&
                    worldBag.FullyInitialized &&
                    worldBag.IsWorldDetectable;
            }
            _instances.UnityPlugin.StartCoroutine(
                itemCoroutines.InvokeAfter(hasBagLanded, action, timeoutSeconds, ticTime)
            );
        }

        protected void DoAfterBagDestroyed(string bagUID, Action action)
        {
            const int timeoutSeconds = 30;
            const float waitTime = .1f;

            var itemCoroutines = _instances.GetCoroutine<ItemCoroutines>();
            itemCoroutines.StartAfterItemDestroyed(bagUID, action, timeoutSeconds, waitTime);
        }
        #endregion
    }
}
