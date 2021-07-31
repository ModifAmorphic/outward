using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.StashPacks.State;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace ModifAmorphic.Outward.StashPacks.WorldInstance.MajorEvents
{
    internal abstract class MajorBagActions
    {
        protected readonly InstanceFactory _instances;

        protected IModifLogger Logger => _getLogger.Invoke();
        protected readonly Func<IModifLogger> _getLogger;

        public MajorBagActions(InstanceFactory instances, Func<IModifLogger> getLogger) => (_instances, _getLogger) = (instances, getLogger);

        protected bool IsWorldLoaded()
        {
            return NetworkLevelLoader.Instance.IsOverallLoadingDone;
        }
        protected bool IsHost(Character character)
        {

            return character.OwnerPlayerSys.IsMasterClient && character.OwnerPlayerSys.PlayerID == 0;
        }
        protected AreaManager.AreaEnum GetCurrentAreaEnum()
        {
            var sceneName = _instances.AreaManager.CurrentArea.SceneName;
            return (AreaManager.AreaEnum)_instances.AreaManager.GetAreaIndexFromSceneName(sceneName);
        }
        protected AreaManager.AreaEnum GetBagAreaEnum(Bag bag)
        {
            return _instances.AreaStashPackItemIds[bag.ItemID];
        }
        protected void ClearBagPreviousOwner(Bag bag)
        {
            bag.PreviousOwnerUID = string.Empty;
        }
        protected void DoAfterBagLoaded(Bag bag, Action action)
        {
            _instances.UnityPlugin.StartCoroutine(AfterBagLoadedCoroutine(bag, action));
        }
        protected IEnumerator AfterBagLoadedCoroutine(Bag bag, Action action)
        {
            if (_instances.TryGetItemManager(out var itemManager))
            {
                var worldBag = itemManager.GetItem(bag.UID);

                while (!itemManager.IsAllItemSynced || worldBag == null || string.IsNullOrWhiteSpace(worldBag.PreviousOwnerUID))
                {
                    worldBag = itemManager.GetItem(bag.UID);
                    yield return new WaitForSeconds(.2f);
                }
                Logger.LogDebug($"{nameof(MajorBagActions)}::{nameof(AfterBagLoadedCoroutine)}: Bag '{bag.Name}' ({bag.UID}) finished" +
                    $" loading into world. Invoking action {action.Method.Name}.");
                action.Invoke();
            }
            else
            {
                Logger.LogError($"{nameof(MajorBagActions)}::{nameof(AfterBagLoadedCoroutine)}: Unexpected error. Unable " +
                    $"to retrieve {nameof(ItemManager)} instance. StashPack functionality may not work correctly for " +
                    $"bag '{bag.Name}' ({bag.UID})");
            }
        }
    }
}
