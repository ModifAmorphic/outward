//using ModifAmorphic.Outward.Logging;
//using ModifAmorphic.Outward.StashPacks.WorldInstance.Extensions;
//using System;
//using System.Collections.Generic;
//using System.Text;

//namespace ModifAmorphic.Outward.StashPacks.Patch.Events
//{
//    internal static class ItemContainerStaticEvents
//    {
//        private static IModifLogger Logger => LoggerFactory?.Invoke();
//        public static Func<IModifLogger> LoggerFactory { get; set; }

//        public enum ContentChanges
//        {
//            ItemRemoved,
//            ItemAdded,
//            ItemQuantityChanged
//        }
//        /// <summary>
//        /// Triggers after an Item is Added or Removed. Quantity changes aren't raised at this time.
//        /// </summary>
//        public static event Action<Bag, ContentChanges> BagContentsChangedAfter;

//        public static void RaiseBagContentsChangedAfter(Bag bag, ContentChanges contentChange)
//        {
//            try
//            {
//                //Extra null check here because it is possible for this event to be raised with a null Bag.
//                Logger?.LogDebug($"{nameof(ItemContainerStaticEvents)}::{nameof(RaiseBagContentsChangedAfter)} triggered for bag '{bag?.Name}'.");
//                if (bag != null && bag.IsStashBag())
//                    BagContentsChangedAfter?.Invoke(bag, contentChange);

//            }
//            catch (Exception ex)
//            {
//                Logger?.LogException($"Exception in {nameof(ItemContainerStaticEvents)}::{nameof(RaiseBagContentsChangedAfter)}.", ex);
//                if (Logger == null)
//                    UnityEngine.Debug.LogError($"Exception in {nameof(ItemContainerStaticEvents)}::{nameof(RaiseBagContentsChangedAfter)}:\n{ex}");
//            }
//        }
//    }
//}
