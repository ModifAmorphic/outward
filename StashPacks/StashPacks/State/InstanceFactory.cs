using BepInEx;
using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.StashPacks.Patch.Events;
using ModifAmorphic.Outward.StashPacks.SaveData.Data;
using ModifAmorphic.Outward.StashPacks.Settings;
using ModifAmorphic.Outward.StashPacks.Sync;
using ModifAmorphic.Outward.StashPacks.WorldInstance.Data;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace ModifAmorphic.Outward.StashPacks.State
{
    internal class InstanceFactory
    {
        private AreaManager _areaManager;
        public AreaManager AreaManager => _areaManager;

        private readonly BaseUnityPlugin _unityPlugin;
        public BaseUnityPlugin UnityPlugin => _unityPlugin;

        private readonly StashPacksSettings _stashPacksSettings;
        public StashPacksSettings StashPacksSettings => _stashPacksSettings;

        private ItemManager _itemManager;

        /// <summary>
        /// CharacterUID, CharacterSaveInstanceHolder
        /// </summary>
        private readonly ConcurrentDictionary<string, CharacterSaveInstanceHolder> _characterSaveInstanceHolders = new ConcurrentDictionary<string, CharacterSaveInstanceHolder>();

        public IReadOnlyDictionary<AreaManager.AreaEnum, (string StashUID, int ItemId)> StashIds => StashPacksConstants.PermenantStashUids;

        /// <summary>
        /// Bag ItemId, Area
        /// </summary>
        public IReadOnlyDictionary<int, AreaManager.AreaEnum> AreaStashPackItemIds => StashPacksConstants.StashBackpackAreas;

        private readonly ConcurrentDictionary<Type, object> _singletons = new ConcurrentDictionary<Type, object>();

        private readonly ConcurrentDictionary<string, ConcurrentDictionary<Type, object>> _characterSingletons = new ConcurrentDictionary<string, ConcurrentDictionary<Type, object>>();

        /// <summary>
        /// Actions that will be executed when a SaveInstance.Save prefix event is triggered. Indexed by CharacterUID.
        /// </summary>
        //private readonly ConcurrentDictionary<string, EventStashSaveExecuter> _stashSaveExecuters = new ConcurrentDictionary<string, EventStashSaveExecuter>();

        private IModifLogger Logger => _getLogger.Invoke();
        private readonly Func<IModifLogger> _getLogger;

        public InstanceFactory(BaseUnityPlugin unityPlugin, StashPacksSettings stashPackSettings, Func<IModifLogger> getLogger)
        {
            _unityPlugin = unityPlugin;
            _getLogger = getLogger;
            _stashPacksSettings = stashPackSettings;
            RoutePatchEvents();
        }

        #region Patch Event Routing
        private void RoutePatchEvents()
        {
            //Remove a character's stash saveExecuter once a save is complete so a new one is created next time.
            //SaveInstanceEvents.SaveAfter += (saveInstance => _stashSaveExecuters.TryRemove(saveInstance.CharSave.CharacterUID, out _));
            SceneManager.sceneLoaded += (s, l) =>
            {
                Logger.LogDebug("New scene loaded. Disposing of scene singletons.");
                foreach (var instance in _singletons.Values)
                    if (instance is IDisposable disposable)
                        disposable.Dispose();
                _singletons.Clear();

                foreach (var charInstances in _characterSingletons.Values)
                    foreach (var instance in charInstances.Values)
                        if (instance is IDisposable disposable)
                            disposable.Dispose();

                _characterSingletons.Clear();

                BagStateService.ClearDisabledBags();

            };
            ItemManagerEvents.AwakeAfter += (itemManager => _itemManager = itemManager);
            AreaManagerEvents.AwakeAfter += (areaManager => _areaManager = areaManager);
            CharacterSaveInstanceHolderEvents.PlayerSaveLoadedAfter += ((charSaveInstanceHolder, character) =>
                _characterSaveInstanceHolders.AddOrUpdate(
                    character.UID.ToString(),
                    charSaveInstanceHolder,
                    (uid, ch) => ch)
            );
        }
        #endregion
        public bool TryGetItemManager(out ItemManager itemManager)
        {
            itemManager = _itemManager;
            return itemManager != null;
        }
        public bool TryGetStashSaveData(string characterUID, out StashSaveData stashSaveData)
        {
            stashSaveData = null;
            if (_characterSaveInstanceHolders.TryGetValue(characterUID, out var characterSaveInstanceHolder))
                stashSaveData = new StashSaveData(_areaManager, characterSaveInstanceHolder, StashIds, _getLogger);

            return stashSaveData != null;
        }
        public bool TryGetStashPackWorldData(out StashPackWorldData stashPackWorldData)
        {
            stashPackWorldData = null;
            if (_itemManager != null)
                stashPackWorldData = new StashPackWorldData(_itemManager, _unityPlugin, AreaStashPackItemIds, _getLogger);
            return stashPackWorldData != null;
        }
        public SyncPlanner GetSyncPlanner()
        {
            if (!_singletons.TryGetValue(typeof(SyncPlanner), out _))
            {
                _singletons.TryAdd(typeof(SyncPlanner), new SyncPlanner(_getLogger));
            }

            return (SyncPlanner)_singletons[typeof(SyncPlanner)];
        }

        public BagStateService GetBagStateService(string characterUID)
        {
            var singletons = _characterSingletons.GetOrAdd(characterUID, new ConcurrentDictionary<Type, object>());

            _ = singletons.GetOrAdd(typeof(BagStateService), new BagStateService(characterUID, this, _getLogger));

            return (BagStateService)_characterSingletons[characterUID][typeof(BagStateService)];
        }
        public bool TryGetStashPackWorldExecuter(out StashPackWorldExecuter planExecuter)
        {
            planExecuter = null;
            if (_itemManager != null)
            {
                planExecuter = new StashPackWorldExecuter(_itemManager, _getLogger);
            }

            return planExecuter != null;
        }
        public StashSaveExecuter GetStashSaveExecuter(string characterUID, StashSaveData stashSaveData)
        {
            var characterInstances = _characterSingletons.GetOrAdd(characterUID, new ConcurrentDictionary<Type, object>());

            if (!characterInstances.ContainsKey(typeof(StashSaveExecuter)))
                characterInstances.TryAdd(typeof(StashSaveExecuter), new StashSaveExecuter(stashSaveData, _getLogger));


            return (StashSaveExecuter)_characterSingletons[characterUID][typeof(StashSaveExecuter)];
        }
        //public StashPackSaveExecuter GetStashPackSaveExecuter()
        //{
        //    if (!_singletons.TryGetValue(typeof(StashPackSaveExecuter), out _))
        //    {
        //        _singletons.TryAdd(typeof(StashPackSaveExecuter), new StashPackSaveExecuter(_getLogger));
        //    }

        //    return (StashPackSaveExecuter)_singletons[typeof(StashPackSaveExecuter)];
        //}
    }
}
