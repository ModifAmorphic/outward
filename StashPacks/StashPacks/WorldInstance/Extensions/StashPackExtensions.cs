using ModifAmorphic.Outward.StashPacks.SaveData.Models;
using ModifAmorphic.Outward.StashPacks.WorldInstance.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModifAmorphic.Outward.StashPacks.WorldInstance.Extensions
{
    public static class StashPackExtensions
    {
        public static bool TryGetStashPackSave(this StashPack stashPack, IReadOnlyDictionary<int, AreaManager.AreaEnum> stashPackItemIds, out StashPackSave stashPackSave)
        {
            stashPackSave = null;
            if (stashPackItemIds.TryGetValue(stashPack.StashBag.ItemID, out var area))
            {
                stashPackSave = new StashPackSave()
                {
                    Area = stashPackItemIds[stashPack.StashBag.ItemID],
                    BasicSaveData = new BasicSaveData(stashPack.StashBag.UID, stashPack.StashBag.ToSaveData()),
                    ItemID = stashPack.StashBag.ItemID,
                    UID = stashPack.StashBag.UID,
                    CharacterUID = stashPack.StashBag.PreviousOwnerUID,
                    ItemsSaveData = stashPack.StashBag.Container.GetContainedItems().Select(i => new BasicSaveData(i.UID, i.ToSaveData()))
                };
            }

            return stashPackSave != null;
        }
    }
}
