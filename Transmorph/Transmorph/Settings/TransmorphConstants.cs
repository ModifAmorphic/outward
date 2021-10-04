using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ModifAmorphic.Outward.Transmorph.Settings
{
    internal static class TransmorphConstants
    {
        public const int TransmogRecipeStartID = -1303000000;
        public const int TransmogPrefix = -1356830026;
        public const int TransmogSecondaryItemID = 6300030;
        
        public static byte[] TransmorgBytePrefix = BitConverter.GetBytes(TransmogPrefix);

        static TransmorphConstants()
        {
        }
        public static TagSourceSelector TransmogTagSelector = new TagSourceSelector(
            new Tag("Axfc-kYcGEOguAqCUHh_fg", "transmog"));

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
