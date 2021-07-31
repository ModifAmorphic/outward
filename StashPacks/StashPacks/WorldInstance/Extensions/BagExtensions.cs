﻿using ModifAmorphic.Outward.StashPacks.SaveData.Models;
using ModifAmorphic.Outward.StashPacks.Settings;
using ModifAmorphic.Outward.StashPacks.State;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModifAmorphic.Outward.StashPacks.WorldInstance.Extensions
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
        /// Converts to a new <see cref="StashPackSave" /> instance. Tries to assign <paramref name="bag"/>.PreviousOwnerUID if set, otherwise <paramref name="bag"/>.OwnerCharacter.UID
        /// </summary>
        /// <param name="bag"></param>
        /// <param name="stashPackItemIds"></param>
        /// <returns></returns>
        public static StashPackSave ToStashPackSave(this Bag bag, IReadOnlyDictionary<int, AreaManager.AreaEnum> stashPackItemIds)
        {
            var characterUID = !string.IsNullOrEmpty(bag.PreviousOwnerUID) ? bag.PreviousOwnerUID :
                bag.OwnerCharacter != null ? bag.OwnerCharacter.UID.ToString() :
                    string.Empty;
            return ToStashPackSave(bag, characterUID, stashPackItemIds);
        }
        /// <summary>
        /// Converts to a new <see cref="StashPackSave" /> instance, setting the <see cref="StashPackSave.CharacterUID"/> to the <paramref name="characterUID"/> parameter.
        /// </summary>
        /// <param name="bag"></param>
        /// <param name="characterUID"></param>
        /// <param name="stashPackItemIds"></param>
        /// <returns></returns>
        public static StashPackSave ToStashPackSave(this Bag bag, string characterUID, IReadOnlyDictionary<int, AreaManager.AreaEnum> stashPackItemIds)
        {
            return new StashPackSave()
            {
                UID = bag.UID,
                Area = stashPackItemIds[bag.ItemID],
                BasicSaveData = new BasicSaveData(bag.UID, bag.ToSaveData()),
                CharacterUID = characterUID,
                ItemID = bag.ItemID,
                ItemsSaveData = bag.Container.GetContainedItems().Select(i => new BasicSaveData(i.UID, i.ToSaveData())).ToList()
            };
        }
        public static StashBag ToStashBag(this Bag bag, IReadOnlyDictionary<int, AreaManager.AreaEnum> stashPackItemIds)
        {
            var characterUID = !string.IsNullOrEmpty(bag.PreviousOwnerUID) ? bag.PreviousOwnerUID :
                bag.OwnerCharacter != null ? bag.OwnerCharacter.UID.ToString() :
                    string.Empty;
            return new StashBag()
            {
                UID = bag.UID,
                Area = stashPackItemIds[bag.ItemID],
                BasicSaveData = new BasicSaveData(bag.UID, bag.ToSaveData()),
                CharacterUID = characterUID,
                ItemID = bag.ItemID,
                ItemsSaveData = bag.Container.GetContainedItems().Select(i => new BasicSaveData(i.UID, i.ToSaveData())).ToList()
            };
        }
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
