using ModifAmorphic.Outward.StashPacks.SaveData.Models;
using ModifAmorphic.Outward.StashPacks.Sync.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModifAmorphic.Outward.StashPacks.SaveData.Extensions
{
    public static class IContainerSaveDataExtensions
    {
        public static ContainerSyncPlan ToContainerSyncPlan(this IContainerSaveData containerSaveData, int? silver = null)
        {
            return new ContainerSyncPlan()
            {
                CharacterUID = containerSaveData.CharacterUID,
                Area = containerSaveData.Area,
                ContainerType = containerSaveData.ContainerType,
                ItemID = containerSaveData.ItemID,
                SaveDataBefore = containerSaveData.BasicSaveData.ToClone(),
                SaveDataAfter = silver != null ? 
                    containerSaveData.BasicSaveData.ToUpdatedContainerSilver((int)silver) :
                    containerSaveData.BasicSaveData.ToClone(),
                ItemsSaveDataBefore = containerSaveData.ItemsSaveData.ToDictionary(s => s.Identifier.ToString(), s => s),
                ItemsSaveDataAfter = containerSaveData.ItemsSaveData.ToDictionary(s => s.Identifier.ToString(), s => s)
            };
        }
    }
}
