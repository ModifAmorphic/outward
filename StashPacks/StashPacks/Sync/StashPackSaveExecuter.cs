using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.StashPacks.SaveData.Extensions;
using ModifAmorphic.Outward.StashPacks.Settings;
using ModifAmorphic.Outward.StashPacks.Sync.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModifAmorphic.Outward.StashPacks.Sync
{
    internal class StashPackSaveExecuter
    {
        private readonly object _syncLock = new object();
        private readonly ItemManager _itemManager;

        private IModifLogger Logger => _getLogger.Invoke();
        private readonly Func<IModifLogger> _getLogger;

        public StashPackSaveExecuter(Func<IModifLogger> getLogger) => _getLogger = getLogger;

        public void ExecutePlan(ContainerSyncPlan syncPlan, ref List<BasicSaveData> listToAlter, bool clearExistingItems = false)
        {
            lock (_syncLock)
            {
                if (clearExistingItems)
                    listToAlter.RemoveAll(item => {
                        var hierData = item.GetHierarchyData();
                        if (hierData != default)
                            return hierData.ParentUID.Replace(StashPacksConstants.BagUidSuffix, "") == syncPlan.SaveDataBefore.Identifier.ToString();
                        return false;
                    });
                else
                {
                    listToAlter.RemoveAll(item => syncPlan.RemovedItems?.ContainsKey(item.Identifier.ToString())??false);
                    listToAlter.RemoveAll(item => syncPlan.ModifiedItems?.ContainsKey(item.Identifier.ToString())??false);
                }

                var upserts = syncPlan.AddedItems.Values.ToList();
                upserts.AddRange(syncPlan.ModifiedItems.Values);
                listToAlter.AddRange(upserts);
            }
        }
    }
}
