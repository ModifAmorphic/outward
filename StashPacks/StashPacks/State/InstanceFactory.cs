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
using System.Text;

namespace ModifAmorphic.Outward.StashPacks.State
{
    internal class InstanceFactory
    {
        private AreaManager _areaManager;
        public AreaManager AreaManager => _areaManager;

        private readonly BaseUnityPlugin _unityPlugin;
        public BaseUnityPlugin UnityPlugin => _unityPlugin;

        private ItemManager _itemManager;
        private readonly ConcurrentDictionary<string, CharacterSaveInstanceHolder> _characterSaveInstanceHolders = new ConcurrentDictionary<string, CharacterSaveInstanceHolder>();
        private IReadOnlyDictionary<AreaManager.AreaEnum, (string StashUID, int ItemId)> _stashIds = StashPacksConstants.PermenantStashUids;
        private IReadOnlyDictionary<int, AreaManager.AreaEnum> _stashPackItemIds = StashPacksConstants.StashBackpackItemIds;
        private readonly ConcurrentDictionary<Type, object> _singletons = new ConcurrentDictionary<Type, object>();

        private IModifLogger Logger => _getLogger.Invoke();
        private readonly Func<IModifLogger> _getLogger;

        public InstanceFactory(BaseUnityPlugin unityPlugin, Func<IModifLogger> getLogger)
        {
            _unityPlugin = unityPlugin;
            _getLogger = getLogger;
            RoutePatchEvents();
        }

        public bool TryGetStashSaveData(string characterUID, out StashSaveData stashSaveData)
        {
            stashSaveData = null;
            if (_characterSaveInstanceHolders.TryGetValue(characterUID, out var characterSaveInstanceHolder))
                stashSaveData = new StashSaveData(_areaManager, characterSaveInstanceHolder, _stashIds, _getLogger);
            
            return stashSaveData != null;
        }
        public bool TryGetStashPackWorldData(out StashPackWorldData stashPackWorldData)
        {
            stashPackWorldData = null;
            if (_itemManager != null)
                stashPackWorldData = new StashPackWorldData(_itemManager, _unityPlugin, _stashPackItemIds, _getLogger);
            return stashPackWorldData != null;
        }
        public SyncPlanner GetSyncPlanner()
        {
            return (SyncPlanner)_singletons.GetOrAdd(typeof(SyncPlanner), new SyncPlanner(_getLogger));
        }
        public StashPackSaveExecuter GetStashPackSaveExecuter()
        {
            return (StashPackSaveExecuter)_singletons.GetOrAdd(typeof(StashPackSaveExecuter), new StashPackSaveExecuter(_getLogger));
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

        #region Patch Event Routing
        private void RoutePatchEvents()
        {
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
    }
}
