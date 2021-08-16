using ModifAmorphic.Outward.StashPacks.Extensions;
using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.StashPacks.Sync.Models;
using System;
using System.Linq;
using ModifAmorphic.Outward.Extensions;

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
            if (!syncPlan.HasChanges())
            {
                Logger.LogInfo($"Sync Plan for '{syncPlan.Area.GetName()}' {syncPlan.ContainerType.GetName()} has no changes to execute.");
                return;
            }
            lock (_syncLock)
            {
                var bag = (Bag)_itemManager.GetItem(syncPlan.UID);
                foreach (var uid in syncPlan.RemovedItems.Keys)
                {
                    var removeItem = bag.Container.GetItem(uid);
                    if (removeItem != null)
                        bag.Container.RemoveItem(removeItem);
                }

                var upserts = syncPlan.AddedItems.Values.ToList();
                upserts.AddRange(syncPlan.ModifiedItems.Values);

                if (syncPlan.HasDifferentSilverAmount())
                {
                    //bag.Container?.SetSilverCount(syncPlan.SaveDataAfter.GetContainerSilver());
                    bag.Container.RemoveAllSilver();
                    bag.Container.AddSilver(syncPlan.SaveDataAfter.GetContainerSilver());
                }

                _itemManager.LoadItems(upserts, false);
            }
        }
    }
}
