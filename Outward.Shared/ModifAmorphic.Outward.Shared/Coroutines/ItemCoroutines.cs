using BepInEx;
using ModifAmorphic.Outward.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace ModifAmorphic.Outward.Coroutines
{
    public class ItemCoroutines : ModifCoroutine
    {
        private readonly BaseUnityPlugin _unityPlugin;
        private readonly Func<ItemManager> _itemManagerFactory;
        private ItemManager _itemManager;
        private ItemManager ItemManager => _itemManager ?? (_itemManager = _itemManagerFactory.Invoke());


        public ItemCoroutines(BaseUnityPlugin unityPlugin, Func<ItemManager> itemManagerFactory, Func<IModifLogger> getLogger) : base(getLogger) => 
            (_unityPlugin, _itemManagerFactory) = (unityPlugin, itemManagerFactory);

        /// <summary>
        /// Executes the action after no item is returned from ItemManager.GetItem(<paramref name="itemUID"/>)
        /// </summary>
        /// <param name="itemUID">The ItemUID to look for</param>
        /// <param name="action">The action to execute on the item once it is loaded.</param>
        /// <param name="timeoutSecs">Time the coroutine should wait until giving up.</param>
        /// <param name="ticSeconds">The time of the tic between each check.</param>
        public void InvokeAfterItemDestroyed(string itemUID, Action action, int timeoutSecs, float ticSeconds = DefaultTicSeconds)
        {
            Func<bool> itemDestroyedcondition = () => (ItemManager.GetItem(itemUID) == null);
            _unityPlugin.StartCoroutine(base.InvokeAfter(itemDestroyedcondition, action, timeoutSecs, ticSeconds));
        }
        /// <summary>
        /// Executes the action after no item is returned from ItemManager.GetItem(<paramref name="itemUID"/>)
        /// </summary>
        /// <param name="itemUID">The ItemUID to look for</param>
        /// <param name="action">The action to execute on the item once it is loaded.</param>
        /// <param name="timeoutSecs">Time the coroutine should wait until giving up.</param>
        /// <param name="ticSeconds">The time of the tic between each check.</param>
        /// <param name="cancelCondition">If this condition is met, the coroutine will be canceled and no action will be invoked.</param>
        public void InvokeAfterItemDestroyed(string itemUID, Action action, int timeoutSecs, float ticSeconds = DefaultTicSeconds, Func<bool> cancelCondition = null)
        {
            Func<bool> itemDestroyedcondition = () => (ItemManager.GetItem(itemUID) == null);
            _unityPlugin.StartCoroutine(base.InvokeAfter(itemDestroyedcondition, action, timeoutSecs, ticSeconds, cancelCondition));
        }

        /// <summary>
        /// Executes the action after ItemManager.IsAllItemSynced and an item is returned from ItemManager.GetItem(<paramref name="itemUID"/>)
        /// </summary>
        /// <param name="itemUID">The ItemUID to look for</param>
        /// <param name="action">The action to execute on the item once it is loaded.</param>
        /// <param name="timeoutSecs">Time the coroutine should wait until giving up.</param>
        /// <param name="ticSeconds">The time of the tic between each check.</param>
        public void InvokeAfterItemLoaded(string itemUID, Action<Item> action, int timeoutSecs, float ticSeconds = DefaultTicSeconds)
        {
            Func<bool> itemLoadedcondition = () => (ItemManager.IsAllItemSynced &&
                                                    ItemManager.GetItem(itemUID) != null);
            Func<Item> getItem = () => ItemManager.GetItem(itemUID);
            _unityPlugin.StartCoroutine(base.InvokeAfter(itemLoadedcondition, action, getItem, timeoutSecs, ticSeconds));
        }
        /// <summary>
        /// Executes the action after ItemManager.IsAllItemSynced and an item is returned from ItemManager.GetItem(<paramref name="itemUID"/>)
        /// </summary>
        /// <param name="itemUID">The ItemUID to look for</param>
        /// <param name="action">The action to execute on the item once it is loaded.</param>
        /// <param name="timeoutSecs">Time the coroutine should wait until giving up.</param>
        /// <param name="ticSeconds">The time of the tic between each check.</param>
        /// <param name="cancelCondition">If this condition is met, the coroutine will be canceled and no action will be invoked.</param>
        public void InvokeAfterItemLoaded(string itemUID, Action<Item> action, int timeoutSecs, float ticSeconds = DefaultTicSeconds, Func<bool> cancelCondition = null)
        {
            Func<bool> itemLoadedcondition = () => (ItemManager.IsAllItemSynced &&
                                                    ItemManager.GetItem(itemUID) != null);
            Func<Item> getItem = () => ItemManager.GetItem(itemUID);
            _unityPlugin.StartCoroutine(base.InvokeAfter(itemLoadedcondition, action, getItem, timeoutSecs, ticSeconds, cancelCondition));
        }
        /// <summary>
        /// Executes the action after ItemManager.IsAllItemSynced, an item is returned from ItemManager.GetItem(<paramref name="itemUID"/>) and
        /// the <paramref name="additonalCondition"/> is met.
        /// </summary>
        /// <param name="itemUID">The ItemUID to look for</param>
        /// <param name="additonalCondition"></param>
        /// <param name="action">The action to execute on the item once it is loaded.</param>
        /// <param name="timeoutSecs">Time the coroutine should wait until giving up.</param>
        /// <param name="ticSeconds">The time of the tic between each check.</param>
        public void InvokeAfterItemLoaded(string itemUID, Func<Item, bool> additonalCondition, Action<Item> action, int timeoutSecs, float ticSeconds = DefaultTicSeconds)
        {
            Func<bool> itemLoadedcondition = () =>
            {
                var item = ItemManager.GetItem(itemUID);
                return ItemManager.IsAllItemSynced &&
                        item != null &&
                        additonalCondition.Invoke(item);
            };
            Func<Item> getItem = () => ItemManager.GetItem(itemUID);
            _unityPlugin.StartCoroutine(base.InvokeAfter(itemLoadedcondition, action, getItem, timeoutSecs, ticSeconds));
        }
        /// <summary>
        /// Executes the action after ItemManager.IsAllItemSynced, an item is returned from ItemManager.GetItem(<paramref name="itemUID"/>) and
        /// the <paramref name="additonalCondition"/> is met.
        /// </summary>
        /// <param name="itemUID">The ItemUID to look for</param>
        /// <param name="additonalCondition"></param>
        /// <param name="action">The action to execute on the item once it is loaded.</param>
        /// <param name="timeoutSecs">Time the coroutine should wait until giving up.</param>
        /// <param name="ticSeconds">The time of the tic between each check.</param>
        /// <param name="cancelCondition">If this condition is met, the coroutine will be canceled and no action will be invoked.</param>
        public void InvokeAfterItemLoaded(string itemUID, Func<Item, bool> additonalCondition, Action<Item> action, int timeoutSecs, float ticSeconds = DefaultTicSeconds, Func<Item, bool> cancelCondition = null)
        {
            Func<bool> itemLoadedcondition = () =>
            {
                var item = ItemManager.GetItem(itemUID);
                return ItemManager.IsAllItemSynced &&
                        item != null &&
                        additonalCondition.Invoke(item);
            };
            Func<Item> getItem = () => ItemManager.GetItem(itemUID);
            _unityPlugin.StartCoroutine(base.InvokeAfter(itemLoadedcondition, action, getItem, timeoutSecs, ticSeconds, () => cancelCondition(getItem.Invoke())));
        }
    }
}
