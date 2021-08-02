using ModifAmorphic.Outward.StashPacks.SaveData.Models;
using System.Collections.Generic;

namespace ModifAmorphic.Outward.StashPacks.State
{
    public class BagState : IContainerSaveData
    {
        public ContainerTypes ContainerType => ContainerTypes.StashPack;
        /// <summary>
        /// The UID of the Area Stash <see cref="Bag"/> Item.
        /// </summary>
        public string UID { get; internal set; }
        /// <summary>
        /// The ItemID of the Area Stash <see cref="Bag"/> Item.
        /// </summary>
        public int ItemID { get; internal set; }
        /// <summary>
        /// The Area containing a Stash that this instance is linked to.
        /// </summary>
        public AreaManager.AreaEnum Area { get; internal set; }
        /// <summary>
        /// The UID of the last character to have this bag equipped or in their inventory.
        /// </summary>
        public string CharacterUID { get; internal set; }
        /// <summary>
        /// The <see cref="BasicSaveData"/> of the Stash <see cref="Bag"/>.
        /// </summary>
        public BasicSaveData BasicSaveData { get; internal set; }
        /// <summary>
        /// Collection of each <see cref="Item"/> <see cref="BasicSaveData"/> contained in the Stash <see cref="Bag"/>'s <see cref="ItemContainer"/>.
        /// </summary>
        public IEnumerable<BasicSaveData> ItemsSaveData { get; internal set; }
    }
}
