using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.StashPacks.State;
using System;
using System.Collections.Generic;

namespace ModifAmorphic.Outward.StashPacks.WorldInstance.MajorActions
{
    internal class ActionInstanceManager
    {
        protected readonly InstanceFactory _instances;
        private IModifLogger Logger => _getLogger.Invoke();
        private readonly Func<IModifLogger> _getLogger;
        public ActionInstanceManager(InstanceFactory instances, Func<IModifLogger> getLogger)
        {
            (_instances, _getLogger) = (instances, getLogger);
        }

        public Dictionary<Type, MajorBagActions> _majorActions = new Dictionary<Type, MajorBagActions>();

        public void StartActions()
        {
            _majorActions.Add(typeof(LevelLoadingActions), new LevelLoadingActions(_instances, _getLogger));
            _majorActions.Add(typeof(BagDropActions), new BagDropActions(_instances, _getLogger));
            _majorActions.Add(typeof(BagPickedActions), new BagPickedActions(_instances, _getLogger));
            _majorActions.Add(typeof(ContentsChangedActions), new ContentsChangedActions(_instances, _getLogger));
            _majorActions.Add(typeof(PlayerActions), new PlayerActions(_instances, _getLogger));
            _majorActions.Add(typeof(CharacterInventoryActions), new CharacterInventoryActions(_instances, _getLogger));
            _majorActions.Add(typeof(BagDisplayActions), new BagDisplayActions(_instances, _getLogger));

            _majorActions[typeof(LevelLoadingActions)].SubscribeToEvents();
            _majorActions[typeof(BagDropActions)].SubscribeToEvents();
            _majorActions[typeof(BagPickedActions)].SubscribeToEvents();
            _majorActions[typeof(ContentsChangedActions)].SubscribeToEvents();
            _majorActions[typeof(PlayerActions)].SubscribeToEvents();
            _majorActions[typeof(CharacterInventoryActions)].SubscribeToEvents();
            _majorActions[typeof(BagDisplayActions)].SubscribeToEvents();

            Logger.LogDebug($"{nameof(ActionInstanceManager)}::{nameof(StartActions)}: MajorAction instances created and subscribed to events.");
        }

    }
}
