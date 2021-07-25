using System;
using System.Collections.Generic;
using System.Text;

namespace ModifAmorphic.Outward.StashPacks.WorldInstance.Models
{
    /// <summary>
    /// Container for a StashBag <see cref="Bag"/> instance.
    /// </summary>
    public class StashPack
    {
        /// <summary>
        /// The Home Area of the Stash <see cref="TreasureChest"/> this StashBag is linked to.
        /// </summary>
        public AreaManager.AreaEnum HomeArea { get; internal set; }
        /// <summary>
        /// Wants to sync this instance's <see cref="StashPack.StashBag"/> contents TO the <see cref="StashPack.HomeArea"/> Stash <see cref="TreasureChest"/>.
        /// </summary>
        public bool WantsSyncToStash { get; set; }
        /// <summary>
        /// Wants to sync this instance's <see cref="StashPack.StashBag"/> contents FROM the linked <see cref="StashPack.HomeArea"/> Stash <see cref="TreasureChest"/>.
        /// </summary>
        public bool WantsSyncFromStash { get; set; }
        /// <summary>
        /// Reference of the StashPack specific <see cref="Bag"/> instance currently loaded into the World.
        /// </summary>
        public Bag StashBag { get; internal set; }
    }
}
