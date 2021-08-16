using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.StashPacks.State;
using System;
using System.Collections.Generic;
using System.Text;

namespace ModifAmorphic.Outward.StashPacks.WorldInstance.MajorActions
{
    internal class ActionInstanceManager
    {
        protected readonly InstanceFactory _instances;
        private IModifLogger Logger => _getLogger.Invoke();
        private readonly Func<IModifLogger> _getLogger;
        public ActionInstanceManager(InstanceFactory instances, Func<IModifLogger> getLogger) => (_instances, _getLogger) = (instances, getLogger);

        public Dictionary<Type, MajorBagActions> _majorActions = new Dictionary<Type, MajorBagActions>();

        public void StartActions()
        {
            _majorActions.Add(typeof(LevelLoadingActions), new LevelLoadingActions(_instances, _getLogger));
            _majorActions.Add(typeof(BagDropActions), new BagDropActions(_instances, _getLogger));
            _majorActions.Add(typeof(BagPickedActions), new BagPickedActions(_instances, _getLogger));
            _majorActions.Add(typeof(ContentsChangedActions), new ContentsChangedActions(_instances, _getLogger));
            _majorActions.Add(typeof(PlayerSaveActions), new PlayerSaveActions(_instances, _getLogger));
            _majorActions.Add(typeof(CharacterInventoryActions), new CharacterInventoryActions(_instances, _getLogger));

            foreach (var a in _majorActions.Values)
                a.SubscribeToEvents();
        }

    }
}
