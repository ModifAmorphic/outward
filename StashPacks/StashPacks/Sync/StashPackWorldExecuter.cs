using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.StashPacks.Sync.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModifAmorphic.Outward.StashPacks.Sync
{
    internal class StashPackWorldExecuter
    {
        private readonly object _syncLock = new object();
        private readonly ItemManager _itemManager;

        private IModifLogger Logger => _getLogger.Invoke();
        private readonly Func<IModifLogger> _getLogger;

        public StashPackWorldExecuter(ItemManager itemManager, Func<IModifLogger> getLogger) => (_itemManager, _getLogger) = (itemManager, getLogger);

        public void ExecutePlan(ContainerSyncPlan syncPlan)
        {
            lock (_syncLock)
            {
                foreach (var uid in syncPlan.RemovedItems.Keys)
                    _itemManager.DestroyItem(uid);

                var upserts = syncPlan.AddedItems.Values.ToList();
                upserts.AddRange(syncPlan.ModifiedItems.Values);
                _itemManager.LoadItems(upserts, false);
            }
        }
    }
}
