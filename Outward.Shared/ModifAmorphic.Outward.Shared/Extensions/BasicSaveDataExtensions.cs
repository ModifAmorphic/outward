using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace ModifAmorphic.Outward.Extensions
{
    public static class BasicSaveDataExtensions
    {
        /// <summary>
        /// Gets the first item in basicSave data with a UID of the passed <paramref name="UID"/>.
        /// </summary>
        /// <param name="basicSaveData">The IEnumerable of BasicSaveData potentially containing the item with the matched <paramref name="UID"/>.</param>
        /// <param name="UID">The UID to search for in this <paramref name="basicSaveData"/> collection.</param>
        /// <returns>A BasicSaveData reference to the matched <paramref name="UID"/> if found, otherwise the default of the BasicSaveData type.</returns>
        public static BasicSaveData GetItemSaveData(this IEnumerable<BasicSaveData> basicSaveData, string UID) =>
            basicSaveData.FirstOrDefault(i => i.Identifier.ToString() == UID);

        /// <summary>
        /// Finds and returns a IEnumerable of references to instances within this collection who's Hierarchy element contains the parent container's UID and ItemId.
        /// </summary>
        /// <param name="basicSaveData">The collection of BasicSaveData to search for matching SyncData Hierachy.</param>
        /// <param name="containerUID">The parent Hierachy UID of items to find.</param>
        /// <param name="containerItemId">The parent Hierachy ItemId of items to find.</param>
        /// <returns>A collection of BasicSaveData references from this IEnumerable BasicSaveData collection.</returns>
        public static IEnumerable<BasicSaveData> GetContainerItems(this IEnumerable<BasicSaveData> basicSaveData, string containerUID, int containerItemId) =>
            basicSaveData.Where(i =>
                                i.SyncData.Contains($"<Hierarchy>1{containerUID};{containerItemId}</Hierarchy>"));


        /// <summary>
        /// Retrieves the ItemID from the this instances SyncData.
        /// </summary>
        /// <param name="basicSaveData">The data in which to extract the ItemID from.</param>
        /// <param name="itemID">Set to the ItemID if one is found and can be parsed as an int, otherwise an 0.</param>
        /// <returns>true if a valid ItemID is found, otherwise false.</returns>
        public static bool TryGetItemID(this BasicSaveData basicSaveData, out int itemID)
        {
            var ownerIdMatch = Regex.Match(basicSaveData.SyncData, @"<ID>(-?\d+)</ID>");

            if (ownerIdMatch.Success && ownerIdMatch.Groups.Count > 1
                && int.TryParse(ownerIdMatch.Groups[1].Value, out itemID))
                return true;
            itemID = 0;
            return false;
        }

        /// <summary>
        /// Tries to retrieve the UID of the PreviousOwner from the this instances SyncData.
        /// </summary>
        /// <param name="basicSaveData">The data in which to extract the UID of the Previous Owner from.</param>
        /// <param name="previousOwnerUID">Set to the PreviousOwnerUID if successful, otherwise an empty string.</param>
        /// <returns>true if a valid PreviousOwnerUID is found, otherwise false.</returns>
        public static bool TryGetPreviousOwnerUID(this BasicSaveData basicSaveData, out string previousOwnerUID)
        {
            var ownerIdMatch = Regex.Match(basicSaveData.SyncData, @"<PreviousOwnerUID>(.+?)</PreviousOwnerUID>");

            if (ownerIdMatch.Success
                && ownerIdMatch.Groups.Count > 1
                && !string.IsNullOrWhiteSpace(ownerIdMatch.Groups[1].Value)
                && ownerIdMatch.Groups[1].Value.Trim() != "-")
            {
                previousOwnerUID = ownerIdMatch.Groups[1].Value;
                return true;
            }
            previousOwnerUID = string.Empty;
            return false;
        }

        /// <summary>
        /// Gets the value of the Contained amount from this BasicSaveData SyncData.
        /// </summary>
        /// <param name="saveData">The BasicSaveData instance who's SyncData potentially contains a TreasureChestContainedSilver or BagSilver property.</param>
        /// <returns>The amount of silver found in the TreasureChestContainedSilver or BagSilver properties, otherwise returns 0.</returns>
        public static int GetContainerSilver(this BasicSaveData saveData)
        {
            const string tchestRegex = @"(?<=TreasureChestContainedSilver/)[\d+]*(?=;)";
            const string bagRegex = @"(?<=BagSilver/)[\d+]*(?=;)";

            var silverMatch = Match.Empty;
            if (saveData.SyncData.Contains("TreasureChestContainedSilver"))
                silverMatch = Regex.Match(saveData.SyncData, tchestRegex);
            else if (saveData.SyncData.Contains("BagSilver"))
                silverMatch = Regex.Match(saveData.SyncData, bagRegex);

            if (silverMatch.Success && int.TryParse(silverMatch.Value, out var silver))
            {
                return silver;
            }
            return 0;
        }

        public static BasicSaveData ToClone(this BasicSaveData saveData)
        {
            return new BasicSaveData(saveData.Identifier.ToString(), saveData.SyncData);
        }
        /// <summary>
        /// Sets the TreasureChestContainedSilver or BagSilver amount in a new copy of this BasicSavaData item.
        /// </summary>
        /// <param name="saveData">The BasicSaveData instance who's SyncData potentially contains a TreasureChestContainedSilver or BagSilver property.</param>
        /// <returns>A new BasicSaveData if a match to a TreasureChestContainedSilver property was found.</returns>
        public static BasicSaveData ToUpdatedContainerSilver(this BasicSaveData saveData, int silver)
        {
            var updatedSilverSyncData = saveData.SyncData;
            if (saveData.SyncData.Contains("TreasureChestContainedSilver"))
                updatedSilverSyncData = Regex.Replace(saveData.SyncData, @"(?<=TreasureChestContainedSilver/)[\d+]*(?=;)", silver.ToString());
            else if (saveData.SyncData.Contains("BagSilver"))
                updatedSilverSyncData = Regex.Replace(saveData.SyncData, @"(?<=BagSilver/)[\d+]*(?=;)", silver.ToString());

            return new BasicSaveData(saveData.Identifier, updatedSilverSyncData);
        }

        /// <summary>
        /// Searches for the Parent Container's UID and Item Id in the Hierarcy element of this instance's <see cref="BasicSaveData.SyncData"/> XML.
        /// </summary>
        /// <param name="basicSaveData"></param>
        /// <returns>A <see cref="Tuple{T1, T2, T3}"/> <list type="number">
        /// <item>Hierarchy xml element the match was performed on.</item><item>The Parent Container UID of this <see cref="BasicSaveData"/>.</item><item>The Parent Container's ItemID.</item></list></returns>
        public static (string HierachyXml, string ParentUID, int ParentItemID) GetHierarchyData(this BasicSaveData basicSaveData)
        {
            var hierarchyMatch = Regex.Match(basicSaveData.SyncData, @"<Hierarchy>1(.+?);(\d+)</Hierarchy>");
            if (!hierarchyMatch.Success || hierarchyMatch.Groups.Count < 2)
                return default;

            //string parentUid = hierarchyMatch.Groups[1].Value.Replace("_Content", "");
            int.TryParse(hierarchyMatch.Groups[2].Value, out var itemId);
            return (hierarchyMatch.Groups[0].Value, hierarchyMatch.Groups[1].Value, itemId);
        }

        /// <summary>
        /// Returns a new <see cref="BasicSaveData"/> instance where the Hierachy element is updated with the <paramref name="parentUID"/> and <paramref name="parentUID"/> parameters.
        /// </summary>
        /// <param name="saveData"></param>
        /// <param name="parentUID">The parent container's UID. If the Parent UID is that of a <see cref="Bag"/>, then <paramref name="parentUID"/> should end with "_Content".</param>
        /// <param name="parentItemId">The parent container's ItemId.</param>
        /// <returns></returns>
        public static BasicSaveData ToUpdatedHierachy(this BasicSaveData saveData, string parentUID, int parentItemId) =>
            new BasicSaveData(saveData.Identifier,
                saveData.SyncData.Replace(
                    saveData.GetHierarchyData().HierachyXml, $"<Hierarchy>1{parentUID};{parentItemId}</Hierarchy>")
                );
    }
}
