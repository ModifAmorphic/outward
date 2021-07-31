using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ModifAmorphic.Outward.StashPacks.Settings
{
    internal static class StashPacksConstants
    {
        public static ReadOnlyDictionary<AreaManager.AreaEnum, (string StashUID, int ItemId)> PermenantStashUids = new ReadOnlyDictionary<AreaManager.AreaEnum, (string StashUID, int ItemId)>(
            new Dictionary<AreaManager.AreaEnum, (string StashUID, int ItemId)>()
            {
                { AreaManager.AreaEnum.CierzoVillage, ("ImqRiGAT80aE2WtUHfdcMw", 1000000) },
                { AreaManager.AreaEnum.Berg, ("ImqRiGAT80aE2WtUHfdcMw", 1000000) },
                { AreaManager.AreaEnum.Levant, ("ZbPXNsPvlUeQVJRks3zBzg", 1000000) },
            });
        /// <summary>
        /// Key Value Collection of Stash Backpacs for area's with stashes. Key = ItemId.
        /// </summary>
        public static ReadOnlyDictionary<int, AreaManager.AreaEnum> StashBackpackItemIds = new ReadOnlyDictionary<int, AreaManager.AreaEnum>(
            new Dictionary<int, AreaManager.AreaEnum>()
            {
                { -1301000, AreaManager.AreaEnum.CierzoVillage },
                { -1301001, AreaManager.AreaEnum.HallowedMarsh },
                { -1301002, AreaManager.AreaEnum.Berg },
                { -1301003, AreaManager.AreaEnum.Levant },
                { -1301004, AreaManager.AreaEnum.Harmattan },
                { -1301005, AreaManager.AreaEnum.Caldera }
            });

        public const string BagUidSuffix = "_Content";
    }
}
