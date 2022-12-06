using HarmonyLib;
using ModifAmorphic.Outward.UnityScripts.Behaviours;
using ModifAmorphic.Outward.UnityScripts.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ModifAmorphic.Outward.UnityScripts.Services
{
    public class LedgerMenuService
    {
        private readonly Func<Logging.Logger> _loggerFactory;
        private Logging.Logger Logger => _loggerFactory.Invoke();
        private readonly BuildingVisualPoolManager _buildingVisualPool;


        private readonly Type _ledgerMenuType = OutwardAssembly.Types.LedgerMenu;
        private readonly Type _buildingType = OutwardAssembly.Types.Building;

        private readonly Enum _foodResourceRequirement;

        public LedgerMenuService(BuildingVisualPoolManager buildingVisualPool, Func<Logging.Logger> loggerFactory)
        {
            (_buildingVisualPool, _loggerFactory) = (buildingVisualPool, loggerFactory);
            Patch();
            BeforeStartInit += AddFoodDisplay;
            AfterShow += HandleCancelButton;

            TryOverrideGetBuildingResource = TryGetBuildingResource;

            var lastValue = Enum.GetValues(typeof(ResourceRequirement))
                .Cast<ResourceRequirement>()
                .Max();
            ResourceRequirement req = lastValue + 1;
            _foodResourceRequirement = req.ToOutwardEnumValue();
        }

        private void HandleCancelButton(MonoBehaviour ledgerMenu)
        {
            ToggleCancelButton(ledgerMenu);
            PositionButtons(ledgerMenu);
        }

        private void AddFoodDisplay(MonoBehaviour ledgerMenu)
        {
            var resourceReqDisplays = ledgerMenu.GetComponentsInChildren(OutwardAssembly.Types.BuildingPhaseResourceReqDisplay, true);
            //if (resourceReqDisplays.Any(rd => rd.name == "ReqFood"))
            //    return;

            var reqFunds = resourceReqDisplays.FirstOrDefault(r => r.name == "ReqFunds");
            bool activeState = reqFunds.gameObject.activeSelf;
            reqFunds.gameObject.SetActive(false);
            var reqFood = UnityEngine.Object.Instantiate(reqFunds, reqFunds.transform.parent);
            reqFood.gameObject.DeCloneNames(true);
            reqFood.gameObject.name = "ReqFood";
            reqFood.SetField(OutwardAssembly.Types.BuildingPhaseResourceReqDisplay, "Resource", _foodResourceRequirement);
            reqFood.transform.SetSiblingIndex(reqFunds.transform.GetSiblingIndex() + 1);
            //var nameText = reqFood.GetPrivateField<BuildingPhaseResourceReqDisplay, Text>("m_lblResourceName");

            reqFunds.gameObject.SetActive(activeState);
            reqFood.gameObject.SetActive(activeState);

            Logger.LogDebug($"Added Food Requirement display label {reqFood.name} to LedgerMenu for building {ledgerMenu.GetFieldValue<Text>(_ledgerMenuType, "m_lblBuildingName")?.text}.");
        }

        private bool TryGetBuildingResource(MonoBehaviour resourceDisplay, out Enum resourceRequirement)
        {
            resourceRequirement = default;
            var resource = resourceDisplay.GetFieldValue<Enum>(OutwardAssembly.Types.BuildingPhaseResourceReqDisplay, "Resource");
            if (!resource.Equals(_foodResourceRequirement))
            {
                //Logger.LogDebug($"{resource.GetType()} {resource} does not equal {_foodResourceRequirement.GetType()} {_foodResourceRequirement}.");
                return false;
            }

            resourceRequirement = ResourceTypes.Food.ToOutwardEnumValue();
            Logger.LogDebug($"Set resourceRequirement {resourceRequirement.GetType()} to {resourceRequirement} {(int)(object)resourceRequirement}.");
            return true;
        }

        private void ToggleCancelButton(MonoBehaviour ledgerMenu)
        {
            var building = GetBuilding(ledgerMenu);
            Logger.LogDebug($"AddCancelButton for Building {building?.name}.");
            if (building == null)
                return;

            //get all the tranforms involved in creating and placing the new button
            var mainXform = ledgerMenu.transform.Find("Content/MainPanel") as RectTransform;
            var constructionXform = mainXform.Find("Construction");
            var cancelXform = constructionXform.Find("btnCancelConstruction") as RectTransform;
            var buildingExt = building.GetComponent<BuildingExt>();
            if (cancelXform != null)
            {
                if (buildingExt != null && buildingExt.DisallowCancelConstruction && cancelXform.gameObject.activeSelf)
                {
                    cancelXform.gameObject.SetActive(false);
                }
                else if (!cancelXform.gameObject.activeSelf)
                {
                    cancelXform.gameObject.SetActive(true);
                }
                return;
            }

            if (buildingExt != null && buildingExt.DisallowCancelConstruction)
                return;

            var startXform = constructionXform.Find("btnStartConstruction") as RectTransform;
            
            //set navigation to explicit before instantiating a copy
            var startButton = startXform.GetComponent<Button>();
            var startNav = startButton.navigation;
            startNav.mode = Navigation.Mode.Explicit;
            startButton.navigation = startNav;
            

            //Instantiate the cancel button from the start button
            var startActive = startXform.gameObject.activeSelf;
            startXform.gameObject.SetActive(false);
            var cancelButton = UnityEngine.Object.Instantiate(startXform.gameObject, constructionXform).GetComponent<Button>();
            cancelXform = cancelButton.GetComponent<RectTransform>();
            if (startActive)
                startXform.gameObject.SetActive(true);

            //configure the button text
            cancelButton.name = "btnCancelConstruction";
            var cancelText = cancelButton.GetComponentInChildren<Text>();
            var uiLocalizer = cancelText.GetComponent(OutwardAssembly.Types.UILocalize);
            if (uiLocalizer != null)
                UnityEngine.Object.Destroy(uiLocalizer);

            cancelText.text = "Cancel Construction";
            
            //set up button click event
            cancelButton.onClick.RemoveAllListeners();
            //This removes any persistent (set in Unity Editor) onClick listeners.
            cancelButton.onClick = new Button.ButtonClickedEvent();
            cancelButton.onClick.AddListener(() => CancelBuilding(ledgerMenu));

            //configure navigation
            cancelXform.SetSiblingIndex(startXform.GetSiblingIndex() + 1);
            startNav.selectOnLeft = cancelButton;
            startNav.selectOnRight = cancelButton;
            startButton.navigation = startNav;
            var cancelNav = cancelButton.navigation;
            cancelNav.selectOnLeft = startButton;
            cancelNav.selectOnRight = startButton;
            cancelButton.navigation = cancelNav;
            AddButtonEventTrigger(startButton);
            AddButtonEventTrigger(cancelButton);
            startButton.onClick.AddListener(() => PositionButtons(ledgerMenu));

            //activate new cancel button
            cancelButton.gameObject.SetActive(true);
        }

        private void PositionButtons(MonoBehaviour ledgerMenu)
        {
            var building = GetBuilding(ledgerMenu);

            if (building == null)
                return;

            var mainXform = ledgerMenu.transform.Find("Content/MainPanel") as RectTransform;
            var constructionXform = mainXform.Find("Construction");
            var cancelXform = constructionXform.Find("btnCancelConstruction") as RectTransform;

            if (cancelXform == null || !cancelXform.gameObject.activeSelf)
                return;

            var startXform = constructionXform.Find("btnStartConstruction") as RectTransform;

            //reposition buttons
            var mainWidth = mainXform.rect.width;
            var btnWidth = startXform.rect.width;
            var widthSpace = mainWidth - btnWidth * 2;
            var padding = widthSpace / 3;
            var startXPos = -(mainWidth / 2) + btnWidth / 2 + padding;
            var cancelXPos = (mainWidth / 2) - btnWidth / 2 - padding;

            if (startXform.gameObject.activeSelf)
                startXform.anchoredPosition = new Vector2(startXPos, startXform.anchoredPosition.y);

            if (cancelXform.gameObject.activeSelf)
            {
                if (!building.GetPropertyValue<bool>(_buildingType, "IsInContruction"))
                    cancelXform.anchoredPosition = new Vector2(cancelXPos, startXform.anchoredPosition.y);
                else
                    cancelXform.anchoredPosition = new Vector2(cancelXPos + padding / 2, startXform.anchoredPosition.y - 68f);
            }
        }

        private void CancelBuilding(MonoBehaviour ledgerMenu)
        {
            var building = GetBuilding(ledgerMenu);
            if (building == null)
                return;

            building.SetField(OutwardAssembly.Types.Building, "m_loadedVisual", null);
            //_buildingVisualPool.PutbackVisual(building.GetFieldValue<int>(OutwardAssembly.Types.Building, "ItemID"), itemVisual);
            int buildingID = building.GetFieldValue<int>(OutwardAssembly.Types.Building, "ItemID");
            _buildingVisualPool.PutbackNewVisual(buildingID);
            UnityEngine.Object.Destroy(building.gameObject);
            //UnityEngine.Object.Destroy(itemVisual.gameObject);
            
            
            
            //var setLinkedItem = OutwardAssembly.Types.ItemVisual.GetMethod("SetLinkedItem", BindingFlags.Public | BindingFlags.Instance, null, new Type[] { OutwardAssembly.Types.Item }, null);
            //setLinkedItem.Invoke(itemVisual, new object[] { null });
            //itemVisual.SetField(OutwardAssembly.Types.BuildingVisual, "m_previousActivePhaseIndex", 0);

            var charUI = (MonoBehaviour)ledgerMenu.GetFieldValue(_ledgerMenuType, "m_characterUI");
            CharacterUIProxy.ShowInfoNotification(charUI, $"{building.GetPropertyValue<string>(_buildingType, "Name")} building construction canceled.");
            var hideMethod = OutwardAssembly.Types.LedgerMenu.GetMethod("Hide", BindingFlags.Public | BindingFlags.Instance, null, new Type[0], null);
            hideMethod.Invoke(ledgerMenu, new object[0]);
        }

        private void AddButtonEventTrigger(Button button)
        {
            var entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.PointerEnter;
            entry.callback.AddListener((data) => button.Select());

            var eventTrigger = button.gameObject.AddComponent<EventTrigger>();
            eventTrigger.triggers.Add(entry);
        }

        private MonoBehaviour GetBuilding(MonoBehaviour ledgerMenu) => (MonoBehaviour) ledgerMenu.GetFieldValue(_ledgerMenuType, "m_building");

        #region Patches
        private void Patch()
        {
            Logger.LogInfo("Patching LedgerMenu");

            var startInit = OutwardAssembly.Types.LedgerMenu.GetMethod("StartInit", BindingFlags.NonPublic | BindingFlags.Instance);
            var startInitPrefix = this.GetType().GetMethod(nameof(StartInitPrefix), BindingFlags.NonPublic | BindingFlags.Static);
            HarmonyPatcher.Instance.Harmony.Patch(startInit, prefix: new HarmonyMethod(startInitPrefix));

            var argTypes = new Type[1] { OutwardAssembly.Types.Item };
            var show = OutwardAssembly.Types.LedgerMenu.GetMethod("Show", BindingFlags.Public | BindingFlags.Instance, null, argTypes, null);
            var showPostfix = this.GetType().GetMethod(nameof(ShowPostfix), BindingFlags.NonPublic | BindingFlags.Static);
            HarmonyPatcher.Instance.Harmony.Patch(show, postfix: new HarmonyMethod(showPostfix));

            var getBuildingResource = OutwardAssembly.Types.BuildingPhaseResourceReqDisplay.GetMethod("GetBuildingResource", BindingFlags.Public | BindingFlags.Instance, null, new Type[0], null);
            var getBuildingResourcePrefix = this.GetType().GetMethod(nameof(GetBuildingResourcePrefix), BindingFlags.NonPublic | BindingFlags.Static);
            HarmonyPatcher.Instance.Harmony.Patch(getBuildingResource, prefix: new HarmonyMethod(getBuildingResourcePrefix));
        }

        public static event Action<MonoBehaviour> BeforeStartInit;

        private static void StartInitPrefix(MonoBehaviour __instance)
        {
            try
            {
#if DEBUG
                ModifScriptsManager.Instance.Logger.LogTrace($"{nameof(LedgerMenuService)}::{nameof(StartInitPrefix)}(): Invoking {nameof(BeforeStartInit)}.");
#endif
                BeforeStartInit?.Invoke(__instance);
            }
            catch (Exception ex)
            {
                ModifScriptsManager.Instance.Logger.LogException($"{nameof(LedgerMenuService)}::{nameof(StartInitPrefix)}(): Exception invoking {nameof(BeforeStartInit)}.", ex);
            }
        }

        public static event Action<MonoBehaviour> AfterShow;

        private static void ShowPostfix(MonoBehaviour __instance) //LedgerMenu
        {
            try
            {
#if DEBUG
                ModifScriptsManager.Instance.Logger.LogTrace($"{nameof(LedgerMenuService)}::{nameof(ShowPostfix)}(): Invoking {nameof(AfterShow)}.");
#endif
                AfterShow?.Invoke(__instance);
            }
            catch (Exception ex)
            {
                ModifScriptsManager.Instance.Logger.LogException($"{nameof(LedgerMenuService)}::{nameof(ShowPostfix)}(): Exception invoking {nameof(AfterShow)}.", ex);
            }
        }

        public delegate bool TryOverrideGetBuildingResourceDelegate(MonoBehaviour resourceDisplay, out Enum resourceTypeResult);
        public static TryOverrideGetBuildingResourceDelegate TryOverrideGetBuildingResource;

        private static bool GetBuildingResourcePrefix(MonoBehaviour __instance, ref object __result)
        {
            try
            {
#if DEBUG
                ModifScriptsManager.Instance.Logger.LogTrace($"{nameof(LedgerMenuService)}::{nameof(GetBuildingResourcePrefix)}(): Invoking {nameof(TryOverrideGetBuildingResource)}.");
#endif
                if (TryOverrideGetBuildingResource?.Invoke(__instance, out var resourceType) ?? false)
                {
                    __result = resourceType;
                    ModifScriptsManager.Instance.Logger.LogTrace($"{nameof(LedgerMenuService)}::{nameof(GetBuildingResourcePrefix)}(): {nameof(TryOverrideGetBuildingResource)} success. Overriding GetBuildingResource result to {__result} {(int)(object)__result}.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                ModifScriptsManager.Instance.Logger.LogException($"{nameof(LedgerMenuService)}::{nameof(GetBuildingResourcePrefix)}(): Exception invoking {nameof(TryOverrideGetBuildingResource)}.", ex);
            }

            return true;
        }
        #endregion
    }
}
