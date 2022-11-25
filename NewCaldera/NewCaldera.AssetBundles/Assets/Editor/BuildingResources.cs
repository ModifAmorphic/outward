using ModifAmorphic.Outward.UnityScripts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class BuildingResources
{
    public BuildingResourceValuesBinder BuildingCosts { get; set; } = new BuildingResourceValuesBinder();
    public BuildingResourceValuesBinder UpkeepCosts { get; set; } = new BuildingResourceValuesBinder();
    public BuildingResourceValuesBinder ProductionAmounts { get; set; } = new BuildingResourceValuesBinder();
    public BuildingResourceValuesBinder CapacityIncrease { get; set; } = new BuildingResourceValuesBinder();
    public int HousingIncrease { get; set; }
    public List<ItemQuantityBinder> ItemCosts { get; set; } = new List<ItemQuantityBinder>();
    public int BuildDays { get; set; }

}
