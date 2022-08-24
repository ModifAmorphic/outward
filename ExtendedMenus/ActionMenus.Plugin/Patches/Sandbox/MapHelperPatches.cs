﻿using HarmonyLib;
using ModifAmorphic.Outward.Extensions;
using ModifAmorphic.Outward.Logging;
using Rewired;
using Rewired.Data;
using Rewired.Data.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ModifAmorphic.Outward.ActionMenus.Patches
{

    //[HarmonyPatch(typeof(Player.ControllerHelper.MapHelper))]
    public class MapHelperPatches
    {

    }

    [HarmonyPatch]
    public class MapHelperConstructorPatch
    {
        private static IModifLogger Logger => LoggerFactory.GetLogger(ModInfo.ModId);


        public delegate void NewMapHelperDelegate(Player.ControllerHelper.MapHelper mapHelper, Player player, Player.ControllerHelper parent);
        public static event NewMapHelperDelegate OnNewMapHelper;

        public static MethodBase TargetMethod()
        {
            return typeof(Player.ControllerHelper.MapHelper)
                    .GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic).FirstOrDefault();
        }
        public static void Postfix(Player.ControllerHelper.MapHelper __instance, Player player, Player.ControllerHelper parent)
        {
            try
            {
                Logger.LogTrace($"{nameof(MapHelperConstructorPatch)}::{nameof(Postfix)}(): Invoked for player {player?.id}.");
                OnNewMapHelper?.Invoke(__instance, player, parent);
            }
            catch (Exception ex)
            {
                Logger.LogException($"{nameof(MapHelperConstructorPatch)}::{nameof(Postfix)}(): Exception", ex);
            }
        }
    }
    [HarmonyPatch]
    public class PlayerConstructorPatch
    {
        private static IModifLogger Logger => LoggerFactory.GetLogger(ModInfo.ModId);


        public delegate void NewPlayerDelegate(Player player);
        public static event NewPlayerDelegate OnNewPlayer;

        public static MethodBase TargetMethod()
        {
            return typeof(Player)
                    .GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic).FirstOrDefault();
        }
        public static void Postfix(Player __instance)
        {
            try
            {
                Logger.LogTrace($"{nameof(PlayerConstructorPatch)}::{nameof(Postfix)}(): Invoked for player {__instance?.id}.");
                OnNewPlayer?.Invoke(__instance);
            }
            catch (Exception ex)
            {
                Logger.LogException($"{nameof(PlayerConstructorPatch)}::{nameof(Postfix)}(): Exception", ex);
            }
        }
    }
}