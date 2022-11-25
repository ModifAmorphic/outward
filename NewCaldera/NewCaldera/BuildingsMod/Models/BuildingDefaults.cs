using System.Collections.Generic;

namespace ModifAmorphic.Outward.NewCaldera.BuildingsMod.Models
{
    internal class BuildingDefaults
    {
        public static BuildingBlueprint HouseA_Blueprint = new BuildingBlueprint()
        {
            BuildingItemID = 9400011,
            Name = "House A",
            BuildingType = Building.BuildingTypes.ResourceProducing,
            Steps = new List<BuildingStep>()
            {
                new BuildingStep() { BlueprintItemID = 9400010, BuildDays = 4, Tier = 1, Step = 1, BuildAmounts = new ResourceAmounts()
                    { FundsAmount = 600, FoodAmount = 10, StoneAmount = 20, TimberAmount = 20}},
                new BuildingStep() { BlueprintItemID = 9400010, BuildDays = 3, Tier = 1, Step = 2, BuildAmounts = new ResourceAmounts()
                    { FundsAmount = 400, FoodAmount = 10, StoneAmount = 20, TimberAmount = 20}},
            }
        };

        public static BuildingBlueprint HouseB_Blueprint = new BuildingBlueprint()
        {
            BuildingItemID = 9400021,
            Name = "House B",
            BuildingType = Building.BuildingTypes.ResourceProducing,
            Steps = new List<BuildingStep>()
            {
                new BuildingStep() { BlueprintItemID = 9400020, BuildDays = 4, Tier = 1, Step = 1, BuildAmounts = new ResourceAmounts()
                    { FundsAmount = 600, FoodAmount = 10, StoneAmount = 20, TimberAmount = 20}},
                new BuildingStep() { BlueprintItemID = 9400020, BuildDays = 3, Tier = 1, Step = 2, BuildAmounts = new ResourceAmounts()
                    { FundsAmount = 400, FoodAmount = 10, StoneAmount = 20, TimberAmount = 20}},
            }
        };

        public static BuildingBlueprint HouseC_Blueprint = new BuildingBlueprint()
        {
            BuildingItemID = 9400031,
            Name = "House C",
            BuildingType = Building.BuildingTypes.ResourceProducing,
            Steps = new List<BuildingStep>()
            {
                new BuildingStep() { BlueprintItemID = 9400030, BuildDays = 4, Tier = 1, Step = 1, BuildAmounts = new ResourceAmounts()
                    { FundsAmount = 600, FoodAmount = 10, StoneAmount = 20, TimberAmount = 20}},
                new BuildingStep() { BlueprintItemID = 9400030, BuildDays = 3, Tier = 1, Step = 2, BuildAmounts = new ResourceAmounts()
                    { FundsAmount = 400, FoodAmount = 10, StoneAmount = 20, TimberAmount = 20}},
            }
        };

        public static BuildingBlueprint HuntingLodgeBlueprint = new BuildingBlueprint()
        {
            BuildingItemID = 9400131,
            Name = "Hunting Lodge",
            BuildingType = Building.BuildingTypes.ResourceProducing,
            Steps = new List<BuildingStep>()
            {
                new BuildingStep() { BlueprintItemID = 9400130, BuildDays = 4, Tier = 1, Step = 1, BuildAmounts = new ResourceAmounts()
                    { FundsAmount = 675, FoodAmount = 10, StoneAmount = 5, TimberAmount = 5, Housing = 1}},
                new BuildingStep() { BlueprintItemID = 9400130, BuildDays = 4, Tier = 1, Step = 2, BuildAmounts = new ResourceAmounts()
                    { FundsAmount = 675, FoodAmount = 10, StoneAmount = 5, TimberAmount = 5, Housing = 1}},
                new BuildingStep() { BlueprintItemID = 9400132, BuildDays = 4, Tier = 2, Step = 3, BuildAmounts = new ResourceAmounts()
                    { FundsAmount = 2500, FoodAmount = 25, StoneAmount = 10, TimberAmount = 10, Housing = 2}},
                new BuildingStep() { BlueprintItemID = 9400133, BuildDays = 4, Tier = 3, Step = 4, BuildAmounts = new ResourceAmounts()
                    { FundsAmount = 2500, FoodAmount = 30, StoneAmount = 10, TimberAmount = 10, Housing = 4}},
            }
        };

        public static BuildingBlueprint MasonsBlueprint = new BuildingBlueprint()
        {
            BuildingItemID = 9400141,
            Name = "Mason Workshop",
            BuildingType = Building.BuildingTypes.ResourceProducing,
            Steps = new List<BuildingStep>()
            {
                new BuildingStep() { BlueprintItemID = 9400140, BuildDays = 4, Tier = 1, Step = 1, BuildAmounts = new ResourceAmounts()
                    { FundsAmount = 500, FoodAmount = 15, StoneAmount = 10, TimberAmount = 5, Housing = 1}},
                new BuildingStep() { BlueprintItemID = 9400140, BuildDays = 4, Tier = 1, Step = 2, BuildAmounts = new ResourceAmounts()
                    { FundsAmount = 500, FoodAmount = 15, StoneAmount = 10, TimberAmount = 5, Housing = 1}},
                new BuildingStep() { BlueprintItemID = 9400142, BuildDays = 4, Tier = 2, Step = 3, BuildAmounts = new ResourceAmounts()
                    { FundsAmount = 500, FoodAmount = 30, StoneAmount = 25, TimberAmount = 15, Housing = 2}},
                new BuildingStep() { BlueprintItemID = 9400143, BuildDays = 4, Tier = 3, Step = 4, BuildAmounts = new ResourceAmounts()
                    { FundsAmount = 500, FoodAmount = 30, StoneAmount = 30, TimberAmount = 20, Housing = 4}},
            }
        };

        public static BuildingBlueprint WoodcuttersBlueprint = new BuildingBlueprint()
        {
            BuildingItemID = 9400151,
            Name = "Woodcutter Lodge",
            BuildingType = Building.BuildingTypes.ResourceProducing,
            Steps = new List<BuildingStep>()
            {
                new BuildingStep() { BlueprintItemID = 9400150, BuildDays = 4, Tier = 1, Step = 1, BuildAmounts = new ResourceAmounts()
                    { FundsAmount = 500, FoodAmount = 15, StoneAmount = 5, TimberAmount = 10, Housing = 1}},
                new BuildingStep() { BlueprintItemID = 9400150, BuildDays = 4, Tier = 1, Step = 2, BuildAmounts = new ResourceAmounts()
                    { FundsAmount = 500, FoodAmount = 15, StoneAmount = 5, TimberAmount = 10, Housing = 1}},
                new BuildingStep() { BlueprintItemID = 9400152, BuildDays = 4, Tier = 2, Step = 3, BuildAmounts = new ResourceAmounts()
                    { FundsAmount = 500, FoodAmount = 30, StoneAmount = 15, TimberAmount = 25, Housing = 2}},
                new BuildingStep() { BlueprintItemID = 9400153, BuildDays = 4, Tier = 3, Step = 4, BuildAmounts = new ResourceAmounts()
                    { FundsAmount = 500, FoodAmount = 30, StoneAmount = 20, TimberAmount = 30, Housing = 4}},
            }
        };

        public static BuildingBlueprint CityHallBlueprint = new BuildingBlueprint()
        {
            BuildingItemID = 9400121,
            Name = "City Hall",
            BuildingType = Building.BuildingTypes.Specialized,
            Steps = new List<BuildingStep>()
            {
                new BuildingStep() { BlueprintItemID = 9400120, BuildDays = 7, Tier = 1, Step = 1},
                new BuildingStep() { BlueprintItemID = 9400120, BuildDays = 7, Tier = 1, Step = 2},
                new BuildingStep() { BlueprintItemID = 9400122, BuildDays = 10, Tier = 2, Step = 3},
                new BuildingStep() { BlueprintItemID = 9400123, BuildDays = 10, Tier = 3, Step = 4},
            }
        };

        public static BuildingBlueprint BlacksmithBlueprint = new BuildingBlueprint()
        {
            BuildingItemID = 9400161,
            Name = "Blacksmith shop",
            BuildingType = Building.BuildingTypes.Specialized,
            Steps = new List<BuildingStep>()
            {
                new BuildingStep() { BlueprintItemID = 9400160, BuildDays = 7, Tier = 1, Step = 1, BuildAmounts = new ResourceAmounts()
                    { FundsAmount = 1000, FoodAmount = 15, StoneAmount = 60, TimberAmount = 30, Housing = 3}},
                new BuildingStep() { BlueprintItemID = 9400160, BuildDays = 7, Tier = 1, Step = 2, BuildAmounts = new ResourceAmounts()
                    { FundsAmount = 1000, FoodAmount = 15, StoneAmount = 60, TimberAmount = 30, Housing = 3}},
                new BuildingStep() { BlueprintItemID = 9400162, BuildDays = 10, Tier = 2, Step = 3, BuildAmounts = new ResourceAmounts()
                    { FundsAmount = 4200, FoodAmount = 25, StoneAmount = 200, TimberAmount = 100, Housing = 3}},
                new BuildingStep() { BlueprintItemID = 9400163, BuildDays = 10, Tier = 3, Step = 4, BuildAmounts = new ResourceAmounts()
                    { FundsAmount = 4200, FoodAmount = 25, StoneAmount = 200, TimberAmount = 100, Housing = 3}},
            }
        };

        public static BuildingBlueprint AlchemistBlueprint = new BuildingBlueprint()
        {
            BuildingItemID = 9400171,
            Name = "Alchemist shop",
            BuildingType = Building.BuildingTypes.Specialized,
            Steps = new List<BuildingStep>()
            {
                new BuildingStep() { BlueprintItemID = 9400170, BuildDays = 7, Tier = 1, Step = 1, BuildAmounts = new ResourceAmounts()
                    { FundsAmount = 1000, FoodAmount = 15, StoneAmount = 30, TimberAmount = 60, Housing = 3}},
                new BuildingStep() { BlueprintItemID = 9400170, BuildDays = 7, Tier = 1, Step = 2, BuildAmounts = new ResourceAmounts()
                    { FundsAmount = 1000, FoodAmount = 15, StoneAmount = 30, TimberAmount = 60, Housing = 3}},
                new BuildingStep() { BlueprintItemID = 9400172, BuildDays = 10, Tier = 2, Step = 3, BuildAmounts = new ResourceAmounts()
                    { FundsAmount = 4200, FoodAmount = 25, StoneAmount = 100, TimberAmount = 200, Housing = 3}},
                new BuildingStep() { BlueprintItemID = 9400173, BuildDays = 10, Tier = 3, Step = 4, BuildAmounts = new ResourceAmounts()
                    { FundsAmount = 5600, FoodAmount = 25, StoneAmount = 100, TimberAmount = 200, Housing = 3}},
            }
        };

        public static BuildingBlueprint EnchantingGuildBlueprint = new BuildingBlueprint()
        {
            BuildingItemID = 9400181,
            Name = "Enchanting Guild",
            BuildingType = Building.BuildingTypes.Specialized,
            Steps = new List<BuildingStep>()
            {
                new BuildingStep() { BlueprintItemID = 9400180, BuildDays = 7, Tier = 1, Step = 1, BuildAmounts = new ResourceAmounts()
                    { FundsAmount = 1750, FoodAmount = 15, StoneAmount = 80, TimberAmount = 50, Housing = 4}},
                new BuildingStep() { BlueprintItemID = 9400180, BuildDays = 7, Tier = 1, Step = 2, BuildAmounts = new ResourceAmounts()
                    { FundsAmount = 1750, FoodAmount = 15, StoneAmount = 80, TimberAmount = 50, Housing = 4}},
                new BuildingStep() { BlueprintItemID = 9400182, BuildDays = 14, Tier = 2, Step = 3, BuildAmounts = new ResourceAmounts()
                    { FundsAmount = 4200, FoodAmount = 25, StoneAmount = 100, TimberAmount = 200, Housing = 4}},
                new BuildingStep() { BlueprintItemID = 9400183, BuildDays = 14, Tier = 3, Step = 4, BuildAmounts = new ResourceAmounts()
                    { FundsAmount = 5600, FoodAmount = 25, StoneAmount = 200, TimberAmount = 100, Housing = 4}},
            }
        };

        public static BuildingBlueprint FoodStoreBlueprint = new BuildingBlueprint()
        {
            BuildingItemID = 9400191,
            Name = "Food Store",
            BuildingType = Building.BuildingTypes.Specialized,
            Steps = new List<BuildingStep>()
            {
                new BuildingStep() { BlueprintItemID = 9400190, BuildDays = 7, Tier = 1, Step = 1, BuildAmounts = new ResourceAmounts()
                    { FundsAmount = 1000, StoneAmount = 40, TimberAmount = 40, Housing = 3}},
                new BuildingStep() { BlueprintItemID = 9400190, BuildDays = 7, Tier = 1, Step = 2, BuildAmounts = new ResourceAmounts()
                    { FundsAmount = 1000, StoneAmount = 40, TimberAmount = 40, Housing = 3}},
                new BuildingStep() { BlueprintItemID = 9400192, BuildDays = 10, Tier = 2, Step = 3, BuildAmounts = new ResourceAmounts()
                    { FundsAmount = 4200, StoneAmount = 160, TimberAmount = 200, Housing = 3}},
                new BuildingStep() { BlueprintItemID = 9400193, BuildDays = 10, Tier = 3, Step = 4, BuildAmounts = new ResourceAmounts()
                    { FundsAmount = 3150, StoneAmount = 120, TimberAmount = 300, Housing = 3}},
            }
        };

        public static BuildingBlueprint GladiatorsArenaBlueprint = new BuildingBlueprint()
        {
            BuildingItemID = 9400201,
            Name = "Gladiator’s Arena",
            BuildingType = Building.BuildingTypes.Specialized,
            Steps = new List<BuildingStep>()
            {
                new BuildingStep() { BlueprintItemID = 9400200, BuildDays = 7, Tier = 1, Step = 1, BuildAmounts = new ResourceAmounts()
                    { FundsAmount = 1750, FoodAmount = 15, StoneAmount = 60, TimberAmount = 30, Housing = 4}},
                new BuildingStep() { BlueprintItemID = 9400200, BuildDays = 7, Tier = 1, Step = 2, BuildAmounts = new ResourceAmounts()
                    { FundsAmount = 1750, FoodAmount = 15, StoneAmount = 60, TimberAmount = 30, Housing = 4}},
                new BuildingStep() { BlueprintItemID = 9400202, BuildDays = 10, Tier = 2, Step = 3, BuildAmounts = new ResourceAmounts()
                    { FundsAmount = 6300, FoodAmount = 25, StoneAmount = 100, TimberAmount = 200, Housing = 5}},
                new BuildingStep() { BlueprintItemID = 9400203, BuildDays = 10, Tier = 3, Step = 4, BuildAmounts = new ResourceAmounts()
                    { FundsAmount = 6300, FoodAmount = 25, StoneAmount = 150, TimberAmount = 150, Housing = 5}},
            }
        };

        public static BuildingBlueprint ChapelBlueprint = new BuildingBlueprint()
        {
            BuildingItemID = 9400211,
            Name = "Chapel",
            BuildingType = Building.BuildingTypes.Specialized,
            Steps = new List<BuildingStep>()
            {
                new BuildingStep() { BlueprintItemID = 9400210, BuildDays = 7, Tier = 1, Step = 1, BuildAmounts = new ResourceAmounts()
                    { FundsAmount = 1750, FoodAmount = 15, StoneAmount = 100, TimberAmount = 50, Housing = 4}},
                new BuildingStep() { BlueprintItemID = 9400210, BuildDays = 7, Tier = 1, Step = 2, BuildAmounts = new ResourceAmounts()
                    { FundsAmount = 1750, FoodAmount = 15, StoneAmount = 100, TimberAmount = 50, Housing = 4}},
                new BuildingStep() { BlueprintItemID = 9400212, BuildDays = 14, Tier = 2, Step = 3, BuildAmounts = new ResourceAmounts()
                    { FundsAmount = 4200, FoodAmount = 25, StoneAmount = 250, TimberAmount = 150, Housing = 4}},
                new BuildingStep() { BlueprintItemID = 9400213, BuildDays = 14, Tier = 3, Step = 4, BuildAmounts = new ResourceAmounts()
                    { FundsAmount = 5600, FoodAmount = 25, StoneAmount = 250, TimberAmount = 150, Housing = 4}},
            }
        };

        public static BuildingBlueprint GeneralStoreBlueprint = new BuildingBlueprint()
        {
            BuildingItemID = 9400221,
            Name = "General Store",
            BuildingType = Building.BuildingTypes.Specialized,
            Steps = new List<BuildingStep>()
            {
                new BuildingStep() { BlueprintItemID = 9400220, BuildDays = 7, Tier = 1, Step = 1, BuildAmounts = new ResourceAmounts()
                    { FundsAmount = 1000, FoodAmount = 15, StoneAmount = 40, TimberAmount = 40, Housing = 3}},
                new BuildingStep() { BlueprintItemID = 9400220, BuildDays = 7, Tier = 1, Step = 2, BuildAmounts = new ResourceAmounts()
                    { FundsAmount = 1000, FoodAmount = 15, StoneAmount = 40, TimberAmount = 40, Housing = 3}},
                new BuildingStep() { BlueprintItemID = 9400222, BuildDays = 14, Tier = 2, Step = 3, BuildAmounts = new ResourceAmounts()
                    { FundsAmount = 3500, FoodAmount = 25, StoneAmount = 100, TimberAmount = 100, Housing = 6}},
                new BuildingStep() { BlueprintItemID = 9400223, BuildDays = 14, Tier = 3, Step = 4, BuildAmounts = new ResourceAmounts()
                    { FundsAmount = 4200, FoodAmount = 25, StoneAmount = 100, TimberAmount = 100, Housing = 6}},
            }
        };

        public static BuildingBlueprint WaterPurifierBlueprint = new BuildingBlueprint()
        {
            BuildingItemID = 9400231,
            Name = "Water Purifier",
            BuildingType = Building.BuildingTypes.Specialized,
            Steps = new List<BuildingStep>()
            {
                new BuildingStep() { BlueprintItemID = 9400230, BuildDays = 7, Tier = 1, Step = 1, BuildAmounts = new ResourceAmounts()
                    { FundsAmount = 1400, FoodAmount = 15, StoneAmount = 55, TimberAmount = 35}},
                new BuildingStep() { BlueprintItemID = 9400230, BuildDays = 7, Tier = 1, Step = 2, BuildAmounts = new ResourceAmounts()
                    { FundsAmount = 1400, FoodAmount = 15, StoneAmount = 55, TimberAmount = 35}},
                new BuildingStep() { BlueprintItemID = 9400232, BuildDays = 10, Tier = 2, Step = 3, BuildAmounts = new ResourceAmounts()
                    { FundsAmount = 4200, FoodAmount = 25, StoneAmount = 150, TimberAmount = 150, Housing = 3}},
                new BuildingStep() { BlueprintItemID = 9400233, BuildDays = 10, Tier = 3, Step = 4, BuildAmounts = new ResourceAmounts()
                    { FundsAmount = 4200, FoodAmount = 25, StoneAmount = 100, TimberAmount = 200}},
            }
        };


        //const string hunterLodgeDesc = "A basic Hunter’s lodge.  Provides {food_production} Food for the settlement each day.\n" +
        //    "----\n" +
        //    "Construction requirements\n" +
        //    "Funds: {funds_cost}\n" +
        //    "Stone: {stone_cost}\n" +
        //    "Timber: {timber_cost}\n" +
        //    "Funds upkeep: {funds_upkeep} / Day\n" +
        //    "Construction:  {build_days} Days";
        //const string hunterGuildDesc = "An improvement to the Hunting lodge.  Provides {food_production} Food for the settlement each day.\n" +
        //    "----\n" +
        //    "Construction requirements\n" +
        //    "Funds: {funds_cost}\n" +
        //    "Stone: {stone_cost}\n" +
        //    "Timber: {timber_cost}\n" +
        //    "Funds upkeep: {funds_upkeep} / Day\n" +
        //    "Construction:  {build_days} Days";
        //const string hunterHallDesc = "An improvement to the Hunting Guild.  Provides {food_production} Food for the settlement each day.\n" +
        //    "----\n" +
        //    "Construction requirements\n" +
        //    "Funds: {funds_cost}\n" +
        //    "Stone: {stone_cost}\n" +
        //    "Timber: {timber_cost}\n" +
        //    "Funds upkeep: {funds_upkeep} / Day\n" +
        //    "Construction:  {build_days} Days";

        public static TieredBuilding HouseA_Building = new TieredBuilding()
        {
            BuildingItemID = 9400011,
            Name = "House A",
            Tiers = new List<BuildingTier> {
                new BuildingTier()
                {
                    Tier = 1,
                    BlueprintItemID = 9400010,
                    //BlueprintDescription = hunterLodgeDesc,
                    //ProductionAmounts =  new ResourceAmounts() { FoodAmount = 15, StoneAmount = 1, TimberAmount = 1 },
                    UpkeepAmounts = new ResourceAmounts() { FoodAmount = 5 },
                }
            }
        };

        public static TieredBuilding HuntingBuilding = new TieredBuilding()
        {
            BuildingItemID = 9400131,
            Name = "Hunting Lodge",
            Tiers = new List<BuildingTier> {
                new BuildingTier()
                {
                    Tier = 1,
                    BlueprintItemID = 9400130,
                    //BlueprintDescription = hunterLodgeDesc,
                    ProductionAmounts =  new ResourceAmounts() { FoodAmount = 24, StoneAmount = 1, TimberAmount = 1 },
                    UpkeepAmounts = new ResourceAmounts() { FundsAmount = 20 },
                    CapacityIncreases = new ResourceAmounts() { FoodAmount = 50 }
                },
                new BuildingTier()
                {
                    Tier = 2,
                    BlueprintItemID = 9400132,
                    //BlueprintDescription = hunterGuildDesc,
                    ProductionAmounts =  new ResourceAmounts() { FoodAmount = 40, StoneAmount = 2, TimberAmount = 2 },
                    UpkeepAmounts = new ResourceAmounts() { FundsAmount = 30 },
                    CapacityIncreases = new ResourceAmounts() { FoodAmount = 150 }
                },
                new BuildingTier()
                {
                    Tier = 3,
                    BlueprintItemID = 9400133,
                    //BlueprintDescription = hunterHallDesc,
                    ProductionAmounts =  new ResourceAmounts() { FoodAmount = 57, StoneAmount = 3, TimberAmount = 3 },
                    UpkeepAmounts = new ResourceAmounts() { FundsAmount = 40 },
                    CapacityIncreases = new ResourceAmounts() { FoodAmount = 300 }
                },
            }
        };

        public static TieredBuilding MasonBuilding = new TieredBuilding()
        {
            BuildingItemID = 9400141,
            Name = "Mason Workshop",
            Tiers = new List<BuildingTier> {
                new BuildingTier()
                {
                    Tier = 1,
                    BlueprintItemID = 9400140,
                    ProductionAmounts =  new ResourceAmounts() { StoneAmount = 4 },
                    UpkeepAmounts = new ResourceAmounts() { FundsAmount = 20, FoodAmount = 2 },
                    CapacityIncreases = new ResourceAmounts() { StoneAmount = 50 }
                },
                new BuildingTier()
                {
                    Tier = 2,
                    BlueprintItemID = 9400142,
                    ProductionAmounts =  new ResourceAmounts() { StoneAmount = 7 },
                    UpkeepAmounts = new ResourceAmounts() { FundsAmount = 30, FoodAmount = 3 },
                    CapacityIncreases = new ResourceAmounts() { StoneAmount = 150 }
                },
                new BuildingTier()
                {
                    Tier = 3,
                    BlueprintItemID = 9400143,
                    ProductionAmounts =  new ResourceAmounts() { StoneAmount = 10 },
                    UpkeepAmounts = new ResourceAmounts() { FundsAmount = 40, FoodAmount = 4 },
                    CapacityIncreases = new ResourceAmounts() { StoneAmount = 300 }
                },
            }
        };

        public static TieredBuilding WoodcuttersBuilding = new TieredBuilding()
        {
            BuildingItemID = 9400151,
            Name = "Woodcutter Lodge",
            Tiers = new List<BuildingTier> {
                new BuildingTier()
                {
                    Tier = 1,
                    BlueprintItemID = 9400150,
                    ProductionAmounts =  new ResourceAmounts() { TimberAmount = 4 },
                    UpkeepAmounts = new ResourceAmounts() { FundsAmount = 20, FoodAmount = 2 },
                    CapacityIncreases = new ResourceAmounts() { TimberAmount = 50 }
                },
                new BuildingTier()
                {
                    Tier = 2,
                    BlueprintItemID = 9400152,
                    ProductionAmounts =  new ResourceAmounts() { TimberAmount = 7 },
                    UpkeepAmounts = new ResourceAmounts() { FundsAmount = 30, FoodAmount = 3 },
                    CapacityIncreases = new ResourceAmounts() { TimberAmount = 150 }
                },
                new BuildingTier()
                {
                    Tier = 3,
                    BlueprintItemID = 9400153,
                    ProductionAmounts =  new ResourceAmounts() { TimberAmount = 10 },
                    UpkeepAmounts = new ResourceAmounts() { FundsAmount = 40, FoodAmount = 4 },
                    CapacityIncreases = new ResourceAmounts() { TimberAmount = 300 }
                },
            }
        };

    }
}
