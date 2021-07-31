﻿using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.StashPacks.Settings;
using System;

namespace ModifAmorphic.Outward.StashPacks.Patch.Events
{
    internal static class BagEvents
    {
        private static IModifLogger Logger => LoggerFactory?.Invoke();
        public static Func<IModifLogger> LoggerFactory { get; set; }

        public static event Action<Bag> BagDropCastBefore;
        public static void RaiseBagDropCastBefore(ref Bag bag)
        {
            try
            {
                //only care about stash backpacks and no other backpacks.
                if (StashPacksConstants.StashBackpackItemIds.ContainsKey(bag.ItemID))
                    BagDropCastBefore?.Invoke(bag);
            }
            catch (Exception ex)
            {
                Logger?.LogException($"Exception in {nameof(BagEvents)}::{nameof(RaiseBagDropCastBefore)}.", ex);
                if (Logger == null)
                    UnityEngine.Debug.LogError($"Exception in {nameof(BagEvents)}::{nameof(RaiseBagDropCastBefore)}:\n{ex}");
            }
        }

        //public static event Action<Bag> PerformEquipBefore;

        //public static void RaisePerformEquipBefore(ref Bag bag)
        //{
        //    try
        //    {
        //        //only care about stash backpacks and no other backpacks.
        //        if (StashPacksConstants.StashBackpackItemIds.ContainsKey(bag.ItemID))
        //            PerformEquipBefore?.Invoke(bag);
        //    }
        //    catch (Exception ex)
        //    {
        //        Logger?.LogException($"Exception in {nameof(BagEvents)}::{nameof(RaisePerformEquipBefore)}.", ex);
        //        if (Logger == null)
        //            UnityEngine.Debug.LogError($"Exception in {nameof(BagEvents)}::{nameof(RaisePerformEquipBefore)}:\n{ex}");
        //    }
        //}
    }
}
