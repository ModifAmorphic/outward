﻿using ModifAmorphic.Outward.StashPacks.SaveData.Extensions;
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
            return syncPlan.SaveDataAfter.SyncData != syncPlan.SaveDataBefore.SyncData  || syncPlan.AddedItems.Any() || syncPlan.ModifiedItems.Any() || syncPlan.RemovedItems.Any();
        }
        public static bool HasDifferentSilverAmount(this ContainerSyncPlan syncPlan)
        {
            UnityEngine.Debug.Log($"HasDifferentSilverAmount:\n" +
                $"  syncPlan.SaveDataBefore {(syncPlan.SaveDataBefore == null ? "is null." : "is not null.")}\n" +
                $"    SyncData: {syncPlan?.SaveDataBefore?.SyncData}\n" +
                $"  syncPlan.SaveDataAfter {(syncPlan.SaveDataAfter == null ? "is null." : "is not null.")}\n" +
                $"    SyncData: {syncPlan?.SaveDataAfter?.SyncData}");
            return syncPlan.SaveDataBefore.GetContainerSilver() != syncPlan.SaveDataAfter.GetContainerSilver();
        }

    }
}
