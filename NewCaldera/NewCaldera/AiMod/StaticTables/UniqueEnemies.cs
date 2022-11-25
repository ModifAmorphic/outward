using ModifAmorphic.Outward.NewCaldera.AiMod.Models.Probablity;
using ModifAmorphic.Outward.NewCaldera.AiMod.Models.UniqueEnemies;
using System;
using System.Collections.Generic;
using System.Text;

namespace ModifAmorphic.Outward.NewCaldera.AiMod.StaticTables
{
    internal class UniqueEnemies
    {
        #region Caldera Uniques
        public static UniqueEnemy CalygreyHero = new UniqueEnemy()
        {
            Name = nameof(CalygreyHero),
            UID = "pMfhK69Stky7MvE9Ro0XMQ",
            SquadUID = "QUuBcx4IxEeYxGriJkG_sw",
            DropTableNames = new HashSet<string>()
            {
                ChanceTables.UnidentifiedSamplesTable1.Name,
                ChanceTables.UniqueGemsTable1.Name,
                ChanceTables.AfterDeathElementalLightIceTable1.Name,
                ChanceTables.UniqueEnchantTable1.Name,
                ChanceTables.BossRewardsCalderaUniquesTable1.Name,
                ChanceTables.BossRewardsCalderaUniquesTable2.Name,
            },
        };

        public static UniqueEnemy CrackedGargoyle = new UniqueEnemy()
        {
            Name = nameof(CrackedGargoyle),
            UID = "-McLNdZsNEa3itw-ny7YBw",
            QuestEventUID = "ptaqlce3I0qmBLRigMBNFQ",
            DropTableNames = new HashSet<string>()
            {
                ChanceTables.UnidentifiedSamplesTable1.Name,
                ChanceTables.UniqueGemsTable1.Name,
                ChanceTables.AfterDeathElementalLightTable1.Name,
                ChanceTables.UniqueEnchantTable1.Name,
                ChanceTables.BossRewardsCalderaUniquesTable1.Name,
                ChanceTables.BossRewardsCalderaUniquesTable2.Name,
            },
        };

        public static UniqueEnemy GrandmotherMedyse = new UniqueEnemy()
        {
            Name = nameof(GrandmotherMedyse),
            UID = "kp9R4kaoG02YfLdS9ROM4w",
            QuestEventUID = "_a9RjUNu7karSmOeKR5Izw",
            DropTableNames = new HashSet<string>()
            {
                ChanceTables.UnidentifiedSamplesTable1.Name,
                ChanceTables.UniqueGemsTable1.Name,
                ChanceTables.AfterDeathElementalEtherTable1.Name,
                ChanceTables.UniqueEnchantTable1.Name,
                ChanceTables.BossRewardsCalderaUniquesTable1.Name,
                ChanceTables.BossRewardsCalderaUniquesTable2.Name,
            },
        };

        public static UniqueEnemy MatriarchMyrmitaur = new UniqueEnemy()
        {
            Name = nameof(MatriarchMyrmitaur),
            UID = "6sB4_5lOJU2bWuMHnOL4Ww",
            DropTableNames = new HashSet<string>()
            {
                ChanceTables.UnidentifiedSamplesTable1.Name,
                ChanceTables.UniqueGemsTable1.Name,
                ChanceTables.AfterDeathElementalDecayTable1.Name,
                ChanceTables.UniqueEnchantTable1.Name,
                ChanceTables.BossRewardsCalderaUniquesTable1.Name,
                ChanceTables.BossRewardsCalderaUniquesTable2.Name,
            },
        };

        public static UniqueEnemy QuartzElemental = new UniqueEnemy()
        {
            Name = nameof(QuartzElemental),
            UID = "LhhpSt8BO0aRN5mbeSuDrw",
            QuestEventUID= "VMhJzN--2Uu9Slw_lb3pJw",
            DropTableNames = new HashSet<string>()
            {
                ChanceTables.UnidentifiedSamplesTable1.Name,
                ChanceTables.UniqueGemsTable1.Name,
                ChanceTables.AfterDeathElementalIceTable1.Name,
                ChanceTables.UniqueEnchantTable1.Name,
                ChanceTables.BossRewardsCalderaUniquesTable1.Name,
                ChanceTables.BossRewardsCalderaUniquesTable2.Name,
            },
        };

        public static UniqueEnemy SheWhoSpeaks = new UniqueEnemy()
        {
            Name = nameof(SheWhoSpeaks),
            UID = "MBooN38mU0GPjQJGRuJ95g",
            QuestEventUID = "Q-pIY69iPUq2uynr8rGS_Q",
            DropTableNames = new HashSet<string>()
            {
                ChanceTables.UnidentifiedSamplesTable1.Name,
                ChanceTables.UniqueGemsTable1.Name,
                ChanceTables.AfterDeathElementalEtherTable1.Name,
                ChanceTables.UniqueEnchantTable1.Name,
                ChanceTables.BossRewardsCalderaUniquesTable1.Name,
                ChanceTables.BossRewardsCalderaUniquesTable2.Name,
            },
        };

        public static UniqueEnemy VolcanicGastrocin = new UniqueEnemy()
        {
            Name = nameof(VolcanicGastrocin),
            UID = "fEFTRdXp1kOWX-Z9OMAkBg",
            QuestEventUID = "K4DD7M3hRkGv9SvzhiPrJw",
            DropTableNames = new HashSet<string>()
            {
                ChanceTables.UnidentifiedSamplesTable1.Name,
                ChanceTables.UniqueGemsTable1.Name,
                ChanceTables.AfterDeathElementalFireTable1.Name,
                ChanceTables.UniqueEnchantTable1.Name,
                ChanceTables.BossRewardsCalderaUniquesTable1.Name,
                ChanceTables.BossRewardsCalderaUniquesTable2.Name,
            },
        };
        #endregion

        public static Dictionary<string, UniqueEnemy> CalderaUniques = new Dictionary<string, UniqueEnemy>()
        {
            { CalygreyHero.Name, CalygreyHero },
            { CrackedGargoyle.Name, CrackedGargoyle },
            { GrandmotherMedyse.Name, GrandmotherMedyse },
            { MatriarchMyrmitaur.Name, MatriarchMyrmitaur },
            { QuartzElemental.Name, QuartzElemental },
            { SheWhoSpeaks.Name, SheWhoSpeaks },
            { VolcanicGastrocin.Name, VolcanicGastrocin },
        };

        public static Dictionary<string, Dictionary<string, UniqueEnemy>> AllRegions = new Dictionary<string, Dictionary<string, UniqueEnemy>>()
        {
            {"Caldera", CalderaUniques}
        };
    }
}
