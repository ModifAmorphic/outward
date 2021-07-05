//using HarmonyLib;
//using ModifAmorphic.Outward.Extensions;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using UnityEngine;
//using ModifAmorphicLogging = ModifAmorphic.Outward.Logging;

//namespace ModifAmorphic.Outward.KeyBindings
//{
//    [HarmonyPatch(typeof(QuickSlotPanel), "Update")]
//    static class QuickSlotPanelListener
//    {
//        private static ModifAmorphicLogging.Logger _logger;
//        private static bool _barsMoved = false;
//        public static void Init(ModifAmorphicLogging.Logger logger)
//        {
//            _logger = logger;
//        }
//        [HarmonyPostfix]
//        private static void OnUpdate_MoveBars(QuickSlotPanel __instance)
//        {
//            _logger.LogTrace($"OnUpdate_MoveBar() called");
//            try
//            {
//                if (!_barsMoved && __instance.name == "Keyboard" && __instance.transform.parent.name == "QuickSlot")
//                {
//                    // original mod doesn't seem to check if the stability bar belongs to the right character. I added a check here just in case.
//                    var stabilityDisplay = Resources.FindObjectsOfTypeAll<StabilityDisplay_Simple>()
//                        .FirstOrDefault(s => s.LocalCharacter.UID == __instance.LocalCharacter.UID);


//                    // Drop the stability bar to 1/3 of its original height
//                    stabilityDisplay.transform.position = new Vector3(
//                        stabilityDisplay.transform.position.x,
//                        stabilityDisplay.transform.position.y / 3f,
//                        stabilityDisplay.transform.position.z
//                    );

//                    // Get stability bar rect bounds
//                    Vector3[] stabilityRect = new Vector3[4];
//                    stabilityDisplay.RectTransform.GetWorldCorners(stabilityRect);
//                    // Set new quickslot bar height
//                    float newY = stabilityRect[1].y + stabilityRect[0].y;
//                    __instance.transform.parent.position = new Vector3(
//                        __instance.transform.parent.position.x,
//                        newY,
//                        __instance.transform.parent.position.z
//                    );
//                    _barsMoved = true;
//                }
//            }
//            catch (Exception ex)
//            {
//                _logger.LogException($"{nameof(OnUpdate_MoveBars)}() error.", ex);
//                throw;
//            }
//        }
        
//        //public void QuickSlotPanel_Update(On.QuickSlotPanel.orig_Update orig, QuickSlotPanel quickSlotPanel)
//        //{
//        //    //orig(quickSlotPanel);
//        //    var uiElement = (UIElement)quickSlotPanel;
//        //    // UIElement.Update() fix:
//        //    if (quickSlotPanel.GetHideWanted() && quickSlotPanel.IsDisplayed)
//        //    {
//        //        quickSlotPanel.HideInstant();
//        //    }

//        //    // get private fields
//        //    var quickSlotDisplays = quickSlotPanel.GetQuickSlotDisplays();
//        //    var localCharacter = uiElement.LocalCharacter;
//        //    var lastCharacter = quickSlotPanel.GetLastCharacter();
//        //    var initialized = quickSlotPanel.GetInitialized();
//        //    _logger.LogTrace($"QuickSlotPanel_Update() localCharacter.UID={localCharacter?.UID}; lastCharacter.UID={lastCharacter?.UID}; quickSlotPanel.GetInitialized()={initialized}");
//        //    if ((ReferenceEquals(localCharacter, null) || lastCharacter?.UID != localCharacter.UID) && initialized)
//        //    {
//        //        quickSlotPanel.SetInitialized(false);
//        //    }
//        //    if (ReferenceEquals(localCharacter, null))
//        //        return;

//        //    if (initialized)
//        //    {
//        //        if (quickSlotPanel.UpdateInputVisibility)
//        //        {
//        //            var isActive = quickSlotPanel.GetActive();
//        //            for (int i = 0; i < quickSlotDisplays.Count(); i++)
//        //            {
//        //                quickSlotDisplays[i].SetInputTargetAlpha((!isActive) ? 0f : 1f);
//        //            }
//        //        }
//        //    }
//        //    else
//        //    {
//        //        InitializeQuickSlotPanel(quickSlotPanel);
//        //    }
//        //}
//        //private void InitializeQuickSlotPanel(QuickSlotPanel quickSlotPanel)
//        //{
//        //    var quickSlotDisplays = quickSlotPanel.GetQuickSlotDisplays();
//        //    for (int j = 0; j < quickSlotDisplays.Length; j++)
//        //    {
//        //        int refSlotID = quickSlotDisplays[j].RefSlotID;
//        //        Debug.Log($"InitializeQuickSlotPanel() j={j}; refSlotID={refSlotID}");
//        //        quickSlotDisplays[j].SetQuickSlot(quickSlotPanel.LocalCharacter.QuickSlotMngr.GetQuickSlot(refSlotID));
//        //    }

//        //    quickSlotPanel.SetLastCharacter(quickSlotPanel.LocalCharacter);
//        //    quickSlotPanel.SetInitialized(true);

//        //    if (quickSlotPanel.name == "Keyboard" && quickSlotPanel.transform.parent.name == "QuickSlot")
//        //    {
//        //        // original mod doesn't seem to check if the stability bar belongs to the right character. I added a check here just in case.
//        //        var stabilityDisplay = Resources.FindObjectsOfTypeAll<StabilityDisplay_Simple>()
//        //            .FirstOrDefault(s => s.LocalCharacter.UID == quickSlotPanel.LocalCharacter.UID);


//        //        // Drop the stability bar to 1/3 of its original height
//        //        stabilityDisplay.transform.position = new Vector3(
//        //            stabilityDisplay.transform.position.x,
//        //            stabilityDisplay.transform.position.y / 3f,
//        //            stabilityDisplay.transform.position.z
//        //        );

//        //        // Get stability bar rect bounds
//        //        Vector3[] stabilityRect = new Vector3[4];
//        //        stabilityDisplay.RectTransform.GetWorldCorners(stabilityRect);
//        //        // Set new quickslot bar height
//        //        float newY = stabilityRect[1].y + stabilityRect[0].y;
//        //        quickSlotPanel.transform.parent.position = new Vector3(
//        //            quickSlotPanel.transform.parent.position.x,
//        //            newY,
//        //            quickSlotPanel.transform.parent.position.z
//        //        );

//        //        //if (_settings.CenterQuickSlots)
//        //        //{
//        //        //    CenterQuickSlotPanel(quickSlotPanel);
//        //        //}
//        //    }
//        //}
//    }
//}
