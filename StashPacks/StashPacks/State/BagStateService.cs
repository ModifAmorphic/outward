using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.StashPacks.Extensions;
using ModifAmorphic.Outward.StashPacks.Network;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace ModifAmorphic.Outward.StashPacks.State
{
    internal class BagStateService
    {
        private readonly string _characterUID;
        public string CharacterUID => _characterUID;

        private readonly InstanceFactory _instances;

        private readonly ConcurrentDictionary<int, BagState> _bagsStates = new ConcurrentDictionary<int, BagState>();

        private readonly ConcurrentDictionary<int, bool> _contentChangesTracked = new ConcurrentDictionary<int, bool>();

        private readonly static ConcurrentDictionary<string, byte> _disabledBags = new ConcurrentDictionary<string, byte>();

        //CharacterUID, BagUID
        private readonly static ConcurrentDictionary<string, string> _linkedBags = new ConcurrentDictionary<string, string>();

        private IModifLogger Logger => _getLogger.Invoke();
        private readonly Func<IModifLogger> _getLogger;

        public BagStateService(string characterUID, InstanceFactory instances, Func<IModifLogger> getLogger)
        {
            (_characterUID, _instances, _getLogger) = (characterUID, instances, getLogger);
        }
        public static void ConfigureNet(StashPackNet stashPackNet)
        {
            stashPackNet.StashPackLinkChanged += StashPackLinkChanged;
        }

        public bool TrySaveState(Bag bag)
        {
            Logger.LogTrace($"{nameof(BagStateService)}::{nameof(TrySaveState)}:[CharacterUID: {CharacterUID}]" +
                $" Called with Bag.ItemID: {bag.ItemID} for bag UID '{bag.UID}'.");

            if (IsBagDisabled(bag.UID))
            {
                Logger.LogDebug($"{nameof(BagStateService)}::{nameof(TrySaveState)}:[CharacterUID: {CharacterUID}]" +
                    $" Bag '{bag.UID}' is currently disabled and not available to use as a StashPack.");
                return false;
            }
            //if (IsTracked(bag.ItemID))
            //{
            //    Logger.LogDebug($"{nameof(BagStateService)}::{nameof(TrySaveState)}:[CharacterUID: {CharacterUID}]" +
            //        $" Bags with an ItemID of {bag.ItemID} currently have state tracking disabled. State will not be added or updated.");
            //    return false;
            //}

            var bagState = bag.ToBagState(CharacterUID, _instances.AreaStashPackItemIds);

            _bagsStates.AddOrUpdate(bag.ItemID, bagState, (id, value) => bagState);
            return true;
        }

        public bool TryGetState(int itemID, out BagState bagState) => _bagsStates.TryGetValue(itemID, out bagState);
        public IEnumerable<BagState> GetAllBagStates() => _bagsStates.Values;
        public void RemoveState(int bagItemID)
        {
            Logger.LogTrace($"{nameof(BagStateService)}::{nameof(RemoveState)}:[CharacterUID: {CharacterUID}]" +
                $" Called with Bag.ItemID: {bagItemID} for bag UID '{bagItemID}'.");
            _bagsStates.TryRemove(bagItemID, out _);
            RemoveTracking(bagItemID);
        }

        public bool TryGetContentChangesTracked(int itemID, out bool isTracked) => _contentChangesTracked.TryGetValue(itemID, out isTracked);
        public bool IsContentChangesTracked(int itemID) => _contentChangesTracked.GetOrAdd(itemID, true);

        public void EnableContentChangeTracking(int itemID)
        {
            _contentChangesTracked.AddOrUpdate(itemID, true, (k, v) => true);
            Logger.LogDebug($"{nameof(BagStateService)}::{nameof(EnableContentChangeTracking)}:[CharacterUID: {CharacterUID}]" +
                $" Enabled tracking for Bag ItemID: {itemID}.");
        }
        public void DisableContentChangeTracking(int itemID)
        {
            _contentChangesTracked.AddOrUpdate(itemID, false, (k, v) => false);
            Logger.LogDebug($"{nameof(BagStateService)}::{nameof(DisableContentChangeTracking)}:[CharacterUID: {CharacterUID}]" +
                $" Disabled tracking for Bag ItemID: {itemID}.");
        }
        public void RemoveTracking(int itemID) => _contentChangesTracked.TryRemove(itemID, out _);

        private static void StashPackLinkChanged((string bagUID, string characterUID, bool isLinked) args)
        {
            if (args.isLinked)
                LinkBag(args.bagUID, args.characterUID);
            else
                UnlinkBag(args.bagUID);
        }
        public static bool IsLinked(string bagUID) => _linkedBags.ContainsKey(bagUID);
        public static void LinkBag(string bagUID, string characterUID) => _linkedBags.AddOrUpdate(bagUID, characterUID, (k, v) => characterUID);
        public static void UnlinkBag(string bagUID) => _linkedBags.TryRemove(bagUID, out var _);
        public static void UnlinkCharacterBags(string characterUID)
        {
            var keys = _linkedBags.Where(kvp => kvp.Value == characterUID).Select(kvp => kvp.Key).ToList();
            foreach (var k in keys)
                _linkedBags.TryRemove(k, out var _);
        }
        public static IEnumerable<(string BagUID, string CharacterUID)> GetLinkedBags() => _linkedBags.Select(kvp => (kvp.Key, kvp.Value));
        public static void ClearLinkedBags() => _linkedBags.Clear();

        public static void EnableBag(string bagUID) => _disabledBags.TryRemove(bagUID, out var _);
        public static void DisableBag(string bagUID) => _disabledBags.TryAdd(bagUID, new byte());
        public static bool IsBagDisabled(string bagUID) => _disabledBags.ContainsKey(bagUID);
        public static void ClearDisabledBags() => _disabledBags.Clear();
    }
}
