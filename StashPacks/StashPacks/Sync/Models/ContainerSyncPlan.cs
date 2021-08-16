using ModifAmorphic.Outward.StashPacks.SaveData.Models;
using System.Collections.Generic;

namespace ModifAmorphic.Outward.StashPacks.Sync.Models
{
    public class ContainerSyncPlan
    {
        public string UID { get; internal set; }
        public string CharacterUID { get; internal set; }
        public ContainerTypes ContainerType { get; internal set; }
        public int ItemID { get; internal set; }
        public AreaManager.AreaEnum Area { get; internal set; }
        public BasicSaveData SaveDataBefore { get; internal set; }
        public BasicSaveData SaveDataAfter { get; internal set; }

        public Dictionary<string, BasicSaveData> AddedItems { get; internal set; }
        public Dictionary<string, BasicSaveData> RemovedItems { get; internal set; }
        public Dictionary<string, BasicSaveData> ModifiedItems { get; internal set; }

        public Dictionary<string, BasicSaveData> ItemsSaveDataBefore { get; internal set; }
        public Dictionary<string, BasicSaveData> ItemsSaveDataAfter { get; internal set; }

    }
}
