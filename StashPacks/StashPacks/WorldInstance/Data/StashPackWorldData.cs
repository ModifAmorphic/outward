using BepInEx;
using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.StashPacks.WorldInstance.Extensions;
using ModifAmorphic.Outward.StashPacks.WorldInstance.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            => (_itemManager, _unityPlugin, _areaStashBags, _getLogger) = (itemManager, unityPlugin, areaStashBags,  getLogger);


        public StashPack GetStashPack(string UID)
        {
            var bag = _itemManager.GetItem(UID) as Bag;
            if (bag == null)
                return null;

            if (!_areaStashBags.TryGetValue(bag.ItemID, out var areaEnum))
                return null;
            return new StashPack()
            {
                StashBag = bag,
                HomeArea = areaEnum
            };
                
        }
        public IEnumerator GetStashPackWait(string UID, Action<StashPack> invokeAfter)
        {
            StashPack stashPack = null;
            while (stashPack == null)
            {
                stashPack = GetStashPack(UID);
                yield return new WaitForSeconds(1f);
            }
            invokeAfter?.Invoke(stashPack);
        }
        /// <summary>
        /// Get's all StashPacks currently loaded into the world for this instances <see cref="CharacterUID"./>.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<StashPack> GetStashPacks(string characterUID)
        {
            var bags = ItemManager.Instance.WorldItems.Values.FindAll(wi =>
                    wi.IsStashBag()
                    && wi.PreviousOwnerUID == characterUID
                );
            if (bags == default)
            {
                Logger.LogDebug($"No stash packs found loaded in world for player character UID {characterUID}");
                return null;
            }

            return bags.Select(b => new StashPack()
            {
                StashBag = b as Bag,
#if DEBUG
                WantsSyncToStash = true,
#else
                WantsSyncToStash = false,
#endif
                HomeArea = _areaStashBags[b.ItemID]
            });
        }
    }
}
