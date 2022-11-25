using ModifAmorphic.Outward.NewCaldera.AiMod.Models.Conditions;
using ModifAmorphic.Outward.NewCaldera.AiMod.Models.Probablity;
using System;
using System.Collections.Generic;
using System.Text;

namespace ModifAmorphic.Outward.NewCaldera.AiMod.StaticTables
{
    internal static class ChanceTables
    {
        public static ConditionalChanceTable UnidentifiedSamplesTable1 = new ConditionalChanceTable()
        {
            Name = nameof(UnidentifiedSamplesTable1),
            WeightedItemTables = new Dictionary<string, decimal>()
            {
                { ItemChances.MolepigDrop1.Name, .33333M },
                { ItemChances.OreSampleDrop1.Name, .33333M },
                { ItemChances.PlantSampleDrop1.Name, .33334M }
            },
            MaxItemTables = 1,
            MinItemTables = 1,
            QuantityChance = 1,
            DropConditions = new List<Condition> { new NotFirstDeathCondition() }
        };

        public static ConditionalChanceTable UniqueGemsTable1 = new ConditionalChanceTable()
        {
            Name = nameof(UniqueGemsTable1),
            WeightedItemTables = new Dictionary<string, decimal>()
            {
                { ItemChances.TinyAquamarineDrop1.Name, .30M },
                { ItemChances.SmallSaphireDrop1.Name, .30M },
                { ItemChances.MediumRubyDrop1.Name, .2M },
                { ItemChances.LargeEmeraldDrop1.Name, .2M },
            },
            MaxItemTables = 2,
            MinItemTables = 1,
            QuantityChance = .25M,
            DropConditions = new List<Condition> { new NotFirstDeathCondition() }
        };

        public static ConditionalChanceTable UniqueEnchantTable1 = new ConditionalChanceTable()
        {
            Name = nameof(UniqueEnchantTable1),
            WeightedItemTables = new Dictionary<string, decimal>()
            {
                { ItemChances.TourmalineDrop1.Name, .70M },
                { ItemChances.PurifyingQuartzDrop1.Name, .30M },
            },
            MaxItemTables = 2,
            MinItemTables = 1,
            QuantityChance = .25M,
            DropConditions = new List<Condition>
            {
                new NotFirstDeathCondition(),
                new UniquesKilledCondition()
                {
                    NumericConditions = new Dictionary<string, decimal>()
                    {
                        {UniquesKilledCondition.UniquesKilledKey, 5 }
                    }
                }
            }
        };

        public static ConditionalChanceTable AfterDeathElementalDecayTable1 = new ConditionalChanceTable()
        {
            Name = nameof(AfterDeathElementalDecayTable1),
            WeightedItemTables = new Dictionary<string, decimal>()
            {
                { ItemChances.ElementalDecayParticleDrop1.Name, 1 },
            },
            MaxItemTables = 1,
            MinItemTables = 1,
            QuantityChance = 1,
            DropConditions = new List<Condition> { new NotFirstDeathCondition() }
        };

        public static ConditionalChanceTable AfterDeathElementalEtherTable1 = new ConditionalChanceTable()
        {
            Name = nameof(AfterDeathElementalEtherTable1),
            WeightedItemTables = new Dictionary<string, decimal>()
            {
                { ItemChances.ElementalEtherParticleDrop1.Name, 1 },
            },
            MaxItemTables = 1,
            MinItemTables = 1,
            QuantityChance = 1,
            DropConditions = new List<Condition> { new NotFirstDeathCondition() }
        };

        public static ConditionalChanceTable AfterDeathElementalFireTable1 = new ConditionalChanceTable()
        {
            Name = nameof(AfterDeathElementalFireTable1),
            WeightedItemTables = new Dictionary<string, decimal>()
            {
                { ItemChances.ElementalFireParticleDrop1.Name, 1 },
            },
            MaxItemTables = 1,
            MinItemTables = 1,
            QuantityChance = 1,
            DropConditions = new List<Condition> { new NotFirstDeathCondition() }
        };

        public static ConditionalChanceTable AfterDeathElementalIceTable1 = new ConditionalChanceTable()
        {
            Name = nameof(AfterDeathElementalIceTable1),
            WeightedItemTables = new Dictionary<string, decimal>()
            {
                { ItemChances.ElementalIceParticleDrop1.Name, 1 },
            },
            MaxItemTables = 1,
            MinItemTables = 1,
            QuantityChance = 1,
            DropConditions = new List<Condition> { new NotFirstDeathCondition() }
        };
        public static ConditionalChanceTable AfterDeathElementalLightIceTable1 = new ConditionalChanceTable()
        {
            Name = nameof(AfterDeathElementalLightIceTable1),
            WeightedItemTables = new Dictionary<string, decimal>()
            {
                { ItemChances.ElementalIceParticleDrop1.Name, .5M },
                { ItemChances.ElementalLightParticleDrop1.Name, .5M },
            },
            MaxItemTables = 1,
            MinItemTables = 1,
            QuantityChance = 1,
            DropConditions = new List<Condition> { new NotFirstDeathCondition() }
        };

        public static ConditionalChanceTable AfterDeathElementalLightTable1 = new ConditionalChanceTable()
        {
            Name = nameof(AfterDeathElementalLightTable1),
            WeightedItemTables = new Dictionary<string, decimal>()
            {
                { ItemChances.ElementalDecayParticleDrop1.Name, 1 },
                { ItemChances.ElementalEtherParticleDrop1.Name, 1 },
                { ItemChances.ElementalFireParticleDrop1.Name, 1 },
                { ItemChances.ElementalIceParticleDrop1.Name, 1 },
                { ItemChances.ElementalLightParticleDrop1.Name, 1 },
            },
            MaxItemTables = 1,
            MinItemTables = 1,
            QuantityChance = 1,
            DropConditions = new List<Condition> { new NotFirstDeathCondition() }
        };

        public static ConditionalChanceTable AfterDeathElementalRandomTable1 = new ConditionalChanceTable()
        {
            Name = nameof(AfterDeathElementalRandomTable1),
            WeightedItemTables = new Dictionary<string, decimal>()
            {
                { ItemChances.ElementalLightParticleDrop1.Name, 1 },
            },
            MaxItemTables = 1,
            MinItemTables = 1,
            QuantityChance = 1,
            DropConditions = new List<Condition> { new NotFirstDeathCondition() }
        };
        
        public static ConditionalChanceTable BossRewardsCalderaUniquesTable1 = new ConditionalChanceTable()
        {
            Name = nameof(BossRewardsCalderaUniquesTable1),
            WeightedItemTables = new Dictionary<string, decimal>()
            {
                { ItemChances.BossRewardGepGenerosity1.Name, .1M },
                { ItemChances.BossRewardPearlBirdCourage1.Name, .1M },
                { ItemChances.BossRewardElattRelic1.Name, .1M },
                { ItemChances.BossRewardScourgeTears1.Name, .1M },
                { ItemChances.BossRewardHauntedMemory1.Name, .1M },
                { ItemChances.BossRewardCalixaRelic1.Name, .1M },
                { ItemChances.BossRewardLeylineFigment1.Name, .1M },
                { ItemChances.BossRewardVendavelHospitality1.Name, .1M },
                { ItemChances.BossRewardFloweringCorruption1.Name, .1M },
                { ItemChances.BossRewardEnchantedMask1.Name, .1M },
                { ItemChances.BossRewardMetalizedBones1.Name, .1M },
                { ItemChances.BossRewardScarletWhisper1.Name, .1M },
                { ItemChances.BossRewardNobleGreed1.Name, .1M },
                { ItemChances.BossRewardCalygreyWisdom1.Name, .1M },
            },
            MaxItemTables = 1,
            MinItemTables = 1,
            QuantityChance = 1,
            DropConditions = new List<Condition> 
            {
                new NotFirstDeathCondition(),
                new UniquesKilledCondition()
                {
                    NumericConditions = new Dictionary<string, decimal>()
                    {
                        {UniquesKilledCondition.UniquesKilledKey, 6 }
                    }
                },
                new PlayerDeathsCondition()
                {
                    NumericConditions = new Dictionary<string, decimal>()
                    {
                        {PlayerDeathsCondition.PlayerDeathsKey, 0 }
                    }
                }
            }
        };

        public static ConditionalChanceTable BossRewardsCalderaUniquesTable2 = new ConditionalChanceTable()
        {
            Name = nameof(BossRewardsCalderaUniquesTable2),
            WeightedItemTables = new Dictionary<string, decimal>()
            {
                { ItemChances.BossRewardGepGenerosity1.Name, .1M },
                { ItemChances.BossRewardPearlBirdCourage1.Name, .1M },
                { ItemChances.BossRewardElattRelic1.Name, .1M },
                { ItemChances.BossRewardScourgeTears1.Name, .1M },
                { ItemChances.BossRewardHauntedMemory1.Name, .1M },
                { ItemChances.BossRewardCalixaRelic1.Name, .1M },
                { ItemChances.BossRewardLeylineFigment1.Name, .1M },
                { ItemChances.BossRewardVendavelHospitality1.Name, .1M },
                { ItemChances.BossRewardFloweringCorruption1.Name, .1M },
                { ItemChances.BossRewardEnchantedMask1.Name, .1M },
                { ItemChances.BossRewardMetalizedBones1.Name, .1M },
                { ItemChances.BossRewardScarletWhisper1.Name, .1M },
                { ItemChances.BossRewardNobleGreed1.Name, .1M },
                { ItemChances.BossRewardCalygreyWisdom1.Name, .1M },
            },
            MaxItemTables = 1,
            MinItemTables = 1,
            QuantityChance = 1,
            DropConditions = new List<Condition>
            {
                new NotFirstDeathCondition(),
                new UniquesKilledCondition()
                {
                    NumericConditions = new Dictionary<string, decimal>()
                    {
                        {UniquesKilledCondition.UniquesKilledKey, 7 }
                    }
                }
            }
        };

        public static ItemChanceTables ItemTables = new ItemChanceTables()
        {
            ChanceTables = new Dictionary<string, ConditionalChanceTable>()
            {
                { AfterDeathElementalDecayTable1.Name, AfterDeathElementalDecayTable1 },
                { AfterDeathElementalEtherTable1.Name, AfterDeathElementalEtherTable1 },
                { AfterDeathElementalFireTable1.Name, AfterDeathElementalFireTable1 },
                { AfterDeathElementalIceTable1.Name, AfterDeathElementalIceTable1 },
                { AfterDeathElementalLightTable1.Name, AfterDeathElementalLightTable1 },
                { AfterDeathElementalLightIceTable1.Name, AfterDeathElementalLightIceTable1 },
                { AfterDeathElementalRandomTable1.Name, AfterDeathElementalRandomTable1 },
                { UnidentifiedSamplesTable1.Name, UnidentifiedSamplesTable1 },
                { UniqueGemsTable1.Name, UniqueGemsTable1 },
                { BossRewardsCalderaUniquesTable1.Name, BossRewardsCalderaUniquesTable1 },
                { BossRewardsCalderaUniquesTable2.Name, BossRewardsCalderaUniquesTable2 },
            },
            ItemQuantityChances = new Dictionary<string, ItemQuantityChance>()
            {
                { ItemChances.MolepigDrop1.Name, ItemChances.MolepigDrop1 },
                { ItemChances.OreSampleDrop1.Name, ItemChances.OreSampleDrop1 },
                { ItemChances.PlantSampleDrop1.Name, ItemChances.PlantSampleDrop1 },
                { ItemChances.LargeEmeraldDrop1.Name, ItemChances.LargeEmeraldDrop1},
                { ItemChances.MediumRubyDrop1.Name, ItemChances.MediumRubyDrop1 },
                { ItemChances.PurifyingQuartzDrop1.Name, ItemChances.PurifyingQuartzDrop1 },
                { ItemChances.SmallSaphireDrop1.Name, ItemChances.SmallSaphireDrop1 },
                { ItemChances.TinyAquamarineDrop1.Name, ItemChances.TinyAquamarineDrop1 },
                { ItemChances.TourmalineDrop1.Name, ItemChances.TourmalineDrop1 },
                { ItemChances.ElementalDecayParticleDrop1.Name, ItemChances.ElementalDecayParticleDrop1 },
                { ItemChances.ElementalEtherParticleDrop1.Name, ItemChances.ElementalEtherParticleDrop1 },
                { ItemChances.ElementalFireParticleDrop1.Name, ItemChances.ElementalFireParticleDrop1 },
                { ItemChances.ElementalIceParticleDrop1.Name, ItemChances.ElementalIceParticleDrop1 },
                { ItemChances.ElementalLightParticleDrop1.Name, ItemChances.ElementalLightParticleDrop1 },
                { ItemChances.BossRewardGepGenerosity1.Name, ItemChances.BossRewardGepGenerosity1 },
                { ItemChances.BossRewardPearlBirdCourage1.Name, ItemChances.BossRewardPearlBirdCourage1 },
                { ItemChances.BossRewardElattRelic1.Name, ItemChances.BossRewardElattRelic1 },
                { ItemChances.BossRewardScourgeTears1.Name, ItemChances.BossRewardScourgeTears1 },
                { ItemChances.BossRewardHauntedMemory1.Name, ItemChances.BossRewardHauntedMemory1 },
                { ItemChances.BossRewardCalixaRelic1.Name, ItemChances.BossRewardCalixaRelic1 },
                { ItemChances.BossRewardLeylineFigment1.Name, ItemChances.BossRewardLeylineFigment1 },
                { ItemChances.BossRewardVendavelHospitality1.Name, ItemChances.BossRewardVendavelHospitality1 },
                { ItemChances.BossRewardFloweringCorruption1.Name, ItemChances.BossRewardFloweringCorruption1 },
                { ItemChances.BossRewardEnchantedMask1.Name, ItemChances.BossRewardEnchantedMask1 },
                { ItemChances.BossRewardMetalizedBones1.Name, ItemChances.BossRewardMetalizedBones1 },
                { ItemChances.BossRewardScarletWhisper1.Name, ItemChances.BossRewardScarletWhisper1 },
                { ItemChances.BossRewardNobleGreed1.Name, ItemChances.BossRewardNobleGreed1 },
                { ItemChances.BossRewardCalygreyWisdom1.Name, ItemChances.BossRewardCalygreyWisdom1 },
            }
        };
    }
}
