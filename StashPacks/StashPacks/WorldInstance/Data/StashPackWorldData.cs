using BepInEx;
using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.StashPacks.Extensions;
using ModifAmorphic.Outward.StashPacks.WorldInstance.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ModifAmorphic.Outward.StashPacks.WorldInstance.Data
{
    internal class StashPackWorldData
    {
        private readonly ItemManager _itemManager;
        private readonly IReadOnlyDictionary<int, AreaManager.AreaEnum> _areaStashBags;
        private readonly BaseUnityPlugin _unityPlugin;
        private IModifLogger Logger => _getLogger.Invoke();
        private readonly Func<IModifLogger> _getLogger;

        public StashPackWorldData(ItemManager itemManager, BaseUnityPlugin unityPlugin, IReadOnlyDictionary<int, AreaManager.AreaEnum> areaStashBags, Func<IModifLogger> getLogger)
        {
            (_itemManager, _unityPlugin, _areaStashBags, _getLogger) = (itemManager, unityPlugin, areaStashBags, getLogger);
        }

        public StashPack GetStashPack(string UID)
        {
            var bag = _itemManager.GetItem(UID) as Bag;
            if (bag == null)
            {
                return null;
            }

            Logger.LogDebug($"{nameof(StashPackWorldData)}::{nameof(GetStashPack)}('{UID}'): Found Bag with ItemID of {bag.ItemID}.");
            if (!_areaStashBags.TryGetValue(bag.ItemID, out var areaEnum))
            {
                return null;
            }

            return new StashPack()
            {
                StashBag = bag,
                HomeArea = areaEnum
            };

        }

        /// <summary>
        /// Get's all StashPacks currently loaded into the world for for the <paramref name="previousOwnerUID"/>.
        /// </summary>
        /// <param name="previousOwnerUID">The UID to compare to the <see cref="Item.PreviousOwnerUID"/> property.</param>
        /// <returns>Collection of <see cref="StashPack"/> instances found in the WorldItems collection.</returns>
        public IEnumerable<StashPack> GetStashPacks(string previousOwnerUID)
        {
            var bags = ItemManager.Instance.WorldItems.Values.FindAll(wi =>
                    wi.IsStashBag()
                    && wi.PreviousOwnerUID == previousOwnerUID
                );
            if (bags == default)
            {
                Logger.LogDebug($"{nameof(StashPackWorldData)}::{nameof(GetAllStashPacks)}: No stash packs found loaded in {nameof(ItemManager.WorldItems)} for player character UID {previousOwnerUID}.");
                return null;
            }

            return bags.Select(b => new StashPack()
            {
                StashBag = b as Bag,
                HomeArea = _areaStashBags[b.ItemID]
            });
        }
        public IEnumerable<StashPack> GetDeployedStashPacks()
        {
            var bags = ItemManager.Instance.WorldItems.Values.FindAll(wi =>
                    wi.IsStashBag() && ((Bag)wi).IsUsable()
                );
            if (bags == default)
            {
                Logger.LogDebug($"{nameof(StashPackWorldData)}::{nameof(GetDeployedStashPacks)}: No stash packs found loaded in {nameof(ItemManager.WorldItems)}.");
                return null;
            }
            return bags.Select(b => new StashPack()
            {
                StashBag = b as Bag,
                HomeArea = _areaStashBags[b.ItemID]
            });
        }
        public IEnumerable<StashPack> GetUnownedStashPacks()
        {
            var bags = ItemManager.Instance.WorldItems.Values.FindAll(wi =>
                    wi.IsStashBag() && ((Bag)wi).PreviousOwnerUID == string.Empty
                    && !wi.IsInContainer
                    && !wi.IsEquipped
                );
            if (bags == default)
            {
                Logger.LogDebug($"{nameof(StashPackWorldData)}::{nameof(GetUnownedStashPacks)}: No stash packs found loaded in {nameof(ItemManager.WorldItems)}.");
                return null;
            }
            return bags.Select(b => new StashPack()
            {
                StashBag = b as Bag,
                HomeArea = _areaStashBags[b.ItemID]
            });
        }
        public IEnumerable<StashPack> GetAllStashPacks()
        {
            var bags = ItemManager.Instance.WorldItems.Values.FindAll(wi =>
                    wi.IsStashBag()
                );
            if (bags == default)
            {
                Logger.LogDebug($"{nameof(StashPackWorldData)}::{nameof(GetAllStashPacks)}: No stash packs found loaded in {nameof(ItemManager.WorldItems)}.");
                return null;
            }
            return bags.Select(b => new StashPack()
            {
                StashBag = b as Bag,
                HomeArea = _areaStashBags[b.ItemID]
            });
        }
    }
}
