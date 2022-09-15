﻿using HarmonyLib;
using ModifAmorphic.Outward.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace ModifAmorphic.Outward.UI.Patches
{
    [HarmonyPatch(typeof(QuickSlotControllerSwitcher))]
    internal class QuickSlotControllerSwitcherPatches
    {
        private static IModifLogger Logger => LoggerFactory.GetLogger(ModInfo.ModId);

        public delegate void StartInit(QuickSlotControllerSwitcher controllerSwitcher);
        public static event StartInit StartInitAfter;
        [HarmonyPatch("StartInit")]
        [HarmonyPostfix]
        private static void StartInitPostfix(QuickSlotControllerSwitcher __instance)
        {
            try
            {
                Logger.LogTrace($"{nameof(QuickSlotControllerSwitcherPatches)}::{nameof(StartInitPostfix)}(): Invoked. Invoking {nameof(StartInitAfter)}({nameof(KeyboardQuickSlotPanel)}).");
                StartInitAfter?.Invoke(__instance);
            }
            catch (Exception ex)
            {
                Logger.LogException($"{nameof(QuickSlotControllerSwitcherPatches)}::{nameof(StartInitPostfix)}(): Exception Invoking {nameof(StartInitAfter)}({nameof(KeyboardQuickSlotPanel)}).", ex);
            }
        }
    }
}
