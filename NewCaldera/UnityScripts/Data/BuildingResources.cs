using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace ModifAmorphic.Outward.UnityScripts.Data
{
    internal class BuildingResources
    {
        public BuildingResourceValuesBinder BuildingCosts { get; set; } = new BuildingResourceValuesBinder();
        public BuildingResourceValuesBinder UpkeepCosts { get; set; } = new BuildingResourceValuesBinder();
        public BuildingResourceValuesBinder ProductionAmounts { get; set; } = new BuildingResourceValuesBinder();
        public BuildingResourceValuesBinder CapacityIncrease { get; set; } = new BuildingResourceValuesBinder();
        public int HousingIncrease { get; set; }
        public List<MonoBehaviour> ItemCosts { get; set; } = new List<MonoBehaviour>();
        public int BuildDays { get; set; }
    }
}
