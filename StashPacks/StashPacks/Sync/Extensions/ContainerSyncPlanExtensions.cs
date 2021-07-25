using ModifAmorphic.Outward.StashPacks.Sync.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModifAmorphic.Outward.StashPacks.Sync.Extensions
{
    public static class ContainerSyncPlanExtensions
    {
        public static bool HasChanges(this ContainerSyncPlan syncPlan)
        {
            return syncPlan.SaveDataAfter != null || syncPlan.AddedItems.Any() || syncPlan.ModifiedItems.Any() || syncPlan.RemovedItems.Any();
        }
    }
}
