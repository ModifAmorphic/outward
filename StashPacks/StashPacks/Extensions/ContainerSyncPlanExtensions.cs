using ModifAmorphic.Outward.Extensions;
using ModifAmorphic.Outward.StashPacks.Sync.Models;
using System.Linq;

namespace ModifAmorphic.Outward.StashPacks.Extensions
{
    public static class ContainerSyncPlanExtensions
    {
        public static bool HasChanges(this ContainerSyncPlan syncPlan)
        {
            return syncPlan.SaveDataAfter.SyncData != syncPlan.SaveDataBefore.SyncData || syncPlan.AddedItems.Any() || syncPlan.ModifiedItems.Any() || syncPlan.RemovedItems.Any();
        }
        public static bool HasDifferentSilverAmount(this ContainerSyncPlan syncPlan)
        {
            return syncPlan.SaveDataBefore.GetContainerSilver() != syncPlan.SaveDataAfter.GetContainerSilver();
        }

    }
}
