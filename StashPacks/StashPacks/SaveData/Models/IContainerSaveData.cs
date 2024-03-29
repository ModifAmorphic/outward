﻿using System.Collections.Generic;

namespace ModifAmorphic.Outward.StashPacks.SaveData.Models
{
    public interface IContainerSaveData
    {
        string CharacterUID { get; }
        ContainerTypes ContainerType { get; }
        string UID { get; }
        int ItemID { get; }
        AreaManager.AreaEnum Area { get; }
        BasicSaveData BasicSaveData { get; }
        IEnumerable<BasicSaveData> ItemsSaveData { get; }
    }
}