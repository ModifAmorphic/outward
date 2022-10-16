using System.Collections.Generic;

namespace ModifAmorphic.Outward.ActionUI.Settings
{
    internal class InventorySettings
    {
        public const int MaxSetItemID = -1310000000;
        public const int MinSetItemID = -1319999999;
        public const int MaxItemID = -1320000000;
        public const int MinItemID = -1329999999;
        public static readonly HashSet<AreaManager.AreaEnum> StashAreas = new HashSet<AreaManager.AreaEnum>()
        {
            AreaManager.AreaEnum.CierzoVillage,
            AreaManager.AreaEnum.Monsoon,
            AreaManager.AreaEnum.Berg,
            AreaManager.AreaEnum.Levant,
            AreaManager.AreaEnum.Harmattan,
            AreaManager.AreaEnum.NewSirocco,
        };
    }
}
