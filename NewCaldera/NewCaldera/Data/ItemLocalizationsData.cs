using ModifAmorphic.Outward.Extensions;
using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.Modules.Localization;
using ModifAmorphic.Outward.NewCaldera.BuildingsMod.Models;
using ModifAmorphic.Outward.NewCaldera.Models;
using ModifAmorphic.Outward.NewCaldera.Settings;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ModifAmorphic.Outward.NewCaldera.Data
{
    internal class BlueprintLocalizations
    {
        public string ProductionHeader { get; set; } = "Produces (per day)";
        public string FoodProduction { get; set; } = "Food: {food_production}";
        public string FundsProduction { get; set; } = "Funds: {funds_production}";
        public string StoneProduction { get; set; } = "Stone: {stone_production}";
        public string TimberProduction { get; set; } = "Timber: {timber_production}";
        public string ConstructionHeader { get; set; } = "Construction requirements";
        public string FoodCost { get; set; } = "Food: {food_cost}";
        public string FundsCost { get; set; } = "Funds: {funds_cost}";
        public string StoneCost { get; set; } = "Stone: {stone_cost}";
        public string TimberCost { get; set; } = "Timber: {timber_cost}";
        public string UpkeepHeader { get; set; } = "Upkeep (per day)";
        public string FoodUpkeep { get; set; } = "Food: {food_upkeep}";
        public string FundsUpkeep { get; set; } = "Funds: {funds_upkeep}";
        public string StoneUpkeep { get; set; } = "Stone: {stone_upkeep}";
        public string TimberUpkeep { get; set; } = "Timber: {timber_upkeep}";
        public string ConstructionTime { get; set; } = "Construction:  {build_days} Days";
    }
    internal class ItemLocalizationsData : JsonDataService<ItemLocalizations>
    {
        
        private readonly LocalizationModule _localizations;

        public ItemLocalizationsData(LocalizationsDirectory directoryHandler, LocalizationModule localizations, Func<IModifLogger> getLogger) : base(directoryHandler, getLogger) =>
            _localizations = localizations;

        private string _overrideFileName;
        protected override string FileName
        {
            get
            {
                if (string.IsNullOrEmpty(_overrideFileName))
                    return "ItemDescriptions.json";
                else
                    return _overrideFileName;
            }
        }

        public bool TryGetLocalization(int itemID, out ItemLocalization localization) => GetData().Items.TryGetValue(itemID, out localization);
        public void RegisterLocalization(ItemLocalization localization) => RegisterLocalization(localization.ItemID, localization.Name, localization.Description);
        public void RegisterLocalization(int itemID, string name, string description) => _localizations.RegisterItemLocalization(itemID, name, description);

        public void DumpLocalizations()
        {
            var currentIndex = LocalizationManager.Instance.CurrentLanguageIndex;
            _overrideFileName = "DumpedItemDescriptions.json";
            for (int i = 0; i < LocalizationManager.Instance.KnownLanguages.Length; i++)
            {
                if (i != currentIndex)
                {
                    LocalizationManager.Instance.SetLanguage(i);
                    LocalizationManager.Instance.Apply();
                    ((LocalizationsDirectory)base._directoryHandler).CurrentLanguageOverride = LocalizationManager.Instance.KnownLanguages[i];
                }
                var locs = new ItemLocalizations();

                //var hunterLoc = new ItemLocalization()
                //{
                //    ItemID = BuildingDefaults.HuntingBuilding.Tiers[0].BlueprintItemID,
                //    Description = BuildingDefaults.HuntingBuilding.Tiers[0].BlueprintDescription,
                //};
                //var hunterGuildLoc = new ItemLocalization()
                //{
                //    ItemID = BuildingDefaults.HuntingBuilding.Tiers[1].BlueprintItemID,
                //    Description = BuildingDefaults.HuntingBuilding.Tiers[1].BlueprintDescription,
                //};
                //var hunterHallLoc = new ItemLocalization()
                //{
                //    ItemID = BuildingDefaults.HuntingBuilding.Tiers[2].BlueprintItemID,
                //    Description = BuildingDefaults.HuntingBuilding.Tiers[2].BlueprintDescription,
                //};

                //locs.Items.Add(hunterLoc.ItemID, hunterLoc);
                //locs.Items.Add(hunterGuildLoc.ItemID, hunterGuildLoc);
                //locs.Items.Add(hunterHallLoc.ItemID, hunterHallLoc);

                var prefabs = ResourcesPrefabManagerExtensions.GetItemPrefabs();
                List<Blueprint> blueprints = new List<Blueprint>();
                foreach(var prefab in prefabs.Values)
                {
                    if (prefab is Blueprint bp)
                    {
                        blueprints.Add(bp);
                    }
                }

                //foreach (var tier in BuildingDefaults.HuntingBuilding.Tiers)
                //{
                //    locs.Items.Add(tier.BlueprintItemID, GetItemLocalization(tier));
                //}

                //foreach (var tier in BuildingDefaults.MasonBuilding.Tiers)
                //{
                //    locs.Items.Add(tier.BlueprintItemID, GetItemLocalization(tier));
                //}

                //foreach (var tier in BuildingDefaults.WoodcuttersBuilding.Tiers)
                //{
                //    locs.Items.Add(tier.BlueprintItemID, GetItemLocalization(tier));
                //}

                foreach(var blueprint in blueprints)
                {
                    locs.Items.Add(blueprint.ItemID, 
                        new ItemLocalization() 
                        { 
                            ItemID = blueprint.ItemID, 
                            Name = LocalizationManager.Instance.GetItemName(blueprint.ItemID),
                            Description = LocalizationManager.Instance.GetItemDesc(blueprint.ItemID) }
                        );
                }

                //locs.Items.Add(9400120, new ItemLocalization() { ItemID = 9400120, Description = LocalizationManager.Instance.GetItemDesc(9400120) });
                //locs.Items.Add(9400122, new ItemLocalization() { ItemID = 9400122, Description = LocalizationManager.Instance.GetItemDesc(9400122) });
                //locs.Items.Add(9400123, new ItemLocalization() { ItemID = 9400123, Description = LocalizationManager.Instance.GetItemDesc(9400123) });

                SaveNew(locs);
                //stop at english
                break;
            }
            _overrideFileName = string.Empty;
            if (LocalizationManager.Instance.CurrentLanguageIndex != currentIndex)
            {
                LocalizationManager.Instance.SetLanguage(currentIndex);
                LocalizationManager.Instance.Apply();
            }

        }

        private ItemLocalization GetItemLocalization(BuildingTier tier)
        {
            //var bp = ResourcesPrefabManager.Instance.GetItemPrefab(tier.BlueprintItemID) as Blueprint;

            return new ItemLocalization()
            {
                ItemID = tier.BlueprintItemID,
                Description = LocalizationManager.Instance.GetItemDesc(tier.BlueprintItemID)
            };
        }
    }
}
