using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ModifAmorphic.Outward.Transmorph.Settings
{
    internal static class TransmorphConstants
    {
        public static Recipe.CraftingType FashionRecipeType = (Recipe.CraftingType)(5);

        public static ReadOnlyDictionary<AreaManager.AreaEnum, (string StashUID, int ItemId)> PermenantStashUids = new ReadOnlyDictionary<AreaManager.AreaEnum, (string StashUID, int ItemId)>(
            new Dictionary<AreaManager.AreaEnum, (string StashUID, int ItemId)>()
            {
                { AreaManager.AreaEnum.CierzoVillage, ("ImqRiGAT80aE2WtUHfdcMw", 1000000) },
                { AreaManager.AreaEnum.Monsoon, ("ImqRiGAT80aE2WtUHfdcMw", 1000000) },
                { AreaManager.AreaEnum.Berg, ("ImqRiGAT80aE2WtUHfdcMw", 1000000) },
                { AreaManager.AreaEnum.Levant, ("ZbPXNsPvlUeQVJRks3zBzg", 1000000) },
                { AreaManager.AreaEnum.Harmattan, ("ImqRiGAT80aE2WtUHfdcMw", 1000000) },
                { AreaManager.AreaEnum.NewSirocco, ("IqUugGqBBkaOcQdRmhnMng", 1000000) }
            });

        /// <summary>
        /// Key Value Collection of Stash Backpacs for area's with stashes. Key = ItemId.
        /// </summary>
        public static ReadOnlyDictionary<int, AreaManager.AreaEnum> StashBackpackAreas = new ReadOnlyDictionary<int, AreaManager.AreaEnum>(
            new Dictionary<int, AreaManager.AreaEnum>()
            {
                { -1301000, AreaManager.AreaEnum.CierzoVillage },
                { -1301002, AreaManager.AreaEnum.Monsoon },
                { -1301004, AreaManager.AreaEnum.Berg },
                { -1301006, AreaManager.AreaEnum.Levant },
                { -1301008, AreaManager.AreaEnum.Harmattan },
                { -1301010, AreaManager.AreaEnum.NewSirocco }
            });

    }
}
