using ModifAmorphic.Outward.StashPacks.Sync.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace ModifAmorphic.Outward.StashPacks.SaveData.Models
{
    /// <summary>
    /// Container for an Area's (Environment / Scene) Stash specific save data, including Item save data.
    /// </summary>
    public class StashSave : IContainerSaveData
    {
        public ContainerTypes ContainerType => ContainerTypes.Stash;
        /// <summary>
        /// The unique ID of the Stash <see cref="TreasureChest"/>
        /// </summary>
        public string UID { get; internal set; }
        /// <summary>
        /// The ItemID of the Area Stash <see cref="TreasureChest"/> Item.
        /// </summary>
        public int ItemID { get; internal set; }
        /// <summary>
        /// Area of the Stash <see cref="TreasureChest"/>
        /// </summary>
        public AreaManager.AreaEnum Area { get; internal set; }
        public string SceneName { get; internal set; }
        public string SaveFilePath { get; internal set; }
        /// <summary>
        /// The <see cref="global::BasicSaveData"/> of the Stash <see cref="TreasureChest"/>.
        /// </summary>
        public BasicSaveData BasicSaveData { get; internal set; }
        /// <summary>
        /// Collection of each <see cref="Item"/> <see cref="global::BasicSaveData"/> contained in the Stash <see cref="TreasureChest"/>.
        /// </summary>
        public IEnumerable<BasicSaveData> ItemsSaveData { get; internal set; }

    }
}
