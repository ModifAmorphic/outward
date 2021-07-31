using ModifAmorphic.Outward.StashPacks.SaveData.Models;
using ModifAmorphic.Outward.StashPacks.Settings;
using ModifAmorphic.Outward.StashPacks.State;
using System.Collections.Generic;
using System.Linq;

namespace ModifAmorphic.Outward.StashPacks.Extensions
{
    public static class BagExtensions
    {
        public static void EmptyContents(this Bag stashBag)
        {
            stashBag.Container.ClearPouch();
            stashBag.Container.RemoveAllSilver();
        }
        public static bool IsStashBag(this Bag bag)
        {
            return StashPacksConstants.StashBackpackItemIds.ContainsKey(bag.ItemID);
        }
        public static bool HasContents(this Bag stashBag)
        {
            return stashBag.ContainedSilver > 0 || stashBag.Container.GetContainedItems().Any();
        }

        /// <summary>
        /// Converts to a new <see cref="BagState" /> instance, setting the <see cref="BagState.CharacterUID"/> to the <paramref name="characterUID"/> parameter.
        /// </summary>
        /// <param name="bag"></param>
        /// <param name="characterUID"></param>
        /// <param name="stashPackItemIds"></param>
        /// <returns></returns>
        public static BagState ToBagState(this Bag bag, string characterUID, IReadOnlyDictionary<int, AreaManager.AreaEnum> stashPackItemIds)
        {
            return new BagState()
            {
                UID = bag.UID,
                Area = stashPackItemIds[bag.ItemID],
                BasicSaveData = new BasicSaveData(bag.UID, bag.ToSaveData()),
                CharacterUID = characterUID,
                ItemID = bag.ItemID,
                ItemsSaveData = bag.Container.GetContainedItems().Select(i => new BasicSaveData(i.UID, i.ToSaveData())).ToList()
            };
        }
    }
}
