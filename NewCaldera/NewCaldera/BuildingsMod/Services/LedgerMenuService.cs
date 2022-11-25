using ModifAmorphic.Outward.Extensions;
using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.NewCaldera.BuildingsMod.Patches;
using System;
using System.Collections.Generic;
using System.Linq;
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
            LedgerMenuPatches.BeforeStartInit += AddFoodDisplay;
            BuildingPhaseResourceReqDisplayPatches.TryOverrideGetBuildingResource = TryGetBuildingResource;
            var lastValue = Enum.GetValues(typeof(BuildingPhaseResourceReqDisplay.ResourceRequirement))
                .Cast<BuildingPhaseResourceReqDisplay.ResourceRequirement>()
                .Max();
            _foodResourceRequirement = lastValue + 1;

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
    }
}
