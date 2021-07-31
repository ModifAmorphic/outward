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
        /// Reference of the StashPack specific <see cref="Bag"/> instance currently loaded into the World.
        /// </summary>
        public Bag StashBag { get; internal set; }
    }
}
