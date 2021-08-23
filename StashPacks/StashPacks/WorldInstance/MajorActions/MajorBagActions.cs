using ModifAmorphic.Outward.StashPacks.Extensions;
using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.StashPacks.Settings;
using ModifAmorphic.Outward.StashPacks.State;
using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using ModifAmorphic.Outward.Extensions;
using System.Reflection;

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
                _lastKnownAllScenesEnabled = instances.StashPacksSettings.AllScenesEnabled.Value;
            };
        }

        public abstract void SubscribeToEvents();

        protected bool IsWorldLoaded()
        {
            return NetworkLevelLoader.Instance.IsOverallLoadingDone;
        }
        protected bool IsCurrentSceneStashPackEnabled()
        {
            return StashPacksConstants.PermenantStashUids.ContainsKey(GetCurrentAreaEnum()) || LastKnownAllScenesEnabled; 
        }
        protected bool IsHost(Character character)
        {
            return character.OwnerPlayerSys.IsMasterClient && character.OwnerPlayerSys.PlayerID == 0;
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
        protected void DisableBag(Bag bag)
        {
            bag.Container.AllowOverCapacity = false;
            BagStateService.DisableBag(bag.UID);
        }
        protected bool DisableHostBagIfInHomeArea(Character character, Bag bag)
        {
            if (IsHost(character) && GetBagAreaEnum(bag) == GetCurrentAreaEnum())
            {
                Logger.LogDebug($"{nameof(MajorBagActions)}::{nameof(DisableHostBagIfInHomeArea)}: Character '{character.UID}' is hosting the game" +
                    $" and is in '{bag.Name}' ({bag.UID}) home area {GetBagAreaEnum(bag).GetName()}. StashBag functionalty disabled for bag.");
                DisableBag(bag);
                return true;
            }
            return false;
        }
        protected void NetworkSyncBag(Bag bag)
        {
            if (_instances.TryGetItemManager(out var itemManger))
            {
                foreach (var i in bag.Container.GetContainedItems())
                {
                    if (PhotonNetwork.isNonMasterClientInRoom)
                    {
                        Logger.LogTrace($"{nameof(MajorBagActions)}::{nameof(NetworkSyncBag)}: Sending network data" +
                            $" to master client for item '{i.Name}'.");
                        Global.RPCManager.SendItemSyncToMaster(i.ToNetworkData());
                    }
                    else
                    {
                        Logger.LogTrace($"{nameof(MajorBagActions)}::{nameof(NetworkSyncBag)}: Sending network data" +
                            $" to clients for item '{i.Name}'.");
                        itemManger.AddItemToSyncToClients(i.UID);
                    }
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
            NetworkSyncBag(bagInstance);
            bagStates.SetSyncedFromStash(bagInstance.ItemID, true);
            bagStates.EnableTracking(bagInstance.ItemID);
        }
        protected void UnclaimClearOtherBags(string characterUID, Bag claimedBag)
        {
            if (_instances.TryGetStashPackWorldData(out var stashPackWorldData))
            {
                var packs = stashPackWorldData.GetStashPacks(characterUID);
                if (packs != null)
                {
                    var bagsToFree = packs.Where(p => p.StashBag.ItemID == claimedBag.ItemID && p.StashBag.UID != claimedBag.UID && p.StashBag.IsUpdateable()).Select(p => p.StashBag);
                    foreach (var bag in bagsToFree)
                    {
                        //bag.PreviousOwnerUID = string.Empty;
                        bag.OnContainerChangedOwner(null);
                        bag.OnContainerChangedOwner(null);
                        bag.EmptyContents();
                        Logger.LogDebug($"{nameof(BagDropActions)}::{nameof(UnclaimClearOtherBags)}: Removed character's ({characterUID}) claim from bag {bag.Name} ({bag.UID}) and emptied its contents.");
                    }
                }
            }
        }
        protected static Vector3 GetTerrainPos(float x, float y)
        {
            //Create object to store raycast data
            RaycastHit hit;

            //Create origin for raycast that is above the terrain. I chose 100.
            Vector3 origin = new Vector3(x, 100, y);

            //Send the raycast.
            Physics.Raycast(origin, Vector3.down, out hit, Mathf.Infinity);

            Debug.Log("Terrain location found at " + hit.point);
            return hit.point;
        }
        protected IEnumerator AfterBagLoadedCoroutine(string bagUID, Action<Bag> action)
        {
            if (_instances.TryGetItemManager(out var itemManager))
            {
                var worldBag = itemManager.GetItem(bagUID);

                while (!itemManager.IsAllItemSynced || worldBag == null || string.IsNullOrWhiteSpace(worldBag.PreviousOwnerUID))
                {
                    worldBag = itemManager.GetItem(bagUID);
                    yield return new WaitForSeconds(.5f);
                }
                Logger.LogDebug($"{nameof(MajorBagActions)}::{nameof(AfterBagLoadedCoroutine)}: Bag '{worldBag.Name}' ({worldBag.UID}) finished" +
                    $" loading into world. Invoking action {action.Method.Name}.");
                action.Invoke((Bag)worldBag);
            }
            else
            {
                Logger.LogError($"{nameof(MajorBagActions)}::{nameof(AfterBagLoadedCoroutine)}: Unexpected error. Unable " +
                    $"to retrieve {nameof(ItemManager)} instance. StashPack functionality may not work correctly for " +
                    $"bag '{bagUID}'");
            }
        }

        protected IEnumerator AfterBagLandedCoroutine(Bag bag, Action action)
        {
            yield return new WaitForSeconds(.2f);

            if (_instances.TryGetItemManager(out var itemManager))
            {
                var worldBag = itemManager.GetItem(bag.UID);
                var rigidBody = worldBag.GetComponent<Rigidbody>();
                var waits = 0;

                while (worldBag == null || rigidBody == null 
                    || rigidBody.velocity.magnitude > 0f
                    || rigidBody.velocity.x > 0f || rigidBody.velocity.y > 0f || rigidBody.velocity.z > 0f)
                {
                    worldBag = itemManager.GetItem(bag.UID);
                    rigidBody = worldBag.GetComponent<Rigidbody>();
                    waits++;
                    Logger.LogTrace($"{nameof(MajorBagActions)}::{nameof(AfterBagLandedCoroutine)}: Bag '{bag.Name}' ({bag.UID}) Velocity " +
                        $"({rigidBody.velocity.magnitude}, {rigidBody.velocity.x}, {rigidBody.velocity.y}, {rigidBody.velocity.z}). Waited {waits} times.");
                    yield return new WaitForSeconds(.1f);
                }
                Logger.LogDebug($"{nameof(MajorBagActions)}::{nameof(AfterBagLandedCoroutine)}: Bag '{bag.Name}' ({bag.UID}) finished" +
                    $" moving. Invoking action {action.Method.Name}.");
                action.Invoke();
            }
            else
            {
                Logger.LogError($"{nameof(MajorBagActions)}::{nameof(AfterBagLandedCoroutine)}: Unexpected error. Unable " +
                    $"to retrieve {nameof(ItemManager)} instance. StashPack functionality may not work correctly for " +
                    $"bag '{bag.Name}' ({bag.UID})");
            }
        }
        protected IEnumerator AfterBagDestroyedCoroutine(string bagUID, Action action)
        {
            yield return new WaitForSeconds(.2f);

            if (_instances.TryGetItemManager(out var itemManager))
            {
                var bag = itemManager.GetItem(bagUID);
                var waits = 0;

                while (bag != null && waits < 300)
                {
                    bag = itemManager.GetItem(bagUID);
                    waits++;
                    yield return new WaitForSeconds(.1f);
                }
                if (waits < 300)
                {
                    Logger.LogDebug($"{nameof(MajorBagActions)}::{nameof(AfterBagDestroyedCoroutine)}: Bag '{bagUID}' destroyed." +
                        $" Invoking action {action.Method.Name}.");
                    action.Invoke();
                }
                else
                {
                    Logger.LogError($"{nameof(MajorBagActions)}::{nameof(AfterBagDestroyedCoroutine)}: Bag '{bagUID}' not destroyed." +
                        $" Action not invoked: {action.Method.Name}.");
                }
            }
            else
            {
                Logger.LogError($"{nameof(MajorBagActions)}::{nameof(AfterBagLandedCoroutine)}: Unexpected error. Unable " +
                    $"to retrieve {nameof(ItemManager)} instance. StashPack functionality may not work correctly for " +
                    $"bag '{bagUID}'");
            }
        }
    }
}
