using ModifAmorphic.Outward.UnityScripts.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace ModifAmorphic.Outward.UnityScripts
{
    [Serializable]
    public class BuildingRequirementBinder : IBoundTypeConverter
    {
        public int ReqBuildingItemID;
        public int UpgradeIndex = -1;

        public static Type GetBindingType() => OutwardAssembly.Types.BuildingRequirement;

        public Type GetBoundType() => GetBindingType();

        public object ToBoundType()
        {
            var type = GetBoundType();
            var buildingRequirement = Activator.CreateInstance(type);

            buildingRequirement.SetField(type, nameof(UpgradeIndex), UpgradeIndex);
            var building = ModifScriptsManager.Instance.PrefabManager.GetItemPrefab(ReqBuildingItemID);
            //var itemType = OutwardAssembly.GetType("Building");
            buildingRequirement.SetField(type, "ReqBuilding", building);

            return buildingRequirement;
        }
    }
}
