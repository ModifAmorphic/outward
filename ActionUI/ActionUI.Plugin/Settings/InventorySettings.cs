using System.Collections.Generic;

namespace ModifAmorphic.Outward.ActionUI.Settings
{
    internal class InventorySettings
    {
        public const int StartingSetItemID = -1310000000;

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
