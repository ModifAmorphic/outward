using ModifAmorphic.Outward.Extensions;
using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.NewCaldera.BuildingsMod.Patches;
using ModifAmorphic.Outward.UnityScripts.Behaviours;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ModifAmorphic.Outward.NewCaldera.BuildingsMod.Services
{
    internal class LedgerMenuService
    {
        private readonly Func<IModifLogger> _getLogger;
        private IModifLogger Logger => _getLogger.Invoke();

        private readonly BuildingPhaseResourceReqDisplay.ResourceRequirement _foodResourceRequirement;

        public LedgerMenuService(Func<IModifLogger> getLogger)
        {
            (_getLogger) = (getLogger);
            //PanelPatches.BeforeLedgerMenuAwakeInit += AddCancelButton;
            LedgerMenuPatches.BeforeStartInit += AddFoodDisplay;
            LedgerMenuPatches.AfterShow += HandleCancelButton;

            BuildingPhaseResourceReqDisplayPatches.TryOverrideGetBuildingResource = TryGetBuildingResource;
            var lastValue = Enum.GetValues(typeof(BuildingPhaseResourceReqDisplay.ResourceRequirement))
                .Cast<BuildingPhaseResourceReqDisplay.ResourceRequirement>()
                .Max();
            _foodResourceRequirement = lastValue + 1;

        }

        private void HandleCancelButton(LedgerMenu ledgerMenu)
        {
            ToggleCancelButton(ledgerMenu);
            PositionButtons(ledgerMenu);
        }

        private void AddFoodDisplay(LedgerMenu ledgerMenu)
        {
            var resourceReqDisplays = ledgerMenu.GetComponentsInChildren<BuildingPhaseResourceReqDisplay>(true);
            //if (resourceReqDisplays.Any(rd => rd.name == "ReqFood"))
            //    return;

            var reqFunds = resourceReqDisplays.FirstOrDefault(r => r.name == "ReqFunds");
            bool activeState = reqFunds.gameObject.activeSelf;
            reqFunds.gameObject.SetActive(false);
            var reqFood = UnityEngine.Object.Instantiate(reqFunds, reqFunds.transform.parent);
            reqFood.gameObject.DeCloneNames(true);
            reqFood.gameObject.name = "ReqFood";
            reqFood.Resource = _foodResourceRequirement;
            reqFood.transform.SetSiblingIndex(reqFunds.transform.GetSiblingIndex() + 1);
            //var nameText = reqFood.GetPrivateField<BuildingPhaseResourceReqDisplay, Text>("m_lblResourceName");

            reqFunds.gameObject.SetActive(activeState);
            reqFood.gameObject.SetActive(activeState);

            Logger.LogDebug($"Added Food Requirement display label {reqFood.name} to LedgerMenu for building {ledgerMenu.GetPrivateField<LedgerMenu, Text>("m_lblBuildingName")?.text}.");
        }

        private bool TryGetBuildingResource(BuildingPhaseResourceReqDisplay resourceDisplay, out BuildingResource.ResourceTypes resourceRequirement)
        {
            resourceRequirement = default;
            if (resourceDisplay.Resource != _foodResourceRequirement)
                return false;

            resourceRequirement = BuildingResource.ResourceTypes.Food;
            return true;
        }

        private void ToggleCancelButton(LedgerMenu ledgerMenu)
        {
            var building = ledgerMenu.GetPrivateField<LedgerMenu, Building>("m_building");
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
            if (cancelText.GetComponent<UILocalize>() != null)
                UnityEngine.Object.Destroy(cancelText.GetComponent<UILocalize>());
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

        private void PositionButtons(LedgerMenu ledgerMenu)
        {
            var building = ledgerMenu.GetPrivateField<LedgerMenu, Building>("m_building");

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
                if (!building.IsInContruction)
                    cancelXform.anchoredPosition = new Vector2(cancelXPos, startXform.anchoredPosition.y);
                else
                    cancelXform.anchoredPosition = new Vector2(cancelXPos + padding / 2, startXform.anchoredPosition.y - 68f);
            }
        }

        private void CancelBuilding(LedgerMenu ledgerMenu)
        {
            var building = ledgerMenu.GetPrivateField<LedgerMenu, Building>("m_building");
            if (building == null)
                return;

            UnityEngine.Object.Destroy(building.gameObject);
            ledgerMenu.CharacterUI.ShowInfoNotification($"{building.Name} building construction canceled.");
            ledgerMenu.Hide();
        }

        private void AddButtonEventTrigger(Button button)
        {
            var entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.PointerEnter;
            entry.callback.AddListener((data) => button.Select());

            var eventTrigger = button.gameObject.AddComponent<EventTrigger>();
            eventTrigger.triggers.Add(entry);
        }
    }
}
